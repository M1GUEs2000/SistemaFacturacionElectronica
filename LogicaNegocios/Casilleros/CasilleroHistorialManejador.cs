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

        public const string FilaDetalle = "D";
        public const string FilaSubtotal = "S";
        public const string FilaTotal = "T";

        // Ordena por tiempo y fecha, e intercala una fila de subtotal después de cada grupo de
        // minutos y una fila de TOTAL al final. TIPOFILA indica si es detalle, subtotal o total.
        public static DataTable DetalleConSubtotales(DataTable detalle)
        {
            var salida = new DataTable();
            salida.Columns.Add("FECHA", typeof(DateTime));
            salida.Columns.Add("CASILLERO", typeof(string));
            salida.Columns.Add("TIEMPO", typeof(int));
            salida.Columns.Add("PERSONA", typeof(string));
            salida.Columns.Add("ESTADO", typeof(string));
            salida.Columns.Add("TIPOFILA", typeof(string));

            var vista = new DataView(detalle) { Sort = "TIEMPO ASC, FECHA ASC" };
            int minutosActual = -1, contadorGrupo = 0, total = 0;

            void CerrarGrupo()
            {
                if (contadorGrupo == 0) return;
                var fila = salida.NewRow();
                fila["CASILLERO"] = "Subtotal";
                fila["TIEMPO"] = minutosActual;
                fila["PERSONA"] = TextoCantidad(contadorGrupo);
                fila["ESTADO"] = "";
                fila["TIPOFILA"] = FilaSubtotal;
                salida.Rows.Add(fila);
                contadorGrupo = 0;
            }

            foreach (DataRowView drv in vista)
            {
                int min = Convert.ToInt32(drv["TIEMPO"]);
                if (min != minutosActual) { CerrarGrupo(); minutosActual = min; }

                var fila = salida.NewRow();
                fila["FECHA"] = drv["FECHA"];
                fila["CASILLERO"] = drv["CASILLERO"];
                fila["TIEMPO"] = min;
                fila["PERSONA"] = drv["PERSONA"];
                fila["ESTADO"] = drv["ESTADO"];
                fila["TIPOFILA"] = FilaDetalle;
                salida.Rows.Add(fila);
                contadorGrupo++;
                total++;
            }
            CerrarGrupo();

            if (total > 0)
            {
                var fila = salida.NewRow();
                fila["CASILLERO"] = "TOTAL";
                fila["PERSONA"] = TextoCantidad(total);
                fila["ESTADO"] = "";
                fila["TIPOFILA"] = FilaTotal;
                salida.Rows.Add(fila);
            }
            return salida;
        }

        public static string TextoCantidad(int n)
        {
            return n + (n == 1 ? " asignación" : " asignaciones");
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
