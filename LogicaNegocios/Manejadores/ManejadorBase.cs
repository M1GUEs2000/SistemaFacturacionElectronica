using AccesoDatos.Abstractions;

namespace LogicaNegocios
{
    public abstract class ManejadorBase
    {
        protected readonly IConexionBD _conexion;
        protected readonly LogManejador _log;

        protected ManejadorBase(IConexionBD conexion, LogManejador log)
        {
            _conexion = conexion;
            _log = log;
        }

        protected int EjecutarConLog(string descripcion, string usuario, string ip, string sql)
        {
            _log.CrearLog(descripcion, usuario, ip, sql);
            return _conexion.Ejecutar(sql);
        }
    }
}
