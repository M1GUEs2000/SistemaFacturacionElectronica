namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudProcesarFacturaConsumidorFinal
    {
        public string NumeroFactura { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string CedulaCliente { get; set; }
        public string Usuario { get; set; }
        public string Ip { get; set; }
    }
}