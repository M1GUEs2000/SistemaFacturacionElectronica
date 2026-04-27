using System;
using System.Configuration;
using AccesoDatosWeb.Configuracion;

namespace Facturacion.api.App_Start
{
    public static class ObtenerBaseGeneral
    {
        public static DatabaseSettings obtenerDatabaseSettingsGeneral()
        {
            return new DatabaseSettings
            {
                Provider = ConfigurationManager.AppSettings["DatabaseSettings.Provider"],
                Server = ConfigurationManager.AppSettings["DatabaseSettings.Server"],
                Database = ConfigurationManager.AppSettings["DatabaseSettings.Database"],
                User = ConfigurationManager.AppSettings["DatabaseSettings.User"],
                Password = ConfigurationManager.AppSettings["DatabaseSettings.Password"],
                TrustServerCertificate = Convert.ToBoolean(
                    ConfigurationManager.AppSettings["DatabaseSettings.TrustServerCertificate"] ?? "false"
                )
            };
        }
    }
}