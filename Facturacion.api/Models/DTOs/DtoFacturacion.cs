namespace Facturacion.api.DTOs
{
    public class DtoFacturacion
    {
        public string Fecha { get; set; }
        public string FormaDePago { get; set; }
        public string Producto { get; set; }
        public string Cantidad { get; set; }
        public string Total { get; set; }
        public string Cliente { get; set; }
        public string Hora { get; set; }
        public string NumeroFactura { get; set; }
    }
}