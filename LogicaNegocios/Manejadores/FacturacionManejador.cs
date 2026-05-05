using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class FacturacionManejador
    {
        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public FacturacionManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }
        public int Insertar(
            string FECHA, string FORMADEPAGO, string PRODUCTO,
            string CANTIDAD, string TOTAL, string CEDULA, string HORA,
            string NUMEROFACTURA,
            string Usuario,
            string IP
        )
        {
            string sql = @"INSERT INTO FACTURACION 
        (FECHA, FORMADEPAGO, PRODUCTO, CANTIDAD, TOTAL, CLIENTE, HORA, NUMEROFACTURA)
        VALUES('" + FECHA + "','" + FORMADEPAGO + "','" + PRODUCTO + "','"
                             + CANTIDAD + "','" + TOTAL + "','" + CEDULA + "','" + HORA + "','" + NUMEROFACTURA + "')";

            // ✔ Log estándar (proceso, usuario, ip, texto)
            _log.CrearLog(
                "Se registro la factura Nº " + NUMEROFACTURA + " del cliente: " + CEDULA,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        public int Eliminar(
            string FECHA, string FORMADEPAGO, string PRODUCTO,
            string CANTIDAD, string TOTAL, string CLIENTE, string HORA,
            string NUMEROFACTURA,
            string Usuario,
            string IP
        )
        {
            string sql =
            @"DELETE FROM FACTURACION 
              WHERE FECHA = '" + FECHA + @"'
              AND FORMADEPAGO = '" + FORMADEPAGO + @"'
              AND PRODUCTO = '" + PRODUCTO + @"'
              AND CANTIDAD = '" + CANTIDAD + @"'
              AND TOTAL = '" + TOTAL + @"'
              AND CLIENTE = '" + CLIENTE + @"'
              AND HORA = '" + HORA + @"'
              AND NUMEROFACTURA = '" + NUMEROFACTURA + @"'";


            // ✔ Log estándar
            _log.CrearLog(
                                    "Se eliminó una factura del cliente: " + CLIENTE + "con numero de factura:" + NUMEROFACTURA,
                                    Usuario,
                                    IP,
                                    sql
    );

            return _conexion.Ejecutar(sql);
        }

        public void EliminarPorSecuencial(string numeroFactura, string usuario, string ip)
        {
            string sql = @"DELETE FROM FACTURACION WHERE NUMEROFACTURA = '" + numeroFactura + "'";
            _log.CrearLog("Factura no enviada al SRI - Eliminada: " + numeroFactura, usuario, ip, sql);
            _conexion.Ejecutar(sql);
        }

        public DataSet ConsultarTotales(string Fecha)
        {
            DataSet dsdatos = new DataSet();
            string sql = "SELECT FECHA, FORMADEPAGO,   SUM(CANTIDAD) AS CANTIDADES, SUM(TOTAL) AS TOTALES, NUMEROFACTURA FROM FACTURACION WHERE FECHA='" + Fecha + "' GROUP BY FECHA,FORMADEPAGO, NUMEROFACTURA ";
            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }

        public DataSet ConsultarFecha(string FechaDesde, string FechaHasta, string Producto, string Cliente, string FormaPago)
        {
            DataSet dsdatos = new DataSet();

            string sql = @"
                         SELECT 
                             F.FECHA,
                             F.FORMADEPAGO AS [FORMA DE PAGO],
                             F.PRODUCTO AS [NOMBRES DE PRODUCTOS],
                             F.CANTIDAD,
                             F.TOTAL,
                             C.NOMBRE AS CLIENTE,
                             C.CEDULA, 
                             F.HORA,
                             F.NUMEROFACTURA
                         FROM FACTURACION AS F
                         LEFT JOIN CLIENTE AS C
                             ON F.CLIENTE = C.CEDULA
                         WHERE F.FECHA >= '" + FechaDesde + @"'
                           AND F.FECHA <= '" + FechaHasta + @"'";

            if (!string.IsNullOrEmpty(Producto) && Producto != "seleccione")
                sql += " AND F.PRODUCTO='" + Producto + "'";

            if (!string.IsNullOrEmpty(Cliente) && Cliente != "seleccione")
                sql += " AND F.CLIENTE = '" + Cliente + "'";

            if (!string.IsNullOrEmpty(FormaPago) && FormaPago != "seleccione")
                sql += " AND F.FORMADEPAGO='" + FormaPago + "'";

            sql += " ORDER BY F.FECHA DESC, F.NUMEROFACTURA DESC";

            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }

        public DataSet ConsultarFechas(string FechaDesde, string FechaHasta)
        {
            DataSet dsdatos = new DataSet();

            string sql = @"
        SELECT 
            F.FECHA,
            F.FORMADEPAGO AS [FORMA DE PAGO],
            F.PRODUCTO AS [NOMBRES DE PRODUCTOS],
            F.CANTIDAD,
            F.TOTAL,
            C.NOMBRE AS CLIENTE,
            C.CEDULA,
            F.HORA,
            F.NUMEROFACTURA
        FROM FACTURACION AS F
        LEFT JOIN CLIENTE AS C
            ON F.CLIENTE = C.CEDULA
        WHERE F.FECHA >= '" + FechaDesde + @"'
          AND F.FECHA <= '" + FechaHasta + @"'
        ORDER BY
            F.FECHA DESC,
            F.NUMEROFACTURA DESC";

            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }
        public DataSet ConsultarFechasPorCliente(string FechaDesde, string FechaHasta, string Cliente)
        {
            DataSet dsdatos = new DataSet();

            string sql = @"
        SELECT 
            F.FECHA,
            F.FORMADEPAGO AS [FORMA DE PAGO],
            F.PRODUCTO AS [NOMBRES DE PRODUCTOS],
            F.CANTIDAD,
            F.TOTAL,
            C.NOMBRE AS CLIENTE,
            C.CEDULA,
            F.HORA,
            F.NUMEROFACTURA
        FROM FACTURACION AS F
        LEFT JOIN CLIENTE AS C
            ON F.CLIENTE = C.CEDULA
        WHERE F.FECHA >= '" + FechaDesde + @"'
          AND F.FECHA <= '" + FechaHasta + @"'
          AND F.CLIENTE = '" + Cliente + @"'
        ORDER BY
            F.FECHA DESC,
            F.NUMEROFACTURA DESC";

            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }

        public DataSet ConsultarCliente()
        {
            DataSet dsdatos = new DataSet();

            string sql = @"
                            SELECT C.NOMBRE, C.CEDULA
                            FROM FACTURACION AS F
                            INNER JOIN CLIENTE AS C
                                ON F.CLIENTE = C.CEDULA
                            GROUP BY C.NOMBRE, C.CEDULA
                            ORDER BY C.NOMBRE;
                        ";

            dsdatos = _conexion.Seleccionar(sql);

            // Agregar fila "seleccione"
            DataRow fila = dsdatos.Tables[0].NewRow();
            fila["NOMBRE"] = "seleccione";
            fila["CEDULA"] = "";
            dsdatos.Tables[0].Rows.InsertAt(fila, 0);

            return dsdatos;
        }


        public DataSet ConsultarProducto()
        {
            DataSet dsdatos = new DataSet();

            string sql = "SELECT DISTINCT PRODUCTO FROM FACTURACION ORDER BY PRODUCTO ASC";
            dsdatos = _conexion.Seleccionar(sql);

            DataRow fila = dsdatos.Tables[0].NewRow();
            fila["PRODUCTO"] = "seleccione";
            dsdatos.Tables[0].Rows.InsertAt(fila, 0);

            return dsdatos;
        }

        public DataSet ConsultarFormaPago()
        {
            DataSet dsdatos = new DataSet();

            string sql = "SELECT DISTINCT FORMADEPAGO FROM FACTURACION ORDER BY FORMADEPAGO ASC";
            dsdatos = _conexion.Seleccionar(sql);

            DataRow fila = dsdatos.Tables[0].NewRow();
            fila["FORMADEPAGO"] = "seleccione";
            dsdatos.Tables[0].Rows.InsertAt(fila, 0);

            return dsdatos;
        }

        public DataSet ConsultarPorNumeroFactura(string numeroFactura)
        {
            if (string.IsNullOrWhiteSpace(numeroFactura))
                throw new Exception("Número de factura vacío.");

            string num = numeroFactura.Trim().Replace("'", "''");

            // IMPORTANTE:
            // En tu FACTURACION tienes FECHA, HORA, CLIENTE, PRODUCTO, CANTIDAD, TOTAL, NUMEROFACTURA.
            // Si algún campo no existe, ajustamos.
            string sql = $@"
                SELECT
                    FECHA,
                    HORA,
                    CLIENTE,
                    PRODUCTO,
                    CANTIDAD,
                    TOTAL,
                    NUMEROFACTURA
                FROM
                    FACTURACION
                WHERE
                    Trim(NUMEROFACTURA) = '{num}'
                ORDER BY
                    FECHA DESC, HORA DESC, PRODUCTO ASC
            ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ConsultarFacturaConsumidorFinal(string fecha, string hora, string numeroFactura)
        {
            string sql = @"
        SELECT 
            FECHA,
            FORMADEPAGO,
            PRODUCTO,
            CANTIDAD,
            TOTAL,
            CLIENTE,
            HORA,
            NUMEROFACTURA
        FROM FACTURACION
        WHERE FECHA = '" + fecha + @"'
          AND HORA = '" + hora + @"'
          AND NUMEROFACTURA = '" + numeroFactura + @"'
        ORDER BY PRODUCTO;
    ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ConsultarFacturaNormal(string fecha, string hora, string cedula, string numeroFactura)
        {
            string sql = @"
        SELECT 
            FECHA,
            FORMADEPAGO,
            PRODUCTO,
            CANTIDAD,
            TOTAL,
            CLIENTE,
            HORA,
            NUMEROFACTURA
        FROM FACTURACION
        WHERE FECHA = '" + fecha + @"'
          AND HORA = '" + hora + @"'
          AND CLIENTE = '" + cedula + @"'
          AND NUMEROFACTURA = '" + numeroFactura + @"'
        ORDER BY PRODUCTO;
    ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet ConsultarPendientesPorFecha(string FechaDesde, string FechaHasta)
        {
            DataSet dsdatos = new DataSet();

            string sql = @"
        SELECT 
            F.FECHA,
            F.FORMADEPAGO AS [FORMA DE PAGO],
            SUM(F.CANTIDAD) AS CANTIDADES,
            SUM(F.TOTAL) AS TOTALES,
            C.NOMBRE AS CLIENTE,
            C.CEDULA,  
            F.HORA,
            F.NUMEROFACTURA
        FROM FACTURACION AS F
        LEFT JOIN CLIENTE AS C
            ON F.CLIENTE = C.CEDULA
        WHERE F.FECHA >= '" + FechaDesde + @"'
          AND F.FECHA <= '" + FechaHasta + @"'
          AND F.NUMEROFACTURA LIKE 'PENDIENTE%'
        GROUP BY 
            F.FECHA,
            F.FORMADEPAGO,
            C.NOMBRE,
            C.CEDULA, 
            F.HORA,
            F.NUMEROFACTURA
        ORDER BY
            F.FECHA DESC,
            F.NUMEROFACTURA DESC
    ";

            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }

        public DataSet ConsultarConsumidorFinalPorFecha(string FechaDesde, string FechaHasta)
        {
            DataSet dsdatos = new DataSet();

            string sql = @"
        SELECT 
            F.FECHA,
            F.FORMADEPAGO AS [FORMA DE PAGO],
            SUM(F.CANTIDAD) AS CANTIDADES,
            SUM(F.TOTAL) AS TOTALES,
            C.NOMBRE AS CLIENTE,
            C.CEDULA,  
            F.HORA,
            F.NUMEROFACTURA
        FROM FACTURACION AS F
        LEFT JOIN CLIENTE AS C
            ON F.CLIENTE = C.CEDULA
        WHERE F.FECHA >= '" + FechaDesde + @"'
          AND F.FECHA <= '" + FechaHasta + @"'
          AND (C.NOMBRE = 'CONSUMIDOR FINAL' OR C.NOMBRE = 'FINAL')
        GROUP BY 
            F.FECHA,
            F.FORMADEPAGO,
            C.NOMBRE,
            C.CEDULA, 
            F.HORA,
            F.NUMEROFACTURA
        ORDER BY
            F.FECHA DESC,
            F.NUMEROFACTURA DESC
    ";

            dsdatos = _conexion.Seleccionar(sql);
            return dsdatos;
        }

        public string ObtenerSecuencialError() => ObtenerSecuencialInternoPorPrefijo("PENDIENTE");

        public string ObtenerSecuencialConsumidor() => ObtenerSecuencialInternoPorPrefijo("FINAL");

        private string ObtenerSecuencialInternoPorPrefijo(string prefijo)
        {
            string sql = $"SELECT NUMEROFACTURA FROM FACTURACION WHERE NUMEROFACTURA LIKE '{prefijo}%'";
            DataSet ds = _conexion.Seleccionar(sql);
            int max = 0;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string valor = row["NUMEROFACTURA"].ToString();
                if (!valor.StartsWith(prefijo)) continue;
                string numero = valor.Substring(prefijo.Length).Trim();
                if (int.TryParse(numero, out int num) && num > max) max = num;
            }

            return prefijo + (max + 1).ToString("000");
        }

        //Consulta General Web 
        private string Escapar(string s)
        {
            return (s ?? "").Trim().Replace("'", "''");
        }

        public DataSet ConsultarDocumentos(
            string tipoDocumento,
            string fechaDesde,
            string fechaHasta,
            string cliente = "",
            string producto = "",
            string formaPago = "",
            string numeroFactura = "",
            string numeroRetencion = "",
            string numeroNota = "",
            string sujetoRetenido = "",
            string estado = "TODOS"
        )
        {
            DataSet dsDatos = new DataSet();

            string sql = "";

            string fd = Escapar(fechaDesde);
            string fh = Escapar(fechaHasta);
            string estadoFiltro = (estado ?? "TODOS").Trim().ToUpper();

            string filtroEstadoSql = "";

            if (estadoFiltro == "PENDIENTE")
            {
                filtroEstadoSql = @"
 AND ISNULL(P.ESTADO, '') LIKE 'PENDIENTE%'
 AND ISNULL(P.ESTADO, '') NOT LIKE 'PENDIENTE_AUTORIZACION%'
 AND ISNULL(P.ESTADO, '') NOT LIKE 'PENDIENTE_CORREO%'";
            }
            else if (estadoFiltro == "PENDIENTE_AUTORIZACION")
            {
                filtroEstadoSql = " AND ISNULL(P.ESTADO, '') LIKE 'PENDIENTE_AUTORIZACION%'";
            }
            else if (estadoFiltro == "PENDIENTE_CORREO")
            {
                filtroEstadoSql = " AND ISNULL(P.ESTADO, '') LIKE 'PENDIENTE_CORREO%'";
            }
            else
            {
                filtroEstadoSql = "";
            }

            // =========================================================
            // FACTURAS
            // SIN AGRUPAR
            // =========================================================
            if (tipoDocumento == "FACTURA")
            {
                sql = @"
SELECT 
    TRY_CONVERT(date, F.FECHA, 103) AS FECHA_DATE,
    CONVERT(varchar(10), TRY_CONVERT(date, F.FECHA, 103), 103) AS FECHA,
    F.FORMADEPAGO AS [FORMA DE PAGO],
    F.PRODUCTO,
    ISNULL(TRY_CONVERT(decimal(18,2), REPLACE(F.CANTIDAD, ',', '.')), 0) AS CANTIDAD,
    ISNULL(TRY_CONVERT(decimal(18,2), REPLACE(F.TOTAL, ',', '.')), 0) AS TOTAL,
    C.NOMBRE AS CLIENTE,
    C.CEDULA,
    F.HORA,
    F.NUMEROFACTURA,
    ISNULL(P.ESTADO, '') AS ESTADO,
    CASE
        WHEN UPPER(LTRIM(RTRIM(F.NUMEROFACTURA))) LIKE 'FINAL%' THEN 'PROCESAR'
        WHEN P.ESTADO LIKE 'PENDIENTE_AUTORIZACION%' THEN 'AUTORIZAR'
        WHEN P.ESTADO LIKE 'PENDIENTE_CORREO%' THEN 'ENVIAR_CORREO'
        WHEN P.ESTADO LIKE 'PENDIENTE%' THEN 'PROCESAR'
        ELSE ''
    END AS ACCION,
    P.CLAVEACCESO,
    P.RUTAXMLFIRMADO,
    P.FECHAREGISTRO,
    P.INTENTOS
FROM FACTURACION F
LEFT JOIN CLIENTE C 
    ON F.CLIENTE = C.CEDULA
LEFT JOIN
(
    SELECT FP1.NUMEROFACTURA, FP1.CLAVEACCESO, FP1.RUTAXMLFIRMADO, FP1.FECHAREGISTRO, FP1.INTENTOS, FP1.ESTADO
    FROM FACTURAS_PENDIENTES FP1
    INNER JOIN
    (
        SELECT NUMEROFACTURA, MAX(FECHAREGISTRO) AS FECHAREGISTRO
        FROM FACTURAS_PENDIENTES
        WHERE TIPO = 'FACTURA'
        GROUP BY NUMEROFACTURA
    ) FP2
        ON FP1.NUMEROFACTURA = FP2.NUMEROFACTURA
       AND FP1.FECHAREGISTRO = FP2.FECHAREGISTRO
    WHERE FP1.TIPO = 'FACTURA'
) P
    ON P.NUMEROFACTURA = F.NUMEROFACTURA
WHERE
    TRY_CONVERT(date, F.FECHA, 103) >= '" + fd + @"'
    AND TRY_CONVERT(date, F.FECHA, 103) <= '" + fh + @"'
";

                if (!string.IsNullOrWhiteSpace(producto) && producto != "seleccione")
                    sql += " AND F.PRODUCTO = '" + Escapar(producto) + "'";

                if (!string.IsNullOrWhiteSpace(cliente) && cliente != "seleccione")
                    sql += " AND F.CLIENTE = '" + Escapar(cliente) + "'";

                if (!string.IsNullOrWhiteSpace(formaPago) && formaPago != "seleccione")
                    sql += " AND F.FORMADEPAGO = '" + Escapar(formaPago) + "'";

                if (!string.IsNullOrWhiteSpace(numeroFactura))
                    sql += " AND F.NUMEROFACTURA = '" + Escapar(numeroFactura) + "'";

                sql += @"
" + filtroEstadoSql + @"
ORDER BY 
    TRY_CONVERT(date, F.FECHA, 103) DESC,
    F.NUMEROFACTURA DESC,
    F.HORA DESC";
            }

            // =========================================================
            // RETENCIONES
            // =========================================================
            else if (tipoDocumento == "RETENCION")
            {
                sql = @"
SELECT
    R.NUMERORETENCION,
    R.FECHAEMISION,
    R.NUMEROFACTURA,
    R.FECHAFACTURA,
    R.SUJETORETENIDO,
    R.IDENTIFICACIONSUJETO AS IDENTIFICACION,
    R.TOTALBASEIMPONIBLE,
    R.TOTALRETENCIONRENTA,
    R.TOTALRETENCIONIVA,
    R.TOTALRETENIDO,
    ISNULL(P.ESTADO, '') AS ESTADO,
    CASE
        WHEN P.ESTADO LIKE 'PENDIENTE_AUTORIZACION%' THEN 'AUTORIZAR'
        WHEN P.ESTADO LIKE 'PENDIENTE_CORREO%' THEN 'ENVIAR_CORREO'
        WHEN P.ESTADO LIKE 'PENDIENTE%' THEN 'PROCESAR'
        ELSE ''
    END AS ACCION,
    P.CLAVEACCESO,
    P.RUTAXMLFIRMADO,
    P.FECHAREGISTRO,
    P.INTENTOS
FROM RETENCIONES R
LEFT JOIN
(
    SELECT FP1.NUMEROFACTURA, FP1.CLAVEACCESO, FP1.RUTAXMLFIRMADO, FP1.FECHAREGISTRO, FP1.INTENTOS, FP1.ESTADO
    FROM FACTURAS_PENDIENTES FP1
    INNER JOIN
    (
        SELECT NUMEROFACTURA, MAX(FECHAREGISTRO) AS FECHAREGISTRO
        FROM FACTURAS_PENDIENTES
        WHERE TIPO = 'RETENCION'
        GROUP BY NUMEROFACTURA
    ) FP2
        ON FP1.NUMEROFACTURA = FP2.NUMEROFACTURA
       AND FP1.FECHAREGISTRO = FP2.FECHAREGISTRO
    WHERE FP1.TIPO = 'RETENCION'
) P
    ON P.NUMEROFACTURA = R.NUMERORETENCION
WHERE
    TRY_CONVERT(date, R.FECHAEMISION, 103) >= '" + fd + @"'
    AND TRY_CONVERT(date, R.FECHAEMISION, 103) <= '" + fh + @"'
";

                if (!string.IsNullOrWhiteSpace(numeroRetencion))
                    sql += " AND UPPER(LTRIM(RTRIM(R.NUMERORETENCION))) = '" + Escapar(numeroRetencion.ToUpper()) + "'";

                if (!string.IsNullOrWhiteSpace(numeroFactura))
                    sql += " AND LTRIM(RTRIM(R.NUMEROFACTURA)) = '" + Escapar(numeroFactura) + "'";

                if (!string.IsNullOrWhiteSpace(sujetoRetenido))
                    sql += " AND UPPER(R.SUJETORETENIDO) LIKE '%" + Escapar(sujetoRetenido.ToUpper()) + "%'";

                sql += filtroEstadoSql + @"
ORDER BY 
    TRY_CONVERT(date, R.FECHAEMISION, 103) DESC,
    R.NUMERORETENCION DESC";
            }

            // =========================================================
            // NOTAS DE CREDITO
            // =========================================================
            else if (tipoDocumento == "NOTA_CREDITO")
            {
                sql = @"
SELECT
    NC.NUMERONOTA,
    NC.NUMEROFACTURA,
    NC.FECHAEMISION,
    C.NOMBRE AS CLIENTE,
    C.CEDULA,
    NC.TOTALCONIMPUESTOS AS TOTAL,
    NC.CREDITOUSADO,
    NC.MOTIVO,
    ISNULL(P.ESTADO, '') AS ESTADO,
    CASE
        WHEN P.ESTADO LIKE 'PENDIENTE_AUTORIZACION%' THEN 'AUTORIZAR'
        WHEN P.ESTADO LIKE 'PENDIENTE_CORREO%' THEN 'ENVIAR_CORREO'
        WHEN P.ESTADO LIKE 'PENDIENTE%' THEN 'PROCESAR'
        ELSE ''
    END AS ACCION,
    P.CLAVEACCESO,
    P.RUTAXMLFIRMADO,
    P.FECHAREGISTRO,
    P.INTENTOS
FROM NOTASCREDITO NC
LEFT JOIN CLIENTE C 
    ON C.CEDULA = NC.CLIENTE
LEFT JOIN
(
    SELECT FP1.NUMEROFACTURA, FP1.CLAVEACCESO, FP1.RUTAXMLFIRMADO, FP1.FECHAREGISTRO, FP1.INTENTOS, FP1.ESTADO
    FROM FACTURAS_PENDIENTES FP1
    INNER JOIN
    (
        SELECT NUMEROFACTURA, MAX(FECHAREGISTRO) AS FECHAREGISTRO
        FROM FACTURAS_PENDIENTES
        WHERE TIPO = 'NOTADECREDITO'
        GROUP BY NUMEROFACTURA
    ) FP2
        ON FP1.NUMEROFACTURA = FP2.NUMEROFACTURA
       AND FP1.FECHAREGISTRO = FP2.FECHAREGISTRO
    WHERE FP1.TIPO = 'NOTADECREDITO'
) P
    ON P.NUMEROFACTURA = NC.NUMERONOTA
WHERE
    TRY_CONVERT(date, NC.FECHAEMISION, 103) >= '" + fd + @"'
    AND TRY_CONVERT(date, NC.FECHAEMISION, 103) <= '" + fh + @"'
";

                if (!string.IsNullOrWhiteSpace(cliente))
                    sql += " AND NC.CLIENTE = '" + Escapar(cliente) + "'";

                if (!string.IsNullOrWhiteSpace(numeroNota))
                    sql += " AND UPPER(LTRIM(RTRIM(NC.NUMERONOTA))) = '" + Escapar(numeroNota.ToUpper()) + "'";

                sql += filtroEstadoSql + @"
ORDER BY 
    TRY_CONVERT(date, NC.FECHAEMISION, 103) DESC,
    NC.NUMERONOTA DESC";
            }

            dsDatos = _conexion.Seleccionar(sql);

            return dsDatos;
        }

    }


}
