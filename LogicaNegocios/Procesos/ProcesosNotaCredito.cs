using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using xmlNotaCredito;

namespace LogicaNegocios.Procesos
{
    public class ResultadoNotaCredito
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public string NumeroNota { get; set; }
        public string NumeroFactura { get; set; }
        public string Motivo { get; set; }
        public string CedulaCliente { get; set; }
        public string FechaFactura { get; set; }
        public string TotalSinImpuestos { get; set; }
        public string TotalConImpuestos { get; set; }
        public DataTable TB_DetalleNC { get; set; }
    }

    public class ResultadoFinalNotaCredito
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public string NumeroNota { get; set; }
        public string NumeroFactura { get; set; }

        public string ClaveAcceso { get; set; }
        public string RutaXmlGenerado { get; set; }
        public string RutaXmlFirmado { get; set; }
        public string RutaXmlAutorizado { get; set; }
        public string RutaPdf { get; set; }

        public bool EnvioCorreoExitoso { get; set; }
        public bool SecuencialRepetido { get; set; }
        public bool Autorizado { get; set; }
    }


    public class ProcesosNotaCredito
    {
        private readonly AppServices _services;

        public ProcesosNotaCredito(AppServices services
        )
        {
            _services = services;
        }

        public ResultadoNotaCredito PrepararNotaCreditoDesdeFactura(
            string numeroFactura,
            string motivo,
            DataTable productosSeleccionados,
            string usuario,
            string ip,
            string numeroNotaForzada = null   // 👈 PENDIENTE###
        )
        {
            var r = new ResultadoNotaCredito();

            try
            {
                if (string.IsNullOrWhiteSpace(numeroFactura))
                    return ErrorNC(r, "Número de factura vacío.");

                if (string.IsNullOrWhiteSpace(motivo))
                    return ErrorNC(r, "Debe ingresar el motivo de la Nota de Crédito.");

                // ==================================================
                // 1) Traer encabezado + detalle completo de FACTURACION
                // ==================================================
                string sql = @"
            SELECT *
            FROM FACTURACION
            WHERE NUMEROFACTURA = '" + numeroFactura.Trim() + @"'
        ";


                DataSet dsFac = _services.Conexion.Seleccionar(sql);
                if (dsFac == null || dsFac.Tables.Count == 0 || dsFac.Tables[0].Rows.Count == 0)
                    return ErrorNC(r, "No se encontró la factura en FACTURACION: " + numeroFactura);

                DataRow first = dsFac.Tables[0].Rows[0];

                string fechaFactura = first["FECHA"].ToString().Trim();
                string cedulaCliente = first["CLIENTE"].ToString().Trim();

                if (string.IsNullOrWhiteSpace(cedulaCliente))
                    return ErrorNC(r, "La factura no tiene CLIENTE (cédula) en FACTURACION.");

                decimal tarifaBase = _services.TarifaIva;

                // ==================================================
                // 3) Preparar TB_NC (detalle a usar en XML y a devolver)
                // ==================================================
                DataTable TB_NC = new DataTable();
                TB_NC.Columns.Add("CANTIDAD", typeof(decimal));
                TB_NC.Columns.Add("PRODUCTO", typeof(string));
                TB_NC.Columns.Add("VALOR", typeof(decimal));   // unitario (final)
                TB_NC.Columns.Add("TOTAL", typeof(decimal));   // total línea
                TB_NC.Columns.Add("CODIGO", typeof(string));   // código producto
                TB_NC.Columns.Add("IVA", typeof(string));   // código producto

                // ==================================================
                // 4) Fuente de cálculo:
                //    - Si hay productosSeleccionados => SOLO esos
                //    - Si no => toda la factura (dsFac)
                // ==================================================
                bool usarSeleccion = (productosSeleccionados != null && productosSeleccionados.Rows.Count > 0);

                decimal acumuladoSinImp = 0m;
                decimal acumuladoIVA = 0m;

                if (usarSeleccion)
                {
                    // Validar columnas mínimas
                    string[] requiredCols = { "PRODUCTO", "CANTIDAD", "VALOR", "TOTAL" };
                    foreach (string col in requiredCols)
                    {
                        if (!productosSeleccionados.Columns.Contains(col))
                            return ErrorNC(r, "productosSeleccionados no contiene la columna requerida: " + col);
                    }

                    foreach (DataRow row in productosSeleccionados.Rows)
                    {
                        string producto = (row["PRODUCTO"]?.ToString() ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(producto)) continue;

                        decimal cantidad = Convert.ToDecimal(row["CANTIDAD"]);
                        if (cantidad <= 0m) cantidad = 1m;

                        decimal totalLinea = Math.Round(Convert.ToDecimal(row["TOTAL"]), 2);
                        decimal precioUnitarioFinal = Math.Round(Convert.ToDecimal(row["VALOR"]), 2);

                        // CODIGO: usar el que venga; si viene vacío, consultar
                        string codigoProd = "";
                        if (productosSeleccionados.Columns.Contains("CODIGO"))
                            codigoProd = (row["CODIGO"]?.ToString() ?? "").Trim();

                        // IVA: si viene en selección lo usamos; si no, consultamos BD
                        string ivaProducto = "";
                        if (productosSeleccionados.Columns.Contains("IVA"))
                            ivaProducto = (row["IVA"]?.ToString() ?? "").Trim().ToUpperInvariant();

                        DataSet dsProdSel = _services.Producto.ConsultaNombre(producto);
                        if (dsProdSel == null || dsProdSel.Tables.Count == 0 || dsProdSel.Tables[0].Rows.Count == 0)
                            return ErrorNC(r, "Producto no existe en BD: " + producto);

                        DataRow prodRowSel = dsProdSel.Tables[0].Rows[0];

                        if (string.IsNullOrWhiteSpace(codigoProd))
                            codigoProd = prodRowSel["CODIGO"].ToString().Trim();

                        if (string.IsNullOrWhiteSpace(ivaProducto))
                            ivaProducto = prodRowSel["IVA"].ToString().Trim().ToUpperInvariant();

                        if (ivaProducto != "SI" && ivaProducto != "NO")
                            ivaProducto = "NO";

                        decimal tarifaLinea = (ivaProducto == "SI") ? tarifaBase : 0m;

                        var iceSel = HelperIva.CalcularIceLinea(prodRowSel, cantidad, totalLinea);
                        var calculoLinea = HelperIva.CalcularBaseEIvaLinea(totalLinea, tarifaLinea, iceSel.Valor);
                        decimal baseLinea = calculoLinea.Base;
                        decimal ivaLinea  = calculoLinea.Iva;

                        acumuladoSinImp += baseLinea;
                        acumuladoIVA += ivaLinea;

                        TB_NC.Rows.Add(cantidad, producto, precioUnitarioFinal, totalLinea, codigoProd, ivaProducto);
                    }
                }
                else
                {
                    foreach (DataRow row in dsFac.Tables[0].Rows)
                    {
                        string producto = row["PRODUCTO"].ToString().Trim();

                        decimal cantidad = Convert.ToDecimal(row["CANTIDAD"]);
                        if (cantidad <= 0m) cantidad = 1m;

                        decimal totalLinea = Math.Round(Convert.ToDecimal(row["TOTAL"]), 2);

                        DataSet dsProd = _services.Producto.ConsultaNombre(producto);
                        if (dsProd == null || dsProd.Tables.Count == 0 || dsProd.Tables[0].Rows.Count == 0)
                            return ErrorNC(r, "Producto no existe en BD: " + producto);

                        string codigoProd = dsProd.Tables[0].Rows[0]["CODIGO"].ToString().Trim();
                        string ivaProducto = dsProd.Tables[0].Rows[0]["IVA"].ToString().Trim().ToUpperInvariant();
                        if (ivaProducto != "SI" && ivaProducto != "NO") ivaProducto = "NO";

                        decimal precioUnitarioFinal = Math.Round(totalLinea / cantidad, 2);

                        decimal tarifaLinea = (ivaProducto == "SI") ? tarifaBase : 0m;

                        var iceNC = HelperIva.CalcularIceLinea(dsProd.Tables[0].Rows[0], cantidad, totalLinea);
                        var calculoLinea = HelperIva.CalcularBaseEIvaLinea(totalLinea, tarifaLinea, iceNC.Valor);
                        decimal baseLinea = calculoLinea.Base;
                        decimal ivaLinea  = calculoLinea.Iva;

                        acumuladoSinImp += baseLinea;
                        acumuladoIVA += ivaLinea;

                        TB_NC.Rows.Add(cantidad, producto, precioUnitarioFinal, totalLinea, codigoProd, ivaProducto);
                    }
                }

                acumuladoSinImp = Math.Round(acumuladoSinImp, 2);
                acumuladoIVA = Math.Round(acumuladoIVA, 2);
                decimal totalConImp = Math.Round(acumuladoSinImp + acumuladoIVA, 2);

                string totalSinImpuestosStr = acumuladoSinImp.ToString("0.00", CultureInfo.InvariantCulture);
                string totalConImpuestosStr = totalConImp.ToString("0.00", CultureInfo.InvariantCulture);

                // ==================================================
                // 5) Generar NUMERONOTA (secuencial tipo 04) / FORZADO
                // ==================================================
                string numeroNota = !string.IsNullOrWhiteSpace(numeroNotaForzada)
                    ? numeroNotaForzada
                    : _services.Param.ObtenerSecuencialFormateado("04");

                // ==================================================
                // RESPUESTA
                // ==================================================
                r.Exito = true;
                r.Mensaje = "Nota de Crédito preparada correctamente.";
                r.NumeroNota = numeroNota;
                r.NumeroFactura = numeroFactura;
                r.Motivo = motivo;
                r.CedulaCliente = cedulaCliente;
                r.FechaFactura = fechaFactura;
                r.TotalSinImpuestos = totalSinImpuestosStr;
                r.TotalConImpuestos = totalConImpuestosStr;
                r.TB_DetalleNC = TB_NC;

                return r;
            }
            catch (Exception ex)
            {
                return ErrorNC(r, "Error preparando Nota de Crédito: " + ex.Message);
            }
        }

        public bool InsertarNotaCredito(
            ResultadoNotaCredito prep,
            DataRow rowPfm,
            string claveAcceso,
            string numeroNota,
            string estado,
            string usuario,
            string ip
        )
        {
            try
            {
                int ok = _services.NotaCredito.Insertar(
                    NumeroNota: numeroNota,
                    ClaveAcceso: claveAcceso ?? "",
                    FechaEmision: DateTime.Now.ToString("dd/MM/yyyy"),
                    HoraEmision: DateTime.Now.ToString("HH:mm:ss"),
                    Ambiente: rowPfm["AMBIENTE"].ToString(),
                    Estado: estado,
                    Codigo: "04",
                    TipoEmision: rowPfm["TIPOEMISION"].ToString(),
                    NumeroFactura: prep.NumeroFactura,
                    ClaveAccesoFactura: "",
                    FechaFactura: prep.FechaFactura,
                    Motivo: prep.Motivo,
                    Cliente: prep.CedulaCliente,
                    TotalSinImpuestos: prep.TotalSinImpuestos,
                    TotalConImpuestos: prep.TotalConImpuestos,
                    "0",
                    Usuario: usuario,
                    IP: ip
                );

                if (ok <= 0)
                    return false;

                foreach (DataRow d in prep.TB_DetalleNC.Rows)
                {
                    _services.NotaCredito.InsertarDetalle(
                        numeroNota,                          // NUMERONOTA
                        d["PRODUCTO"].ToString(),            // PRODUCTO
                        d["CANTIDAD"].ToString(),            // CANTIDAD
                        d["VALOR"].ToString(),               // PRECIO
                        d["IVA"].ToString(),                 // IVA ✅
                        prep.NumeroFactura                   // NUMEROFACTURA ✅
                    );
                }


                return true;
            }
            catch
            {
                return false;
            }
        }


        // ======================================================
        // PASO 2: GENERAR XML NOTA DE CRÉDITO (SIN FIRMAR)
        // ======================================================
        public bool GenerarXmlNotaCredito(
            string numeroNota,                 // 001-002-000000759
            string numeroFactura,              // 001-002-000000759 (factura afectada)
            string fechaFactura,               // dd/MM/yyyy (de FACTURACION.FECHA) - puede venir también yyyy/MM/dd desde BD
            string motivo,                     // motivo NC
            DataTable TB_NC,                   // detalle: CANTIDAD, PRODUCTO, VALOR, TOTAL, CODIGO
            DataRow rowCliente,                // CLIENTE
            DataRow rowEmpresa,                // EMPRESA
            DataRow rowPfm,                    // PARAMETROS FACTURAS (SRI)
            DataRow rowPmNC,                   // PARAMETROS tipo 04 (secuencial/cod numérico si lo tienes ahí)
            string usuario,
            string ip,
            out string claveAcceso,
            out string rutaXmlGenerado,
            out string error
        )
        {
            claveAcceso = "";
            rutaXmlGenerado = "";
            error = "";

            try
            {
                if (string.IsNullOrWhiteSpace(numeroNota))
                {
                    error = "Número de Nota de Crédito vacío.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    error = "Número de factura (documento modificado) vacío.";
                    return false;
                }

                if (TB_NC == null || TB_NC.Rows.Count == 0)
                {
                    error = "No hay detalle para generar Nota de Crédito.";
                    return false;
                }

                // ==========================================
                // 1) Parsear estab/ptoEmi/secuencial desde numeroNota
                // ==========================================
                string estab = "001";
                string ptoEmi = "001";
                string secuencial = "000000001";

                string[] partes = numeroNota.Split('-');
                if (partes.Length == 3)
                {
                    estab = partes[0].Trim();
                    ptoEmi = partes[1].Trim();
                    secuencial = partes[2].Trim();
                }
                else
                {
                    estab = rowPfm["ESTAB"].ToString().Trim();
                    ptoEmi = rowPfm["PUNTOEMISION"].ToString().Trim();
                    secuencial = numeroNota.Trim();
                }

                // Normalizar secuencial a 9 dígitos
                if (!string.IsNullOrWhiteSpace(secuencial))
                    secuencial = secuencial.PadLeft(9, '0');

                // ==========================================
                // 2) Tarifario base por CODIGOPORCENTAJE (misma lógica factura)
                // ==========================================
                string codigoImpuesto = rowPfm["CODIGOIMPUESTO"].ToString().Trim();      // típicamente "2"
                string codigoPorcentajeBase = rowPfm["CODIGOPORCENTAJE"].ToString().Trim();

                decimal tarifaBase = _services.TarifaIva;

                // ==========================================
                // 3) Calcular totales (base + iva) por línea y agrupar impuestos
                //    (CIERRA EXACTO CON EL TOTAL REAL DE LA LÍNEA)
                // ==========================================
                decimal totalSinImpuestos = 0m;
                decimal totalIva = 0m;
                decimal totalICE = 0m;

                // clave: codigoPorcentaje | tarifa% (para agrupar IVA)
                var impuestosAgrupados = new Dictionary<string, (string codPorc, decimal tarifaPct, decimal baseImp, decimal valor)>();
                // clave: ICE_CODIGO (para agrupar ICE)
                var iceAgrupado = new Dictionary<string, (decimal baseImp, decimal valor, decimal tarifa)>();

                foreach (DataRow r in TB_NC.Rows)
                {
                    string producto = r["PRODUCTO"].ToString().Trim();

                    decimal cantidad = Convert.ToDecimal(r["CANTIDAD"]);
                    if (cantidad <= 0m) cantidad = 1m;

                    decimal totalLinea = Math.Round(Convert.ToDecimal(r["TOTAL"]), 2);

                    DataSet dsProd = _services.Producto.ConsultaNombre(producto);
                    if (dsProd == null || dsProd.Tables.Count == 0 || dsProd.Tables[0].Rows.Count == 0)
                        throw new Exception("Producto no existe en BD: " + producto);

                    DataRow prodRow = dsProd.Tables[0].Rows[0];
                    string ivaProducto = prodRow["IVA"].ToString().Trim().ToUpperInvariant();

                    decimal tarifaLinea = (ivaProducto == "SI") ? tarifaBase : 0m;
                    string codPorcentajeLinea = (tarifaLinea == 0m) ? "0" : codigoPorcentajeBase;

                    decimal baseLinea = totalLinea;

                    // ICE
                    var ice = HelperIva.CalcularIceLinea(prodRow, cantidad, baseLinea);
                    if (ice.Aplica)
                    {
                        totalICE += ice.Valor;
                        string iceKey = ice.Codigo;
                        if (!iceAgrupado.ContainsKey(iceKey))
                            iceAgrupado[iceKey] = (0m, 0m, ice.Tarifa);
                        var iceEntry = iceAgrupado[iceKey];
                        iceEntry.baseImp += ice.BaseImponible;
                        iceEntry.valor   += ice.Valor;
                        iceAgrupado[iceKey] = iceEntry;
                    }

                    // IVA — base incluye ICE (norma SRI)
                    decimal baseIVALinea = baseLinea + ice.Valor;
                    decimal ivaLinea = tarifaLinea > 0m ? Math.Round(baseIVALinea * tarifaLinea, 2) : 0m;

                    totalSinImpuestos += baseLinea;
                    totalIva += ivaLinea;

                    string key = codPorcentajeLinea + "|" + (tarifaLinea * 100m).ToString("0.00", CultureInfo.InvariantCulture);
                    if (!impuestosAgrupados.ContainsKey(key))
                        impuestosAgrupados[key] = (codPorcentajeLinea, tarifaLinea * 100m, 0m, 0m);
                    var cur = impuestosAgrupados[key];
                    cur.baseImp += baseIVALinea;
                    cur.valor   += ivaLinea;
                    impuestosAgrupados[key] = cur;
                }

                decimal importeTotal = Math.Round(totalSinImpuestos + totalICE + totalIva, 2);
                totalSinImpuestos = Math.Round(totalSinImpuestos, 2);
                totalIva = Math.Round(totalIva, 2);
                totalICE = Math.Round(totalICE, 2);


                // ==========================================
                // 4) Generar clave de acceso (igual que factura) — codDoc NC = "04"
                // ==========================================
                string codDoc = "04";
                string ruc = rowEmpresa["RUC"].ToString().Trim();
                string ambiente = rowPfm["AMBIENTE"].ToString().Trim();
                string tipoEmision = rowPfm["TIPOEMISION"].ToString().Trim();

                // Código numérico: si en PARAMETROS tipo 04 lo tienes, úsalo; caso contrario fijo.
                string codigoNumerico = "12345678";
                if (rowPmNC != null && rowPmNC.Table != null && rowPmNC.Table.Columns.Contains("CODIGONUMERICO"))
                {
                    string tmp = rowPmNC["CODIGONUMERICO"].ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(tmp)) codigoNumerico = tmp;
                }

                string fechaEmisionNC = DateTime.Now.ToString("dd/MM/yyyy");
                string fechaClave = DateTime.Now.ToString("ddMMyyyy");

                CFuncionesComprobantesElectronicos cfunciones = new CFuncionesComprobantesElectronicos();

                string ClaveAccesoGenerada = cfunciones.ClaveAcceso(
                    fechaClave,
                    codDoc,
                    ruc,
                    ambiente,
                    estab,
                    ptoEmi,
                    secuencial,
                    codigoNumerico,
                    tipoEmision
                );

                // ==========================================
                // 5) Armar XML Nota de Crédito
                //    Cambios clave:
                //    - fechaEmisionDocSustento en dd/MM/yyyy
                //    - numDocModificado idealmente en formato 001-002-000000755
                // ==========================================
                string nombreComercial =
                    rowEmpresa.Table.Columns.Contains("NOMBRE") ? rowEmpresa["NOMBRE"].ToString().Trim() : "";

                string dirMatriz =
                    rowEmpresa.Table.Columns.Contains("DIRECCION") ? rowEmpresa["DIRECCION"].ToString().Trim() : "";

                string obligadoContab = rowPfm["OBLIGADOCONTABILIDAD"].ToString().Trim();
                string contribRimpe = rowPfm["CONTRIBUYENTERIMPE"].ToString().Trim();

                // Cliente (en NC NO mandamos direccionComprador en infoNotaCredito)
                string moneda = rowPfm["MONEDA"].ToString().Trim();

                string razonSocialComprador = rowCliente["NOMBRE"].ToString().Trim();
                string identificacionComprador = rowCliente["CEDULA"].ToString().Trim();

                string tipoIdComprador =
                    identificacionComprador.Length == 13 ? "04" :
                    identificacionComprador.Length == 10 ? "05" :
                    "08";

                // Formatos numDocModificado y fecha sustento
                string codDocModificado = "01";

                string numDocModificado = (numeroFactura ?? "").Trim();
                if (!numDocModificado.Contains("-"))
                {
                    // si te llega solo secuencial, completa con estab-ptoEmi
                    numDocModificado = $"{estab}-{ptoEmi}-{numDocModificado.PadLeft(9, '0')}";
                }

                string fechaDocSustento = (fechaFactura ?? "").Trim();
                DateTime dtSustento;

                if (DateTime.TryParseExact(
                        fechaDocSustento,
                        new[] { "dd/MM/yyyy", "yyyy/MM/dd", "yyyy-MM-dd", "dd-MM-yyyy" },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dtSustento))
                {
                    fechaDocSustento = dtSustento.ToString("dd/MM/yyyy");
                }
                else
                {
                    throw new Exception("Fecha factura inválida para SRI: " + fechaFactura);
                }

                string totalSinImpStr = totalSinImpuestos.ToString("0.00", CultureInfo.InvariantCulture);
                string importeTotalStr = importeTotal.ToString("0.00", CultureInfo.InvariantCulture);

                // ==========================================
                // 5) Armar objeto tipado NotaCredito
                // ==========================================
                var totalConImpuestosNC = new List<TotalImpuestoNC>();

                // ICE primero (código 3), luego IVA (código 2) — orden obligatorio SRI
                foreach (var kv in iceAgrupado)
                {
                    totalConImpuestosNC.Add(new TotalImpuestoNC
                    {
                        Codigo = "3",
                        CodigoPorcentaje = kv.Key,
                        BaseImponible = kv.Value.baseImp.ToString("0.00", CultureInfo.InvariantCulture),
                        Valor = kv.Value.valor.ToString("0.00", CultureInfo.InvariantCulture)
                    });
                }
                foreach (var kv in impuestosAgrupados.Values)
                {
                    totalConImpuestosNC.Add(new TotalImpuestoNC
                    {
                        Codigo = codigoImpuesto,
                        CodigoPorcentaje = kv.codPorc,
                        BaseImponible = kv.baseImp.ToString("0.00", CultureInfo.InvariantCulture),
                        Valor = kv.valor.ToString("0.00", CultureInfo.InvariantCulture)
                    });
                }

                var detallesNC = new List<DetalleNC>();
                foreach (DataRow r in TB_NC.Rows)
                {
                    string producto = r["PRODUCTO"].ToString().Trim();
                    decimal cantidad = Convert.ToDecimal(r["CANTIDAD"]);
                    if (cantidad <= 0m) cantidad = 1m;

                    decimal totalLinea = Math.Round(Convert.ToDecimal(r["TOTAL"]), 2);

                    DataSet dsProd = _services.Producto.ConsultaNombre(producto);
                    if (dsProd == null || dsProd.Tables.Count == 0 || dsProd.Tables[0].Rows.Count == 0)
                        throw new Exception("Producto no existe en BD: " + producto);

                    DataRow prodRow2 = dsProd.Tables[0].Rows[0];
                    string ivaProducto = prodRow2["IVA"].ToString().Trim().ToUpperInvariant();

                    decimal tarifa = (ivaProducto == "SI") ? tarifaBase : 0m;
                    string codPorcentaje = (tarifa == 0m) ? "0" : codigoPorcentajeBase;
                    decimal baseLinea = totalLinea;

                    var ice2 = HelperIva.CalcularIceLinea(prodRow2, cantidad, baseLinea);
                    decimal baseIVALinea2 = baseLinea + ice2.Valor;
                    decimal ivaLinea = tarifa > 0m ? Math.Round(baseIVALinea2 * tarifa, 2) : 0m;
                    decimal precioUnitarioSinIva = Math.Round(baseLinea / cantidad, 6);

                    string codigoProd = r.Table.Columns.Contains("CODIGO")
                        ? r["CODIGO"].ToString().Trim()
                        : producto;

                    var impuestosDetalle = new List<ImpuestoNC>();

                    if (ice2.Aplica && !string.IsNullOrEmpty(ice2.Codigo))
                    {
                        impuestosDetalle.Add(new ImpuestoNC
                        {
                            Codigo = "3",
                            CodigoPorcentaje = ice2.Codigo,
                            Tarifa = ice2.Tarifa.ToString("0.00", CultureInfo.InvariantCulture),
                            BaseImponible = ice2.BaseImponible.ToString("0.00", CultureInfo.InvariantCulture),
                            Valor = ice2.Valor.ToString("0.00", CultureInfo.InvariantCulture)
                        });
                    }

                    impuestosDetalle.Add(new ImpuestoNC
                    {
                        Codigo = codigoImpuesto,
                        CodigoPorcentaje = codPorcentaje,
                        Tarifa = (tarifa * 100m).ToString("0.00", CultureInfo.InvariantCulture),
                        BaseImponible = baseIVALinea2.ToString("0.00", CultureInfo.InvariantCulture),
                        Valor = ivaLinea.ToString("0.00", CultureInfo.InvariantCulture)
                    });

                    detallesNC.Add(new DetalleNC
                    {
                        CodigoInterno = codigoProd,
                        Descripcion = producto,
                        Cantidad = cantidad.ToString("0.00", CultureInfo.InvariantCulture),
                        PrecioUnitario = precioUnitarioSinIva.ToString("0.000000", CultureInfo.InvariantCulture),
                        Descuento = "0.00",
                        PrecioTotalSinImpuesto = baseLinea.ToString("0.00", CultureInfo.InvariantCulture),
                        Impuestos = new ImpuestosNC { Impuesto = impuestosDetalle }
                    });
                }

                // infoAdicional (opcional: correo del cliente)
                InfoAdicionalNC infoAdicional = null;
                if (rowCliente.Table.Columns.Contains("CORREO"))
                {
                    string correo = rowCliente["CORREO"].ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(correo))
                    {
                        infoAdicional = new InfoAdicionalNC
                        {
                            CampoAdicional = new List<CampoAdicionalNC>
                            {
                                new CampoAdicionalNC { Nombre = "MAIL", Text = correo }
                            }
                        };
                    }
                }

                var nc = new NotaCredito
                {
                    Id = "comprobante",
                    Version = "1.0.0",
                    InfoTributaria = new InfoTributariaNC
                    {
                        Ambiente = ambiente,
                        TipoEmision = tipoEmision,
                        RazonSocial = nombreComercial,
                        NombreComercial = nombreComercial,
                        Ruc = ruc,
                        ClaveAcceso = ClaveAccesoGenerada,
                        CodDoc = codDoc,
                        Estab = estab,
                        PtoEmi = ptoEmi,
                        Secuencial = secuencial,
                        DirMatriz = dirMatriz,
                        ContribuyenteRimpe = contribRimpe
                    },
                    InfoNotaCredito = new InfoNotaCredito
                    {
                        FechaEmision = fechaEmisionNC,
                        DirEstablecimiento = dirMatriz,
                        TipoIdentificacionComprador = tipoIdComprador,
                        RazonSocialComprador = razonSocialComprador,
                        IdentificacionComprador = identificacionComprador,
                        ObligadoContabilidad = obligadoContab,
                        CodDocModificado = codDocModificado,
                        NumDocModificado = numDocModificado,
                        FechaEmisionDocSustento = fechaDocSustento,
                        TotalSinImpuestos = totalSinImpStr,
                        ValorModificacion = importeTotalStr,
                        Moneda = moneda,
                        TotalConImpuestos = new TotalConImpuestosNC { TotalImpuesto = totalConImpuestosNC },
                        Motivo = motivo
                    },
                    Detalles = new DetallesNC { Detalle = detallesNC },
                    InfoAdicional = infoAdicional
                };

                // ==========================================
                // 6) Serializar y guardar XML en disco
                // ==========================================
                string carpeta = Path.Combine(_services.Paths.NotasCredito, "XML");
                Directory.CreateDirectory(carpeta);
                rutaXmlGenerado = Path.Combine(carpeta, ClaveAccesoGenerada + ".xml");

                var serializer = new XmlSerializer(typeof(NotaCredito));
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                var settings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = false
                };

                using (var writer = XmlWriter.Create(rutaXmlGenerado, settings))
                {
                    serializer.Serialize(writer, nc, ns);
                }

                claveAcceso = ClaveAccesoGenerada;
                return true;
            }
            catch (Exception ex)
            {
                error = "Error GenerarXmlNotaCredito: " + ex.Message;
                return false;
            }
        }

        // Escape XML simple (evita romper tags con & < >)

        public ResultadoFirma GuardarYFirmarNotaCreditoXml(
           string claveAcceso,
           DataRow rowEm,
           string usuario,
           string ip
       )
        {
            ResultadoFirma r = new ResultadoFirma();

            try
            {
                if (string.IsNullOrWhiteSpace(claveAcceso))
                {
                    r.Exito = false;
                    r.Mensaje = "Clave de acceso vacía para firmar Nota de Crédito.";
                    return r;
                }

                // ============================================
                // RUTAS DESDE CONFIGURACIÓN
                // ============================================
                string carpetaXml = Path.Combine(_services.Paths.NotasCredito, "XML");
                string carpetaXmlFirmado = Path.Combine(_services.Paths.NotasCredito, "XMLFIRMADOS");

                Directory.CreateDirectory(carpetaXml);
                Directory.CreateDirectory(carpetaXmlFirmado);

                string rutaXmlSinFirmar = Path.Combine(carpetaXml, claveAcceso + ".xml");
                string rutaXmlFirmado = Path.Combine(carpetaXmlFirmado, claveAcceso + ".xml");

                if (!File.Exists(rutaXmlSinFirmar))
                {
                    r.Exito = false;
                    r.Mensaje = "No existe el XML sin firmar de la Nota de Crédito: " + rutaXmlSinFirmar;
                    return r;
                }

                // ============================================
                // DATOS FIRMA
                // ============================================
                string nombreFirma = rowEm["UBICACIONARCHIVOP12"].ToString();
                string claveFirma = rowEm["CONTRASENA"].ToString();

                string rutaArchivoP12 = Path.Combine(
                    _services.Paths.General,
                    "FIRMAELECTRONICA",
                    nombreFirma
                );

                if (!File.Exists(rutaArchivoP12))
                {
                    r.Exito = false;
                    r.Mensaje = "No existe el archivo P12: " + rutaArchivoP12;
                    return r;
                }

                // ============================================
                // FIRMAR XML
                // ============================================
                Consultas consulta = new Consultas();

                bool firmado = consulta.Firma(
                    rutaArchivoP12,
                    claveFirma,
                    rutaXmlSinFirmar,
                    rutaXmlFirmado
                );

                if (!firmado)
                {
                    r.Exito = false;
                    r.Mensaje = "No se pudo firmar el XML de la Nota de Crédito.";
                    return r;
                }

                // ============================================
                // RESPUESTA
                // ============================================
                r.Exito = true;
                r.Mensaje = "XML de Nota de Crédito firmado correctamente.";
                r.RutaXml = rutaXmlSinFirmar;
                r.RutaXmlFirmado = rutaXmlFirmado;
                r.ClaveAcceso = claveAcceso;

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error al firmar XML Nota de Crédito: " + ex.Message;
                return r;
            }
        }

        public async Task<ResultadoFinalNotaCredito> ProcesarNotaCreditoElectronicaCompletaAsync(
            string numeroFactura,
            string motivo,
            DataTable productos,
            string usuarioActual,
            string ipActual,
            string numeroNotaPendiente = null
        )
        {
            var r = new ResultadoFinalNotaCredito
            {
                NumeroFactura = numeroFactura
            };

            const int MAX_REINTENTOS_SECUENCIAL = 2;
            int intento = 0;

            try
            {
                while (intento < MAX_REINTENTOS_SECUENCIAL)
                {
                    intento++;

                    // ==================================================
                    // 0) VALIDACIONES
                    // ==================================================
                    if (string.IsNullOrWhiteSpace(numeroFactura))
                        return ErrorNC("Número de factura vacío.");

                    if (string.IsNullOrWhiteSpace(motivo))
                        return ErrorNC("Motivo vacío.");

                    if (productos == null || productos.Rows.Count == 0)
                        return ErrorNC("No existen productos.");

                    bool vieneDePendiente =
                        !string.IsNullOrWhiteSpace(numeroNotaPendiente) &&
                        numeroNotaPendiente.StartsWith("PENDIENTE", StringComparison.OrdinalIgnoreCase);

                    // ==================================================
                    // 1) SECUENCIAL REAL
                    // ==================================================
                    string numeroNotaReal = await Task.Run(() =>
                        _services.Param.ObtenerSecuencialFormateado("04")
                    );

                    // ==================================================
                    // 2) PREPARAR NOTA
                    // ==================================================
                    var prep = await Task.Run(() =>
                        PrepararNotaCreditoDesdeFactura(
                            numeroFactura,
                            motivo,
                            productos,
                            usuarioActual,
                            numeroNotaReal
                        )
                    );

                    if (!prep.Exito)
                        return ErrorNC(prep.Mensaje);

                    r.NumeroNota = vieneDePendiente ? numeroNotaPendiente : prep.NumeroNota;

                    // ==================================================
                    // 3) CARGAR BASE
                    // ==================================================
                    DataSet dsEmp = await Task.Run(() => _services.Empresa.ConsultaNombre(usuarioActual));
                    DataSet dsPfm = await Task.Run(() => _services.ParamFactura.ConsultarNombre(usuarioActual));
                    DataSet dsPmNC = await Task.Run(() => _services.Param.ConsultarPorTipo("04"));
                    DataSet dsCli = await Task.Run(() => _services.Cliente.ConsultarCedula(prep.CedulaCliente));

                    if (dsEmp.Tables[0].Rows.Count == 0 ||
                        dsPfm.Tables[0].Rows.Count == 0 ||
                        dsPmNC.Tables[0].Rows.Count == 0 ||
                        dsCli.Tables[0].Rows.Count == 0)
                        return ErrorNC("No se pudieron cargar datos base.");

                    DataRow rowEmpresa = dsEmp.Tables[0].Rows[0];
                    DataRow rowPfm = dsPfm.Tables[0].Rows[0];
                    DataRow rowPmNC = dsPmNC.Tables[0].Rows[0];
                    DataRow rowCliente = dsCli.Tables[0].Rows[0];

                    // ==================================================
                    // 4) XML  (NO TOCADO)
                    // ==================================================
                    var xmlRes = await Task.Run(() =>
                    {
                        string _claveAcceso = "";
                        string _rutaXmlGenerado = "";
                        string _errorXml = "";

                        bool ok = GenerarXmlNotaCredito(
                            numeroNotaReal,
                            prep.NumeroFactura,
                            prep.FechaFactura,
                            motivo,
                            prep.TB_DetalleNC,
                            rowCliente,
                            rowEmpresa,
                            rowPfm,
                            rowPmNC,
                            usuarioActual,
                            ipActual,
                            out _claveAcceso,
                            out _rutaXmlGenerado,
                            out _errorXml
                        );

                        return new
                        {
                            Ok = ok,
                            ClaveAcceso = _claveAcceso,
                            RutaXmlGenerado = _rutaXmlGenerado,
                            ErrorXml = _errorXml
                        };
                    });

                    if (!xmlRes.Ok)
                        return ErrorNC(xmlRes.ErrorXml);

                    string claveAcceso = xmlRes.ClaveAcceso;
                    string rutaXmlGenerado = xmlRes.RutaXmlGenerado;

                    r.ClaveAcceso = claveAcceso;
                    r.RutaXmlGenerado = rutaXmlGenerado;


                    // ==================================================
                    // 5) FIRMAR
                    // ==================================================
                    var firma = await Task.Run(() =>
                        GuardarYFirmarNotaCreditoXml(
                            claveAcceso,
                            rowEmpresa,
                            usuarioActual,
                            ipActual
                        )
                    );

                    if (!firma.Exito)
                        return ErrorNC(firma.Mensaje);

                    r.RutaXmlFirmado = firma.RutaXmlFirmado;

                    // ==================================================
                    // 6) RECEPCIÓN SRI
                    // ==================================================
                    var recepcion = await Task.Run(() =>
                        _services.ProcesosGenerales.EnviarRecepcion(
                            firma.RutaXmlFirmado,
                            claveAcceso,
                            numeroNotaReal,
                            "NOTADECREDITO",
                            usuarioActual,
                            ipActual
                        )
                    );

                    // -----------------------------
                    // 6.1 SECUENCIAL REPETIDO
                    // -----------------------------
                    if (recepcion.SecuencialRepetido)
                    {
                        await Task.Run(() => _services.Param.IncrementarSecuencial("04"));
                        continue;
                    }

                    // -----------------------------
                    // 6.2 ERROR → PENDIENTE
                    // -----------------------------
                    if (!recepcion.Exito)
                    {
                        string numeroPendiente =
                            !string.IsNullOrWhiteSpace(recepcion.EstadoEspecial)
                                ? recepcion.EstadoEspecial
                                : _services.Pendientes.ObtenerEstadoPendientePorTipo("NOTADECREDITO");

                        await Task.Run(() =>
                            InsertarNotaCredito(
                                prep,
                                rowPfm,
                                claveAcceso,
                                numeroPendiente,
                                "PENDIENTE",
                                usuarioActual,
                                ipActual
                            )
                        );

                        r.Exito = false;
                        r.Autorizado = false;
                        r.NumeroNota = numeroPendiente;
                        r.Mensaje =
                            "Nota registrada como pendiente.\n\n" +
                            "Número: " + numeroPendiente + "\n\n" +
                            recepcion.Mensaje;

                        return r;
                    }

                    // -----------------------------
                    // 6.3 PENDIENTE_AUTORIZACION
                    // -----------------------------
                    if (!string.IsNullOrWhiteSpace(recepcion.EstadoEspecial) &&
                        recepcion.EstadoEspecial.StartsWith("PENDIENTE_AUTORIZACION"))
                    {
                        if (vieneDePendiente)
                        {
                            await Task.Run(() =>
                                _services.NotaCredito.ActualizarNumeroNota_EncabezadoYDetalle(
                                    numeroNotaPendiente,
                                    numeroNotaReal
                                )
                            );
                        }
                        else
                        {
                            await Task.Run(() =>
                                InsertarNotaCredito(
                                    prep,
                                    rowPfm,
                                    claveAcceso,
                                    numeroNotaReal,
                                    "PENDIENTE_AUTORIZACION",
                                    usuarioActual,
                                    ipActual
                                )
                            );
                        }

                        await Task.Run(() => _services.Param.IncrementarSecuencial("04"));

                        r.Exito = true;
                        r.Autorizado = false;
                        r.NumeroNota = numeroNotaReal;
                        r.Mensaje =
                            "Nota recibida por el SRI.\n\nPendiente de autorización.";

                        return r;
                    }

                    // ==================================================
                    // 7) AUTORIZACIÓN
                    // ==================================================
                    var sri = await Task.Run(() =>
                        _services.ProcesosGenerales.ConsultarAutorizacion(
                            firma.RutaXmlFirmado,
                            claveAcceso,
                            "NOTADECREDITO",
                            numeroNotaReal,
                            Path.Combine(_services.Paths.NotasCredito, "XMLAUTORIZADOS"),
                            1,
                            (i, m) => { },
                            (i, m) => { },
                            usuarioActual,
                            ipActual
                        )
                    );

                    if (!sri.Exito)
                    {
                        InsertarNotaCredito(prep, rowPfm, claveAcceso, numeroNotaReal, "NO_AUTORIZADO", usuarioActual, ipActual);
                        return ErrorNC(sri.Mensaje);
                    }

                    r.RutaXmlAutorizado = sri.RutaXmlAutorizado;

                    // ==================================================
                    // 8) INSERTAR / ACTUALIZAR AUTORIZADO
                    // ==================================================
                    if (vieneDePendiente)
                    {
                        await Task.Run(() =>
                            _services.NotaCredito.ActualizarNumeroNota_EncabezadoYDetalle(
                                numeroNotaPendiente,
                                numeroNotaReal
                            )
                        );
                    }
                    else
                    {
                        await Task.Run(() =>
                            InsertarNotaCredito(
                                prep,
                                rowPfm,
                                claveAcceso,
                                numeroNotaReal,
                                "AUTORIZADO",
                                usuarioActual,
                                ipActual
                            )
                        );
                    }

                    // ==================================================
                    // 9) PDF
                    // ==================================================
                    var pdf = await Task.Run(() =>
                        _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                            sri.XmlAutorizado,
                            rowEmpresa,
                            Path.Combine(_services.Paths.NotasCredito, "PDF"),
                            (gen, carpeta, xml, logo) =>
                                gen.GeneracionRideNotaCreditoSRI(carpeta, xml, logo)
                        )
                    );

                    if (!pdf.Exito)
                        return ErrorNC(pdf.Mensaje);

                    r.RutaPdf = pdf.RutaPdf;

                    // ==================================================
                    // 10) CORREO
                    // ==================================================
                    var solicitud = new SolicitudCorreoDocumento
                    {
                        TipoDocumento = "NOTADECREDITO",
                        NumeroDocumento = numeroNotaReal,
                        RutaPdf = r.RutaPdf,
                        RutaXmlAutorizado = r.RutaXmlAutorizado
                    };

                    var correo = await Task.Run(() =>
                        _services.ProcesosGenerales.EnviarCorreoDocumento(
                            solicitud,
                            rowCliente,
                            "NOTADECREDITO",
                            usuarioActual,
                            ipActual
                        )
                    );

                    r.EnvioCorreoExitoso = correo.Exito;

                    // ==================================================
                    // 11) INCREMENTAR SECUENCIAL FINAL
                    // ==================================================
                    await Task.Run(() => _services.Param.IncrementarSecuencial("04"));

                    // ==================================================
                    // 12) LIMPIAR XML TRANSITORIOS
                    // ==================================================
                    _services.ProcesosGenerales.LimpiarXmlTransitorios(
                        rutaXmlGenerado,
                        firma.RutaXmlFirmado,
                        r.RutaXmlAutorizado);

                    r.Exito = true;
                    r.Autorizado = true;
                    r.NumeroNota = numeroNotaReal;
                    r.Mensaje = vieneDePendiente
                        ? "Nota pendiente autorizada correctamente."
                        : "Nota de Crédito autorizada correctamente.";

                    return r;
                }

                return ErrorNC("No se pudo generar Nota de Crédito por conflictos de secuencial.");
            }
            catch (Exception ex)
            {
                return ErrorNC("Error general NC: " + ex.Message);
            }
        }


        public ResultadoFinalNotaCredito ProcesarNotaDesdeAutorizacion(
            string numeroNota,
            string claveAcceso,
            string rutaXmlFirmado,
            DataRow rowEmpresa,
            DataRow rowCliente,
            string usuarioActual,
            string ipActual,
            string proceso // "AUTORIZACION" | "PDF" | "CORREO"
        )
        {
            var r = new ResultadoFinalNotaCredito
            {
                NumeroNota = numeroNota,
                ClaveAcceso = claveAcceso
            };

            try
            {
                // ==========================================================
                // VALIDACIONES BASE
                // ==========================================================
                if (string.IsNullOrWhiteSpace(numeroNota))
                    return ErrorNC(r, "Número de Nota vacío.");

                if (string.IsNullOrWhiteSpace(claveAcceso))
                    return ErrorNC(r, "Clave de acceso vacía.");

                if (string.IsNullOrWhiteSpace(rutaXmlFirmado))
                    return ErrorNC(r, "Ruta XML firmado vacía.");

                if (rowEmpresa == null)
                    return ErrorNC(r, "Empresa no cargada.");

                if (rowCliente == null)
                    return ErrorNC(r, "Cliente no cargado.");

                if (string.IsNullOrWhiteSpace(proceso))
                    proceso = "AUTORIZACION";

                proceso = proceso.Trim().ToUpperInvariant();

                XDocument xmlAutorizado = null;

                // ==========================================================
                // RUTAS BASE (ÚNICA FUENTE DE VERDAD)
                // ==========================================================
                string carpetaXmlAutorizado = Path.Combine(_services.Paths.NotasCredito, "XMLAUTORIZADOS");
                string carpetaPdf = Path.Combine(_services.Paths.NotasCredito, "PDF");

                string rutaXmlAutorizado = Path.Combine(carpetaXmlAutorizado, claveAcceso + ".xml");
                string rutaPdf = Path.Combine(carpetaPdf, claveAcceso + ".pdf");

                // ==========================================================
                // 1) AUTORIZACIÓN
                // ==========================================================
                if (proceso == "AUTORIZACION")
                {
                    var sri = _services.ProcesosGenerales.ConsultarAutorizacion(
                        rutaXmlFirmado,
                        claveAcceso,
                        "NOTADECREDITO",
                        numeroNota,
                        Path.Combine(_services.Paths.NotasCredito, "XMLAUTORIZADOS"),
                        1,
                        (i, m) => { },
                        (i, m) => { },
                        usuarioActual,
                        ipActual
                    );

                    if (!sri.Exito)
                    {
                        r.Exito = true;
                        r.Autorizado = false;
                        r.Mensaje = "La Nota aún no está autorizada: " + sri.Mensaje;
                        return r;
                    }

                    xmlAutorizado = sri.XmlAutorizado;
                    r.RutaXmlAutorizado = sri.RutaXmlAutorizado;
                    r.Autorizado = true;

                    proceso = "PDF";
                }

                // ==========================================================
                // 2) PDF
                // ==========================================================
                if (proceso == "PDF")
                {
                    if (xmlAutorizado == null)
                    {
                        if (!File.Exists(rutaXmlAutorizado))
                            return ErrorNC(r, "No existe XML autorizado para generar PDF.");

                        xmlAutorizado = XDocument.Load(rutaXmlAutorizado);
                        r.RutaXmlAutorizado = rutaXmlAutorizado;
                    }

                    var pdf = _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                        xmlAutorizado,
                        rowEmpresa,
                        Path.Combine(_services.Paths.NotasCredito, "PDF"),
                        (gen, carpeta, xml, logo) =>
                            gen.GeneracionRideNotaCreditoSRI(carpeta, xml, logo)
                    );

                    if (!pdf.Exito)
                        return ErrorNC(r, "No se pudo generar PDF: " + pdf.Mensaje);

                    r.RutaPdf = pdf.RutaPdf;
                    proceso = "CORREO";
                }

                // ==========================================================
                // 3) CORREO
                // ==========================================================
                if (proceso == "CORREO")
                {
                    // 🔒 GARANTIZAR XML AUTORIZADO
                    if (string.IsNullOrWhiteSpace(r.RutaXmlAutorizado))
                    {
                        if (!File.Exists(rutaXmlAutorizado))
                            return ErrorNC(r, "No existe XML autorizado para enviar.");

                        r.RutaXmlAutorizado = rutaXmlAutorizado;
                    }

                    // 🔒 GARANTIZAR PDF
                    if (string.IsNullOrWhiteSpace(r.RutaPdf))
                    {
                        if (!File.Exists(rutaPdf))
                            return ErrorNC(r, "No existe PDF para enviar.");

                        r.RutaPdf = rutaPdf;
                    }

                    var solicitud = new SolicitudCorreoDocumento
                    {
                        TipoDocumento = "NOTADECREDITO",
                        NumeroDocumento = numeroNota,
                        RutaPdf = r.RutaPdf,
                        RutaXmlAutorizado = r.RutaXmlAutorizado
                    };

                    var correo = _services.ProcesosGenerales.EnviarCorreoDocumento(
                        solicitud,
                        rowCliente,
                        "NOTADECREDITO",
                        usuarioActual,
                        ipActual
                    );

                    r.EnvioCorreoExitoso = correo.Exito;

                    // Limpiar XML transitorios (XML y XMLFIRMADO) ahora que ya existe XMLAUTORIZADO
                    {
                        string rutaXmlBase = Path.Combine(
                            Path.GetDirectoryName(Path.GetDirectoryName(rutaXmlFirmado)),
                            "XML",
                            Path.GetFileName(rutaXmlFirmado));

                        _services.ProcesosGenerales.LimpiarXmlTransitorios(
                            rutaXmlBase,
                            rutaXmlFirmado,
                            r.RutaXmlAutorizado);
                    }

                    r.Exito = true;
                    r.Autorizado = true;
                    r.Mensaje = correo.Exito
                        ? "Nota procesada correctamente. Correo enviado."
                        : "Nota autorizada, pero no se pudo enviar el correo.";

                    return r;
                }

                return ErrorNC(r, "Proceso no reconocido: " + proceso);
            }
            catch (Exception ex)
            {
                return ErrorNC(r, "Error ProcesarNotaDesdeAutorizacion: " + ex.Message);
            }
        }

        public ResultadoFinalNotaCredito NotaPendienteAutorizacionDesdeApi(
        string numeroNota,
        string fechaNota,
        string usuarioActual,
        string ipActual,
        string proceso = "AUTORIZACION"
        )
        {
            if (!_services.ProcesosGenerales.PrepararPendienteDocumento(
                "NOTA_CREDITO",
                numeroNota,
                fechaNota,
                usuarioActual,
                out string claveAcceso,
                out string rutaXmlFirmado,
                out DataRow rowEmpresa,
                out DataRow rowCliente,
                out string mensajeError
            ))
            {
                return new ResultadoFinalNotaCredito
                {
                    Exito = false,
                    Mensaje = mensajeError
                };
            }

            return ProcesarNotaDesdeAutorizacion(
                numeroNota,
                claveAcceso,
                rutaXmlFirmado,
                rowEmpresa,
                rowCliente,
                usuarioActual,
                ipActual,
                proceso
            );
        }

        public async Task<ResultadoPDF> ProcesarNotaCreditoPreviewAsync(
    string numeroFactura,
    string motivo,
    DataTable productos,
    string usuarioActual
)
        {
            ResultadoPDF r = new ResultadoPDF();

            try
            {
                // ==================================================
                // 1) VALIDACIONES
                // ==================================================
                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    r.Exito = false;
                    r.Mensaje = "Número de factura vacío.";
                    return r;
                }

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    r.Exito = false;
                    r.Mensaje = "Motivo vacío.";
                    return r;
                }

                if (productos == null || productos.Rows.Count == 0)
                {
                    r.Exito = false;
                    r.Mensaje = "No existen productos.";
                    return r;
                }

                // ==================================================
                // 2) GENERAR SECUENCIAL PREVIEW (NO AFECTA BD)
                // ==================================================
                string numeroNotaPreview =
                    _services.Param.ObtenerSecuencialFormateado("04");

                // ==================================================
                // 3) PREPARAR NOTA
                // ==================================================
                var prep = PrepararNotaCreditoDesdeFactura(
                    numeroFactura,
                    motivo,
                    productos,
                    usuarioActual,
                    numeroNotaPreview
                );

                if (!prep.Exito)
                {
                    r.Exito = false;
                    r.Mensaje = prep.Mensaje;
                    return r;
                }

                // ==================================================
                // 4) CARGAR BASE
                // ==================================================
                DataSet dsEmp = _services.Empresa.ConsultaNombre(usuarioActual);
                DataSet dsPfm = _services.ParamFactura.ConsultarNombre(usuarioActual);
                DataSet dsPmNC = _services.Param.ConsultarPorTipo("04");
                DataSet dsCli = _services.Cliente.ConsultarCedula(prep.CedulaCliente);

                if (dsEmp.Tables[0].Rows.Count == 0 ||
                    dsPfm.Tables[0].Rows.Count == 0 ||
                    dsPmNC.Tables[0].Rows.Count == 0 ||
                    dsCli.Tables[0].Rows.Count == 0)
                {
                    r.Exito = false;
                    r.Mensaje = "No se pudieron cargar datos base.";
                    return r;
                }

                DataRow rowEmpresa = dsEmp.Tables[0].Rows[0];
                DataRow rowPfm = dsPfm.Tables[0].Rows[0];
                DataRow rowPmNC = dsPmNC.Tables[0].Rows[0];
                DataRow rowCliente = dsCli.Tables[0].Rows[0];

                // ==================================================
                // 5) GENERAR XML (SIN FIRMAR)
                // ==================================================
                string claveAcceso;
                string rutaXmlGenerado;
                string errorXml;

                bool ok = GenerarXmlNotaCredito(
                    numeroNotaPreview,
                    prep.NumeroFactura,
                    prep.FechaFactura,
                    motivo,
                    prep.TB_DetalleNC,
                    rowCliente,
                    rowEmpresa,
                    rowPfm,
                    rowPmNC,
                    usuarioActual,
                    "",
                    out claveAcceso,
                    out rutaXmlGenerado,
                    out errorXml
                );

                if (!ok)
                {
                    r.Exito = false;
                    r.Mensaje = errorXml;
                    return r;
                }

                // ==================================================
                // 6) CONVERTIR A XDOCUMENT
                // ==================================================
                XDocument xmlDoc = XDocument.Load(rutaXmlGenerado);

                // ==================================================
                // 7) GENERAR PDF OFFLINE (SIN AUTORIZACIÓN)
                // ==================================================
                string carpeta = Path.Combine(_services.Paths.NotasCredito, "PDFPREVIEW");
                Directory.CreateDirectory(carpeta);
                _services.ProcesosGenerales.LimpiarPreviewsAntiguos(carpeta, TimeSpan.FromHours(2));

                string rutaLogo = Path.Combine(
                    _services.Paths.General,
                    "LOGOFACTURA",
                    rowEmpresa["IMAGEN"].ToString()
                );

                if (!File.Exists(rutaLogo))
                {
                    r.Exito = false;
                    r.Mensaje = "Logo no encontrado.";
                    return r;
                }

                using (var logoStream = new MemoryStream(File.ReadAllBytes(rutaLogo)))
                {
                    logoStream.Position = 0;

                    var gen = new CGenerarRide();

                    string rutaPdf = gen.GeneracionRideNotaCreditoSRI_OfflineSinAutorizacion(
                        carpeta,
                        xmlDoc,
                        logoStream
                    );

                    if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
                    {
                        r.Exito = false;
                        r.Mensaje = "No se pudo generar el PDF.";
                        return r;
                    }

                    r.Exito = true;
                    r.RutaPdf = rutaPdf;
                    return r;
                }
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = ex.Message;
                return r;
            }
        }

        // Helper local
        private static ResultadoFinalNotaCredito ErrorNC(string mensaje)
        {
            return new ResultadoFinalNotaCredito
            {
                Exito = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoFinalNotaCredito ErrorNC(ResultadoFinalNotaCredito r, string mensaje)
        {
            r.Exito = false;
            r.Mensaje = mensaje;
            return r;
        }

        private static ResultadoNotaCredito ErrorNC(ResultadoNotaCredito r, string msg)
        {
            r.Exito = false;
            r.Mensaje = msg;
            return r;
        }



    }
}
