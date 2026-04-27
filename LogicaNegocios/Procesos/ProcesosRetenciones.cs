using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using xmlRetencion;

namespace LogicaNegocios.Procesos
{
    public class ProcesosRetenciones
    {
        private readonly AppServices _services;

        public ProcesosRetenciones(AppServices services
        )
        {
            _services = services;
        }

        //Helpers DTO
        public class ResultadoRetencion
        {
            public bool Exito { get; set; }
            public string Mensaje { get; set; }

            public string NumeroRetencion { get; set; }
            public string NumeroFactura { get; set; }

            public string TotalBaseImponible { get; set; }
            public string TotalRetencionRenta { get; set; }
            public string TotalRetencionIva { get; set; }
            public string TotalRetenido { get; set; }

            public DataTable TB_DetalleRetencion { get; set; }
        }

        public class ResultadoFinalRetencion
        {
            public bool Exito { get; set; }
            public string Mensaje { get; set; }

            public string NumeroRetencion { get; set; }
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

        public class DtoRetencionManual
        {
            // ======================
            // PROVEEDOR
            // ======================
            public string TipoIdentificacion { get; set; }   // 04, 05, 06
            public string Identificacion { get; set; }
            public string RazonSocial { get; set; }
            public string Direccion { get; set; }
            public string Telefono { get; set; }
            public string Correo { get; set; }

            public string TipoPersona { get; set; }           // PN / PJ
            public bool EsRimpe { get; set; }
            public string TipoRimpe { get; set; }
            public bool EsProfesional { get; set; }
            public bool EsArrendador { get; set; }

            // ======================
            // FACTURA PROVEEDOR
            // ======================
            public string NumeroFactura { get; set; }
            public string NumeroRetencion { get; set; }
            public DateTime FechaFactura { get; set; }
            public decimal BaseImponible { get; set; }
            public decimal Iva { get; set; }
            public decimal Total { get; set; }

            // ======================
            // OPERACIÓN
            // ======================
            public string TipoOperacion { get; set; }
        }


        //Metodos

        public ResultadoRetencion PrepararRetencion(
            DtoRetencionManual datos,
            DataTable conceptosRetencion,
            string numeroRetencionForzado = null
        )
        {
            var r = new ResultadoRetencion();

            try
            {
                // ==================================================
                // 1) VALIDACIONES
                // ==================================================
                if (datos == null)
                    throw new Exception("Datos de la retención vacíos.");

                if (string.IsNullOrWhiteSpace(datos.NumeroFactura))
                    throw new Exception("Número de factura del proveedor vacío.");

                if (conceptosRetencion == null || conceptosRetencion.Rows.Count == 0)
                    throw new Exception("No existen conceptos de retención.");

                // ==================================================
                // 2) NÚMERO DE RETENCIÓN
                // ==================================================
                string numeroRetencion =
                    !string.IsNullOrWhiteSpace(numeroRetencionForzado)
                        ? numeroRetencionForzado
                        : _services.Param.ObtenerSecuencialFormateado("07");

                // ==================================================
                // 3) CALCULAR TOTALES + DETALLE NORMALIZADO
                // ==================================================
                decimal totalBase = 0m;
                decimal totalRenta = 0m;
                decimal totalIva = 0m;

                DataTable TB_RET = new DataTable();
                TB_RET.Columns.Add("TIPOIMPUESTO");
                TB_RET.Columns.Add("CODIGOIMPUESTO");
                TB_RET.Columns.Add("BASEIMPONIBLE");
                TB_RET.Columns.Add("PORCENTAJERETENCION");
                TB_RET.Columns.Add("VALORRETENIDO");

                bool tieneValorRetenido =
                    conceptosRetencion.Columns.Contains("VALORRETENIDO");

                foreach (DataRow row in conceptosRetencion.Rows)
                {
                    string tipoImp = row["TIPOIMPUESTO"].ToString().Trim().ToUpperInvariant();
                    string codigo = row["CODIGOIMPUESTO"].ToString().Trim();

                    decimal baseImp = Convert.ToDecimal(row["BASEIMPONIBLE"]);
                    decimal porcentaje = Convert.ToDecimal(row["PORCENTAJERETENCION"]);

                    decimal valorRetenido =
                        tieneValorRetenido && row["VALORRETENIDO"] != DBNull.Value
                            ? Convert.ToDecimal(row["VALORRETENIDO"])
                            : Math.Round(baseImp * porcentaje / 100m, 2);

                    totalBase += baseImp;

                    if (tipoImp == "RENTA") totalRenta += valorRetenido;
                    if (tipoImp == "IVA") totalIva += valorRetenido;

                    TB_RET.Rows.Add(
                        tipoImp,
                        codigo,
                        baseImp.ToString("0.00", CultureInfo.InvariantCulture),
                        porcentaje.ToString("0.00", CultureInfo.InvariantCulture),
                        valorRetenido.ToString("0.00", CultureInfo.InvariantCulture)
                    );
                }

                // ==================================================
                // 4) RESPUESTA
                // ==================================================
                r.Exito = true;
                r.NumeroRetencion = numeroRetencion;
                r.NumeroFactura = datos.NumeroFactura;
                r.TotalBaseImponible = totalBase.ToString("0.00", CultureInfo.InvariantCulture);
                r.TotalRetencionRenta = totalRenta.ToString("0.00", CultureInfo.InvariantCulture);
                r.TotalRetencionIva = totalIva.ToString("0.00", CultureInfo.InvariantCulture);
                r.TotalRetenido = (totalRenta + totalIva).ToString("0.00", CultureInfo.InvariantCulture);
                r.TB_DetalleRetencion = TB_RET;

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error PrepararRetencion: " + ex.Message;
                return r;
            }
        }


        public bool InsertarRetencion(
    DtoRetencionManual datos,
    ResultadoRetencion prep,
    DataRow rowPfm,
    string numeroRetencion,
    string claveAcceso,
    string estado,
    string usuario,
    string ip
)
        {
            try
            {
                // ==================================================
                // 1) INSERTAR ENCABEZADO
                // ==================================================
                int okEnc = _services.Retencion.Insertar(
                    NumeroRetencion: numeroRetencion,
                    ClaveAcceso: claveAcceso ?? "",
                    FechaEmision: DateTime.Now.ToString("dd/MM/yyyy"),
                    HoraEmision: DateTime.Now.ToString("HH:mm:ss"),
                    Ambiente: rowPfm["AMBIENTE"].ToString(),
                    Estado: estado,
                    Codigo: "07",
                    TipoEmision: rowPfm["TIPOEMISION"].ToString(),

                    NumeroFactura: datos.NumeroFactura,
                    ClaveAccesoFactura: "",
                    FechaFactura: datos.FechaFactura.ToString("dd/MM/yyyy"),

                    SujetoRetenido: datos.RazonSocial,
                    IdentificacionSujeto: datos.Identificacion,
                    TipoIdentificacionSujeto: datos.TipoIdentificacion,
                    DireccionSujeto: datos.Direccion,
                    RegimenSujeto: datos.TipoRimpe ?? "",

                    TotalBaseImponible: prep.TotalBaseImponible,
                    TotalRetencionRenta: prep.TotalRetencionRenta,
                    TotalRetencionIva: prep.TotalRetencionIva,
                    TotalRetenido: prep.TotalRetenido,

                    Observaciones: "RETENCIÓN ELECTRÓNICA",
                    Usuario: usuario,
                    IP: ip
                );

                if (okEnc <= 0)
                    return false;

                // ==================================================
                // 2) INSERTAR DETALLE
                // ==================================================
                foreach (DataRow d in prep.TB_DetalleRetencion.Rows)
                {
                    _services.Retencion.InsertarDetalle(
                        NumeroRetencion: prep.NumeroRetencion,
                        TipoImpuesto: d["TIPOIMPUESTO"].ToString(),
                        CodigoImpuesto: d["CODIGOIMPUESTO"].ToString(),
                        BaseImponible: d["BASEIMPONIBLE"].ToString(),
                        PorcentajeRetencion: d["PORCENTAJERETENCION"].ToString(),
                        ValorRetenido: d["VALORRETENIDO"].ToString(),
                        TipoOperacion: datos.TipoOperacion
                    );
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool GenerarXmlRetencion(
            string numeroRetencion,
            string numeroFactura,
            string fechaFactura,
            DataTable TB_RET,              // Detalle de retención
            DataRow rowCliente,
            DataRow rowEmpresa,
            DataRow rowPfm,
            DataRow rowPmRet,              // Parámetros doc 07
            DtoRetencionManual datos,      // 🔒 FUENTE ÚNICA DEL SUJETO RETENIDO
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
                // ======================================================
                // 1. VALIDACIONES BÁSICAS
                // ======================================================
                if (string.IsNullOrWhiteSpace(numeroRetencion))
                {
                    error = "Número de retención vacío.";
                    return false;
                }

                if (TB_RET == null || TB_RET.Rows.Count == 0)
                {
                    error = "No existe detalle de retención.";
                    return false;
                }

                if (datos == null)
                {
                    error = "Datos del sujeto retenido vacíos.";
                    return false;
                }

                // ======================================================
                // 2. PARSEAR ESTABLECIMIENTO / PTO EMISIÓN / SECUENCIAL
                // ======================================================
                string estab = rowPfm["ESTAB"].ToString().Trim();
                string ptoEmi = rowPfm["PUNTOEMISION"].ToString().Trim();
                string secuencial = rowPmRet["SECUENCIAL"].ToString().Trim();

                string[] partes = numeroRetencion.Split('-');
                if (partes.Length == 3)
                {
                    estab = partes[0];
                    ptoEmi = partes[1];
                    secuencial = partes[2];
                }

                secuencial = secuencial.PadLeft(9, '0');

                // ======================================================
                // 3. NORMALIZAR NÚMERO DE FACTURA (SUSTENTO)
                // ======================================================
                string numDocSustento = numeroFactura.Replace("-", "").Trim();

                if (numDocSustento.Length != 15 || !numDocSustento.All(char.IsDigit))
                {
                    error = $"Número de factura inválido para SRI: {numeroFactura}";
                    return false;
                }

                // ======================================================
                // 4. GENERAR CLAVE DE ACCESO (DOC 07)
                // ======================================================
                string codDoc = "07";
                string ruc = rowEmpresa["RUC"].ToString().Trim();
                string ambiente = rowPfm["AMBIENTE"].ToString().Trim();
                string tipoEmision = rowPfm["TIPOEMISION"].ToString().Trim();

                string codigoNumerico = rowPmRet["CODIGONUMERICO"].ToString().Trim();
                if (string.IsNullOrWhiteSpace(codigoNumerico))
                    codigoNumerico = "12345678";

                string fechaEmision = DateTime.Now.ToString("dd/MM/yyyy");
                string fechaClave = DateTime.Now.ToString("ddMMyyyy");

                CFuncionesComprobantesElectronicos cfunciones =
                    new CFuncionesComprobantesElectronicos();

                string claveAccesoGenerada = cfunciones.ClaveAcceso(
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

                // ======================================================
                // 5. CONSTRUIR OBJETO TIPADO ComprobanteRetencion
                // ======================================================
                string contribuyenteEspecial = "";
                if (rowPfm.Table != null && rowPfm.Table.Columns.Contains("CONTRIBUYENTEESPECIAL"))
                    contribuyenteEspecial = (rowPfm["CONTRIBUYENTEESPECIAL"]?.ToString() ?? "").Trim();

                DateTime dtFactura = DateTime.ParseExact(
                    fechaFactura,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );

                var impuestosRet = new List<ImpuestoRetencion>();
                foreach (DataRow r in TB_RET.Rows)
                {
                    string tipo = (r["TIPOIMPUESTO"]?.ToString() ?? "").Trim().ToUpperInvariant();
                    string codigo = (r["CODIGOIMPUESTO"]?.ToString() ?? "").Trim();
                    decimal baseImp = ToDec(r["BASEIMPONIBLE"]);
                    decimal porc = ToDec(r["PORCENTAJERETENCION"]);
                    decimal valor = ToDec(r["VALORRETENIDO"]);

                    impuestosRet.Add(new ImpuestoRetencion
                    {
                        Codigo = tipo == "RENTA" ? "1" : "2",
                        CodigoRetencion = codigo,
                        BaseImponible = baseImp.ToString("F2", CultureInfo.InvariantCulture),
                        PorcentajeRetener = porc.ToString("F2", CultureInfo.InvariantCulture),
                        ValorRetenido = valor.ToString("F2", CultureInfo.InvariantCulture),
                        CodDocSustento = "01",
                        NumDocSustento = numDocSustento,
                        FechaEmisionDocSustento = fechaFactura
                    });
                }

                var compRetencion = new ComprobanteRetencion
                {
                    Id = "comprobante",
                    Version = "1.0.0",
                    InfoTributaria = new InfoTributariaRet
                    {
                        Ambiente = ambiente,
                        TipoEmision = tipoEmision,
                        RazonSocial = rowEmpresa["NOMBRE"].ToString(),
                        NombreComercial = rowEmpresa["NOMBRE"].ToString(),
                        Ruc = ruc,
                        ClaveAcceso = claveAccesoGenerada,
                        CodDoc = "07",
                        Estab = estab,
                        PtoEmi = ptoEmi,
                        Secuencial = secuencial,
                        DirMatriz = rowEmpresa["DIRECCION"].ToString()
                    },
                    InfoCompRetencion = new InfoCompRetencion
                    {
                        FechaEmision = fechaEmision,
                        DirEstablecimiento = rowEmpresa["DIRECCION"].ToString(),
                        ContribuyenteEspecial = contribuyenteEspecial,
                        ObligadoContabilidad = rowPfm["OBLIGADOCONTABILIDAD"].ToString(),
                        TipoIdentificacionSujetoRetenido = datos.TipoIdentificacion,
                        RazonSocialSujetoRetenido = datos.RazonSocial,
                        IdentificacionSujetoRetenido = datos.Identificacion,
                        PeriodoFiscal = dtFactura.ToString("MM/yyyy")
                    },
                    Impuestos = new ImpuestosRetencion { Impuesto = impuestosRet }
                };

                // ======================================================
                // 6. SERIALIZAR Y GUARDAR XML
                // ======================================================
                string carpeta = Path.Combine(_services.Paths.Retenciones, "XML");
                Directory.CreateDirectory(carpeta);
                rutaXmlGenerado = Path.Combine(carpeta, claveAccesoGenerada + ".xml");

                var serializer = new XmlSerializer(typeof(ComprobanteRetencion));
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                var settings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = false
                };

                using (var writer = XmlWriter.Create(rutaXmlGenerado, settings))
                {
                    serializer.Serialize(writer, compRetencion, ns);
                }

                claveAcceso = claveAccesoGenerada;
                return true;

            }
            catch (Exception ex)
            {
                error = "Error GenerarXmlRetencion: " + ex.Message;
                return false;
            }
        }


        // ===================== helpers locales =====================

        public static decimal ToDec(object v)
        {
            if (v == null || v == DBNull.Value) return 0m;

            if (v is decimal d) return d;
            if (v is double db) return Convert.ToDecimal(db);
            if (v is float f) return Convert.ToDecimal(f);
            if (v is int i) return i;
            if (v is long l) return l;

            string s = v.ToString().Trim();

            if (string.IsNullOrEmpty(s))
                return 0m;

            // 🔑 NORMALIZACIÓN CLAVE
            // siempre convertir a formato invariante
            s = s.Replace(",", ".");

            if (!decimal.TryParse(
                    s,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var result))
            {
                throw new FormatException($"No se pudo convertir a decimal: '{v}'");
            }

            return result;
        }

        public static decimal NormalizarPorcentaje(decimal porc)
        {
            // Regla: el % real debe quedar entre 0 y 100
            // Si viene escalado (175, 1000), lo bajamos hasta rango.
            while (porc > 100m)
                porc = porc / 100m;

            // Si viene como fracción (0.0175) accidental, lo subimos (raro pero posible)
            // (solo si es >0 y menor a 1)
            if (porc > 0m && porc < 1m)
                porc = porc * 100m;

            return Math.Round(porc, 2);
        }

        public static decimal CalcularValorRetenido(decimal baseImp, decimal porc)
        {
            // base * (porc/100)
            return Math.Round(baseImp * porc / 100m, 2, MidpointRounding.AwayFromZero);
        }

        public ResultadoFirma GuardarYFirmarRetencionXml(
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
                    r.Mensaje = "Clave de acceso vacía para firmar Retención.";
                    return r;
                }

                // ============================================
                // RUTAS BASE
                // ============================================

                // XML sin firmar
                string carpetaXml = Path.Combine(_services.Paths.Retenciones, "XML");
                string rutaXmlSinFirmar = Path.Combine(carpetaXml, claveAcceso + ".xml");

                // XML firmado
                string carpetaXmlFirmado = Path.Combine(_services.Paths.Retenciones, "XMLFIRMADOS");
                string rutaXmlFirmado = Path.Combine(carpetaXmlFirmado, claveAcceso + ".xml");

                // Crear carpetas si no existen
                Directory.CreateDirectory(carpetaXml);
                Directory.CreateDirectory(carpetaXmlFirmado);

                // ============================================
                // VALIDAR XML SIN FIRMAR
                // ============================================
                if (!File.Exists(rutaXmlSinFirmar))
                {
                    r.Exito = false;
                    r.Mensaje = "No existe el XML sin firmar de la Retención: " + rutaXmlSinFirmar;
                    return r;
                }

                // ============================================
                // DATOS FIRMA ELECTRÓNICA
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
                // FIRMAR XML (MISMA CLASE QUE FACTURA / NC)
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
                    r.Mensaje = "No se pudo firmar el XML de la Retención.";
                    return r;
                }

                // ============================================
                // RESPUESTA
                // ============================================
                r.Exito = true;
                r.Mensaje = "XML de Retención firmado correctamente.";
                r.RutaXml = rutaXmlSinFirmar;
                r.RutaXmlFirmado = rutaXmlFirmado;
                r.ClaveAcceso = claveAcceso;

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = "Error al firmar XML Retención: " + ex.Message;
                return r;
            }
        }

        public async Task<ResultadoFinalRetencion> ProcesarRetencionElectronicaCompleta(
            DtoRetencionManual datos,
            DataTable conceptosRetencion,
            string usuarioActual,
            string ipActual,
            string numeroRetencionPendiente = null
        )
        {
            var r = new ResultadoFinalRetencion();

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
                    if (datos == null)
                        return Error(r, "Datos de la retención vacíos.");

                    if (conceptosRetencion == null || conceptosRetencion.Rows.Count == 0)
                        return Error(r, "No existen conceptos de retención.");

                    if (string.IsNullOrWhiteSpace(datos.NumeroFactura))
                        return Error(r, "Número de factura vacío.");

                    bool vieneDePendiente =
                        !string.IsNullOrWhiteSpace(numeroRetencionPendiente) &&
                        numeroRetencionPendiente.StartsWith("PENDIENTE", StringComparison.OrdinalIgnoreCase);

                    // ==================================================
                    // 1) SECUENCIAL REAL
                    // ==================================================
                    string numeroRetencionReal = await Task.Run(() =>
                        _services.Param.ObtenerSecuencialFormateado("07")
                    );

                    // ==================================================
                    // 2) EMPRESA Y PARÁMETROS
                    // ==================================================
                    DataSet dsEmp = await Task.Run(() => _services.Empresa.ConsultaNombre(usuarioActual));
                    DataSet dsPfm = await Task.Run(() => _services.ParamFactura.ConsultarNombre(usuarioActual));
                    DataSet dsPmRet = await Task.Run(() => _services.Param.ConsultarPorTipo("07"));

                    if (dsEmp.Tables[0].Rows.Count == 0 ||
                        dsPfm.Tables[0].Rows.Count == 0 ||
                        dsPmRet.Tables[0].Rows.Count == 0)
                        return Error(r, "No se pudieron cargar datos base.");

                    DataRow rowEmpresa = dsEmp.Tables[0].Rows[0];
                    DataRow rowPfm = dsPfm.Tables[0].Rows[0];
                    DataRow rowPmRet = dsPmRet.Tables[0].Rows[0];

                    // ==================================================
                    // 3) SUJETO RETENIDO
                    // ==================================================
                    DataTable dtSujeto = new DataTable();
                    dtSujeto.Columns.Add("NOMBRE");
                    dtSujeto.Columns.Add("CEDULA");
                    dtSujeto.Columns.Add("CORREO");
                    dtSujeto.Columns.Add("DIRECCION");
                    dtSujeto.Rows.Add(datos.RazonSocial, datos.Identificacion, datos.Correo, datos.Direccion);
                    DataRow rowCliente = dtSujeto.Rows[0];

                    // ==================================================
                    // 4) PREPARAR RETENCIÓN
                    // ==================================================
                    var prep = await Task.Run(() =>
                        PrepararRetencion(
                            datos,
                            conceptosRetencion,
                            numeroRetencionReal
                        )
                    );

                    if (!prep.Exito)
                        return Error(r, prep.Mensaje);

                    r.NumeroFactura = prep.NumeroFactura;

                    // ==================================================
                    // 5) XML
                    // ==================================================
                    string claveAcceso = "";
                    string rutaXmlGenerado = "";
                    string errorXml = "";

                    bool xmlOk = GenerarXmlRetencion(
                        prep.NumeroRetencion,
                        datos.NumeroFactura,
                        datos.FechaFactura.ToString("dd/MM/yyyy"),
                        prep.TB_DetalleRetencion,
                        rowCliente,
                        rowEmpresa,
                        rowPfm,
                        rowPmRet,
                        datos,
                        out claveAcceso,
                        out rutaXmlGenerado,
                        out errorXml
                    );

                    if (!xmlOk)
                        return Error(r, errorXml);


                    r.ClaveAcceso = claveAcceso;
                    r.RutaXmlGenerado = rutaXmlGenerado;

                    // ==================================================
                    // 6) FIRMAR
                    // ==================================================
                    var firma = await Task.Run(() =>
                        GuardarYFirmarRetencionXml(
                            claveAcceso,
                            rowEmpresa,
                            usuarioActual,
                            ipActual
                        )
                    );

                    if (!firma.Exito)
                        return Error(r, firma.Mensaje);

                    r.RutaXmlFirmado = firma.RutaXmlFirmado;

                    // ==================================================
                    // 7) RECEPCIÓN SRI
                    // ==================================================
                    var recepcion = await Task.Run(() =>
                        _services.ProcesosGenerales.EnviarRecepcion(
                            firma.RutaXmlFirmado,
                            claveAcceso,
                            numeroRetencionReal,
                            "RETENCION",
                            usuarioActual,
                            ipActual
                        )
                    );

                    // -----------------------------
                    // 7.1 SECUENCIAL REPETIDO
                    // -----------------------------
                    if (recepcion.SecuencialRepetido)
                    {
                        await Task.Run(() => _services.Param.IncrementarSecuencial("07"));
                        continue;
                    }

                    // -----------------------------
                    // 7.2 ERROR → PENDIENTE
                    // -----------------------------
                    if (!recepcion.Exito)
                    {
                        string numeroPendiente =
                            !string.IsNullOrWhiteSpace(recepcion.EstadoEspecial)
                                ? recepcion.EstadoEspecial
                                : _services.Pendientes.ObtenerEstadoPendientePorTipo("RETENCION");

                        bool insPendiente = await Task.Run(() =>
                            InsertarRetencion(
                                datos,
                                prep,
                                rowPfm,
                                numeroPendiente,
                                claveAcceso,
                                "PENDIENTE",
                                usuarioActual,
                                ipActual
                            )
                        );

                        r.Exito = false;
                        r.Autorizado = false;
                        r.NumeroRetencion = numeroPendiente;
                        r.Mensaje = insPendiente
                            ? "RETENCIÓN ELECTRÓNICA\n\n" +
                              "Registrada como pendiente.\n\n" +
                              "Número: " + numeroPendiente + "\n\n" +
                              recepcion.Mensaje
                            : "RETENCIÓN ELECTRÓNICA\n\n" +
                              "Error SRI y además error al registrar pendiente.\n\n" +
                              recepcion.Mensaje;

                        return r;
                    }

                    // -----------------------------
                    // 7.3 PENDIENTE_AUTORIZACION
                    // -----------------------------
                    if (!string.IsNullOrWhiteSpace(recepcion.EstadoEspecial) &&
                        recepcion.EstadoEspecial.StartsWith("PENDIENTE_AUTORIZACION"))
                    {
                        bool insPendAuth = await Task.Run(() =>
                            InsertarRetencion(
                                datos,
                                prep,
                                rowPfm,
                                prep.NumeroRetencion,
                                claveAcceso,
                                "PENDIENTE_AUTORIZACION",
                                usuarioActual,
                                ipActual
                            )
                        );

                        await Task.Run(() => _services.Param.IncrementarSecuencial("07"));

                        r.Exito = true;
                        r.Autorizado = false;
                        r.NumeroRetencion = prep.NumeroRetencion;
                        r.Mensaje = insPendAuth
                            ? "RETENCIÓN ELECTRÓNICA\n\n" +
                              "Documento recibido por el SRI.\n\n" +
                              "Pendiente de autorización."
                            : "RETENCIÓN ELECTRÓNICA\n\n" +
                              "Documento recibido por el SRI pero error al registrar en BD.\n\n" +
                              "Pendiente de autorización.";

                        return r;
                    }

                    // ==================================================
                    // 8) AUTORIZACIÓN
                    // ==================================================
                    var sri = await Task.Run(() =>
                        _services.ProcesosGenerales.ConsultarAutorizacion(
                            firma.RutaXmlFirmado,
                            claveAcceso,
                            "RETENCION",
                            prep.NumeroRetencion,
                            Path.Combine(_services.Paths.Retenciones, "XMLAUTORIZADOS"),
                            5,
                            (i, m) => { },
                            (i, m) => { },
                            usuarioActual,
                            ipActual
                        )
                    );

                    if (!sri.Exito)
                    {
                        InsertarRetencion(datos, prep, rowPfm, prep.NumeroRetencion, claveAcceso, "NO_AUTORIZADO", usuarioActual, ipActual);
                        return Error(r, sri.Mensaje);
                    }

                    r.RutaXmlAutorizado = sri.RutaXmlAutorizado;

                    // ==================================================
                    // 9) INSERTAR AUTORIZADO
                    // ==================================================
                    bool insOk = await Task.Run(() =>
                        InsertarRetencion(
                            datos,
                            prep,
                            rowPfm,
                            prep.NumeroRetencion,
                            claveAcceso,
                            "AUTORIZADO",
                            usuarioActual,
                            ipActual
                        )
                    );

                    if (!insOk)
                    {
                        // Retención ya autorizada por SRI pero no pudo guardarse en BD.
                        // Registrar como PENDIENTE para recuperación.
                        try
                        {
                            _services.Pendientes.Insertar(
                                prep.NumeroRetencion,
                                claveAcceso,
                                firma.RutaXmlFirmado ?? "",
                                DateTime.Now,
                                0,
                                "PENDIENTE",
                                "RETENCION",
                                usuarioActual,
                                ipActual
                            );
                        }
                        catch { /* no bloquear el flujo */ }

                        return Error(r,
                            "Retención AUTORIZADA por SRI pero error al registrar en BD. " +
                            "Guardada como pendiente para recuperación. " +
                            "ClaveAcceso: " + claveAcceso);
                    }

                    // ==================================================
                    // 10) PDF
                    // ==================================================
                    var pdf = await Task.Run(() =>
                        _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                            sri.XmlAutorizado,
                            rowEmpresa,
    Path.Combine(_services.Paths.Retenciones, "PDF"),
                            (gen, carpeta, xml, logo) =>
                                gen.GeneracionRideRetencionSRI(carpeta, xml, logo)
                        )
                    );

                    if (!pdf.Exito)
                        return Error(r, pdf.Mensaje);

                    r.RutaPdf = pdf.RutaPdf;

                    // ==================================================
                    // 11) CORREO
                    // ==================================================
                    var solicitudCorreo = new SolicitudCorreoDocumento
                    {
                        TipoDocumento = "RETENCION",
                        NumeroDocumento = prep.NumeroRetencion,
                        RutaPdf = r.RutaPdf,
                        RutaXmlAutorizado = r.RutaXmlAutorizado
                    };

                    var correo = await Task.Run(() =>
                        _services.ProcesosGenerales.EnviarCorreoDocumento(
                            solicitudCorreo,
                            rowCliente,
                            "RETENCION",
                            usuarioActual,
                            ipActual
                        )
                    );

                    r.EnvioCorreoExitoso = correo.Exito;

                    // ==================================================
                    // 12) INCREMENTAR SECUENCIAL FINAL
                    // ==================================================
                    await Task.Run(() => _services.Param.IncrementarSecuencial("07"));

                    // ==================================================
                    // 13) LIMPIAR XML TRANSITORIOS
                    // ==================================================
                    _services.ProcesosGenerales.LimpiarXmlTransitorios(
                        rutaXmlGenerado,
                        firma.RutaXmlFirmado,
                        r.RutaXmlAutorizado);

                    r.Exito = true;
                    r.Autorizado = true;
                    r.NumeroRetencion = prep.NumeroRetencion;
                    r.Mensaje = vieneDePendiente
                        ? "Retención pendiente autorizada correctamente."
                        : "Retención electrónica autorizada correctamente.";

                    return r;
                }

                return Error(r, "No se pudo generar la retención por conflictos de secuencial.");
            }
            catch (Exception ex)
            {
                return Error(r, "Error general Retención: " + ex.Message);
            }
        }



        public ResultadoFinalRetencion ProcesarRetencionDesdeAutorizacion(
            string numeroRetencion,
            string claveAcceso,
            string rutaXmlFirmado,
            DataRow rowEmpresa,
            DataRow rowCliente,
            string usuarioActual,
            string ipActual,
            string proceso // "AUTORIZACION" | "PDF" | "CORREO"
        )
        {
            var r = new ResultadoFinalRetencion
            {
                NumeroRetencion = numeroRetencion,
                ClaveAcceso = claveAcceso
            };

            try
            {
                // ==========================================================
                // VALIDACIONES BASE
                // ==========================================================
                if (string.IsNullOrWhiteSpace(numeroRetencion))
                    return Error(r, "Número de Retención vacío.");

                if (string.IsNullOrWhiteSpace(claveAcceso))
                    return Error(r, "Clave de acceso vacía.");

                if (string.IsNullOrWhiteSpace(rutaXmlFirmado))
                    return Error(r, "Ruta XML firmado vacía.");

                if (rowEmpresa == null)
                    return Error(r, "Empresa no cargada.");

                if (rowCliente == null)
                    return Error(r, "Sujeto retenido no cargado.");

                if (string.IsNullOrWhiteSpace(proceso))
                    proceso = "AUTORIZACION";

                proceso = proceso.Trim().ToUpperInvariant();

                XDocument xmlAutorizado = null;

                // ==========================================================
                // RUTAS BASE (ÚNICA FUENTE DE VERDAD)
                // ==========================================================
                string carpetaXmlAutorizado = Path.Combine(_services.Paths.Retenciones, "XMLAUTORIZADOS");
                string carpetaPdf = Path.Combine(_services.Paths.Retenciones, "PDF");

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
                        "RETENCION",
                        numeroRetencion,
                        Path.Combine(_services.Paths.Retenciones, "XMLAUTORIZADOS"),
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
                        r.Mensaje = "La Retención aún no está autorizada: " + sri.Mensaje;
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
                            return Error(r, "No existe XML autorizado para generar PDF.");

                        xmlAutorizado = XDocument.Load(rutaXmlAutorizado);
                        r.RutaXmlAutorizado = rutaXmlAutorizado;
                    }

                    var pdf = _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                        xmlAutorizado,
                        rowEmpresa,
                        Path.Combine(_services.Paths.Retenciones, "PDF"),
                        (gen, carpeta, xml, logo) =>
                            gen.GeneracionRideRetencionSRI(carpeta, xml, logo)
                    );

                    if (!pdf.Exito)
                        return Error(r, "No se pudo generar PDF: " + pdf.Mensaje);

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
                            return Error(r, "No existe XML autorizado para enviar.");

                        r.RutaXmlAutorizado = rutaXmlAutorizado;
                    }

                    // 🔒 GARANTIZAR PDF
                    if (string.IsNullOrWhiteSpace(r.RutaPdf))
                    {
                        if (!File.Exists(rutaPdf))
                            return Error(r, "No existe PDF para enviar.");

                        r.RutaPdf = rutaPdf;
                    }

                    var solicitud = new SolicitudCorreoDocumento
                    {
                        TipoDocumento = "RETENCION",
                        NumeroDocumento = numeroRetencion,
                        RutaPdf = r.RutaPdf,
                        RutaXmlAutorizado = r.RutaXmlAutorizado
                    };

                    var correo = _services.ProcesosGenerales.EnviarCorreoDocumento(
                        solicitud,
                        rowCliente,
                        "RETENCION",
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
                        ? "Retención procesada correctamente. Correo enviado."
                        : "Retención autorizada, pero no se pudo enviar el correo.";

                    return r;
                }

                return Error(r, "Proceso no reconocido: " + proceso);
            }
            catch (Exception ex)
            {
                return Error(r, "Error ProcesarRetencionDesdeAutorizacion: " + ex.Message);
            }
        }

        public ResultadoFinalRetencion RetencionPendienteAutorizacionDesdeApi(
        string numeroRetencion,
        string fechaRetencion,
        string usuarioActual,
        string ipActual,
        string proceso = "AUTORIZACION"
        )
        {
            if (!_services.ProcesosGenerales.PrepararPendienteDocumento(
                "RETENCION",
                numeroRetencion,
                fechaRetencion,
                usuarioActual,
                out string claveAcceso,
                out string rutaXmlFirmado,
                out DataRow rowEmpresa,
                out DataRow rowCliente,
                out string mensajeError
            ))
            {
                return new ResultadoFinalRetencion
                {
                    Exito = false,
                    Mensaje = mensajeError
                };
            }

            return ProcesarRetencionDesdeAutorizacion(
                numeroRetencion,
                claveAcceso,
                rutaXmlFirmado,
                rowEmpresa,
                rowCliente,
                usuarioActual,
                ipActual,
                proceso
            );
        }

        public async Task<ResultadoPDF> ProcesarRetencionPreviewAsync(
    DtoRetencionManual datos,
    DataTable conceptosRetencion,
    string usuarioActual
)
        {
            ResultadoPDF r = new ResultadoPDF();

            try
            {
                // ==================================================
                // VALIDACIONES
                // ==================================================
                if (datos == null)
                    return ErrorPreview(r, "Datos de la retención vacíos.");

                if (conceptosRetencion == null || conceptosRetencion.Rows.Count == 0)
                    return ErrorPreview(r, "No existen conceptos de retención.");

                if (string.IsNullOrWhiteSpace(datos.NumeroFactura))
                    return ErrorPreview(r, "Número de factura vacío.");

                // ==================================================
                // SECUENCIAL PREVIEW (NO AFECTA BD)
                // ==================================================
                string numeroRetencionPreview =
                    _services.Param.ObtenerSecuencialFormateado("07");

                // ==================================================
                // CARGAR BASE
                // ==================================================
                DataSet dsEmp = _services.Empresa.ConsultaNombre(usuarioActual);
                DataSet dsPfm = _services.ParamFactura.ConsultarNombre(usuarioActual);
                DataSet dsPmRet = _services.Param.ConsultarPorTipo("07");

                if (dsEmp.Tables[0].Rows.Count == 0 ||
                    dsPfm.Tables[0].Rows.Count == 0 ||
                    dsPmRet.Tables[0].Rows.Count == 0)
                    return ErrorPreview(r, "No se pudieron cargar datos base.");

                DataRow rowEmpresa = dsEmp.Tables[0].Rows[0];
                DataRow rowPfm = dsPfm.Tables[0].Rows[0];
                DataRow rowPmRet = dsPmRet.Tables[0].Rows[0];

                // ==================================================
                // SUJETO RETENIDO
                // ==================================================
                DataTable dtSujeto = new DataTable();
                dtSujeto.Columns.Add("NOMBRE");
                dtSujeto.Columns.Add("CEDULA");
                dtSujeto.Columns.Add("CORREO");
                dtSujeto.Columns.Add("DIRECCION");

                dtSujeto.Rows.Add(
                    datos.RazonSocial,
                    datos.Identificacion,
                    datos.Correo,
                    datos.Direccion
                );

                DataRow rowCliente = dtSujeto.Rows[0];

                // ==================================================
                // PREPARAR RETENCIÓN
                // ==================================================
                var prep = PrepararRetencion(
                    datos,
                    conceptosRetencion,
                    numeroRetencionPreview
                );

                if (!prep.Exito)
                    return ErrorPreview(r, prep.Mensaje);

                // ==================================================
                // GENERAR XML (SIN FIRMA)
                // ==================================================
                string claveAcceso;
                string rutaXmlGenerado;
                string errorXml;

                bool xmlOk = GenerarXmlRetencion(
                    prep.NumeroRetencion,
                    datos.NumeroFactura,
                    datos.FechaFactura.ToString("dd/MM/yyyy"),
                    prep.TB_DetalleRetencion,
                    rowCliente,
                    rowEmpresa,
                    rowPfm,
                    rowPmRet,
                    datos,
                    out claveAcceso,
                    out rutaXmlGenerado,
                    out errorXml
                );

                if (!xmlOk)
                    return ErrorPreview(r, errorXml);

                // ==================================================
                // CONVERTIR A XDOCUMENT
                // ==================================================
                XDocument xmlDoc = XDocument.Load(rutaXmlGenerado);

                // ==================================================
                // GENERAR PDF OFFLINE
                // ==================================================
                string carpeta = Path.Combine(_services.Paths.Retenciones, "PDFPREVIEW");
                Directory.CreateDirectory(carpeta);
                _services.ProcesosGenerales.LimpiarPreviewsAntiguos(carpeta, TimeSpan.FromHours(2));

                string rutaLogo = Path.Combine(
                    _services.Paths.General,
                    "LOGOFACTURA",
                    rowEmpresa["IMAGEN"].ToString()
                );

                if (!File.Exists(rutaLogo))
                    return ErrorPreview(r, "Logo no encontrado.");

                using (var logoStream = new MemoryStream(File.ReadAllBytes(rutaLogo)))
                {
                    logoStream.Position = 0;

                    var gen = new CGenerarRide();

                    string rutaPdf = gen.GeneracionRideRetencionSRI_OfflineSinAutorizacion(
                        carpeta,
                        xmlDoc,
                        logoStream
                    );

                    if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
                        return ErrorPreview(r, "No se pudo generar el PDF.");

                    r.Exito = true;
                    r.RutaPdf = rutaPdf;
                    return r;
                }
            }
            catch (Exception ex)
            {
                return ErrorPreview(r, ex.Message);
            }
        }

        private static ResultadoPDF ErrorPreview(ResultadoPDF r, string mensaje)
        {
            r.Exito = false;
            r.Mensaje = mensaje;
            return r;
        }



        public static string SoloConsultarAutorizacionSRI_Prueba(string claveAcceso)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claveAcceso))
                    return "Clave vacía.";

                // ======================================================
                // CREAR INSTANCIA (porque NO es static)
                // ======================================================
                CSriws sriws = new CSriws();

                // CONSULTA REAL
                var resp = sriws.AutorizacionComprobantePrueba(claveAcceso);

                if (resp == null)
                    return "SRI no respondió.";

                string estado = (resp.Estado ?? "").Trim().ToUpperInvariant();

                if (estado == "AUTORIZADO")
                    return "✅ AUTORIZADO";

                if (estado == "NO AUTORIZADO")
                    return "❌ NO AUTORIZADO";

                if (estado == "EN PROCESAMIENTO")
                    return "⏳ EN PROCESAMIENTO";

                // Mostrar error si existe
                if (resp.Errores != null && resp.Errores.Count > 0)
                {
                    var e = resp.Errores[0];
                    return "⚠️ " + e.Tipo + " - " + e.Mensaje +
                           "\n" + e.InformacionAdicional;
                }

                return "Estado desconocido: " + estado;
            }
            catch (Exception ex)
            {
                return "Error consultando autorización: " + ex.Message;
            }
        }



        // Helper interno
        private static ResultadoFinalRetencion Error(ResultadoFinalRetencion r, string mensaje)
        {
            r.Exito = false;
            r.Mensaje = mensaje;
            return r;
        }



    }
}
