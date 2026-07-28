using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    /// <summary>
    /// Manejador de la tabla lookup PARAMETROS_TRANSACCIONES (NOMBRE / VALOR / CODIGO / ACTIVO).
    ///
    /// Relación entre registros (por CODIGO compartido):
    ///   NOMBRE=TIPOPAGO      VALOR=DIFERIDO/CORRIENTE/AMBOS   CODIGO=D/C/A   (ACTIVO=1 marca el modo vigente)
    ///   NOMBRE=TIPODIFERIDO  VALOR=SIN INTERESES...           CODIGO=T1..T7  (ACTIVO=1 marca los habilitados en el sistema)
    ///   NOMBRE=CUOTA         VALOR=(nº cuotas)                CODIGO=T1..T7  (ligada a su TIPODIFERIDO por mismo CODIGO)
    ///   NOMBRE=MINIMOFIRMA   VALOR=10                         CODIGO=F1
    ///   NOMBRE=MODOPAGO      VALOR=MANUAL - TOKEN MANUAL/BANDA/CHIP/FALLBACK MANUAL (CHIP)/FALLBACK BANDA (CHIP)/CTLS - TOKEN CTLS   CODIGO=01..06
    ///   NOMBRE=PUBLICIDAD    VALOR=(texto de publicidad)      CODIGO=P       (ACTIVO=1 marca si se muestra)
    /// </summary>
    public class ParametrosTransaccionesManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public ParametrosTransaccionesManejador(
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
            string sql = @"SELECT NOMBRE, VALOR, CODIGO, ACTIVO
                           FROM PARAMETROS_TRANSACCIONES
                           ORDER BY NOMBRE, CODIGO";

            return _conexion.Seleccionar(sql);
        }

        // ===========================
        // CONSULTAR POR NOMBRE
        // ===========================
        public DataSet ConsultarPorNombre(string Nombre)
        {
            string sql = @"SELECT NOMBRE, VALOR, CODIGO, ACTIVO
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE NOMBRE = @nombre
                           ORDER BY CODIGO";

            return _conexion.Seleccionar(sql, ("nombre", Nombre));
        }

        // ===========================
        // CONSULTAR POR CODIGO
        // ===========================
        public DataSet ConsultarPorCodigo(string Codigo)
        {
            string sql = @"SELECT NOMBRE, VALOR, CODIGO, ACTIVO
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE CODIGO = @codigo";

            return _conexion.Seleccionar(sql, ("codigo", Codigo));
        }

        // ===========================
        // FILTROS RELACIONADOS
        // ===========================

        /// <summary>Opciones de TIPO DE PAGO (CORRIENTE / DIFERIDO / AMBOS). La marcada ACTIVO=1 es el modo vigente.</summary>
        public DataSet ObtenerTiposPago()
        {
            return ConsultarPorNombre("TIPOPAGO");
        }

        /// <summary>Todas las opciones de TIPO DIFERIDO (códigos T1..T7), activas o no.</summary>
        public DataSet ObtenerTiposDiferido()
        {
            return ConsultarPorNombre("TIPODIFERIDO");
        }

        /// <summary>Tipos de diferido habilitados (ACTIVO=1) — lo que debe ofrecer el resto del sistema.</summary>
        public DataSet ObtenerTiposDiferidoActivos()
        {
            string sql = @"SELECT NOMBRE, VALOR, CODIGO, ACTIVO
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE NOMBRE = 'TIPODIFERIDO'
                             AND ACTIVO = 1
                           ORDER BY CODIGO";

            return _conexion.Seleccionar(sql);
        }

        /// <summary>
        /// Cuota asociada a un tipo diferido. Se relacionan por CODIGO:
        /// el diferido con CODIGO 'T1' comparte código con la fila NOMBRE 'CUOTA', CODIGO 'T1'.
        /// </summary>
        public DataSet ObtenerCuotasPorDiferido(string CodigoDiferido)
        {
            string sql = @"SELECT NOMBRE, VALOR, CODIGO, ACTIVO
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE NOMBRE = 'CUOTA'
                             AND CODIGO = @codigo";

            return _conexion.Seleccionar(sql, ("codigo", CodigoDiferido));
        }

        /// <summary>Todas las opciones de MODO DE PAGO (códigos 01..06), activas o no.</summary>
        public DataSet ObtenerModosPago()
        {
            return ConsultarPorNombre("MODOPAGO");
        }

        /// <summary>Modos de pago habilitados (ACTIVO=1) — lo que debe ofrecer el resto del sistema.</summary>
        public DataSet ObtenerModosPagoActivos()
        {
            string sql = @"SELECT NOMBRE, VALOR, CODIGO, ACTIVO
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE NOMBRE = 'MODOPAGO'
                             AND ACTIVO = 1
                           ORDER BY CODIGO";

            return _conexion.Seleccionar(sql);
        }

        /// <summary>Valor único de un parámetro por su NOMBRE (ej: MINIMOFIRMA => "10"). "" si no existe.</summary>
        public string ObtenerValorPorNombre(string Nombre)
        {
            string sql = @"SELECT VALOR
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE NOMBRE = @nombre";

            DataSet ds = _conexion.Seleccionar(sql, ("nombre", Nombre));

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return "";

            return ds.Tables[0].Rows[0]["VALOR"].ToString().Trim();
        }

        /// <summary>Monto mínimo para firma de baucher (fila NOMBRE 'MINIMOFIRMA').</summary>
        public string ObtenerMinimoFirma()
        {
            return ObtenerValorPorNombre("MINIMOFIRMA");
        }

        /// <summary>
        /// Texto de publicidad del baucher (fila NOMBRE 'PUBLICIDAD', CODIGO 'P').
        /// Solo devuelve el texto si la fila está ACTIVO=1; si está inactiva devuelve ""
        /// y el baucher imprime esa línea en blanco.
        /// </summary>
        public string ObtenerPublicidad()
        {
            string sql = @"SELECT VALOR
                           FROM PARAMETROS_TRANSACCIONES
                           WHERE NOMBRE = 'PUBLICIDAD'
                             AND CODIGO = 'P'
                             AND ACTIVO = 1";

            DataSet ds = _conexion.Seleccionar(sql);

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return "";

            return ds.Tables[0].Rows[0]["VALOR"].ToString().Trim();
        }

        // ===========================
        // INSERTAR
        // ===========================
        public int Insertar(string Nombre, string Valor, string Codigo, string Usuario, string IP)
        {
            string sql = @"
        INSERT INTO PARAMETROS_TRANSACCIONES(NOMBRE, VALOR, CODIGO)
        VALUES(@nombre, @valor, @codigo)";

            _log.CrearLog(
                "Insertó parámetro de transacción: " + Nombre + " (" + Codigo + ")",
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql, ("nombre", Nombre), ("valor", Valor), ("codigo", Codigo));
        }

        // ===========================
        // ACTUALIZAR (por CODIGO + NOMBRE)
        // ===========================
        // CODIGO ya no es único por fila: TIPODIFERIDO y su CUOTA comparten el mismo
        // CODIGO (ej. "T1"). El WHERE debe filtrar también por NOMBRE para no pisar
        // la fila hermana.
        public int Actualizar(string Codigo, string Nombre, string Valor, string Usuario, string IP)
        {
            string sql = @"
        UPDATE PARAMETROS_TRANSACCIONES SET
            VALOR = @valor
        WHERE CODIGO = @codigo
          AND NOMBRE = @nombre";

            _log.CrearLog(
                "Actualizó parámetro de transacción: " + Nombre + " (" + Codigo + ")",
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql, ("valor", Valor), ("codigo", Codigo), ("nombre", Nombre));
        }

        /// <summary>Actualiza el texto de publicidad (fila NOMBRE 'PUBLICIDAD', CODIGO 'P').</summary>
        public int ActualizarPublicidad(string Valor, string Usuario, string IP)
        {
            return Actualizar("P", "PUBLICIDAD", Valor, Usuario, IP);
        }

        // ===========================
        // ACTUALIZAR ACTIVO (por CODIGO + NOMBRE)
        // ===========================
        public int ActualizarActivo(string Codigo, string Nombre, bool Activo, string Usuario, string IP)
        {
            string sql = @"
        UPDATE PARAMETROS_TRANSACCIONES SET
            ACTIVO = @activo
        WHERE CODIGO = @codigo
          AND NOMBRE = @nombre";

            _log.CrearLog(
                "Actualizó estado activo de parámetro de transacción: " + Nombre + " (" + Codigo + ") => " + Activo,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql, ("activo", Activo), ("codigo", Codigo), ("nombre", Nombre));
        }

        // ===========================
        // ELIMINAR (por CODIGO + NOMBRE)
        // ===========================
        public int Eliminar(string Codigo, string Nombre, string Usuario, string IP)
        {
            string sql = @"
        DELETE FROM PARAMETROS_TRANSACCIONES
        WHERE CODIGO = @codigo
          AND NOMBRE = @nombre";

            _log.CrearLog(
                "Eliminó parámetro de transacción: " + Nombre + " (" + Codigo + ")",
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql, ("codigo", Codigo), ("nombre", Nombre));
        }
    }
}
