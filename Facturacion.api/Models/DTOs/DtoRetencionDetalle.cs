namespace Facturacion.api.DTOs
{
    public class DtoRetencionDetalle
    {
        public string NumeroRetencion { get; set; }
        public string TipoImpuesto { get; set; }
        public string CodigoImpuesto { get; set; }
        public string BaseImponible { get; set; }
        public string PorcentajeRetencion { get; set; }
        public string ValorRetenido { get; set; }
        public string TipoOperacion { get; set; }
    }
}