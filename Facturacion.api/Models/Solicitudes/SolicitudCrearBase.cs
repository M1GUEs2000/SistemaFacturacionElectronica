namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudCrearBase
    {
        // Conexión a la base destino
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public bool TrustServerCertificate { get; set; }

        // Nombre del tenant (para seed de PARAMETROS_FACTURAS)
        public string NombreEmpresa { get; set; }

        // Auditoría opcional
        public string Usuario { get; set; }
        public string Ip { get; set; }
    }
}