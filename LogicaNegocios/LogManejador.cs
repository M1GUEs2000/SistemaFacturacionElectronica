using AccesoDatos.Abstractions;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace LogicaNegocios
{
    public class LogManejador
    {
        private readonly IConexionBD _conexion;

        public LogManejador(IConexionBD conexion)
        {
            _conexion = conexion;
        }
        public void GrabarMensaje(string mensaje)
        {
            try
            {
                string fecha = DateTime.Now.ToString("yyyyMMdd");
                string fechahora = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                string fileName = AppDomain.CurrentDomain.BaseDirectory + @"LOG\log" + fecha + ".txt";
                //crea una instancia para escribir en archivo txt sin bloquear
                FileStream myStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 255, true);
                //make sure you close the file
                mensaje = "\r\n " + fechahora + ": " + mensaje;
                byte[] info = new UTF8Encoding(true).GetBytes(mensaje);
                myStream.Write(info, 0, info.Length);
                myStream.Flush();
                myStream.Close();
                myStream.Dispose();
            }
            catch (Exception e)
            {
                string error = e.Message.ToString();
            }
        }
        public int CrearLog(string Proceso, string Usuario, string IP, string Texto)
        {
            int filas = 0;
            string Fecha = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string HInicio = Convert.ToString(DateTime.Now.TimeOfDay).Substring(0, 8);
            string HFin = Convert.ToString(DateTime.Now.TimeOfDay).Substring(0, 8);
            string usuarioComillas = Usuario.Replace("'", "''");
            string sql = "INSERT INTO LOG(PROCESO,USUARIO, IP, TEXTO, FECHA) VALUES ('" + Proceso + "', '" + usuarioComillas + "', '" + IP + "','" + Texto.Replace("'", "") + "' , '" + Fecha + "')";
            try
            {
                filas = _conexion.Ejecutar(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR en CrearLog: " + ex.Message);
                Console.WriteLine(sql);
            }

            return filas;
        }
        //FUNCIONES PARA REALIZAR REPORTE
        public DataSet ConsultarLog(string Proceso, string Texto, string FechaDesde, string FechaHasta)
        {
            DataSet dsDatos = new DataSet();

            string sql = @"SELECT PROCESO, USUARIO, IP, TEXTO, FECHA
                   FROM LOG 
                   WHERE LEFT(FECHA,10) >= '" + FechaDesde + @"'
                     AND LEFT(FECHA,10) <= '" + FechaHasta + @"'";

            if (Proceso != "Seleccione")
            {
                sql += " AND PROCESO = '" + Proceso + "' ";
            }

            if (!string.IsNullOrWhiteSpace(Texto))
            {
                sql += " AND TEXTO LIKE '%" + Texto + "%' ";
            }

            // 🔥 ORDEN REAL POR FECHA Y HORA
            sql += " ORDER BY CDate(FECHA) DESC";

            dsDatos = _conexion.Seleccionar(sql);
            return dsDatos;
        }


        public DataSet MostrarProceso()
        {
            DataSet dsdatos = new DataSet();
            string sql = @"
                        SELECT
                            'Seleccione' AS PROCESO
                        FROM
                            (
                                SELECT
                                    TOP 1 * FROM LOG
                            )
                        UNION ALL
                        SELECT DISTINCT PROCESO FROM LOG
                        WHERE PROCESO IS NOT NULL
                            AND PROCESO <> ''
                        ";
            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }



    }
}
