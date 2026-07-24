using System;
using System.Data;
using System.Globalization;
using DF_PinPad.Wrapper.Models;
using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using Microsoft.Reporting.WinForms;

namespace SistemaFacturacion
{
    /// <summary>
    /// Impresión del baucher de consumo con tarjeta (rptBaucher.rdlc).
    ///
    /// Sale como una SEGUNDA tira, después del recibo, y SOLO cuando el cobro fue con
    /// tarjeta y el total alcanza MINIMOFIRMA (ver <see cref="ProcesosTarjetas.RequiereFirmaBaucher"/>).
    /// El recibo de la venta se sigue imprimiendo siempre, en toda compra: esta clase no
    /// lo toca.
    ///
    /// Todos los datos entran al reporte como ReportParameters (el .rdlc no tiene
    /// DataSets) porque son campos escalares: así se evita un Tablix por dato.
    /// Reutiliza <see cref="Impresora"/> tal cual — el render a EMF y la impresora
    /// predeterminada son los mismos que usa el recibo.
    /// </summary>
    internal static class BaucherTarjeta
    {
        public const string RotuloOriginal = "ORIGINAL";
        public const string RotuloCopia = "COPIA";

        /// <summary>
        /// Imprime una tira del baucher.
        /// </summary>
        /// <param name="services">Contenedor de servicios (tarifa de IVA y tabla de parámetros).</param>
        /// <param name="empresa">Fila de EMPRESA ya consultada por el form (nombre, RUC, dirección, teléfono).</param>
        /// <param name="cobro">Respuesta cruda del pinpad: autorización, lote, referencia, tarjeta.</param>
        /// <param name="totales">Montos REALMENTE cobrados (los que se mandaron al datafast).</param>
        /// <param name="rotulo"><see cref="RotuloOriginal"/> o <see cref="RotuloCopia"/>.</param>
        public static void Imprimir(
            AppServices services,
            DataRow empresa,
            ProcesoPagoResult cobro,
            TotalesFactura totales,
            string rotulo)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (cobro == null) throw new ArgumentNullException(nameof(cobro));
            if (totales == null) throw new ArgumentNullException(nameof(totales));

            string nombreEmpresa = Campo(empresa, "NOMBRE");

            var rdlc = new LocalReport { ReportPath = @"rptBaucher.rdlc" };

            rdlc.SetParameters(new[]
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
                new ReportParameter("MODOLECTURA",
                    services.ProcesosTarjetas.DescribirModoLectura(cobro.ModoLectura)),

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
                // no está fija en 12: hoy son 15.
                new ReportParameter("TARIFAIVA",   services.TarifaIva.ToString("0.##", CultureInfo.CurrentCulture)),
                new ReportParameter("BASEGRAVADA", Monto(totales.BaseImponible)),
                new ReportParameter("BASE0",       Monto(totales.Base0)),
                new ReportParameter("SUBTOTAL",    Monto(totales.Base0 + totales.BaseImponible)),
                new ReportParameter("IVA",         Monto(totales.Iva)),
                new ReportParameter("TOTAL",       Monto(totales.Total)),

                // --- Pie y firma -------------------------------------------
                new ReportParameter("RED",        Texto(cobro.RedAdquirente)),
                // El pinpad devuelve en TarjetaHabiente el modo de lectura ("CONTACTLESS"),
                // no el nombre del cliente. Se imprime en blanco: la línea NOMBRE queda
                // como espacio para escribir a mano, igual que C.I. y TELEFONO.
                new ReportParameter("HABIENTE",   ""),
                new ReportParameter("ROTULO",     string.IsNullOrWhiteSpace(rotulo) ? RotuloOriginal : rotulo),
                new ReportParameter("PUBLICIDAD", nombreEmpresa)
            });

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

        /// <summary>Monto con el formato del baucher ("$ 21,40").</summary>
        private static string Monto(decimal valor)
        {
            return "$ " + valor.ToString("N2", CultureInfo.CurrentCulture);
        }
    }
}
