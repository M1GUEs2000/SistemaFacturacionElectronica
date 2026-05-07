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
        public string Identificador { get; set; }
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
                    var errores = ExtraerMensajesSri(soapResult);

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

                var errores = ExtraerMensajesSri(soapResult);

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

                var errores = ExtraerMensajesSri(soapResult);

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

        private List<CErrorSri> ExtraerMensajesSri(XDocument soapResult)
        {
            var errores = new List<CErrorSri>();
            if (soapResult == null)
                return errores;

            foreach (var m in soapResult.Descendants("mensaje"))
            {
                bool esNodoMensajeSri =
                    m.Element("identificador") != null ||
                    m.Element("mensaje") != null ||
                    m.Element("informacionAdicional") != null ||
                    m.Element("tipo") != null;

                if (!esNodoMensajeSri)
                    continue;

                errores.Add(new CErrorSri
                {
                    Identificador = m.Element("identificador")?.Value ?? "",
                    Mensaje = m.Element("mensaje")?.Value ?? "",
                    InformacionAdicional = m.Element("informacionAdicional")?.Value ?? "",
                    Tipo = m.Element("tipo")?.Value ?? ""
                });
            }

            return errores;
        }
    }
}
