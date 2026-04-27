//using System;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Xml;
//using System.Xml.Linq;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using FacturacionElectronicaNet.datos;

//namespace GenerarXml.Dto
//{
//    public class CFuncionesComprobantesElectronicos
//    {
   
//        private string tipoIdentificacionComprador;
     

//        public string ClaveAcceso(string fecha, string tipoComprobante, string rucEmpresa,
//            string producionPrueba, string estab, string ptoEmi, string secuencial, string idcod)
//        {
//            int suma = 0, factor = 7;
//            var claveAcceso = fecha + tipoComprobante + rucEmpresa + producionPrueba + estab + ptoEmi + secuencial + idcod + "1";
//            var clave = claveAcceso.ToCharArray();
//            foreach (var item in clave)
//            {
//                suma = suma + Convert.ToInt32(item.ToString()) * factor;
//                factor = factor - 1;
//                if (factor == 1)
//                    factor = 7;

//            }
//            var digitoVerificador = (suma % 11);
//            digitoVerificador = 11 - digitoVerificador;
//            if (digitoVerificador == 11)
//                digitoVerificador = 0;
//            if (digitoVerificador == 10)
//                digitoVerificador = 1;
//            return claveAcceso = claveAcceso + digitoVerificador.ToString();
//        }

//         private static XmlDocument GetXmlDocument(XDocument document)
//        {
//            if (document == null)
//                return null;
//            using (XmlReader xmlReader = document.CreateReader())
//            {
//                XmlDocument xmlDoc = new XmlDocument();
//                xmlDoc.Load(xmlReader);
//                if (document.Declaration != null)
//                {
//                    XmlDeclaration dec = xmlDoc.CreateXmlDeclaration(document.Declaration.Version,
//                        document.Declaration.Encoding, document.Declaration.Standalone);
//                    xmlDoc.InsertBefore(dec, xmlDoc.FirstChild);
//                }
//                return xmlDoc;

//            }
//        }
//        public XmlDocument GetXmlFacturaAsync(int ComprobanteID, bool FEProducionOPrueba, string TipoComprobante, string Idempresa)
//        {

//            XDocument m_doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
//            var GetComprobanteventa = context.TB_FACTURA.
//                FirstOrDefault(c => c.CodFactura == ComprobanteID );
//            if (GetComprobanteventa == null)
//                return GetXmlDocument(m_doc = null);

//            var GetEmpresa = context.TB_INSTITUCION.
//                FirstOrDefault(e => e.Ruc == Idempresa);
//            if (GetEmpresa == null)
//                return GetXmlDocument(m_doc = null);

//            var GetCliente = context.TB_CONTRIBUYENTE.FirstOrDefault(c => c.Cedula == GetComprobanteventa.Cedula);
//            if (GetCliente == null)
//                return GetXmlDocument(m_doc = null);

//            var GetDetalles = context.DetalleVenta.Where(d => d.FkComprovanteVenta.Equals(GetComprobanteventa.CodFactura)).ToList();
//            if (GetDetalles == null)
//                return GetXmlDocument(m_doc = null);


//            string xmlClaveAcceso;
//            ///Codigo de numero de 8 digitos 
//            //string comprobanteid = "00000000";
//            //int comprobanteidnum = "00000000".Length;
//            //int _comprobante = ComprobanteID.ToString().Length;
//            //string comNum8 = comprobanteid.Insert(comprobanteidnum - _comprobante, ComprobanteID.ToString());
//            //string comnum8 = comNum8.Substring(0, 8);

//            Random generator = new Random();
//            String numeroGenerado = generator.Next(0, 100000000).ToString("D8");
//            string ambiente = "";
//            string secuencial = GetComprobanteventa.ComprobantevNumero;
//            var fecha= Convert.ToDateTime( GetComprobanteventa.FechaLectuta);
//            if (FEProducionOPrueba == false)
//            {
//                //Prueba

//                xmlClaveAcceso = ClaveAcceso(fecha.ToString("ddMMyyyy"), "01", GetEmpresa.Ruc,
//                    "1", secuencial[0].ToString(), secuencial[1].ToString(), secuencial[2].ToString().Trim(), numeroGenerado);

//                GetComprobanteventa.Docsri = xmlClaveAcceso;
//                context.SaveChanges();

//                ambiente = "1";
//            }
//            else
//            {
//                //produccion

//                xmlClaveAcceso = ClaveAcceso(fecha.ToString("ddMMyyyy"), "01", GetEmpresa.Ruc
//,
//                    "2", secuencial[0].ToString(), secuencial[1].ToString(), secuencial[2].ToString().Trim(), numeroGenerado);

//                GetComprobanteventa.Docsri = xmlClaveAcceso;
//                context.SaveChanges();

//                ambiente = "2";
//            }
//            if (xmlClaveAcceso.Length == 49)
//            {
//                XElement m_com = new XElement("factura", new XAttribute("version", "1.1.0"), new XAttribute("id", "comprobante"));
//                XElement m_info_tributaria = new XElement("infoTributaria"
//                                                                        , new XElement("ambiente", ambiente)
//                                                                        , new XElement("tipoEmision", "1")
//                                                                        , new XElement("razonSocial", GetEmpresa.Nombre)
//                                                                        , new XElement("nombreComercial", GetEmpresa.Propietario)
//                                                                        , new XElement("ruc", GetEmpresa.Ruc)
//                                                                        , new XElement("claveAcceso", xmlClaveAcceso.ToString())
//                                                                        , new XElement("codDoc", "01")
//                                                                        , new XElement("estab", secuencial[0].ToString())
//                                                                        , new XElement("ptoEmi", secuencial[1].ToString())
//                                                                        , new XElement("secuencial", secuencial[2].ToString())
//                                                                        , new XElement("dirMatriz", GetEmpresa.Direccion)

//                                                          );
//                m_com.Add(m_info_tributaria);

//                string rucCiCliente;
//                if (GetCliente.Cedula.Length == 10 && !string.IsNullOrWhiteSpace(GetCliente.Cedula))
//                {
//                    tipoIdentificacionComprador = "05";
//                    rucCiCliente = GetCliente.Cedula;
//                }
//                else if (GetCliente.Cedula.Length == 13 && !string.IsNullOrWhiteSpace(GetCliente.Cedula))
//                {
//                    tipoIdentificacionComprador = "04";
//                    rucCiCliente = GetCliente.Cedula;
//                }
//                else
//                {
//                    return GetXmlDocument(m_doc = null);
//                }




//                XElement infoFactura = new XElement("infoFactura"
//                                                                  , new XElement("fechaEmision",Convert.ToDateTime( GetComprobanteventa.FechaLectuta)
//                                                                  , new XElement("dirEstablecimiento", GetEmpresa.Direccion)   
//                    // , new XElement("contribuyenteEspecial", GetEmpresa.EmpresaClasecontribuyente)
//                                                                  , new XElement("obligadoContabilidad", GetEmpresa.ObligadoContabilidad)
//                                                                  , new XElement("tipoIdentificacionComprador", tipoIdentificacionComprador)
//                                                                  , new XElement("razonSocialComprador", GetCliente.Nombre)
//                                                                  , new XElement("identificacionComprador", rucCiCliente)
//                                                                  , new XElement("direccionComprador", GetCliente.Direccion)
//                                                                  , new XElement("totalSinImpuestos", GetComprobanteventa.TotalPagar)
//                                                                  , new XElement("totalDescuento", "0.00")

//              ));
//                if (GetComprobanteventa.TotalPagar > 0.00M)
//                {
//                    XElement totalConImpuestos = new XElement("totalConImpuestos");
//                    XElement totalImpuesto = new XElement("totalImpuesto");

//                    totalImpuesto = new XElement("totalImpuesto");
//                    totalImpuesto.Add(new XElement("codigo", "2"));
//                    totalImpuesto.Add(new XElement("codigoPorcentaje", "0"));
//                    totalImpuesto.Add(new XElement("baseImponible", GetComprobanteventa.TotalPagar
//));
//                    totalImpuesto.Add(new XElement("valor", "0.00"));

//                    totalConImpuestos.Add(totalImpuesto);
//                    infoFactura.Add(totalConImpuestos);

//                }
           


//                infoFactura.Add(new XElement("propina", "0.00"));
//                infoFactura.Add(new XElement("importeTotal", GetComprobanteventa.TotalPagar));
//                infoFactura.Add(new XElement("moneda", "DÓLAR"));


//                XElement pagos = new XElement("pagos");
//                XElement pago = new XElement("pago");
//                pago = new XElement("pago");
//                pago.Add(new XElement("formaPago", "01"));
//                pago.Add(new XElement("total", GetComprobanteventa.TotalPagar  ));
//                pagos.Add(pago);
//                infoFactura.Add(pagos);

//                m_com.Add(infoFactura);



//                XElement detalles = new XElement("detalles");
//                XElement detalle = new XElement("detalle");

//                foreach (var item in GetDetalles)
//                {
//                    var GetProducto = context.TB_PERIODOS.FirstOrDefault(p => p.CodPeriodos.Equals(GetComprobanteventa.Periodo));
//                    detalle = new XElement("detalle");
//                    detalle.Add(new XElement("codigoPrincipal", GetProducto.CodPeriodos));
//                    detalle.Add(new XElement("codigoAuxiliar", GetProducto.CodPeriodos));
//                    detalle.Add(new XElement("descripcion", GetProducto.PeriodoPor));
//                    detalle.Add(new XElement("cantidad", item.DetallevCantidad));
//                    detalle.Add(new XElement("precioUnitario", GetComprobanteventa.CostoUnitarioBasico  ));
//                    detalle.Add(new XElement("descuento", "0.00"));
//                    detalle.Add(new XElement("precioTotalSinImpuesto", item.DetallevTotal));
//                    XElement impuestos = new XElement("impuestos");
//                    XElement impuesto = new XElement("impuesto");
//                    impuesto = new XElement("impuesto");
//                    impuesto.Add(new XElement("codigo", "2"));
//                    if (item.DetallevIva.Equals("0.00"))
//                    {
//                        impuesto.Add(new XElement("codigoPorcentaje", "0"));
//                        impuesto.Add(new XElement("tarifa", "0"));
//                        impuesto.Add(new XElement("baseImponible", item.DetallevTotal));
//                        impuesto.Add(new XElement("valor", "0.00"));
//                    }
//                    else
//                    {
//                        impuesto.Add(new XElement("codigoPorcentaje", "2"));
//                        impuesto.Add(new XElement("tarifa", "12"));
//                        impuesto.Add(new XElement("baseImponible", item.DetallevTotal));
//                        impuesto.Add(new XElement("valor", (item.DetallevIva)));
//                    }



//                    impuestos.Add(impuesto);
//                    detalle.Add(impuestos);
//                    detalles.Add(detalle);

//                }

//                m_com.Add(detalles);

//                XElement m_info_adicional = new XElement("infoAdicional");
//                XElement m_campo_adicional = new XElement("campoAdicional");
//                m_campo_adicional.Add(new XAttribute("nombre", "Email"));

//                if (!string.IsNullOrEmpty(GetCliente.Mail))
//                    m_campo_adicional.Value = GetCliente.Mail;
//                else
//                    m_campo_adicional.Value = "";


//                m_info_adicional.Add(m_campo_adicional);
//                m_com.Add(m_info_adicional);
//                //< campoAdicional nombre =”Email”> gfeguiguren@sri.gob.ec </ campoAdicional >

//                m_doc.Add(m_com);
//                return GetXmlDocument(m_doc);
//            }
//            return GetXmlDocument(m_doc = null);

//        }

//        public CRespuestaRecepcion RecepcionComprobantePrueba(String path)
//        {
//            var xmlByte = File.ReadAllBytes(path);

//            CRespuestaRecepcion resRecepcion = cSriws.RecepcionComprobantePrueba(Convert.ToBase64String(xmlByte));
//            return resRecepcion;
//        }
//        public CRespuestaAutorizacion AutorizacionComprobantePrueba(String claveAcceso)
//        {

//            CRespuestaAutorizacion resAutorizacion = cSriws.AutorizacionComprobantePrueba(claveAcceso);
//            return resAutorizacion;
//        }
//        public CRespuestaRecepcion RecepcionComprobante(String path)
//        {
//            var xmlByte = File.ReadAllBytes(path);

//            CRespuestaRecepcion resRecepcion = cSriws.RecepcionComprobanteooffline(Convert.ToBase64String(xmlByte));
//            return resRecepcion;
//        }
//        public CRespuestaAutorizacion AutorizacionComprobante(String claveAcceso)
//        {

//            CRespuestaAutorizacion resAutorizacion = cSriws.AutorizacionComprobanteoffline(claveAcceso);
//            return resAutorizacion;
//        }

//        public void xmlAutorizado(string CData, String patchOUT, string estadoAutorizado, string numeroAutorizado, string fechaAutorizado)
//        {

//            try
//            {

//                if (!string.IsNullOrEmpty(estadoAutorizado) && !string.IsNullOrEmpty(numeroAutorizado) && !string.IsNullOrEmpty(fechaAutorizado))
//                {
//                    XmlDocument doc = new XmlDocument();
//                    XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
//                    XmlNode root = doc.DocumentElement;
//                    doc.InsertBefore(xmlDeclaration, root);
//                    XmlElement raizAutorizacion = doc.CreateElement("autorizacion");
//                    doc.AppendChild(raizAutorizacion);
//                    XmlElement estado = doc.CreateElement("estado");
//                    estado.AppendChild(doc.CreateTextNode(estadoAutorizado));
//                    raizAutorizacion.AppendChild(estado);

//                    XmlElement numeroAutorizacion = doc.CreateElement("numeroAutorizacion");
//                    numeroAutorizacion.AppendChild(doc.CreateTextNode(numeroAutorizado));
//                    raizAutorizacion.AppendChild(numeroAutorizacion);

//                    XmlElement fechaAutorizacion = doc.CreateElement("fechaAutorizacion");
//                    fechaAutorizacion.SetAttribute("class", "fechaAutorizacion");
//                    fechaAutorizacion.AppendChild(doc.CreateTextNode(fechaAutorizado));
//                    raizAutorizacion.AppendChild(fechaAutorizacion);
//                    string xmlText = CData;
//                    var cdata = new XmlDocument();
//                    cdata.LoadXml(xmlText);

//                    XmlElement comprobante = doc.CreateElement("comprobante");
//                    comprobante.AppendChild(doc.CreateCDataSection(xmlText.ToString()));
//                    raizAutorizacion.AppendChild(comprobante);

//                    XmlElement mensaje = doc.CreateElement("mensajes");
//                    raizAutorizacion.AppendChild(mensaje);
//                    doc.Save(patchOUT);
//                }

//            }
//            catch (Exception)
//            {


//            }

//        }
//        public void xmlnoAutorizado(String patchCData, String patchOUT)
//        {


//            CRespuestaAutorizacion RespuestaAutorizacion = new CRespuestaAutorizacion();


//            foreach (var Respuesta_Autorizacion in RespuestaAutorizacion.Comprobantes)
//            {
//                if (Respuesta_Autorizacion.Estado.Equals("NO AUTORIZADO"))
//                {
//                    XmlDocument doc = new XmlDocument();

//                    XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
//                    XmlNode root = doc.DocumentElement;
//                    doc.InsertBefore(xmlDeclaration, root);
//                    XmlElement raizAutorizacion = doc.CreateElement("autorizacion");
//                    doc.AppendChild(raizAutorizacion);

//                    XmlElement estado = doc.CreateElement("estado");
//                    estado.AppendChild(doc.CreateTextNode(Respuesta_Autorizacion.Estado));
//                    raizAutorizacion.AppendChild(estado);

//                    XmlElement fechaAutorizacion = doc.CreateElement("fechaAutorizacion");
//                    fechaAutorizacion.SetAttribute("class", "fechaAutorizacion");
//                    fechaAutorizacion.AppendChild(doc.CreateTextNode(Respuesta_Autorizacion.FechaAutorizacion));
//                    raizAutorizacion.AppendChild(fechaAutorizacion);

//                    string xmlText = File.ReadAllText(patchCData);
//                    var cdata = new XmlDocument();
//                    cdata.LoadXml(xmlText);

//                    XmlElement comprobante = doc.CreateElement("comprobante");
//                    comprobante.AppendChild(doc.CreateCDataSection(xmlText.ToString()));
//                    raizAutorizacion.AppendChild(comprobante);
//                    XmlNode nodomensaje = doc.DocumentElement;

//                    XmlNode mensajes = doc.CreateElement("mensajes");
//                    XmlNode mensaje = doc.CreateElement("mensaje");
//                    XmlNode mensaje1 = doc.CreateElement("mensaje");

//                    doc.Save(patchOUT);
//                }
//            }
//        }

//        public string DeleteDirectory(string target_dir)
//        {
//            string[] files = Directory.GetFiles(target_dir); string[] dirs = Directory.GetDirectories(target_dir); foreach (string file in files)
//            {
//                File.SetAttributes(file, FileAttributes.Normal);
//                File.Delete(file);
//            }
//            foreach (string dir in dirs) { DeleteDirectory(dir); }
//            Directory.Delete(target_dir, false);
//            return "Se Borro";
//        }


//    }

//}
