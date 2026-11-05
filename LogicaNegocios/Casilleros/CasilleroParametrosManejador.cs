using AccesoDatos.Abstractions;
using System;

namespace LogicaNegocios.Casilleros
{
    public class CasilleroParametrosManejador : ManejadorBase
    {
        private const int MinutosPorDefecto = 10;

        public CasilleroParametrosManejador(IConexionBD conexion, LogManejador log) : base(conexion, log)
        {
        }

        public int ObtenerMinutosNotificacion()
        {
            var ds = _conexion.Seleccionar("SELECT TOP 1 ID, MINUTOSNOTIFICACIONPREVIA FROM CASILLERO_PARAMETROS");
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return MinutosPorDefecto;

            return Convert.ToInt32(ds.Tables[0].Rows[0]["MINUTOSNOTIFICACIONPREVIA"]);
        }

        public void GuardarMinutosNotificacion(int minutos, string usuario, string ip)
        {
            if (minutos <= 0)
                throw new ArgumentException("Los minutos de notificación deben ser mayores a cero.");

            var ds = _conexion.Seleccionar("SELECT TOP 1 ID FROM CASILLERO_PARAMETROS");
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                _conexion.Ejecutar(
                    "INSERT INTO CASILLERO_PARAMETROS (MINUTOSNOTIFICACIONPREVIA) VALUES (@min)",
                    ("min", minutos));
            }
            else
            {
                int id = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"]);
                _conexion.Ejecutar(
                    "UPDATE CASILLERO_PARAMETROS SET MINUTOSNOTIFICACIONPREVIA = @min WHERE ID = @id",
                    ("min", minutos), ("id", id));
            }

            _log.CrearLog("Se actualizaron los minutos de notificación de casilleros a " + minutos, usuario, ip, "");
        }
    }
}
