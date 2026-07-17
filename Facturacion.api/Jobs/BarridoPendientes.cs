using System;
using System.Data;
using System.Globalization;
using Facturacion.api.Factories;
using Facturacion.api.Servicios;
using LogicaNegocios.Services;
using Serilog;

namespace Facturacion.api.Jobs
{
    /// <summary>
    /// Un barrido de reproceso: recorre las empresas activas y, por cada
    /// documento pendiente en estado PENDIENTE_AUTORIZACION* o PENDIENTE_CORREO*,
    /// dispara el reproceso correspondiente reusando la logica ya existente
    /// (consulta de autorizacion SRI y envio de correo). El estado puro
    /// "PENDIENTE" (reproceso completo desde cero) queda fuera de alcance por ahora.
    ///
    /// Datos que la fila de FACTURAS_PENDIENTES no guarda y se derivan:
    ///  - Fecha de emision: primeros 8 digitos (ddMMyyyy) de la clave de acceso,
    ///    que es como se localiza el XML firmado en disco.
    ///  - "Usuario": la logica lo usa como NOMBRE de la empresa (SELECT ... FROM
    ///    EMPRESA WHERE NOMBRE = @usuario); se toma de la BD local de la empresa.
    ///
    /// Si alguno de esos datos sale mal, el metodo de reproceso devuelve error y
    /// NO toca el documento: falla seguro, sin reenviar nada al SRI.
    /// </summary>
    public static class BarridoPendientes
    {
        private const string IpSistema = "127.0.0.1";

        public static void Ejecutar()
        {
            var empresas = new ServicioEmpresasGeneral().listarNombresActivas();

            Log.Information(
                "Barrido de reproceso iniciado. Empresas activas: {Total}.",
                empresas.Count);

            foreach (var empresa in empresas)
            {
                try
                {
                    ProcesarEmpresa(empresa);
                }
                catch (Exception ex)
                {
                    Log.Error(ex,
                        "Error procesando pendientes de la empresa {Empresa}.", empresa);
                }
            }
        }

        private static void ProcesarEmpresa(string empresa)
        {
            var app = AppServicesFactory.Create(empresa);

            // La logica de reproceso usa el "usuario" como NOMBRE de empresa
            // para cargar su fila EMPRESA; se toma de la propia BD de la empresa.
            string nombreLocal = ObtenerNombreEmpresaLocal(app);
            if (string.IsNullOrWhiteSpace(nombreLocal))
            {
                Log.Warning(
                    "Empresa {Empresa}: sin EMPRESA activa en su BD; se omite.", empresa);
                return;
            }

            DataSet ds = app.Pendientes.Mostrar();
            if (ds == null || ds.Tables.Count == 0)
                return;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                try
                {
                    ProcesarDocumento(app, empresa, nombreLocal, row);
                }
                catch (Exception ex)
                {
                    Log.Error(ex,
                        "Error en pendiente de {Empresa}: {Numero}.",
                        empresa, Convert.ToString(row["NUMEROFACTURA"]));
                }
            }
        }

        private static void ProcesarDocumento(
            AppServices app, string empresa, string nombreLocal, DataRow row)
        {
            string numero = Convert.ToString(row["NUMEROFACTURA"]).Trim();
            string clave = Convert.ToString(row["CLAVEACCESO"]).Trim();
            string estado = Convert.ToString(row["ESTADO"]).Trim().ToUpperInvariant();
            string tipo = Convert.ToString(row["TIPO"]).Trim().ToUpperInvariant();

            // Estado -> sub-proceso. Solo autorizacion y correo por ahora.
            string proceso;
            if (estado.StartsWith("PENDIENTE_AUTORIZACION"))
                proceso = "AUTORIZACION";
            else if (estado.StartsWith("PENDIENTE_CORREO"))
                proceso = "CORREO";
            else
                return; // NO_AUTORIZADO, PENDIENTE puro u otros: fuera de alcance.

            if (string.IsNullOrWhiteSpace(numero))
                return;

            if (!DerivarFechaDesdeClave(clave, out string fecha))
            {
                Log.Warning(
                    "{Empresa}/{Numero}: clave de acceso invalida ('{Clave}'); sin fecha.",
                    empresa, numero, clave);
                return;
            }

            bool exito;
            bool autorizado;
            string mensaje;

            switch (tipo)
            {
                case "FACTURA":
                    var rf = app.ProcesosFacturacion.FacturaPendienteAutorizacionDesdeApi(
                        numero, fecha, nombreLocal, IpSistema);
                    exito = rf.Exito; autorizado = rf.Autorizado; mensaje = rf.Mensaje;
                    break;

                case "NOTADECREDITO":
                case "NOTA_CREDITO":
                    var rn = app.ProcesosNotaCredito.NotaPendienteAutorizacionDesdeApi(
                        numero, fecha, nombreLocal, IpSistema, proceso);
                    exito = rn.Exito; autorizado = rn.Autorizado; mensaje = rn.Mensaje;
                    break;

                case "RETENCION":
                    var rr = app.ProcesosRetenciones.RetencionPendienteAutorizacionDesdeApi(
                        numero, fecha, nombreLocal, IpSistema, proceso);
                    exito = rr.Exito; autorizado = rr.Autorizado; mensaje = rr.Mensaje;
                    break;

                default:
                    Log.Warning(
                        "{Empresa}/{Numero}: tipo desconocido '{Tipo}'; se omite.",
                        empresa, numero, tipo);
                    return;
            }

            Log.Information(
                "{Empresa}/{Tipo} {Numero} [{Estado} -> {Proceso}]: " +
                "exito={Exito} autorizado={Autorizado}. {Mensaje}",
                empresa, tipo, numero, estado, proceso, exito, autorizado, mensaje);
        }

        private static string ObtenerNombreEmpresaLocal(AppServices app)
        {
            DataSet ds = app.Empresa.MostrarEmpresa(null);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;

            return Convert.ToString(ds.Tables[0].Rows[0]["NOMBRE"]).Trim();
        }

        private static bool DerivarFechaDesdeClave(string clave, out string fecha)
        {
            fecha = null;

            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8)
                return false;

            // La clave de acceso del SRI empieza con la fecha de emision ddMMyyyy.
            string ddMMyyyy = clave.Substring(0, 8);

            if (!DateTime.TryParseExact(
                    ddMMyyyy, "ddMMyyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime f))
                return false;

            fecha = f.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return true;
        }
    }
}
