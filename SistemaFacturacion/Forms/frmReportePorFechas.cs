using DF_PinPad.Wrapper.Models;
using LogicaNegocios;
using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SistemaFacturacion.ProcesosGeneralesUI;

namespace SistemaFacturacion
{
    public partial class frmReportePorFechas : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private int _batchOk;
        private int _batchPendiente;
        private int _batchError;

        // Columnas del cobro con pinpad (vienen siempre en la consulta, pero se
        // muestran solo al expandir con chkVerTarjeta). El grid las oculta por
        // defecto y las revela junto con ensanchar el form y la tabla.
        // colAnular NO viene de la consulta (es un botón), pero se lista aquí para
        // que aparezca y desaparezca junto con el resto del bloque de tarjeta.
        private static readonly string[] ColumnasTarjeta =
            { "AUTORIZACION", "REFERENCIA", "NUMEROTARJETA", "NOMBREGRUPOTARJETA", "ESTADO", "colAnular" };

        private const int DeltaTarjeta = 620; // cuánto crece form/grid al expandir
        private int _anchoFormOriginal;
        private int _anchoGridOriginal;

        public frmReportePorFechas(AppServices services)
        {
            InitializeComponent();
            btnPendientes.Visible = false;
            _services = services;
            MostrarClientes();
            MostrarProductos();
            MostrarFormaPago();

            gvReporteFecha.CellClick += gvReporteFecha_CellClick;

            // Un solo hook: tras cada rebind del DataSource (consulta, ver
            // pendientes, ver consumidor, recarga) se aplica la visibilidad de
            // las columnas de tarjeta según el estado del expander.
            gvReporteFecha.DataBindingComplete += (s, e) => AplicarVisibilidadTarjeta();
        }

        // ======================================================
        // EXPANDER "VER COLUMNAS DE TARJETA"
        // ======================================================

        private void chkVerTarjeta_CheckedChanged(object sender, EventArgs e)
        {
            // El grid (control hijo) puede crecer sin tope, pero el form NO puede
            // superar el ancho de la pantalla: Windows recorta el tamaño de ventana.
            // Por eso capamos el crecimiento al área de trabajo del monitor y, si se
            // sale por la derecha, corremos el form a la izquierda para que quepa.
            var area = Screen.FromControl(this).WorkingArea;

            if (chkVerTarjeta.Checked)
            {
                int anchoDeseado = Math.Min(_anchoFormOriginal + DeltaTarjeta, area.Width);
                int crecimiento = anchoDeseado - _anchoFormOriginal;

                this.Width = anchoDeseado;
                gvReporteFecha.Width = _anchoGridOriginal + crecimiento;

                if (this.Right > area.Right)
                    this.Left = Math.Max(area.Left, area.Right - this.Width);

                chkVerTarjeta.Text = "Ocultar columnas de tarjeta «";
            }
            else
            {
                this.Width = _anchoFormOriginal;
                gvReporteFecha.Width = _anchoGridOriginal;
                chkVerTarjeta.Text = "Ver columnas de tarjeta »";
            }

            AplicarVisibilidadTarjeta();
        }

        private void AplicarVisibilidadTarjeta()
        {
            bool ver = chkVerTarjeta.Checked;

            foreach (string col in ColumnasTarjeta)
            {
                if (!gvReporteFecha.Columns.Contains(col))
                    continue;

                var columna = gvReporteFecha.Columns[col];
                columna.Visible = ver;

                if (ver)
                {
                    columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    if (columna.Width < 140)
                        columna.Width = 150;
                }
            }
        }

        private static void RepetirDatosTarjetaPorFactura(DataTable tabla)
        {
            if (tabla == null ||
                !tabla.Columns.Contains("NUMEROFACTURA") ||
                !tabla.Columns.Contains("AUTORIZACION"))
                return;

            string[] camposTarjeta =
            {
                "AUTORIZACION",
                "REFERENCIA",
                "NUMEROTARJETA",
                "NOMBREGRUPOTARJETA",
                "ESTADO"
            };

            string[] camposIdentificacionTarjeta =
            {
                "AUTORIZACION",
                "REFERENCIA",
                "NUMEROTARJETA",
                "NOMBREGRUPOTARJETA"
            };

            var datosPorFactura = tabla.AsEnumerable()
                .Where(fila => !fila.IsNull("NUMEROFACTURA"))
                .GroupBy(fila => fila.Field<string>("NUMEROFACTURA")?.Trim())
                .Where(grupo => !string.IsNullOrWhiteSpace(grupo.Key));

            foreach (var factura in datosPorFactura)
            {
                DataRow filaConTarjeta = factura.FirstOrDefault(fila =>
                    camposIdentificacionTarjeta.Any(campo =>
                        tabla.Columns.Contains(campo) &&
                        !fila.IsNull(campo) &&
                        !string.IsNullOrWhiteSpace(fila[campo].ToString())));

                if (filaConTarjeta == null)
                    continue;

                foreach (DataRow fila in factura)
                {
                    foreach (string campo in camposTarjeta)
                    {
                        if (tabla.Columns.Contains(campo))
                            fila[campo] = filaConTarjeta[campo];
                    }
                }
            }
        }

        // ======================================================
        // CARGA COMBOS
        // ======================================================

        public void MostrarClientes()
        {
            try
            {
                var dsDato = _services.Facturacion.ConsultarCliente();
                cbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
                cbCliente.DisplayMember = "NOMBRE";
                cbCliente.ValueMember = "CEDULA";
                cbCliente.DataSource = dsDato.Tables[0];
                cbCliente.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        public void MostrarProductos()
        {
            try
            {
                var dsDato = _services.Facturacion.ConsultarProducto();
                cbProducto.DropDownStyle = ComboBoxStyle.DropDownList;
                cbProducto.DataSource = dsDato.Tables[0];
                cbProducto.DisplayMember = "PRODUCTO";
                cbProducto.ValueMember = "PRODUCTO";
                cbProducto.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        public void MostrarFormaPago()
        {
            try
            {
                var dsDato = _services.Facturacion.ConsultarFormaPago();
                cmbFormaPago.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbFormaPago.DataSource = dsDato.Tables[0];
                cmbFormaPago.DisplayMember = "FORMADEPAGO";
                cmbFormaPago.ValueMember = "FORMADEPAGO";
                cmbFormaPago.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message);
            }
        }

        private bool EsSeleccionPorDefecto(ComboBox combo)
        {
            string valor = combo?.SelectedValue?.ToString();
            string texto = combo?.Text;

            if (!string.IsNullOrWhiteSpace(valor) &&
                valor.Trim().Equals("seleccione", StringComparison.OrdinalIgnoreCase))
                return true;

            return string.IsNullOrWhiteSpace(valor) &&
                   !string.IsNullOrWhiteSpace(texto) &&
                   texto.Trim().Equals("seleccione", StringComparison.OrdinalIgnoreCase);
        }

        private void frmReportePorFechas_Load(object sender, EventArgs e)
        {
            // Anchos base ya escalados por la fuente del monitor real (AutoScaleMode.Font).
            _anchoFormOriginal = this.Width;
            _anchoGridOriginal = gvReporteFecha.Width;
        }

        // ======================================================
        // BOTÓN CONSULTAR
        // ======================================================

        private void btnConsultar_Click(object sender, EventArgs e)
        {

            DataSet dsDatos = new DataSet();

            string FechaDesde = dtpFechaDesde.Text;
            string FechaHasta = dtpFechaHasta.Text;

            bool sinCliente = EsSeleccionPorDefecto(cbCliente);
            string cedulaCliente = sinCliente ? "" : cbCliente.SelectedValue?.ToString();

            bool sinProducto = EsSeleccionPorDefecto(cbProducto);
            bool sinFormaPago = EsSeleccionPorDefecto(cmbFormaPago);

            bool soloFechas = sinCliente && sinProducto && sinFormaPago;
            bool soloCliente = (!sinCliente && sinProducto && sinFormaPago);

            // =====================================================================
            // 1️⃣ SOLO FECHAS → AGRUPADO (DEBE MOSTRAR FACTURAR – PDF – XML)
            // =====================================================================
            if (soloFechas)
            {
                dsDatos = _services.Facturacion.ConsultarFechas(FechaDesde, FechaHasta);

                RepetirDatosTarjetaPorFactura(dsDatos.Tables[0]);
                gvReporteFecha.DataSource = dsDatos.Tables[0];
                gvReporteFecha.Columns["CEDULA"].Visible = false;

                // ✔ agregar siempre las columnas porque es agrupado
                AgregarColumnasAcciones();
                PintarColumnasFacturar();
                AjustarAnchoNumeroFactura();
                AjustarAnchoCliente();
            }

            // =====================================================================
            // 2️⃣ SOLO CLIENTE → AGRUPADO POR CLIENTE (DEBE MOSTRAR COLUMNA ACCIONES)
            // =====================================================================
            else if (soloCliente)
            {
                dsDatos = _services.Facturacion.ConsultarFechasPorCliente(FechaDesde, FechaHasta, cedulaCliente);

                RepetirDatosTarjetaPorFactura(dsDatos.Tables[0]);
                gvReporteFecha.DataSource = dsDatos.Tables[0];
                gvReporteFecha.Columns["CEDULA"].Visible = false;

                // ✔ agregar siempre las columnas porque es agrupado por cliente
                AgregarColumnasAcciones();
                PintarColumnasFacturar();
                AjustarAnchoNumeroFactura();
                AjustarAnchoCliente();
            }

            // =====================================================================
            // 3️⃣ CONSULTA DETALLADA → NO deben mostrarse FACTURAR / PDF / XML
            // =====================================================================
            else
            {
                dsDatos = _services.Facturacion.ConsultarFecha(
                    FechaDesde,
                    FechaHasta,
                    sinProducto ? "seleccione" : cbProducto.Text,
                    cedulaCliente,
                    sinFormaPago ? "seleccione" : cmbFormaPago.Text
                );

                RepetirDatosTarjetaPorFactura(dsDatos.Tables[0]);
                gvReporteFecha.DataSource = dsDatos.Tables[0];
                gvReporteFecha.Columns["CEDULA"].Visible = false;

                // ❗ BORRAR LAS COLUMNAS
                LimpiarColumnasAcciones();

                // ANULAR sí aplica: la consulta detallada también trae AUTORIZACION y
                // ESTADO. Como es una fila por producto, el mismo cobro aparece repetido
                // en varias filas; anular desde cualquiera de ellas es lo mismo.
                AgregarColumnaAnular();
                PintarColumnaAnular();

                AjustarAnchoNumeroFactura();
                AjustarAnchoCliente();
            }

            // =====================================================================
            // CALCULAR TOTALES
            // =====================================================================
            if (dsDatos.Tables[0].Rows.Count > 0)
            {
                DataTable tabla = dsDatos.Tables[0];

                string campoTotal = tabla.Columns.Contains("TOTALES") ? "TOTALES" : "TOTAL";
                object SumaValor = tabla.Compute($"SUM({campoTotal})", "");
                lblTotales.Text = "TOTAL: " + Convert.ToDecimal(SumaValor).ToString("0.00");

                string campoCant = tabla.Columns.Contains("CANTIDADES") ? "CANTIDADES" : "CANTIDAD";
                object SumaCantidad = tabla.Compute($"SUM({campoCant})", "");
                lblTotalCantidad.Text = "TOTAL CANTIDAD: " + Convert.ToDecimal(SumaCantidad).ToString("0");
            }
            else
            {
                gvReporteFecha.DataSource = null;
                lblTotales.Text = "0.00";
                lblTotalCantidad.Text = "0.00";
            }

            bool hayPendientes = dsDatos.Tables[0].AsEnumerable()
            .Any(r => r["NUMEROFACTURA"].ToString().StartsWith("PENDIENTE"));

            btnPendientes.Visible = hayPendientes;
        }

        // ======================================================
        // LOAD
        // ======================================================


        // ======================================================
        // CLICK ACCIONES (FACTURAR / PDF / XML)
        // ======================================================

        private void gvReporteFecha_CellClick(object sender, DataGridViewCellEventArgs e)
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

                // ANULACIÓN DE COBRO CON TARJETA
                // Va antes del filtro de abajo y con su propio flujo: no depende del
                // estado del documento en el SRI (una factura autorizada igual puede
                // tener el cobro anulado), sino del estado del cobro en el pinpad.
                if (columna == "colAnular")
                {
                    if (fila.Cells["colAnular"].Value?.ToString().Trim().Length > 0)
                        AnularCobroTarjeta(fila);

                    return;
                }

                if (columna != "colAcciones" &&
                    columna != "colPDF" &&
                    columna != "colXML")
                    return;

                string numeroFactura = fila.Cells["NUMEROFACTURA"].Value?
                    .ToString()
                    .Trim()
                    .ToUpperInvariant() ?? "";

                if (string.IsNullOrWhiteSpace(numeroFactura))
                    return;

                string cliente = fila.Cells["CLIENTE"].Value?
                    .ToString()
                    .Trim()
                    .ToUpperInvariant() ?? "";

                bool esConsumidorFinal =
                    cliente == "FINAL" ||
                    numeroFactura.StartsWith("FINAL");

                var accionDoc = _services.Pendientes.ConsultarAccionPendienteDocumento(numeroFactura, "FACTURA");
                bool esNoAutorizado = accionDoc.Accion == "NO_AUTORIZADO";

                // ==================================================
                // PDF / XML → MÉTODO GLOBAL
                // ==================================================
                if (columna == "colPDF")
                {
                    if (!esConsumidorFinal && !esNoAutorizado)
                    {
                        AbrirDocumentoPorGrid(
                            fila,
                            "FECHA",
                            numeroFactura,
                            _services.Paths.Facturas,   // carpeta base
                            "PDF",                      // subcarpeta
                            "pdf",
                            "Factura"
                        );
                    }
                    return;
                }

                if (columna == "colXML")
                {
                    if (!esConsumidorFinal && !esNoAutorizado)
                    {
                        AbrirDocumentoPorGrid(
                            fila,
                            "FECHA",
                            numeroFactura,
                            _services.Paths.Facturas,   // carpeta base
                            "XMLAUTORIZADOS",           // subcarpeta correcta
                            "xml",
                            "Factura"
                        );
                    }
                    return;
                }

                // ==================================================
                // DESDE AQUÍ SOLO ACCIONES
                // ==================================================
                var accionPendiente =
                    _services.Pendientes
                        .ConsultarAccionPendienteDocumento(
                            numeroFactura,
                            "FACTURA"
                        );

                if (esConsumidorFinal)
                {
                    accionPendiente.Accion = "PROCESAR";
                    accionPendiente.Existe = true;
                }

                if (!accionPendiente.Existe)
                    return;

                if (columna == "colAcciones")
                {
                    switch (accionPendiente.Accion)
                    {
                        // ==========================================
                        // FACTURAR (NORMAL / CONSUMIDOR FINAL)
                        // ==========================================
                        case "PROCESAR":
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
                                // CONSUMIDOR FINAL
                                if (esConsumidorFinal)
                                {
                                    frmPopUpCedula popup = new frmPopUpCedula();
                                    var r = popup.ShowDialog(this);

                                    if (r != DialogResult.OK || !popup.UsuarioConfirmo)
                                    {
                                        Notificaciones.Show(this,
                                            "Operación cancelada.",
                                            "advertencia");
                                        return;
                                    }

                                    string cedula = popup.CedulaIngresada;

                                    DataSet dsCli =
                                        _services.Cliente.ConsultarCedula(cedula);

                                    if (dsCli.Tables[0].Rows.Count == 0)
                                    {
                                        string nuevaCedula =
                                            AbrirRegistroClienteDesdeReporte(cedula);

                                        if (string.IsNullOrEmpty(nuevaCedula))
                                        {
                                            Notificaciones.Show(this,
                                                "No se registró el cliente.",
                                                "advertencia");
                                            return;
                                        }

                                        cedula = nuevaCedula;
                                    }

                                    FacturarConsumidor(fila, cedula);
                                    return;
                                }

                                // FACTURA NORMAL / PENDIENTE
                                string cedulaReal =
                                    fila.Cells["CEDULA"]?.Value?.ToString()?.Trim();

                                if (string.IsNullOrEmpty(cedulaReal))
                                {
                                    Notificaciones.Show(this,
                                        "No se encontró la cédula del cliente.",
                                        "error");
                                    return;
                                }

                                FacturarPendiente(fila, cedulaReal);
                                return;
                            }

                        // ==========================================
                        // AUTORIZAR (PENDIENTE_AUTORIZACION)
                        // ==========================================
                        case "AUTORIZAR":
                            ConsultarAutorizacionFacturaPendiente(fila);
                            return;

                        // ==========================================
                        // ENVIAR CORREO
                        // ==========================================
                        case "CORREO":
                            EnviarCorreoFacturaPendiente(fila);
                            return;

                        default:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error en la tabla de Facturación:\n" + ex.Message,
                    "error");
            }
        }

        // ======================================================
        // AGREGAR COLUMNAS ACCIONES
        // ======================================================

        private async void FacturarConsumidor(DataGridViewRow fila, string cedulaPopup)
        {
            try
            {

                // ============================
                // DATOS VIEJOS
                // ============================
                string fechaVieja = fila.Cells["FECHA"].Value.ToString();
                string horaVieja = fila.Cells["HORA"].Value.ToString();
                string numeroViejo = fila.Cells["NUMEROFACTURA"].Value.ToString().Trim();

                DataSet facturaCompleta =
                    _services.Facturacion.ConsultarFacturaConsumidorFinal(
                        fechaVieja, horaVieja, numeroViejo
                    );

                if (facturaCompleta.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(this,
                        "No se encontraron productos de la factura vieja.",
                        "error");
                    return;
                }

                // ============================
                // ARMAR DETALLE
                // ============================
                DataTable detalle = new DataTable();
                detalle.Columns.Add("CANTIDAD");
                detalle.Columns.Add("PRODUCTO");
                detalle.Columns.Add("VALOR");
                detalle.Columns.Add("TOTAL");

                string formaPago = "";
                decimal totalFactura = 0m;

                foreach (DataRow r in facturaCompleta.Tables[0].Rows)
                {
                    detalle.Rows.Add(
                        r["CANTIDAD"].ToString(),
                        r["PRODUCTO"].ToString(),
                        r["TOTAL"].ToString(),
                        r["TOTAL"].ToString()
                    );

                    formaPago = r["FORMADEPAGO"].ToString();

                    if (decimal.TryParse(r["TOTAL"].ToString(), out decimal t))
                        totalFactura += t;
                }

                string fechaNueva = DateTime.Now.ToString("yyyy/MM/dd");
                string horaNueva = DateTime.Now.ToString("HH:mm:ss");

                Notificaciones.Show(this,
                    $"Factura {numeroViejo} enviada a procesamiento…",
                    "proceso");

                // ============================
                // PARÁMETROS (ANTES DE ENCOLAR)
                // ============================
                var em = _services.Empresa.MostrarEmpresa(UsuarioActual);
                var pfm = _services.ParamFactura.ConsultarNombre(UsuarioActual);
                var pm = _services.Param.ConsultarPorTipo("01");
                var dsCli = _services.Cliente.ConsultarCedula(cedulaPopup);

                if (dsCli.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(this, "Cliente no encontrado.", "error");
                    Notificaciones.CerrarProceso(this);
                    return;
                }

                DataRow rowCliente = dsCli.Tables[0].Rows[0];

                // ============================
                // FIFO
                // ============================
                _services.FacturacionQueue.Enqueue(async () =>
                {
                    var resultado = await _services.ProcesosFacturacion
                        .ProcesarFacturaElectronicaCompleta(
                            em,
                            pfm,
                            pm,
                            rowCliente,
                            detalle,
                            cedulaPopup,
                            "SI",
                            fechaNueva,
                            totalFactura.ToString("0.00"),
                            formaPago,
                            "Gracias por su compra!",
                            UsuarioActual,
                            IPActual,
                            numeroViejo
                        );

                    this.Invoke(new Action(() =>
                    {
                        // -------------------------
                        // ERROR GENERAL
                        // -------------------------
                        if (!resultado.Exito)
                        {
                            Notificaciones.Show(
                                this,
                                resultado.Mensaje,
                                "error",
                                UsuarioActual,
                                IPActual
                            );
                            btnConsultar.PerformClick();

                            Notificaciones.CerrarProceso(this);
                            RecargarSoloFechas();
                            return;
                        }

                        // -------------------------
                        // PENDIENTE AUTORIZACIÓN
                        // -------------------------
                        if (resultado.Exito && !resultado.Autorizado)
                        {
                            Notificaciones.Show(
                                this,
                                "⚠ FACTURACIÓN ELECTRÓNICA Nº: " + resultado.NumeroFactura + "\n\n" +
                                "Documento enviado al SRI.\n" +
                                "Estado: PENDIENTE DE AUTORIZACIÓN.\n\n" +
                                "Puede consultarlo más tarde desde Consultas.",
                                "advertencia"
                            );

                            _services.Log.CrearLog(
                                "FACTURA PENDIENTE AUTORIZACION (CONSULTA)",
                                UsuarioActual,
                                IPActual,
                                "ClaveAcceso: " + resultado.ClaveAcceso
                            );

                            _services.Facturacion.EliminarPorSecuencial(
                                numeroViejo,
                                UsuarioActual,
                                IPActual
                            );

                            btnConsultar.PerformClick();

                            Notificaciones.CerrarProceso(this);
                            RecargarSoloFechas();
                            return;
                        }

                        // -------------------------
                        // AUTORIZADO
                        // -------------------------
                        if (resultado.Autorizado)
                        {

                            _services.Facturacion.EliminarPorSecuencial(
                                numeroViejo,
                                UsuarioActual,
                                IPActual
                            );

                            if (!resultado.EnvioCorreoExitoso)
                            {
                                Notificaciones.Show(
                                    this,
                                    "⚠ FACTURA AUTORIZADA\n\n" +
                                    "La factura fue autorizada correctamente,\n" +
                                    "pero NO se pudo enviar el correo.\n\n" +
                                    "Puede reenviarlo desde Consultas.",
                                    "advertencia",
                                    UsuarioActual,
                                    IPActual
                                );
                            }
                            else
                            {
                                Notificaciones.Show(
                                    this,
                                    "Factura electrónica AUTORIZADA: " + resultado.NumeroFactura +
                                    "\nCliente: " + cedulaPopup,
                                    "exito"
                                );
                            }
                            btnConsultar.PerformClick();

                            Notificaciones.CerrarProceso(this);
                            RecargarSoloFechas();
                            return;
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error FINAL: " + ex.Message,
                    "error");
                Notificaciones.CerrarProceso(this);
            }
        }

        private async void FacturarPendiente(DataGridViewRow fila, string cedulaReal)
        {
            try
            {

                string numeroViejo = fila.Cells["NUMEROFACTURA"].Value.ToString().Trim();

                Notificaciones.Show(this,
                    $"Factura {numeroViejo} enviada a procesamiento…",
                    "proceso");

                _services.FacturacionQueue.Enqueue(async () =>
                {
                    var resultado = await ProcesarPendienteAsync(fila);

                    this.Invoke(new Action(() =>
                    {
                        // -------------------------
                        // ERROR GENERAL
                        // -------------------------
                        if (!resultado.Exito)
                        {
                            Notificaciones.Show(
                                this,
                                resultado.Mensaje,
                                "error",
                                UsuarioActual,
                                IPActual
                            );

                            btnConsultar.PerformClick();

                            Notificaciones.CerrarProceso(this);
                            return;
                        }

                        // -------------------------
                        // PENDIENTE AUTORIZACIÓN
                        // -------------------------
                        if (resultado.Exito && !resultado.Autorizado)
                        {
                            Notificaciones.Show(
                                this,
                                "⚠ FACTURACIÓN ELECTRÓNICA Nº: " + resultado.NumeroFactura + "\n\n" +
                                "Documento enviado al SRI.\n" +
                                "Estado: PENDIENTE DE AUTORIZACIÓN.\n\n" +
                                "Puede consultarlo más tarde desde Consultas.",
                                "advertencia"
                            );

                            _services.Log.CrearLog(
                                "FACTURA PENDIENTE AUTORIZACION (REPROCESO)",
                                UsuarioActual,
                                IPActual,
                                "ClaveAcceso: " + resultado.ClaveAcceso
                            );

                            btnConsultar.PerformClick();

                            Notificaciones.CerrarProceso(this);
                            return;
                        }

                        // -------------------------
                        // AUTORIZADO
                        // -------------------------
                        if (resultado.Autorizado)
                        {
                            if (!resultado.EnvioCorreoExitoso)
                            {
                                Notificaciones.Show(
                                    this,
                                    "⚠ FACTURA AUTORIZADA\n\n" +
                                    "La factura fue autorizada correctamente,\n" +
                                    "pero NO se pudo enviar el correo.\n\n" +
                                    "Puede reenviarlo desde Consultas.",
                                    "advertencia"
                                );
                            }
                            else
                            {
                                Notificaciones.Show(
                                    this,
                                    "Factura pendiente AUTORIZADA correctamente.\n" +
                                    "Nueva factura: " + resultado.NumeroFactura,
                                    "exito"
                                );
                            }

                            Notificaciones.CerrarProceso(this);
                            RecargarSoloFechas();
                            btnConsultar.PerformClick();
                            return;
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error PENDIENTE: " + ex.Message,
                    "error");
            }
        }

        private async Task<ResultadoFinalFactura> ProcesarPendienteAsync(
    DataGridViewRow fila
)
        {
            string numeroViejo = fila.Cells["NUMEROFACTURA"].Value.ToString().Trim();
            string cedula = fila.Cells["CEDULA"].Value.ToString().Trim();

            if (!numeroViejo.StartsWith("PENDIENTE", StringComparison.OrdinalIgnoreCase))
                throw new Exception("No es pendiente.");

            string fechaVieja = fila.Cells["FECHA"].Value.ToString();
            string horaVieja = fila.Cells["HORA"].Value.ToString();

            var dsFactura = _services.Facturacion.ConsultarFacturaNormal(
                fechaVieja, horaVieja, cedula, numeroViejo
            );

            if (dsFactura.Tables[0].Rows.Count == 0)
                throw new Exception("Factura sin detalle.");

            // ============================
            // ARMAR DETALLE
            // ============================
            DataTable detalle = new DataTable();
            detalle.Columns.Add("CANTIDAD");
            detalle.Columns.Add("PRODUCTO");
            detalle.Columns.Add("VALOR");
            detalle.Columns.Add("TOTAL");

            string formaPago = "";

            foreach (DataRow r in dsFactura.Tables[0].Rows)
            {
                detalle.Rows.Add(
                    r["CANTIDAD"].ToString(),
                    r["PRODUCTO"].ToString(),
                    r["TOTAL"].ToString(),
                    r["TOTAL"].ToString()
                );

                formaPago = r["FORMADEPAGO"].ToString();
            }

            // ============================
            // PARÁMETROS
            // ============================
            var em = _services.Empresa.MostrarEmpresa(UsuarioActual);
            var pfm = _services.ParamFactura.ConsultarNombre(UsuarioActual);
            var pm = _services.Param.ConsultarPorTipo("01");

            var dsCli = _services.Cliente.ConsultarCedula(cedula);
            if (dsCli.Tables[0].Rows.Count == 0)
                throw new Exception("Cliente no encontrado.");

            DataRow rowCliente = dsCli.Tables[0].Rows[0];

            // ============================
            // PROCESO ELECTRÓNICO ASYNC
            // ============================
            var resultado = await _services.ProcesosFacturacion.ProcesarFacturaElectronicaCompleta(
                em,
                pfm,
                pm,
                rowCliente,
                detalle,
                cedula,
                "SI",
                DateTime.Now.ToString("yyyy/MM/dd"),
                (gvReporteFecha.Columns["TOTALES"] != null ? fila.Cells["TOTALES"] : fila.Cells["TOTAL"]).Value?.ToString() ?? "",
                formaPago,
                "Gracias por su compra!",
                UsuarioActual,
                IPActual,
                numeroViejo   // ← CLAVE: viene de pendiente
            );

            // ============================
            // LIMPIEZA SI AUTORIZADO
            // ============================
            // Solo eliminar el registro viejo si el proceso fue exitoso
            // (PENDIENTE o AUTORIZADO). Si hubo error (!Exito), se conserva.
            if (resultado.Exito)
            {
                _services.Facturacion.EliminarPorSecuencial(
                    numeroViejo,
                    UsuarioActual,
                    IPActual
                );
            }

            return resultado;
        }

        private bool ProcesarTodasPendientes(
            DataGridViewRow fila,
            ref string error
        )
        {
            try
            {
                var resultado = ProcesarPendienteAsync(fila)
                    .GetAwaiter()
                    .GetResult();

                // -------------------------
                // ERROR
                // -------------------------
                if (!resultado.Exito)
                {
                    _batchError++;
                    error = resultado.Mensaje;
                    return false;
                }

                // -------------------------
                // PENDIENTE AUTORIZACIÓN
                // -------------------------
                if (resultado.Exito && !resultado.Autorizado)
                {
                    _batchPendiente++;
                    error = "";
                    return true;
                }

                // -------------------------
                // AUTORIZADO
                // -------------------------
                if (resultado.Autorizado)
                {
                    _batchOk++;
                    error = "";
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                _batchError++;
                error = ex.Message;
                return false;
            }
        }


        private string AbrirRegistroClienteDesdeReporte(string cedula)
        {
            frmClientesDatos ventana = new frmClientesDatos(_services, true);
            ventana.UsuarioActual = this.UsuarioActual;
            ventana.IPActual = this.IPActual;

            // CONSULTAR CLIENTE
            DataSet ds = _services.Cliente.ConsultarCedula(cedula);

            // EXISTE → editar
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];

                ventana.txtCedula.Text = row["CEDULA"].ToString();
                ventana.txtNombre.Text = row["NOMBRE"].ToString();
                ventana.txtCorreo.Text = row["CORREO"].ToString();
                ventana.txtDireccion.Text = row["DIRECCION"].ToString();
                ventana.txtTelefono.Text = row["TELEFONO"].ToString();

                ventana.btnGuardar.Visible = false;
                ventana.btnModificar.Visible = true;
                ventana.btnEliminar.Visible = true;
            }
            else
            {
                // NO EXISTE → crear
                ventana.txtCedula.Text = cedula;
                ventana.btnGuardar.Visible = true;
                ventana.btnModificar.Visible = false;
                ventana.btnEliminar.Visible = false;
            }

            if (ventana.ShowDialog() == DialogResult.OK &&
                !string.IsNullOrEmpty(ventana.CedulaRegistrada))
            {
                return ventana.CedulaRegistrada;
            }

            return null;
        }

        private async void btnPendientes_Click(object sender, EventArgs e)
        {
            try
            {
                // ========================================
                // 1) RECOPILAR FILAS PENDIENTES
                // ========================================
                var filasPendientes = gvReporteFecha.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r =>
                        r.Cells["NUMEROFACTURA"].Value != null &&
                        r.Cells["NUMEROFACTURA"].Value.ToString()
                            .StartsWith("PENDIENTE", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.Cells["FECHA"].Value?.ToString())
                    .ThenBy(r => r.Cells["HORA"].Value?.ToString())
                    .ToList();

                if (filasPendientes.Count == 0)
                {
                    Notificaciones.Show(this, "No existen facturas pendientes.", "advertencia");
                    return;
                }

                // ========================================
                // 2) CONVERTIR FILAS → SolicitudItemLote
                // ========================================
                var solicitudes = filasPendientes
                    .GroupBy(f => f.Cells["NUMEROFACTURA"].Value?.ToString()?.Trim() ?? "")
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                    .Select(g => g.First())
                    .Select(f => new SolicitudItemLote
                    {
                        NumeroViejo = f.Cells["NUMEROFACTURA"].Value?.ToString()?.Trim() ?? "",
                        Cedula      = f.Cells["CEDULA"].Value?.ToString()?.Trim() ?? "",
                        Fecha       = f.Cells["FECHA"].Value?.ToString() ?? "",
                        Hora        = f.Cells["HORA"].Value?.ToString() ?? "",
                        Total       = (gvReporteFecha.Columns["TOTALES"] != null ? f.Cells["TOTALES"] : f.Cells["TOTAL"]).Value?.ToString() ?? ""
                    })
                    .ToList();

                // ========================================
                // 3) ENVIAR POR LOTE
                // ========================================
                Notificaciones.Show(this,
                    $"Enviando {solicitudes.Count} factura(s) por lote al SRI…",
                    "proceso");

                ResultadoLote resultado = await _services.ProcesosLote.ProcesarPendientesEnLote(
                    solicitudes,
                    UsuarioActual,
                    IPActual,
                    msg => Notificaciones.ActualizarProceso(this, msg)
                );

                // ========================================
                // 4) MOSTRAR RESULTADO
                // ========================================
                Notificaciones.CerrarProceso(this);

                string mensajeFinal =
                    $"✔ Autorizadas:   {resultado.Autorizados}\n" +
                    $"⏳ Pendientes:    {resultado.PendienteAutorizacion}\n" +
                    $"❌ Errores:       {resultado.Errores}";

                var detallesConProblema = resultado.Items
                    .Where(i => !i.Exito || !i.EnvioCorreoExitoso)
                    .Select(i => $"  {i.NumeroViejo}: {i.Mensaje}")
                    .ToList();

                if (detallesConProblema.Count > 0)
                    mensajeFinal += "\n\nDetalles:\n" + string.Join("\n", detallesConProblema);

                string tipoNotif = resultado.Errores > 0 ? "error" :
                                   resultado.PendienteAutorizacion > 0 ? "advertencia" : "exito";

                Notificaciones.Show(this, mensajeFinal, tipoNotif);

                btnConsultar.PerformClick();
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);
                Notificaciones.Show(this, "Error general en envío por lote: " + ex.Message, "error");
            }
        }
        private void btnVerPendientes_Click(object sender, EventArgs e)
        {
            try
            {
                string desde = dtpFechaDesde.Text;
                string hasta = dtpFechaHasta.Text;

                var ds = _services.Facturacion.ConsultarPendientesPorFecha(desde, hasta);
                RepetirDatosTarjetaPorFactura(ds.Tables[0]);
                gvReporteFecha.DataSource = ds.Tables[0];
                gvReporteFecha.Columns["CEDULA"].Visible = false;

                AgregarColumnasAcciones();
                PintarColumnasFacturar();

                btnPendientes.Visible = ds.Tables[0].Rows.Count > 0;
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error cargando pendientes: " + ex.Message, "error");
            }
        }

        private void btnVerConsumidor_Click(object sender, EventArgs e)
        {
            btnPendientes.Visible = false;

            try
            {
                string desde = dtpFechaDesde.Text;
                string hasta = dtpFechaHasta.Text;

                var ds = _services.Facturacion.ConsultarConsumidorFinalPorFecha(desde, hasta);
                RepetirDatosTarjetaPorFactura(ds.Tables[0]);
                gvReporteFecha.DataSource = ds.Tables[0];
                gvReporteFecha.Columns["CEDULA"].Visible = false;

                AgregarColumnasAcciones();
                PintarColumnasFacturar();
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error cargando consumidor final: " + ex.Message, "error");
            }
        }

        private void LimpiarColumnasAcciones()
        {
            if (gvReporteFecha.Columns.Contains("colAcciones"))
                gvReporteFecha.Columns.Remove("colAcciones");

            if (gvReporteFecha.Columns.Contains("colPDF"))
                gvReporteFecha.Columns.Remove("colPDF");

            if (gvReporteFecha.Columns.Contains("colXML"))
                gvReporteFecha.Columns.Remove("colXML");
        }

        private void AgregarColumnasAcciones()
        {
            LimpiarColumnasAcciones();
            AgregarColumnasAccionesReportes(
                gvReporteFecha,
                "colAcciones",
                "ACCIONES",
                150
            );

            AgregarColumnaAnular();
        }

        // ======================================================
        // COLUMNA ANULAR (COBRO CON TARJETA)
        // ======================================================

        /// <summary>
        /// Agrega el botón ANULAR. Va al final a propósito: pertenece al bloque de
        /// tarjeta, que solo se ve al expandir con chkVerTarjeta.
        /// </summary>
        private void AgregarColumnaAnular()
        {
            if (gvReporteFecha.Columns.Contains("colAnular"))
                gvReporteFecha.Columns.Remove("colAnular");

            // Sin ESTADO ni AUTORIZACION no hay forma de saber qué cobro se puede
            // anular (grid vacío o consulta sin las columnas de tarjeta): la columna
            // quedaría siempre en blanco, así que mejor no agregarla.
            if (!gvReporteFecha.Columns.Contains("ESTADO") ||
                !gvReporteFecha.Columns.Contains("AUTORIZACION"))
                return;

            gvReporteFecha.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colAnular",
                HeaderText = "ANULAR",
                UseColumnTextForButtonValue = false,
                Width = 150,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,

                // Nace con la visibilidad del expander. A diferencia del resto de
                // ColumnasTarjeta, esta columna NO viene del DataSource: se agrega
                // DESPUÉS, cuando DataBindingComplete (y con él AplicarVisibilidadTarjeta)
                // ya corrió. Si naciera visible, se quedaría visible.
                Visible = chkVerTarjeta.Checked
            });
        }

        /// <summary>
        /// Escribe "ANULAR" solo en las filas que realmente se pueden anular: cobro
        /// todavía APROBADO y con número de autorización. Sin autorización el datáfono
        /// no puede identificar la transacción, así que ni se ofrece el botón.
        ///
        /// El resto queda con la celda vacía, que es como el DataGridView deja el botón
        /// sin dibujar.
        /// </summary>
        private void PintarColumnaAnular()
        {
            if (!gvReporteFecha.Columns.Contains("colAnular") ||
                !gvReporteFecha.Columns.Contains("ESTADO") ||
                !gvReporteFecha.Columns.Contains("AUTORIZACION"))
                return;

            foreach (DataGridViewRow fila in gvReporteFecha.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                string estado = fila.Cells["ESTADO"].Value?.ToString().Trim().ToUpperInvariant() ?? "";
                string autorizacion = fila.Cells["AUTORIZACION"].Value?.ToString().Trim() ?? "";

                bool anulable = estado == "APROBADA" && autorizacion.Length > 0;

                fila.Cells["colAnular"].Value = anulable ? "ANULAR" : "";
            }
        }

        private void PintarColumnasFacturar()
        {
            PintarColumnasDocumento(
                gvReporteFecha,
                "NUMEROFACTURA",
                "colAcciones",
                "FACTURA",
                _services,
                true
            );

            PintarColumnaAnular();
        }

        private void AjustarAnchoNumeroFactura()
        {
            if (!gvReporteFecha.Columns.Contains("NUMEROFACTURA"))
                return;

            var columna = gvReporteFecha.Columns["NUMEROFACTURA"];
            columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            columna.Width = 130;
        }

        private void AjustarAnchoCliente()
        {
            if (!gvReporteFecha.Columns.Contains("CLIENTE"))
                return;

            var columna = gvReporteFecha.Columns["CLIENTE"];
            columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            columna.Width = 200;
        }


        private void RecargarSoloFechas()
        {
            try
            {
                DataSet dsDatos = new DataSet();

                string FechaDesde = dtpFechaDesde.Text;
                string FechaHasta = dtpFechaHasta.Text;

                dsDatos = _services.Facturacion.ConsultarFechas(FechaDesde, FechaHasta);

                RepetirDatosTarjetaPorFactura(dsDatos.Tables[0]);
                gvReporteFecha.DataSource = dsDatos.Tables[0];
                if (gvReporteFecha.Columns.Contains("CEDULA"))
                    gvReporteFecha.Columns["CEDULA"].Visible = false;

                AgregarColumnasAcciones();
                PintarColumnasFacturar();
                AjustarAnchoNumeroFactura();
                AjustarAnchoCliente();

                // ===========================
                // CALCULAR TOTALES
                // ===========================
                if (dsDatos.Tables.Count > 0 && dsDatos.Tables[0].Rows.Count > 0)
                {
                    DataTable tabla = dsDatos.Tables[0];

                    string campoTotal = tabla.Columns.Contains("TOTALES") ? "TOTALES" : "TOTAL";
                    object sumaValor = tabla.Compute($"SUM({campoTotal})", "");
                    lblTotales.Text = "TOTAL: " + Convert.ToDecimal(sumaValor).ToString("0.00");

                    string campoCant = tabla.Columns.Contains("CANTIDADES") ? "CANTIDADES" : "CANTIDAD";
                    object sumaCantidad = tabla.Compute($"SUM({campoCant})", "");
                    lblTotalCantidad.Text = "TOTAL CANTIDAD: " + Convert.ToDecimal(sumaCantidad).ToString("0");
                }
                else
                {
                    gvReporteFecha.DataSource = null;
                    lblTotales.Text = "0.00";
                    lblTotalCantidad.Text = "0.00";
                }

                // ===========================
                // PENDIENTES
                // ===========================
                bool hayPendientes = dsDatos.Tables.Count > 0 &&
                                     dsDatos.Tables[0].Rows.Count > 0 &&
                                     dsDatos.Tables[0].AsEnumerable()
                                         .Any(r => r["NUMEROFACTURA"].ToString().StartsWith("PENDIENTE"));

                btnPendientes.Visible = hayPendientes;
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error recargando por fechas: " + ex.Message, "error");
            }
        }

        //Metodos extra 

        private bool ObtenerXmlFirmadoFactura(
        DataGridViewRow fila,
        string numeroFactura,
        out string claveAcceso,
        out string rutaXmlFirmado
    )
        {
            claveAcceso = "";
            rutaXmlFirmado = "";

            try
            {
                string fechaOriginal = fila.Cells["FECHA"]?.Value?.ToString() ?? "";
                if (!DateTime.TryParse(fechaOriginal, out DateTime fecha))
                    return false;

                string fechaFormato = fecha.ToString("ddMMyyyy");

                // 🔎 patrón: fecha + número factura
                string patron = $"*{fechaFormato}*{numeroFactura}*.xml";

                // 📁 carpeta XML FIRMADO FACTURA
                string carpeta = Path.Combine(_services.Paths.Facturas, "XMLFIRMADOS");

                if (!Directory.Exists(carpeta))
                    return false;

                string[] archivos = Directory.GetFiles(carpeta, patron);

                if (archivos.Length == 0)
                    return false;

                rutaXmlFirmado = archivos[0];

                // 🔑 claveAcceso = nombre del archivo
                claveAcceso = Path.GetFileNameWithoutExtension(rutaXmlFirmado);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void EnviarCorreoFacturaPendiente(DataGridViewRow fila)
        {
            Notificaciones.Show(
                this,
                "Enviando correo de la Factura…",
                "proceso",
                UsuarioActual,
                IPActual
            );

            try
            {
                string numeroFactura =
                    fila.Cells["NUMEROFACTURA"]?.Value?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    Notificaciones.Show(this,
                        "Número de Factura inválido.",
                        "advertencia",
                        UsuarioActual,
                        IPActual);
                    return;
                }

                // ==========================================
                // XML FIRMADO + CLAVE ACCESO
                // ==========================================
                if (!ObtenerXmlFirmadoFactura(
                    fila,
                    numeroFactura,
                    out string claveAcceso,
                    out string rutaXmlFirmado))
                {
                    Notificaciones.Show(this,
                        "No se pudo localizar el XML firmado de la Factura.",
                        "error",
                        UsuarioActual,
                        IPActual);
                    return;
                }

                // ==========================================
                // EMPRESA
                // ==========================================
                DataSet dsEmp = await Task.Run(() =>
                    _services.Empresa.ConsultaNombre(UsuarioActual)
                );

                if (dsEmp.Tables[0].Rows.Count == 0)
                    throw new Exception("Empresa no encontrada.");

                // ==========================================
                // CLIENTE
                // ==========================================
                string cedula =
                    fila.Cells["CEDULA"]?.Value?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    Notificaciones.Show(this,
                        "No se encontró la cédula del cliente.",
                        "error",
                        UsuarioActual,
                        IPActual);
                    return;
                }

                DataSet dsCliente = await Task.Run(() =>
                    _services.Cliente.ConsultarCedula(cedula)
                );

                if (dsCliente.Tables[0].Rows.Count == 0)
                {
                    Notificaciones.Show(this,
                        "No se encontró el cliente para enviar el correo.",
                        "error",
                        UsuarioActual,
                        IPActual);
                    return;
                }

                // ==========================================
                // FLUJO PESADO
                // ==========================================
                var res = await Task.Run(() =>
                    _services.ProcesosFacturacion.FacturaPendienteAutorizacion(
                        numeroFactura,
                        claveAcceso,
                        rutaXmlFirmado,
                        dsEmp.Tables[0].Rows[0],
                        dsCliente.Tables[0].Rows[0],
                        UsuarioActual,
                        IPActual
                    )
                );

                Notificaciones.Show(this,
                    res.Mensaje,
                    res.Exito ? "exito" : "error",
                    UsuarioActual,
                    IPActual);
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error enviando correo de la Factura:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual);
            }
            finally
            {
                Notificaciones.CerrarProceso(this);
                btnConsultar.PerformClick();
            }
        }

        private async void ConsultarAutorizacionFacturaPendiente(DataGridViewRow fila)
        {
            try
            {
                string numeroFactura =
                    fila.Cells["NUMEROFACTURA"]?.Value?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    Notificaciones.Show(this,
                        "Número de Factura inválido.",
                        "advertencia");
                    return;
                }

                Notificaciones.Show(this,
                    $"Consultando Factura número: {numeroFactura}…",
                    "proceso");

                var res = await Task.Run(() =>
                {
                    if (!ObtenerXmlFirmadoFactura(
                        fila,
                        numeroFactura,
                        out string claveAcceso,
                        out string rutaXmlFirmado))
                    {
                        return new ResultadoFinalFactura
                        {
                            Exito = false,
                            Mensaje = "No se pudo localizar el XML firmado de la Factura."
                        };
                    }

                    var dsEmp = _services.Empresa.ConsultaNombre(UsuarioActual);
                    if (dsEmp.Tables[0].Rows.Count == 0)
                        throw new Exception("Empresa no encontrada.");

                    string cedula =
                        fila.Cells["CEDULA"]?.Value?.ToString()?.Trim();

                    var dsCli = _services.Cliente.ConsultarCedula(cedula);
                    if (dsCli.Tables[0].Rows.Count == 0)
                        throw new Exception("Cliente no encontrado.");

                    return _services.ProcesosFacturacion.FacturaPendienteAutorizacion(
                        numeroFactura,
                        claveAcceso,
                        rutaXmlFirmado,
                        dsEmp.Tables[0].Rows[0],
                        dsCli.Tables[0].Rows[0],
                        UsuarioActual,
                        IPActual
                    );
                });

                Notificaciones.CerrarProceso(this);

                Notificaciones.Show(this,
                    res.Mensaje,
                    res.Exito ? "exito" : "error");

                btnConsultar.PerformClick();
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);

                Notificaciones.Show(this,
                    "Error consultando autorización de la Factura:\n" + ex.Message,
                    "error");
            }
        }

        // ======================================================
        // ANULAR COBRO CON TARJETA (PINPAD)
        // ======================================================

        /// <summary>
        /// Anula en el pinpad el cobro con tarjeta de la factura de la fila.
        ///
        /// La factura electrónica NO se toca: esto reversa el cargo a la tarjeta, que es
        /// una operación del datáfono. Para dejar sin efecto el comprobante ante el SRI
        /// hay que emitir la nota de crédito aparte.
        ///
        /// El identificador que pide la DLL es AUTORIZACION + REFERENCIA + RED; el número
        /// de factura solo sirve para ubicar esa fila en PINPAD_AUTORIZADAS y para dejar
        /// registrada la anulación con la factura real.
        /// </summary>
        private async void AnularCobroTarjeta(DataGridViewRow fila)
        {
            string numeroFactura = fila.Cells["NUMEROFACTURA"]?.Value?.ToString()?.Trim() ?? "";
            string autorizacion = fila.Cells["AUTORIZACION"]?.Value?.ToString()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(numeroFactura) || string.IsNullOrWhiteSpace(autorizacion))
            {
                Notificaciones.Show(this,
                    "La fila no tiene número de factura o autorización:\n" +
                    "no se puede identificar el cobro a anular.",
                    "advertencia");
                return;
            }

            try
            {
                // ==========================================
                // DATOS DEL COBRO ORIGINAL
                // ==========================================
                if (_services.PinPadLog.YaAnulada(numeroFactura))
                {
                    Notificaciones.Show(this,
                        "El cobro de la factura " + numeroFactura + " ya fue anulado.",
                        "advertencia");
                    btnConsultar.PerformClick();
                    return;
                }

                DatosAnulacion datos =
                    _services.PinPadLog.ConsultarCobroParaAnular(numeroFactura, autorizacion);

                if (datos == null)
                {
                    Notificaciones.Show(this,
                        "No se encontró el cobro con tarjeta de la factura " + numeroFactura +
                        "\ncon la autorización " + autorizacion + ".",
                        "error");
                    return;
                }

                if (string.IsNullOrWhiteSpace(datos.Referencia))
                {
                    Notificaciones.Show(this,
                        "El cobro no tiene número de REFERENCIA registrado.\n" +
                        "El datáfono no puede identificar la transacción sin ese dato;\n" +
                        "la anulación debe hacerse desde el equipo.",
                        "error");
                    return;
                }

                // ==========================================
                // CONFIRMACIÓN — mueve plata real
                // ==========================================
                bool confirmado = Notificaciones.Show(
                    this,
                    "⚠ ANULAR COBRO CON TARJETA\n\n" +
                    "Factura:       " + numeroFactura + "\n" +
                    "Monto:         " + datos.Monto.ToString("0.00") + "\n" +
                    "Tarjeta:       " + datos.NumeroTarjeta + " " + datos.NombreGrupoTarjeta + "\n" +
                    "Autorización:  " + datos.Autorizacion + "\n" +
                    "Referencia:    " + datos.Referencia + "\n\n" +
                    "Esta operación reversa el cargo en el datáfono.\n" +
                    "La factura electrónica NO se anula: para eso emita una nota de crédito.\n\n" +
                    "¿Desea continuar?",
                    "confirmacion"
                );

                if (!confirmado)
                    return;

                Notificaciones.Show(this,
                    "Anulando el cobro en el datáfono…",
                    "proceso");

                // ==========================================
                // ANULACIÓN — bloqueante, fuera del hilo de UI
                // ==========================================
                var req = new AnulacionRequest
                {
                    // El selector de red es el mismo que se mandó al cobrar, no el adquirente.
                    Red = string.IsNullOrWhiteSpace(datos.Red) ? "Datafast" : datos.Red,
                    Autorizacion = datos.Autorizacion,
                    Referencia = datos.Referencia,
                    UsuarioSistema = UsuarioActual
                };

                AnulacionResult res = await Task.Run(() => _services.PinPad.Anular(req));

                if (res == null || !res.Exitoso)
                {
                    string motivo = res?.MensajeRespuesta
                        ?? res?.ExcepcionMensaje
                        ?? "Sin respuesta del datáfono.";

                    Notificaciones.Show(this,
                        "No se pudo anular el cobro de la factura " + numeroFactura + ":\n" + motivo,
                        "error",
                        UsuarioActual,
                        IPActual);
                    return;
                }

                // ==========================================
                // REGISTRO CON LA FACTURA REAL
                // ==========================================
                // La DLL ya grabó su propia fila, pero bajo un marcador "OP-..." porque
                // AnulacionRequest no lleva factura. Sin este registro el reporte seguiría
                // mostrando el cobro como APROBADA.
                _services.PinPadLog.RegistrarAnulacion(
                    numeroFactura,
                    datos.Referencia,
                    datos.Autorizacion,
                    string.IsNullOrWhiteSpace(res.RedAdquirente) ? datos.RedAdquirente : res.RedAdquirente);

                _services.Log.CrearLog(
                    "ANULACION COBRO TARJETA",
                    UsuarioActual,
                    IPActual,
                    "Factura: " + numeroFactura +
                    " | Monto: " + datos.Monto.ToString("0.00") +
                    " | Autorizacion: " + datos.Autorizacion +
                    " | Referencia: " + datos.Referencia);

                Notificaciones.Show(this,
                    "Cobro ANULADO correctamente.\n" +
                    "Factura:       " + numeroFactura + "\n" +
                    "Monto:         " + datos.Monto.ToString("0.00") + "\n" +
                    "Autorización:  " + (string.IsNullOrWhiteSpace(res.Autorizacion) ? datos.Autorizacion : res.Autorizacion) + "\n" +
                    "Referencia:    " + (string.IsNullOrWhiteSpace(res.Referencia) ? datos.Referencia : res.Referencia),
                    "exito");

                // El ESTADO de la fila quedó viejo (sigue diciendo APROBADA): recargar.
                btnConsultar.PerformClick();
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this,
                    "Error anulando el cobro con tarjeta:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual);
            }
            finally
            {
                // Solo cierra el "esperando" (si se llegó a abrir). NO se recarga el grid
                // aquí: al cancelar la confirmación o fallar una validación no cambió nada,
                // y recargar sacaría al usuario de las vistas de pendientes/consumidor.
                Notificaciones.CerrarProceso(this);
            }
        }
    }
}
