namespace AccesoDatosWeb.Configuracion
{
    public class DatabaseSettings
    {
        public string Provider { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool TrustServerCertificate { get; set; } = true;
    }
}