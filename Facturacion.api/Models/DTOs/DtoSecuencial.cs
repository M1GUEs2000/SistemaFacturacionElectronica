namespace Facturacion.api.DTOs
{
    public class DtoSecuencial
    {
        public string TipoComprobante { get; set; }
        public long Secuencial { get; set; }
        public string SecuencialFormateado { get; set; }
        public string CodigoNumerico { get; set; }
        public string FechaActualizacion { get; set; }
    }
}