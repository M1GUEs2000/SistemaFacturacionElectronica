using System;

namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudPendiente
    {
        public string NumeroDocumento { get; set; }
        public string ClaveAcceso { get; set; }
        public string RutaXmlFirmado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int Intentos { get; set; }
        public string Estado { get; set; }
        public string Tipo { get; set; }

        public string Usuario { get; set; }
        public string Ip { get; set; }
    }
}