using AccesoDatos.Abstractions;
using System.Data.SqlClient;
using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace AccesoDatos
{
    public class ConexionBD : IConexionBD
    {
        private string sqlcadena = AppDomain.CurrentDomain.BaseDirectory + "ConexionSQL.dat";

        private string ObtenerProveedor()
        {
            string provider = Environment.GetEnvironmentVariable("DB_PROVIDER");
            return string.IsNullOrEmpty(provider) ? "ACCESS" : provider.ToUpper();
        }

        public bool EsSqlServer { get { return ObtenerProveedor() == "SQL"; } }

        private string ObtenerCadenaConexion()
        {
            string provider = ObtenerProveedor();

            if (provider == "SQL")
            {
                string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
                string database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "";
                string user = Environment.GetEnvironmentVariable("DB_USER") ?? "";
                string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
                string trust = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE") ?? "True";

                return $"Server={server};Database={database};User Id={user};Password={password};Encrypt=True;TrustServerCertificate={trust};";
            }
            else
            {
                return LeerArchivo(sqlcadena);
            }
        }
        private string LeerArchivo(string archivo)
        {
            using (StreamReader objReader = new StreamReader(archivo))
            {
                return objReader.ReadLine();
            }
        }

        // Enlaza parámetros para OleDb (Access). Dos tipos NO deben ir por AddWithValue,
        // porque el inferido choca con la columna y Jet/ACE tira "Data type mismatch in
        // criteria expression" aunque la columna sea correcta:
        //   - DateTime  -> AddWithValue lo infiere DBTimeStamp; columna Fecha/Hora lo rechaza.
        //                  OleDbType.Date (fecha OLE, conserva la hora) sí lo acepta.
        //   - decimal   -> AddWithValue lo infiere Decimal/Numeric; columna Currency lo
        //                  rechaza. OleDbType.Currency mapea limpio a Access Moneda.
        // El resto (strings, bool->YesNo, int) va como antes.
        private static void AgregarParametrosOleDb(OleDbCommand cmd, (string nombre, object valor)[] parametros)
        {
            foreach (var (nombre, valor) in parametros)
            {
                if (valor is DateTime fecha)
                    cmd.Parameters.Add(nombre, OleDbType.Date).Value = fecha;
                else if (valor is decimal monto)
                    cmd.Parameters.Add(nombre, OleDbType.Currency).Value = monto;
                else
                    cmd.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
            }
        }

        public int Ejecutar(string SQL)
        {
            int filas = 0;
            string cadena = ObtenerCadenaConexion();
            string provider = ObtenerProveedor();

            if (provider == "SQL")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(SQL, con))
                    {
                        filas = cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (OleDbConnection con = new OleDbConnection(cadena))
                {
                    con.Open();
                    using (OleDbCommand cmd = new OleDbCommand(SQL, con))
                    {
                        filas = cmd.ExecuteNonQuery();
                    }
                }
            }

            return filas;
        }

        public DataSet Seleccionar(string SQL)
        {
            DataSet dsDatos = new DataSet();
            string cadena = ObtenerCadenaConexion();
            string provider = ObtenerProveedor();

            if (provider == "SQL")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(SQL, con))
                    {
                        da.Fill(dsDatos);
                    }
                }
            }
            else
            {
                using (OleDbConnection con = new OleDbConnection(cadena))
                {
                    con.Open();
                    using (OleDbDataAdapter da = new OleDbDataAdapter(SQL, con))
                    {
                        da.Fill(dsDatos);
                    }
                }
            }

            return dsDatos;
        }

        public int Ejecutar(string sql, params (string nombre, object valor)[] parametros)
        {
            if (parametros == null || parametros.Length == 0)
                return Ejecutar(sql);

            int filas = 0;
            string cadena = ObtenerCadenaConexion();
            string provider = ObtenerProveedor();

            if (provider == "SQL")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        foreach (var (nombre, valor) in parametros)
                            cmd.Parameters.AddWithValue("@" + nombre.TrimStart('@'), valor ?? DBNull.Value);
                        filas = cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                string oleDbSql = System.Text.RegularExpressions.Regex.Replace(sql, @"@\w+", "?");
                using (OleDbConnection con = new OleDbConnection(cadena))
                {
                    con.Open();
                    using (OleDbCommand cmd = new OleDbCommand(oleDbSql, con))
                    {
                        AgregarParametrosOleDb(cmd, parametros);
                        filas = cmd.ExecuteNonQuery();
                    }
                }
            }

            return filas;
        }

        public DataSet Seleccionar(string SQL, params (string nombre, object valor)[] parametros)
        {
            if (parametros == null || parametros.Length == 0)
                return Seleccionar(SQL);

            DataSet dsDatos = new DataSet();
            string cadena = ObtenerCadenaConexion();
            string provider = ObtenerProveedor();

            if (provider == "SQL")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(SQL, con))
                    {
                        foreach (var (nombre, valor) in parametros)
                            cmd.Parameters.AddWithValue("@" + nombre.TrimStart('@'), valor ?? DBNull.Value);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dsDatos);
                    }
                }
            }
            else
            {
                // OleDb usa parámetros posicionales (?), reemplazamos @nombre en orden
                string oleDbSql = System.Text.RegularExpressions.Regex.Replace(SQL, @"@\w+", "?");
                using (OleDbConnection con = new OleDbConnection(cadena))
                {
                    con.Open();
                    using (OleDbCommand cmd = new OleDbCommand(oleDbSql, con))
                    {
                        AgregarParametrosOleDb(cmd, parametros);
                        using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                            da.Fill(dsDatos);
                    }
                }
            }

            return dsDatos;
        }
    }
}