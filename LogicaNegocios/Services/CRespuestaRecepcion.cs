using System.Collections.Generic;
using System.Xml.Serialization;

namespace LogicaNegocios.Services
{
    [XmlRoot("RespuestaRecepcionComprobante")]
    public class CRespuestaRecepcion
    {
        [XmlElement("estado")]
        public string Estado { get; set; }

        [XmlArray(ElementName = "comprobantes")]
        [XmlArrayItem(typeof(Comprobante), ElementName = "comprobante")]
        public List<Comprobante> Comprobantes { get; set; }

        public List<CErrorSri> Errores { get; set; } = new List<CErrorSri>();

    }

    public class Comprobante
    {
        [XmlElement("claveAcceso")]
        public string ClaveAcceso { get; set; }

        [XmlArray(ElementName = "mensajes")]
        [XmlArrayItem(typeof(Mensaje), ElementName = "mensaje")]
        public List<Mensaje> Mensajes { get; set; }
    }

    public class Mensaje
    {
        [XmlElement("identificador")]
        public string Identificador { get; set; }

        [XmlElement("mensaje")]
        public string mensaje { get; set; }

        [XmlElement("informacionAdicional")]
        public string InformacionAdicional { get; set; }

        [XmlElement("tipo")]
        public string Tipo { get; set; }
    }
}
