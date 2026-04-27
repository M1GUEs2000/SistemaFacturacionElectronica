using AccesoDatosWeb.Configuracion;
using Facturacion.api.App_Start;
using Facturacion.api.Configurations;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios
{
    public class ServicioCrearBase : IServicioCrearBase
    {
        private readonly AppServices _services;
        private readonly DatabaseSettings general_db;

        public ServicioCrearBase(AppServices services)
        {
            _services = services;
            general_db = ObtenerBaseGeneral.obtenerDatabaseSettingsGeneral();
        }

        public async Task<RespuestaGeneral<string>> EjecutarScriptAsync(SolicitudCrearBase solicitud)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(solicitud?.ServerName) ||
                    string.IsNullOrWhiteSpace(solicitud?.DatabaseName) ||
                    string.IsNullOrWhiteSpace(solicitud?.DbUser) ||
                    string.IsNullOrWhiteSpace(solicitud?.DbPassword))
                {
                    return RespuestaGeneral<string>.Fail(
                        "Debe especificar los datos de conexión (ServerName, DatabaseName, DbUser, DbPassword)."
                    );
                }

                var connBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = solicitud.ServerName,
                    InitialCatalog = solicitud.DatabaseName,
                    UserID = solicitud.DbUser,
                    Password = solicitud.DbPassword,
                    IntegratedSecurity = false,
                    MultipleActiveResultSets = true,
                    TrustServerCertificate = solicitud.TrustServerCertificate
                };

                using (var conexion = new SqlConnection(connBuilder.ConnectionString))
                {
                    await conexion.OpenAsync();

                    using (var transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            // Crear tablas
                            string script = ScriptCrearBase.Obtener();
                            foreach (var bloque in SepararScriptPorGo(script))
                            {
                                if (string.IsNullOrWhiteSpace(bloque)) continue;

                                using (var cmd = new SqlCommand(bloque, conexion, transaccion))
                                {
                                    cmd.CommandTimeout = 180;
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            // Seed PARAMETROS_SRI — 3 módulos (solo si la tabla quedó vacía)
                            const string sqlSeedSri = @"
IF NOT EXISTS (SELECT 1 FROM dbo.PARAMETROS_SRI)
BEGIN
    INSERT INTO dbo.PARAMETROS_SRI (IDFACTURA, TIPOCOMPROBANTE, SECUENCIAL, CODIGONUMERICO, FECHAACTUALIZACION)
    VALUES
        (1, '01', 1, @codFac, @fecha),
        (2, '04', 1, @codNC,  @fecha),
        (3, '07', 1, @codRet, @fecha)
END";

                            var rnd = new Random();
                            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                            using (var cmd = new SqlCommand(sqlSeedSri, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@codFac", GenerarCodigoNumerico(rnd));
                                cmd.Parameters.AddWithValue("@codNC",  GenerarCodigoNumerico(rnd));
                                cmd.Parameters.AddWithValue("@codRet", GenerarCodigoNumerico(rnd));
                                cmd.Parameters.AddWithValue("@fecha",  fecha);
                                cmd.CommandTimeout = 60;
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Seed PARAMETROS_FACTURAS (solo si la tabla quedó vacía)
                            const string sqlSeedParametros = @"
IF NOT EXISTS (SELECT 1 FROM dbo.PARAMETROS_FACTURAS)
    INSERT INTO dbo.PARAMETROS_FACTURAS
        (NOMBRE, AMBIENTE, TIPOEMISION, PRUEBAPRODUCCION, MONEDA, NUMERODIGITOS, FECHAACTUALIZACION)
    VALUES
        (@nombreEmpresa, '1', '1', 'PRUEBAS', 'DOLAR', 9, @fecha)";

                            using (var cmd = new SqlCommand(sqlSeedParametros, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@nombreEmpresa", solicitud.NombreEmpresa ?? "");
                                cmd.Parameters.AddWithValue("@fecha", fecha);
                                cmd.CommandTimeout = 60;
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaccion.Commit();

                            return RespuestaGeneral<string>.Ok(
                                "Base de datos inicializada correctamente."
                            );
                        }
                        catch (Exception ex)
                        {
                            transaccion.Rollback();
                            return RespuestaGeneral<string>.Fail(
                                "Error ejecutando script de creación de base: " + ex.Message
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail(
                    "Error preparando creación de base: " + ex.Message
                );
            }
        }

        public async Task<RespuestaGeneral<string>> InsertarEmpresaAsync(SolicitudEmpresaGeneral s)
        {
            try
            {
                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM Empresas
    WHERE UPPER(LTRIM(RTRIM(NombreEmpresa))) = UPPER(LTRIM(RTRIM(@nombre)))
)
BEGIN
    INSERT INTO Empresas
        (NombreEmpresa, CodigoEmpresa, Provider, ServerName, DatabaseName,
         DbUser, DbPassword, TrustServerCertificate, Activa)
    VALUES
        (@nombre, @codigo, @provider, @server, @database,
         @dbUser, @dbPassword, @trust, 1)
END
ELSE
BEGIN
    RAISERROR('La empresa ya existe.', 16, 1)
END";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@nombre",   s.NombreEmpresa);
                        cmd.Parameters.AddWithValue("@codigo",   s.CodigoEmpresa ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@provider", s.Provider      ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@server",   s.ServerName    ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@database", s.DatabaseName  ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@dbUser",   s.DbUser        ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@dbPassword", s.DbPassword  ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@trust",    s.TrustServerCertificate);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RespuestaGeneral<string>.Ok("Empresa creada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error insertando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ActualizarEmpresaAsync(SolicitudEmpresaGeneral s)
        {
            try
            {
                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sql = @"
UPDATE Empresas
SET
    CodigoEmpresa        = @codigo,
    Provider             = @provider,
    ServerName           = @server,
    DatabaseName         = @database,
    DbUser               = @dbUser,
    DbPassword           = @dbPassword,
    TrustServerCertificate = @trust,
    Activa               = @activa
WHERE UPPER(LTRIM(RTRIM(NombreEmpresa))) = UPPER(LTRIM(RTRIM(@nombre)));

IF @@ROWCOUNT = 0
    RAISERROR('No se encontró la empresa.', 16, 1)";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@nombre",   s.NombreEmpresa);
                        cmd.Parameters.AddWithValue("@codigo",   s.CodigoEmpresa ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@provider", s.Provider      ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@server",   s.ServerName    ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@database", s.DatabaseName  ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@dbUser",   s.DbUser        ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@dbPassword", s.DbPassword  ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@trust",    s.TrustServerCertificate);
                        cmd.Parameters.AddWithValue("@activa",   s.Activa);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RespuestaGeneral<string>.Ok("Empresa actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error actualizando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> DesactivarEmpresaAsync(string nombreEmpresa)
        {
            try
            {
                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sql = @"
UPDATE Empresas
SET Activa = 0
WHERE UPPER(LTRIM(RTRIM(NombreEmpresa))) = UPPER(LTRIM(RTRIM(@nombre)));

IF @@ROWCOUNT = 0
    RAISERROR('No se encontró la empresa.', 16, 1)";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombreEmpresa);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RespuestaGeneral<string>.Ok("Empresa desactivada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error desactivando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<List<DtoEmpresaGeneral>>> ListarEmpresasAsync()
        {
            try
            {
                var lista = new List<DtoEmpresaGeneral>();

                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sql = @"
SELECT NombreEmpresa, CodigoEmpresa, Provider, ServerName, DatabaseName,
       DbUser, DbPassword, TrustServerCertificate, Activa
FROM Empresas
ORDER BY NombreEmpresa";

                    using (var cmd = new SqlCommand(sql, con))
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            lista.Add(new DtoEmpresaGeneral
                            {
                                NombreEmpresa          = rd["NombreEmpresa"]?.ToString(),
                                CodigoEmpresa          = rd["CodigoEmpresa"]?.ToString(),
                                Provider               = rd["Provider"]?.ToString(),
                                ServerName             = rd["ServerName"]?.ToString(),
                                DatabaseName           = rd["DatabaseName"]?.ToString(),
                                DbUser                 = rd["DbUser"]?.ToString(),
                                DbPassword             = rd["DbPassword"]?.ToString(),
                                TrustServerCertificate = rd["TrustServerCertificate"] != DBNull.Value && (bool)rd["TrustServerCertificate"],
                                Activa                 = rd["Activa"] != DBNull.Value && (bool)rd["Activa"]
                            });
                        }
                    }
                }

                if (lista.Count == 0)
                    return RespuestaGeneral<List<DtoEmpresaGeneral>>.Fail("No existen empresas.");

                return RespuestaGeneral<List<DtoEmpresaGeneral>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoEmpresaGeneral>>.Fail("Error listando empresas: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<bool>> ExisteBaseAsync(string databaseName)
        {
            try
            {
                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sql = "SELECT COUNT(1) FROM sys.databases WHERE name = @nombre";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@nombre", databaseName);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return RespuestaGeneral<bool>.Ok(count > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<bool>.Fail("Error validando base: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> CrearCarpetasEmpresaAsync(string nombreEmpresa)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(nombreEmpresa))
                        return RespuestaGeneral<string>.Fail("Nombre de empresa requerido.");

                    var paths = FacturacionConfig.GetFacturacionPaths(nombreEmpresa);

                    Directory.CreateDirectory(Path.Combine(paths.General, "FIRMAELECTRONICA"));
                    Directory.CreateDirectory(Path.Combine(paths.General, "LOGOFACTURA"));
                    Directory.CreateDirectory(Path.Combine(paths.General, "ELECTRONICA"));

                    Directory.CreateDirectory(Path.Combine(paths.Facturas, "XML"));
                    Directory.CreateDirectory(Path.Combine(paths.Facturas, "XMLFIRMADOS"));
                    Directory.CreateDirectory(Path.Combine(paths.Facturas, "XMLAUTORIZADOS"));
                    Directory.CreateDirectory(Path.Combine(paths.Facturas, "PDF"));
                    Directory.CreateDirectory(Path.Combine(paths.Facturas, "PDFPREVIEW"));

                    Directory.CreateDirectory(Path.Combine(paths.NotasCredito, "XML"));
                    Directory.CreateDirectory(Path.Combine(paths.NotasCredito, "XMLFIRMADOS"));
                    Directory.CreateDirectory(Path.Combine(paths.NotasCredito, "XMLAUTORIZADOS"));
                    Directory.CreateDirectory(Path.Combine(paths.NotasCredito, "PDF"));
                    Directory.CreateDirectory(Path.Combine(paths.NotasCredito, "PDFPREVIEW"));

                    Directory.CreateDirectory(Path.Combine(paths.Retenciones, "XML"));
                    Directory.CreateDirectory(Path.Combine(paths.Retenciones, "XMLFIRMADOS"));
                    Directory.CreateDirectory(Path.Combine(paths.Retenciones, "XMLAUTORIZADOS"));
                    Directory.CreateDirectory(Path.Combine(paths.Retenciones, "PDF"));
                    Directory.CreateDirectory(Path.Combine(paths.Retenciones, "PDFPREVIEW"));

                    return RespuestaGeneral<string>.Ok("Carpetas creadas correctamente.");
                }
                catch (Exception ex)
                {
                    return RespuestaGeneral<string>.Fail("Error creando carpetas: " + ex.Message);
                }
            });
        }

        public async Task<RespuestaGeneral<string>> SubirArchivoEmpresaAsync(SolicitudSubirArchivo solicitud)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(solicitud?.Empresa))
                        return RespuestaGeneral<string>.Fail("Empresa requerida.");
                    if (string.IsNullOrWhiteSpace(solicitud?.NombreArchivo))
                        return RespuestaGeneral<string>.Fail("Nombre de archivo requerido.");
                    if (string.IsNullOrWhiteSpace(solicitud?.ContenidoBase64))
                        return RespuestaGeneral<string>.Fail("Contenido del archivo requerido.");

                    var paths = FacturacionConfig.GetFacturacionPaths(solicitud.Empresa);

                    string subcarpeta = string.Equals(solicitud.Tipo, "p12", StringComparison.OrdinalIgnoreCase)
                        ? "FIRMAELECTRONICA"
                        : "LOGOFACTURA";

                    string carpeta = Path.Combine(paths.General, subcarpeta);
                    Directory.CreateDirectory(carpeta);

                    string nombreLimpio = Path.GetFileName(solicitud.NombreArchivo);
                    string rutaCompleta = Path.Combine(carpeta, nombreLimpio);

                    byte[] bytes = Convert.FromBase64String(solicitud.ContenidoBase64);
                    File.WriteAllBytes(rutaCompleta, bytes);

                    return RespuestaGeneral<string>.Ok(nombreLimpio);
                }
                catch (Exception ex)
                {
                    return RespuestaGeneral<string>.Fail("Error subiendo archivo: " + ex.Message);
                }
            });
        }

        public async Task<RespuestaGeneral<string>> EliminarEmpresaCompletaAsync(string nombreEmpresa)
        {
            if (string.IsNullOrWhiteSpace(nombreEmpresa))
                return RespuestaGeneral<string>.Fail("Nombre de empresa requerido.");

            // 1 — Obtener credenciales del tenant desde BD General
            SqlConnectionStringBuilder tenantConn = null;
            try
            {
                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sqlGet = @"
SELECT TOP 1 ServerName, DatabaseName, DbUser, DbPassword, TrustServerCertificate
FROM Empresas
WHERE UPPER(LTRIM(RTRIM(NombreEmpresa))) = UPPER(LTRIM(RTRIM(@nombre)))";

                    using (var cmd = new SqlCommand(sqlGet, con))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombreEmpresa);
                        using (var rd = await cmd.ExecuteReaderAsync())
                        {
                            if (await rd.ReadAsync())
                            {
                                tenantConn = new SqlConnectionStringBuilder
                                {
                                    DataSource             = rd["ServerName"]?.ToString(),
                                    InitialCatalog         = rd["DatabaseName"]?.ToString(),
                                    UserID                 = rd["DbUser"]?.ToString(),
                                    Password               = rd["DbPassword"]?.ToString(),
                                    TrustServerCertificate = rd["TrustServerCertificate"] != DBNull.Value && (bool)rd["TrustServerCertificate"],
                                    Encrypt                = true
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error obteniendo datos del tenant: " + ex.Message);
            }

            // 2 — Limpiar todas las tablas del tenant
            if (tenantConn != null)
            {
                try
                {
                    using (var con = new SqlConnection(tenantConn.ConnectionString))
                    {
                        await con.OpenAsync();

                        const string sqlLimpiar = @"
DELETE FROM dbo.RETENCIONES_DETALLE
DELETE FROM dbo.RETENCIONES
DELETE FROM dbo.NOTASCREDITO_DETALLE
DELETE FROM dbo.NOTASCREDITO
DELETE FROM dbo.FACTURAS_PENDIENTES
DELETE FROM dbo.FACTURACION
DELETE FROM dbo.PARAMETROS_SRI
DELETE FROM dbo.PARAMETROS_FACTURAS
DELETE FROM dbo.FORMASDEPAGO
DELETE FROM dbo.PRODUCTOS
DELETE FROM dbo.PROVEEDORES
DELETE FROM dbo.LOG
DELETE FROM dbo.CLIENTE
DELETE FROM dbo.EMPRESA";

                        using (var cmd = new SqlCommand(sqlLimpiar, con))
                        {
                            cmd.CommandTimeout = 120;
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    return RespuestaGeneral<string>.Fail("Error limpiando tablas del tenant: " + ex.Message);
                }
            }

            // 3 — Eliminar carpetas del servidor
            try
            {
                var paths = FacturacionConfig.GetFacturacionPaths(nombreEmpresa);
                string empresaRoot = System.IO.Path.GetDirectoryName(paths.General);

                if (Directory.Exists(empresaRoot))
                    Directory.Delete(empresaRoot, recursive: true);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error eliminando carpetas: " + ex.Message);
            }

            // 4 — Eliminar de la BD General
            try
            {
                using (var con = new SqlConnection(BuildConnectionString(general_db)))
                {
                    await con.OpenAsync();

                    const string sql = @"
DELETE FROM Empresas
WHERE UPPER(LTRIM(RTRIM(NombreEmpresa))) = UPPER(LTRIM(RTRIM(@nombre)));

IF @@ROWCOUNT = 0
    RAISERROR('No se encontró la empresa.', 16, 1)";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombreEmpresa);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return RespuestaGeneral<string>.Ok("Empresa eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error eliminando empresa de la BD General: " + ex.Message);
            }
        }

        private string BuildConnectionString(DatabaseSettings db)
        {
            return new SqlConnectionStringBuilder
            {
                DataSource             = db.Server,
                InitialCatalog         = db.Database,
                UserID                 = db.User,
                Password               = db.Password,
                TrustServerCertificate = db.TrustServerCertificate,
                Encrypt                = true
            }.ConnectionString;
        }

        private string GenerarCodigoNumerico(Random rnd)
        {
            return rnd.Next(100_000_000, 999_999_999).ToString();
        }

        private string[] SepararScriptPorGo(string script)
        {
            return Regex.Split(
                script,
                @"^\s*GO\s*($|\-\-.*$)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );
        }
    }
}
