using LogicaNegocios.Services;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LogicaNegocios.Procesos.ProcesosRetenciones;
using static SistemaFacturacion.ProcesosGeneralesUI;


namespace SistemaFacturacion
{
    public partial class frmConsultarRetenciones : Form
    {

        private readonly AppServices _services;

        public frmConsultarRetenciones(
              AppServices services

          )
        {
            _services = services;


            InitializeComponent();
            try
            {
                dtpDesde.Format = DateTimePickerFormat.Custom;
                dtpDesde.CustomFormat = "yyyy/MM/dd";

                dtpHasta.Format = DateTimePickerFormat.Custom;
                dtpHasta.CustomFormat = "yyyy/MM/dd";

                CargarComboSujetos();
                CargarComboRetenciones();
                CargarComboFacturas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inicializando formulario: " + ex.Message);
            }
        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private void InsertarFilaSeleccione(
        ComboBox combo,
        string campoTexto,
        string campoValor
    )
        {
            if (combo.DataSource is DataTable dt)
            {
                bool yaExiste = dt.AsEnumerable().Any(r =>
                    (r[campoTexto]?.ToString() ?? "").Trim().ToLower() == "seleccione"
                );

                if (!yaExiste)
                {
                    DataRow r = dt.NewRow();
                    r[campoTexto] = "Seleccione";
                    r[campoValor] = "";
                    dt.Rows.InsertAt(r, 0);
                }

                combo.SelectedIndex = 0;
            }
            else
            {
                combo.SelectedIndex = 0;
            }
        }

        private void CargarComboRetenciones()
        {
            try
            {
                DataSet ds = _services.Retencion.ListarNumerosRetencion();

                cmbRetencion.DisplayMember = "NUMERORETENCION";
                cmbRetencion.ValueMember = "NUMERORETENCION";
                cmbRetencion.DataSource = ds.Tables[0];

                InsertarFilaSeleccione(
                    cmbRetencion,
                    "NUMERORETENCION",
                    "NUMERORETENCION"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando números de retención: " + ex.Message);
            }
        }


        private void CargarComboSujetos()
        {
            try
            {
                DataSet ds = _services.Retencion.ListarSujetosRetenidos();

                cmbSujeto.DisplayMember = "SUJETORETENIDO";
                cmbSujeto.ValueMember = "IDENTIFICACIONSUJETO";
                cmbSujeto.DataSource = ds.Tables[0];

                InsertarFilaSeleccione(
                    cmbSujeto,
                    "SUJETORETENIDO",
                    "IDENTIFICACIONSUJETO"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando sujetos retenidos: " + ex.Message);
            }
        }
        private void CargarComboFacturas()
        {
            try
            {
                DataSet ds = _services.Retencion.ListarNumerosFactura();

                cmbFactura.DisplayMember = "NUMEROFACTURA";
                cmbFactura.ValueMember = "NUMEROFACTURA";
                cmbFactura.DataSource = ds.Tables[0];

                InsertarFilaSeleccione(
                    cmbFactura,
                    "NUMEROFACTURA",
                    "NUMEROFACTURA"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando números de factura: " + ex.Message);
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            try
            {
                string fechaDesde = dtpDesde.Text;
                string fechaHasta = dtpHasta.Text;

                // ---------------------------------
                // Sujeto retenido
                // ---------------------------------
                string sujeto = "";
                if (cmbSujeto.Text != null &&
                    cmbSujeto.Text.Trim().ToLower() != "seleccione")
                {
                    sujeto = cmbSujeto.Text.Trim();
                }

                // ---------------------------------
                // Número de retención
                // ---------------------------------
                string numeroRetencion = "";
                if (cmbRetencion.Text != null &&
                    cmbRetencion.Text.Trim().ToLower() != "seleccione")
                {
                    numeroRetencion = cmbRetencion.Text.Trim().ToUpperInvariant();
                }

                // ---------------------------------
                // Número de factura
                // ---------------------------------
                string numeroFactura = "";
                if (cmbFactura.Text != null &&
                    cmbFactura.Text.Trim().ToLower() != "seleccione")
                {
                    numeroFactura = cmbFactura.Text.Trim();
                }

                // ==========================================================
                // CONSULTA ÚNICA (AVANZADA)
                // ==========================================================
                DataSet dsDatos = _services.Retencion.ConsultarAvanzado(
                    fechaDesde: fechaDesde,
                    fechaHasta: fechaHasta,
                    sujetoRetenido: sujeto,
                    numeroRetencion: numeroRetencion,
                    numeroFactura: numeroFactura
                );

                // ==========================================================
                // BIND GRID + ACCIONES
                // ==========================================================
                if (dsDatos != null &&
                    dsDatos.Tables.Count > 0 &&
                    dsDatos.Tables[0].Rows.Count > 0)
                {
                    dgvRetenciones.AutoGenerateColumns = true;
                    dgvRetenciones.DataSource = dsDatos.Tables[0];

                    AgregarColumnasAcciones();
                    PintarColumnasProcesar();
                }
                else
                {
                    dgvRetenciones.DataSource = null;
                    LimpiarColumnasAcciones();
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(
                    this,
                    "Error consultando Retenciones: " + ex.Message,
                    "error"
                );
            }
        }

        private async void ProcesarRetencionPendiente(DataGridViewRow fila)
        {
            try
            {

                // ======================================================
                // 0) VALIDAR NÚMERO PENDIENTE
                // ======================================================
                string numeroPendiente = fila.Cells["NUMERORETENCION"]
                    .Value?.ToString()?.Trim().ToUpperInvariant() ?? "";

                if (string.IsNullOrWhiteSpace(numeroPendiente) ||
                    !numeroPendiente.StartsWith("PENDIENTE"))
                {
                    Notificaciones.Show(
                        this,
                        "La retención no se encuentra en estado pendiente.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ======================================================
                // 1) ENCABEZADO
                // ======================================================
                DataSet dsEnc = await Task.Run(() =>
                    _services.Retencion.ConsultarPorNumero(numeroPendiente)
                );

                if (dsEnc == null || dsEnc.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No se encontró el encabezado de la Retención pendiente.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataRow enc = dsEnc.Tables[0].Rows[0];

                // ======================================================
                // 2) DETALLE
                // ======================================================
                DataSet dsDet = await Task.Run(() =>
                    _services.Retencion.ConsultarDetalle(numeroPendiente)
                );

                if (dsDet == null || dsDet.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No se encontró el detalle de la Retención pendiente.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataTable detalle = dsDet.Tables[0];

                string identificacion = enc["IDENTIFICACIONSUJETO"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(identificacion))
                    throw new Exception("Identificación del sujeto retenido vacía.");

                DataSet dsProv = await Task.Run(() =>
                    _services.Proveedor.ConsultarPorIdentificacion(identificacion)
                );

                if (dsProv == null ||
                    dsProv.Tables.Count == 0 ||
                    dsProv.Tables[0].Rows.Count == 0)
                {
                    throw new Exception(
                        "No se encontró el proveedor para obtener el tipo de identificación."
                    );
                }

                DataRow prov = dsProv.Tables[0].Rows[0];

                string tipoIdentificacion = prov["TIPOIDENTIFICACION"]
                    ?.ToString()
                    ?.Trim();


                // ======================================================
                // 3) DTO
                // ======================================================
                DtoRetencionManual datos = new DtoRetencionManual
                {
                    NumeroRetencion = numeroPendiente,
                    NumeroFactura = enc["NUMEROFACTURA"]?.ToString()?.Trim(),
                    FechaFactura = DateTime.TryParse(
                        enc["FECHAFACTURA"]?.ToString(),
                        out DateTime f) ? f : DateTime.Now,
                    RazonSocial = enc["SUJETORETENIDO"]?.ToString()?.Trim(),
                    Identificacion = identificacion,
                    TipoIdentificacion = tipoIdentificacion, // 👈 AQUÍ ESTABA LO QUE FALTABA
                    Correo = prov.Table.Columns.Contains("CORREO")
                        ? prov["CORREO"]?.ToString()?.Trim()
                        : ""
                };


                // ======================================================
                // 4) PROCESAR
                // ======================================================
                Notificaciones.Show(
                    this,
                    $"Procesando Retención pendiente {numeroPendiente}…",
                    "proceso",
                    UsuarioActual,
                    IPActual
                );

                ResultadoFinalRetencion res =
                    await _services.ProcesosRetenciones.ProcesarRetencionElectronicaCompleta(
                        datos,
                        detalle,
                        UsuarioActual,
                        IPActual,
                        numeroPendiente
                    );

                // ======================================================
                // 5) LIMPIEZA CON LOS MISMOS IFs QUE NOTA DE CRÉDITO
                // ======================================================

                // 5.1) Si NO está autorizado (error o pendiente)
                if (!res.Autorizado)
                {
                    _services.Retencion.EliminarDetalle(numeroPendiente);
                    _services.Retencion.Eliminar(numeroPendiente, UsuarioActual, IPActual);
                }

                // 5.2) Si está autorizado
                if (res.Exito && res.Autorizado)
                {
                    _services.Retencion.EliminarDetalle(numeroPendiente);
                    _services.Retencion.Eliminar(numeroPendiente, UsuarioActual, IPActual);
                }

                // ======================================================
                // 6) CERRAR PROCESO
                // ======================================================
                Notificaciones.CerrarProceso(this);

                // ======================================================
                // 7) RESULTADO
                // ======================================================
                if (!res.Exito)
                {
                    Notificaciones.Show(
                        this,
                        res.Mensaje,
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    btnConsultar.PerformClick();
                    return;
                }

                if (res.Exito && !res.Autorizado)
                {
                    Notificaciones.Show(
                        this,
                        "⚠ RETENCIÓN ELECTRÓNICA\n\n" +
                        "Documento enviado al SRI pero quedó\n" +
                        "PENDIENTE de autorización.\n\n" +
                        "Número: " + res.NumeroRetencion,
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    btnConsultar.PerformClick();
                    return;
                }

                Notificaciones.Show(
                    this,
                    "✅ RETENCIÓN ELECTRÓNICA AUTORIZADA\n\n" +
                    "Número: " + res.NumeroRetencion,
                    "exito",
                    UsuarioActual,
                    IPActual
                );

                btnConsultar.PerformClick();
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);

                Notificaciones.Show(
                    this,
                    "Error procesando Retención pendiente:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
        }

        private void dgvRetenciones_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                var grid = (DataGridView)sender;
                var fila = grid.Rows[e.RowIndex];
                if (fila == null || fila.IsNewRow)
                    return;

                string columna = grid.Columns[e.ColumnIndex].Name;

                if (columna != "colProcesar" &&
                    columna != "colPDF" &&
                    columna != "colXML")
                    return;

                string numeroRetencion = GetCellString(fila, "NUMERORETENCION")
                    .Trim()
                    .ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(numeroRetencion))
                    return;

                // ==================================================
                // PDF / XML → MÉTODO GLOBAL (IGUAL QUE NC)
                // ==================================================
                var accionDoc = _services.Pendientes.ConsultarAccionPendienteDocumento(numeroRetencion, "RETENCION");
                bool esNoAutorizado = accionDoc.Accion == "NO_AUTORIZADO";

                if (columna == "colPDF")
                {
                    if (!esNoAutorizado)
                    {
                        AbrirDocumentoPorGrid(
                            fila,
                            "FECHAEMISION",
                            numeroRetencion,
                            _services.Paths.Retenciones,   // carpeta base
                            "PDF",                         // subcarpeta
                            "pdf",
                            "Retención"
                        );
                    }
                    return;
                }

                if (columna == "colXML")
                {
                    if (!esNoAutorizado)
                    {
                        AbrirDocumentoPorGrid(
                            fila,
                            "FECHAEMISION",
                            numeroRetencion,
                            _services.Paths.Retenciones,   // carpeta base
                            "XMLAUTORIZADOS",              // subcarpeta correcta
                            "xml",
                            "Retención"
                        );
                    }
                    return;
                }

                // ==================================================
                // DESDE AQUÍ SOLO PENDIENTES / ACCIONES
                // ==================================================
                var accionPendiente =
                    _services.Pendientes
                        .ConsultarAccionPendienteDocumento(
                            numeroRetencion,
                            "RETENCION"
                        );

                if (!accionPendiente.Existe)
                    return;

                if (columna == "colProcesar")
                {
                    // =============================================================
                    // -1) VALIDAR AMBIENTE (PRUEBAS / PRODUCCIÓN)
                    // =============================================================
                    bool esProduccion = _services.ParamFactura
                        .EsProduccion(UsuarioActual);

                    if (!esProduccion)
                    {
                        bool continuar = Notificaciones.Show(
                            this,
                            "⚠ USTED ESTÁ EN AMBIENTE DE PRUEBAS.\n\n" +
                            "Los comprobantes NO tendrán validez tributaria.\n\n" +
                            "¿Desea continuar en PRUEBAS?",
                            "confirmacion"
                        );

                        if (!continuar)
                        {
                            bool cambiar = Notificaciones.Show(
                                this,
                                "¿Desea cambiar ahora a PRODUCCIÓN?",
                                "confirmacion"
                            );

                            if (cambiar)
                            {
                                _services.ParamFactura
                                    .CambiarAProduccion(UsuarioActual,UsuarioActual, IPActual);

                                Notificaciones.Show(
                                    this,
                                    "Sistema cambiado a PRODUCCIÓN correctamente.\n" +
                                    "Vuelva a presionar Procesar.",
                                    "exito"
                                );
                            }

                            return; // Detener proceso
                        }
                    }
                    switch (accionPendiente.Accion)
                    {
                        case "PROCESAR":
                            ProcesarRetencionPendiente(fila);
                            return;

                        case "AUTORIZAR":
                            ConsultarAutorizacionRetencionPendiente(fila);
                            return;

                        case "CORREO":
                            EnviarCorreoRetencionPendiente(fila);
                            return;

                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(
                    this,
                    "Error en la tabla de Retenciones:\n" + ex.Message,
                    "error"
                );
            }
        }

        //Columnas

        private void PintarColumnasProcesar()
        {
            PintarColumnasDocumento(
                dgvRetenciones,
                "NUMERORETENCION",
                "colProcesar",
                "RETENCION",
                _services
            );
        }

        private void AgregarColumnasAcciones()
        {
            AgregarColumnasAccionesReportes(
      dgvRetenciones,
      "colProcesar",
      "ACCIÓN",
      100
  );

        }

        private void LimpiarColumnasAcciones()
        {
            if (dgvRetenciones.Columns.Contains("colProcesar"))
                dgvRetenciones.Columns.Remove("colProcesar");

            if (dgvRetenciones.Columns.Contains("colPDF"))
                dgvRetenciones.Columns.Remove("colPDF");

            if (dgvRetenciones.Columns.Contains("colXML"))
                dgvRetenciones.Columns.Remove("colXML");
        }

        private string GetCellString(DataGridViewRow fila, string colName)
        {
            if (fila == null) return "";
            if (fila.DataGridView == null) return "";
            if (!fila.DataGridView.Columns.Contains(colName)) return "";
            return fila.Cells[colName].Value?.ToString() ?? "";
        }


        //Pendientes

        private bool ObtenerXmlFirmadoRetencion(
            DataGridViewRow fila,
            string numeroRetencion,
            out string claveAcceso,
            out string rutaXmlFirmado
        )
        {
            claveAcceso = "";
            rutaXmlFirmado = "";

            try
            {
                string fechaOriginal = fila.Cells["FECHAEMISION"]?.Value?.ToString() ?? "";
                if (!DateTime.TryParse(fechaOriginal, out DateTime fecha))
                    return false;

                string fechaFormato = fecha.ToString("ddMMyyyy");

                // 🔎 patrón: fecha + numero retención
                string patron = $"*{fechaFormato}*{numeroRetencion}*.xml";

                // 📁 carpeta XML FIRMADO FACTURA
                string carpeta = Path.Combine(_services.Paths.Retenciones, "XMLFIRMADOS");

                if (!Directory.Exists(carpeta))
                    return false;

                string[] archivos = Directory.GetFiles(carpeta, patron);

                if (archivos.Length == 0)
                    return false;

                // 👉 Tomamos el primero (debe ser único)
                rutaXmlFirmado = archivos[0];

                // 🔑 claveAcceso = nombre del archivo sin extensión
                claveAcceso = Path.GetFileNameWithoutExtension(rutaXmlFirmado);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void EnviarCorreoRetencionPendiente(DataGridViewRow fila)
        {
            Notificaciones.Show(
                this,
                "Enviando correo de la Retención…",
                "proceso",
                UsuarioActual,
                IPActual
            );

            try
            {
                string numeroRetencion = GetCellString(fila, "NUMERORETENCION")?.Trim();

                if (string.IsNullOrWhiteSpace(numeroRetencion))
                {
                    Notificaciones.Show(
                        this,
                        "Número de Retención inválido.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================
                // XML FIRMADO + CLAVE ACCESO
                // ==========================================
                bool okXml = ObtenerXmlFirmadoRetencion(
                    fila,
                    numeroRetencion,
                    out string claveAcceso,
                    out string rutaXmlFirmado
                );

                if (!okXml)
                {
                    Notificaciones.Show(
                        this,
                        "No se pudo localizar el XML firmado de la Retención.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================
                // CARGAR EMPRESA Y RETENCIÓN (BD)
                // ==========================================
                DataSet dsEmp = await Task.Run(() =>
                    _services.Empresa.ConsultaNombre(UsuarioActual)
                );

                DataSet dsRet = await Task.Run(() =>
                    _services.Retencion.ConsultarPorNumero(numeroRetencion)
                );

                if (dsEmp.Tables[0].Rows.Count == 0 ||
                    dsRet.Tables[0].Rows.Count == 0)
                    throw new Exception("Datos no encontrados.");

                DataRow rowRet = dsRet.Tables[0].Rows[0];

                // ==========================================
                // OBTENER SUJETO RETENIDO (CON CORREO)
                // ==========================================
                string identificacion =
                    rowRet["IDENTIFICACIONSUJETO"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    Notificaciones.Show(
                        this,
                        "La Retención no tiene identificación del sujeto retenido.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataSet dsCliente = await Task.Run(() =>
                    _services.Cliente.ConsultarCedula(identificacion)
                );

                if (dsCliente.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No se encontró el sujeto retenido para enviar el correo.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================
                // PROCESAR CORREO (FLUJO PESADO)
                // ==========================================
                var res = await Task.Run(() =>
                    _services.ProcesosRetenciones.ProcesarRetencionDesdeAutorizacion(
                        numeroRetencion,
                        claveAcceso,
                        rutaXmlFirmado,
                        dsEmp.Tables[0].Rows[0],
                        dsCliente.Tables[0].Rows[0],
                        UsuarioActual,
                        IPActual,
                        "CORREO"
                    )
                );

                // ==========================================
                // RESULTADO
                // ==========================================
                Notificaciones.Show(
                    this,
                    res.Mensaje,
                    res.Exito ? "exito" : "error",
                    UsuarioActual,
                    IPActual
                );
            }
            catch (Exception ex)
            {
                Notificaciones.Show(
                    this,
                    "Error enviando correo de la Retención:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
            finally
            {
                // 🔒 SIEMPRE CERRAR PROCESO
                Notificaciones.CerrarProceso(this);
                btnConsultar.PerformClick();
            }
        }

        private async void ConsultarAutorizacionRetencionPendiente(DataGridViewRow fila)
        {
            try
            {
                string numeroRetencion = GetCellString(fila, "NUMERORETENCION").Trim();

                if (string.IsNullOrWhiteSpace(numeroRetencion))
                {
                    Notificaciones.Show(this,
                        "Número de Retención inválido.",
                        "advertencia");
                    return;
                }

                // ===============================
                // MOSTRAR PROCESO (UI THREAD)
                // ===============================
                Notificaciones.Show(this,
                    $"Consultando Retención número: {numeroRetencion}…",
                    "proceso");

                // ===============================
                // PROCESO PESADO → HILO BACKGROUND
                // ===============================
                var res = await Task.Run(() =>
                {
                    // ==========================================
                    // OBTENER XML FIRMADO
                    // ==========================================
                    if (!ObtenerXmlFirmadoRetencion(
                        fila,
                        numeroRetencion,
                        out string claveAcceso,
                        out string rutaXmlFirmado))
                    {
                        return new ResultadoFinalRetencion
                        {
                            Exito = false,
                            Mensaje = "No se pudo localizar el XML firmado de la Retención."
                        };
                    }

                    // ==========================================
                    // EMPRESA
                    // ==========================================
                    var dsEmp = _services.Empresa.ConsultaNombre(UsuarioActual);
                    if (dsEmp.Tables[0].Rows.Count == 0)
                        throw new Exception("Empresa no encontrada.");

                    // ==========================================
                    // RETENCIÓN (ENCABEZADO)
                    // ==========================================
                    var dsRet = _services.Retencion.ConsultarPorNumero(numeroRetencion);
                    if (dsRet.Tables[0].Rows.Count == 0)
                        throw new Exception("Retención no encontrada.");

                    // ==========================================
                    // CONTINUAR FLUJO DESDE AUTORIZACIÓN
                    // ==========================================
                    return _services.ProcesosRetenciones.ProcesarRetencionDesdeAutorizacion(
                        numeroRetencion,
                        claveAcceso,
                        rutaXmlFirmado,
                        dsEmp.Tables[0].Rows[0],
                        dsRet.Tables[0].Rows[0],
                        UsuarioActual,
                        IPActual,
                        "AUTORIZACION"
                    );
                });

                // ===============================
                // CERRAR PROCESO (UI THREAD)
                // ===============================
                Notificaciones.CerrarProceso(this);

                // ===============================
                // RESULTADO FINAL
                // ===============================
                Notificaciones.Show(this,
                    res.Mensaje,
                    res.Exito ? "exito" : "error");

                btnConsultar.PerformClick();
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);

                Notificaciones.Show(this,
                    "Error consultando autorización de la Retención:\n" + ex.Message,
                    "error");
            }
        }


    }
}
