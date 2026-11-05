using AccesoDatos.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

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
                    AND NOT EXISTS (
                        SELECT *
                        FROM FACTURAS_PENDIENTES
                        WHERE Trim(FACTURAS_PENDIENTES.NUMEROFACTURA) = Trim(FACTURACION.NUMEROFACTURA)
                          AND Trim(FACTURAS_PENDIENTES.TIPO) = 'FACTURA'
                    )
            ) AS Q
            ORDER BY Q.NUMEROFACTURA_TRIM DESC;
        ";

            return QuitarFacturasSinCredito(_conexion.Seleccionar(sql));
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

            return MarcarFacturasSinCredito(_conexion.Seleccionar(sql));
        }

        // ======================================================
        // CRÉDITO DISPONIBLE POR FACTURA
        // ======================================================

        /// <summary>
        /// Facturas que ya no admiten más notas de crédito porque todo lo que
        /// facturaron ya fue acreditado. Una factura de la que se emitió una nota
        /// por el total no vuelve a ofrecerse: no queda crédito que dar.
        ///
        /// La comparación es por CANTIDAD de cada producto, no por monto. Los
        /// totales de NOTASCREDITO son texto, y con ICE la base imponible de la
        /// nota no coincide con el TOTAL de la factura, así que restar montos
        /// daría un saldo engañoso. El formulario mueve líneas completas al grid
        /// de la nota, de modo que las cantidades describen exactamente lo
        /// acreditado.
        ///
        /// Cuentan todas las notas MENOS las NO_AUTORIZADO: una nota rechazada por
        /// el SRI no consume crédito, pero una EMITIENDO o PENDIENTE_AUTORIZACION
        /// sí lo reserva, para no acreditar dos veces lo mismo mientras el SRI
        /// responde.
        /// </summary>
        public HashSet<string> ObtenerFacturasSinCreditoDisponible()
        {
            var agotadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string sqlAcreditado = @"
            SELECT
                Trim(D.NUMEROFACTURA) AS NUMFAC,
                Trim(D.PRODUCTO)      AS PROD,
                D.CANTIDAD            AS CANT
            FROM NOTASCREDITO_DETALLE AS D
                INNER JOIN NOTASCREDITO AS N
                    ON Trim(D.NUMERONOTA) = Trim(N.NUMERONOTA)
            WHERE
                (N.ESTADO IS NULL OR Trim(UCase(N.ESTADO)) <> 'NO_AUTORIZADO')
                AND D.NUMEROFACTURA IS NOT NULL
                AND Trim(D.NUMEROFACTURA) <> ''
        ";

            var acreditado = AgruparCantidades(_conexion.Seleccionar(sqlAcreditado));

            // Sin notas emitidas todas las facturas conservan su crédito completo.
            if (acreditado.Count == 0)
                return agotadas;

            // Solo se traen las facturas que ya tienen alguna nota; del resto se
            // sabe de antemano que no están agotadas.
            string sqlFacturado = @"
            SELECT
                Trim(NUMEROFACTURA) AS NUMFAC,
                Trim(PRODUCTO)      AS PROD,
                CANTIDAD            AS CANT
            FROM FACTURACION
            WHERE
                NUMEROFACTURA IS NOT NULL
                AND Trim(NUMEROFACTURA) <> ''
                AND EXISTS (
                    SELECT *
                    FROM NOTASCREDITO_DETALLE
                    WHERE Trim(NOTASCREDITO_DETALLE.NUMEROFACTURA) = Trim(FACTURACION.NUMEROFACTURA)
                )
        ";

            var facturado = AgruparCantidades(_conexion.Seleccionar(sqlFacturado));

            foreach (var factura in facturado)
            {
                Dictionary<string, decimal> devueltoPorProducto;

                if (!acreditado.TryGetValue(factura.Key, out devueltoPorProducto))
                    continue;

                bool quedaCredito = false;

                foreach (var producto in factura.Value)
                {
                    decimal devuelto;
                    devueltoPorProducto.TryGetValue(producto.Key, out devuelto);

                    // Tolerancia mínima: las cantidades de la nota viajan como texto
                    // y no deben dejar viva una factura por una milésima de resto.
                    if (producto.Value - devuelto > 0.001m)
                    {
                        quedaCredito = true;
                        break;
                    }
                }

                if (!quedaCredito)
                    agotadas.Add(factura.Key);
            }

            return agotadas;
        }

        /// <summary>
        /// Cantidad ya acreditada por producto en una factura, sumando todas sus
        /// notas y con el mismo criterio de estados que
        /// <see cref="ObtenerFacturasSinCreditoDisponible"/>.
        ///
        /// Es lo que hay que descontar del detalle de la factura antes de ofrecerlo
        /// en una nota nueva: un producto ya devuelto no puede volver a devolverse.
        /// </summary>
        public Dictionary<string, decimal> ObtenerCantidadesAcreditadas(string NumeroFactura)
        {
            var porProducto = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            string numeroFactura = (NumeroFactura ?? "").Trim();
            if (numeroFactura.Length == 0)
                return porProducto;

            string sql = @"
            SELECT
                Trim(D.NUMEROFACTURA) AS NUMFAC,
                Trim(D.PRODUCTO)      AS PROD,
                D.CANTIDAD            AS CANT
            FROM NOTASCREDITO_DETALLE AS D
                INNER JOIN NOTASCREDITO AS N
                    ON Trim(D.NUMERONOTA) = Trim(N.NUMERONOTA)
            WHERE
                (N.ESTADO IS NULL OR Trim(UCase(N.ESTADO)) <> 'NO_AUTORIZADO')
                AND Trim(D.NUMEROFACTURA) = @numfac
        ";

            var agrupado = AgruparCantidades(_conexion.Seleccionar(sql, ("numfac", numeroFactura)));

            Dictionary<string, decimal> acreditado;

            return agrupado.TryGetValue(numeroFactura, out acreditado)
                ? acreditado
                : porProducto;
        }

        /// <summary>Columna que marca, en un listado de facturas, cuáles ya agotaron su cupo.</summary>
        public const string ColumnaSinCredito = "SINCREDITO";

        /// <summary>
        /// Marca cada fila con <see cref="ColumnaSinCredito"/> según haya quedado o
        /// no crédito por dar.
        ///
        /// A diferencia de <see cref="QuitarFacturasSinCredito"/>, las agotadas NO se
        /// quitan: el formulario las muestra en gris y sin poder elegirse, para que el
        /// cajero vea que la factura existe y por qué no sirve. Si desaparecieran sin
        /// más, el cajero no sabría si ya la usó o si escribió mal el número.
        /// </summary>
        private DataSet MarcarFacturasSinCredito(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0)
                return ds;

            DataTable tabla = ds.Tables[0];

            if (!tabla.Columns.Contains(ColumnaSinCredito))
                tabla.Columns.Add(ColumnaSinCredito, typeof(bool));

            if (tabla.Rows.Count == 0)
                return ds;

            HashSet<string> agotadas = ObtenerFacturasSinCreditoDisponible();

            foreach (DataRow row in tabla.Rows)
            {
                string numeroFactura = (row["NUMEROFACTURA"] ?? "").ToString().Trim();
                row[ColumnaSinCredito] = agotadas.Contains(numeroFactura);
            }

            tabla.AcceptChanges();

            return ds;
        }

        /// <summary>
        /// Saca del resultado las facturas ya acreditadas por completo.
        ///
        /// El filtro se aplica en memoria y no dentro del SQL porque las cantidades
        /// de NOTASCREDITO_DETALLE son texto: compararlas en Access obligaría a un
        /// CDbl, que interpreta el separador decimal según la configuración regional
        /// de la máquina y convertiría "1.5" en 15 en un Windows en español.
        /// </summary>
        private DataSet QuitarFacturasSinCredito(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return ds;

            HashSet<string> agotadas = ObtenerFacturasSinCreditoDisponible();
            if (agotadas.Count == 0)
                return ds;

            DataTable tabla = ds.Tables[0];

            for (int i = tabla.Rows.Count - 1; i >= 0; i--)
            {
                string numeroFactura = (tabla.Rows[i]["NUMEROFACTURA"] ?? "").ToString().Trim();

                if (agotadas.Contains(numeroFactura))
                    tabla.Rows.RemoveAt(i);
            }

            tabla.AcceptChanges();

            return ds;
        }

        /// <summary>Cantidades sumadas por factura y producto, a partir de NUMFAC / PROD / CANT.</summary>
        private static Dictionary<string, Dictionary<string, decimal>> AgruparCantidades(DataSet ds)
        {
            var porFactura = new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);

            if (ds == null || ds.Tables.Count == 0)
                return porFactura;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string numeroFactura = (row["NUMFAC"] ?? "").ToString().Trim();
                string producto = (row["PROD"] ?? "").ToString().Trim();

                if (numeroFactura.Length == 0 || producto.Length == 0)
                    continue;

                Dictionary<string, decimal> porProducto;

                if (!porFactura.TryGetValue(numeroFactura, out porProducto))
                {
                    porProducto = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    porFactura[numeroFactura] = porProducto;
                }

                decimal acumulado;
                porProducto.TryGetValue(producto, out acumulado);

                porProducto[producto] = acumulado + LeerCantidad(row["CANT"]);
            }

            return porFactura;
        }

        /// <summary>
        /// Cantidad que llega numérica desde FACTURACION o como texto desde
        /// NOTASCREDITO_DETALLE. El texto se escribió con la cultura de la máquina,
        /// así que se prueban las dos convenciones decimales antes de rendirse.
        /// NumberStyles.Float y no Any: Any admite separador de miles y convertiría
        /// "1,5" en 15 en lugar de fallar y dejar que lo resuelva el segundo intento.
        /// </summary>
        private static decimal LeerCantidad(object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return 0m;

            if (valor is decimal) return (decimal)valor;
            if (valor is int) return (int)valor;
            if (valor is short) return (short)valor;
            if (valor is long) return (long)valor;
            if (valor is double) return (decimal)(double)valor;

            string texto = valor.ToString().Trim();
            if (texto.Length == 0)
                return 0m;

            decimal cantidad;

            if (decimal.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out cantidad))
                return cantidad;

            if (decimal.TryParse(texto.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out cantidad))
                return cantidad;

            return 0m;
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

        public void ActualizarNumeroYEstado(
            string numeroActual,
            string numeroNuevo,
            string estado,
            string usuario,
            string ip)
        {
            string sqlEncabezado = @"
        UPDATE NOTASCREDITO SET
            NUMERONOTA = @numeroNuevo,
            ESTADO = @estado
        WHERE NUMERONOTA = @numeroActual";

            _log.CrearLog(
                "Nota de crédito renumerada " + numeroActual + " -> " +
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
        UPDATE NOTASCREDITO_DETALLE SET
            NUMERONOTA = @numeroNuevo
        WHERE NUMERONOTA = @numeroActual";

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
                "SELECT NUMERONOTA FROM NOTASCREDITO WHERE NUMERONOTA LIKE 'EMITIENDO%'");
            int max = 0;

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string valor = row["NUMERONOTA"].ToString().Trim();
                    if (!valor.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
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
