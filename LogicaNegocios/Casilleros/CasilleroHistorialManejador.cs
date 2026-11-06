using AccesoDatos.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;

namespace LogicaNegocios.Casilleros
{
    public class FilaConsolidado
    {
        public int Minutos { get; set; }
        public int Cantidad { get; set; }
        public int SubtotalMinutos { get { return Minutos * Cantidad; } }
    }

    public class CasilleroHistorialManejador : ManejadorBase
    {
        public CasilleroHistorialManejador(IConexionBD conexion, LogManejador log) : base(conexion, log)
        {
        }

        public void RegistrarAsignacion(
            int idAsignacion, int idCasillero, string codigo, string nombreCliente,
            int minutos, DateTime inicio, DateTime fin, string usuario)
        {
            _conexion.Ejecutar(
                @"INSERT INTO CASILLERO_HISTORIAL
                    (IDASIGNACION, IDCASILLERO, CODIGOCASILLERO, NOMBRECLIENTE, MINUTOS, FECHAINICIO, FECHAFIN, ESTADO, USUARIO)
                  VALUES
                    (@idAsignacion, @idCasillero, @codigo, @nombre, @minutos, @inicio, @fin, @estado, @usuario)",
                ("idAsignacion", idAsignacion), ("idCasillero", idCasillero), ("codigo", codigo),
                ("nombre", nombreCliente), ("minutos", minutos), ("inicio", inicio), ("fin", fin),
                ("estado", "ACTIVA"), ("usuario", (object)usuario ?? ""));
        }

        public void ActualizarEstado(int idAsignacion, string estado)
        {
            _conexion.Ejecutar(
                "UPDATE CASILLERO_HISTORIAL SET ESTADO = @estado WHERE IDASIGNACION = @id",
                ("estado", estado), ("id", idAsignacion));
        }

        // Nombres distintos de clientes que alguna vez usaron un casillero.
        public DataSet ListarClientes()
        {
            return _conexion.Seleccionar(
                "SELECT DISTINCT NOMBRECLIENTE FROM CASILLERO_HISTORIAL ORDER BY NOMBRECLIENTE");
        }

        // Minutos distintos usados alguna vez (para el filtro de tiempo).
        public DataSet ListarMinutos()
        {
            return _conexion.Seleccionar(
                "SELECT DISTINCT MINUTOS FROM CASILLERO_HISTORIAL ORDER BY MINUTOS");
        }

        // desde/hasta son fechas (se incluye todo el día "hasta"). cliente y minutos son opcionales.
        public DataTable Consultar(DateTime desde, DateTime hasta, string cliente, int? minutos)
        {
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha 'Hasta' no puede ser menor que 'Desde'.");

            var sql = @"SELECT FECHAINICIO AS FECHA, CODIGOCASILLERO AS CASILLERO, MINUTOS AS TIEMPO,
                               NOMBRECLIENTE AS PERSONA, ESTADO
                        FROM CASILLERO_HISTORIAL
                        WHERE FECHAINICIO >= @desde AND FECHAINICIO < @hasta";
            var parametros = new List<(string, object)>
            {
                ("desde", desde.Date),
                ("hasta", hasta.Date.AddDays(1))
            };

            if (!string.IsNullOrWhiteSpace(cliente))
            {
                sql += " AND NOMBRECLIENTE = @cliente";
                parametros.Add(("cliente", cliente.Trim()));
            }

            if (minutos.HasValue)
            {
                sql += " AND MINUTOS = @minutos";
                parametros.Add(("minutos", minutos.Value));
            }

            sql += " ORDER BY FECHAINICIO, CODIGOCASILLERO";

            var ds = _conexion.Seleccionar(sql, parametros.ToArray());
            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }

        // "30 minutos", "1 hora", "2 horas", "1 h 30 min".
        public static string TextoTiempo(int minutos)
        {
            if (minutos < 60) return minutos + (minutos == 1 ? " minuto" : " minutos");
            int h = minutos / 60, m = minutos % 60;
            if (m == 0) return h + (h == 1 ? " hora" : " horas");
            return h + " h " + m + " min";
        }

        public static string TextoMinutos(int minutos)
        {
            return minutos + (minutos == 1 ? " minuto" : " minutos");
        }

        public static string TextoUsos(int usos)
        {
            return usos + (usos == 1 ? " uso de casillero" : " usos de casillero");
        }

        // Cuenta cuántas asignaciones hubo por cada valor de minutos.
        public static List<FilaConsolidado> Consolidar(DataTable detalle)
        {
            var conteo = new SortedDictionary<int, int>();
            foreach (DataRow fila in detalle.Rows)
            {
                int min = Convert.ToInt32(fila["TIEMPO"]);
                conteo[min] = conteo.TryGetValue(min, out int actual) ? actual + 1 : 1;
            }

            var resultado = new List<FilaConsolidado>();
            foreach (var par in conteo)
                resultado.Add(new FilaConsolidado { Minutos = par.Key, Cantidad = par.Value });
            return resultado;
        }
    }
}
