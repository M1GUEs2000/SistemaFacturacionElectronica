using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Facturacion.api.Auth;

namespace Facturacion.api.Seguridad
{
    /// <summary>
    /// Rate limiting FixedWindow en memoria para WebAPI 2 (.NET 4.8 no trae AddRateLimiter).
    /// Particiona por empresa del JWT (si el token es valido) o por IP de origen.
    ///
    /// A diferencia de una version basada solo en la ruta, aqui la politica tambien
    /// mira el verbo HTTP: asi los GET de lectura que comparten prefijo con un POST
    /// (p. ej. /api/v1/facturas/pdf vs POST /api/v1/facturas) no gastan cuota.
    ///
    /// Politicas:
    ///   auth      -> 10/min  por IP      : logins y validacion de clave (anti fuerza bruta).
    ///   emision   -> 120/min por empresa : POST de facturas/notas/retenciones (firma + SRI).
    ///   escritura -> 40/min  por empresa : resto de POST/PUT/PATCH/DELETE (crear/editar/borrar).
    ///   lectura (GET) sin limite.
    ///
    /// Al superar el limite: 429 Too Many Requests + header Retry-After.
    ///
    /// Almacen en memoria: suficiente para una sola instancia. Tras balanceador o
    /// varias instancias habria que mover los contadores a un store compartido.
    /// </summary>
    public sealed class RateLimitingHandler : DelegatingHandler
    {
        private const int Http429 = 429;

        private sealed class Politica
        {
            public string Nombre;
            public int Limite;
            public TimeSpan Ventana;
            public bool PorIp;   // true: particiona por IP; false: por empresa del JWT (fallback IP).
        }

        private sealed class Contador
        {
            public int Conteo;
            public DateTime InicioVentana;
        }

        private static readonly Politica Auth =
            new Politica { Nombre = "auth", Limite = 10, Ventana = TimeSpan.FromMinutes(1), PorIp = true };

        private static readonly Politica Emision =
            new Politica { Nombre = "emision", Limite = 120, Ventana = TimeSpan.FromMinutes(1), PorIp = false };

        private static readonly Politica Escritura =
            new Politica { Nombre = "escritura", Limite = 40, Ventana = TimeSpan.FromMinutes(1), PorIp = false };

        private static readonly ConcurrentDictionary<string, Contador> Contadores =
            new ConcurrentDictionary<string, Contador>();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Los preflight CORS (OPTIONS) no consumen cuota.
            if (request.Method == HttpMethod.Options)
                return base.SendAsync(request, cancellationToken);

            var politica = SeleccionarPolitica(request);
            if (politica == null)
                return base.SendAsync(request, cancellationToken);

            var clave = politica.Nombre + "|" + Particion(request, politica);
            var ahora = DateTime.UtcNow;

            var contador = Contadores.GetOrAdd(clave, _ => new Contador { Conteo = 0, InicioVentana = ahora });

            int conteo;
            DateTime inicio;
            lock (contador)
            {
                if (ahora - contador.InicioVentana >= politica.Ventana)
                {
                    contador.Conteo = 0;
                    contador.InicioVentana = ahora;
                }
                contador.Conteo++;
                conteo = contador.Conteo;
                inicio = contador.InicioVentana;
            }

            if (conteo > politica.Limite)
            {
                var resetEn = (int)Math.Ceiling((inicio + politica.Ventana - ahora).TotalSeconds);
                if (resetEn < 1) resetEn = 1;

                var respuesta = request.CreateResponse(
                    (HttpStatusCode)Http429,
                    new { codigo = "RATE_LIMIT", mensaje = "Demasiadas solicitudes. Reintente mas tarde." });
                respuesta.Headers.Add("Retry-After", resetEn.ToString());
                return Task.FromResult(respuesta);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static Politica SeleccionarPolitica(HttpRequestMessage request)
        {
            var path = (request.RequestUri.AbsolutePath ?? string.Empty).ToLowerInvariant();
            var metodo = request.Method;

            // 1) Autenticacion: logins y validacion de clave. Cubre cualquier verbo.
            //    /api/v1/login* incluye tambien /api/v1/loginempresasgeneral.
            if (path.IndexOf("/api/v1/login", StringComparison.Ordinal) >= 0 ||
                path.IndexOf("/api/v1/usuariosgenerales/verificar", StringComparison.Ordinal) >= 0)
                return Auth;

            bool esEscritura =
                metodo == HttpMethod.Post || metodo == HttpMethod.Put ||
                metodo == HttpMethod.Delete || metodo == new HttpMethod("PATCH");

            // Los GET (incluidas descargas de PDF/XML) quedan sin limite.
            if (!esEscritura)
                return null;

            // 2) Emision al SRI: solo POST de facturas/notas/retenciones.
            //    El slash final evita colisiones con /notascredito y /retencionestabla,
            //    que son CRUD de tablas y caen en "escritura".
            if (metodo == HttpMethod.Post &&
                (path.IndexOf("/api/v1/facturas", StringComparison.Ordinal) >= 0 ||
                 path.IndexOf("/api/v1/notas/", StringComparison.Ordinal) >= 0 ||
                 path.IndexOf("/api/v1/retenciones/", StringComparison.Ordinal) >= 0))
                return Emision;

            // 3) Resto de operaciones que modifican datos.
            return Escritura;
        }

        private static string Particion(HttpRequestMessage request, Politica politica)
        {
            if (!politica.PorIp)
            {
                var auth = request.Headers.Authorization;
                if (auth != null && auth.Scheme == "Bearer" && !string.IsNullOrWhiteSpace(auth.Parameter))
                {
                    string empresa;
                    if (JwtHelper.Validar(auth.Parameter, out empresa) && !string.IsNullOrEmpty(empresa))
                        return "emp:" + empresa;
                }
            }

            return "ip:" + ObtenerIp();
        }

        private static string ObtenerIp()
        {
            var contexto = HttpContext.Current;
            var ip = contexto?.Request?.UserHostAddress;
            return string.IsNullOrEmpty(ip) ? "desconocida" : ip;
        }
    }
}
