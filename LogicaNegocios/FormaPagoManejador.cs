using AccesoDatos.Abstractions;
using System.Data;

namespace LogicaNegocios
{
    public class FormaPagoManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public FormaPagoManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        public DataSet ConsultarTotales(string Fecha)
        {
            DataSet dsdatos = new DataSet();
            string sql = "SELECT  FECHA,FORMADEPAGO,  SUM(CANTIDAD) AS CANTIDADES, SUM(TOTAL) AS TOTALES FROM FORMAS DE PAGO] WHERE FECHA='" + Fecha + "' GROUP BY FECHA,FORMADEPAGO ";
            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }

        public DataSet Mostrar()
        {
            DataSet dsdatos = new DataSet();
            string sql = "SELECT * FROM FORMASDEPAGO ORDER BY DESC";
            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }
        public DataSet MostrarFormaPago(string Formas)
        {
            DataSet dsdatos = new DataSet();
            string sql = "SELECT FORMAS,IMAGEN,CODIGO FROM  FORMASDEPAGO ";
            if (Formas != "")
            {
                sql += " WHERE FORMAS LIKE '" + Formas + "%'";
            }
            sql += " ORDER BY Formas";
            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }
        public DataSet ConsultaFormas(string Formas)
        {
            DataSet dsdatos = new DataSet();
            string sql = "SELECT * FROM  FORMASDEPAGO WHERE FORMAS='" + Formas + "'";
            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }
        public int Insertar(string Formas, string Imagen, string Codigo, string Usuario, string IP)
        {
            string sql = @"INSERT INTO FORMASDEPAGO(
                        FORMAS,
                        IMAGEN,
                        CODIGO
                   ) VALUES (
                        '" + Formas + @"',
                        '" + Imagen + @"',
                        '" + Codigo + @"'
                   )";

            _log.CrearLog(
                "Se insertó la forma de pago: " + Formas,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }


        public int Actualizar(string Formas, string Imagen, string Codigo, string Usuario, string IP)
        {
            string sql = @"UPDATE FORMASDEPAGO SET
                       IMAGEN = '" + Imagen + @"',
                       CODIGO = '" + Codigo + @"'
                   WHERE FORMAS = '" + Formas + @"'";

            _log.CrearLog(
                "Se actualizó la forma de pago: " + Formas,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }


        public int Eliminar(string Formas, string Usuario, string IP)
        {
            string sql = @"DELETE FROM FORMASDEPAGO
                   WHERE FORMAS = '" + Formas + @"'";

            // ✔ Log estándar
            _log.CrearLog(
                "Se eliminó la forma de pago: " + Formas,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

    }
}
