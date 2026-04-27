namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudProcesarPendienteDocumento
    {
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string Fecha { get; set; }
        public string Usuario { get; set; }
        public string Ip { get; set; }
        public string Proceso { get; set; }
    }
}