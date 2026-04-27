using AccesoDatosWeb;
using AccesoDatosWeb.Configuracion;
using LogicaNegocios.Services;

namespace Facturacion.api.App_Start
{
    public class BuilderAppServices
    {
        public AppServices construir(DatabaseSettings tenant_db)
        {
            var paths = FacturacionConfig.GetFacturacionPaths();
            var conexion_bd = new ConexionBD(tenant_db);

            return new AppServices(paths, conexion_bd);
        }
    }
}