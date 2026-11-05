using AccesoDatos.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;

namespace LogicaNegocios.Casilleros
{
    public class CasilleroManejador : ManejadorBase
    {
        private const int AnchoCodigo = 3;

        public CasilleroManejador(IConexionBD conexion, LogManejador log) : base(conexion, log)
        {
        }

        public DataSet ConsultarTodos()
        {
            return _conexion.Seleccionar("SELECT * FROM CASILLEROS ORDER BY CODIGO");
        }

        public DataSet ConsultarDisponibles()
        {
            return _conexion.Seleccionar(
                "SELECT * FROM CASILLEROS WHERE ESTADO = @estado AND ACTIVO = @activo ORDER BY CODIGO",
                ("estado", "DISPONIBLE"), ("activo", true));
        }

        public int ContarActivos()
        {
            var ds = _conexion.Seleccionar(
                "SELECT COUNT(*) AS CNT FROM CASILLEROS WHERE ACTIVO = @activo", ("activo", true));
            return Convert.ToInt32(ds.Tables[0].Rows[0]["CNT"]);
        }

        public DataSet ConsultarConAsignacion()
        {
            return _conexion.Seleccionar(
                @"SELECT c.ID, c.CODIGO, c.ESTADO, c.ACTIVO,
                         a.ID AS IDASIGNACION, a.NOMBRECLIENTE, a.FECHAINICIO, a.FECHAFIN,
                         a.MINUTOSSOLICITADOS, a.NOTIFICADO
                  FROM CASILLEROS c LEFT JOIN
                    (SELECT ID, IDCASILLERO, NOMBRECLIENTE, FECHAINICIO, FECHAFIN, MINUTOSSOLICITADOS, NOTIFICADO
                     FROM CASILLERO_ASIGNACIONES WHERE ESTADO IN ('ACTIVA','VENCIDA')) a
                    ON a.IDCASILLERO = c.ID
                  WHERE c.ACTIVO = @activo
                  ORDER BY c.CODIGO",
                ("activo", true));
        }

        private int Insertar(string codigo, string usuario, string ip)
        {
            _log.CrearLog("Se creó el casillero " + codigo, usuario, ip, "");
            return _conexion.Ejecutar(
                "INSERT INTO CASILLEROS (CODIGO, UBICACION, ESTADO, ACTIVO) VALUES (@codigo, @ubicacion, @estado, @activo)",
                ("codigo", codigo), ("ubicacion", ""), ("estado", "DISPONIBLE"), ("activo", true));
        }

        public int CambiarActivo(int id, bool activo, string usuario, string ip)
        {
            _log.CrearLog((activo ? "Se activó" : "Se desactivó") + " el casillero ID " + id, usuario, ip, "");
            return _conexion.Ejecutar(
                "UPDATE CASILLEROS SET ACTIVO = @activo WHERE ID = @id",
                ("activo", activo), ("id", id));
        }

        public List<string> FijarCantidad(int cantidad, string usuario, string ip)
        {
            if (cantidad < 0)
                throw new ArgumentException("La cantidad no puede ser negativa.");

            var pendientes = new List<string>();

            var ds = _conexion.Seleccionar(
                "SELECT ID, CODIGO, ESTADO FROM CASILLEROS WHERE ACTIVO = @activo ORDER BY CODIGO",
                ("activo", true));

            int existentes = ds.Tables[0].Rows.Count;

            if (cantidad > existentes)
            {
                for (int numero = existentes + 1; numero <= cantidad; numero++)
                    Insertar(GenerarCodigo(numero), usuario, ip);
            }
            else if (cantidad < existentes)
            {
                int aDesactivar = existentes - cantidad;

                for (int i = ds.Tables[0].Rows.Count - 1; i >= 0 && aDesactivar > 0; i--)
                {
                    var fila = ds.Tables[0].Rows[i];
                    if (fila["ESTADO"].ToString() != "DISPONIBLE")
                    {
                        pendientes.Add(fila["CODIGO"].ToString());
                        continue;
                    }

                    CambiarActivo(Convert.ToInt32(fila["ID"]), false, usuario, ip);
                    aDesactivar--;
                }
            }

            return pendientes;
        }

        private static string GenerarCodigo(int numero) => "C-" + numero.ToString().PadLeft(AnchoCodigo, '0');

        internal int MarcarEstado(int id, string estado)
        {
            return _conexion.Ejecutar(
                "UPDATE CASILLEROS SET ESTADO = @estado WHERE ID = @id",
                ("estado", estado), ("id", id));
        }

        internal bool IntentarOcupar(int id)
        {
            int filas = _conexion.Ejecutar(
                "UPDATE CASILLEROS SET ESTADO = @nuevo WHERE ID = @id AND ESTADO = @actual",
                ("nuevo", "OCUPADO"), ("id", id), ("actual", "DISPONIBLE"));
            return filas == 1;
        }

        internal int Liberar(int id)
        {
            return MarcarEstado(id, "DISPONIBLE");
        }
    }
}
