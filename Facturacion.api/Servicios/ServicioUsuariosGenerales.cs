using Facturacion.api.App_Start;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios
{
    public class ServicioUsuariosGenerales : IServicioUsuariosGenerales
    {
        private readonly string _connectionString;

        public ServicioUsuariosGenerales()
        {
            var db = ObtenerBaseGeneral.obtenerDatabaseSettingsGeneral();
            _connectionString = BuildConnectionString(db);
        }

        // ======================================================
        // OBTENER TODOS
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoUsuarioGeneral>>> ObtenerTodosAsync()
        {
            try
            {
                var lista = new List<DtoUsuarioGeneral>();

                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT ID, USUARIO, ROL, ACTIVO, FECHACREACION
                        FROM USUARIOS
                        ORDER BY ID";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(MapRow(reader));
                        }
                    }
                }

                if (lista.Count == 0)
                    return RespuestaGeneral<List<DtoUsuarioGeneral>>.Fail("No existen usuarios.");

                return RespuestaGeneral<List<DtoUsuarioGeneral>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoUsuarioGeneral>>.Fail("Error obteniendo usuarios: " + ex.Message);
            }
        }

        // ======================================================
        // OBTENER POR ID
        // ======================================================

        public async Task<RespuestaGeneral<DtoUsuarioGeneral>> ObtenerPorIdAsync(int id)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT ID, USUARIO, ROL, ACTIVO, FECHACREACION
                        FROM USUARIOS
                        WHERE ID = @Id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return RespuestaGeneral<DtoUsuarioGeneral>.Fail("Usuario no encontrado.");

                            return RespuestaGeneral<DtoUsuarioGeneral>.Ok(MapRow(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoUsuarioGeneral>.Fail("Error consultando usuario: " + ex.Message);
            }
        }

        // ======================================================
        // VERIFICAR ADMIN (LOGIN)
        // ======================================================

        public async Task<RespuestaGeneral<DtoUsuarioGeneral>> VerificarAdminAsync(SolicitudLoginAdmin solicitud)
        {
            try
            {
                if (solicitud == null ||
                    string.IsNullOrWhiteSpace(solicitud.NombreUsuario) ||
                    string.IsNullOrWhiteSpace(solicitud.Contrasena))
                    return RespuestaGeneral<DtoUsuarioGeneral>.Fail("Usuario y contraseña son requeridos.");

                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT TOP 1 ID, USUARIO, CONTRASENA, ROL, ACTIVO, FECHACREACION
                        FROM USUARIOS
                        WHERE UPPER(LTRIM(RTRIM(USUARIO))) = UPPER(LTRIM(RTRIM(@Usuario)))
                          AND ACTIVO = 1";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", solicitud.NombreUsuario.Trim());

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return RespuestaGeneral<DtoUsuarioGeneral>.Fail("Usuario o contraseña incorrectos.");

                            string storedHash = reader["CONTRASENA"]?.ToString();
                            if (!Helpers.PasswordHelper.Verify(solicitud.Contrasena, storedHash))
                                return RespuestaGeneral<DtoUsuarioGeneral>.Fail("Usuario o contraseña incorrectos.");

                            return RespuestaGeneral<DtoUsuarioGeneral>.Ok(MapRow(reader), "Acceso correcto.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoUsuarioGeneral>.Fail("Error verificando acceso: " + ex.Message);
            }
        }

        // ======================================================
        // CREAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> CrearAsync(SolicitudUsuarioGeneral solicitud)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(solicitud?.NombreUsuario))
                    return RespuestaGeneral<string>.Fail("El nombre de usuario es requerido.");

                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        INSERT INTO USUARIOS (USUARIO, CONTRASENA, ROL, ACTIVO, FECHACREACION)
                        VALUES (@Usuario, @Contrasena, @Rol, @Activo, GETDATE())";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", solicitud.NombreUsuario.Trim());
                        cmd.Parameters.AddWithValue("@Contrasena", Helpers.PasswordHelper.Hash(solicitud.Contrasena ?? ""));
                        cmd.Parameters.AddWithValue("@Rol", solicitud.Rol ?? "usuario");
                        cmd.Parameters.AddWithValue("@Activo", solicitud.Activo ? 1 : 0);

                        int filas = await cmd.ExecuteNonQueryAsync();

                        if (filas <= 0)
                            return RespuestaGeneral<string>.Fail("No se pudo crear el usuario.");
                    }
                }

                return RespuestaGeneral<string>.Ok("Usuario creado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error creando usuario: " + ex.Message);
            }
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudUsuarioGeneral solicitud)
        {
            try
            {
                if (solicitud?.Id == null)
                    return RespuestaGeneral<string>.Fail("ID requerido para actualizar.");

                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    bool cambiaContrasena = !string.IsNullOrEmpty(solicitud.Contrasena);

                    string sql = cambiaContrasena
                        ? @"UPDATE USUARIOS
                            SET USUARIO    = @Usuario,
                                CONTRASENA = @Contrasena,
                                ROL        = @Rol,
                                ACTIVO     = @Activo
                            WHERE ID = @Id"
                        : @"UPDATE USUARIOS
                            SET USUARIO = @Usuario,
                                ROL     = @Rol,
                                ACTIVO  = @Activo
                            WHERE ID = @Id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", solicitud.Id.Value);
                        cmd.Parameters.AddWithValue("@Usuario", solicitud.NombreUsuario?.Trim() ?? "");
                        if (cambiaContrasena)
                            cmd.Parameters.AddWithValue("@Contrasena", Helpers.PasswordHelper.Hash(solicitud.Contrasena));
                        cmd.Parameters.AddWithValue("@Rol", solicitud.Rol ?? "usuario");
                        cmd.Parameters.AddWithValue("@Activo", solicitud.Activo ? 1 : 0);

                        int filas = await cmd.ExecuteNonQueryAsync();

                        if (filas <= 0)
                            return RespuestaGeneral<string>.Fail("No se pudo actualizar el usuario.");
                    }
                }

                return RespuestaGeneral<string>.Ok("Usuario actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error actualizando usuario: " + ex.Message);
            }
        }

        // ======================================================
        // ELIMINAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> EliminarAsync(int id)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = "DELETE FROM USUARIOS WHERE ID = @Id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int filas = await cmd.ExecuteNonQueryAsync();

                        if (filas <= 0)
                            return RespuestaGeneral<string>.Fail("No se pudo eliminar el usuario.");
                    }
                }

                return RespuestaGeneral<string>.Ok("Usuario eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error eliminando usuario: " + ex.Message);
            }
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private static DtoUsuarioGeneral MapRow(SqlDataReader reader)
        {
            return new DtoUsuarioGeneral
            {
                Id = Convert.ToInt32(reader["ID"]),
                NombreUsuario = Convert.ToString(reader["USUARIO"]),
                Rol = Convert.ToString(reader["ROL"]),
                Activo = Convert.ToBoolean(reader["ACTIVO"]),
                FechaCreacion = Convert.ToDateTime(reader["FECHACREACION"])
            };
        }

        private static string BuildConnectionString(AccesoDatosWeb.Configuracion.DatabaseSettings db)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = db.Server,
                InitialCatalog = db.Database,
                UserID = db.User,
                Password = db.Password,
                TrustServerCertificate = db.TrustServerCertificate,
                Encrypt = true
            };
            return builder.ConnectionString;
        }
    }
}
