using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios.Casilleros
{
    public class CasilleroNotificacionManejador : ManejadorBase
    {
        public CasilleroNotificacionManejador(IConexionBD conexion, LogManejador log) : base(conexion, log)
        {
        }

        public void Crear(int idAsignacion, string mensaje, string tipo)
        {
            _conexion.Ejecutar(
                @"INSERT INTO CASILLERO_NOTIFICACIONES (IDASIGNACION, MENSAJE, TIPO, FECHACREACION, MOSTRADA, LEIDA)
                  VALUES (@idAsignacion, @mensaje, @tipo, @fecha, @mostrada, @leida)",
                ("idAsignacion", idAsignacion), ("mensaje", mensaje), ("tipo", tipo),
                ("fecha", DateTime.Now), ("mostrada", false), ("leida", false));
        }

        public DataSet ObtenerNoMostradas()
        {
            return _conexion.Seleccionar(
                "SELECT ID, MENSAJE, TIPO FROM CASILLERO_NOTIFICACIONES WHERE MOSTRADA = @mostrada ORDER BY FECHACREACION",
                ("mostrada", false));
        }

        public void MarcarMostrada(int id)
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_NOTIFICACIONES SET MOSTRADA = @mostrada WHERE ID = @id",
                ("mostrada", true), ("id", id));
        }

        public DataSet ObtenerNoLeidas()
        {
            return _conexion.Seleccionar(
                "SELECT ID, MENSAJE, TIPO, FECHACREACION FROM CASILLERO_NOTIFICACIONES WHERE LEIDA = @leida ORDER BY FECHACREACION DESC",
                ("leida", false));
        }

        public void MarcarLeida(int id)
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_NOTIFICACIONES SET LEIDA = @leida WHERE ID = @id",
                ("leida", true), ("id", id));
        }

        public void MarcarTodasLeidas()
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_NOTIFICACIONES SET LEIDA = @leida WHERE LEIDA = @actual",
                ("leida", true), ("actual", false));
        }

        public void MarcarLeidasDeAsignacion(int idAsignacion)
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_NOTIFICACIONES SET LEIDA = @leida WHERE IDASIGNACION = @idAsignacion",
                ("leida", true), ("idAsignacion", idAsignacion));
        }

        public void MarcarLeidasAnterioresAHoy()
        {
            DateTime inicioDeHoy = DateTime.Today;
            _conexion.Ejecutar(
                "UPDATE CASILLERO_NOTIFICACIONES SET LEIDA = @leida WHERE LEIDA = @actual AND FECHACREACION < @inicioHoy",
                ("leida", true), ("actual", false), ("inicioHoy", inicioDeHoy));
        }
    }
}
