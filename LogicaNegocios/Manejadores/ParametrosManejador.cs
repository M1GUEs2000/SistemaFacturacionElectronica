using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class ParametrosManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public ParametrosManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // Obtener todos los parámetros
        public DataSet Mostrar()
        {
            string sql = @"SELECT *
                           FROM PARAMETROS_SRI
                           ORDER BY TIPOCOMPROBANTE";
            return _conexion.Seleccionar(sql);
        }

        // Obtener parámetros por tipo de comprobante (ej. '01')
        public DataSet ConsultarPorTipo(string tipoComprobante)
        {
            string tipo = "'" + tipoComprobante + "'";
            string sql = @"SELECT 
                                IDFACTURA,
                                TIPOCOMPROBANTE,
                                SECUENCIAL,
                                CODIGONUMERICO,
                                FECHAACTUALIZACION
                           FROM PARAMETROS_SRI
                           WHERE TIPOCOMPROBANTE = " + tipo;
            return _conexion.Seleccionar(sql);
        }

        public int ObtenerNumeroDigitosFactura()
        {
            string sql = "SELECT TOP 1 NUMERODIGITOS FROM PARAMETROS_FACTURAS";

            var ds = _conexion.Seleccionar(sql);

            if (ds != null &&
                ds.Tables.Count > 0 &&
                ds.Tables[0].Rows.Count > 0)
            {
                int digitos = 9; // valor por defecto

                int.TryParse(ds.Tables[0].Rows[0]["NUMERODIGITOS"].ToString(), out digitos);

                return digitos;
            }

            return 9; // fallback
        }

        // ==========================================================
        // 🔵 NUEVO: Obtener secuencial real (sin padding)
        // ==========================================================
        public long ObtenerSecuencialReal(string tipoComprobante)
        {
            var ds = ConsultarPorTipo(tipoComprobante);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                long sec = 1;
                long.TryParse(ds.Tables[0].Rows[0]["SECUENCIAL"].ToString(), out sec);
                return sec <= 0 ? 1 : sec;
            }
            return 1;
        }

        // ==========================================================
        // Ahora usa ObtenerSecuencialReal()
        // ==========================================================
        public string ObtenerSecuencialFormateado(string tipoComprobante)
        {
            long secReal = ObtenerSecuencialReal(tipoComprobante);

            // ← obtener número de dígitos desde PARAMETROS_FACTURAS
            int digitos = ObtenerNumeroDigitosFactura();

            return secReal.ToString().PadLeft(digitos, '0');
        }


        // Helper: obtener codigo numerico desde la tabla
        public string ObtenerCodigoNumerico(string tipoComprobante)
        {
            var ds = ConsultarPorTipo(tipoComprobante);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0]["CODIGONUMERICO"].ToString().PadLeft(8, '0');
            }
            return "00000000";
        }

        // Insertar nuevo parámetro
        public int Insertar(
            string tipoComprobante,
            long secuencial,
            string codigoNumerico,
            string fechaActualizacion,
            string Usuario,
            string IP
        )
        {
            string sql = @"INSERT INTO PARAMETROS_SRI(
                        TIPOCOMPROBANTE,
                        SECUENCIAL,
                        CODIGONUMERICO,
                        FECHAACTUALIZACION
                    ) VALUES (
                        '" + tipoComprobante + @"',
                        " + secuencial + @",
                        '" + codigoNumerico + @"',
                        '" + fechaActualizacion + @"'
                    )";

            // ✔ Log estándar
            _log.CrearLog(
                "Se insertaron los parametros del SRI: " + tipoComprobante,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }


        // Actualizar secuencial después de autorización
        public int IncrementarSecuencial(string tipoComprobante)
        {
            string sql = @"UPDATE PARAMETROS_SRI
                           SET SECUENCIAL = SECUENCIAL + 1,
                               FECHAACTUALIZACION = '" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + @"'
                           WHERE TIPOCOMPROBANTE = '" + tipoComprobante + @"'";
            return _conexion.Ejecutar(sql);
        }

        // Actualizar total
        public int Actualizar(
            string tipoComprobante,
            long secuencial,
            string codigoNumerico,
            string fechaActualizacion,
            string Usuario,
            string IP
        )
        {
            string sql = @"UPDATE PARAMETROS_SRI
                   SET SECUENCIAL = " + secuencial + @",
                       CODIGONUMERICO = '" + codigoNumerico + @"',
                       FECHAACTUALIZACION = '" + fechaActualizacion + @"'
                   WHERE TIPOCOMPROBANTE = '" + tipoComprobante + @"'";

            // ✔ Log estándar
            _log.CrearLog(
                "Se actualizó el parámetro SRI: " + tipoComprobante,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // Eliminar por tipo
        public int EliminarPorTipo(string tipoComprobante, string Usuario, string IP)
        {
            string sql = @"DELETE FROM PARAMETROS_SRI WHERE TIPOCOMPROBANTE = '" + tipoComprobante + "'";

            _log.CrearLog(
                "Se eliminó el parámetro SRI: " + tipoComprobante,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }


    }
}
