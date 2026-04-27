using System.Collections.Generic;

namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudNota
    {
        public string NumeroFactura { get; set; }
        public string Motivo { get; set; }

        public List<ProductoNotaCreditoSolicitud> Productos { get; set; }
            = new List<ProductoNotaCreditoSolicitud>();

        public string Usuario { get; set; }
        public string Ip { get; set; }
    }

    public class ProductoNotaCreditoSolicitud
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Valor { get; set; }
        public decimal Total { get; set; }
    }
}