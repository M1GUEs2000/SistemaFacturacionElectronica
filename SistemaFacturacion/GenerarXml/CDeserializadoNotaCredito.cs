using System.Collections.Generic;
using System.Xml.Serialization;

namespace xmlNotaCredito
{
    [XmlRoot(ElementName = "infoTributaria")]
    public class InfoTributariaNC
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

        [XmlElement(ElementName = "contribuyenteRimpe")]
        public string ContribuyenteRimpe { get; set; }

        public bool ShouldSerializeContribuyenteRimpe()
            => !string.IsNullOrWhiteSpace(ContribuyenteRimpe);
    }

    // totalConImpuestos de NC no lleva <tarifa> — clase separada de la de Factura
    [XmlRoot(ElementName = "totalImpuesto")]
    public class TotalImpuestoNC
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }
        [XmlElement(ElementName = "baseImponible")]
        public string BaseImponible { get; set; }
        [XmlElement(ElementName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "totalConImpuestos")]
    public class TotalConImpuestosNC
    {
        [XmlElement(ElementName = "totalImpuesto")]
        public List<TotalImpuestoNC> TotalImpuesto { get; set; }
    }

    // impuesto de detalle SÍ lleva <tarifa> (igual que Factura)
    [XmlRoot(ElementName = "impuesto")]
    public class ImpuestoNC
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "codigoPorcentaje")]
        public string CodigoPorcentaje { get; set; }
        [XmlElement(ElementName = "tarifa")]
        public string Tarifa { get; set; }
        [XmlElement(ElementName = "baseImponible")]
        public string BaseImponible { get; set; }
        [XmlElement(ElementName = "valor")]
        public string Valor { get; set; }
    }

    [XmlRoot(ElementName = "impuestos")]
    public class ImpuestosNC
    {
        [XmlElement(ElementName = "impuesto")]
        public List<ImpuestoNC> Impuesto { get; set; }
    }

    [XmlRoot(ElementName = "infoNotaCredito")]
    public class InfoNotaCredito
    {
        [XmlElement(ElementName = "fechaEmision")]
        public string FechaEmision { get; set; }
        [XmlElement(ElementName = "dirEstablecimiento")]
        public string DirEstablecimiento { get; set; }
        [XmlElement(ElementName = "tipoIdentificacionComprador")]
        public string TipoIdentificacionComprador { get; set; }
        [XmlElement(ElementName = "razonSocialComprador")]
        public string RazonSocialComprador { get; set; }
        [XmlElement(ElementName = "identificacionComprador")]
        public string IdentificacionComprador { get; set; }

        [XmlElement(ElementName = "obligadoContabilidad")]
        public string ObligadoContabilidad { get; set; }

        public bool ShouldSerializeObligadoContabilidad()
            => !string.IsNullOrWhiteSpace(ObligadoContabilidad);

        [XmlElement(ElementName = "codDocModificado")]
        public string CodDocModificado { get; set; }
        [XmlElement(ElementName = "numDocModificado")]
        public string NumDocModificado { get; set; }
        [XmlElement(ElementName = "fechaEmisionDocSustento")]
        public string FechaEmisionDocSustento { get; set; }
        [XmlElement(ElementName = "totalSinImpuestos")]
        public string TotalSinImpuestos { get; set; }
        [XmlElement(ElementName = "valorModificacion")]
        public string ValorModificacion { get; set; }
        [XmlElement(ElementName = "moneda")]
        public string Moneda { get; set; }
        [XmlElement(ElementName = "totalConImpuestos")]
        public TotalConImpuestosNC TotalConImpuestos { get; set; }
        [XmlElement(ElementName = "motivo")]
        public string Motivo { get; set; }
    }

    [XmlRoot(ElementName = "detalle")]
    public class DetalleNC
    {
        [XmlElement(ElementName = "codigoInterno")]
        public string CodigoInterno { get; set; }
        [XmlElement(ElementName = "descripcion")]
        public string Descripcion { get; set; }
        [XmlElement(ElementName = "cantidad")]
        public string Cantidad { get; set; }
        [XmlElement(ElementName = "precioUnitario")]
        public string PrecioUnitario { get; set; }
        [XmlElement(ElementName = "descuento")]
        public string Descuento { get; set; }
        [XmlElement(ElementName = "precioTotalSinImpuesto")]
        public string PrecioTotalSinImpuesto { get; set; }
        [XmlElement(ElementName = "impuestos")]
        public ImpuestosNC Impuestos { get; set; }
    }

    [XmlRoot(ElementName = "detalles")]
    public class DetallesNC
    {
        [XmlElement(ElementName = "detalle")]
        public List<DetalleNC> Detalle { get; set; }
    }

    [XmlRoot(ElementName = "campoAdicional")]
    public class CampoAdicionalNC
    {
        [XmlAttribute(AttributeName = "nombre")]
        public string Nombre { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "infoAdicional")]
    public class InfoAdicionalNC
    {
        [XmlElement(ElementName = "campoAdicional")]
        public List<CampoAdicionalNC> CampoAdicional { get; set; }
    }

    [XmlRoot(ElementName = "notaCredito")]
    public class NotaCredito
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlElement(ElementName = "infoTributaria")]
        public InfoTributariaNC InfoTributaria { get; set; }
        [XmlElement(ElementName = "infoNotaCredito")]
        public InfoNotaCredito InfoNotaCredito { get; set; }
        [XmlElement(ElementName = "detalles")]
        public DetallesNC Detalles { get; set; }
        [XmlElement(ElementName = "infoAdicional")]
        public InfoAdicionalNC InfoAdicional { get; set; }

        public bool ShouldSerializeInfoAdicional()
            => InfoAdicional?.CampoAdicional?.Count > 0;
    }
}
