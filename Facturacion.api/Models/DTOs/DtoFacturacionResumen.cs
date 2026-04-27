namespace Facturacion.api.DTOs
{
    public class DtoFacturacionResumen
    {
        public string Fecha { get; set; }
        public string FormaDePago { get; set; }
        public string Cantidades { get; set; }
        public string Totales { get; set; }
        public string Cliente { get; set; }
        public string Cedula { get; set; }
        public string Hora { get; set; }
        public string NumeroFactura { get; set; }
    }
}