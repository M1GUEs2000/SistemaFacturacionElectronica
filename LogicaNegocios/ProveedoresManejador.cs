using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class ProveedoresManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public ProveedoresManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // ======================================================
        // INSERTAR PROVEEDOR
        // ======================================================
        public int Insertar(
            string TipoIdentificacion,
            string Identificacion,
            string RazonSocial,
            string Correo,
            string Direccion,
            string Telefono,
            string TipoPersona,
            bool EsRimpe,
            string TipoRimpe,
            bool EsProfesional,
            bool EsArrendador,
            string Estado,
            string Usuario,
            string IP
        )
        {
            string fechaSql = "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";

            string sql = @"
                INSERT INTO PROVEEDORES
                (
                    TIPOIDENTIFICACION,
                    IDENTIFICACION,
                    RAZONSOCIAL,
                    CORREO,
                    DIRECCION,
                    TELEFONO,
                    TIPOPERSONA,
                    ESRIMPE,
                    TIPORIMPE,
                    ESPROFESIONAL,
                    ESARRENDADOR,
                    ESTADO,
                    FECHAREGISTRO
                )
                VALUES
                (
                    '" + Escapar(TipoIdentificacion) + @"',
                    '" + Escapar(Identificacion) + @"',
                    '" + Escapar(RazonSocial) + @"',
                    '" + Escapar(Correo) + @"',
                    '" + Escapar(Direccion) + @"',
                    '" + Escapar(Telefono) + @"',
                    '" + Escapar(TipoPersona) + @"',
                    " + (EsRimpe ? "1" : "0") + @",
                    '" + Escapar(TipoRimpe) + @"',
                    " + (EsProfesional ? "1" : "0") + @",
                    " + (EsArrendador ? "1" : "0") + @",
                    '" + Escapar(Estado) + @"',
                    " + fechaSql + @"
                )
            ";

            _log.CrearLog(
                "Se insertó el proveedor: " + RazonSocial,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ACTUALIZAR PROVEEDOR
        // ======================================================
        public int Actualizar(
            int IdProveedor,
            string TipoIdentificacion,
            string Identificacion,
            string RazonSocial,
            string Correo,
            string Direccion,
            string Telefono,
            string TipoPersona,
            bool EsRimpe,
            string TipoRimpe,
            bool EsProfesional,
            bool EsArrendador,
            string Estado,
            string Usuario,
            string IP
        )
        {
            string sql = @"
                UPDATE PROVEEDORES
                SET
                    TIPOIDENTIFICACION = '" + Escapar(TipoIdentificacion) + @"',
                    IDENTIFICACION     = '" + Escapar(Identificacion) + @"',
                    RAZONSOCIAL        = '" + Escapar(RazonSocial) + @"',
                    CORREO             = '" + Escapar(Correo) + @"',
                    DIRECCION          = '" + Escapar(Direccion) + @"',
                    TELEFONO           = '" + Escapar(Telefono) + @"',
                    TIPOPERSONA        = '" + Escapar(TipoPersona) + @"',
                    ESRIMPE            = " + (EsRimpe ? "1" : "0") + @",
                    TIPORIMPE          = '" + Escapar(TipoRimpe) + @"',
                    ESPROFESIONAL      = " + (EsProfesional ? "1" : "0") + @",
                    ESARRENDADOR       = " + (EsArrendador ? "1" : "0") + @",
                    ESTADO             = '" + Escapar(Estado) + @"'
                WHERE
                    IDPROVEEDOR = " + IdProveedor + @"
            ";

            _log.CrearLog(
                "Se actualizó el proveedor: " + RazonSocial,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ELIMINAR PROVEEDOR
        // ======================================================
        public int Eliminar(int IdProveedor, string Usuario, string IP)
        {
            string sql = @"
                DELETE FROM PROVEEDORES
                WHERE IDPROVEEDOR = " + IdProveedor + @"
            ";

            _log.CrearLog(
                "Se eliminó el proveedor ID: " + IdProveedor,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // CONSULTAR POR ID
        // ======================================================
        public DataSet ConsultarPorId(int IdProveedor)
        {
            string sql = @"
                SELECT *
                FROM PROVEEDORES
                WHERE IDPROVEEDOR = " + IdProveedor + @"
            ";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // CONSULTAR POR IDENTIFICACION
        // ======================================================
        public DataSet ConsultarPorIdentificacion(string Identificacion)
        {
            string sql = @"
                SELECT *
                FROM PROVEEDORES
                WHERE IDENTIFICACION = '" + Escapar(Identificacion) + @"'
            ";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // EXISTE PROVEEDOR
        // ======================================================
        public bool Existe(string Identificacion)
        {
            string sql = @"
                SELECT IDPROVEEDOR
                FROM PROVEEDORES
                WHERE IDENTIFICACION = '" + Escapar(Identificacion) + @"'
            ";

            DataSet ds = _conexion.Seleccionar(sql);

            return (
                ds != null &&
                ds.Tables.Count > 0 &&
                ds.Tables[0].Rows.Count > 0
            );
        }

        // ======================================================
        // LISTAR TODOS
        // ======================================================
        public DataSet Listar()
        {
            string sql = @"
                SELECT *
                FROM PROVEEDORES
                ORDER BY RAZONSOCIAL
            ";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // LISTAR ACTIVOS
        // ======================================================
        public DataSet ListarActivos()
        {
            string sql = @"
                SELECT *
                FROM PROVEEDORES
                WHERE ESTADO = 'ACTIVO'
                ORDER BY RAZONSOCIAL
            ";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // ESCAPAR STRINGS
        // ======================================================
        private string Escapar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? ""
                : valor.Replace("'", "''").Trim();
        }
    }
}