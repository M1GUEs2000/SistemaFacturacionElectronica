using AccesoDatos.Abstractions;
using System.Data;

namespace LogicaNegocios
{
    public class ClienteManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;


        public ClienteManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // Mostrar todos
        public DataSet Mostrar()
        {
            string sql = @"SELECT 
                            CEDULA,
                            NOMBRE,
                            CORREO,
                            DIRECCION,
                            TELEFONO
                        FROM CLIENTE
                        ORDER BY NOMBRE";

            return _conexion.Seleccionar(sql);
        }

        // Buscar por CÉDULA
        public DataSet ConsultarCedula(string Cedula)
        {
            string sql = @"SELECT
                            CEDULA,
                            NOMBRE,
                            CORREO,
                            DIRECCION,
                            TELEFONO
                        FROM CLIENTE
                        WHERE CEDULA LIKE '%" + Cedula + "%' ORDER BY NOMBRE";

            return _conexion.Seleccionar(sql);
        }

        // Buscar por NOMBRE
        public DataSet ConsultarNombre(string Nombre)
        {
            string sql = @"SELECT
                            CEDULA,
                            NOMBRE,
                            CORREO,
                            DIRECCION,
                            TELEFONO
                        FROM CLIENTE
                        WHERE NOMBRE LIKE '%" + Nombre + "%' ORDER BY NOMBRE";

            return _conexion.Seleccionar(sql);
        }

        // Insertar
        public int Insertar(
            string Cedula,
            string Nombre,
            string Correo,
            string Direccion,
            string Telefono,
            string Usuario,
            string IP)
        {
            string sql = @"INSERT INTO CLIENTE(
                            CEDULA,
                            NOMBRE,
                            CORREO,
                            DIRECCION,
                            TELEFONO
                        ) VALUES (
                            '" + Cedula + @"',
                            '" + Nombre + @"',
                            '" + Correo + @"',
                            '" + Direccion + @"',
                            '" + Telefono + @"'
                        )";

            _log.CrearLog(
                "Se insertó el cliente: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        public int Actualizar(
            string Cedula,
            string Nombre,
            string Correo,
            string Direccion,
            string Telefono,
            string Usuario,
            string IP)
        {
            string sql = @"UPDATE CLIENTE SET
                            NOMBRE = '" + Nombre + @"',
                            CORREO = '" + Correo + @"',
                            DIRECCION = '" + Direccion + @"',
                            TELEFONO = '" + Telefono + @"'
                        WHERE CEDULA = '" + Cedula + @"'";

            _log.CrearLog(
                "Se actualizó el cliente: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        public int Eliminar(string Cedula, string Nombre, string Usuario, string IP)
        {
            string sql = @"DELETE FROM CLIENTE 
                           WHERE CEDULA = '" + Cedula + @"'";

            _log.CrearLog(
                "Se eliminó el cliente: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }
    }
}