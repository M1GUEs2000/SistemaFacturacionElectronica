using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;

namespace LogicaNegocios.Procesos
{
    /// <summary>
    /// Genera el .xlsx del resumen de ventas por forma de pago y producto.
    /// Escribe el formato directamente para no depender de que Excel esté instalado
    /// ni agregar una librería al proyecto.
    /// </summary>
    public static class ReporteFechasExcel
    {
        private const int EstiloTitulo = 1;
        private const int EstiloEncabezado = 2;
        private const int EstiloTexto = 3;
        private const int EstiloCantidad = 4;
        private const int EstiloMoneda = 5;
        private const int EstiloRango = 6;

        /// <param name="datos">Resultado de FacturacionManejador.ConsultarResumenFormaPagoProducto.</param>
        public static void Generar(DataTable datos, string nombreEmpresa, string fechaDesde, string fechaHasta, DateTime fechaReporte, Stream destino)
        {
            string titulo = "Reporte Por Fechas " + fechaReporte.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
            string rango = "Desde " + fechaDesde + " Hasta " + fechaHasta;

            if (!string.IsNullOrWhiteSpace(nombreEmpresa))
                titulo = nombreEmpresa + " - " + titulo;

            using (var zip = new ZipArchive(destino, ZipArchiveMode.Create, true))
            {
                Agregar(zip, "[Content_Types].xml", TiposContenido);
                Agregar(zip, "_rels/.rels", RelacionesPaquete);
                Agregar(zip, "xl/workbook.xml", LibroXml);
                Agregar(zip, "xl/_rels/workbook.xml.rels", RelacionesLibro);
                Agregar(zip, "xl/styles.xml", EstilosXml);
                Agregar(zip, "xl/worksheets/sheet1.xml", HojaXml(datos, titulo, rango));
            }
        }

        private static void Agregar(ZipArchive zip, string nombre, string contenido)
        {
            using (var escritor = new StreamWriter(zip.CreateEntry(nombre).Open(), new UTF8Encoding(false)))
            {
                escritor.Write(contenido);
            }
        }

        private static string HojaXml(DataTable datos, string titulo, string rango)
        {
            var filas = new StringBuilder();

            filas.Append("<row r=\"1\" ht=\"52\" customHeight=\"1\">")
                 .Append(CeldaTexto("A1", titulo, EstiloTitulo))
                 .Append("</row>");

            filas.Append("<row r=\"2\" ht=\"32\" customHeight=\"1\">")
                 .Append(CeldaTexto("A2", rango, EstiloRango))
                 .Append("</row>");

            filas.Append("<row r=\"3\">")
                 .Append(CeldaTexto("A3", "Fecha", EstiloEncabezado))
                 .Append(CeldaTexto("B3", "Formas de Pago", EstiloEncabezado))
                 .Append(CeldaTexto("C3", "Productos", EstiloEncabezado))
                 .Append(CeldaTexto("D3", "Cantidad", EstiloEncabezado))
                 .Append(CeldaTexto("E3", "Total", EstiloEncabezado))
                 .Append("</row>");

            int numero = 4;
            foreach (DataRow fila in datos.Rows)
            {
                filas.Append("<row r=\"").Append(numero).Append("\">")
                     .Append(CeldaTexto("A" + numero, fila["FECHADESDE"] + " - " + fila["FECHAHASTA"], EstiloTexto))
                     .Append(CeldaTexto("B" + numero, fila["FORMADEPAGO"].ToString(), EstiloTexto))
                     .Append(CeldaTexto("C" + numero, fila["PRODUCTO"].ToString(), EstiloTexto))
                     .Append(CeldaNumero("D" + numero, fila["CANTIDADES"], EstiloCantidad))
                     .Append(CeldaNumero("E" + numero, fila["TOTALES"], EstiloMoneda))
                     .Append("</row>");
                numero++;
            }

            // Tabla de totales por forma de pago, separada del detalle por una fila vacía.
            numero++;
            filas.Append("<row r=\"").Append(numero).Append("\">")
                 .Append(CeldaTexto("A" + numero, "Forma de Pago", EstiloEncabezado))
                 .Append(CeldaTexto("B" + numero, "Cantidad", EstiloEncabezado))
                 .Append(CeldaTexto("C" + numero, "Total", EstiloEncabezado))
                 .Append("</row>");
            numero++;

            var totalesPorFormaPago = datos.AsEnumerable()
                .GroupBy(fila => fila["FORMADEPAGO"].ToString());

            foreach (var formaPago in totalesPorFormaPago)
            {
                filas.Append("<row r=\"").Append(numero).Append("\">")
                     .Append(CeldaTexto("A" + numero, formaPago.Key, EstiloTexto))
                     .Append(CeldaNumero("B" + numero, formaPago.Sum(fila => Numero(fila["CANTIDADES"])), EstiloCantidad))
                     .Append(CeldaNumero("C" + numero, formaPago.Sum(fila => Numero(fila["TOTALES"])), EstiloMoneda))
                     .Append("</row>");
                numero++;
            }

            return Encabezado
                + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                + "<cols>"
                + "<col min=\"1\" max=\"1\" width=\"25\" customWidth=\"1\"/>"
                + "<col min=\"2\" max=\"2\" width=\"22\" customWidth=\"1\"/>"
                + "<col min=\"3\" max=\"3\" width=\"45\" customWidth=\"1\"/>"
                + "<col min=\"4\" max=\"5\" width=\"13\" customWidth=\"1\"/>"
                + "</cols>"
                + "<sheetData>" + filas + "</sheetData>"
                + "<mergeCells count=\"2\"><mergeCell ref=\"A1:E1\"/><mergeCell ref=\"A2:E2\"/></mergeCells>"
                + "</worksheet>";
        }

        private static string CeldaTexto(string referencia, string valor, int estilo)
        {
            return "<c r=\"" + referencia + "\" s=\"" + estilo + "\" t=\"inlineStr\"><is><t>"
                + SecurityElement.Escape(valor) + "</t></is></c>";
        }

        private static string CeldaNumero(string referencia, object valor, int estilo)
        {
            return "<c r=\"" + referencia + "\" s=\"" + estilo + "\"><v>"
                + Numero(valor).ToString(CultureInfo.InvariantCulture) + "</v></c>";
        }

        private static decimal Numero(object valor)
        {
            return valor == DBNull.Value ? 0m : Convert.ToDecimal(valor);
        }

        private const string Encabezado = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";

        private const string TiposContenido = Encabezado
            + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
            + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
            + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
            + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
            + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
            + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>"
            + "</Types>";

        private const string RelacionesPaquete = Encabezado
            + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
            + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
            + "</Relationships>";

        private const string LibroXml = Encabezado
            + "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\""
            + " xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">"
            + "<sheets><sheet name=\"Reporte\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
            + "</workbook>";

        private const string RelacionesLibro = Encabezado
            + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
            + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
            + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>"
            + "</Relationships>";

        // El orden de cellXfs define los valores de las constantes Estilo*.
        private const string EstilosXml = Encabezado
            + "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
            + "<numFmts count=\"1\"><numFmt numFmtId=\"164\" formatCode=\"0.00\"/></numFmts>"
            + "<fonts count=\"3\">"
            + "<font><sz val=\"11\"/><name val=\"Calibri\"/></font>"
            + "<font><b/><sz val=\"14\"/><name val=\"Calibri\"/></font>"
            + "<font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font>"
            + "</fonts>"
            + "<fills count=\"3\">"
            + "<fill><patternFill patternType=\"none\"/></fill>"
            + "<fill><patternFill patternType=\"gray125\"/></fill>"
            + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFD9E1F2\"/><bgColor indexed=\"64\"/></patternFill></fill>"
            + "</fills>"
            + "<borders count=\"2\">"
            + "<border><left/><right/><top/><bottom/><diagonal/></border>"
            + "<border><left style=\"thin\"/><right style=\"thin\"/><top style=\"thin\"/><bottom style=\"thin\"/><diagonal/></border>"
            + "</borders>"
            + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
            + "<cellXfs count=\"7\">"
            + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>"
            + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\" wrapText=\"1\"/></xf>"
            + "<xf numFmtId=\"0\" fontId=\"2\" fillId=\"2\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
            + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyBorder=\"1\"/>"
            + "<xf numFmtId=\"1\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyBorder=\"1\"/>"
            + "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyNumberFormat=\"1\" applyBorder=\"1\"/>"
            + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf>"
            + "</cellXfs>"
            + "</styleSheet>";
    }
}
