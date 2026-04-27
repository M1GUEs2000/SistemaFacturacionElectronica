using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class ParametrosFacturasManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public ParametrosFacturasManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // ===========================
        // MOSTRAR
        // ===========================
        public DataSet Mostrar()
        {
            string sql = @"SELECT * 
                           FROM PARAMETROS_FACTURAS 
                           ORDER BY NOMBRE";

            return _conexion.Seleccionar(sql);
        }

        // ===========================
        // CONSULTAR POR NOMBRE
        // ===========================
        public DataSet ConsultarNombre(string Nombre)
        {
            Nombre = Nombre.Replace("'", "''");

            string sql = $@"SELECT *
                            FROM PARAMETROS_FACTURAS
                            WHERE NOMBRE LIKE '{Nombre}'";

            return _conexion.Seleccionar(sql);
        }

        public bool EsProduccion(string nombre)
        {
            nombre = nombre.Replace("'", "''");

            string sql = $@"
        SELECT AMBIENTE
        FROM PARAMETROS_FACTURAS
        WHERE NOMBRE = '{nombre}'";

            DataSet ds = _conexion.Seleccionar(sql);

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                throw new Exception("No existen parámetros configurados para el nombre indicado.");

            DataRow row = ds.Tables[0].Rows[0];

            string ambiente = row["AMBIENTE"].ToString().Trim();

            // PRODUCCIÓN SOLO SI AMBOS SON 2
            return ambiente == "2";
        }

        public void CambiarAProduccion(string nombre, string Usuario, string IP)
        {
            nombre = nombre.Replace("'", "''");

            string sql = $@"
        UPDATE PARAMETROS_FACTURAS
        SET AMBIENTE = 2
        WHERE NOMBRE = '{nombre}'";

            _log.CrearLog(
                "Cambió a PRODUCCIÓN los parámetros de factura de: " + nombre,
                Usuario,
                IP,
                sql
            );

            _conexion.Ejecutar(sql);
        }

        public int Insertar(
           string Nombre,
           string Ambiente,
           string TipoEmision,
           string AgenteRetencion,
           string ContribuyenteRimpe,
           string CodDoc,
           string Estab,
           string PuntoEmision,
           string NumeroDigitos,
           string ContribuyenteEspecial,
           string ObligadoContabilidad,
           string TipoIdentComprador,
           string Moneda,
           string CodigoImpuesto,
           string CodigoPorcentaje,
           string FechaActualizacion,
           string SMTPSERVER,
           string SMTPPORT,
           string SMTPUSER,
           string SMTPPASS,
           string Usuario,
           string IP
        )
        {
            Nombre = Nombre.Replace("'", "''");

            string sql = @"
        INSERT INTO PARAMETROS_FACTURAS(
            NOMBRE,
            AMBIENTE,
            TIPOEMISION,
            AGENTERETENCION,
            CONTRIBUYENTERIMPE,
            CODDOC,
            ESTAB,
            PUNTOEMISION,
            NUMERODIGITOS,
            CONTRIBUYENTEESPECIAL,
            OBLIGADOCONTABILIDAD,
            TIPOIDENTIFICADORCOMPRADOR,
            MONEDA,
            CODIGOIMPUESTO,
            CODIGOPORCENTAJE,
            FECHAACTUALIZACION,
            SMTPSERVER,
            SMTPPORT,
            SMTPUSER,
            SMTPPASS
        ) VALUES (
            '" + Nombre + @"',
            '" + Ambiente + @"',
            '" + TipoEmision + @"',
            '" + AgenteRetencion + @"',
            '" + ContribuyenteRimpe + @"',
            '" + CodDoc + @"',
            '" + Estab + @"',
            '" + PuntoEmision + @"',
            '" + NumeroDigitos + @"',
            '" + ContribuyenteEspecial + @"',
            '" + ObligadoContabilidad + @"',
            '" + TipoIdentComprador + @"',
            '" + Moneda + @"',
            '" + CodigoImpuesto + @"',
            '" + CodigoPorcentaje + @"',
            '" + FechaActualizacion + @"',
            '" + SMTPSERVER + @"',
            '" + SMTPPORT + @"',
            '" + SMTPUSER + @"',
            '" + SMTPPASS + @"'
        )";

            _log.CrearLog(
                "Insertó parámetros de factura de: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }


        // ===========================
        // ACTUALIZAR
        // ===========================
        public int Actualizar(
           string Nombre,
           string Ambiente,
           string TipoEmision,
           string AgenteRetencion,
           string ContribuyenteRimpe,
           string CodDoc,
           string Estab,
           string PuntoEmision,
           string NumeroDigitos,
           string ContribuyenteEspecial,
           string ObligadoContabilidad,
           string TipoIdentComprador,
           string Moneda,
           string CodigoImpuesto,
           string CodigoPorcentaje,
           string FechaActualizacion,
           string SMTPSERVER,
           string SMTPPORT,
           string SMTPUSER,
           string SMTPPASS,
           string Usuario,
           string IP
        )
        {
            Nombre = Nombre.Replace("'", "''");

            string sql = @"
        UPDATE PARAMETROS_FACTURAS SET
            AMBIENTE = '" + Ambiente + @"',
            TIPOEMISION = '" + TipoEmision + @"',
            AGENTERETENCION = '" + AgenteRetencion + @"',
            CONTRIBUYENTERIMPE = '" + ContribuyenteRimpe + @"',
            CODDOC = '" + CodDoc + @"',
            ESTAB = '" + Estab + @"',
            PUNTOEMISION = '" + PuntoEmision + @"',
            NUMERODIGITOS = '" + NumeroDigitos + @"',
            CONTRIBUYENTEESPECIAL = '" + ContribuyenteEspecial + @"',
            OBLIGADOCONTABILIDAD = '" + ObligadoContabilidad + @"',
            TIPOIDENTIFICADORCOMPRADOR = '" + TipoIdentComprador + @"',
            MONEDA = '" + Moneda + @"',
            CODIGOIMPUESTO = '" + CodigoImpuesto + @"',
            CODIGOPORCENTAJE = '" + CodigoPorcentaje + @"',
            FECHAACTUALIZACION = '" + FechaActualizacion + @"',
            SMTPSERVER = '" + SMTPSERVER + @"',
            SMTPPORT = '" + SMTPPORT + @"',
            SMTPUSER = '" + SMTPUSER + @"',
            SMTPPASS = '" + SMTPPASS + @"'
        WHERE NOMBRE = '" + Nombre + @"'
    ";

            _log.CrearLog(
                "Actualizó parámetros de factura de: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ===========================
        // ELIMINAR
        // ===========================
        public int Eliminar(string Nombre, string Usuario, string IP)
        {
            Nombre = Nombre.Replace("'", "''");

            string sql = @"
        DELETE FROM PARAMETROS_FACTURAS
        WHERE NOMBRE = '" + Nombre + @"'
    ";

            _log.CrearLog(
                "Eliminó parámetros de factura: " + Nombre,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }


    }
}
