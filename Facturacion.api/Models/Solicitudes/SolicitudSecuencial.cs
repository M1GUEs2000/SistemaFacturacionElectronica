namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudSecuencial
    {
        public string TipoComprobante { get; set; }
        public long Secuencial { get; set; }
        public string CodigoNumerico { get; set; }
        public string FechaActualizacion { get; set; }

        public string Usuario { get; set; }
        public string Ip { get; set; }
    }
}