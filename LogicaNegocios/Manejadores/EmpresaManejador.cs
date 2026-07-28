using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class EmpresaManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public EmpresaManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // ======================================================
        // MOSTRAR EMPRESA
        // ======================================================
        public DataSet MostrarEmpresa(string Nombre)
        {
            string sql = @"
        SELECT
            NOMBRE,
            DIRECCION,
            USUARIO,
            CLAVEINGRESO,
            CLAVETOTALES,
            CLAVEELIMINACION,
            CLAVECONSULTA,
            CLAVETABLAS,
            FACTURACION,
            IMPRESION,
            TELEFONO,
            PROPIETARIO,
            EMAIL,
            UBICACIONARCHIVOP12,
            CONTRASENA,
            IMAGEN,
            ESTADORUC,
            RUC
        FROM EMPRESA
        WHERE ESTADORUC = 'ACTIVO'";

            if (!string.IsNullOrEmpty(Nombre))
            {
                Nombre = Nombre.Replace("'", "''");
                sql += " AND NOMBRE LIKE '" + Nombre + "%'";
            }

            sql += " ORDER BY NOMBRE";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // CONSULTA POR NOMBRE
        // ======================================================
        public DataSet ConsultaNombre(string Nombre)
        {
            if (!string.IsNullOrEmpty(Nombre))
                Nombre = Nombre.Replace("'", "''");


            string sql = @"
        SELECT
            NOMBRE,
            DIRECCION,
            USUARIO,
            CLAVEINGRESO,
            CLAVETOTALES,
            CLAVEELIMINACION,
            CLAVECONSULTA,
            CLAVETABLAS,
            FACTURACION,
            IMPRESION,
            TELEFONO,
            PROPIETARIO,
            EMAIL,
            UBICACIONARCHIVOP12,
            CONTRASENA,
            IMAGEN,
            ESTADORUC,
            RUC
        FROM EMPRESA
        WHERE ESTADORUC = 'ACTIVO'
        AND NOMBRE = '" + Nombre + "'";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // INSERTAR
        // ======================================================
        public int Insertar(
            string Nombre,
            string Direccion,
            string UsuarioLogin,
            string ClaveIngreso,
            string ClaveTotales,
            string ClaveEliminar,
            string ClaveConsulta,
            string ClaveTabla,
            string Facturacion,
            string Impresion,
            string Telefono,
            string Propietario,
            string Email,
            string UbicacionArchivoP12,
            string Contrasena,
            string Imagen,
            string EstadoRuc,
            string Ruc,
            string Usuario,
            string IP
        )
        {
            if (!string.IsNullOrEmpty(Nombre))
                Nombre = Nombre.Replace("'", "''");
            if (!string.IsNullOrEmpty(UsuarioLogin))
                UsuarioLogin = UsuarioLogin.Replace("'", "''");

            string sql = @"
INSERT INTO EMPRESA(
    NOMBRE,
    DIRECCION,
    USUARIO,
    CLAVEINGRESO,
    CLAVETOTALES,
    CLAVEELIMINACION,
    CLAVECONSULTA,
    CLAVETABLAS,
    FACTURACION,
    IMPRESION,
    TELEFONO,
    PROPIETARIO,
    EMAIL,
    UBICACIONARCHIVOP12,
    CONTRASENA,
    IMAGEN,
    ESTADORUC,
    RUC
) VALUES (
    '" + Nombre + @"',
    '" + Direccion + @"',
    '" + UsuarioLogin + @"',
    '" + ClaveIngreso + @"',
    '" + ClaveTotales + @"',
    '" + ClaveEliminar + @"',
    '" + ClaveConsulta + @"',
    '" + ClaveTabla + @"',
    '" + Facturacion + @"',
    '" + Impresion + @"',
    '" + Telefono + @"',
    '" + Propietario + @"',
    '" + Email + @"',
    '" + UbicacionArchivoP12 + @"',
    '" + Contrasena + @"',
    '" + Imagen + @"',
    '" + EstadoRuc + @"',
    '" + Ruc + @"'
)";

            _log.CrearLog(
            "Se insertó la empresa: " + Nombre,
            Usuario,
            IP,
            sql
        );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================
        public int Actualizar(
            string Nombre,
            string Direccion,
            string UsuarioLogin,
            string ClaveIngreso,
            string ClaveTotales,
            string ClaveEliminar,
            string ClaveConsulta,
            string ClaveTabla,
            string Facturacion,
            string Impresion,
            string Telefono,
            string Propietario,
            string Email,
            string Ruc,
            string UbicacionArchivoP12,
            string Contrasena,
            string Imagen,
            string EstadoRuc,
            string Usuario,
            string IP
        )
        {
            if (!string.IsNullOrEmpty(Nombre))
                Nombre = Nombre.Replace("'", "''");
            if (!string.IsNullOrEmpty(UsuarioLogin))
                UsuarioLogin = UsuarioLogin.Replace("'", "''");

            string sql = @"
UPDATE EMPRESA SET
    DIRECCION = '" + Direccion + @"',
    USUARIO = '" + UsuarioLogin + @"',
    CLAVEINGRESO = '" + ClaveIngreso + @"',
    CLAVETOTALES = '" + ClaveTotales + @"',
    CLAVEELIMINACION = '" + ClaveEliminar + @"',
    CLAVECONSULTA = '" + ClaveConsulta + @"',
    CLAVETABLAS = '" + ClaveTabla + @"',
    FACTURACION = '" + Facturacion + @"',
    IMPRESION = '" + Impresion + @"',
    TELEFONO = '" + Telefono + @"',
    PROPIETARIO = '" + Propietario + @"',
    EMAIL = '" + Email + @"',
    UBICACIONARCHIVOP12 = '" + UbicacionArchivoP12 + @"',
    CONTRASENA = '" + Contrasena + @"',
    IMAGEN = '" + Imagen + @"',
    ESTADORUC = '" + EstadoRuc + @"',
    RUC = '" + Ruc + @"'
WHERE NOMBRE = '" + Nombre + @"'";

            _log.CrearLog(
                "Se actualizó la empresa: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ELIMINAR
        // ======================================================
        public int Eliminar(string Nombre, string Usuario, string IP)
        {
            if (!string.IsNullOrEmpty(Nombre))
                Nombre = Nombre.Replace("'", "''");

            string NombreEmpresa = '"' + Nombre + '"';

            string sql = @"DELETE FROM EMPRESA 
                           WHERE NOMBRE = " + NombreEmpresa;

            _log.CrearLog(
                "Se eliminó la empresa: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ACTUALIZAR ESTADO RUC
        // ======================================================
        public int ActualizarEstadoRuc(
            string nombreEmpresa,
            string nuevoEstado,
            string usuario,
            string ip
        )
        {
            if (string.IsNullOrWhiteSpace(nombreEmpresa))
                throw new Exception("Empresa vacía.");

            string nom = nombreEmpresa.Trim().Replace("'", "''");
            string est = nuevoEstado.Trim().Replace("'", "''");

            string sql = $@"
                UPDATE EMPRESA
                SET ESTADORUC = '{est}'
                WHERE Trim(NOMBRE) = '{nom}'
            ";

            _log.CrearLog(
                "Se cambió el estado de la empresa: " + nombreEmpresa + " a " + nuevoEstado,
                usuario,
                ip,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // SINCRONIZAR NOMBRE EN PARAMETROS_FACTURAS
        // ======================================================
        public int SincronizarNombreParametros(string nombre)
        {
            nombre = (nombre ?? "").Replace("'", "''");

            string sql = $"UPDATE PARAMETROS_FACTURAS SET NOMBRE = '{nombre}'";

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // RUC PROVEEDOR (solo lectura)
        // ======================================================
        public string ObtenerRucProveedor()
        {
            string sql = "SELECT TOP 1 RUCPROVEEDOR FROM EMPRESA";
            var ds = _conexion.Seleccionar(sql);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return "";

            var val = ds.Tables[0].Rows[0]["RUCPROVEEDOR"];
            return val == DBNull.Value ? "" : Convert.ToString(val).Trim();
        }

        // ======================================================
        // CREDENCIALES MODIFICADAS
        // ======================================================
        public bool ObtenerCredencialesModificadas()
        {
            string sql = "SELECT TOP 1 CREDENCIALES_MODIFICADAS FROM EMPRESA";
            var ds = _conexion.Seleccionar(sql);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return false;

            var val = ds.Tables[0].Rows[0]["CREDENCIALES_MODIFICADAS"];
            return val != DBNull.Value && Convert.ToBoolean(val);
        }

        // ======================================================
        // CAMBIAR CREDENCIALES
        // ======================================================
        public int CambiarCredenciales(string nuevoUsuario, string nuevaClave)
        {
            nuevoUsuario = (nuevoUsuario ?? "").Replace("'", "''");
            nuevaClave   = (nuevaClave   ?? "").Replace("'", "''");

            string sql = $@"
UPDATE EMPRESA
SET USUARIO = '{nuevoUsuario}',
    CLAVEINGRESO = '{nuevaClave}',
    CREDENCIALES_MODIFICADAS = 1";

            return _conexion.Ejecutar(sql);
        }
    }
}