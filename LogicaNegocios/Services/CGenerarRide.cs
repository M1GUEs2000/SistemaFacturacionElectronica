using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LogicaNegocios.Services
{
    public class CGenerarRide
    {
        static readonly BaseColor Azul = new BaseColor(54, 81, 167);

        public string GeneracionRideFacturaSRI(string patch, XDocument xml, Stream imagen)
            => GenerarPdf(patch, xml, imagen, "FACTURA", consultarSRI: true);

        public string GeneracionRideFacturaSRI_OfflineSinAutorizacion(string patch, XDocument xml, Stream imagen)
            => GenerarPdf(patch, xml, imagen, "FACTURA", consultarSRI: false);

        public string GeneracionRideNotaCreditoSRI(string patch, XDocument xml, Stream imagen)
            => GenerarPdf(patch, xml, imagen, "NOTA_CREDITO", consultarSRI: true);

        public string GeneracionRideNotaCreditoSRI_OfflineSinAutorizacion(string patch, XDocument xml, Stream imagen)
            => GenerarPdf(patch, xml, imagen, "NOTA_CREDITO", consultarSRI: false);

        public string GeneracionRideRetencionSRI(string patch, XDocument xml, Stream imagen)
            => GenerarPdf(patch, xml, imagen, "RETENCION", consultarSRI: true);

        public string GeneracionRideRetencionSRI_OfflineSinAutorizacion(string patch, XDocument xml, Stream imagen)
            => GenerarPdf(patch, xml, imagen, "RETENCION", consultarSRI: false);

        private string GenerarPdf(string carpeta, XDocument xml, Stream imagen, string tipo, bool consultarSRI)
        {
            try
            {
                string ambiente = X(xml, "ambiente");
                string claveAcceso = X(xml, "claveAcceso");
                string outPdf = Path.Combine(carpeta.TrimEnd('/', '\\'), claveAcceso + ".pdf");

                string fechaAut = consultarSRI ? ConsultarFechaAut(ambiente, claveAcceso) : null;

                using (var fs = new FileStream(outPdf, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var doc = new Document(PageSize.A4, 30, 30, 30, 30);
                    var writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    doc.Add(TablaEncabezado(xml, imagen, tipo, fechaAut, writer.DirectContent));

                    switch (tipo)
                    {
                        case "FACTURA": CuerpoFactura(doc, xml); break;
                        case "NOTA_CREDITO": CuerpoNotaCredito(doc, xml); break;
                        case "RETENCION": CuerpoRetencion(doc, xml); break;
                    }

                    var camposAdicionales = xml.Descendants("campoAdicional")
                        .Where(e => !(e.Attribute("nombre")?.Value ?? "").Contains("-transportista--"))
                        .Select(e => new KeyValuePair<string, string>(e.Attribute("nombre")?.Value ?? "", e.Value))
                        .ToList();

                    if (camposAdicionales.Count > 0)
                        doc.Add(TablaInfoAdicional(camposAdicionales));

                    doc.Close();
                }

                return outPdf;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private PdfPTable TablaEncabezado(XDocument xml, Stream imagen, string tipo, string fechaAut, PdfContentByte cb)
        {
            string razonSocial = X(xml, "razonSocial");
            string nombreCom = X(xml, "nombreComercial");
            if (string.IsNullOrWhiteSpace(nombreCom)) nombreCom = razonSocial;
            string ruc = X(xml, "ruc");
            string dirMatriz = X(xml, "dirMatriz");
            string dirSucursal = ObtenerDireccionSucursal(xml, tipo, dirMatriz);
            string rimpe = X(xml, "contribuyenteRimpe");
            string obligado = X(xml, "obligadoContabilidad");
            string ambiente = X(xml, "ambiente");
            string claveAcceso = X(xml, "claveAcceso");
            string numero = X(xml, "estab") + "-" + X(xml, "ptoEmi") + "-" + X(xml, "secuencial");

            string tituloDoc;
            switch (tipo)
            {
                case "FACTURA": tituloDoc = "F A C T U R A"; break;
                case "NOTA_CREDITO": tituloDoc = "N O T A   D E   CREDITO"; break;
                default: tituloDoc = "COMPROBANTE DE RETENCION"; break;
            }

            var bloqueIzquierdo = new PdfPTable(1);
            bloqueIzquierdo.WidthPercentage = 100;
            bloqueIzquierdo.DefaultCell.Border = 0;

            var tablaLogo = new PdfPTable(1);
            tablaLogo.WidthPercentage = 100;
            tablaLogo.DefaultCell.Border = 0;
            if (imagen != null)
            {
                imagen.Position = 0;
                var logo = Image.GetInstance(imagen);
                logo.ScaleToFit(220, 165);
                logo.Alignment = Element.ALIGN_LEFT;
                tablaLogo.AddCell(new PdfPCell(logo)
                {
                    Border = 0,
                    FixedHeight = 175f,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 0
                });
            }
            else
            {
                tablaLogo.AddCell(new PdfPCell(new Phrase(string.Empty, Fnt(8)))
                {
                    Border = 0,
                    FixedHeight = 175f
                });
            }

            bloqueIzquierdo.AddCell(new PdfPCell(tablaLogo)
            {
                Border = 0,
                Padding = 0,
                PaddingBottom = 24f
            });

            var tablaEmpresa = new PdfPTable(2);
            tablaEmpresa.WidthPercentage = 100;
            tablaEmpresa.SetWidths(new float[] { 36, 64 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase(nombreCom, Fnt(10, Font.BOLD))) { Colspan = 2, Padding = 4f, Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase("R.U.C.:", Fnt(9, Font.BOLD))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase(ruc, Fnt(12, Font.BOLD))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase("Direccion Matriz:", Fnt(8, Font.BOLD))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase(dirMatriz, Fnt(8))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase("Direccion Sucursal:", Fnt(8, Font.BOLD))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase(dirSucursal, Fnt(8))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase("Obligado a llevar contabilidad:", Fnt(8, Font.BOLD))) { Border = 0 });
            tablaEmpresa.AddCell(new PdfPCell(new Phrase(string.IsNullOrWhiteSpace(obligado) ? "NO" : obligado, Fnt(8))) { Border = 0 });
            if (!string.IsNullOrWhiteSpace(rimpe))
            {
                tablaEmpresa.AddCell(new PdfPCell(new Phrase("RIMPE:", Fnt(8, Font.BOLD))) { Border = 0 });
                tablaEmpresa.AddCell(new PdfPCell(new Phrase(rimpe, Fnt(8))) { Border = 0 });
            }

            bloqueIzquierdo.AddCell(new PdfPCell(tablaEmpresa)
            {
                Border = Rectangle.BOX,
                Padding = 4f
            });

            var bloqueDerecho = new PdfPTable(2);
            bloqueDerecho.WidthPercentage = 100;
            bloqueDerecho.DefaultCell.Border = Rectangle.BOX;
            bloqueDerecho.SetWidths(new float[] { 38, 62 });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("R.U.C.:", Fnt(12, Font.BOLD))) { BorderWidthBottom = 0, BorderWidthRight = 0, Padding = 3f });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase(ruc, Fnt(12))) { BorderWidthBottom = 0, BorderWidthLeft = 0, Padding = 3f });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase(tituloDoc, Fnt(14, Font.BOLD)))
            {
                Colspan = 2,
                HorizontalAlignment = Element.ALIGN_LEFT,
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                PaddingTop = 18f,
                PaddingBottom = 18f,
                PaddingLeft = 3f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("NO:", Fnt(12, Font.BOLD))) { BorderWidthTop = 0, BorderWidthBottom = 0, BorderWidthRight = 0, Padding = 3f });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase(numero, Fnt(12))) { BorderWidthTop = 0, BorderWidthBottom = 0, BorderWidthLeft = 0, Padding = 3f });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("NUMERO DE AUTORIZACION", Fnt(8, Font.BOLD)))
            {
                Colspan = 2,
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                PaddingTop = 18f,
                PaddingBottom = 8f,
                PaddingLeft = 3f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase(claveAcceso, Fnt(8)))
            {
                Colspan = 2,
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                PaddingLeft = 3f,
                PaddingBottom = 18f
            });
            var fraseFechaAut = new Phrase();
            fraseFechaAut.Leading = 9f;
            fraseFechaAut.Add(new Chunk("FECHA Y HORA DE\nAUTORIZACION", Fnt(8, Font.BOLD)));
            bloqueDerecho.AddCell(new PdfPCell(fraseFechaAut)
            {
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthRight = 0,
                Padding = 3f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase(string.IsNullOrEmpty(fechaAut) ? "NO AUTORIZADO" : fechaAut, Fnt(10)))
            {
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthLeft = 0,
                Padding = 3f,
                VerticalAlignment = Element.ALIGN_MIDDLE
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("AMBIENTE:", Fnt(8, Font.BOLD)))
            {
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthRight = 0,
                PaddingTop = 16f,
                PaddingLeft = 3f,
                PaddingBottom = 10f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase(ambiente == "1" ? "PRUEBAS" : "PRODUCCION", Fnt(10)))
            {
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthLeft = 0,
                PaddingTop = 16f,
                PaddingBottom = 10f,
                PaddingLeft = 3f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("EMISION:", Fnt(8, Font.BOLD)))
            {
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthRight = 0,
                PaddingTop = 10f,
                PaddingLeft = 3f,
                PaddingBottom = 14f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("NORMAL", Fnt(10)))
            {
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                BorderWidthLeft = 0,
                PaddingTop = 10f,
                PaddingBottom = 14f,
                PaddingLeft = 3f
            });
            bloqueDerecho.AddCell(new PdfPCell(new Phrase("CLAVE DE ACCESO", Fnt(10, Font.BOLD)))
            {
                Colspan = 2,
                HorizontalAlignment = Element.ALIGN_CENTER,
                BorderWidthTop = 0,
                BorderWidthBottom = 0,
                PaddingTop = 6f,
                PaddingBottom = 2f
            });

            var bar = new Barcode128();
            bar.Code = claveAcceso;
            var barcode = bar.CreateImageWithBarcode(cb, null, null);
            barcode.ScalePercent(95f, 115f);
            bloqueDerecho.AddCell(new PdfPCell(barcode)
            {
                Colspan = 2,
                HorizontalAlignment = Element.ALIGN_CENTER,
                BorderWidthTop = 0,
                PaddingTop = 0,
                PaddingBottom = 3f
            });

            var outer = new PdfPTable(2);
            outer.WidthPercentage = 100;
            outer.SetWidths(new float[] { 45, 55 });
            outer.SpacingAfter = 18f;
            outer.AddCell(new PdfPCell(bloqueIzquierdo) { Border = 0, PaddingRight = 10f });
            outer.AddCell(new PdfPCell(bloqueDerecho) { Border = 0 });
            return outer;
        }

        private void CuerpoFactura(Document doc, XDocument xml)
        {
            string razonC = Xp(xml, "infoFactura", "razonSocialComprador");
            string idC = Xp(xml, "infoFactura", "identificacionComprador");
            string fechaE = Xp(xml, "infoFactura", "fechaEmision");

            var tCli = new PdfPTable(4);
            tCli.WidthPercentage = 100;
            tCli.SetWidths(new float[] { 35, 30, 20, 15 });
            tCli.SpacingBefore = 10f;
            tCli.AddCell(Lb("Razon Social / Nombres y Apellidos:"));
            tCli.AddCell(new PdfPCell(new Phrase(razonC, Fnt(8))) { Colspan = 3 });
            tCli.AddCell(Lb("Identificacion:"));
            tCli.AddCell(Va(idC));
            tCli.AddCell(Lb("Fecha Emision:"));
            tCli.AddCell(Va(fechaE));
            doc.Add(tCli);

            string[] cols = { "Cod.Principal", "Cod.Auxiliar", "Cant.", "Descripcion", "P.Unitario", "Descuento", "P.T.Sin Imp." };
            float[] anchos = { 14, 14, 8, 35, 12, 12, 15 };
            doc.Add(THead(cols, anchos));

            var tItems = new PdfPTable(7);
            tItems.WidthPercentage = 100;
            tItems.SetWidths(anchos);

            decimal s0 = 0, s5 = 0, s12 = 0, s13 = 0, s14 = 0, s15 = 0, sEx = 0, sNo = 0;
            decimal v5 = 0, v12 = 0, v13 = 0, v14 = 0, v15 = 0, ice = 0;

            foreach (var det in xml.Descendants("detalle"))
            {
                string codP = det.Element("codigoPrincipal")?.Value ?? det.Element("codigoInterno")?.Value ?? "";
                string codA = det.Element("codigoAuxiliar")?.Value ?? det.Element("codigoAdicional")?.Value ?? "";
                tItems.AddCell(TD(codP));
                tItems.AddCell(TD(codA));
                tItems.AddCell(TDR(det.Element("cantidad")?.Value ?? ""));
                tItems.AddCell(TD(det.Element("descripcion")?.Value ?? ""));
                tItems.AddCell(TDR(det.Element("precioUnitario")?.Value ?? ""));
                tItems.AddCell(TDR(det.Element("descuento")?.Value ?? ""));
                tItems.AddCell(TDR(det.Element("precioTotalSinImpuesto")?.Value ?? ""));

                decimal baseD = Dec(det.Element("precioTotalSinImpuesto")?.Value);
                foreach (var imp in det.Elements("impuestos").Elements("impuesto"))
                {
                    string cod = imp.Element("codigo")?.Value ?? "";
                    string cp = imp.Element("codigoPorcentaje")?.Value ?? "";
                    decimal val = Dec(imp.Element("valor")?.Value);
                    if (cod == "3") { ice += val; continue; }
                    switch (cp)
                    {
                        case "0": s0 += baseD; break;
                        case "5": s5 += baseD; v5 += val; break;
                        case "2": s12 += baseD; v12 += val; break;
                        case "10": s13 += baseD; v13 += val; break;
                        case "3": s14 += baseD; v14 += val; break;
                        case "4": s15 += baseD; v15 += val; break;
                        case "6": sNo += baseD; break;
                        case "7": sEx += baseD; break;
                    }
                }
            }
            doc.Add(tItems);

            doc.Add(TotalesIVA(s0, s5, s12, s13, s14, s15, sEx, sNo, v5, v12, v13, v14, v15, ice,
                Xp(xml, "infoFactura", "totalSinImpuestos"),
                Xp(xml, "infoFactura", "totalDescuento"),
                Xp(xml, "infoFactura", "importeTotal"),
                "VALOR TOTAL"));

            string formaPago = xml.Descendants("formaPago").FirstOrDefault()?.Value ?? "";
            string total = xml.Descendants("total").FirstOrDefault()?.Value ?? "";
            var tFP = new PdfPTable(2);
            tFP.WidthPercentage = 60;
            tFP.HorizontalAlignment = Element.ALIGN_LEFT;
            tFP.SpacingBefore = 12f;
            tFP.AddCell(TH("Forma de Pago"));
            tFP.AddCell(TH("Valor"));
            tFP.AddCell(TD(DescripcionFormaPago(formaPago)));
            tFP.AddCell(TDR(total));
            doc.Add(tFP);
        }

        private void CuerpoNotaCredito(Document doc, XDocument xml)
        {
            string razonC = Xp(xml, "infoNotaCredito", "razonSocialComprador");
            string idC = Xp(xml, "infoNotaCredito", "identificacionComprador");
            string fechaE = Xp(xml, "infoNotaCredito", "fechaEmision");
            string docMod = Xp(xml, "infoNotaCredito", "numDocModificado");
            string fechaSus = Xp(xml, "infoNotaCredito", "fechaEmisionDocSustento");
            string motivo = Xp(xml, "infoNotaCredito", "motivo");
            string valMod = Xp(xml, "infoNotaCredito", "valorModificacion");
            string subtSI = Xp(xml, "infoNotaCredito", "totalSinImpuestos");

            var tCli = new PdfPTable(4);
            tCli.WidthPercentage = 100;
            tCli.SetWidths(new float[] { 28, 24, 24, 24 });
            tCli.SpacingBefore = 10f;
            tCli.AddCell(Lb("Razon Social:"));
            tCli.AddCell(new PdfPCell(new Phrase(razonC, Fnt(8))) { Colspan = 3 });
            tCli.AddCell(Lb("Identificacion:"));
            tCli.AddCell(Va(idC));
            tCli.AddCell(Lb("Fecha Emision:"));
            tCli.AddCell(Va(fechaE));
            tCli.AddCell(Lb("Doc. Modificado:"));
            tCli.AddCell(Va(docMod));
            tCli.AddCell(Lb("Fecha Sustento:"));
            tCli.AddCell(Va(fechaSus));
            doc.Add(tCli);

            float[] anchos = { 14, 14, 8, 35, 12, 12, 15 };
            doc.Add(THead(new[] { "Cod.Principal", "Cod.Auxiliar", "Cant.", "Descripcion", "P.Unitario", "Descuento", "P.Total" }, anchos));

            var tItems = new PdfPTable(7);
            tItems.WidthPercentage = 100;
            tItems.SetWidths(anchos);

            decimal s0 = 0, s5 = 0, s12 = 0, s13 = 0, s14 = 0, s15 = 0, sEx = 0, sNo = 0;
            decimal v5 = 0, v12 = 0, v13 = 0, v14 = 0, v15 = 0;

            foreach (var det in xml.Descendants("detalle"))
            {
                string codP = det.Element("codigoInterno")?.Value ?? det.Element("codigoPrincipal")?.Value ?? "";
                string codA = det.Element("codigoAuxiliar")?.Value ?? "";
                decimal cant = Dec(det.Element("cantidad")?.Value);
                if (cant <= 0) cant = 1;
                decimal ptsImp = Dec(det.Element("precioTotalSinImpuesto")?.Value);
                decimal ivaV = 0, iceV = 0;
                foreach (var imp in det.Elements("impuestos").Elements("impuesto"))
                {
                    if (imp.Element("codigo")?.Value == "3") iceV += Dec(imp.Element("valor")?.Value);
                    else ivaV += Dec(imp.Element("valor")?.Value);
                }
                decimal totalD = ptsImp + ivaV + iceV;
                decimal unitD = Math.Round(totalD / cant, 6);

                tItems.AddCell(TD(codP));
                tItems.AddCell(TD(codA));
                tItems.AddCell(TDR(cant.ToString("0.00")));
                tItems.AddCell(TD(det.Element("descripcion")?.Value ?? ""));
                tItems.AddCell(TDR(unitD.ToString("0.000000")));
                tItems.AddCell(TDR(det.Element("descuento")?.Value ?? "0"));
                tItems.AddCell(TDR(totalD.ToString("0.00")));

                var impPrincipal = det.Elements("impuestos").Elements("impuesto").FirstOrDefault();
                string cp = impPrincipal?.Element("codigoPorcentaje")?.Value ?? "0";
                decimal vl = Dec(impPrincipal?.Element("valor")?.Value);
                switch (cp)
                {
                    case "0": s0 += ptsImp; break;
                    case "5": s5 += ptsImp; v5 += vl; break;
                    case "2": s12 += ptsImp; v12 += vl; break;
                    case "10": s13 += ptsImp; v13 += vl; break;
                    case "3": s14 += ptsImp; v14 += vl; break;
                    case "4": s15 += ptsImp; v15 += vl; break;
                    case "6": sNo += ptsImp; break;
                    case "7": sEx += ptsImp; break;
                }
            }
            doc.Add(tItems);

            if (!string.IsNullOrWhiteSpace(motivo))
            {
                var tMot = new PdfPTable(1);
                tMot.WidthPercentage = 100;
                tMot.SpacingBefore = 6f;
                tMot.AddCell(new PdfPCell(new Phrase("Motivo: " + motivo, Fnt(8, Font.BOLD))));
                doc.Add(tMot);
            }

            doc.Add(TotalesIVA(s0, s5, s12, s13, s14, s15, sEx, sNo, v5, v12, v13, v14, v15, 0,
                subtSI, "0", valMod, "VALOR TOTAL (NC)"));
        }

        private void CuerpoRetencion(Document doc, XDocument xml)
        {
            string idSujeto = Xp(xml, "infoCompRetencion", "identificacionSujetoRetenido");
            string rzSujeto = Xp(xml, "infoCompRetencion", "razonSocialSujetoRetenido");
            string fechaE = Xp(xml, "infoCompRetencion", "fechaEmision");
            string periodo = Xp(xml, "infoCompRetencion", "periodoFiscal");

            var tInfo = new PdfPTable(2);
            tInfo.WidthPercentage = 100;
            tInfo.SpacingBefore = 10f;
            tInfo.AddCell(Lb("RUC/CI:")); tInfo.AddCell(Va(idSujeto));
            tInfo.AddCell(Lb("Razon Social:")); tInfo.AddCell(Va(rzSujeto));
            tInfo.AddCell(Lb("Fecha Emision:")); tInfo.AddCell(Va(fechaE));
            tInfo.AddCell(Lb("Periodo Fiscal:")); tInfo.AddCell(Va(periodo));
            doc.Add(tInfo);

            float[] anchos = { 22, 16, 10, 10, 17, 12, 13 };
            doc.Add(THead(new[] { "Doc. Sustento", "Fecha Emision", "Tipo", "Codigo", "Base Imponible", "% Retencion", "Valor Retenido" }, anchos));

            var tItems = new PdfPTable(7);
            tItems.WidthPercentage = 100;
            tItems.SetWidths(anchos);

            decimal totalRenta = 0, totalIVA = 0;
            foreach (var imp in xml.Descendants("impuesto"))
            {
                string cod = imp.Element("codigo")?.Value ?? "";
                string tipo = cod == "1" ? "RENTA" : "IVA";
                decimal baseI = Dec(imp.Element("baseImponible")?.Value);
                decimal porc = Dec(imp.Element("porcentajeRetener")?.Value);
                decimal val = Dec(imp.Element("valorRetenido")?.Value);
                if (tipo == "RENTA") totalRenta += val;
                else totalIVA += val;

                tItems.AddCell(TD(imp.Element("numDocSustento")?.Value ?? ""));
                tItems.AddCell(TD(imp.Element("fechaEmisionDocSustento")?.Value ?? ""));
                tItems.AddCell(TD(tipo));
                tItems.AddCell(TD(imp.Element("codigoRetencion")?.Value ?? ""));
                tItems.AddCell(TDR(baseI.ToString("0.00")));
                tItems.AddCell(TDR(porc.ToString("0.00")));
                tItems.AddCell(TDR(val.ToString("0.00")));
            }
            doc.Add(tItems);

            var tTot = new PdfPTable(2);
            tTot.HorizontalAlignment = Element.ALIGN_RIGHT;
            tTot.WidthPercentage = 45;
            tTot.SpacingBefore = 6f;
            tTot.SetWidths(new float[] { 65, 35 });
            tTot.AddCell(Lb("TOTAL RETENCION RENTA")); tTot.AddCell(TDR(totalRenta.ToString("0.00")));
            tTot.AddCell(Lb("TOTAL RETENCION IVA")); tTot.AddCell(TDR(totalIVA.ToString("0.00")));
            tTot.AddCell(new PdfPCell(new Phrase("TOTAL RETENIDO", Fnt(8, Font.BOLD, BaseColor.WHITE))) { BackgroundColor = Azul });
            tTot.AddCell(new PdfPCell(new Phrase((totalRenta + totalIVA).ToString("0.00"), Fnt(8, Font.BOLD, BaseColor.WHITE))) { BackgroundColor = Azul, HorizontalAlignment = Element.ALIGN_RIGHT });
            doc.Add(tTot);
        }

        private PdfPTable TotalesIVA(
            decimal s0, decimal s5, decimal s12, decimal s13, decimal s14, decimal s15,
            decimal sEx, decimal sNo, decimal v5, decimal v12, decimal v13, decimal v14, decimal v15,
            decimal ice, string subtSinImp, string descuento, string importeTotal, string labelTotal)
        {
            var t = new PdfPTable(2);
            t.HorizontalAlignment = Element.ALIGN_RIGHT;
            t.WidthPercentage = 45;
            t.SpacingBefore = 8f;
            t.SetWidths(new float[] { 65, 35 });

            AddSubtotal(t, "SUBTOTAL IVA 0%", s0, 0);
            AddSubtotal(t, "SUBTOTAL IVA 5%", s5, v5);
            AddSubtotal(t, "SUBTOTAL IVA 12%", s12, v12);
            AddSubtotal(t, "SUBTOTAL IVA 13%", s13, v13);
            AddSubtotal(t, "SUBTOTAL IVA 14%", s14, v14);
            AddSubtotal(t, "SUBTOTAL IVA 15%", s15, v15);
            AddSubtotal(t, "EXENTO DE IVA", sEx, 0);
            AddSubtotal(t, "NO OBJETO IVA", sNo, 0);
            if (ice > 0) { t.AddCell(Lb("ICE")); t.AddCell(TDR(ice.ToString("0.00"))); }

            t.AddCell(Lb("SUBTOTAL SIN IMPUESTOS")); t.AddCell(TDR(subtSinImp));
            t.AddCell(Lb("DESCUENTO")); t.AddCell(TDR(descuento));
            t.AddCell(new PdfPCell(new Phrase(labelTotal, Fnt(8, Font.BOLD, BaseColor.WHITE))) { BackgroundColor = Azul });
            t.AddCell(new PdfPCell(new Phrase(importeTotal, Fnt(8, Font.BOLD, BaseColor.WHITE))) { BackgroundColor = Azul, HorizontalAlignment = Element.ALIGN_RIGHT });
            return t;
        }

        private static void AddSubtotal(PdfPTable t, string label, decimal subtotal, decimal iva)
        {
            if (subtotal <= 0) return;
            t.AddCell(new PdfPCell(new Phrase(label, FontFactory.GetFont("Arial", 8, Font.BOLD))));
            t.AddCell(new PdfPCell(new Phrase(subtotal.ToString("0.00"), FontFactory.GetFont("Arial", 8))) { HorizontalAlignment = Element.ALIGN_RIGHT });
            if (iva > 0)
            {
                string ivaLabel = label.Replace("SUBTOTAL ", "");
                t.AddCell(new PdfPCell(new Phrase(ivaLabel, FontFactory.GetFont("Arial", 8))));
                t.AddCell(new PdfPCell(new Phrase(iva.ToString("0.00"), FontFactory.GetFont("Arial", 8))) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }
        }

        private static PdfPTable TablaInfoAdicional(List<KeyValuePair<string, string>> campos)
        {
            var t = new PdfPTable(2);
            t.WidthPercentage = 60;
            t.HorizontalAlignment = Element.ALIGN_LEFT;
            t.SpacingBefore = 8f;
            t.SetWidths(new float[] { 40, 60 });
            t.AddCell(new PdfPCell(new Phrase("Informacion Adicional", FontFactory.GetFont("Arial", 10, Font.BOLD))) { Colspan = 2, BorderWidthBottom = 1f });
            foreach (var kv in campos)
            {
                t.AddCell(new PdfPCell(new Phrase(kv.Key, FontFactory.GetFont("Arial", 8, Font.BOLD))));
                t.AddCell(new PdfPCell(new Phrase(kv.Value, FontFactory.GetFont("Arial", 8))));
            }
            return t;
        }

        private static PdfPTable THead(string[] cols, float[] widths)
        {
            var t = new PdfPTable(cols.Length);
            t.WidthPercentage = 100;
            t.SetWidths(widths);
            t.SpacingBefore = 10f;
            foreach (var col in cols)
                t.AddCell(new PdfPCell(new Phrase(col, FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.WHITE)))
                { BackgroundColor = new BaseColor(54, 81, 167), HorizontalAlignment = Element.ALIGN_CENTER });
            return t;
        }

        private static PdfPCell Lb(string text) => new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 8, Font.BOLD)));
        private static PdfPCell Va(string text) => new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 8)));
        private static PdfPCell TD(string text) => new PdfPCell(new Phrase(text ?? "", FontFactory.GetFont("Arial", 8)));
        private static PdfPCell TH(string text) => new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.WHITE)))
            { BackgroundColor = new BaseColor(54, 81, 167) };
        private static PdfPCell TDR(string text) => new PdfPCell(new Phrase(text ?? "", FontFactory.GetFont("Arial", 8)))
            { HorizontalAlignment = Element.ALIGN_RIGHT };
        private static PdfPCell CeldaNB(string text, int size, int style = Font.NORMAL) =>
            new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", size, style))) { Border = 0, PaddingBottom = 2 };

        private static Font Fnt(int size, int style = Font.NORMAL, BaseColor color = null)
            => FontFactory.GetFont("Arial", size, style, color ?? BaseColor.BLACK);

        private static string X(XDocument d, string name)
            => d.Descendants(name).FirstOrDefault()?.Value ?? "";

        private static string Xp(XDocument d, string parent, string name)
            => d.Descendants(parent).FirstOrDefault()?.Element(name)?.Value ?? "";

        private static decimal Dec(string s)
        {
            decimal.TryParse(s ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out decimal r);
            return r;
        }

        private static string ObtenerDireccionSucursal(XDocument xml, string tipo, string dirMatriz)
        {
            string parent =
                tipo == "FACTURA" ? "infoFactura" :
                tipo == "NOTA_CREDITO" ? "infoNotaCredito" :
                "infoCompRetencion";

            string dirSucursal = Xp(xml, parent, "dirEstablecimiento");
            return string.IsNullOrWhiteSpace(dirSucursal) ? dirMatriz : dirSucursal;
        }

        private static string DescripcionFormaPago(string codigo)
        {
            switch (codigo)
            {
                case "01": return "SIN UTILIZACION DEL SISTEMA FINANCIERO";
                case "15": return "COMPENSACION DE DEUDAS";
                case "16": return "TARJETA DE DEBITO";
                case "17": return "DINERO ELECTRONICO";
                case "18": return "TARJETA PREPAGO";
                case "19": return "TARJETA DE CREDITO";
                case "20": return "OTROS CON UTILIZACION DEL SISTEMA FINANCIERO";
                case "21": return "ENDOSO DE TITULOS";
                default: return codigo;
            }
        }

        private static string ConsultarFechaAut(string ambiente, string claveAcceso)
        {
            try
            {
                var cf = new CFuncionesComprobantesElectronicos();
                var aut = ambiente == "2"
                    ? cf.AutorizacionComprobante(claveAcceso)
                    : cf.AutorizacionComprobantePrueba(claveAcceso);

                if (aut?.Comprobantes?.Count > 0 && aut.Comprobantes[0].Estado == "AUTORIZADO")
                    return aut.Comprobantes[0].FechaAutorizacion;
                return "";
            }
            catch { return ""; }
        }
    }
}
