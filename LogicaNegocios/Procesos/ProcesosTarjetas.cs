using System;
using System.Data;
using System.Globalization;
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

        /// <summary>
        /// Totales que se COBRARON realmente (los que se le mandaron al pinpad). Se
        /// devuelven para que el baucher imprima exactamente esos montos y no un
        /// recálculo posterior que podría diferir en centavos.
        /// </summary>
        public TotalesFactura Totales { get; set; }
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
        /// <param name="mesesGracia">Meses de gracia, solo para los diferidos que lo requieren.</param>
        /// <param name="usuarioSistema">Usuario que realiza el cobro (para la auditoría).</param>
        public ResultadoCobroTarjeta CobrarFactura(
            DataTable detalleFactura,
            string cliente,
            string formaPago,
            string tipoPago,
            string diferidoNombre,
            int cuotas,
            int mesesGracia,
            string usuarioSistema)
        {
            var totales = _services.ProcesosFacturacion.CalcularTotalesFactura(detalleFactura);
            bool diferido = string.Equals(tipoPago, "D", StringComparison.OrdinalIgnoreCase);

            // MISMO número que emitirá PrepararFactura (peek del secuencial, no incrementa).
            // Así la auditoría queda con el número real desde el INSERT; VincularFactura solo
            // lo corrige si el SRI cambia el secuencial por reintento.
            string numeroFacturaPrevisto = _services.ProcesosFacturacion
                .ObtenerNumeroFacturaPrevisto(cliente, formaPago);

            string tipoCredito = diferido ? MapearTipoCredito(diferidoNombre) : null;
            var resultado = new ResultadoCobroTarjeta { Totales = totales };

            if (diferido
                && (tipoCredito == "DiferidoConInteresesConMesesDeGracia"
                    || tipoCredito == "DiferidoSinInteresesConMesesDeGracia")
                && mesesGracia <= 0)
            {
                resultado.Aprobado = false;
                resultado.Motivo = "Seleccione los meses de gracia para el diferido elegido.";
                return resultado;
            }

            var req = new ProcesoPagoRequest
            {
                NumeroFactura = numeroFacturaPrevisto,
                Monto = totales.Total,
                BaseImponible = totales.BaseImponible,
                Base0 = totales.Base0,
                IVA = totales.Iva,
                Red = "Datafast",
                TipoTransaccion = diferido ? "Diferido" : "Corriente",
                TipoCredito = tipoCredito,
                PlazoDiferido = (diferido && cuotas > 0) ? (int?)cuotas : null,
                MesesGracia = (diferido && mesesGracia > 0) ? (int?)mesesGracia : null,
                UsuarioSistema = usuarioSistema
            };

            try
            {
                // Aquí adentro el wrapper YA persiste la auditoría en Access.
                ProcesoPagoResult cobro = _services.PinPad.ProcesarPago(req);

                resultado.Detalle = cobro;
                // La venta solo continúa si el pinpad y el autorizador responden "00".
                // El wrapper marca Exitoso por CodigoRespuestaAut, pero un error de trama
                // llega en CodigoRespuesta y también debe detener la emisión.
                resultado.Aprobado = EsRespuestaAprobada(cobro);
                resultado.PinpadLogId = cobro?.TransaccionLogId ?? 0;
                resultado.Motivo = resultado.Aprobado ? null : MotivoRespuestaNoAprobada(cobro);
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

        private static bool EsRespuestaAprobada(ProcesoPagoResult cobro)
        {
            return cobro != null
                && string.Equals((cobro.CodigoRespuesta ?? "").Trim(), "00", StringComparison.Ordinal)
                && string.Equals((cobro.CodigoRespuestaAut ?? "").Trim(), "00", StringComparison.Ordinal);
        }

        private static string MotivoRespuestaNoAprobada(ProcesoPagoResult cobro)
        {
            if (!string.IsNullOrWhiteSpace(cobro?.MensajeRespuestaAut))
                return cobro.MensajeRespuestaAut;

            if (!string.IsNullOrWhiteSpace(cobro?.ExcepcionMensaje))
                return cobro.ExcepcionMensaje;

            return "Respuesta no aprobada del datafast. Código pinpad: " +
                (cobro?.CodigoRespuesta ?? "sin respuesta") +
                ", código autorizador: " + (cobro?.CodigoRespuestaAut ?? "sin respuesta") + ".";
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
        /// ¿El consumo obliga a imprimir el baucher con firma? Compara el total cobrado
        /// contra MINIMOFIRMA (PARAMETROS_TRANSACCIONES, NOMBRE='MINIMOFIRMA', CODIGO='F1').
        ///
        /// Si el parámetro no existe o no es un número, devuelve false: se mantiene el
        /// comportamiento de siempre (solo el recibo) en vez de imprimir un baucher
        /// disparado por un valor basura.
        /// </summary>
        public bool RequiereFirmaBaucher(decimal total)
        {
            string valor = _services.ParamTransaccion.ObtenerMinimoFirma();
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            if (!TryParseMonto(valor, out decimal minimo))
                return false;

            return total >= minimo;
        }

        /// <summary>
        /// Número de tarjeta a mostrar en recibo y baucher.
        ///
        /// El campo TarjetaTruncadaOTT viene vacío en las transacciones normales (la DLL
        /// solo lo llena en el flujo OTT/tokenizado), así que se usa NumeroTarjeta, que es
        /// el PAN ya enmascarado que devuelve el pinpad. Se deja TarjetaTruncadaOTT como
        /// respaldo por si en algún flujo llega solo ese.
        /// </summary>
        public static string NumeroTarjetaVisible(ProcesoPagoResult cobro)
        {
            if (cobro == null)
                return "";

            string numero = (cobro.NumeroTarjeta ?? "").Trim();
            if (numero.Length > 0)
                return numero;

            return (cobro.TarjetaTruncadaOTT ?? "").Trim();
        }

        /// <summary>
        /// Fecha de expiración de la tarjeta con el formato del baucher ("MM/AA").
        ///
        /// El pinpad la devuelve como 4 dígitos sin separador; se le mete la barra sin
        /// reordenar nada. Si llegara con otra longitud (o vacía) se devuelve tal cual:
        /// más vale imprimir el dato crudo que inventarle un formato.
        ///
        /// Se toma como entrada un string (no el ProcesoPagoResult) para que la reimpresión
        /// de una COPIA pueda pasarle el valor leído de PINPAD_AUTORIZADAS.FECHAVENCIMIENTO.
        /// </summary>
        public static string FormatearVencimiento(string valor)
        {
            string v = (valor ?? "").Trim();
            if (v.Length != 4)
                return v;

            foreach (char c in v)
                if (!char.IsDigit(c))
                    return v;

            return v.Substring(0, 2) + "/" + v.Substring(2, 2);
        }

        /// <summary>
        /// Texto del "TIPO DE LECTURA" del baucher. El pinpad devuelve
        /// <see cref="ProcesoPagoResult.ModoLectura"/> y la tabla de parámetros tiene la
        /// descripción en NOMBRE='MODOPAGO' (CODIGO 01..06).
        ///
        /// Resuelve por CODIGO; si lo que llegó no es uno de esos códigos (la DLL ya manda
        /// el texto, o manda algo nuevo), devuelve el valor crudo en vez de vaciar el campo.
        /// </summary>
        public string DescribirModoLectura(string modoLectura)
        {
            string modo = (modoLectura ?? "").Trim();
            if (modo.Length == 0)
                return "";

            try
            {
                DataSet ds = _services.ParamTransaccion.ObtenerModosPago();
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow fila in ds.Tables[0].Rows)
                    {
                        string codigo = (fila["CODIGO"] ?? "").ToString().Trim();
                        if (string.Equals(codigo, modo, StringComparison.OrdinalIgnoreCase))
                            return (fila["VALOR"] ?? "").ToString().Trim();
                    }
                }
            }
            catch
            {
                // Un fallo de lectura de la tabla no debe dejar el baucher sin imprimir:
                // se cae al valor crudo del pinpad.
            }

            return modo;
        }

        /// <summary>
        /// Parsea un monto guardado como texto en la tabla de parámetros. Se intenta con la
        /// cultura del equipo y con la invariante, porque el valor pudo grabarse con coma
        /// o con punto decimal según cómo estuviera configurada la caja.
        /// </summary>
        private static bool TryParseMonto(string valor, out decimal resultado)
        {
            const NumberStyles estilos = NumberStyles.Number;

            return decimal.TryParse(valor, estilos, CultureInfo.CurrentCulture, out resultado)
                || decimal.TryParse(valor, estilos, CultureInfo.InvariantCulture, out resultado);
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
            string n = NormalizarTexto(nombreDiferido);

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

        private static string NormalizarTexto(string valor)
        {
            string descompuesto = (valor ?? "").Normalize(NormalizationForm.FormD);
            var resultado = new StringBuilder(descompuesto.Length);

            foreach (char caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                    resultado.Append(caracter);
            }

            return resultado.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }
    }
}
