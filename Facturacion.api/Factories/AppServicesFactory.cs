using AccesoDatos.Abstractions;
using AccesoDatosWeb;
using AccesoDatosWeb.Configuracion;
using Facturacion.api.App_Start;
using Facturacion.api.Servicios;
using LogicaNegocios.Services;
using System;

namespace Facturacion.api.Factories
{
    public static class AppServicesFactory
    {
        public static AppServices Create(string empresa = null)
        {
            var facturacionPaths = FacturacionConfig.GetFacturacionPaths(empresa);
            DatabaseSettings databaseSettings;

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var servicioGeneral = new ServicioEmpresasGeneral();
                databaseSettings = servicioGeneral.obtenerDatabaseSettings(empresa);

                if (databaseSettings == null)
                    throw new Exception($"No se encontró la empresa '{empresa}' o está inactiva.");
            }
            else
            {
                databaseSettings = FacturacionConfig.GetDatabaseSettings();
            }

            IConexionBD conexion = new ConexionBD(databaseSettings);
            return new AppServices(facturacionPaths, conexion);
        }
    }
}
