namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudNotaCreditoDetalle
    {
        public string NumeroNota { get; set; }
        public string Producto { get; set; }
        public string Cantidad { get; set; }
        public string Precio { get; set; }
        public string Iva { get; set; }
        public string NumeroFactura { get; set; }
    }
}