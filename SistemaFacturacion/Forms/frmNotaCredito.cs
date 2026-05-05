using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SistemaFacturacion.ProcesosGeneralesUI;


namespace SistemaFacturacion
{
    public partial class frmNotaCredito : Form
    {
        private readonly AppServices _services;
        public frmNotaCredito(AppServices services
        )
        {
            _services = services;
            InitializeComponent();
            dgvNotas.CellClick += dgvNotas_CellClick;
        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }



        private void frmNotaCredito_Load(object sender, EventArgs e)
        {
            try
            {
                // Si tu helper AccessDateFromText asume dd/MM/yyyy,
                // configura el DateTimePicker así:
                dtpDesde.Format = DateTimePickerFormat.Custom;
                dtpDesde.CustomFormat = "yyyy/MM/dd";

                dtpHasta.Format = DateTimePickerFormat.Custom;
                dtpHasta.CustomFormat = "yyyy/MM/dd";

                MostrarClientes();
                MostrarNotas();

                // OPCIONAL: si vas a usar combo de números de nota:
                // MostrarNumerosNota();

                // Ajustes grid
                dgvNotas.AutoGenerateColumns = true;
                dgvNotas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvNotas.MultiSelect = false;

                // Totales
                lblTotal.Text = "TOTAL: 0.00";
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error en carga: " + ex.Message, "error");
            }
        }

        // ======================================================
        // CARGA COMBO CLIENTES
        // ======================================================
        public void MostrarClientes()
        {
            try
            {
                var dsDato = _services.Facturacion.ConsultarCliente();

                cmbCliente.DisplayMember = "NOMBRE";
                cmbCliente.ValueMember = "CEDULA";
                cmbCliente.DataSource = dsDato.Tables[0];

                // Si quieres obligar a "seleccione", lo más seguro es insertar fila:
                InsertarFilaSeleccione(cmbCliente, "NOMBRE", "CEDULA");
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error cargando clientes:\n" + ex.Message,
                    "error");
            }
        }
        public void MostrarNotas()
        {
            try
            {
                var dsDato = _services.NotaCredito.Listar(1000);

                cmbNota.DisplayMember = "NUMERONOTA";
                cmbNota.ValueMember = "NUMERONOTA";
                cmbNota.DataSource = dsDato.Tables[0];

                // Si quieres obligar a "seleccione", lo más seguro es insertar fila:
                InsertarFilaSeleccione(cmbNota, "NUMERONOTA", "NUMERONOTA");
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error cargando notas:\n" + ex.Message,
                    "error");
            }
        }
        private void dgvNotas_CellClick(object sender, DataGridViewCellEventArgs e)
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

                string numeroNota = GetCellString(fila, "NUMERONOTA")
                    .Trim()
                    .ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(numeroNota))
                    return;

                // ==================================================
                // PDF / XML → MÉTODO GLOBAL
                // ==================================================
                var accionDoc = _services.Pendientes.ConsultarAccionPendienteDocumento(numeroNota, "NOTADECREDITO");
                bool esNoAutorizado = accionDoc.Accion == "NO_AUTORIZADO";

                if (columna == "colPDF")
                {
                    if (!esNoAutorizado)
                    {
                        AbrirDocumentoPorGrid(
                            fila,
                            "FECHAEMISION",
                            numeroNota,
                            _services.Paths.NotasCredito,   // carpeta base
                            "PDF",                          // subcarpeta
                            "pdf",
                            "Nota de Crédito"
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
                            numeroNota,
                            _services.Paths.NotasCredito,   // carpeta base
                            "XMLAUTORIZADOS",               // subcarpeta correcta
                            "xml",
                            "Nota de Crédito"
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
                            numeroNota,
                            "NOTADECREDITO"
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
                                    .CambiarAProduccion(UsuarioActual, UsuarioActual, IPActual);

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
                            ProcesarNotaPendiente(fila);
                            return;

                        case "AUTORIZAR":
                            ConsultarAutorizacionNotaPendiente(fila);
                            return;

                        case "CORREO":
                            EnviarCorreoNotaCredito(fila);
                            return;

                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error en la tabla de Notas de Crédito:\n" + ex.Message,
                    "error");
            }
        }
        private void InsertarFilaSeleccione(ComboBox combo, string campoTexto, string campoValor)
        {
            if (combo.DataSource is DataTable dt)
            {
                // Evitar duplicar
                bool yaExiste = dt.AsEnumerable().Any(r =>
                    (r[campoTexto]?.ToString() ?? "").Trim().ToLower() == "seleccione");

                if (!yaExiste)
                {
                    DataRow r = dt.NewRow();
                    r[campoTexto] = "seleccione";
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

        // ======================================================
        // OPCIONAL: CARGAR COMBO NUMEROS NOTA
        // ======================================================
        public void MostrarNumerosNota()
        {
            try
            {
                var ds = _services.NotaCredito.Listar(1000);
                cmbNota.DisplayMember = "NUMERONOTA";
                cmbNota.ValueMember = "NUMERONOTA";
                cmbNota.DataSource = ds.Tables[0];

                // insertar "seleccione"
                InsertarFilaSeleccione(cmbNota, "NUMERONOTA", "NUMERONOTA");
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error cargando números: " + ex.Message, "error");
            }
        }

        private void CalcularTotales(DataTable tabla)
        {
            try
            {
                if (tabla == null || tabla.Rows.Count == 0)
                {
                    lblTotal.Text = "TOTAL: 0.00";
                    return;
                }

                // En tu SELECT devolvemos TOTALCONIMPUESTOS
                object suma = tabla.Compute("SUM(TOTALCONIMPUESTOS)", "");
                decimal total = 0;
                if (suma != null && suma != DBNull.Value)
                    total = Convert.ToDecimal(suma);

                lblTotal.Text = "TOTAL: " + total.ToString("0.00");
            }
            catch
            {
                lblTotal.Text = "TOTAL: 0.00";
            }
        }

        // ======================================================
        // BOTÓN CONSULTAR
        // ======================================================
        private void btnConsultar_Click_1(object sender, EventArgs e)
        {
            try
            {
                DataSet dsDatos;

                string fechaDesde = dtpDesde.Text;
                string fechaHasta = dtpHasta.Text;

                // -----------------------------
                // Cliente
                // -----------------------------
                string cedulaCliente = cmbCliente.SelectedValue?.ToString();
                bool sinCliente =
                    string.IsNullOrWhiteSpace(cedulaCliente) ||
                    cmbCliente.Text.Trim().ToLower() == "Seleccione";

                // -----------------------------
                // Numero Nota
                // -----------------------------
                string numeroNota = cmbNota.SelectedValue?.ToString();
                bool sinNumeroNota =
                    string.IsNullOrWhiteSpace(numeroNota) ||
                    cmbNota.Text.Trim().ToLower() == "Seleccione";

                if (!sinNumeroNota)
                    numeroNota = numeroNota.Trim().ToUpperInvariant();

                // ==========================================================
                // CONSULTA (prioridad: ambos -> nota -> cliente -> fechas)
                // ==========================================================
                if (!sinNumeroNota && !sinCliente)
                {
                    dsDatos = _services.NotaCredito.ConsultarPorClienteYNumeroNota(
                        fechaDesde, fechaHasta, cedulaCliente, numeroNota
                    );
                }
                else if (!sinNumeroNota)
                {
                    dsDatos = _services.NotaCredito.ConsultarPorNumeroNotaFechas(
                        fechaDesde, fechaHasta, numeroNota
                    );
                }
                else if (!sinCliente)
                {
                    dsDatos = _services.NotaCredito.ConsultarPorCliente(
                        fechaDesde, fechaHasta, cedulaCliente
                    );
                }
                else
                {
                    dsDatos = _services.NotaCredito.ConsultarPorFechas(
                        fechaDesde, fechaHasta
                    );
                }

                // ==========================================================
                // BIND GRID + ACCIONES (PROCESAR / PDF / XML)
                // ==========================================================
                if (dsDatos != null && dsDatos.Tables.Count > 0 && dsDatos.Tables[0].Rows.Count > 0)
                {
                    dgvNotas.AutoGenerateColumns = true;
                    dgvNotas.DataSource = dsDatos.Tables[0];


                    // Totales
                    CalcularTotales(dsDatos.Tables[0]);

                    // Columnas acciones
                    AgregarColumnasAcciones();
                    PintarColumnasProcesar();
                }
                else
                {
                    dgvNotas.DataSource = null;

                    // limpiar acciones por seguridad
                    LimpiarColumnasAcciones();

                    lblTotal.Text = "TOTAL: 0.00";
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error consultando: " + ex.Message, "error");
            }
        }

        private async void ProcesarNotaPendiente(DataGridViewRow fila)
        {

            Notificaciones.Show(
                this,
                "Procesando Nota de Crédito pendiente…",
                "proceso",
                UsuarioActual,
                IPActual
            );

            try
            {
                // ==========================================================
                // 0) VALIDAR ESTADO PENDIENTE
                // ==========================================================
                string numeroPendiente =
                    fila.Cells["NUMERONOTA"].Value?.ToString()?.Trim().ToUpperInvariant() ?? "";

                var accion = await Task.Run(() =>
                    _services.Pendientes
                        .ConsultarAccionPendienteDocumento(numeroPendiente, "NOTADECREDITO")
                );

                if (!accion.Existe || accion.Accion != "PROCESAR")
                {
                    Notificaciones.Show(
                        this,
                        "La nota no se encuentra en un estado procesable.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================================
                // 1) CARGAR ENCABEZADO
                // ==========================================================
                DataSet dsEnc = await Task.Run(() =>
                    _services.NotaCredito.ConsultarPorNumeroNota(numeroPendiente)
                );

                if (dsEnc == null || dsEnc.Tables.Count == 0 || dsEnc.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No se encontró el encabezado de la Nota de Crédito.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataRow enc = dsEnc.Tables[0].Rows[0];

                string numeroFactura = enc["NUMEROFACTURA"]?.ToString()?.Trim() ?? "";
                string motivo = enc["MOTIVO"]?.ToString()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    Notificaciones.Show(
                        this,
                        "La nota pendiente no tiene factura asociada.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    Notificaciones.Show(
                        this,
                        "La nota pendiente no tiene motivo registrado.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================================
                // 2) CARGAR DETALLE
                // ==========================================================
                DataSet dsDet = await Task.Run(() =>
                    _services.NotaCredito.ConsultarDetallePorNumeroNota(numeroPendiente)
                );

                if (dsDet == null || dsDet.Tables.Count == 0 || dsDet.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No se encontró detalle de la Nota de Crédito pendiente.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================================
                // 3) CONSTRUIR PRODUCTOS
                // ==========================================================
                DataTable productos = ConstruirTbNcParaXml(dsDet.Tables[0]);

                if (productos == null || productos.Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "El detalle no contiene filas válidas para procesar.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================================
                // 4) EJECUTAR MÉTODO GLOBAL (PESADO)
                // ==========================================================
                var res = await
                    _services.ProcesosNotaCredito.ProcesarNotaCreditoElectronicaCompletaAsync(
                        numeroFactura: numeroFactura,
                        motivo: motivo,
                        productos: productos,
                        usuarioActual: UsuarioActual,
                        ipActual: IPActual,
                        numeroNotaPendiente: numeroPendiente
                    );


                // ==========================================================
                // 5) INTERPRETACIÓN DEL RESULTADO
                // ==========================================================

                // ============================
                // LIMPIEZA SI AUTORIZADO
                // ============================
                if (!res.Autorizado)
                {
                    _services.NotaCredito.EliminarDetallePorNumeroNota(
                        numeroPendiente
                    );
                    _services.NotaCredito.Eliminar(
                        numeroPendiente,
                        UsuarioActual,
                        IPActual
                    );
                }

                if (res.Exito && res.Autorizado)
                {
                    _services.NotaCredito.EliminarDetallePorNumeroNota(
                       numeroPendiente
                   );
                    _services.NotaCredito.Eliminar(
                        numeroPendiente,
                        UsuarioActual,
                        IPActual
                    );
                }

                // ❌ ERROR REAL
                if (!res.Exito)
                {
                    Notificaciones.Show(
                        this,
                        res.Mensaje,
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ⚠️ PENDIENTE DE AUTORIZACIÓN
                if (res.Exito && !res.Autorizado)
                {
                    Notificaciones.Show(
                        this,
                        "⚠ NOTA DE CRÉDITO ELECTRÓNICA\n\n" +
                        "La Nota de Crédito fue ENVIADA al SRI\n" +
                        "pero quedó PENDIENTE de autorización.\n\n" +
                        "Número: " + res.NumeroNota + "\n\n" +
                        "Puede consultarla más tarde desde Consultas.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ⚠️ AUTORIZADA SIN CORREO
                if (res.Exito && res.Autorizado && !res.EnvioCorreoExitoso)
                {
                    Notificaciones.Show(
                        this,
                        "⚠ NOTA DE CRÉDITO AUTORIZADA\n\n" +
                        "La Nota de Crédito fue AUTORIZADA correctamente,\n" +
                        "pero el CORREO no pudo ser enviado.\n\n" +
                        "Número: " + res.NumeroNota + "\n\n" +
                        "Puede reenviarlo desde Consultas.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ✅ ÉXITO TOTAL
                Notificaciones.Show(
                    this,
                    "✅ NOTA DE CRÉDITO ELECTRÓNICA AUTORIZADA\n" +
                    "Número: " + res.NumeroNota,
                    "exito",
                    UsuarioActual,
                    IPActual
                );
            }
            catch (Exception ex)
            {
                Notificaciones.Show(
                    this,
                    "Error procesando Nota pendiente:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
            finally
            {
                // ==========================================================
                // 🔒 SIEMPRE CERRAR PROCESO
                // ==========================================================
                Notificaciones.CerrarProceso(this);
                btnConsultar.PerformClick();
            }
        }

        //Columnas acciones

        private DataTable ConstruirTbNcParaXml(DataTable detalleDb)
        {
            DataTable tb = new DataTable();
            tb.Columns.Add("CANTIDAD", typeof(decimal));
            tb.Columns.Add("PRODUCTO", typeof(string));
            tb.Columns.Add("VALOR", typeof(decimal));
            tb.Columns.Add("TOTAL", typeof(decimal));
            tb.Columns.Add("CODIGO", typeof(string));

            foreach (DataRow r in detalleDb.Rows)
            {
                string producto = r["PRODUCTO"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(producto)) continue;

                decimal precio = 0m;
                decimal.TryParse(r["PRECIO"]?.ToString() ?? "0", out precio);

                // ✅ AHORA SÍ: tomar cantidad real desde el detalle
                decimal cantidad = 0m;
                decimal.TryParse(r["CANTIDAD"]?.ToString() ?? "0", out cantidad);
                if (cantidad <= 0m) cantidad = 1m; // fallback defensivo

                // Total coherente con lo que guardas: si tu detalle ya trae TOTAL úsalo; si no, calcula
                decimal total = 0m;
                if (detalleDb.Columns.Contains("TOTAL"))
                {
                    decimal.TryParse(r["TOTAL"]?.ToString() ?? "0", out total);
                    total = Math.Round(total, 2);
                }
                else
                {
                    total = Math.Round(precio * cantidad, 2);
                }

                string codigo = "";
                try
                {
                    var dsProd = _services.Producto.ConsultaNombre(producto);
                    if (dsProd != null && dsProd.Tables.Count > 0 && dsProd.Tables[0].Rows.Count > 0)
                        codigo = dsProd.Tables[0].Rows[0]["CODIGO"].ToString().Trim();
                }
                catch { /* no revientes por código */ }

                tb.Rows.Add(cantidad, producto, precio, total, codigo);
            }

            return tb;
        }

        private void LimpiarColumnasAcciones()
        {
            if (dgvNotas.Columns.Contains("colProcesar"))
                dgvNotas.Columns.Remove("colProcesar");

            if (dgvNotas.Columns.Contains("colPDF"))
                dgvNotas.Columns.Remove("colPDF");

            if (dgvNotas.Columns.Contains("colXML"))
                dgvNotas.Columns.Remove("colXML");
        }

        private void AgregarColumnasAcciones()
        {
            AgregarColumnasAccionesReportes(
         dgvNotas,
         "colProcesar",
         "PROCESAR",
         100
     );

        }

        private void PintarColumnasProcesar()
        {
            PintarColumnasDocumento(
                 dgvNotas,
                 "NUMERONOTA",
                 "colProcesar",
                 "NOTADECREDITO",
                 _services
             );
        }

        private string GetCellString(DataGridViewRow fila, string colName)
        {
            if (fila == null) return "";
            if (fila.DataGridView == null) return "";
            if (!fila.DataGridView.Columns.Contains(colName)) return "";
            return fila.Cells[colName].Value?.ToString() ?? "";
        }

        // Autoriza y Enviar Correos
        private bool ObtenerXmlFirmadoNotaCredito(
            DataGridViewRow fila,
            string numeroNota,
            out string claveAcceso,
            out string rutaXmlFirmado
        )
        {
            claveAcceso = "";
            rutaXmlFirmado = "";

            try
            {
                string fechaOriginal = fila.Cells["FECHAEMISION"].Value?.ToString() ?? "";
                if (!DateTime.TryParse(fechaOriginal, out DateTime fecha))
                    return false;

                string fechaFormato = fecha.ToString("ddMMyyyy");

                // 🔎 patrón: fecha + numero nota
                string patron = $"*{fechaFormato}*{numeroNota}*.xml";

                // 📁 carpeta XML FIRMADO FACTURA
                string carpeta = Path.Combine(_services.Paths.NotasCredito, "XMLFIRMADOS");

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

        private async void EnviarCorreoNotaCredito(DataGridViewRow fila)
        {
            Notificaciones.Show(
                this,
                "Enviando correo de la Nota de Crédito…",
                "proceso",
                UsuarioActual,
                IPActual
            );

            try
            {
                string numeroNota = GetCellString(fila, "NUMERONOTA")?.Trim();

                if (string.IsNullOrWhiteSpace(numeroNota))
                {
                    Notificaciones.Show(
                        this,
                        "Número de Nota inválido.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================
                // XML FIRMADO + CLAVE ACCESO
                // ==========================================
                bool okXml = ObtenerXmlFirmadoNotaCredito(
                    fila,
                    numeroNota,
                    out string claveAcceso,
                    out string rutaXmlFirmado
                );

                if (!okXml)
                {
                    Notificaciones.Show(
                        this,
                        "No se pudo localizar el XML firmado de la Nota de Crédito.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================
                // CARGAR EMPRESA Y NOTA (BD)
                // ==========================================
                DataSet dsEmp = await Task.Run(() =>
                    _services.Empresa.ConsultaNombre(UsuarioActual)
                );

                DataSet dsNc = await Task.Run(() =>
                    _services.NotaCredito.ConsultarPorNumeroNota(numeroNota)
                );

                if (dsEmp.Tables[0].Rows.Count == 0 ||
                    dsNc.Tables[0].Rows.Count == 0)
                    throw new Exception("Datos no encontrados.");

                // ==========================================
                // OBTENER CLIENTE REAL (CON CORREO)
                // ==========================================
                string cedulaCliente =
                    dsNc.Tables[0].Rows[0]["CLIENTE"]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(cedulaCliente))
                {
                    Notificaciones.Show(
                        this,
                        "La Nota de Crédito no tiene identificación de cliente.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                DataSet dsCliente = await Task.Run(() =>
                    _services.Cliente.ConsultarCedula(cedulaCliente)
                );

                if (dsCliente.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "No se encontró el cliente para enviar el correo.",
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==========================================
                // PROCESAR CORREO (PESADO)
                // ==========================================
                var res = await Task.Run(() =>
                    _services.ProcesosNotaCredito.ProcesarNotaDesdeAutorizacion(
                        numeroNota,
                        claveAcceso,
                        rutaXmlFirmado,
                        dsEmp.Tables[0].Rows[0],
                        dsCliente.Tables[0].Rows[0], // 👈 cliente con correo
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
                    "Error enviando correo de la Nota de Crédito:\n" + ex.Message,
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

        private async void ConsultarAutorizacionNotaPendiente(DataGridViewRow fila)
        {
            try
            {
                string numeroNota = GetCellString(fila, "NUMERONOTA").Trim();

                if (string.IsNullOrWhiteSpace(numeroNota))
                {
                    Notificaciones.Show(this,
                        "Número de Nota inválido.",
                        "advertencia");
                    return;
                }

                // ===============================
                // MOSTRAR PROCESO
                // ===============================
                Notificaciones.Show(this,
                    $"Consultando Nota número: {numeroNota}…",
                    "proceso");

                // ===============================
                // PROCESO PESADO
                // ===============================
                var res = await Task.Run(() =>
                {
                    if (!ObtenerXmlFirmadoNotaCredito(
                        fila,
                        numeroNota,
                        out string claveAcceso,
                        out string rutaXmlFirmado))
                    {
                        return new ResultadoFinalNotaCredito
                        {
                            Exito = false,
                            Mensaje = "No se pudo localizar el XML firmado de la Nota de Crédito."
                        };
                    }

                    var dsEmp = _services.Empresa.ConsultaNombre(UsuarioActual);
                    if (dsEmp.Tables[0].Rows.Count == 0)
                        throw new Exception("Empresa no encontrada.");

                    var dsNc = _services.NotaCredito.ConsultarPorNumeroNota(numeroNota);
                    if (dsNc.Tables[0].Rows.Count == 0)
                        throw new Exception("Nota de crédito no encontrada.");

                    return _services.ProcesosNotaCredito.ProcesarNotaDesdeAutorizacion(
                        numeroNota,
                        claveAcceso,
                        rutaXmlFirmado,
                        dsEmp.Tables[0].Rows[0],
                        dsNc.Tables[0].Rows[0],
                        UsuarioActual,
                        IPActual,
                        "AUTORIZACION"
                    );
                });

                // ===============================
                // RESULTADO
                // ===============================
                Notificaciones.Show(this,
                    res.Mensaje,
                    res.Exito ? "exito" : "error");

                btnConsultar.PerformClick();
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error consultando autorización de la Nota de Crédito:\n" + ex.Message,
                    "error");
            }
            finally
            {
                // 🔒 SIEMPRE cerrar proceso
                Notificaciones.CerrarProceso(this);
            }
        }



    }
}
