using AccesoDatosWeb.Configuracion;
using Facturacion.api.App_Start;
using Facturacion.api.Mappers;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Facturacion.api.Servicios
{
    public class ServicioEmpresasGeneral : IServicioEmpresasGeneral
    {
        private readonly DatabaseSettings general_db;

        public ServicioEmpresasGeneral()
        {
            general_db = ObtenerBaseGeneral.obtenerDatabaseSettingsGeneral();
        }

        public DtoEmpresasGeneral obtenerPorNombreOCodigo(string empresa)
        {
            if (string.IsNullOrWhiteSpace(empresa))
                return null;

            string connection_string = buildConnectionString(general_db);

            using (var sql_connection = new SqlConnection(connection_string))
            {
                sql_connection.Open();

                string sql = @"
                    SELECT TOP 1
                        NombreEmpresa,
                        CodigoEmpresa,
                        Provider,
                        ServerName,
                        DatabaseName,
                        DbUser,
                        DbPassword,
                        TrustServerCertificate,
                        Activa
                    FROM Empresas
                    WHERE Activa = 1
                      AND (
                            UPPER(LTRIM(RTRIM(CodigoEmpresa))) = UPPER(LTRIM(RTRIM(@Empresa)))
                         OR UPPER(LTRIM(RTRIM(NombreEmpresa))) = UPPER(LTRIM(RTRIM(@Empresa)))
                      )";

                using (var sql_command = new SqlCommand(sql, sql_connection))
                {
                    sql_command.Parameters.AddWithValue("@Empresa", empresa.Trim());

                    using (var sql_reader = sql_command.ExecuteReader())
                    {
                        if (!sql_reader.Read())
                            return null;

                        return new DtoEmpresasGeneral
                        {
                            CodigoEmpresa = Convert.ToString(sql_reader["CodigoEmpresa"]),
                            NombreEmpresa = Convert.ToString(sql_reader["NombreEmpresa"]),
                            Provider = Convert.ToString(sql_reader["Provider"]),
                            ServerName = Convert.ToString(sql_reader["ServerName"]),
                            DatabaseName = Convert.ToString(sql_reader["DatabaseName"]),
                            DbUser = Convert.ToString(sql_reader["DbUser"]),
                            DbPassword = Convert.ToString(sql_reader["DbPassword"]),
                            TrustServerCertificate = Convert.ToBoolean(sql_reader["TrustServerCertificate"]),
                            Activa = Convert.ToBoolean(sql_reader["Activa"])
                        };
                    }
                }
            }
        }

        public bool existeEmpresa(string empresa)
        {
            return obtenerPorNombreOCodigo(empresa) != null;
        }

        public List<string> listarNombresActivas()
        {
            var nombres = new List<string>();
            string connection_string = buildConnectionString(general_db);

            using (var sql_connection = new SqlConnection(connection_string))
            {
                sql_connection.Open();

                string sql = @"
                    SELECT NombreEmpresa
                    FROM Empresas
                    WHERE Activa = 1
                    ORDER BY NombreEmpresa";

                using (var sql_command = new SqlCommand(sql, sql_connection))
                using (var sql_reader = sql_command.ExecuteReader())
                {
                    while (sql_reader.Read())
                    {
                        string nombre = Convert.ToString(sql_reader["NombreEmpresa"]);
                        if (!string.IsNullOrWhiteSpace(nombre))
                            nombres.Add(nombre.Trim());
                    }
                }
            }

            return nombres;
        }

        public DatabaseSettings obtenerDatabaseSettings(string empresa)
        {
            var dto_empresas_general = obtenerPorNombreOCodigo(empresa);

            if (dto_empresas_general == null)
                return null;

            return MapperEmpresasGeneral.toDatabaseSettings(dto_empresas_general);
        }

        private string buildConnectionString(DatabaseSettings database_settings)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = database_settings.Server,
                InitialCatalog = database_settings.Database,
                UserID = database_settings.User,
                Password = database_settings.Password,
                TrustServerCertificate = database_settings.TrustServerCertificate,
                Encrypt = true
            };

            return builder.ConnectionString;
        }
    }
}