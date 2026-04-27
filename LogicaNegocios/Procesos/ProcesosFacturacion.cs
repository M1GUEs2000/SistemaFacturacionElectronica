using LogicaNegocios.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using xmlFactura;

namespace LogicaNegocios.Procesos
{

    //Clases auxiliares
    public class ResultadoVenta
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public string NumeroFactura { get; set; }
        public DataTable TB_Venta { get; set; }
        public DataTable TB_Transacciones { get; set; }
    }

    public class ResultadoFirma
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public string RutaXml { get; set; }
        public string RutaXmlFirmado { get; set; }
        public string ClaveAcceso { get; set; }
    }



    public class ResultadoPDF
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public string RutaPdf { get; set; }
    }

    public class ResultadoCorreo
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
    }

    public class ResultadoSecuencial
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
    }

    public class ResultadoFacturaPreparada
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public string NumeroFactura { get; set; }
        public bool EsElectronica { get; set; }

        public DataTable TB_Venta { get; set; }
        public DataTable TB_Transacciones { get; set; }

        public DataTable DetalleOriginal { get; set; } // copia segura
    }

    public class ResultadoFacturaInsertada
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public string NumeroFactura { get; set; }
    }


    public class ResultadoFinalFactura
    {
        public bool Exito { get; set; }
        public bool EnvioCorreoExitoso { get; set; }
        public bool SecuencialRepetido { get; set; }
        public bool EsElectronica { get; set; }
        public bool Autorizado { get; set; }
        public string NumeroFactura { get; set; }
        public string Mensaje { get; set; }
        public string ClaveAcceso { get; set; }
        public string RutaPdf { get; set; }
        public string RutaXmlAutorizado { get; set; }
        public string SecuencialUsado { get; set; }
        public DataTable TB_Venta { get; set; }
        public DataTable TB_Transacciones { get; set; }
    }

    //Metodos


    public class ProcesosFacturacion
    {


        private readonly AppServices _services;

        public ProcesosFacturacion(AppServices services

        )
        {
            _services = services;

        }

        public ResultadoFacturaPreparada PrepararFactura(
            DataTable detalleFactura,
            string cliente,
            string formaPago,
            string lblFacturaElectronica,
            string fecha,
            string hora
        )
        {
            var r = new ResultadoFacturaPreparada();

            try
            {
                if (detalleFactura == null || detalleFactura.Rows.Count == 0)
                    throw new Exception("No existen productos en la factura.");

                string clienteUpper = cliente.Trim().ToUpperInvariant();
                string formaUpper = formaPago.Trim().ToUpperInvariant();

                bool esElectronica =
                    clienteUpper != "FINAL" &&
                    formaUpper != "COMPRAS" &&
                    lblFacturaElectronica == "SI";

                string numeroFactura;
                if (clienteUpper == "FINAL")
                    numeroFactura = _services.Facturacion.ObtenerSecuencialConsumidor();
                else if (formaUpper == "COMPRAS")
                    numeroFactura = "COMPRA";
                else
                    numeroFactura = _services.Param.ObtenerSecuencialFormateado("01");

                // Tablas temp (si las quieres conservar)
                DataTable TB_VENTA = new DataTable();
                TB_VENTA.Columns.Add("CANTIDAD", typeof(string));
                TB_VENTA.Columns.Add("PRODUCTO", typeof(string));
                TB_VENTA.Columns.Add("VALOR", typeof(string));
                TB_VENTA.Columns.Add("TOTAL", typeof(double));
                TB_VENTA.Columns.Add("CODIGO", typeof(string));

                DataTable TB_TRAS = new DataTable();
                TB_TRAS.Columns.Add("CNT", typeof(int));
                TB_TRAS.Columns.Add("VALOR", typeof(double));
                TB_TRAS.Columns.Add("PRODUCTO", typeof(string));
                TB_TRAS.Columns.Add("TOTAL", typeof(double));
                TB_TRAS.Columns.Add("FORMADEPAGO", typeof(string));
                TB_TRAS.Columns.Add("HORA", typeof(string));
                TB_TRAS.Columns.Add("NUMEROFACTURA", typeof(string));
                TB_TRAS.Columns.Add("CODIGO", typeof(string));

                foreach (DataRow row in detalleFactura.Rows)
                {
                    int cantidad = Convert.ToInt32(row["CANTIDAD"]);
                    string producto = row["PRODUCTO"].ToString();

                    string codigo = detalleFactura.Columns.Contains("CODIGO")
                        ? row["CODIGO"].ToString()
                        : "";

                    // Usar TOTAL_DISPLAY (con ICE+IVA) si está disponible; si no, caer en TOTAL
                    double totalFila = detalleFactura.Columns.Contains("TOTAL_DISPLAY")
                        ? Convert.ToDouble(row["TOTAL_DISPLAY"].ToString().Replace(",", "."), CultureInfo.InvariantCulture)
                        : Convert.ToDouble(row["TOTAL"].ToString().Replace(",", "."), CultureInfo.InvariantCulture);

                    if (formaUpper == "COMPRAS")
                        totalFila *= -1;

                    // VALOR unitario con impuestos = total con impuestos / cantidad
                    string valorConImp = cantidad > 0
                        ? Math.Round(totalFila / cantidad, 2).ToString(CultureInfo.InvariantCulture)
                        : row["VALOR"].ToString();

                    TB_VENTA.Rows.Add(cantidad.ToString(), producto, valorConImp, totalFila, codigo);

                    TB_TRAS.Rows.Add(
                        cantidad,
                        Convert.ToDouble(valorConImp, CultureInfo.InvariantCulture),
                        producto,
                        totalFila,
                        formaPago,
                        hora,
                        numeroFactura,
                        codigo
                    );
                }


                r.Exito = true;
                r.NumeroFactura = numeroFactura;
                r.EsElectronica = esElectronica;
                r.DetalleOriginal = detalleFactura.Copy();
                r.TB_Venta = TB_VENTA;
                r.TB_Transacciones = TB_TRAS;

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error PrepararFactura: " + ex.Message;
                return r;
            }
        }


        public ResultadoFacturaInsertada InsertarFactura(
            DataTable detalle,
            string fecha,
            string hora,
            string formaPago,
            string cliente,
            string numeroFactura,
            string usuario,
            string ip
        )
        {
            var r = new ResultadoFacturaInsertada();

            try
            {
                if (detalle == null || detalle.Rows.Count == 0)
                    throw new Exception("Detalle vacío.");

                foreach (DataRow row in detalle.Rows)
                {
                    _services.Facturacion.Insertar(
                        fecha,
                        formaPago,
                        row["PRODUCTO"].ToString(),
                        row["CANTIDAD"].ToString(),
                        row["TOTAL"].ToString(),
                        cliente,
                        hora,
                        numeroFactura,
                        usuario,
                        ip
                    );
                }

                r.Exito = true;
                r.NumeroFactura = numeroFactura;
                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error InsertarFactura: " + ex.Message;
                return r;
            }
        }




        public Factura GenerarXmlFactura(
            DataSet em,
            DataSet pfm,
            DataSet pm,
            DataRow rowCliente,
            DataTable detalleFactura,
            string numeroFactura,
            string fechaPago,
            string totalVisual,
            string formaPago,
            string comentario
        )
        {
            try
            {
                DataRow rowEm = em.Tables[0].Rows[0];
                DataRow rowPfm = pfm.Tables[0].Rows[0];
                DataRow rowPm = pm.Tables[0].Rows[0];

                string rucInst = rowEm["RUC"].ToString();
                string nombreInst = rowEm["NOMBRE"].ToString();
                string direccionInst = rowEm["DIRECCION"].ToString();

                string tipoEmision1 = rowPfm["TIPOEMISION"].ToString();
                string contribuyenteRimpe1 = rowPfm["CONTRIBUYENTERIMPE"].ToString().Trim();
                if (string.IsNullOrWhiteSpace(contribuyenteRimpe1))
                    contribuyenteRimpe1 = null;
                string codDoc1 = rowPfm["CODDOC"].ToString();
                string estab1 = rowPfm["ESTAB"].ToString();
                string ptoEmi1 = rowPfm["PUNTOEMISION"].ToString();

                string codigoImpuesto = rowPfm["CODIGOIMPUESTO"].ToString();       // normalmente "2"
                string codigoPorcentaje1 = rowPfm["CODIGOPORCENTAJE"].ToString();  // "2","3","4","5","10"

                string secuenciaFactComp = _services.Param.ObtenerSecuencialFormateado("01");
                string codigoNumerico = _services.Param.ObtenerCodigoNumerico("01");


                DateTime fecha_emision;

                string[] formatos = new[]
                {
                    "dd/MM/yyyy",
                    "yyyy/MM/dd",
                    "yyyy-MM-dd",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-ddTHH:mm:ss.fff",
                    "yyyy-MM-ddTHH:mm:ss.FFF",
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "yyyy-MM-ddTHH:mm:ss.fffK",
                    "yyyy-MM-ddTHH:mm:ssK"
                };

                if (!DateTime.TryParseExact(
                        fechaPago,
                        formatos,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                        out fecha_emision))
                {
                    throw new Exception("FechaPago inválida: " + fechaPago);
                }

                string fechaPagoUnido = fecha_emision.ToString("ddMMyyyy");
                string fechaEmision1 = fecha_emision.ToString("dd/MM/yyyy");

                // ===================================================
                // CLAVE DE ACCESO
                // ===================================================
                CFuncionesComprobantesElectronicos cfunciones = new CFuncionesComprobantesElectronicos();
                string claveAcceso = cfunciones.ClaveAcceso(
                    fechaPagoUnido,
                    codDoc1,
                    rucInst,
                    rowPfm["AMBIENTE"].ToString(),
                    estab1,
                    ptoEmi1,
                    secuenciaFactComp,
                    codigoNumerico,
                    tipoEmision1
                );

                // ===================================================
                // FACTURA
                // ===================================================
                Factura factura = new Factura
                {
                    Id = "comprobante",
                    Version = "1.1.0"
                };

                factura.InfoTributariaFactura = new InfoTributariaFactura
                {
                    Ambiente = rowPfm["AMBIENTE"].ToString(),
                    TipoEmision = tipoEmision1,
                    RazonSocial = nombreInst,
                    NombreComercial = nombreInst,
                    Ruc = rucInst,
                    ClaveAcceso = claveAcceso,
                    CodDoc = codDoc1,
                    Estab = estab1,
                    PtoEmi = ptoEmi1,
                    Secuencial = secuenciaFactComp,
                    DirMatriz = direccionInst,
                    ContribuyenteRimpe = contribuyenteRimpe1
                };

                // ===================================================
                // CLIENTE
                // ===================================================
                string cedulaCliente = rowCliente["CEDULA"].ToString();
                string direccionCliente = rowCliente["DIRECCION"].ToString();
                string nombreCliente = rowCliente["NOMBRE"].ToString();
                string correoCliente = rowCliente["CORREO"].ToString();

                string tipoIdent =
                    cedulaCliente == "9999999999999" ? "07" :
                    cedulaCliente.Length == 13 ? "04" :
                    cedulaCliente.Length == 10 ? "05" :
                    "08";

                // ===================================================
                // INFO FACTURA
                // ===================================================
                InfoFactura infoFactura = new InfoFactura
                {
                    FechaEmision = fechaEmision1,
                    DirEstablecimiento = direccionInst,
                    ObligadoContabilidad = rowPfm["OBLIGADOCONTABILIDAD"].ToString(),
                    TipoIdentificacionComprador = tipoIdent,
                    RazonSocialComprador = nombreCliente,
                    IdentificacionComprador = cedulaCliente,
                    DireccionComprador = direccionCliente,
                    Moneda = rowPfm["MONEDA"].ToString(),
                    TotalDescuento = "0.00",
                    Propina = "0.00"
                };

                decimal tarifaBase = _services.TarifaIva;

                // ===================================================
                // DETALLES + ACUMULADOS (0% vs GRAVADO)
                // ===================================================
                decimal base0 = 0m;
                decimal baseGravada = 0m;
                decimal baseGravadaConICE = 0m;
                decimal ivaGravado = 0m;
                decimal totalICE = 0m;
                // Key = ICE_CODIGO, Value = [baseImponible, valor, tarifa]
                var iceAcumulado = new Dictionary<string, decimal[]>();

                DetallesFactura detalles = new DetallesFactura
                {
                    DetalleFacturas = new List<DetalleFactura>()
                };

                foreach (DataRow row in detalleFactura.Rows)
                {
                    string producto = row["PRODUCTO"].ToString();
                    decimal cantidad = Convert.ToDecimal(row["CANTIDAD"]);

                    // TOTAL real de la línea (lo que ya cobra tu sistema)
                    decimal totalLinea = Convert.ToDecimal(row["TOTAL"]);

                    // =====================================================
                    // OBTENER CODIGO E IVA DESDE BD
                    // =====================================================
                    DataSet dsProd = _services.Producto.ConsultaNombre(producto);
                    if (dsProd == null || dsProd.Tables[0].Rows.Count == 0)
                        throw new Exception("Producto no existe en BD: " + producto);

                    string codProd = dsProd.Tables[0].Rows[0]["CODIGO"].ToString();
                    string ivaProducto = dsProd.Tables[0].Rows[0]["IVA"].ToString().Trim().ToUpperInvariant();

                    // En tu BD es "SI"/"NO"
                    bool tieneIva = (ivaProducto == "SI");
                    decimal tarifaLinea = tieneIva ? tarifaBase : 0m;

                    // =====================================================
                    // BASE SIN IMPUESTOS
                    // =====================================================
                    decimal subtotalSinImp = totalLinea;
                    decimal precioSinIVA = cantidad == 0m ? 0m : Math.Round(subtotalSinImp / cantidad, 6);

                    // =====================================================
                    // ICE POR LÍNEA
                    // =====================================================
                    var iceLinea = HelperIva.CalcularIceLinea(dsProd.Tables[0].Rows[0], cantidad, subtotalSinImp);
                    decimal iceValorLinea = iceLinea.Valor;
                    decimal iceBaseImponibleLinea = iceLinea.BaseImponible;
                    decimal iceTarifaDecimal = iceLinea.Tarifa;
                    string iceCodigo = iceLinea.Codigo;
                    bool aplicaIce = iceLinea.Aplica;

                    if (aplicaIce)
                    {
                        totalICE += iceValorLinea;

                        if (!string.IsNullOrEmpty(iceCodigo))
                        {
                            if (!iceAcumulado.ContainsKey(iceCodigo))
                                iceAcumulado[iceCodigo] = new decimal[] { 0m, 0m, iceTarifaDecimal };
                            iceAcumulado[iceCodigo][0] += iceBaseImponibleLinea;
                            iceAcumulado[iceCodigo][1] += iceValorLinea;
                        }
                    }

                    // =====================================================
                    // IVA (base SRI = precio + ICE)
                    // =====================================================
                    decimal baseIVALinea = subtotalSinImp + iceValorLinea;
                    decimal valorIVA = HelperIva.CalcularBaseEIvaLinea(subtotalSinImp, tarifaLinea, iceValorLinea).Iva;

                    // =====================================================
                    // ACUMULAR POR TARIFA
                    // =====================================================
                    if (tarifaLinea == 0m)
                        base0 += subtotalSinImp;
                    else
                    {
                        baseGravada += subtotalSinImp;
                        baseGravadaConICE += baseIVALinea;
                        ivaGravado += valorIVA;
                    }

                    // Código porcentaje por línea
                    string codigoPorcentajeFinal = (tarifaLinea == 0m) ? "0" : codigoPorcentaje1;

                    // =====================================================
                    // IMPUESTOS POR LÍNEA
                    // =====================================================
                    Impuestos impuestos = new Impuestos
                    {
                        Impuesto = new List<Impuesto>()
                    };

                    // ICE primero (código 3), luego IVA (código 2) — orden SRI
                    if (aplicaIce && !string.IsNullOrEmpty(iceCodigo))
                    {
                        impuestos.Impuesto.Add(new Impuesto
                        {
                            Codigo = "3",
                            CodigoPorcentaje = iceCodigo,
                            Tarifa = iceTarifaDecimal.ToString("0.00", CultureInfo.InvariantCulture),
                            BaseImponible = iceBaseImponibleLinea.ToString("0.00", CultureInfo.InvariantCulture),
                            Valor = iceValorLinea.ToString("0.00", CultureInfo.InvariantCulture)
                        });
                    }

                    impuestos.Impuesto.Add(new Impuesto
                    {
                        Codigo = codigoImpuesto,
                        CodigoPorcentaje = codigoPorcentajeFinal,
                        Tarifa = (tarifaLinea * 100).ToString("0.00", CultureInfo.InvariantCulture),
                        BaseImponible = baseIVALinea.ToString("0.00", CultureInfo.InvariantCulture),
                        Valor = valorIVA.ToString("0.00", CultureInfo.InvariantCulture)
                    });

                    // =====================================================
                    // DETALLE
                    // =====================================================
                    DetalleFactura det = new DetalleFactura
                    {
                        CodigoPrincipal = codProd,
                        CodigoAuxiliar = codProd,
                        Descripcion = producto,
                        Cantidad = cantidad.ToString("0.00", CultureInfo.InvariantCulture),
                        PrecioUnitario = precioSinIVA.ToString("0.000000", CultureInfo.InvariantCulture),
                        Descuento = "0.00",
                        PrecioTotalSinImpuesto = subtotalSinImp.ToString("0.00", CultureInfo.InvariantCulture),
                        Impuestos = impuestos
                    };

                    detalles.DetalleFacturas.Add(det);
                }

                // ===================================================
                // TOTALES (COHERENTES CON DETALLE)
                // ===================================================
                decimal totalSinImpuestos = Math.Round(base0 + baseGravada, 2);
                decimal importeTotal = Math.Round(totalSinImpuestos + totalICE + ivaGravado, 2);

                infoFactura.TotalSinImpuestos = totalSinImpuestos.ToString("0.00", CultureInfo.InvariantCulture);
                infoFactura.ImporteTotal = importeTotal.ToString("0.00", CultureInfo.InvariantCulture);

                // ===================================================
                // TOTAL CON IMPUESTOS
                // ===================================================
                TotalConImpuestos tc = new TotalConImpuestos();
                tc.TotalImpuesto = new List<TotalImpuesto>();

                // ICE primero, agrupado por código
                foreach (KeyValuePair<string, decimal[]> kvp in iceAcumulado)
                {
                    tc.TotalImpuesto.Add(new TotalImpuesto
                    {
                        Codigo = "3",
                        CodigoPorcentaje = kvp.Key,
                        BaseImponible = Math.Round(kvp.Value[0], 2).ToString("0.00", CultureInfo.InvariantCulture),
                        Tarifa = kvp.Value[2].ToString("0.00", CultureInfo.InvariantCulture),
                        Valor = Math.Round(kvp.Value[1], 2).ToString("0.00", CultureInfo.InvariantCulture)
                    });
                }

                // IVA al final
                tc.TotalImpuesto.Add(new TotalImpuesto
                {
                    Codigo = codigoImpuesto,
                    CodigoPorcentaje = codigoPorcentaje1,
                    BaseImponible = Math.Round(baseGravadaConICE, 2).ToString("0.00", CultureInfo.InvariantCulture),
                    Tarifa = (tarifaBase * 100).ToString("0.00", CultureInfo.InvariantCulture),
                    Valor = Math.Round(ivaGravado, 2).ToString("0.00", CultureInfo.InvariantCulture)
                });
                infoFactura.TotalConImpuestos = tc;

                // ===================================================
                // FORMA DE PAGO DESDE BD
                // ===================================================
                string formaPagoCodigo = "01";
                try
                {
                    DataSet dsFormas = _services.FormaPago.MostrarFormaPago("");
                    if (dsFormas != null && dsFormas.Tables.Count > 0 && dsFormas.Tables[0].Rows.Count > 0)
                    {
                        var tablaFormas = dsFormas.Tables[0];
                        var filaForma = tablaFormas.AsEnumerable()
                            .FirstOrDefault(r =>
                                r["FORMAS"].ToString().Trim()
                                 .Equals(formaPago.Trim(), StringComparison.OrdinalIgnoreCase));

                        if (filaForma != null)
                            formaPagoCodigo = filaForma["CODIGO"].ToString().Trim();
                    }
                }
                catch
                {
                    // se queda "01"
                }

                Pagos pagos = new Pagos();
                Pago pago = new Pago
                {
                    FormaPago = formaPagoCodigo,
                    Total = importeTotal.ToString("0.00", CultureInfo.InvariantCulture)
                };
                pagos.Pago = pago;
                infoFactura.Pagos = pagos;

                // ===================================================
                // INFO ADICIONAL (guardamos bases para el “patch” XML)
                // ===================================================
                InfoAdicional infoAd = new InfoAdicional();
                infoAd.CampoAdicional = new List<CampoAdicional>
        {
            new CampoAdicional { Nombre = "MAIL", Text = correoCliente },
            new CampoAdicional { Nombre = "COMENTARIO", Text = comentario },


        };

                factura.InfoFactura = infoFactura;
                factura.DetallesFactura = detalles;
                factura.InfoAdicional = infoAd;

                return factura;
            }
            catch (Exception ex)
            {
                LogFactura("ERROR GenerarXmlFactura: " + ex);
                return null;
            }
        }

        public ResultadoFirma GuardarYFirmarFacturaXml(
            Factura factura,
            DataRow rowEm)
        {
            ResultadoFirma r = new ResultadoFirma();

            try
            {
                string claveAcceso = factura.InfoTributariaFactura.ClaveAcceso;

                string carpetaXml = Path.Combine(_services.Paths.Facturas, "XML");
                string carpetaXmlFirmado = Path.Combine(_services.Paths.Facturas, "XMLFIRMADOS");

                Directory.CreateDirectory(carpetaXml);
                Directory.CreateDirectory(carpetaXmlFirmado);

                string rutaXmlSinFirmar = Path.Combine(carpetaXml, claveAcceso + ".xml");
                string rutaXmlFirmado = Path.Combine(carpetaXmlFirmado, claveAcceso + ".xml");

                XmlSerializer serializer = new XmlSerializer(typeof(Factura));

                using (StreamWriter writer = new StreamWriter(rutaXmlSinFirmar))
                {
                    serializer.Serialize(writer, factura);
                }

                string nombreFirma = rowEm["UBICACIONARCHIVOP12"].ToString();
                string claveFirma = rowEm["CONTRASENA"].ToString();

                string rutaArchivoP12 = Path.Combine(
                    _services.Paths.General,
                    "FIRMAELECTRONICA",
                    nombreFirma
                );

                Consultas consulta = new Consultas();

                bool firmado = consulta.Firma(
                    rutaArchivoP12,
                    claveFirma,
                    rutaXmlSinFirmar,
                    rutaXmlFirmado
                );

                if (!firmado)
                    throw new Exception("La firma devolvió false sin detalle.");

                r.Exito = true;
                r.Mensaje = "XML generado y firmado correctamente.";
                r.RutaXml = rutaXmlSinFirmar;
                r.RutaXmlFirmado = rutaXmlFirmado;
                r.ClaveAcceso = claveAcceso;

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = ex.ToString();
                return r;
            }
        }

        public ResultadoSecuencial IncrementarSecuencialFactura(string tipoComprobante = "01")
        {
            ResultadoSecuencial r = new ResultadoSecuencial();

            try
            {
                int actualizado = _services.Param.IncrementarSecuencial(tipoComprobante);

                if (actualizado <= 0)
                {
                    r.Exito = false;
                    r.Mensaje = "No se pudo incrementar el secuencial en la base de datos.";
                    return r;
                }

                r.Exito = true;
                r.Mensaje = "Secuencial incrementado correctamente.";
                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error incrementando secuencial: " + ex.Message;
                return r;
            }
        }

        public async Task<ResultadoFinalFactura> ProcesarFacturaElectronicaCompleta(
            DataSet em,
            DataSet pfm,
            DataSet pm,
            DataRow rowCliente,
            DataTable detalleFactura,
            string clienteTexto,
            string lblFacturaElectronica,
            string fechaPago,
            string totalVisual,
            string formaPago,
            string comentario,
            string usuarioActual,
            string ipActual,
            string numeroFacturaPendiente = null
        )
        {
            ResultadoFinalFactura r = new ResultadoFinalFactura();

            const int MAX_REINTENTOS_SECUENCIAL = 2;
            int intento = 0;

            try
            {
                while (intento < MAX_REINTENTOS_SECUENCIAL)
                {
                    intento++;

                    // ==================================================
                    // 0) VALIDACIONES BASE
                    // ==================================================
                    if (em == null || em.Tables[0].Rows.Count == 0)
                        return ErrorFactura(r, "No se pudo cargar EMPRESA.");

                    if (pfm == null || pfm.Tables[0].Rows.Count == 0)
                        return ErrorFactura(r, "No se pudo cargar PARÁMETROS FACTURAS.");

                    if (pm == null || pm.Tables[0].Rows.Count == 0)
                        return ErrorFactura(r, "No se pudo cargar PARÁMETROS.");

                    if (rowCliente == null)
                        return ErrorFactura(r, "No se pudo cargar CLIENTE.");

                    if (detalleFactura == null || detalleFactura.Rows.Count == 0)
                        return ErrorFactura(r, "No existen productos en la factura.");

                    bool vieneDePendiente =
                        !string.IsNullOrWhiteSpace(numeroFacturaPendiente) &&
                        numeroFacturaPendiente.StartsWith("PENDIENTE", StringComparison.OrdinalIgnoreCase);

                    // ==================================================
                    // 1) PREPARAR FACTURA
                    // ==================================================
                    var prep = PrepararFactura(
                        detalleFactura,
                        clienteTexto,
                        formaPago,
                        lblFacturaElectronica,
                        fechaPago,
                        DateTime.Now.ToString("HH:mm:ss")
                    );

                    if (!prep.Exito)
                        return ErrorFactura(r, prep.Mensaje);

                    // ==================================================
                    // 1.1) CONSUMIDOR FINAL (NO ELECTRÓNICA)
                    // ==================================================
                    if (!prep.EsElectronica)
                    {
                        // INSERTAR FACTURA NORMAL (CONSUMIDOR FINAL)
                        var insConsumidor = InsertarFactura(
                            prep.DetalleOriginal,
                            fechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            formaPago,
                            clienteTexto,
                            prep.NumeroFactura,
                            usuarioActual,
                            ipActual
                        );

                        if (!insConsumidor.Exito)
                            return ErrorFactura(r, "Error guardando factura consumidor final: " + insConsumidor.Mensaje);

                        r.Exito = true;
                        r.Autorizado = false;
                        r.EsElectronica = false;
                        r.NumeroFactura = prep.NumeroFactura;
                        r.Mensaje = "Factura de consumidor final registrada correctamente.";
                        return r;

                    }


                    string numeroFacturaReal = prep.NumeroFactura;

                    r.EsElectronica = true;

                    r.NumeroFactura = numeroFacturaReal;
                    r.TB_Venta = prep.TB_Venta;
                    r.TB_Transacciones = prep.TB_Transacciones;

                    // ==================================================
                    // 2) XML
                    // ==================================================
                    var factura = GenerarXmlFactura(
                        em,
                        pfm,
                        pm,
                        rowCliente,
                        prep.DetalleOriginal,
                        numeroFacturaReal,
                        fechaPago,
                        totalVisual,
                        formaPago,
                        comentario
                    );

                    if (factura == null)
                        return ErrorFactura(r, "Error generando XML de factura.");

                    r.SecuencialUsado = factura.InfoTributariaFactura.Secuencial;

                    // ==================================================
                    // 3) FIRMAR
                    // ==================================================
                    await Task.Delay(1500);

                    var firma = GuardarYFirmarFacturaXml(
                        factura,
                        em.Tables[0].Rows[0]
                    );

                    if (!firma.Exito)
                        return ErrorFactura(r, "Error firmando XML: " + firma.Mensaje);

                    r.ClaveAcceso = firma.ClaveAcceso;

                    // ==================================================
                    // 4) RECEPCIÓN SRI
                    // ==================================================
                    var recepcion = await Task.Run(() =>
                        _services.ProcesosGenerales.EnviarRecepcion(
                            firma.RutaXmlFirmado,
                            firma.ClaveAcceso,
                            numeroFacturaReal,
                            "FACTURA",
                            usuarioActual,
                            ipActual
                        )
                    );

                    // --------------------------------------------------
                    // 4.1 SECUENCIAL REPETIDO
                    // --------------------------------------------------
                    if (recepcion.SecuencialRepetido)
                    {
                        IncrementarSecuencialFactura("01");
                        continue;
                    }

                    // --------------------------------------------------
                    // 4.2 ERROR → PENDIENTE###
                    // --------------------------------------------------
                    if (!recepcion.Exito)
                    {
                        string numeroPendiente =
                            !string.IsNullOrWhiteSpace(recepcion.EstadoEspecial)
                                ? recepcion.EstadoEspecial
                                : _services.Facturacion.ObtenerSecuencialError();

                        var insPendiente = InsertarFactura(
                            prep.DetalleOriginal,
                            fechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            formaPago,
                            clienteTexto,
                            numeroPendiente,
                            usuarioActual,
                            ipActual
                        );

                        if (insPendiente.Exito && vieneDePendiente)
                        {
                            try
                            {
                                _services.Facturacion.EliminarPorSecuencial(
                                    numeroFacturaPendiente,
                                    usuarioActual,
                                    ipActual
                                );
                            }
                            catch
                            {
                            }

                            try
                            {
                                _services.Pendientes.Eliminar(
                                    numeroFacturaPendiente,
                                    "FACTURA",
                                    usuarioActual,
                                    ipActual
                                );
                            }
                            catch
                            {
                            }
                        }

                        r.Exito = false;
                        r.Autorizado = false;
                        r.NumeroFactura = numeroPendiente;
                        r.Mensaje = insPendiente.Exito
                            ? "FACTURA ELECTRÓNICA\n\n" +
                              "Registrada como pendiente.\n\n" +
                              "Número: " + numeroPendiente + "\n\n" +
                              recepcion.Mensaje
                            : "FACTURA ELECTRÓNICA\n\n" +
                              "Error SRI y además error al registrar pendiente: " + insPendiente.Mensaje + "\n\n" +
                              recepcion.Mensaje;

                        return r;
                    }

                    // --------------------------------------------------
                    // 4.3 PENDIENTE_AUTORIZACION
                    // --------------------------------------------------
                    if (!string.IsNullOrWhiteSpace(recepcion.EstadoEspecial) &&
                        recepcion.EstadoEspecial.StartsWith("PENDIENTE_AUTORIZACION"))
                    {
                        InsertarFactura(
                            prep.DetalleOriginal,
                            fechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            formaPago,
                            clienteTexto,
                            numeroFacturaReal,
                            usuarioActual,
                            ipActual
                        );

                        IncrementarSecuencialFactura("01");

                        r.Exito = true;
                        r.Autorizado = false;
                        r.Mensaje =
                            "FACTURA ELECTRÓNICA\n\n" +
                            "Documento recibido por el SRI.\n\n" +
                            "Pendiente de autorización.";

                        return r;
                    }

                    // ==================================================
                    // 5) AUTORIZACIÓN
                    // ==================================================
                    var sri = await Task.Run(() =>
                        _services.ProcesosGenerales.ConsultarAutorizacion(
                            firma.RutaXmlFirmado,
                            firma.ClaveAcceso,
                            "FACTURA",
                            numeroFacturaReal,
                            Path.Combine(_services.Paths.Facturas, "XMLAUTORIZADOS"),
                            1,
                            (i, m) => { },
                            (i, m) => { },
                            usuarioActual,
                            ipActual
                        )
                    );

                    if (!sri.Exito)
                    {
                        InsertarFactura(
                            prep.DetalleOriginal,
                            fechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            formaPago,
                            clienteTexto,
                            numeroFacturaReal,
                            usuarioActual,
                            ipActual
                        );
                        IncrementarSecuencialFactura("01");
                        return ErrorFactura(r, sri.Mensaje);
                    }

                    r.RutaXmlAutorizado = sri.RutaXmlAutorizado;

                    // ==================================================
                    // 6) INSERTAR DEFINITIVO
                    // ==================================================
                    try
                    {
                        LogFactura("========== INICIO INSERTAR FACTURA DEFINITIVA ==========");
                        LogFactura("numeroFacturaReal: " + (numeroFacturaReal ?? "NULL"));
                        LogFactura("fechaPago: " + (fechaPago ?? "NULL"));
                        LogFactura("horaActual: " + DateTime.Now.ToString("HH:mm:ss"));
                        LogFactura("formaPago: " + (formaPago ?? "NULL"));
                        LogFactura("clienteTexto: " + (clienteTexto ?? "NULL"));
                        LogFactura("usuarioActual: " + (usuarioActual ?? "NULL"));
                        LogFactura("ipActual: " + (ipActual ?? "NULL"));
                        LogFactura("detalleFactura.Rows.Count: " + (prep.DetalleOriginal != null ? prep.DetalleOriginal.Rows.Count.ToString() : "NULL"));
                        LogFactura("claveAcceso: " + (r.ClaveAcceso ?? "NULL"));
                        LogFactura("rutaXmlAutorizado: " + (r.RutaXmlAutorizado ?? "NULL"));

                        InsertarFactura(
                            prep.DetalleOriginal,
                            fechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            formaPago,
                            clienteTexto,
                            numeroFacturaReal,
                            usuarioActual,
                            ipActual
                        );

                        LogFactura("DESPUES InsertarFactura()");
                        LogFactura("========== FIN INSERTAR FACTURA DEFINITIVA ==========");
                    }
                    catch (Exception exInsert)
                    {
                        LogFactura("ERROR EN INSERTAR FACTURA DEFINITIVA: " + exInsert);

                        // La factura ya fue autorizada por el SRI pero no pudo guardarse en BD.
                        // Se registra como PENDIENTE para que no se pierda.
                        try
                        {
                            _services.Pendientes.Insertar(
                                numeroFacturaReal,
                                r.ClaveAcceso ?? "",
                                firma.RutaXmlFirmado ?? "",
                                DateTime.Now,
                                0,
                                "PENDIENTE",
                                "FACTURA",
                                usuarioActual,
                                ipActual
                            );
                        }
                        catch (Exception exPend)
                        {
                            LogFactura("ERROR registrando pendiente de rescate: " + exPend);
                        }

                        return ErrorFactura(r,
                            "Factura AUTORIZADA por SRI pero error al registrar en BD. " +
                            "Guardada como pendiente para recuperación. " +
                            "ClaveAcceso: " + (r.ClaveAcceso ?? "N/A") + ". " +
                            "Error: " + exInsert.Message);
                    }

                    // ==================================================
                    // 7) PDF
                    // ==================================================
                    var pdf = await Task.Run(() =>
                        _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                            sri.XmlAutorizado,
                            em.Tables[0].Rows[0],
                            Path.Combine(_services.Paths.Facturas, "PDF"),
                            (gen, carpeta, xml, logo) =>
                                gen.GeneracionRideFacturaSRI(carpeta, xml, logo)
                        )
                    );

                    if (!pdf.Exito)
                        return ErrorFactura(r, pdf.Mensaje);

                    r.RutaPdf = pdf.RutaPdf;

                    // ==================================================
                    // 8) CORREO
                    // ==================================================
                    var solicitudCorreo = new SolicitudCorreoDocumento
                    {
                        TipoDocumento = "FACTURA",
                        NumeroDocumento = numeroFacturaReal,
                        RutaPdf = r.RutaPdf,
                        RutaXmlAutorizado = r.RutaXmlAutorizado
                    };

                    var correo = await Task.Run(() =>
                        _services.ProcesosGenerales.EnviarCorreoDocumento(
                            solicitudCorreo,
                            rowCliente,
                            "FACTURA",
                            usuarioActual,
                            ipActual
                        )
                    );

                    r.EnvioCorreoExitoso = correo.Exito;

                    // ==================================================
                    // 9) INCREMENTAR SECUENCIAL FINAL
                    // ==================================================
                    IncrementarSecuencialFactura("01");

                    // ==================================================
                    // 10) LIMPIAR XML TRANSITORIOS
                    // ==================================================
                    _services.ProcesosGenerales.LimpiarXmlTransitorios(
                        firma.RutaXml,
                        firma.RutaXmlFirmado,
                        r.RutaXmlAutorizado);

                    r.Exito = true;
                    r.Autorizado = true;
                    r.Mensaje = vieneDePendiente
                        ? "Factura pendiente autorizada correctamente."
                        : "Factura electrónica autorizada correctamente.";

                    return r;
                }

                return ErrorFactura(r, "No se pudo generar la factura por conflictos de secuencial.");
            }
            catch (Exception ex)
            {
                return ErrorFactura(r, "Error general FACTURA: " + ex.Message);
            }
        }

        private void LogFactura(string mensaje)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string carpeta = Path.Combine(baseDir, "logs");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                string archivo = Path.Combine(carpeta, "factura_debug.log");
                File.AppendAllText(
                    archivo,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + mensaje + Environment.NewLine
                );
            }
            catch
            {
            }
        }




        public ResultadoFinalFactura FacturaPendienteAutorizacion(
            string numeroFactura,
            string claveAcceso,
            string rutaXmlFirmado,
            DataRow rowEmpresa,
            DataRow rowCliente,
            string usuarioActual,
            string ipActual
        )
        {
            ResultadoFinalFactura r = new ResultadoFinalFactura
            {
                NumeroFactura = numeroFactura,
                ClaveAcceso = claveAcceso
            };

            try
            {
                // ===============================================================
                // 1) CONSULTAR AUTORIZACIÓN SRI
                // ===============================================================
                var sri = _services.ProcesosGenerales.ConsultarAutorizacion(
                    rutaXmlFirmado,
                    claveAcceso,
                    "FACTURA",
                    numeroFactura,
                    Path.Combine(_services.Paths.Facturas, "XMLAUTORIZADOS"),
                    3,
                    (i, m) => { },   // sigue pendiente
                    (i, m) => { },   // error final
                    usuarioActual,
                    ipActual
                );

                // ===============================================================
                // 2) AÚN NO AUTORIZADO
                // ===============================================================
                if (!sri.Exito)
                {
                    r.Exito = true;
                    r.Autorizado = false;
                    r.Mensaje = "La factura aún no está autorizada por el SRI." +
                                (string.IsNullOrWhiteSpace(sri.Mensaje) ? "" : Environment.NewLine + sri.Mensaje);
                    return r;
                }

                // ===============================================================
                // 3) XML AUTORIZADO YA DISPONIBLE
                // ===============================================================
                r.RutaXmlAutorizado = sri.RutaXmlAutorizado;

                // ===============================================================
                // 4) GENERAR PDF
                // ===============================================================
                var pdf = _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                    sri.XmlAutorizado,
                    rowEmpresa,
                    Path.Combine(_services.Paths.Facturas, "PDF"),
                    (gen, carpeta, xml, logo) =>
                        gen.GeneracionRideFacturaSRI(carpeta, xml, logo)
                );

                if (!pdf.Exito)
                {
                    r.Exito = false;
                    r.Mensaje = "Factura autorizada, pero no se pudo generar el PDF.";
                    return r;
                }

                r.RutaPdf = pdf.RutaPdf;

                // ===============================================================
                // 5) ENVIAR CORREO
                // ===============================================================
                var solicitudCorreo = new SolicitudCorreoDocumento
                {
                    TipoDocumento = "FACTURA",
                    NumeroDocumento = $"{numeroFactura}",
                    RutaPdf = pdf.RutaPdf,
                    RutaXmlAutorizado = sri.RutaXmlAutorizado
                };

                var correo = _services.ProcesosGenerales.EnviarCorreoDocumento(
                    solicitudCorreo,
                    rowCliente,
                    "FACTURA",
                    usuarioActual,
                    ipActual
                );

                r.EnvioCorreoExitoso = correo.Exito;


                // ===============================================================
                // 6) ELIMINAR DE s.Pendientes SOLO SI CORREO OK
                // ===============================================================
                if (correo.Exito)
                {
                    try
                    {
                        _services.Pendientes.Eliminar(
                            numeroFactura,
                            solicitudCorreo.TipoDocumento,
                            usuarioActual,
                            ipActual
                        );
                    }
                    catch
                    {
                        // No rompemos el proceso por limpieza
                    }
                }

                // ===============================================================
                // 7) LIMPIAR XML TRANSITORIOS
                // ===============================================================
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

                // ===============================================================
                // FINAL
                // ===============================================================
                r.Exito = true;
                r.Mensaje = correo.Exito
                    ? "Factura autorizada. PDF generado y correo enviado."
                    : "Factura autorizada. PDF generado, pero no se pudo enviar el correo.";

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error al continuar factura pendiente: " + ex.Message;
                return r;
            }
        }




        public async Task<ResultadoPDF> ProcesarFacturaPreview(
            DataSet em,
            DataSet pfm,
            DataSet pm,
            DataRow rowCliente,
            DataTable detalleFactura,
            string clienteTexto,
            string lblFacturaElectronica,
            string fechaPago,
            string totalVisual,
            string formaPago,
            string comentario
        )
        {
            ResultadoPDF r = new ResultadoPDF();

            try
            {
                // ==========================================
                // 1) PREPARAR FACTURA
                // ==========================================
                var prep = PrepararFactura(
                    detalleFactura,
                    clienteTexto,
                    formaPago,
                    lblFacturaElectronica,
                    fechaPago,
                    DateTime.Now.ToString("HH:mm:ss")
                );

                if (!prep.Exito)
                {
                    r.Exito = false;
                    r.Mensaje = prep.Mensaje;
                    return r;
                }

                if (!prep.EsElectronica)
                {
                    r.Exito = false;
                    r.Mensaje = "Solo aplica para factura electrónica.";
                    return r;
                }

                string numeroFacturaReal = prep.NumeroFactura;

                // ==========================================
                // 2) GENERAR XML (OBJETO FACTURA)
                // ==========================================
                var factura = GenerarXmlFactura(
                    em,
                    pfm,
                    pm,
                    rowCliente,
                    prep.DetalleOriginal,
                    numeroFacturaReal,
                    fechaPago,
                    totalVisual,
                    formaPago,
                    comentario
                );

                if (factura == null)
                {
                    r.Exito = false;
                    r.Mensaje = "Error generando XML.";
                    return r;
                }

                // ==========================================
                // 3) CONVERTIR A XDOCUMENT (CORRECTO)
                // ==========================================
                XDocument xmlDoc = ConvertirFacturaAXDocument(factura);

                // ==========================================
                // 4) GENERAR PDF OFFLINE (SIN FIRMAR)
                // ==========================================
                string carpeta = Path.Combine(_services.Paths.Facturas, "PDFPREVIEW");
                Directory.CreateDirectory(carpeta);
                _services.ProcesosGenerales.LimpiarPreviewsAntiguos(carpeta, TimeSpan.FromHours(2));

                string rutaLogo = Path.Combine(
                    _services.Paths.General,
                    "LOGOFACTURA",
                    em.Tables[0].Rows[0]["IMAGEN"].ToString()
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

                    string rutaPdf = gen.GeneracionRideFacturaSRI_OfflineSinAutorizacion(
                        carpeta,
                        xmlDoc,      // ✅ AHORA ES XDOCUMENT
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

        //API

        public async Task<ResultadoFinalFactura> ProcesarFacturaConsumidorFinalAsync(
            string fechaVieja,
            string horaVieja,
            string numeroViejo,
            string cedulaNueva,
            string usuarioActual,
            string ipActual
        )
        {
            ResultadoFinalFactura r = new ResultadoFinalFactura();

            try
            {
                // ==================================================
                // 0) VALIDACIONES BASE
                // ==================================================
                if (string.IsNullOrWhiteSpace(fechaVieja))
                    return ErrorFactura(r, "La fecha de la factura es obligatoria.");

                if (string.IsNullOrWhiteSpace(horaVieja))
                    return ErrorFactura(r, "La hora de la factura es obligatoria.");

                if (string.IsNullOrWhiteSpace(numeroViejo))
                    return ErrorFactura(r, "El número de factura es obligatorio.");

                if (string.IsNullOrWhiteSpace(cedulaNueva))
                    return ErrorFactura(r, "La cédula del cliente es obligatoria.");

                numeroViejo = numeroViejo.Trim().ToUpperInvariant();
                cedulaNueva = cedulaNueva.Trim();

                bool esConsumidorFinal =
                    numeroViejo.StartsWith("FINAL", StringComparison.OrdinalIgnoreCase);

                if (!esConsumidorFinal)
                    return ErrorFactura(r, "La factura seleccionada no corresponde a consumidor final.");

                // ==================================================
                // 1) CONSULTAR FACTURA VIEJA
                // ==================================================
                DataSet facturaCompleta =
                    _services.Facturacion.ConsultarFacturaConsumidorFinal(
                        fechaVieja,
                        horaVieja,
                        numeroViejo
                    );

                if (facturaCompleta == null ||
                    facturaCompleta.Tables.Count == 0 ||
                    facturaCompleta.Tables[0].Rows.Count == 0)
                {
                    return ErrorFactura(r, "No se encontraron productos de la factura vieja.");
                }

                // ==================================================
                // 2) ARMAR DETALLE
                // ==================================================
                DataTable detalle = new DataTable();
                detalle.Columns.Add("CANTIDAD");
                detalle.Columns.Add("PRODUCTO");
                detalle.Columns.Add("VALOR");
                detalle.Columns.Add("TOTAL");

                string formaPago = "";
                decimal totalFactura = 0m;

                foreach (DataRow row in facturaCompleta.Tables[0].Rows)
                {
                    decimal totalLinea = 0m;
                    decimal.TryParse(row["TOTAL"]?.ToString(), out totalLinea);

                    detalle.Rows.Add(
                        row["CANTIDAD"]?.ToString() ?? "0",
                        row["PRODUCTO"]?.ToString() ?? "",
                        totalLinea.ToString("0.00"),
                        totalLinea.ToString("0.00")
                    );

                    formaPago = row["FORMADEPAGO"]?.ToString() ?? "";

                    totalFactura += totalLinea;
                }

                // ==================================================
                // 3) CARGAR PARÁMETROS
                // ==================================================
                var em = _services.Empresa.MostrarEmpresa(usuarioActual);
                var pfm = _services.ParamFactura.ConsultarNombre(usuarioActual);
                var pm = _services.Param.ConsultarPorTipo("01");

                if (em == null || em.Tables.Count == 0 || em.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se pudo cargar EMPRESA.");

                if (pfm == null || pfm.Tables.Count == 0 || pfm.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se pudo cargar PARÁMETROS FACTURAS.");

                if (pm == null || pm.Tables.Count == 0 || pm.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se pudo cargar PARÁMETROS.");

                // ==================================================
                // 4) BUSCAR CLIENTE REAL
                // ==================================================
                var dsCli = _services.Cliente.ConsultarCedula(cedulaNueva);

                if (dsCli == null ||
                    dsCli.Tables.Count == 0 ||
                    dsCli.Tables[0].Rows.Count == 0)
                {
                    return ErrorFactura(r, "Cliente no encontrado.");
                }

                DataRow rowCliente = dsCli.Tables[0].Rows[0];

                // ==================================================
                // 5) FECHA NUEVA DE PROCESO
                // ==================================================
                string fechaNueva = DateTime.Now.ToString("dd/MM/yyyy");

                // ==================================================
                // 6) LLAMAR AL PROCESO GENERAL
                // ==================================================
                var resultado = await ProcesarFacturaElectronicaCompleta(
                    em,
                    pfm,
                    pm,
                    rowCliente,
                    detalle,
                    cedulaNueva,
                    "SI",
                    fechaNueva,
                    totalFactura.ToString("0.00"),
                    formaPago,
                    "Gracias por su compra!",
                    usuarioActual,
                    ipActual,
                    numeroViejo
                );

                // ==================================================
                // 7) ELIMINAR FACTURA VIEJA FINAL...
                // ==================================================
                // Igual que en escritorio:
                // - si quedó pendiente de autorización => borrar vieja
                // - si quedó autorizada => borrar vieja
                // - si hubo error general => NO borrar vieja
                if (resultado.Exito && !resultado.Autorizado)
                {
                    _services.Facturacion.EliminarPorSecuencial(
                        numeroViejo,
                        usuarioActual,
                        ipActual
                    );
                }

                if (resultado.Exito && resultado.Autorizado)
                {
                    _services.Facturacion.EliminarPorSecuencial(
                        numeroViejo,
                        usuarioActual,
                        ipActual
                    );
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return ErrorFactura(r, "Error procesando factura de consumidor final: " + ex.Message);
            }
        }

        // API — Reprocesar factura PENDIENTE normal (igual que escritorio ProcesarPendienteAsync)
        public async Task<ResultadoFinalFactura> ReprocesarFacturaPendienteAsync(
            string fechaVieja,
            string horaVieja,
            string numeroViejo,
            string cedulaCliente,
            string usuarioActual,
            string ipActual
        )
        {
            ResultadoFinalFactura r = new ResultadoFinalFactura();

            try
            {
                if (string.IsNullOrWhiteSpace(numeroViejo))
                    return ErrorFactura(r, "Número de factura obligatorio.");

                if (string.IsNullOrWhiteSpace(cedulaCliente))
                    return ErrorFactura(r, "Cédula del cliente obligatoria.");

                if (string.IsNullOrWhiteSpace(fechaVieja))
                    return ErrorFactura(r, "Fecha obligatoria.");

                if (string.IsNullOrWhiteSpace(horaVieja))
                    return ErrorFactura(r, "Hora obligatoria.");

                // 1) CONSULTAR FACTURA DESDE LA BD
                var dsFactura = _services.Facturacion.ConsultarFacturaNormal(
                    fechaVieja, horaVieja, cedulaCliente, numeroViejo
                );

                if (dsFactura == null || dsFactura.Tables.Count == 0 || dsFactura.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se encontró el detalle de la factura.");

                // 2) ARMAR DETALLE
                DataTable detalle = new DataTable();
                detalle.Columns.Add("CANTIDAD");
                detalle.Columns.Add("PRODUCTO");
                detalle.Columns.Add("VALOR");
                detalle.Columns.Add("TOTAL");

                string formaPago = "";
                decimal totalFactura = 0m;

                foreach (DataRow row in dsFactura.Tables[0].Rows)
                {
                    decimal totalLinea = 0m;
                    decimal.TryParse(row["TOTAL"]?.ToString(), out totalLinea);

                    detalle.Rows.Add(
                        row["CANTIDAD"]?.ToString() ?? "0",
                        row["PRODUCTO"]?.ToString() ?? "",
                        totalLinea.ToString("0.00"),
                        totalLinea.ToString("0.00")
                    );

                    formaPago = row["FORMADEPAGO"]?.ToString() ?? "";
                    totalFactura += totalLinea;
                }

                // 3) PARÁMETROS
                var em = _services.Empresa.MostrarEmpresa(usuarioActual);
                var pfm = _services.ParamFactura.ConsultarNombre(usuarioActual);
                var pm = _services.Param.ConsultarPorTipo("01");

                if (em == null || em.Tables.Count == 0 || em.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se pudo cargar EMPRESA.");

                if (pfm == null || pfm.Tables.Count == 0 || pfm.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se pudo cargar PARÁMETROS FACTURAS.");

                if (pm == null || pm.Tables.Count == 0 || pm.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "No se pudo cargar PARÁMETROS.");

                // 4) CLIENTE
                var dsCli = _services.Cliente.ConsultarCedula(cedulaCliente);

                if (dsCli == null || dsCli.Tables.Count == 0 || dsCli.Tables[0].Rows.Count == 0)
                    return ErrorFactura(r, "Cliente no encontrado.");

                DataRow rowCliente = dsCli.Tables[0].Rows[0];

                // 5) PROCESO ELECTRÓNICO COMPLETO
                var resultado = await ProcesarFacturaElectronicaCompleta(
                    em,
                    pfm,
                    pm,
                    rowCliente,
                    detalle,
                    cedulaCliente,
                    "SI",
                    DateTime.Now.ToString("dd/MM/yyyy"),
                    totalFactura.ToString("0.00"),
                    formaPago,
                    "Gracias por su compra!",
                    usuarioActual,
                    ipActual,
                    numeroViejo   // ← CLAVE: reemplaza el pendiente
                );

                // 6) LIMPIAR PENDIENTE VIEJO
                if (!resultado.Autorizado || (resultado.Exito && resultado.Autorizado))
                {
                    _services.Facturacion.EliminarPorSecuencial(
                        numeroViejo,
                        usuarioActual,
                        ipActual
                    );
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return ErrorFactura(r, "Error reprocesando factura pendiente: " + ex.Message);
            }
        }

        public ResultadoFinalFactura FacturaPendienteAutorizacionDesdeApi(
    string numeroFactura,
    string fechaFactura,
    string usuarioActual,
    string ipActual
)
        {
            try
            {
                if (!_services.ProcesosGenerales.PrepararPendienteDocumento(
                    "FACTURA",
                    numeroFactura,
                    fechaFactura,
                    usuarioActual,
                    out string claveAcceso,
                    out string rutaXmlFirmado,
                    out DataRow rowEmpresa,
                    out DataRow rowCliente,
                    out string mensajeError
                ))
                {
                    return new ResultadoFinalFactura
                    {
                        Exito = false,
                        NumeroFactura = numeroFactura,
                        Mensaje = mensajeError
                    };
                }

                return FacturaPendienteAutorizacion(
                    numeroFactura,
                    claveAcceso,
                    rutaXmlFirmado,
                    rowEmpresa,
                    rowCliente,
                    usuarioActual,
                    ipActual
                );
            }
            catch (Exception ex)
            {
                return new ResultadoFinalFactura
                {
                    Exito = false,
                    NumeroFactura = numeroFactura,
                    Mensaje = "Error al autorizar factura pendiente: " + ex.Message
                };
            }
        }


        public static class FacturacionQueue
        {
            private static readonly BlockingCollection<Action> cola =
                new BlockingCollection<Action>();

            static FacturacionQueue()
            {
                Thread worker = new Thread(() =>
                {
                    foreach (var trabajo in cola.GetConsumingEnumerable())
                    {
                        try
                        {
                            // Evita colisiones con SRI
                            Thread.Sleep(3500);
                            trabajo();
                        }
                        catch
                        {
                            // Nunca romper la cola
                        }
                        finally
                        {
                            Thread.Sleep(1500);
                        }
                    }
                });

                worker.IsBackground = true;
                worker.Start();
            }

            public static void Enqueue(Action tarea)
            {
                if (tarea != null)
                    cola.Add(tarea);
            }
        }

        private static ResultadoFinalFactura ErrorFactura(ResultadoFinalFactura r, string mensaje)
        {
            r.Exito = false;
            r.Mensaje = mensaje;
            return r;
        }


        public class FacturacionQueueAsync
        {
            private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
            private readonly Queue<Func<Task>> _queue = new Queue<Func<Task>>();
            private readonly object _lock = new object();
            private bool _running = false;

            public bool HayTrabajoPendiente
            {
                get { lock (_lock) { return _running || _queue.Count > 0; } }
            }

            public void Enqueue(Func<Task> work)
            {
                lock (_lock)
                {
                    _queue.Enqueue(work);
                    _signal.Release();

                    if (!_running)
                    {
                        _running = true;
                        _ = Task.Run(ProcessQueueAsync);
                    }
                }
            }

            private async Task ProcessQueueAsync()
            {
                while (true)
                {
                    await _signal.WaitAsync();

                    Func<Task> work = null;

                    lock (_lock)
                    {
                        if (_queue.Count > 0)
                            work = _queue.Dequeue();
                        else
                        {
                            _running = false;
                            return;
                        }
                    }

                    try
                    {
                        await work();
                    }
                    catch (Exception ex)
                    {

                    }

                    lock (_lock)
                    {
                        if (_queue.Count == 0)
                        {
                            _running = false;
                            return;
                        }
                    }
                }
            }
        }

        private static XDocument ConvertirFacturaAXDocument(xmlFactura.Factura factura)
        {
            var serializer = new XmlSerializer(typeof(xmlFactura.Factura));

            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, factura);
                ms.Position = 0;
                return XDocument.Load(ms);
            }
        }

    }

}

