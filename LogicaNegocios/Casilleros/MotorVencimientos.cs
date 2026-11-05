using System;
using System.Threading;

namespace LogicaNegocios.Casilleros
{
    public class MotorVencimientos
    {
        private readonly CasilleroAsignacionManejador _asignaciones;
        private readonly CasilleroNotificacionManejador _notificaciones;
        private readonly CasilleroParametrosManejador _parametros;
        private readonly LogManejador _log;
        private readonly TimeSpan _intervalo;

        private Timer _timer;
        private int _ejecutando;

        public MotorVencimientos(
            CasilleroAsignacionManejador asignaciones,
            CasilleroNotificacionManejador notificaciones,
            CasilleroParametrosManejador parametros,
            LogManejador log,
            TimeSpan? intervalo = null)
        {
            _asignaciones = asignaciones;
            _notificaciones = notificaciones;
            _parametros = parametros;
            _log = log;
            _intervalo = intervalo ?? TimeSpan.FromSeconds(5);
        }

        public void Iniciar()
        {
            _timer = new Timer(_ => Tick(), null, _intervalo, _intervalo);
        }

        public void Detener()
        {
            _timer?.Dispose();
        }

        private void Tick()
        {
            if (Interlocked.Exchange(ref _ejecutando, 1) == 1)
                return;

            try
            {
                int minutosAviso = _parametros.ObtenerMinutosNotificacion();

                var porNotificar = _asignaciones.ObtenerActivasPorNotificar(minutosAviso);
                foreach (System.Data.DataRow fila in porNotificar.Tables[0].Rows)
                {
                    int idAsignacion = Convert.ToInt32(fila["ID"]);
                    string codigo = fila["CODIGO"].ToString();
                    string cliente = fila["NOMBRECLIENTE"].ToString();
                    DateTime fin = Convert.ToDateTime(fila["FECHAFIN"]);

                    string mensaje = "El casillero " + codigo + " de " + cliente +
                                      " vence a las " + fin.ToString("HH:mm") + ".";

                    _notificaciones.Crear(idAsignacion, mensaje, "POR_VENCER");
                    _asignaciones.MarcarNotificada(idAsignacion);
                }

                var vencidas = _asignaciones.ObtenerVencidas();
                foreach (System.Data.DataRow fila in vencidas.Tables[0].Rows)
                {
                    int idAsignacion = Convert.ToInt32(fila["ID"]);
                    int idCasillero = Convert.ToInt32(fila["IDCASILLERO"]);
                    string codigo = fila["CODIGO"].ToString();
                    string cliente = fila["NOMBRECLIENTE"].ToString();

                    _asignaciones.MarcarVencida(idAsignacion, idCasillero);

                    _notificaciones.Crear(idAsignacion,
                        "El casillero " + codigo + " de " + cliente + " venció y debe liberarse.",
                        "VENCIDO");
                }
            }
            catch (Exception ex)
            {
                _log.CrearLog("CASILLEROS-MOTOR", "SISTEMA", "", "ERROR en tick de vencimientos: " + ex);
            }
            finally
            {
                Interlocked.Exchange(ref _ejecutando, 0);
            }
        }
    }
}
