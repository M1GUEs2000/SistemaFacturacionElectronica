using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using AccesoDatosWeb.Configuracion;
using LogicaNegocios;

namespace Facturacion.api.App_Start
{
    public static class FacturacionConfig
    {
        public static FacturacionPaths GetFacturacionPaths(string empresa = null)
        {
            if (!string.IsNullOrWhiteSpace(empresa))
            {
                string basePath = ResolverRuta(
                    GetConfigValue("FACTURACION_BASE", "FacturacionPaths.Base", "~/App_Data/")
                );
                string sanitized = SanitizarNombreEmpresa(empresa);
                string empresaRoot = Path.Combine(basePath, "FACTURACION_" + sanitized);

                return new FacturacionPaths
                {
                    General = Path.Combine(empresaRoot, "GENERAL"),
                    Facturas = Path.Combine(empresaRoot, "FACTURAS"),
                    NotasCredito = Path.Combine(empresaRoot, "NOTASCREDITO"),
                    Retenciones = Path.Combine(empresaRoot, "RETENCIONES")
                };
            }

            // Backward compat: use old per-section keys when no empresa is provided
            return new FacturacionPaths
            {
                General = ResolverRuta(GetConfigValue("FACTURACION_GENERAL", "FacturacionPaths.General")),
                Facturas = ResolverRuta(GetConfigValue("FACTURACION_FACTURAS", "FacturacionPaths.Facturas")),
                NotasCredito = ResolverRuta(GetConfigValue("FACTURACION_NOTAS_CREDITO", "FacturacionPaths.NotasCredito")),
                Retenciones = ResolverRuta(GetConfigValue("FACTURACION_RETENCIONES", "FacturacionPaths.Retenciones"))
            };
        }

        public static string SanitizarNombreEmpresa(string nombre)
        {
            return Regex.Replace(nombre.Trim().ToUpperInvariant(), @"[^A-Z0-9]", "_");
        }

        public static DatabaseSettings GetDatabaseSettings()
        {
            return new DatabaseSettings
            {
                Provider = GetConfigValue("DB_PROVIDER", "DatabaseSettings.Provider"),
                Server = GetConfigValue("DB_SERVER", "DatabaseSettings.Server"),
                Database = GetConfigValue("DB_DATABASE", "DatabaseSettings.Database"),
                User = GetConfigValue("DB_USER", "DatabaseSettings.User"),
                Password = GetConfigValue("DB_PASSWORD", "DatabaseSettings.Password"),
                TrustServerCertificate = GetConfigBoolValue(
                    "DB_TRUST_SERVER_CERTIFICATE",
                    "DatabaseSettings.TrustServerCertificate",
                    false
                )
            };
        }

        private static string GetConfigValue(string envKey, string appSettingKey, string defaultValue = null)
        {
            var envValue = Environment.GetEnvironmentVariable(envKey);

            if (!string.IsNullOrWhiteSpace(envValue))
                return envValue;

            var appValue = ConfigurationManager.AppSettings[appSettingKey];

            if (!string.IsNullOrWhiteSpace(appValue))
                return appValue;

            if (defaultValue != null)
                return defaultValue;

            throw new Exception(
                $"No se encontró valor para la variable de entorno '{envKey}' ni para appSettings '{appSettingKey}'."
            );
        }

        private static bool GetConfigBoolValue(string envKey, string appSettingKey, bool defaultValue = false)
        {
            var value = Environment.GetEnvironmentVariable(envKey);

            if (string.IsNullOrWhiteSpace(value))
                value = ConfigurationManager.AppSettings[appSettingKey];

            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (bool.TryParse(value, out bool result))
                return result;

            throw new Exception(
                $"El valor configurado para '{envKey}' o '{appSettingKey}' no es un booleano válido."
            );
        }

        public static DatabaseSettings GetGeneralDatabaseSettings()
        {
            return new DatabaseSettings
            {
                Provider = ConfigurationManager.AppSettings["GeneralDb.Provider"],
                Server = ConfigurationManager.AppSettings["GeneralDb.Server"],
                Database = ConfigurationManager.AppSettings["GeneralDb.Database"],
                User = ConfigurationManager.AppSettings["GeneralDb.User"],
                Password = ConfigurationManager.AppSettings["GeneralDb.Password"],
                TrustServerCertificate = Convert.ToBoolean(
                    ConfigurationManager.AppSettings["GeneralDb.TrustServerCertificate"] ?? "false"
                )
            };
        }



        private static string ResolverRuta(string rutaConfig)
        {
            if (string.IsNullOrWhiteSpace(rutaConfig))
                throw new Exception("La ruta configurada está vacía o no existe en appSettings.");

            if (rutaConfig.StartsWith("~/") || rutaConfig.StartsWith("/"))
            {
                string rutaFisica = HostingEnvironment.MapPath(rutaConfig);

                if (string.IsNullOrWhiteSpace(rutaFisica))
                    throw new Exception($"No se pudo resolver la ruta física de '{rutaConfig}'.");

                return rutaFisica;
            }

            return rutaConfig;
        }
    }
}