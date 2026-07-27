using AccesoDatos.Abstractions;
using System.Data;

namespace LogicaNegocios
{
    public class RetencionesManejador
    {

        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public RetencionesManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }

        // ======================================================
        // INSERTAR ENCABEZADO
        // ======================================================
        public int Insertar(
            string NumeroRetencion,
            string ClaveAcceso,
            string FechaEmision,
            string HoraEmision,
            string Ambiente,
            string Estado,
            string Codigo,
            string TipoEmision,

            string NumeroFactura,
            string ClaveAccesoFactura,
            string FechaFactura,

            string SujetoRetenido,
            string IdentificacionSujeto,
            string TipoIdentificacionSujeto,
            string DireccionSujeto,
            string RegimenSujeto,

            string TotalBaseImponible,
            string TotalRetencionRenta,
            string TotalRetencionIva,
            string TotalRetenido,

            string Observaciones,
            string Usuario,
            string IP
        )
        {
            string sql = @"
                INSERT INTO RETENCIONES(
                    NUMERORETENCION,
                    CLAVEACCESO,
                    FECHAEMISION,
                    HORAEMISION,
                    AMBIENTE,
                    ESTADO,
                    CODIGO,
                    TIPOEMISION,

                    NUMEROFACTURA,
                    CLAVEACCESOFACTURA,
                    FECHAFACTURA,

                    SUJETORETENIDO,
                    IDENTIFICACIONSUJETO,
                    TIPOIDENTIFICACIONSUJETO,
                    DIRECCIONSUJETO,
                    REGIMENSUJETO,

                    TOTALBASEIMPONIBLE,
                    TOTALRETENCIONRENTA,
                    TOTALRETENCIONIVA,
                    TOTALRETENIDO,

                    OBSERVACIONES
                ) VALUES (
                    '" + Escapar(NumeroRetencion) + @"',
                    '" + Escapar(ClaveAcceso) + @"',
                    '" + Escapar(FechaEmision) + @"',
                    '" + Escapar(HoraEmision) + @"',
                    '" + Escapar(Ambiente) + @"',
                    '" + Escapar(Estado) + @"',
                    '" + Escapar(Codigo) + @"',
                    '" + Escapar(TipoEmision) + @"',

                    '" + Escapar(NumeroFactura) + @"',
                    '" + Escapar(ClaveAccesoFactura) + @"',
                    '" + Escapar(FechaFactura) + @"',

                    '" + Escapar(SujetoRetenido) + @"',
                    '" + Escapar(IdentificacionSujeto) + @"',
                    '" + Escapar(TipoIdentificacionSujeto) + @"',
                    '" + Escapar(DireccionSujeto) + @"',
                    '" + Escapar(RegimenSujeto) + @"',

                    '" + Escapar(TotalBaseImponible) + @"',
                    '" + Escapar(TotalRetencionRenta) + @"',
                    '" + Escapar(TotalRetencionIva) + @"',
                    '" + Escapar(TotalRetenido) + @"',

                    '" + Escapar(Observaciones) + @"'
                )
            ";

            _log.CrearLog(
                "Se insertó la retención: " + NumeroRetencion,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ACTUALIZAR ENCABEZADO
        // ======================================================
        public int Actualizar(
            string NumeroRetencion,
            string Estado,
            string Codigo,
            string ClaveAcceso,
            string Ambiente,
            string TipoEmision,
            string FechaEmision,
            string HoraEmision,
            string Usuario,
            string IP
        )
        {
            string sql = @"
                UPDATE RETENCIONES SET
                    ESTADO = '" + Escapar(Estado) + @"',
                    CODIGO = '" + Escapar(Codigo) + @"',
                    CLAVEACCESO = '" + Escapar(ClaveAcceso) + @"',
                    AMBIENTE = '" + Escapar(Ambiente) + @"',
                    TIPOEMISION = '" + Escapar(TipoEmision) + @"',
                    FECHAEMISION = '" + Escapar(FechaEmision) + @"',
                    HORAEMISION = '" + Escapar(HoraEmision) + @"'
                WHERE
                    NUMERORETENCION = '" + Escapar(NumeroRetencion) + @"'
            ";

            _log.CrearLog(
                "Se actualizó la retención: " + NumeroRetencion,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ELIMINAR ENCABEZADO
        // ======================================================
        public int Eliminar(string NumeroRetencion, string Usuario, string IP)
        {
            string sql = @"
                DELETE FROM RETENCIONES
                WHERE NUMERORETENCION = '" + Escapar(NumeroRetencion) + @"'
            ";

            _log.CrearLog(
                "Se eliminó la retención: " + NumeroRetencion,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // CONSULTAR ENCABEZADO
        // ======================================================
        public DataSet ConsultarPorNumero(string NumeroRetencion)
        {
            string sql = @"
                SELECT *
                FROM RETENCIONES
                WHERE NUMERORETENCION = '" + Escapar(NumeroRetencion) + @"'
            ";

            return _conexion.Seleccionar(sql);
        }


        // ======================================================
        // INSERTAR DETALLE
        // ======================================================
        public int InsertarDetalle(
            string NumeroRetencion,
            string TipoImpuesto,
            string CodigoImpuesto,
            string BaseImponible,
            string PorcentajeRetencion,
            string ValorRetenido,
            string TipoOperacion
        )
        {
            string sql = @"
                INSERT INTO RETENCIONES_DETALLE(
                    NUMERORETENCION,
                    TIPOIMPUESTO,
                    CODIGOIMPUESTO,
                    BASEIMPONIBLE,
                    PORCENTAJERETENCION,
                    VALORRETENIDO,
                    TIPOOPERACION
                ) VALUES (
                    '" + Escapar(NumeroRetencion) + @"',
                    '" + Escapar(TipoImpuesto) + @"',
                    '" + Escapar(CodigoImpuesto) + @"',
                    '" + Escapar(BaseImponible) + @"',
                    '" + Escapar(PorcentajeRetencion) + @"',
                    '" + Escapar(ValorRetenido) + @"',
                    '" + Escapar(TipoOperacion) + @"'
                )
            ";

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // CONSULTAR DETALLE
        // ======================================================
        public DataSet ConsultarDetalle(string NumeroRetencion)
        {
            string sql = @"
                SELECT *
                FROM RETENCIONES_DETALLE
                WHERE NUMERORETENCION = '" + Escapar(NumeroRetencion) + @"'
                ORDER BY TIPOIMPUESTO
            ";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // ELIMINAR DETALLE
        // ======================================================
        public int EliminarDetalle(string NumeroRetencion)
        {
            string sql = @"
                DELETE FROM RETENCIONES_DETALLE
                WHERE NUMERORETENCION = '" + Escapar(NumeroRetencion) + @"'
            ";

            return _conexion.Ejecutar(sql);
        }

        public void ActualizarNumeroYEstado(
            string numeroActual,
            string numeroNuevo,
            string estado,
            string usuario,
            string ip)
        {
            string sqlEncabezado = @"
                UPDATE RETENCIONES SET
                    NUMERORETENCION = @numeroNuevo,
                    ESTADO = @estado
                WHERE NUMERORETENCION = @numeroActual";

            _log.CrearLog(
                "Retención renumerada " + numeroActual + " -> " +
                numeroNuevo + " [" + estado + "]",
                usuario,
                ip,
                sqlEncabezado
            );

            _conexion.Ejecutar(
                sqlEncabezado,
                ("numeroNuevo", numeroNuevo),
                ("estado", estado),
                ("numeroActual", numeroActual)
            );

            string sqlDetalle = @"
                UPDATE RETENCIONES_DETALLE SET
                    NUMERORETENCION = @numeroNuevo
                WHERE NUMERORETENCION = @numeroActual";

            _conexion.Ejecutar(
                sqlDetalle,
                ("numeroNuevo", numeroNuevo),
                ("numeroActual", numeroActual)
            );
        }

        public string ObtenerSecuencialEmitiendo()
        {
            const string prefijo = "EMITIENDO";
            DataSet ds = _conexion.Seleccionar(
                "SELECT NUMERORETENCION FROM RETENCIONES WHERE NUMERORETENCION LIKE 'EMITIENDO%'");
            int max = 0;

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string valor = row["NUMERORETENCION"].ToString().Trim();
                    if (!valor.StartsWith(prefijo, System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    int numero;
                    if (int.TryParse(valor.Substring(prefijo.Length).Trim(), out numero) &&
                        numero > max)
                    {
                        max = numero;
                    }
                }
            }

            return prefijo + (max + 1).ToString("000");
        }

        // ======================================================
        // HELPERS
        // ======================================================
        private string Escapar(string s)
        {
            return (s ?? "").Trim().Replace("'", "''");
        }

        public DataSet Listar(int Top, string Filtro = "")
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(Filtro))
                where = " WHERE " + Filtro;

            string sql = @"
        SELECT TOP " + Top + @" *
        FROM RETENCIONES
        " + where + @"
        ORDER BY FECHAEMISION DESC, HORAEMISION DESC
    ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ListarNumerosFactura()
        {
            string sql = @"
        SELECT Q.NUMEROFACTURA
        FROM
        (
            SELECT DISTINCT
                Trim(NUMEROFACTURA) AS NUMEROFACTURA
            FROM RETENCIONES
            WHERE
                NUMEROFACTURA IS NOT NULL
                AND Trim(NUMEROFACTURA) <> ''
                AND Left(Trim(NUMEROFACTURA), 1) BETWEEN '0' AND '9'
        ) AS Q
        ORDER BY Q.NUMEROFACTURA DESC
    ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ListarSujetosRetenidos()
        {
            string sql = @"
        SELECT
            Q.SUJETORETENIDO,
            Q.IDENTIFICACIONSUJETO
        FROM
        (
            SELECT DISTINCT
                Trim(SUJETORETENIDO) AS SUJETORETENIDO,
                Trim(IDENTIFICACIONSUJETO) AS IDENTIFICACIONSUJETO
            FROM RETENCIONES
            WHERE
                SUJETORETENIDO IS NOT NULL
                AND Trim(SUJETORETENIDO) <> ''
        ) AS Q
        ORDER BY Q.SUJETORETENIDO
    ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ListarNumerosRetencion()
        {
            string sql = @"
        SELECT Q.NUMERORETENCION
        FROM
        (
            SELECT DISTINCT
                Trim(NUMERORETENCION) AS NUMERORETENCION,
                FECHAEMISION
            FROM RETENCIONES
            WHERE
                NUMERORETENCION IS NOT NULL
                AND Trim(NUMERORETENCION) <> ''
        ) AS Q
        ORDER BY
            CDate(Q.FECHAEMISION) DESC,
            Q.NUMERORETENCION DESC
    ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ConsultarAvanzado(
            string fechaDesde,
            string fechaHasta,
            string sujetoRetenido,
            string numeroRetencion,
            string numeroFactura
        )
        {
            string fd = Escapar(fechaDesde);
            string fh = Escapar(fechaHasta);
            string sujeto = Escapar(sujetoRetenido);
            string nr = Escapar(numeroRetencion);
            string nf = Escapar(numeroFactura);

            // ---------------------------------
            // WHERE base (FECHAS OBLIGATORIAS)
            // ---------------------------------
            string where = @"
        DateValue(" + AccessDateFromText(fd) + @") <= DateValue(CDate(FECHAEMISION))
        AND DateValue(CDate(FECHAEMISION)) <= DateValue(" + AccessDateFromText(fh) + @")
    ";

            // ---------------------------------
            // FILTROS ACUMULATIVOS (SIN PRIORIDAD)
            // ---------------------------------
            if (!string.IsNullOrWhiteSpace(nr))
            {
                where += $" AND Trim(UCase(NUMERORETENCION)) = '{nr.ToUpperInvariant()}'";
            }

            if (!string.IsNullOrWhiteSpace(nf))
            {
                where += $" AND Trim(NUMEROFACTURA) = '{nf}'";
            }

            if (!string.IsNullOrWhiteSpace(sujeto))
            {
                where += $" AND Trim(UCase(SUJETORETENIDO)) LIKE '%{sujeto.ToUpperInvariant()}%'";
            }

            string sql = @"
        SELECT
            NUMERORETENCION,
            FECHAEMISION,
            NUMEROFACTURA,
            FECHAFACTURA,
            SUJETORETENIDO,
            IDENTIFICACIONSUJETO AS IDENTIFICACION,
            TOTALBASEIMPONIBLE,
            TOTALRETENCIONRENTA,
            TOTALRETENCIONIVA,
            TOTALRETENIDO
        FROM RETENCIONES
        WHERE " + where + @"
        ORDER BY
            CDate(FECHAEMISION) DESC,
            NUMERORETENCION DESC
    ";

            return _conexion.Seleccionar(sql);
        }
        private string AccessDateFromText(string yyyyMMdd)
        {
            var p = (yyyyMMdd ?? "").Trim().Split('/');
            if (p.Length != 3) return "Date()";

            // yyyy, MM, dd  ← ORDEN CORRECTO
            return $"DateSerial({p[0]},{p[1]},{p[2]})";
        }


    }
}
