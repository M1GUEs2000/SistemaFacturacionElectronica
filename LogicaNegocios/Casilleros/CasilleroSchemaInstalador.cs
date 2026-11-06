using AccesoDatos.Abstractions;
using System;

namespace LogicaNegocios.Casilleros
{
    public static class CasilleroSchemaInstalador
    {
        public static void AsegurarTablas(IConexionBD conexion, LogManejador log)
        {
            CrearSiNoExiste(conexion, log, "CASILLEROS", Ddl.Casilleros(conexion.EsSqlServer));
            CrearSiNoExiste(conexion, log, "CASILLERO_ASIGNACIONES", Ddl.Asignaciones(conexion.EsSqlServer));
            CrearSiNoExiste(conexion, log, "CASILLERO_PARAMETROS", Ddl.Parametros(conexion.EsSqlServer));
            CrearSiNoExiste(conexion, log, "CASILLERO_NOTIFICACIONES", Ddl.Notificaciones(conexion.EsSqlServer));

            bool historialNuevo = !ExisteTabla(conexion, "CASILLERO_HISTORIAL");
            CrearSiNoExiste(conexion, log, "CASILLERO_HISTORIAL", Ddl.Historial(conexion.EsSqlServer));
            if (historialNuevo)
                RespaldarHistorialExistente(conexion, log);

            AsegurarFilaParametros(conexion);
            AsegurarColumnasNotificaciones(conexion, log);
            AsegurarColumnaSonido(conexion, log);
        }

        private static bool ExisteTabla(IConexionBD conexion, string nombreTabla)
        {
            try
            {
                conexion.Seleccionar("SELECT TOP 1 * FROM " + nombreTabla);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ExisteColumna(IConexionBD conexion, string tabla, string columna)
        {
            try
            {
                conexion.Seleccionar("SELECT TOP 1 " + columna + " FROM " + tabla);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Copia al histórico las asignaciones que ya existían antes de crear la tabla.
        private static void RespaldarHistorialExistente(IConexionBD conexion, LogManejador log)
        {
            try
            {
                conexion.Ejecutar(
                    @"INSERT INTO CASILLERO_HISTORIAL
                        (IDASIGNACION, IDCASILLERO, CODIGOCASILLERO, NOMBRECLIENTE, MINUTOS, FECHAINICIO, FECHAFIN, ESTADO, USUARIO)
                      SELECT a.ID, a.IDCASILLERO, c.CODIGO, a.NOMBRECLIENTE, a.MINUTOSSOLICITADOS,
                             a.FECHAINICIO, a.FECHAFIN, a.ESTADO, a.USUARIO
                      FROM CASILLERO_ASIGNACIONES a INNER JOIN CASILLEROS c ON a.IDCASILLERO = c.ID");
                log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "Histórico inicial copiado desde CASILLERO_ASIGNACIONES");
            }
            catch (Exception ex)
            {
                log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "ERROR copiando histórico inicial: " + ex);
                throw;
            }
        }

        private static void AsegurarColumnasNotificaciones(IConexionBD conexion, LogManejador log)
        {
            if (!ExisteColumna(conexion, "CASILLERO_NOTIFICACIONES", "TIPO"))
            {
                try
                {
                    conexion.Ejecutar(conexion.EsSqlServer
                        ? "ALTER TABLE CASILLERO_NOTIFICACIONES ADD TIPO VARCHAR(20) NULL"
                        : "ALTER TABLE CASILLERO_NOTIFICACIONES ADD COLUMN TIPO TEXT(20)");
                    conexion.Ejecutar("UPDATE CASILLERO_NOTIFICACIONES SET TIPO = @tipo WHERE TIPO IS NULL", ("tipo", "POR_VENCER"));
                    log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "Columna TIPO agregada a CASILLERO_NOTIFICACIONES");
                }
                catch (Exception ex)
                {
                    log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "ERROR agregando columna TIPO: " + ex);
                }
            }

            if (!ExisteColumna(conexion, "CASILLERO_NOTIFICACIONES", "MOSTRADA"))
            {
                try
                {
                    conexion.Ejecutar(conexion.EsSqlServer
                        ? "ALTER TABLE CASILLERO_NOTIFICACIONES ADD MOSTRADA BIT NULL"
                        : "ALTER TABLE CASILLERO_NOTIFICACIONES ADD COLUMN MOSTRADA YESNO");
                    conexion.Ejecutar("UPDATE CASILLERO_NOTIFICACIONES SET MOSTRADA = @mostrada WHERE MOSTRADA IS NULL", ("mostrada", true));
                    log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "Columna MOSTRADA agregada a CASILLERO_NOTIFICACIONES");
                }
                catch (Exception ex)
                {
                    log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "ERROR agregando columna MOSTRADA: " + ex);
                }
            }
        }

        private static void AsegurarColumnaSonido(IConexionBD conexion, LogManejador log)
        {
            if (ExisteColumna(conexion, "CASILLERO_PARAMETROS", "SONIDONOTIFICACION"))
                return;

            try
            {
                conexion.Ejecutar(conexion.EsSqlServer
                    ? "ALTER TABLE CASILLERO_PARAMETROS ADD SONIDONOTIFICACION VARCHAR(100) NULL"
                    : "ALTER TABLE CASILLERO_PARAMETROS ADD COLUMN SONIDONOTIFICACION TEXT(100)");
                conexion.Ejecutar(
                    "UPDATE CASILLERO_PARAMETROS SET SONIDONOTIFICACION = @sonido WHERE SONIDONOTIFICACION IS NULL",
                    ("sonido", "campana.wav"));
                log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "Columna SONIDONOTIFICACION agregada a CASILLERO_PARAMETROS");
            }
            catch (Exception ex)
            {
                log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "ERROR agregando columna SONIDONOTIFICACION: " + ex);
            }
        }

        private static void CrearSiNoExiste(IConexionBD conexion, LogManejador log, string tabla, string ddl)
        {
            if (ExisteTabla(conexion, tabla))
                return;

            try
            {
                conexion.Ejecutar(ddl);
                log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "Tabla creada: " + tabla);
            }
            catch (Exception ex)
            {
                log.CrearLog("CASILLEROS-INSTALADOR", "SISTEMA", "", "ERROR creando tabla " + tabla + ": " + ex);
                throw;
            }
        }

        private static void AsegurarFilaParametros(IConexionBD conexion)
        {
            var ds = conexion.Seleccionar("SELECT TOP 1 ID FROM CASILLERO_PARAMETROS");
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                conexion.Ejecutar(
                    "INSERT INTO CASILLERO_PARAMETROS (MINUTOSNOTIFICACIONPREVIA) VALUES (@min)",
                    ("min", 10));
            }
        }

        private static class Ddl
        {
            public static string Casilleros(bool sql) => sql
                ? @"CREATE TABLE CASILLEROS (
                        ID INT IDENTITY(1,1) PRIMARY KEY,
                        CODIGO VARCHAR(20) NOT NULL,
                        UBICACION VARCHAR(100) NULL,
                        ESTADO VARCHAR(20) NOT NULL,
                        ACTIVO BIT NOT NULL
                    )"
                : @"CREATE TABLE CASILLEROS (
                        ID AUTOINCREMENT PRIMARY KEY,
                        CODIGO TEXT(20) NOT NULL,
                        UBICACION TEXT(100),
                        ESTADO TEXT(20) NOT NULL,
                        ACTIVO YESNO NOT NULL
                    )";

            public static string Asignaciones(bool sql) => sql
                ? @"CREATE TABLE CASILLERO_ASIGNACIONES (
                        ID INT IDENTITY(1,1) PRIMARY KEY,
                        IDCASILLERO INT NOT NULL,
                        NOMBRECLIENTE VARCHAR(150) NOT NULL,
                        MINUTOSSOLICITADOS INT NOT NULL,
                        FECHAINICIO DATETIME NOT NULL,
                        FECHAFIN DATETIME NOT NULL,
                        ESTADO VARCHAR(20) NOT NULL,
                        NOTIFICADO BIT NOT NULL,
                        USUARIO VARCHAR(100) NULL,
                        FECHACREACION DATETIME NOT NULL
                    )"
                : @"CREATE TABLE CASILLERO_ASIGNACIONES (
                        ID AUTOINCREMENT PRIMARY KEY,
                        IDCASILLERO LONG NOT NULL,
                        NOMBRECLIENTE TEXT(150) NOT NULL,
                        MINUTOSSOLICITADOS LONG NOT NULL,
                        FECHAINICIO DATETIME NOT NULL,
                        FECHAFIN DATETIME NOT NULL,
                        ESTADO TEXT(20) NOT NULL,
                        NOTIFICADO YESNO NOT NULL,
                        USUARIO TEXT(100),
                        FECHACREACION DATETIME NOT NULL
                    )";

            public static string Parametros(bool sql) => sql
                ? @"CREATE TABLE CASILLERO_PARAMETROS (
                        ID INT IDENTITY(1,1) PRIMARY KEY,
                        MINUTOSNOTIFICACIONPREVIA INT NOT NULL
                    )"
                : @"CREATE TABLE CASILLERO_PARAMETROS (
                        ID AUTOINCREMENT PRIMARY KEY,
                        MINUTOSNOTIFICACIONPREVIA LONG NOT NULL
                    )";

            public static string Historial(bool sql) => sql
                ? @"CREATE TABLE CASILLERO_HISTORIAL (
                        ID INT IDENTITY(1,1) PRIMARY KEY,
                        IDASIGNACION INT NOT NULL,
                        IDCASILLERO INT NOT NULL,
                        CODIGOCASILLERO VARCHAR(20) NOT NULL,
                        NOMBRECLIENTE VARCHAR(150) NOT NULL,
                        MINUTOS INT NOT NULL,
                        FECHAINICIO DATETIME NOT NULL,
                        FECHAFIN DATETIME NOT NULL,
                        ESTADO VARCHAR(20) NOT NULL,
                        USUARIO VARCHAR(100) NULL
                    )"
                : @"CREATE TABLE CASILLERO_HISTORIAL (
                        ID AUTOINCREMENT PRIMARY KEY,
                        IDASIGNACION LONG NOT NULL,
                        IDCASILLERO LONG NOT NULL,
                        CODIGOCASILLERO TEXT(20) NOT NULL,
                        NOMBRECLIENTE TEXT(150) NOT NULL,
                        MINUTOS LONG NOT NULL,
                        FECHAINICIO DATETIME NOT NULL,
                        FECHAFIN DATETIME NOT NULL,
                        ESTADO TEXT(20) NOT NULL,
                        USUARIO TEXT(100)
                    )";

            public static string Notificaciones(bool sql) => sql
                ? @"CREATE TABLE CASILLERO_NOTIFICACIONES (
                        ID INT IDENTITY(1,1) PRIMARY KEY,
                        IDASIGNACION INT NOT NULL,
                        MENSAJE VARCHAR(255) NOT NULL,
                        TIPO VARCHAR(20) NOT NULL,
                        FECHACREACION DATETIME NOT NULL,
                        MOSTRADA BIT NOT NULL,
                        LEIDA BIT NOT NULL
                    )"
                : @"CREATE TABLE CASILLERO_NOTIFICACIONES (
                        ID AUTOINCREMENT PRIMARY KEY,
                        IDASIGNACION LONG NOT NULL,
                        MENSAJE TEXT(255) NOT NULL,
                        TIPO TEXT(20) NOT NULL,
                        FECHACREACION DATETIME NOT NULL,
                        MOSTRADA YESNO NOT NULL,
                        LEIDA YESNO NOT NULL
                    )";
        }
    }
}
