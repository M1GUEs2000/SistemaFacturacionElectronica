namespace Facturacion.api.DTOs
{
    public class DtoRetencionConsulta
    {
        public string NumeroRetencion { get; set; }
        public string FechaEmision { get; set; }
        public string NumeroFactura { get; set; }
        public string FechaFactura { get; set; }
        public string SujetoRetenido { get; set; }
        public string Identificacion { get; set; }
        public string TotalBaseImponible { get; set; }
        public string TotalRetencionRenta { get; set; }
        public string TotalRetencionIva { get; set; }
        public string TotalRetenido { get; set; }
    }
}