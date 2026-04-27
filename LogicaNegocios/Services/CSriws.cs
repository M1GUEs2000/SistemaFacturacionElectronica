using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LogicaNegocios.Services
{

    public class CErrorSri
    {
        public string Mensaje { get; set; }
        public string InformacionAdicional { get; set; }
        public string Tipo { get; set; }
    }


    public class CSriws
    {
        public CRespuestaRecepcion RecepcionComprobantePrueba(string xml)
        {
            String respuesta = "";
            CRespuestaRecepcion RespuestaRecepcionPrueba = new CRespuestaRecepcion();
            var soapEnvelopeXml = new XmlDocument();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl");

            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            soapEnvelopeXml.LoadXml(new CSoapXML().RecepcionComprobanteSoap(xml));

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(requestStream);
            }
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    respuesta = rd.ReadToEnd();

                    // ============================================
                    // PARSEO DEL SOAP COMPLETO
                    // ============================================
                    var soapResult = XDocument.Parse(respuesta);

                    // ============================================
                    // EXTRAER LOS ERRORES DEL SRI (mensaje, info, tipo)
                    // ============================================
                    var errores = new List<CErrorSri>();

                    var mensajesXml = soapResult.Descendants("mensaje");

                    foreach (var m in mensajesXml)
                    {
                        errores.Add(new CErrorSri
                        {
                            Mensaje = m.Element("mensaje")?.Value ?? "",
                            InformacionAdicional = m.Element("informacionAdicional")?.Value ?? "",
                            Tipo = m.Element("tipo")?.Value ?? ""
                        });
                    }

                    // ============================================
                    // DESERIALIZAR RESPUESTA NORMAL
                    // ============================================
                    var responseXml = soapResult.Descendants("RespuestaRecepcionComprobante").ToList();
                    foreach (var xmlDoc in responseXml)
                    {
                        RespuestaRecepcionPrueba =
                            (CRespuestaRecepcion)DeserializeFromXElement(xmlDoc, typeof(CRespuestaRecepcion));
                    }

                    // ============================================
                    // AGREGAR LOS ERRORES EXTRAÍDOS
                    // ============================================
                    RespuestaRecepcionPrueba.Errores = errores;
                }
            }

            return RespuestaRecepcionPrueba;

        }
        public CRespuestaRecepcion RecepcionLotePrueba(string loteXmlBase64)
        {
            string respuesta = "";
            CRespuestaRecepcion resultado = new CRespuestaRecepcion();
            var soapEnvelopeXml = new XmlDocument();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(
                "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl");

            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            soapEnvelopeXml.LoadXml(new CSoapXML().RecepcionLoteSoap(loteXmlBase64));

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(requestStream);
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    respuesta = rd.ReadToEnd();

                    CRespuestaRecepcion parseado = ParsearRespuestaRecepcion(respuesta);
                    if (parseado != null)
                        resultado = parseado;
                }
            }
            catch (WebException ex)
            {
                string respuestaError = LeerRespuestaError(ex);
                CRespuestaRecepcion parseado = ParsearRespuestaRecepcion(respuestaError);
                if (parseado != null)
                    return parseado;

                resultado.Estado = "ERROR";
                resultado.Errores = new List<CErrorSri>
                {
                    new CErrorSri
                    {
                        Mensaje = "HTTP " + ObtenerCodigoHttp(ex),
                        InformacionAdicional = string.IsNullOrWhiteSpace(respuestaError) ? ex.Message : respuestaError,
                        Tipo = "SOAP"
                    }
                };
            }

            return resultado;
        }

        public CRespuestaRecepcion RecepcionLoteOffline(string loteXmlBase64)
        {
            string respuesta = "";
            CRespuestaRecepcion resultado = new CRespuestaRecepcion();
            var soapEnvelopeXml = new XmlDocument();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(
                "https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl");

            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            soapEnvelopeXml.LoadXml(new CSoapXML().RecepcionLoteSoap(loteXmlBase64));

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(requestStream);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    respuesta = rd.ReadToEnd();
                    CRespuestaRecepcion parseado = ParsearRespuestaRecepcion(respuesta);
                    if (parseado != null)
                        resultado = parseado;
                }
            }
            catch (WebException ex)
            {
                string respuestaError = LeerRespuestaError(ex);
                CRespuestaRecepcion parseado = ParsearRespuestaRecepcion(respuestaError);
                if (parseado != null)
                    return parseado;

                resultado.Estado = "ERROR";
                resultado.Errores = new List<CErrorSri>
                {
                    new CErrorSri
                    {
                        Mensaje = "HTTP " + ObtenerCodigoHttp(ex),
                        InformacionAdicional = string.IsNullOrWhiteSpace(respuestaError) ? ex.Message : respuestaError,
                        Tipo = "SOAP"
                    }
                };
            }

            return resultado;
        }

        private string LeerRespuestaError(WebException ex)
        {
            if (ex == null || ex.Response == null)
                return "";

            try
            {
                using (var stream = ex.Response.GetResponseStream())
                {
                    if (stream == null)
                        return "";

                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private string ObtenerCodigoHttp(WebException ex)
        {
            var response = ex.Response as HttpWebResponse;
            if (response == null)
                return "0";

            return ((int)response.StatusCode).ToString();
        }

        private CRespuestaRecepcion ParsearRespuestaRecepcion(string xmlTexto)
        {
            if (string.IsNullOrWhiteSpace(xmlTexto))
                return null;

            try
            {
                var soapResult = XDocument.Parse(xmlTexto);
                CRespuestaRecepcion resultado = new CRespuestaRecepcion();

                var errores = new List<CErrorSri>();
                foreach (var m in soapResult.Descendants("mensaje"))
                {
                    string mensajeTexto = m.Element("mensaje") != null
                        ? m.Element("mensaje").Value
                        : (m.Value ?? "");

                    errores.Add(new CErrorSri
                    {
                        Mensaje = mensajeTexto,
                        InformacionAdicional = m.Element("informacionAdicional")?.Value ?? "",
                        Tipo = m.Element("tipo")?.Value ?? ""
                    });
                }

                var responseXml = soapResult.Descendants("RespuestaRecepcionComprobante").ToList();
                foreach (var xmlDoc in responseXml)
                {
                    resultado = (CRespuestaRecepcion)DeserializeFromXElement(xmlDoc, typeof(CRespuestaRecepcion));
                }

                if (resultado == null)
                    resultado = new CRespuestaRecepcion();

                if (errores.Count > 0)
                    resultado.Errores = errores;

                return resultado;
            }
            catch
            {
                return new CRespuestaRecepcion
                {
                    Estado = "ERROR",
                    Errores = new List<CErrorSri>
                    {
                        new CErrorSri
                        {
                            Mensaje = "Respuesta no parseable del SRI",
                            InformacionAdicional = xmlTexto,
                            Tipo = "SOAP"
                        }
                    }
                };
            }
        }

        public CRespuestaAutorizacion AutorizacionComprobantePrueba(string claveAcceso)
        {
            var soapEnvelopeXml = new XmlDocument();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(
                "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl"
            );

            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            soapEnvelopeXml.LoadXml(new CSoapXML().AutorizacionComprobanteSoap(claveAcceso));

            using (Stream requestStream = webRequest.GetRequestStream())
                soapEnvelopeXml.Save(requestStream);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    return ParsearRespuestaAutorizacion(rd.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                string respuestaError = LeerRespuestaError(ex);
                CRespuestaAutorizacion parseado = ParsearRespuestaAutorizacion(respuestaError);
                if (parseado != null) return parseado;

                return new CRespuestaAutorizacion
                {
                    Errores = new List<CErrorSri>
                    {
                        new CErrorSri
                        {
                            Mensaje = "HTTP " + ObtenerCodigoHttp(ex),
                            InformacionAdicional = string.IsNullOrWhiteSpace(respuestaError) ? ex.Message : respuestaError,
                            Tipo = "SOAP"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new CRespuestaAutorizacion
                {
                    Errores = new List<CErrorSri>
                    {
                        new CErrorSri { Mensaje = ex.Message, Tipo = "ERROR" }
                    }
                };
            }
        }
        public CRespuestaRecepcion RecepcionComprobanteooffline(string xml)
        {
            CRespuestaRecepcion resultado = new CRespuestaRecepcion();
            var soapEnvelopeXml = new XmlDocument();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline?wsdl");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            soapEnvelopeXml.LoadXml(new CSoapXML().RecepcionComprobanteSoap(xml));

            using (Stream requestStream = webRequest.GetRequestStream())
                soapEnvelopeXml.Save(requestStream);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    CRespuestaRecepcion parseado = ParsearRespuestaRecepcion(rd.ReadToEnd());
                    if (parseado != null) resultado = parseado;
                }
            }
            catch (WebException ex)
            {
                string respuestaError = LeerRespuestaError(ex);
                CRespuestaRecepcion parseado = ParsearRespuestaRecepcion(respuestaError);
                if (parseado != null) return parseado;

                resultado.Estado = "ERROR";
                resultado.Errores = new List<CErrorSri>
                {
                    new CErrorSri
                    {
                        Mensaje = "HTTP " + ObtenerCodigoHttp(ex),
                        InformacionAdicional = string.IsNullOrWhiteSpace(respuestaError) ? ex.Message : respuestaError,
                        Tipo = "SOAP"
                    }
                };
            }

            return resultado;
        }

        public CRespuestaAutorizacion AutorizacionComprobanteoffline(string claveAcceso)
        {
            var soapEnvelopeXml = new XmlDocument();

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline?wsdl");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            soapEnvelopeXml.LoadXml(new CSoapXML().AutorizacionComprobanteSoap(claveAcceso));

            using (Stream requestStream = webRequest.GetRequestStream())
                soapEnvelopeXml.Save(requestStream);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    return ParsearRespuestaAutorizacion(rd.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                string respuestaError = LeerRespuestaError(ex);
                CRespuestaAutorizacion parseado = ParsearRespuestaAutorizacion(respuestaError);
                if (parseado != null) return parseado;

                return new CRespuestaAutorizacion
                {
                    Errores = new List<CErrorSri>
                    {
                        new CErrorSri
                        {
                            Mensaje = "HTTP " + ObtenerCodigoHttp(ex),
                            InformacionAdicional = string.IsNullOrWhiteSpace(respuestaError) ? ex.Message : respuestaError,
                            Tipo = "SOAP"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new CRespuestaAutorizacion
                {
                    Errores = new List<CErrorSri>
                    {
                        new CErrorSri { Mensaje = ex.Message, Tipo = "ERROR" }
                    }
                };
            }
        }

        private CRespuestaAutorizacion ParsearRespuestaAutorizacion(string xmlTexto)
        {
            if (string.IsNullOrWhiteSpace(xmlTexto))
                return null;

            try
            {
                var soapResult = XDocument.Parse(xmlTexto);
                CRespuestaAutorizacion resultado = new CRespuestaAutorizacion();

                var errores = new List<CErrorSri>();
                foreach (var m in soapResult.Descendants("mensaje"))
                {
                    errores.Add(new CErrorSri
                    {
                        Mensaje = m.Element("mensaje")?.Value ?? m.Value ?? "",
                        InformacionAdicional = m.Element("informacionAdicional")?.Value ?? "",
                        Tipo = m.Element("tipo")?.Value ?? ""
                    });
                }

                var responseXml = soapResult.Descendants("RespuestaAutorizacionComprobante").ToList();
                foreach (var xmlDoc in responseXml)
                {
                    resultado = (CRespuestaAutorizacion)DeserializeFromXElement(xmlDoc, typeof(CRespuestaAutorizacion));
                }

                if (resultado == null)
                    resultado = new CRespuestaAutorizacion();

                if (errores.Count > 0)
                    resultado.Errores = errores;

                return resultado;
            }
            catch
            {
                return new CRespuestaAutorizacion
                {
                    Errores = new List<CErrorSri>
                    {
                        new CErrorSri
                        {
                            Mensaje = "Respuesta no parseable del SRI",
                            InformacionAdicional = xmlTexto,
                            Tipo = "SOAP"
                        }
                    }
                };
            }
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
            catch (Exception)
            {


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
        public object DeserializeFromXElement(XElement element, Type t)
        {
            try
            {
                using (XmlReader reader1 = element.CreateReader())
                {
                    XmlSerializer serializer = new XmlSerializer(t);
                    return serializer.Deserialize(reader1);
                }
            }
            catch (Exception ex)
            {
                string g = ex.Message;
                string k = ex.ToString();
                return null;
            }



        }
    }
}
