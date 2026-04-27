namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudLogin
    {
        public string Usuario { get; set; }

        public string ClaveIngreso { get; set; }

        public string UsuarioSistema { get; set; }

        public string Ip { get; set; }
    }
}