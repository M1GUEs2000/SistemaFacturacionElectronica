using AccesoDatos.Abstractions;
using System;

namespace LogicaNegocios.Casilleros
{
    public class CasilleroParametrosManejador : ManejadorBase
    {
        private const int MinutosPorDefecto = 10;
        private const string SonidoPorDefecto = "campana.wav";

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

        public string ObtenerSonidoNotificacion()
        {
            var ds = _conexion.Seleccionar("SELECT TOP 1 SONIDONOTIFICACION FROM CASILLERO_PARAMETROS");
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0]["SONIDONOTIFICACION"] == DBNull.Value)
                return SonidoPorDefecto;

            string sonido = ds.Tables[0].Rows[0]["SONIDONOTIFICACION"].ToString();
            return string.IsNullOrWhiteSpace(sonido) ? SonidoPorDefecto : sonido;
        }

        public void GuardarSonidoNotificacion(string clave, string usuario, string ip)
        {
            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException("Debe seleccionar un sonido de notificación.");

            var ds = _conexion.Seleccionar("SELECT TOP 1 ID FROM CASILLERO_PARAMETROS");
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                _conexion.Ejecutar(
                    "INSERT INTO CASILLERO_PARAMETROS (MINUTOSNOTIFICACIONPREVIA, SONIDONOTIFICACION) VALUES (@min, @sonido)",
                    ("min", MinutosPorDefecto), ("sonido", clave));
            }
            else
            {
                int id = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"]);
                _conexion.Ejecutar(
                    "UPDATE CASILLERO_PARAMETROS SET SONIDONOTIFICACION = @sonido WHERE ID = @id",
                    ("sonido", clave), ("id", id));
            }

            _log.CrearLog("Se actualizó el sonido de notificación de casilleros a " + clave, usuario, ip, "");
        }
    }
}
