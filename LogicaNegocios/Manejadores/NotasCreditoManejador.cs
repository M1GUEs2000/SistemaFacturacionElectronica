using AccesoDatos.Abstractions;
using System;
using System.Data;

namespace LogicaNegocios
{
    public class NotasCreditoManejador
    {

        private readonly IConexionBD _conexion;
        private readonly LogManejador _log;

        public NotasCreditoManejador(
            IConexionBD conexion,
            LogManejador log
        )
        {
            _conexion = conexion;
            _log = log;
        }


        // ======================================================
        // INSERTAR
        // ======================================================
        public int Insertar(
            string NumeroNota,
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
            string Motivo,
            string Cliente,
            string TotalSinImpuestos,
            string TotalConImpuestos,
            string CreditoUsado,
            string Usuario,
            string IP
        )
        {
            string sql = @"
                INSERT INTO NOTASCREDITO(
                    NUMERONOTA,
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
                    MOTIVO,
                    CLIENTE,
                    TOTALSINIMPUESTOS,
                    TOTALCONIMPUESTOS,
                    CREDITOUSADO
                ) VALUES (
                    '" + NumeroNota + @"',
                    '" + ClaveAcceso + @"',
                    '" + FechaEmision + @"',
                    '" + HoraEmision + @"',
                    '" + Ambiente + @"',
                    '" + Estado + @"',
                    '" + Codigo + @"',
                    '" + TipoEmision + @"',


                    '" + NumeroFactura + @"',
                    '" + ClaveAccesoFactura + @"',
                    '" + FechaFactura + @"',
                    '" + Motivo + @"',
                    '" + Cliente + @"',
                    '" + TotalSinImpuestos + @"',
                    '" + TotalConImpuestos + @"',
                    '" + CreditoUsado + @"'

                )
            ";

            _log.CrearLog(
                "Se insertó Nota de Crédito: " + NumeroNota + " | Factura: " + NumeroFactura,
                Usuario,
                IP,
                sql
            );


            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ACTUALIZAR (por NUMERONOTA)
        // ======================================================
        public int Actualizar(
            string NumeroNota,
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
            string Motivo,
            string Cliente,
            string TotalSinImpuestos,
            string TotalConImpuestos,
            string Usuario,
            string IP
        )
        {
            string sql = @"
                UPDATE NOTASCREDITO SET
                    CLAVEACCESO = '" + ClaveAcceso + @"',
                    FECHAEMISION = '" + FechaEmision + @"',
                    HORAEMISION = '" + HoraEmision + @"',
                    AMBIENTE = '" + Ambiente + @"',
                    ESTADO = '" + Estado + @"',
                    CODIGO = '" + Codigo + @"',
                    TIPOEMISION = '" + TipoEmision + @"',
                    NUMEROFACTURA = '" + NumeroFactura + @"',
                    CLAVEACCESOFACTURA = '" + ClaveAccesoFactura + @"',
                    FECHAFACTURA = '" + FechaFactura + @"',
                    MOTIVO = '" + Motivo + @"',
                    CLIENTE = '" + Cliente + @"',
                    TOTALSINIMPUESTOS = '" + TotalSinImpuestos + @"',
                    TOTALCONIMPUESTOS = '" + TotalConImpuestos + @"'
                WHERE
                    NUMERONOTA = '" + NumeroNota + @"'
            ";

            _log.CrearLog(
                "Se actualizó Nota de Crédito: " + NumeroNota,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // ELIMINAR (por NUMERONOTA)
        // ======================================================
        public int Eliminar(string NumeroNota, string Usuario, string IP)
        {
            string sql = @"
                DELETE FROM NOTASCREDITO
                WHERE NUMERONOTA = '" + NumeroNota + @"'
            ";

            _log.CrearLog(
                "Se eliminó Nota de Crédito: " + NumeroNota,
                Usuario,
                IP,
                sql
            );

            return _conexion.Ejecutar(sql);
        }

        // ======================================================
        // CONSULTAR POR NUMERONOTA
        // ======================================================
        public DataSet ConsultarPorNumeroNota(string NumeroNota)
        {
            string sql = @"
                SELECT *
                FROM NOTASCREDITO
                WHERE NUMERONOTA = '" + NumeroNota + @"'
            ";

            return _conexion.Seleccionar(sql);
        }

        // ======================================================
        // LISTAR ÚLTIMAS (TOP N)
        // ======================================================
        public DataSet Listar(int Top, string Filtro = "")
        {
            // Access: TOP N
            string where = "";
            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                where = " WHERE " + Filtro;
            }

            string sql = @"
                SELECT TOP " + Top + @" *
                FROM NOTASCREDITO
                " + where + @"
                ORDER BY FECHAEMISION DESC, HORAEMISION DESC
            ";

            return _conexion.Seleccionar(sql);
        }

        // ------------------------------------------------------
        // Helpers
        // ------------------------------------------------------
        private string Escapar(string s)
        {
            return (s ?? "").Trim().Replace("'", "''");
        }

        // Convierte "dd/MM/yyyy" a Date en Access usando DateSerial
        // Evita problemas regionales y el uso de #01/13/2026# etc.
        private string AccessDateFromText(string yyyyMMdd)
        {
            var p = (yyyyMMdd ?? "").Trim().Split('/');
            if (p.Length != 3) return "Date()";

            // yyyy, MM, dd  ← ORDEN CORRECTO
            return $"DateSerial({p[0]},{p[1]},{p[2]})";
        }

        public DataSet ListarFacturasUnicas()
        {
            string sql = @"
            SELECT Q.NUMEROFACTURA_TRIM AS NUMEROFACTURA
            FROM
            (
                SELECT DISTINCT
                    Trim([NUMEROFACTURA]) AS NUMEROFACTURA_TRIM
                FROM FACTURACION
                WHERE
                    [NUMEROFACTURA] IS NOT NULL
                    AND Trim([NUMEROFACTURA]) <> ''
                    AND Left(Trim([NUMEROFACTURA]), 1) BETWEEN '0' AND '9'
                    AND Trim([CLIENTE]) <> '9999999999999'
                    AND NOT EXISTS (
                        SELECT *
                        FROM FACTURAS_PENDIENTES
                        WHERE Trim(FACTURAS_PENDIENTES.NUMEROFACTURA) = Trim(FACTURACION.NUMEROFACTURA)
                          AND Trim(FACTURAS_PENDIENTES.TIPO) = 'FACTURA'
                    )
            ) AS Q
            ORDER BY Q.NUMEROFACTURA_TRIM DESC;
        ";

            return _conexion.Seleccionar(sql);
        }

        public DataSet BuscarFacturasPorNumero(string termino)
        {
            string sql = @"
            SELECT Q.NUMEROFACTURA_TRIM AS NUMEROFACTURA
            FROM
            (
                SELECT DISTINCT
                    Trim([NUMEROFACTURA]) AS NUMEROFACTURA_TRIM
                FROM FACTURACION
                WHERE
                    [NUMEROFACTURA] IS NOT NULL
                    AND Trim([NUMEROFACTURA]) <> ''
                    AND Left(Trim([NUMEROFACTURA]), 1) BETWEEN '0' AND '9'
                    AND Trim([CLIENTE]) <> '9999999999999'
                    AND NOT EXISTS (
                        SELECT *
                        FROM FACTURAS_PENDIENTES
                        WHERE Trim(FACTURAS_PENDIENTES.NUMEROFACTURA) = Trim(FACTURACION.NUMEROFACTURA)
                          AND Trim(FACTURAS_PENDIENTES.TIPO) = 'FACTURA'
                    )
            ) AS Q
            WHERE Q.NUMEROFACTURA_TRIM LIKE '%" + termino + @"%'
            ORDER BY Q.NUMEROFACTURA_TRIM DESC;
        ";

            return _conexion.Seleccionar(sql);
        }


        //---------------------Consultas Tabla-------------------//

        // ------------------------------------------------------
        // 5) FECHAS + NUMERO NOTA (NUEVO)
        // ------------------------------------------------------

        // ------------------------------------------------------
        // 1) SOLO FECHAS
        // ------------------------------------------------------
        public DataSet ConsultarPorFechas(string fechaDesde, string fechaHasta)
        {
            string fd = Escapar(fechaDesde);
            string fh = Escapar(fechaHasta);

            string sql = $@"
                SELECT
                    NC.NUMERONOTA,
                    NC.NUMEROFACTURA,
                    NC.FECHAEMISION,
                    C.NOMBRE AS CLIENTE,
                    C.CEDULA AS CEDULA,
                    NC.TOTALCONIMPUESTOS AS TOTAL,
                    NC.CREDITOUSADO,
                    NC.MOTIVO
                FROM
                    NOTASCREDITO NC
                    LEFT JOIN CLIENTE C ON C.CEDULA = NC.CLIENTE
                WHERE
                    DateValue({AccessDateFromText(fd)}) <= DateValue(CDate(NC.FECHAEMISION))
                    AND DateValue(CDate(NC.FECHAEMISION)) <= DateValue({AccessDateFromText(fh)})
                ORDER BY
                    CDate(NC.FECHAEMISION) DESC, NC.NUMERONOTA DESC;
            ";

            return _conexion.Seleccionar(sql);
        }

        // ------------------------------------------------------
        // 2) FECHAS + CLIENTE
        // ------------------------------------------------------
        public DataSet ConsultarPorCliente(string fechaDesde, string fechaHasta, string cedulaCliente)
        {
            string fd = Escapar(fechaDesde);
            string fh = Escapar(fechaHasta);
            string ced = Escapar(cedulaCliente);

            string sql = $@"
        SELECT
            NC.NUMERONOTA,
            NC.NUMEROFACTURA,
            NC.FECHAEMISION,
            C.NOMBRE AS CLIENTE,
            C.CEDULA AS CEDULA,
            NC.TOTALCONIMPUESTOS AS TOTAL,
            NC.CREDITOUSADO,
            NC.MOTIVO
        FROM
            NOTASCREDITO NC
            LEFT JOIN CLIENTE C ON C.CEDULA = NC.CLIENTE
        WHERE
            NC.CLIENTE = '{ced}'
            AND DateValue({AccessDateFromText(fd)}) <= DateValue(CDate(NC.FECHAEMISION))
            AND DateValue(CDate(NC.FECHAEMISION)) <= DateValue({AccessDateFromText(fh)})
        ORDER BY
            CDate(NC.FECHAEMISION) DESC, NC.NUMERONOTA DESC;
    ";

            return _conexion.Seleccionar(sql);
        }

        // ------------------------------------------------------
        // 3) FECHAS + NUMERONOTA
        // ------------------------------------------------------
        public DataSet ConsultarPorNumeroNotaFechas(string fechaDesde, string fechaHasta, string numeroNota)
        {
            string fd = Escapar(fechaDesde);
            string fh = Escapar(fechaHasta);
            string nn = Escapar(numeroNota).ToUpperInvariant();

            string sql = $@"
        SELECT
            NC.NUMERONOTA,
            NC.NUMEROFACTURA,
            NC.FECHAEMISION,
            C.NOMBRE AS CLIENTE,
            C.CEDULA AS CEDULA,
            NC.TOTALCONIMPUESTOS AS TOTAL,
            NC.CREDITOUSADO,
            NC.MOTIVO
        FROM
            NOTASCREDITO NC
            LEFT JOIN CLIENTE C ON C.CEDULA = NC.CLIENTE
        WHERE
            Trim(UCase(NC.NUMERONOTA)) = '{nn}'
            AND DateValue({AccessDateFromText(fd)}) <= DateValue(CDate(NC.FECHAEMISION))
            AND DateValue(CDate(NC.FECHAEMISION)) <= DateValue({AccessDateFromText(fh)})
        ORDER BY
            CDate(NC.FECHAEMISION) DESC, NC.NUMERONOTA DESC;
    ";

            return _conexion.Seleccionar(sql);
        }

        // ------------------------------------------------------
        // 4) FECHAS + NUMERONOTA + CLIENTE
        // ------------------------------------------------------
        public DataSet ConsultarPorClienteYNumeroNota(
    string fechaDesde,
    string fechaHasta,
    string cedulaCliente,
    string numeroNota)
        {
            string fd = Escapar(fechaDesde);
            string fh = Escapar(fechaHasta);
            string ced = Escapar(cedulaCliente);
            string nn = Escapar(numeroNota).ToUpperInvariant();

            string sql = $@"
        SELECT
            NC.NUMERONOTA,
            NC.NUMEROFACTURA,
            NC.FECHAEMISION,
            C.NOMBRE AS CLIENTE,
            C.CEDULA AS CEDULA,
            NC.TOTALCONIMPUESTOS AS TOTAL,
            NC.CREDITOUSADO,
            NC.MOTIVO
        FROM
            NOTASCREDITO NC
            LEFT JOIN CLIENTE C ON C.CEDULA = NC.CLIENTE
        WHERE
            NC.CLIENTE = '{ced}'
            AND Trim(UCase(NC.NUMERONOTA)) = '{nn}'
            AND DateValue({AccessDateFromText(fd)}) <= DateValue(CDate(NC.FECHAEMISION))
            AND DateValue(CDate(NC.FECHAEMISION)) <= DateValue({AccessDateFromText(fh)})
        ORDER BY
            CDate(NC.FECHAEMISION) DESC,
            NC.NUMERONOTA DESC;
    ";

            return _conexion.Seleccionar(sql);
        }

        // ------------------- DETALLE ------------------------- //

        public int InsertarDetalle(
            string NumeroNota,
            string Producto,
            string Cantidad,
            string Precio,
            string Iva,
            string NumeroFactura
        )
        {
            string productoSeguro = (Producto ?? "").Replace("'", "''");
            string precioSeguro = (Precio ?? "").Replace("'", "''");
            string cantidadSeguro = (Cantidad ?? "").Replace("'", "''");
            string ivaSeguro = (Iva ?? "").Trim().ToUpperInvariant();

            if (ivaSeguro != "SI" && ivaSeguro != "NO")
                ivaSeguro = "NO";

            string sql = @"
        INSERT INTO NOTASCREDITO_DETALLE(
            NUMERONOTA,
            PRODUCTO,
            CANTIDAD,
            PRECIO,
            IVA,
            NUMEROFACTURA
        ) VALUES (
            '" + (NumeroNota ?? "").Trim() + @"',
            '" + productoSeguro + @"',
            '" + cantidadSeguro + @"',
            '" + precioSeguro + @"',
            '" + ivaSeguro + @"',
            '" + (NumeroFactura ?? "").Trim() + @"'
        )
    ";

            return _conexion.Ejecutar(sql);
        }

        public DataSet ConsultarDetallePorNumeroNota(string NumeroNota)
        {
            string sql = @"
        SELECT
            PRODUCTO,
            CANTIDAD,
            PRECIO,
            IVA
        FROM NOTASCREDITO_DETALLE
        WHERE NUMERONOTA = '" + (NumeroNota ?? "").Trim() + @"'
        ORDER BY PRODUCTO
    ";

            return _conexion.Seleccionar(sql);
        }

        public void ActualizarNumeroNota_EncabezadoYDetalle(string vieja, string nueva)
        {
            string fecha = DateTime.Now.ToString("dd/MM/yyyy");
            string hora = DateTime.Now.ToString("HH:mm:ss");

            // ==================================================
            // 1) ACTUALIZAR ENCABEZADO (RENOMBRAR + ESTADO + FECHA/HORA)
            // ==================================================
            string sql1 = @"
        UPDATE NOTASCREDITO
        SET 
            NUMERONOTA   = '" + (nueva ?? "").Trim() + @"',
            ESTADO       = 'AUTORIZADO',
            FECHAEMISION = '" + fecha + @"',
            HORAEMISION  = '" + hora + @"'
        WHERE NUMERONOTA = '" + (vieja ?? "").Trim() + @"'
    ";

            _conexion.Ejecutar(sql1);

            // ==================================================
            // 2) ACTUALIZAR DETALLE (SOLO RENOMBRAR CLAVE)
            // ==================================================
            string sql2 = @"
        UPDATE NOTASCREDITO_DETALLE
        SET NUMERONOTA = '" + (nueva ?? "").Trim() + @"'
        WHERE NUMERONOTA = '" + (vieja ?? "").Trim() + @"'
    ";

            _conexion.Ejecutar(sql2);
        }

        public int EliminarDetallePorNumeroNota(string NumeroNota)
        {
            string sql = @"
        DELETE FROM NOTASCREDITO_DETALLE
        WHERE NUMERONOTA = '" + (NumeroNota ?? "").Trim() + @"'
    ";
            return _conexion.Ejecutar(sql);
        }

    }
}
