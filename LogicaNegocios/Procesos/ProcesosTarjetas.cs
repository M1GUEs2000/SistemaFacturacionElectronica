using System;
using System.Data;
using System.Text;
using DF_PinPad.Wrapper.Models;
using LogicaNegocios.Services;

namespace LogicaNegocios.Procesos
{
    /// <summary>
    /// Resultado del cobro con tarjeta (pinpad). Nunca representa una excepción:
    /// un rechazo o un fallo de comunicación llega como Aprobado=false + Motivo.
    /// </summary>
    public class ResultadoCobroTarjeta
    {
        public bool Aprobado { get; set; }
        public string Motivo { get; set; }               // razón del rechazo si no aprobado
        public long PinpadLogId { get; set; }            // Id de auditoría (PINPAD_LOG) para vincular la factura
        public ProcesoPagoResult Detalle { get; set; }   // respuesta cruda del wrapper (autorización, lote, etc.)
    }

    /// <summary>
    /// Orquesta el cobro con el pinpad Datafast y su correlación con la factura.
    ///
    /// La AUDITORÍA en Access (tablas PINPAD_LOG / PINPAD_AUTORIZADAS / PINPAD_ANULACIONES)
    /// la hace SOLA el wrapper: al llamar <c>_services.PinPad.ProcesarPago(...)</c>, el
    /// <see cref="PinPadService"/> invoca internamente al <c>PinPadLogManejador</c>
    /// (ISqlLogger) que hace los INSERT/UPDATE. Aquí NO se escribe en la BD directamente;
    /// esta clase solo arma el request, dispara el cobro e interpreta el resultado.
    ///
    /// No contiene NADA de UI (ni toasts ni MessageBox): eso queda en el form. El cobro
    /// es SÍNCRONO y BLOQUEANTE (espera la tarjeta), así que el form debe llamarlo dentro
    /// de <c>Task.Run(...)</c> para no congelar la ventana.
    /// </summary>
    public class ProcesosTarjetas
    {
        private readonly AppServices _services;

        public ProcesosTarjetas(AppServices services)
        {
            _services = services;
        }

        /// <summary>¿La forma de pago seleccionada es TARJETA? (el botón de forma de pago
        /// deja el nombre del método en txtFormaPago del form).</summary>
        public static bool EsPagoTarjeta(string formaPago)
        {
            return !string.IsNullOrWhiteSpace(formaPago)
                && formaPago.Trim().ToUpperInvariant().Contains("TARJETA");
        }

        /// <summary>
        /// Cobra el total de la factura con el pinpad. Recalcula los totales desde el
        /// detalle (mismas fórmulas del XML, para que Total = Base0 + BaseImponible + IVA
        /// cuadre EXACTO y Datafast no rechace por "valores incorrectos").
        ///
        /// Nunca lanza por un rechazo ni por un fallo de comunicación: ambos vienen en
        /// <see cref="ResultadoCobroTarjeta.Aprobado"/> = false + Motivo.
        /// </summary>
        /// <param name="detalleFactura">Detalle de la venta (columnas PRODUCTO/CANTIDAD/TOTAL).</param>
        /// <param name="cliente">Cédula/tipo de cliente (para resolver el número igual que la emisión).</param>
        /// <param name="formaPago">Forma de pago (para distinguir COMPRAS del secuencial normal).</param>
        /// <param name="tipoPago">"C" corriente / "D" diferido.</param>
        /// <param name="diferidoNombre">Nombre del tipo de diferido (solo si tipoPago="D").</param>
        /// <param name="cuotas">Plazo diferido en cuotas (solo si tipoPago="D").</param>
        /// <param name="usuarioSistema">Usuario que realiza el cobro (para la auditoría).</param>
        public ResultadoCobroTarjeta CobrarFactura(
            DataTable detalleFactura,
            string cliente,
            string formaPago,
            string tipoPago,
            string diferidoNombre,
            int cuotas,
            string usuarioSistema)
        {
            var totales = _services.ProcesosFacturacion.CalcularTotalesFactura(detalleFactura);
            bool diferido = string.Equals(tipoPago, "D", StringComparison.OrdinalIgnoreCase);

            // MISMO número que emitirá PrepararFactura (peek del secuencial, no incrementa).
            // Así la auditoría queda con el número real desde el INSERT; VincularFactura solo
            // lo corrige si el SRI cambia el secuencial por reintento.
            string numeroFacturaPrevisto = _services.ProcesosFacturacion
                .ObtenerNumeroFacturaPrevisto(cliente, formaPago);

            var req = new ProcesoPagoRequest
            {
                NumeroFactura = numeroFacturaPrevisto,
                Monto = totales.Total,
                BaseImponible = totales.BaseImponible,
                Base0 = totales.Base0,
                IVA = totales.Iva,
                Red = "Datafast",
                TipoTransaccion = diferido ? "Diferido" : "Corriente",
                TipoCredito = diferido ? MapearTipoCredito(diferidoNombre) : null,
                PlazoDiferido = (diferido && cuotas > 0) ? (int?)cuotas : null,
                UsuarioSistema = usuarioSistema
            };

            var resultado = new ResultadoCobroTarjeta();
            try
            {
                // Aquí adentro el wrapper YA persiste la auditoría en Access.
                ProcesoPagoResult cobro = _services.PinPad.ProcesarPago(req);

                resultado.Detalle = cobro;
                // La guía indica usar SIEMPRE result.Exitoso (ya incorpora CodigoRespuestaAut=="00");
                // no re-chequear a mano para no rechazar por error un cobro realmente aprobado.
                resultado.Aprobado = cobro != null && cobro.Exitoso;
                resultado.PinpadLogId = cobro?.TransaccionLogId ?? 0;
                resultado.Motivo = resultado.Aprobado
                    ? null
                    : (cobro?.MensajeRespuestaAut ?? cobro?.ExcepcionMensaje ?? "Sin respuesta del datafast.");
            }
            catch (Exception ex)
            {
                // ProcesarPago relanza ante una excepción real de comunicación/IO. La
                // convertimos en rechazo controlado para que el form no tenga que envolver
                // en try/catch: la factura simplemente no se emite.
                resultado.Aprobado = false;
                resultado.Motivo = ex.Message;
            }

            return resultado;
        }

        /// <summary>
        /// Correlaciona un cobro ya registrado con el número REAL de factura, una vez que
        /// el backend lo confirma (el secuencial previsto pudo cambiar por reintento del SRI).
        /// Traga cualquier error: la auditoría nunca debe tumbar la emisión.
        /// </summary>
        public void VincularFactura(long pinpadLogId, string numeroFacturaReal)
        {
            if (pinpadLogId == 0) return;
            try { _services.PinPad.VincularNumeroFactura(pinpadLogId, numeroFacturaReal); }
            catch { /* auditoría no bloquea la factura */ }
        }

        /// <summary>
        /// Diagnóstico del "no se guardaba": verifica que las 3 tablas de auditoría existan
        /// y sean consultables. Devuelve null si todo está bien, o el detalle del problema
        /// por tabla. Es necesario porque el logger TRAGA los errores de INSERT (para no
        /// perder un cobro real): si una tabla falta o está mal nombrada, no verías nada
        /// en pantalla — este método hace visible ese fallo.
        /// </summary>
        public string VerificarAuditoria()
        {
            string[] tablas = { "PINPAD_LOG", "PINPAD_AUTORIZADAS", "PINPAD_ANULACIONES" };
            var problemas = new StringBuilder();

            foreach (string tabla in tablas)
            {
                try
                {
                    _services.Conexion.Seleccionar("SELECT COUNT(*) FROM " + tabla);
                }
                catch (Exception ex)
                {
                    problemas.AppendLine(tabla + ": " + ex.Message);
                }
            }

            return problemas.Length == 0 ? null : problemas.ToString().Trim();
        }

        /// <summary>
        /// Mapea el nombre del tipo de diferido del POS (VALOR de PARAMETROS_TRANSACCIONES)
        /// al string de DF_PinPad.TipoCredito que espera el wrapper. Se resuelve por palabras
        /// clave para no depender de la puntuación/orden exacto del texto.
        /// </summary>
        private static string MapearTipoCredito(string nombreDiferido)
        {
            string n = (nombreDiferido ?? "").ToUpperInvariant();

            bool gracia = n.Contains("GRACIA");
            bool plus = n.Contains("PLUS");

            if (plus)
                return n.Contains("CUOTA") ? "DiferidoPlusCuotas" : "DiferidoPlus";

            if (n.Contains("SIN INTERES"))
                return gracia ? "DiferidoSinInteresesConMesesDeGracia" : "DiferidoSinIntereses";

            if (n.Contains("CON INTERES"))
                return gracia ? "DiferidoConInteresesConMesesDeGracia" : "DiferidoConIntereses";

            // "CORRIENTE" u otro: diferido corriente por defecto
            return "DiferidoCorriente";
        }
    }
}
