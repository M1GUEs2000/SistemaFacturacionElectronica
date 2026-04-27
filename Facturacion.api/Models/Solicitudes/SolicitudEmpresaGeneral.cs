namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudEmpresaGeneral
    {
        public string NombreEmpresa { get; set; }

        public string CodigoEmpresa { get; set; }

        public string Provider { get; set; }

        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public string DbUser { get; set; }

        public string DbPassword { get; set; }

        public bool TrustServerCertificate { get; set; }

        public bool Activa { get; set; } = true;
    }
}