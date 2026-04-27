namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudCliente
    {
        public string Cedula { get; set; }

        public string Nombre { get; set; }

        public string Correo { get; set; }

        public string Direccion { get; set; }

        public string Telefono { get; set; }

        // Auditoría
        public string Usuario { get; set; }

        public string Ip { get; set; }
    }
}