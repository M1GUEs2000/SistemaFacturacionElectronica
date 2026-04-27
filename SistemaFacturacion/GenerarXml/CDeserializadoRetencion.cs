using System.Collections.Generic;
using System.Xml.Serialization;

namespace xmlRetencion
{
    [XmlRoot(ElementName = "infoTributaria")]
    public class InfoTributariaRet
    {
        [XmlElement(ElementName = "ambiente")]
        public string Ambiente { get; set; }
        [XmlElement(ElementName = "tipoEmision")]
        public string TipoEmision { get; set; }
        [XmlElement(ElementName = "razonSocial")]
        public string RazonSocial { get; set; }
        [XmlElement(ElementName = "nombreComercial")]
        public string NombreComercial { get; set; }
        [XmlElement(ElementName = "ruc")]
        public string Ruc { get; set; }
        [XmlElement(ElementName = "claveAcceso")]
        public string ClaveAcceso { get; set; }
        [XmlElement(ElementName = "codDoc")]
        public string CodDoc { get; set; }
        [XmlElement(ElementName = "estab")]
        public string Estab { get; set; }
        [XmlElement(ElementName = "ptoEmi")]
        public string PtoEmi { get; set; }
        [XmlElement(ElementName = "secuencial")]
        public string Secuencial { get; set; }
        [XmlElement(ElementName = "dirMatriz")]
        public string DirMatriz { get; set; }
    }

    [XmlRoot(ElementName = "infoCompRetencion")]
    public class InfoCompRetencion
    {
        [XmlElement(ElementName = "fechaEmision")]
        public string FechaEmision { get; set; }
        [XmlElement(ElementName = "dirEstablecimiento")]
        public string DirEstablecimiento { get; set; }

        [XmlElement(ElementName = "contribuyenteEspecial")]
        public string ContribuyenteEspecial { get; set; }

        public bool ShouldSerializeContribuyenteEspecial()
            => !string.IsNullOrWhiteSpace(ContribuyenteEspecial);

        [XmlElement(ElementName = "obligadoContabilidad")]
        public string ObligadoContabilidad { get; set; }
        [XmlElement(ElementName = "tipoIdentificacionSujetoRetenido")]
        public string TipoIdentificacionSujetoRetenido { get; set; }
        [XmlElement(ElementName = "razonSocialSujetoRetenido")]
        public string RazonSocialSujetoRetenido { get; set; }
        [XmlElement(ElementName = "identificacionSujetoRetenido")]
        public string IdentificacionSujetoRetenido { get; set; }
        [XmlElement(ElementName = "periodoFiscal")]
        public string PeriodoFiscal { get; set; }
    }

    [XmlRoot(ElementName = "impuesto")]
    public class ImpuestoRetencion
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "codigoRetencion")]
        public string CodigoRetencion { get; set; }
        [XmlElement(ElementName = "baseImponible")]
        public string BaseImponible { get; set; }
        [XmlElement(ElementName = "porcentajeRetener")]
        public string PorcentajeRetener { get; set; }
        [XmlElement(ElementName = "valorRetenido")]
        public string ValorRetenido { get; set; }
        [XmlElement(ElementName = "codDocSustento")]
        public string CodDocSustento { get; set; }
        [XmlElement(ElementName = "numDocSustento")]
        public string NumDocSustento { get; set; }
        [XmlElement(ElementName = "fechaEmisionDocSustento")]
        public string FechaEmisionDocSustento { get; set; }
    }

    [XmlRoot(ElementName = "impuestos")]
    public class ImpuestosRetencion
    {
        [XmlElement(ElementName = "impuesto")]
        public List<ImpuestoRetencion> Impuesto { get; set; }
    }

    [XmlRoot(ElementName = "campoAdicional")]
    public class CampoAdicionalRet
    {
        [XmlAttribute(AttributeName = "nombre")]
        public string Nombre { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "infoAdicional")]
    public class InfoAdicionalRet
    {
        [XmlElement(ElementName = "campoAdicional")]
        public List<CampoAdicionalRet> CampoAdicional { get; set; }
    }

    [XmlRoot(ElementName = "comprobanteRetencion")]
    public class ComprobanteRetencion
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlElement(ElementName = "infoTributaria")]
        public InfoTributariaRet InfoTributaria { get; set; }
        [XmlElement(ElementName = "infoCompRetencion")]
        public InfoCompRetencion InfoCompRetencion { get; set; }
        [XmlElement(ElementName = "impuestos")]
        public ImpuestosRetencion Impuestos { get; set; }
        [XmlElement(ElementName = "infoAdicional")]
        public InfoAdicionalRet InfoAdicional { get; set; }

        public bool ShouldSerializeInfoAdicional()
            => InfoAdicional?.CampoAdicional?.Count > 0;
    }
}
