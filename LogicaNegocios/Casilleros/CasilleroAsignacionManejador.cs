using AccesoDatos.Abstractions;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace LogicaNegocios.Casilleros
{
    public class ResultadoAsignacion
    {
        public int IdAsignacion { get; set; }
        public int IdCasillero { get; set; }
        public string CodigoCasillero { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class CasilleroAsignacionManejador : ManejadorBase
    {
        private static readonly Regex RegexNombreValido = new Regex(@"^[a-zA-ZÀ-ÿñÑ][a-zA-ZÀ-ÿñÑ\s'.-]*$", RegexOptions.Compiled);

        private readonly CasilleroManejador _casilleros;
        private readonly CasilleroNotificacionManejador _notificaciones;
        private readonly CasilleroHistorialManejador _historial;
        private const int MaxIntentosAsignacion = 3;

        public CasilleroAsignacionManejador(
            IConexionBD conexion, LogManejador log, CasilleroManejador casilleros, CasilleroNotificacionManejador notificaciones)
            : base(conexion, log)
        {
            _casilleros = casilleros;
            _notificaciones = notificaciones;
            _historial = new CasilleroHistorialManejador(conexion, log);
        }

        public ResultadoAsignacion AsignarAutomatico(string nombreCliente, int minutosSolicitados, string usuario, string ip)
        {
            ValidarNuevaAsignacion(nombreCliente, minutosSolicitados);

            for (int intento = 0; intento < MaxIntentosAsignacion; intento++)
            {
                var disponibles = _casilleros.ConsultarDisponibles();
                if (disponibles.Tables.Count == 0 || disponibles.Tables[0].Rows.Count == 0)
                    throw new InvalidOperationException("No hay casilleros disponibles en este momento.");

                int idCasillero = Convert.ToInt32(disponibles.Tables[0].Rows[0]["ID"]);
                string codigo = disponibles.Tables[0].Rows[0]["CODIGO"].ToString();

                if (!_casilleros.IntentarOcupar(idCasillero))
                    continue;

                return CrearAsignacion(idCasillero, codigo, nombreCliente, minutosSolicitados, usuario, ip);
            }

            throw new InvalidOperationException("No se pudo asignar un casillero disponible, intente nuevamente.");
        }

        public ResultadoAsignacion AsignarACasillero(int idCasillero, string codigo, string nombreCliente, int minutosSolicitados, string usuario, string ip)
        {
            ValidarNuevaAsignacion(nombreCliente, minutosSolicitados);

            if (!_casilleros.IntentarOcupar(idCasillero))
                throw new InvalidOperationException("Ese casillero ya no está disponible, seguramente otro cajero lo tomó primero. Elige otra tarjeta.");

            return CrearAsignacion(idCasillero, codigo, nombreCliente, minutosSolicitados, usuario, ip);
        }

        private void ValidarNuevaAsignacion(string nombreCliente, int minutosSolicitados)
        {
            if (string.IsNullOrWhiteSpace(nombreCliente))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (!RegexNombreValido.IsMatch(nombreCliente.Trim()))
                throw new ArgumentException("El nombre del cliente solo puede contener letras y espacios, sin números ni caracteres especiales.");
            if (minutosSolicitados <= 0)
                throw new ArgumentException("El tiempo solicitado debe ser mayor a cero.");
            if (ExisteClienteActivo(nombreCliente))
                throw new InvalidOperationException("Ya hay un casillero activo asignado a ese nombre.");
        }

        private bool ExisteClienteActivo(string nombreCliente)
        {
            var ds = _conexion.Seleccionar(
                @"SELECT TOP 1 ID FROM CASILLERO_ASIGNACIONES
                  WHERE ESTADO IN ('ACTIVA','VENCIDA') AND NOMBRECLIENTE = @nombre",
                ("nombre", nombreCliente.Trim()));
            return ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
        }

        private ResultadoAsignacion CrearAsignacion(int idCasillero, string codigo, string nombreCliente, int minutosSolicitados, string usuario, string ip)
        {
            DateTime inicio = DateTime.Now;
            DateTime fin = inicio.AddMinutes(minutosSolicitados);

            _conexion.Ejecutar(
                @"INSERT INTO CASILLERO_ASIGNACIONES
                    (IDCASILLERO, NOMBRECLIENTE, MINUTOSSOLICITADOS, FECHAINICIO, FECHAFIN, ESTADO, NOTIFICADO, USUARIO, FECHACREACION)
                  VALUES
                    (@idCasillero, @nombre, @minutos, @inicio, @fin, @estado, @notificado, @usuario, @creacion)",
                ("idCasillero", idCasillero), ("nombre", nombreCliente), ("minutos", minutosSolicitados),
                ("inicio", inicio), ("fin", fin), ("estado", "ACTIVA"), ("notificado", false),
                ("usuario", (object)usuario ?? ""), ("creacion", DateTime.Now));

            var ds = _conexion.Seleccionar(
                "SELECT TOP 1 ID FROM CASILLERO_ASIGNACIONES WHERE IDCASILLERO = @id ORDER BY ID DESC",
                ("id", idCasillero));
            int idAsignacion = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"]);

            _historial.RegistrarAsignacion(
                idAsignacion, idCasillero, codigo, nombreCliente.Trim(), minutosSolicitados, inicio, fin, usuario);

            _log.CrearLog(
                "Casillero " + codigo + " asignado a " + nombreCliente + " por " + minutosSolicitados + " minutos",
                usuario, ip, "");

            return new ResultadoAsignacion
            {
                IdAsignacion = idAsignacion,
                IdCasillero = idCasillero,
                CodigoCasillero = codigo,
                FechaFin = fin
            };
        }

        public int FinalizarManual(int idAsignacion, string usuario, string ip)
        {
            var ds = _conexion.Seleccionar(
                "SELECT IDCASILLERO FROM CASILLERO_ASIGNACIONES WHERE ID = @id", ("id", idAsignacion));
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                throw new InvalidOperationException("La asignación no existe.");

            int idCasillero = Convert.ToInt32(ds.Tables[0].Rows[0]["IDCASILLERO"]);

            int filas = _conexion.Ejecutar(
                "UPDATE CASILLERO_ASIGNACIONES SET ESTADO = @estado WHERE ID = @id",
                ("estado", "FINALIZADA"), ("id", idAsignacion));

            _historial.ActualizarEstado(idAsignacion, "FINALIZADA");
            _casilleros.Liberar(idCasillero);
            _notificaciones.MarcarLeidasDeAsignacion(idAsignacion);

            _log.CrearLog("Se liberó manualmente la asignación " + idAsignacion, usuario, ip, "");

            return filas;
        }

        public DataSet ListarActivas()
        {
            return _conexion.Seleccionar(
                @"SELECT a.ID, a.NOMBRECLIENTE, a.FECHAINICIO, a.FECHAFIN, a.ESTADO, c.CODIGO
                  FROM CASILLERO_ASIGNACIONES a INNER JOIN CASILLEROS c ON a.IDCASILLERO = c.ID
                  WHERE a.ESTADO = @estado
                  ORDER BY a.FECHAFIN",
                ("estado", "ACTIVA"));
        }

        public DataSet ObtenerActivasPorNotificar(int minutosAntes)
        {
            DateTime ahora = DateTime.Now;
            DateTime umbral = ahora.AddMinutes(minutosAntes);

            return _conexion.Seleccionar(
                @"SELECT a.ID, a.NOMBRECLIENTE, a.FECHAFIN, c.CODIGO
                  FROM CASILLERO_ASIGNACIONES a INNER JOIN CASILLEROS c ON a.IDCASILLERO = c.ID
                  WHERE a.ESTADO = @estado AND a.NOTIFICADO = @notificado
                    AND a.FECHAFIN > @ahora AND a.FECHAFIN <= @umbral",
                ("estado", "ACTIVA"), ("notificado", false), ("ahora", ahora), ("umbral", umbral));
        }

        public DataSet ObtenerVencidas()
        {
            return _conexion.Seleccionar(
                @"SELECT a.ID, a.IDCASILLERO, a.NOMBRECLIENTE, c.CODIGO
                  FROM CASILLERO_ASIGNACIONES a INNER JOIN CASILLEROS c ON a.IDCASILLERO = c.ID
                  WHERE a.ESTADO = @estado AND a.FECHAFIN <= @ahora",
                ("estado", "ACTIVA"), ("ahora", DateTime.Now));
        }

        public void MarcarNotificada(int idAsignacion)
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_ASIGNACIONES SET NOTIFICADO = @notificado WHERE ID = @id",
                ("notificado", true), ("id", idAsignacion));
        }

        public void MarcarVencida(int idAsignacion, int idCasillero)
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_ASIGNACIONES SET ESTADO = @estado WHERE ID = @id",
                ("estado", "VENCIDA"), ("id", idAsignacion));

            _historial.ActualizarEstado(idAsignacion, "VENCIDA");
            _casilleros.MarcarEstado(idCasillero, "VENCIDO");
        }
    }
}
