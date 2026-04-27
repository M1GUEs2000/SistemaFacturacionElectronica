using System;
using System.IO;
using System.Xml;

namespace LogicaNegocios.Services
{
    public class CFuncionesComprobantesElectronicos
    {

        CSriws cSriws = new CSriws();
        public string ClaveAcceso(string fecha, string tipoComprobante, string rucEmpresa,
        string producionPrueba, string estab, string ptoEmi, string secuencial, string idcod, string TipoEmision)
        {
            int suma = 0, factor = 7;
            var claveAcceso = fecha + tipoComprobante + rucEmpresa + producionPrueba + estab + ptoEmi + secuencial + idcod + TipoEmision;
            var clave = claveAcceso.ToCharArray();
            foreach (var item in clave)
            {

                suma = suma + Convert.ToInt32(item.ToString()) * factor;
                factor = factor - 1;
                if (factor == 1)
                    factor = 7;

            }
            var digitoVerificador = suma % 11;
            digitoVerificador = 11 - digitoVerificador;
            if (digitoVerificador == 11)
                digitoVerificador = 0;
            if (digitoVerificador == 10)
                digitoVerificador = 1;
            return claveAcceso = claveAcceso + digitoVerificador.ToString();
        }



        public CRespuestaRecepcion RecepcionComprobantePrueba(String path)
        {
            var xmlByte = File.ReadAllBytes(path);

            CRespuestaRecepcion resRecepcion = cSriws.RecepcionComprobantePrueba(Convert.ToBase64String(xmlByte));
            return resRecepcion;
        }
        public CRespuestaRecepcion RecepcionLotePrueba(string loteXml)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(loteXml);
            return cSriws.RecepcionLotePrueba(Convert.ToBase64String(bytes));
        }

        public CRespuestaRecepcion RecepcionLoteOffline(string loteXml)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(loteXml);
            return cSriws.RecepcionLoteOffline(Convert.ToBase64String(bytes));
        }

        public CRespuestaAutorizacion AutorizacionComprobantePrueba(String claveAcceso)
        {

            CRespuestaAutorizacion resAutorizacion = cSriws.AutorizacionComprobantePrueba(claveAcceso);
            return resAutorizacion;
        }
        public CRespuestaRecepcion RecepcionComprobante(String path)
        {
            var xmlByte = File.ReadAllBytes(path);

            CRespuestaRecepcion resRecepcion = cSriws.RecepcionComprobanteooffline(Convert.ToBase64String(xmlByte));
            return resRecepcion;
        }
        public CRespuestaAutorizacion AutorizacionComprobante(String claveAcceso)
        {

            CRespuestaAutorizacion resAutorizacion = cSriws.AutorizacionComprobanteoffline(claveAcceso);
            return resAutorizacion;
        }

        public void xmlAutorizado(String patchCData, String patchOUT, string estadoAutorizado, string numeroAutorizado, string fechaAutorizado)
        {

            try
            {

                if (!string.IsNullOrEmpty(estadoAutorizado) && !string.IsNullOrEmpty(numeroAutorizado) && !string.IsNullOrEmpty(fechaAutorizado))
                {
                    XmlDocument doc = new XmlDocument();
                    XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    XmlNode root = doc.DocumentElement;
                    doc.InsertBefore(xmlDeclaration, root);
                    XmlElement raizAutorizacion = doc.CreateElement("autorizacion");
                    doc.AppendChild(raizAutorizacion);
                    XmlElement estado = doc.CreateElement("estado");
                    estado.AppendChild(doc.CreateTextNode(estadoAutorizado));
                    raizAutorizacion.AppendChild(estado);

                    XmlElement numeroAutorizacion = doc.CreateElement("numeroAutorizacion");
                    numeroAutorizacion.AppendChild(doc.CreateTextNode(numeroAutorizado));
                    raizAutorizacion.AppendChild(numeroAutorizacion);

                    XmlElement fechaAutorizacion = doc.CreateElement("fechaAutorizacion");
                    fechaAutorizacion.SetAttribute("class", "fechaAutorizacion");
                    fechaAutorizacion.AppendChild(doc.CreateTextNode(fechaAutorizado));
                    raizAutorizacion.AppendChild(fechaAutorizacion);
                    string xmlText = File.ReadAllText(patchCData);
                    var cdata = new XmlDocument();
                    cdata.LoadXml(xmlText);

                    XmlElement comprobante = doc.CreateElement("comprobante");
                    comprobante.AppendChild(doc.CreateCDataSection(xmlText.ToString()));
                    raizAutorizacion.AppendChild(comprobante);

                    XmlElement mensaje = doc.CreateElement("mensajes");
                    raizAutorizacion.AppendChild(mensaje);
                    doc.Save(patchOUT);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error generando XML autorizado: " + ex.Message, ex);
            }

        }
        public void xmlnoAutorizado(String patchCData, String patchOUT)
        {


            CRespuestaAutorizacion RespuestaAutorizacion = new CRespuestaAutorizacion();


            foreach (var Respuesta_Autorizacion in RespuestaAutorizacion.Comprobantes)
            {
                if (Respuesta_Autorizacion.Estado.Equals("NO AUTORIZADO"))
                {
                    XmlDocument doc = new XmlDocument();

                    XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    XmlNode root = doc.DocumentElement;
                    doc.InsertBefore(xmlDeclaration, root);
                    XmlElement raizAutorizacion = doc.CreateElement("autorizacion");
                    doc.AppendChild(raizAutorizacion);

                    XmlElement estado = doc.CreateElement("estado");
                    estado.AppendChild(doc.CreateTextNode(Respuesta_Autorizacion.Estado));
                    raizAutorizacion.AppendChild(estado);

                    XmlElement fechaAutorizacion = doc.CreateElement("fechaAutorizacion");
                    fechaAutorizacion.SetAttribute("class", "fechaAutorizacion");
                    fechaAutorizacion.AppendChild(doc.CreateTextNode(Respuesta_Autorizacion.FechaAutorizacion));
                    raizAutorizacion.AppendChild(fechaAutorizacion);

                    string xmlText = File.ReadAllText(patchCData);
                    var cdata = new XmlDocument();
                    cdata.LoadXml(xmlText);

                    XmlElement comprobante = doc.CreateElement("comprobante");
                    comprobante.AppendChild(doc.CreateCDataSection(xmlText.ToString()));
                    raizAutorizacion.AppendChild(comprobante);
                    XmlNode nodomensaje = doc.DocumentElement;

                    XmlNode mensajes = doc.CreateElement("mensajes");
                    XmlNode mensaje = doc.CreateElement("mensaje");
                    XmlNode mensaje1 = doc.CreateElement("mensaje");

                    doc.Save(patchOUT);
                }
            }
        }
    }

}
