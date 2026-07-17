using System;
using System.Configuration;
using System.Threading;
using Facturacion.api.Servicios;
using Serilog;

namespace Facturacion.api.Jobs
{
    /// <summary>
    /// Demonio en proceso: cada N minutos recorre las empresas activas para
    /// reprocesar sus documentos pendientes.
    ///
    /// Lote 7.0 (andamiaje): solo registra el barrido y cuenta las empresas
    /// activas; NO toca todavia la logica de facturacion. El Lote 7.1
    /// conectara autorizacion -> correo -> reproceso dentro de <see cref="Ejecutar"/>.
    ///
    /// Se controla con el toggle "ReprocesoAutomatico.Enabled" (apagado por
    /// defecto). Corre dentro del proceso de IIS: en Plesk el app pool debe
    /// quedar "Always On" para que no se duerma por inactividad.
    /// </summary>
    public static class ReprocesoAutomaticoDaemon
    {
        private static Timer _timer;
        private static int _corriendo;             // 0 = libre, 1 = barrido en curso
        private static readonly object _arranque = new object();

        public static void Iniciar()
        {
            if (!EstaHabilitado())
            {
                Log.Information(
                    "Reproceso automatico deshabilitado (ReprocesoAutomatico.Enabled != true).");
                return;
            }

            lock (_arranque)
            {
                if (_timer != null)
                    return;

                var intervalo = ObtenerIntervalo();

                // Primer disparo tras un intervalo completo (no al instante),
                // para no chocar con el arranque de la app.
                _timer = new Timer(Ejecutar, null, intervalo, intervalo);

                Log.Information(
                    "Reproceso automatico iniciado. Intervalo: {Segundos} s.",
                    intervalo.TotalSeconds);
            }
        }

        public static void Detener()
        {
            lock (_arranque)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        private static void Ejecutar(object estado)
        {
            // Candado anti-solapamiento: si el barrido anterior sigue en curso,
            // se salta este ciclo en vez de correr dos a la vez.
            if (Interlocked.CompareExchange(ref _corriendo, 1, 0) != 0)
            {
                Log.Warning("Barrido de reproceso saltado: el anterior sigue en curso.");
                return;
            }

            try
            {
                var empresas = new ServicioEmpresasGeneral().listarNombresActivas();

                Log.Information(
                    "Barrido de reproceso ejecutado. Empresas activas: {Total}.",
                    empresas.Count);

                // Lote 7.1: por cada empresa -> autorizacion SRI, correo y reproceso.
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en el barrido de reproceso automatico.");
            }
            finally
            {
                Interlocked.Exchange(ref _corriendo, 0);
            }
        }

        private static bool EstaHabilitado()
        {
            var valor = LeerConfig("ReprocesoAutomatico.Enabled");
            return bool.TryParse(valor, out bool activo) && activo;
        }

        private static TimeSpan ObtenerIntervalo()
        {
            // Los segundos tienen prioridad: pensados para pruebas rapidas.
            var seg = LeerConfig("ReprocesoAutomatico.IntervaloSegundos");
            if (int.TryParse(seg, out int segundos) && segundos > 0)
                return TimeSpan.FromSeconds(segundos);

            var min = LeerConfig("ReprocesoAutomatico.IntervaloMinutos");
            if (int.TryParse(min, out int minutos) && minutos > 0)
                return TimeSpan.FromMinutes(minutos);

            return TimeSpan.FromMinutes(10);
        }

        private static string LeerConfig(string clave)
        {
            var envKey = clave.Replace(".", "_").ToUpperInvariant();
            var env = Environment.GetEnvironmentVariable(envKey);

            if (!string.IsNullOrWhiteSpace(env))
                return env;

            return ConfigurationManager.AppSettings[clave];
        }
    }
}
