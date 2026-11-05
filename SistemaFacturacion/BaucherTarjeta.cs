using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DF_PinPad.Wrapper.Models;
using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using Microsoft.Reporting.WinForms;

namespace SistemaFacturacion
{
    /// <summary>
    /// Impresión del baucher de consumo con tarjeta.
    ///
    /// Sale como una SEGUNDA tira, después del recibo, y SOLO cuando el cobro fue con
    /// tarjeta y el total alcanza MINIMOFIRMA (ver <see cref="ProcesosTarjetas.RequiereFirmaBaucher"/>).
    /// El recibo de la venta se sigue imprimiendo siempre, en toda compra: esta clase no
    /// lo toca.
    ///
    /// Hay DOS reportes y se elige según cómo se cobró:
    ///   - rptBaucher.rdlc          → CORRIENTE
    ///   - rptBaucherDiferido.rdlc  → DIFERIDO (agrega la fila DIF/MESES/GRACIA y las
    ///                                líneas INTERES y GRAN TOTAL)
    /// Son dos archivos y no uno con visibilidad condicional porque el cuerpo del .rdlc
    /// posiciona todo en absoluto: ocultar los Textbox del diferido dejaría los huecos.
    ///
    /// Todos los datos entran al reporte como ReportParameters (los .rdlc no tienen
    /// DataSets) porque son campos escalares: así se evita un Tablix por dato.
    /// Reutiliza <see cref="Impresora"/> tal cual — el render a EMF y la impresora
    /// predeterminada son los mismos que usa el recibo.
    /// </summary>
    internal static class BaucherTarjeta
    {
        public const string RotuloOriginal = "ORIGINAL";
        public const string RotuloCopia = "COPIA";

        private const string ReporteCorriente = @"rptBaucher.rdlc";
        private const string ReporteDiferido = @"rptBaucherDiferido.rdlc";

        /// <summary>
        /// Imprime una tira del baucher.
        /// </summary>
        /// <param name="services">Contenedor de servicios (tarifa de IVA y tabla de parámetros).</param>
        /// <param name="empresa">Fila de EMPRESA ya consultada por el form (nombre, RUC, dirección, teléfono).</param>
        /// <param name="cobro">Respuesta cruda del pinpad: autorización, lote, referencia, tarjeta.</param>
        /// <param name="totales">Montos REALMENTE cobrados (los que se mandaron al datafast).</param>
        /// <param name="rotulo"><see cref="RotuloOriginal"/> o <see cref="RotuloCopia"/>.</param>
        /// <param name="diferido">
        /// Condiciones del diferido si el cobro fue diferido; null si fue corriente.
        /// Es lo que decide cuál de los dos .rdlc se imprime.
        /// </param>
        public static void Imprimir(
            AppServices services,
            DataRow empresa,
            ProcesoPagoResult cobro,
            TotalesFactura totales,
            string rotulo,
            DiferidoCobrado diferido = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (cobro == null) throw new ArgumentNullException(nameof(cobro));
            if (totales == null) throw new ArgumentNullException(nameof(totales));

            string nombreEmpresa = Campo(empresa, "NOMBRE");
            string modoLectura = services.ProcesosTarjetas.DescribirModoLectura(cobro.ModoLectura);

            var rdlc = new LocalReport
            {
                ReportPath = diferido == null ? ReporteCorriente : ReporteDiferido
            };

            var parametros = new List<ReportParameter>
            {
                // --- Comercio (EMPRESA) -------------------------------------
                new ReportParameter("COMERCIO",  nombreEmpresa),
                new ReportParameter("RUC",       Campo(empresa, "RUC")),
                new ReportParameter("DIRECCION", Campo(empresa, "DIRECCION")),
                new ReportParameter("TELEFONO",  Campo(empresa, "TELEFONO")),
                // Sin campo de ciudad en la tabla EMPRESA: va en blanco a propósito.
                new ReportParameter("CIUDAD",    ""),

                // --- Terminal ----------------------------------------------
                new ReportParameter("TID", Texto(cobro.TID)),
                new ReportParameter("MID", Texto(cobro.MID)),
                new ReportParameter("MODOLECTURA", modoLectura),

                // --- Transacción -------------------------------------------
                new ReportParameter("GRUPOTARJETA", Texto(cobro.NombreGrupoTarjeta)),
                new ReportParameter("TARJETA",      ProcesosTarjetas.NumeroTarjetaVisible(cobro)),
                new ReportParameter("VENCIMIENTO",
                    ProcesosTarjetas.FormatearVencimiento(cobro.FechaVencimiento)),
                new ReportParameter("LOTE",         Texto(cobro.Lote)),
                new ReportParameter("REFERENCIA",   Texto(cobro.Referencia)),
                new ReportParameter("ADQUIRIENTE",  Texto(cobro.NombreAdquirente)),
                new ReportParameter("FECHA",        Texto(cobro.Fecha)),
                new ReportParameter("HORA",         Texto(cobro.Hora)),
                new ReportParameter("APROBACION",   Texto(cobro.Autorizacion)),

                // --- Montos ------------------------------------------------
                // La tarifa del rótulo sale de PARAMETROS_FACTURAS (CODIGOPORCENTAJE),
                // no está fija en 12: hoy son 15. TarifaIva viene como FRACCIÓN (0.15)
                // porque el resto del sistema la usa de multiplicador; aquí se pasa a
                // porcentaje para que el rótulo diga "TARIFA 15" y no "TARIFA 0,15".
                new ReportParameter("TARIFAIVA",
                    (services.TarifaIva * 100m).ToString("0.##", CultureInfo.CurrentCulture)),
                new ReportParameter("BASEGRAVADA", Monto(totales.BaseImponible)),
                new ReportParameter("BASE0",       Monto(totales.Base0)),
                new ReportParameter("SUBTOTAL",    Monto(totales.Base0 + totales.BaseImponible)),
                new ReportParameter("IVA",         Monto(totales.Iva)),
                new ReportParameter("TOTAL",       Monto(totales.Total)),

                // --- Pie y firma -------------------------------------------
                new ReportParameter("RED",        Texto(cobro.RedAdquirente)),
                // Mismo dato que se audita en PINPAD_AUTORIZADAS.TARJETAHABIENTE (se graba
                // desde este mismo ProcesoPagoResult), por eso se toma del objeto y no de
                // la BD. Ojo: en contactless el pinpad devuelve aquí el modo de lectura y
                // no el nombre; en ese caso la línea NOMBRE se deja en blanco para escribir
                // a mano, igual que C.I. y TELEFONO.
                new ReportParameter("HABIENTE",   NombreHabiente(cobro, modoLectura)),
                new ReportParameter("ROTULO",     string.IsNullOrWhiteSpace(rotulo) ? RotuloOriginal : rotulo),
                // La línea final ya no es el nombre del comercio: es el texto libre de
                // publicidad (PARAMETROS_TRANSACCIONES, NOMBRE='PUBLICIDAD', CODIGO='P').
                // Si está vacío el reporte no imprime nada en esa línea.
                new ReportParameter("PUBLICIDAD", Texto(services.ParamTransaccion.ObtenerPublicidad()))
            };

            // --- Diferido ---------------------------------------------------
            // Solo el rptBaucherDiferido.rdlc declara estos parámetros; mandarlos
            // al reporte corriente reventaría el SetParameters, por eso se agregan
            // únicamente cuando el cobro fue diferido.
            if (diferido != null)
            {
                // El reporte ya antepone "DIF ", así que aquí va solo el tipo:
                // "SIN INTERESES" sale impreso como "DIF SIN INTERESES".
                parametros.Add(new ReportParameter("TIPODIFERIDO", Texto(diferido.Nombre)));

                // Cuotas y gracia con 2 dígitos, como el baucher del datafast ("MESES: 03").
                // En 0 se mandan vacías y el reporte no imprime el rótulo.
                parametros.Add(new ReportParameter("CUOTAS", Meses(diferido.Cuotas)));
                parametros.Add(new ReportParameter("GRACIA", Meses(diferido.MesesGracia)));

                // INTERES hoy siempre 0.00 (el POS no lo calcula todavía), así que
                // GRAN TOTAL sale igual al VR. TOTAL. La línea se imprime de todas
                // formas para respetar el formato del baucher de diferido.
                parametros.Add(new ReportParameter("INTERES", Monto(diferido.Interes)));
                parametros.Add(new ReportParameter("GRANTOTAL", Monto(totales.Total + diferido.Interes)));
            }

            rdlc.SetParameters(parametros);

            var imp = new Impresora();
            try { imp.Imprime(rdlc); }
            finally { imp.Dispose(); }
        }

        /// <summary>Valor de una columna de EMPRESA, "" si la fila o la columna no están.</summary>
        private static string Campo(DataRow fila, string columna)
        {
            if (fila == null || !fila.Table.Columns.Contains(columna))
                return "";

            object valor = fila[columna];
            return valor == null || valor == DBNull.Value ? "" : valor.ToString().Trim();
        }

        /// <summary>
        /// Los ReportParameter no aceptan null: cualquier campo que el pinpad no haya
        /// devuelto se manda como cadena vacía para que el reporte no falle al renderizar.
        /// </summary>
        private static string Texto(string valor)
        {
            return (valor ?? "").Trim();
        }

        /// <summary>
        /// Nombre del tarjetahabiente para la línea NOMBRE del baucher.
        ///
        /// El pinpad no siempre manda el nombre en TarjetaHabiente: cuando la lectura es
        /// contactless repite ahí el modo de lectura ("CONTACTLESS"). Si el valor coincide
        /// con el modo —crudo o ya descrito— se devuelve vacío para no imprimir un rótulo
        /// técnico donde el cliente debe firmar su nombre.
        /// </summary>
        private static string NombreHabiente(ProcesoPagoResult cobro, string modoDescrito)
        {
            string habiente = Texto(cobro.TarjetaHabiente);
            if (habiente.Length == 0)
                return "";

            if (string.Equals(habiente, Texto(cobro.ModoLectura), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(habiente, Texto(modoDescrito), StringComparison.OrdinalIgnoreCase))
                return "";

            return habiente;
        }

        /// <summary>Monto con el formato del baucher ("$ 21,40").</summary>
        private static string Monto(decimal valor)
        {
            return "$ " + valor.ToString("N2", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Cuotas / meses de gracia con el formato del baucher (2 dígitos: "03").
        /// En 0 devuelve "" para que el reporte no imprima el rótulo — es el caso de
        /// los diferidos que no usan meses de gracia.
        /// </summary>
        private static string Meses(int valor)
        {
            return valor <= 0 ? "" : valor.ToString("00", CultureInfo.InvariantCulture);
        }
    }
}
