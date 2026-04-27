using AccesoDatos.Abstractions;
using AccesoDatosWeb.Configuracion;
using System.Data.SqlClient;
using System;
using System.Data;

namespace AccesoDatosWeb
{
    public class ConexionBD : IConexionBD
    {
        private readonly DatabaseSettings _db;

        public ConexionBD(DatabaseSettings dbSettings)
        {
            _db = dbSettings ?? throw new ArgumentNullException(nameof(dbSettings));
        }

        public bool EsSqlServer { get { return true; } }

        private string ObtenerCadenaConexion()
        {
            if (!string.Equals(_db.Provider, "SQL", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Proveedor de base de datos no soportado: '{_db.Provider}'.");
            }

            if (string.IsNullOrWhiteSpace(_db.Server) ||
                string.IsNullOrWhiteSpace(_db.Database) ||
                string.IsNullOrWhiteSpace(_db.User) ||
                string.IsNullOrWhiteSpace(_db.Password))
            {
                throw new Exception("Configuración de base de datos no configurada correctamente.");
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _db.Server,
                InitialCatalog = _db.Database,
                UserID = _db.User,
                Password = _db.Password,
                TrustServerCertificate = _db.TrustServerCertificate,
                Encrypt = true
            };

            return builder.ConnectionString;
        }

        public int Ejecutar(string sql)
        {
            int filas = 0;
            string cadena = ObtenerCadenaConexion();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    filas = cmd.ExecuteNonQuery();
                }
            }

            return filas;
        }

        public DataSet Seleccionar(string sql)
        {
            DataSet dsDatos = new DataSet();
            string cadena = ObtenerCadenaConexion();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                con.Open();

                using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                {
                    da.Fill(dsDatos);
                }
            }

            return dsDatos;
        }

        public DataSet Seleccionar(string sql, params (string nombre, object valor)[] parametros)
        {
            if (parametros == null || parametros.Length == 0)
                return Seleccionar(sql);

            DataSet dsDatos = new DataSet();
            string cadena = ObtenerCadenaConexion();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    foreach (var (nombre, valor) in parametros)
                        cmd.Parameters.AddWithValue("@" + nombre.TrimStart('@'), valor ?? DBNull.Value);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dsDatos);
                }
            }

            return dsDatos;
        }
    }
}