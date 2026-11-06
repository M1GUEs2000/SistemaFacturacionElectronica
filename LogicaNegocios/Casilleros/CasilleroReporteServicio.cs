using iTextSharp.text;
using iTextSharp.text.pdf;
using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LogicaNegocios.Casilleros
{
    public class FiltrosReporteCasilleros
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public string Cliente { get; set; }   // null/vacío = todos
        public int? Minutos { get; set; }     // null = todos
    }

    public class CasilleroReporteServicio
    {
        private readonly AppServices _services;

        public CasilleroReporteServicio(AppServices services)
        {
            _services = services;
        }

        public string GenerarPdf(DataTable detalle, List<FilaConsolidado> consolidado, FiltrosReporteCasilleros filtros, string rutaDestino)
        {
            var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
            var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

            using (var fs = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write))
            {
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                doc.Add(new Paragraph("Historial de casilleros", fuenteTitulo));
                doc.Add(new Paragraph(DescribirFiltros(filtros), fuenteNormal) { SpacingAfter = 10f });

                doc.Add(new Paragraph("Consolidado por tiempo", fuenteNegrita) { SpacingAfter = 4f });
                var tablaCons = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
                tablaCons.AddCell(Celda("Tiempo (min)", fuenteNegrita, true));
                tablaCons.AddCell(Celda("Cantidad", fuenteNegrita, true));
                int total = 0;
                foreach (var f in consolidado)
                {
                    tablaCons.AddCell(Celda(f.Minutos.ToString(), fuenteNormal, false));
                    tablaCons.AddCell(Celda(f.Cantidad.ToString(), fuenteNormal, false));
                    total += f.Cantidad;
                }
                tablaCons.AddCell(Celda("Total", fuenteNegrita, false));
                tablaCons.AddCell(Celda(total.ToString(), fuenteNegrita, false));
                doc.Add(tablaCons);

                doc.Add(new Paragraph("Detalle", fuenteNegrita) { SpacingBefore = 12f, SpacingAfter = 4f });
                var tabla = new PdfPTable(new float[] { 3f, 1.5f, 1.5f, 4f, 2f }) { WidthPercentage = 100 };
                foreach (var h in new[] { "Fecha", "Casillero", "Tiempo (min)", "Persona", "Estado" })
                    tabla.AddCell(Celda(h, fuenteNegrita, true));
                tabla.HeaderRows = 1;

                foreach (DataRow r in detalle.Rows)
                {
                    tabla.AddCell(Celda(Convert.ToDateTime(r["FECHA"]).ToString("dd/MM/yyyy HH:mm"), fuenteNormal, false));
                    tabla.AddCell(Celda(Convert.ToString(r["CASILLERO"]), fuenteNormal, false));
                    tabla.AddCell(Celda(Convert.ToString(r["TIEMPO"]), fuenteNormal, false));
                    tabla.AddCell(Celda(Convert.ToString(r["PERSONA"]), fuenteNormal, false));
                    tabla.AddCell(Celda(Convert.ToString(r["ESTADO"]), fuenteNormal, false));
                }
                doc.Add(tabla);

                doc.Close();
            }
            return rutaDestino;
        }

        public void EnviarPorCorreo(string destinatario, DataTable detalle, List<FilaConsolidado> consolidado,
            FiltrosReporteCasilleros filtros, string usuario)
        {
            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("Debe indicar el correo del destinatario.");
            var direccion = new MailAddress(destinatario.Trim()); // valida el formato

            string rutaPdf = Path.Combine(Path.GetTempPath(),
                "HistorialCasilleros_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");

            try
            {
                GenerarPdf(detalle, consolidado, filtros, rutaPdf);

                string nombreEmpresa = "Facturación";
                var dsEmpresa = _services.Empresa.MostrarEmpresa(usuario);
                if (dsEmpresa != null && dsEmpresa.Tables.Count > 0 && dsEmpresa.Tables[0].Rows.Count > 0
                    && dsEmpresa.Tables[0].Columns.Contains("NOMBRE"))
                {
                    string n = Convert.ToString(dsEmpresa.Tables[0].Rows[0]["NOMBRE"]).Trim();
                    if (n.Length > 0) nombreEmpresa = n;
                }

                var dsSmtp = _services.ParamFactura.ConsultarNombre(usuario);
                if (dsSmtp == null || dsSmtp.Tables.Count == 0 || dsSmtp.Tables[0].Rows.Count == 0)
                    throw new InvalidOperationException("No se encontraron parámetros SMTP.");

                var p = dsSmtp.Tables[0].Rows[0];
                string servidor = Convert.ToString(p["SMTPSERVER"]).Trim();
                string usuarioSmtp = Convert.ToString(p["SMTPUSER"]).Trim();
                string clave = Convert.ToString(p["SMTPPASS"]);
                if (string.IsNullOrWhiteSpace(servidor) || string.IsNullOrWhiteSpace(usuarioSmtp))
                    throw new InvalidOperationException("La configuración SMTP está incompleta.");
                if (!int.TryParse(Convert.ToString(p["SMTPPORT"]).Trim(), out int puerto))
                    throw new InvalidOperationException("SMTPPORT no es numérico.");

                using (var cliente = new SmtpClient(servidor)
                {
                    Port = puerto,
                    Credentials = new NetworkCredential(usuarioSmtp, clave),
                    EnableSsl = true
                })
                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(usuarioSmtp, nombreEmpresa);
                    msg.To.Add(direccion);
                    msg.Subject = "Historial de casilleros " + filtros.Desde.ToString("dd/MM/yyyy")
                                  + " - " + filtros.Hasta.ToString("dd/MM/yyyy");
                    msg.IsBodyHtml = true;
                    msg.Body = ArmarHtml(detalle, consolidado, filtros);
                    msg.Attachments.Add(new Attachment(rutaPdf));
                    cliente.Send(msg);
                }
            }
            finally
            {
                try { if (File.Exists(rutaPdf)) File.Delete(rutaPdf); }
                catch (Exception ex)
                {
                    _services.Log.CrearLog("No se pudo borrar el PDF temporal del historial de casilleros: " + ex.Message,
                        usuario ?? "", "", "");
                }
            }
        }

        private static string DescribirFiltros(FiltrosReporteCasilleros f)
        {
            return "Desde " + f.Desde.ToString("dd/MM/yyyy") + " hasta " + f.Hasta.ToString("dd/MM/yyyy")
                   + " | Persona: " + (string.IsNullOrWhiteSpace(f.Cliente) ? "Todas" : f.Cliente)
                   + " | Tiempo: " + (f.Minutos.HasValue ? f.Minutos.Value + " min" : "Todos");
        }

        private static string ArmarHtml(DataTable detalle, List<FilaConsolidado> consolidado, FiltrosReporteCasilleros f)
        {
            const string estiloTabla = "border-collapse:collapse;font-family:Segoe UI,Arial,sans-serif;font-size:13px";
            const string th = "border:1px solid #ccc;padding:5px 10px;background:#f0f0f0;text-align:left";
            const string td = "border:1px solid #ccc;padding:5px 10px";

            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:13px\">");
            sb.Append("<h3>Historial de casilleros</h3><p>")
              .Append(WebUtility.HtmlEncode(DescribirFiltros(f))).Append("</p>");

            sb.Append("<h4>Consolidado por tiempo</h4><table style=\"" + estiloTabla + "\">");
            sb.Append("<tr><th style=\"" + th + "\">Tiempo (min)</th><th style=\"" + th + "\">Cantidad</th></tr>");
            int total = 0;
            foreach (var c in consolidado)
            {
                sb.Append("<tr><td style=\"" + td + "\">").Append(c.Minutos)
                  .Append("</td><td style=\"" + td + "\">").Append(c.Cantidad).Append("</td></tr>");
                total += c.Cantidad;
            }
            sb.Append("<tr><td style=\"" + td + "\"><b>Total</b></td><td style=\"" + td + "\"><b>")
              .Append(total).Append("</b></td></tr></table>");

            sb.Append("<h4>Detalle</h4><table style=\"" + estiloTabla + "\"><tr>");
            foreach (var h in new[] { "Fecha", "Casillero", "Tiempo (min)", "Persona", "Estado" })
                sb.Append("<th style=\"" + th + "\">").Append(h).Append("</th>");
            sb.Append("</tr>");
            foreach (DataRow r in detalle.Rows)
            {
                sb.Append("<tr>");
                sb.Append("<td style=\"" + td + "\">").Append(Convert.ToDateTime(r["FECHA"]).ToString("dd/MM/yyyy HH:mm")).Append("</td>");
                sb.Append("<td style=\"" + td + "\">").Append(WebUtility.HtmlEncode(Convert.ToString(r["CASILLERO"]))).Append("</td>");
                sb.Append("<td style=\"" + td + "\">").Append(Convert.ToString(r["TIEMPO"])).Append("</td>");
                sb.Append("<td style=\"" + td + "\">").Append(WebUtility.HtmlEncode(Convert.ToString(r["PERSONA"]))).Append("</td>");
                sb.Append("<td style=\"" + td + "\">").Append(WebUtility.HtmlEncode(Convert.ToString(r["ESTADO"]))).Append("</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table><p>Se adjunta el reporte en PDF.</p></div>");
            return sb.ToString();
        }

        private static PdfPCell Celda(string texto, Font fuente, bool encabezado)
        {
            var celda = new PdfPCell(new Phrase(texto ?? "", fuente)) { Padding = 4f };
            if (encabezado) celda.BackgroundColor = new BaseColor(230, 230, 230);
            return celda;
        }
    }
}
