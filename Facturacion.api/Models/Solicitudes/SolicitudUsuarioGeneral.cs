namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudUsuarioGeneral
    {
        public int? Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }

        // Auditoría (inyectado por fetchApi)
        public string Usuario { get; set; }
        public string Ip { get; set; }
    }

    public class SolicitudLoginAdmin
    {
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
    }
}
