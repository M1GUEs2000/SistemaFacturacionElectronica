using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmNuevaNotaCredito : Form
    {

        private readonly AppServices _services;

        public frmNuevaNotaCredito(AppServices services
           )
        {
            _services = services;

            InitializeComponent();
            this.Load += frmNuevaNotaCredito_Load;
        }
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private DataTable _dtOriginal;
        private DataTable _dtNota;

        private bool _suppressSearch = false;

        private void frmNuevaNotaCredito_Load(object sender, EventArgs e)
        {
            ConfigurarGrids();
            VincularEventos();
            CargarComboFacturas();
            NumeroNota();
        }

        private void VincularEventos()
        {
            txtNota.TextChanged += txtNota_TextChanged_Buscar;
            txtNota.KeyDown += txtNota_KeyDown;
            lstNotas.Click += lstNotas_Click;

            dgvOriginal.CellClick += dgvOriginal_CellClick;
            dgvNota.CellClick += dgvNota_CellClick;

            btnGenerar.Click += btnGenerar_Click;
        }

        private void NumeroNota()
        {
            int secuencial = (int)_services.Param.ObtenerSecuencialReal("04");
            lblNumeroNota.Text = secuencial.ToString();
        }

        // ======================================================
        // BUSQUEDA DE FACTURAS
        // ======================================================
        private void CargarComboFacturas()
        {
            txtMotivo.Text = "DEVOLUCION";
            lstNotas.Visible = false;
            LimpiarVistaFactura();
        }

        private void txtNota_TextChanged_Buscar(object sender, EventArgs e)
        {
            if (_suppressSearch) return;

            string termino = txtNota.Text.Trim();

            if (string.IsNullOrWhiteSpace(termino))
            {
                lstNotas.Items.Clear();
                lstNotas.Visible = false;
                return;
            }

            var ds = _services.NotaCredito.BuscarFacturasPorNumero(termino);
            lstNotas.Items.Clear();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                    lstNotas.Items.Add(row["NUMEROFACTURA"].ToString().Trim());

                lstNotas.Visible = true;
            }
            else
            {
                lstNotas.Visible = false;
            }
        }

        private void lstNotas_Click(object sender, EventArgs e)
        {
            if (lstNotas.SelectedItem == null) return;

            string numeroFactura = lstNotas.SelectedItem.ToString().Trim();

            _suppressSearch = true;
            txtNota.Text = numeroFactura;
            lstNotas.Visible = false;
            _suppressSearch = false;

            try
            {
                CargarFacturaSeleccionada(numeroFactura);
                _dtNota.Clear();
                RecalcularTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error seleccionando factura: " + ex.Message);
            }

            txtNota.Focus();
        }

        private void txtNota_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstNotas.Visible && lstNotas.Items.Count > 0)
            {
                lstNotas.Focus();
                lstNotas.SelectedIndex = 0;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                lstNotas.Visible = false;
                return;
            }
        }

        private void LimpiarVistaFactura()
        {
            lblFactura.Text = "";
            lblCedula.Text = "";
            lblFecha.Text = "";
            lblNombre.Text = "";
            txtMotivo.Text = "DEVOLUCION";
            _dtOriginal?.Clear();
            _dtNota?.Clear();
            RecalcularTotales();
        }

        // ======================================================
        // GRIDS
        // ======================================================
        private void ConfigurarGrids()
        {
            _dtOriginal = CrearTablaDetalle();
            _dtNota = CrearTablaDetalle();

            dgvOriginal.AutoGenerateColumns = true;
            dgvNota.AutoGenerateColumns = true;

            dgvOriginal.DataSource = _dtOriginal;
            dgvNota.DataSource = _dtNota;

            AplicarFormatoGrid(dgvOriginal);
            AplicarFormatoGrid(dgvNota);

            RecalcularTotales();
        }

        private DataTable CrearTablaDetalle()
        {
            var dt = new DataTable();
            dt.Columns.Add("CODIGO", typeof(string));
            dt.Columns.Add("PRODUCTO", typeof(string));
            dt.Columns.Add("IVA", typeof(string));        // SI / NO
            dt.Columns.Add("CANTIDAD", typeof(decimal));
            dt.Columns.Add("VALOR", typeof(decimal));     // unitario (estimado)
            dt.Columns.Add("TOTAL", typeof(decimal));     // total línea
            return dt;
        }

        private void AplicarFormatoGrid(DataGridView dgv)
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            if (dgv.Columns.Contains("VALOR"))
                dgv.Columns["VALOR"].DefaultCellStyle.Format = "0.00";
            if (dgv.Columns.Contains("TOTAL"))
                dgv.Columns["TOTAL"].DefaultCellStyle.Format = "0.00";
            if (dgv.Columns.Contains("CANTIDAD"))
                dgv.Columns["CANTIDAD"].DefaultCellStyle.Format = "0.##";

            if (dgv.Columns.Contains("PRODUCTO"))
                dgv.Columns["PRODUCTO"].Width = 260;
        }

        // ======================================================
        // 1) Selección de factura -> Labels + dgvOriginal
        // ======================================================

        private void CargarFacturaSeleccionada(string numeroFactura)
        {
            var dsFac = _services.Facturacion.ConsultarPorNumeroFactura(numeroFactura);

            if (dsFac == null || dsFac.Tables.Count == 0 || dsFac.Tables[0].Rows.Count == 0)
                throw new Exception("No se encontró la factura: " + numeroFactura);

            DataRow first = dsFac.Tables[0].Rows[0];

            // OJO: asegúrate que tu query devuelva estas columnas con estos nombres
            string cedula = first["CLIENTE"].ToString().Trim();
            string fecha = first["FECHA"].ToString().Trim();

            var dsCli = _services.Cliente.ConsultarCedula(cedula);
            string nombre = (dsCli != null && dsCli.Tables.Count > 0 && dsCli.Tables[0].Rows.Count > 0)
                ? dsCli.Tables[0].Rows[0]["NOMBRE"].ToString().Trim()
                : "";

            lblFactura.Text = numeroFactura;
            lblCedula.Text = cedula;
            lblFecha.Text = fecha;
            lblNombre.Text = nombre;

            _dtOriginal.Clear();


            foreach (DataRow r in dsFac.Tables[0].Rows)
            {
                string nombreProd = r["PRODUCTO"].ToString().Trim();

                decimal cantidad = Convert.ToDecimal(r["CANTIDAD"]);
                decimal totalLinea = Convert.ToDecimal(r["TOTAL"]);   // NET (sin IVA), tal como viene de BD
                if (cantidad <= 0m) cantidad = 1m;

                var dsProd = _services.Producto.ConsultaNombre(nombreProd);
                if (dsProd == null || dsProd.Tables.Count == 0 || dsProd.Tables[0].Rows.Count == 0)
                    throw new Exception("Producto no existe en BD: " + nombreProd);

                string codigo = dsProd.Tables[0].Rows[0]["CODIGO"].ToString().Trim();
                string iva   = dsProd.Tables[0].Rows[0]["IVA"].ToString().Trim().ToUpper();

                decimal precioUnitario = cantidad > 0m ? Math.Round(totalLinea / cantidad, 2) : 0m;

                // Guardar valores NET — PrepararNotaCreditoDesdeFactura los recibirá así
                _dtOriginal.Rows.Add(codigo, nombreProd, iva, cantidad, precioUnitario, totalLinea);
            }

            RecalcularTotales();
        }

        // ======================================================
        // 2) Mover líneas con 1 click
        // ======================================================
        private void dgvOriginal_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var drv = dgvOriginal.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (drv == null) return;

            MoverFila(drv.Row, _dtOriginal, _dtNota);
            RecalcularTotales();
        }

        private void dgvNota_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var drv = dgvNota.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (drv == null) return;

            MoverFila(drv.Row, _dtNota, _dtOriginal);
            RecalcularTotales();
        }

        private void MoverFila(DataRow row, DataTable from, DataTable to)
        {
            to.Rows.Add(
                row["CODIGO"],
                row["PRODUCTO"],
                row["IVA"],
                row["CANTIDAD"],
                row["VALOR"],
                row["TOTAL"]
            );

            from.Rows.Remove(row);
        }

        // ======================================================
        // 3) Totales
        // ======================================================
        private void RecalcularTotales()
        {
            decimal cantOri = SumarDecimal(_dtOriginal, "CANTIDAD");
            decimal totalOri = SumarDecimalConIva(_dtOriginal, _services.TarifaIva);

            lblCantidadOriginal.Text = cantOri.ToString("0.##");
            lblTotalOriginal.Text = totalOri.ToString("0.00");

            decimal cantNota = SumarDecimal(_dtNota, "CANTIDAD");
            decimal totalNota = SumarDecimalConIva(_dtNota, _services.TarifaIva);

            lblCantidadNota.Text = cantNota.ToString("0.##");
            lblTotalNota.Text = totalNota.ToString("0.00");
        }

        private decimal SumarDecimal(DataTable dt, string col)
        {
            if (dt == null || dt.Rows.Count == 0) return 0m;
            if (!dt.Columns.Contains(col)) return 0m;

            return dt.AsEnumerable()
                     .Where(r => r[col] != DBNull.Value)
                     .Sum(r => Convert.ToDecimal(r[col]));
        }

        // Suma totales NET + IVA por línea para mostrar el total con impuestos al usuario
        private decimal SumarDecimalConIva(DataTable dt, decimal tarifa)
        {
            if (dt == null || dt.Rows.Count == 0) return 0m;
            if (!dt.Columns.Contains("TOTAL")) return 0m;

            decimal total = 0m;
            bool tieneIvaCol = dt.Columns.Contains("IVA");

            foreach (DataRow r in dt.Rows)
            {
                if (r["TOTAL"] == DBNull.Value) continue;
                decimal linea = Convert.ToDecimal(r["TOTAL"]);
                string ivaFlag = tieneIvaCol ? (r["IVA"]?.ToString() ?? "NO").Trim().ToUpper() : "NO";
                if (ivaFlag == "SI" && tarifa > 0m)
                    linea = Math.Round(linea * (1m + tarifa), 2);
                total += linea;
            }
            return total;
        }

        // ======================================================
        // 4) Generar NC
        // ======================================================
        private async void btnGenerar_Click(object sender, EventArgs e)
        {
            try
            {
                // =============================================================
                // 0) VALIDAR AMBIENTE (PRUEBAS / PRODUCCIÓN)
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
                string numeroFactura = lblFactura.Text?.Trim();

                // ==================================================
                // VALIDACIONES BASE (antes de mostrar "Procesando")
                // ==================================================
                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    Notificaciones.Show(
                        this,
                        "Seleccione una factura.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                if (_dtNota == null || _dtNota.Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "Debe seleccionar al menos un producto para la Nota de Crédito.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMotivo.Text))
                {
                    Notificaciones.Show(
                        this,
                        "El motivo de la Nota de Crédito no puede estar vacío.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                Notificaciones.Show(this, "Procesando Nota de Crédito.", "proceso", UsuarioActual, IPActual);

                // Copia defensiva del detalle
                DataTable detalleSeleccionado = _dtNota.Copy();

                // ==================================================
                // PROCESO ELECTRÓNICO COMPLETO
                // ==================================================
                var resultado = await _services.ProcesosNotaCredito
                    .ProcesarNotaCreditoElectronicaCompletaAsync(
                        numeroFactura,
                        txtMotivo.Text.Trim(),
                        detalleSeleccionado,
                        UsuarioActual,
                        IPActual
                    );

                // ==================================================
                // INTERPRETACIÓN CORRECTA DEL RESULTADO
                // ==================================================

                // ❌ ERROR REAL
                if (!resultado.Exito)
                {
                    Notificaciones.Show(
                        this,
                        resultado.Mensaje,
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    Notificaciones.CerrarProceso(this);
                    LimpiarVistaFactura();
                    return;
                }

                // ⚠️ ENVIADA / PENDIENTE DE AUTORIZACIÓN
                if (resultado.Exito && !resultado.Autorizado)
                {
                    Notificaciones.Show(
                        this,
                        "⚠ NOTA DE CRÉDITO ELECTRÓNICA\n\n" +
                        "La Nota de Crédito fue ENVIADA al SRI\n" +
                        "pero quedó PENDIENTE de autorización.\n\n" +
                        "Número: " + resultado.NumeroNota + "\n\n" +
                        "Puede consultarla más tarde desde Consultas.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    _services.Log.CrearLog(
                        "NOTA CREDITO PENDIENTE AUTORIZACION",
                        UsuarioActual,
                        IPActual,
                        "ClaveAcceso: " + resultado.ClaveAcceso
                    );

                    Notificaciones.CerrarProceso(this);

                    LimpiarVistaFactura();
                    NumeroNota();

                    return;
                }

                // ⚠️ AUTORIZADA PERO SIN CORREO
                if (resultado.Exito && resultado.Autorizado && !resultado.EnvioCorreoExitoso)
                {
                    Notificaciones.Show(
                        this,
                        "⚠ NOTA DE CRÉDITO AUTORIZADA\n\n" +
                        "La Nota de Crédito fue AUTORIZADA correctamente,\n" +
                        "pero el CORREO no pudo ser enviado.\n\n" +
                        "Número: " + resultado.NumeroNota + "\n" +
                        "Puede reenviarlo desde Consultas.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    _services.Log.CrearLog(
                        "NOTA CREDITO AUTORIZADA SIN CORREO",
                        UsuarioActual,
                        IPActual,
                        "ClaveAcceso: " + resultado.ClaveAcceso
                    );

                    Notificaciones.CerrarProceso(this);

                    LimpiarVistaFactura();
                    NumeroNota();

                    return;
                }

                // ✅ ÉXITO TOTAL
                Notificaciones.Show(
                    this,
                    "✅ NOTA DE CRÉDITO ELECTRÓNICA AUTORIZADA\n" +
                    "Número: " + resultado.NumeroNota,
                    "exito",
                    UsuarioActual,
                    IPActual
                );
                Notificaciones.CerrarProceso(this);
                LimpiarVistaFactura();
                NumeroNota();
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);
                Notificaciones.Show(
                    this,
                    "Error generando Nota de Crédito:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
        }

        private async void btnPDF_Click(object sender, EventArgs e)
        {
            await VisualizarPDF();
        }

        private async Task VisualizarPDF()
        {
            try
            {
                string numeroFactura = lblFactura.Text?.Trim();



                // ==================================================
                // VALIDACIONES (IGUAL QUE GENERAR)
                // ==================================================
                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    Notificaciones.Show(
                        this,
                        "Seleccione una factura.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                if (_dtNota == null || _dtNota.Rows.Count == 0)
                {
                    Notificaciones.Show(
                        this,
                        "Debe seleccionar al menos un producto para la Nota de Crédito.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMotivo.Text))
                {
                    Notificaciones.Show(
                        this,
                        "El motivo de la Nota de Crédito no puede estar vacío.",
                        "advertencia",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                // ==================================================
                // COPIA DEFENSIVA
                // ==================================================
                DataTable detalleSeleccionado = _dtNota.Copy();

                // ==================================================
                // PREVIEW (SIN SRI / SIN FIRMA / SIN INSERTAR)
                // ==================================================
                Notificaciones.Show(
                    this,
                    "Generando vista previa del PDF...",
                    "proceso",
                    UsuarioActual,
                    IPActual
                );

                var resultado = await _services.ProcesosNotaCredito
                    .ProcesarNotaCreditoPreviewAsync(
                        numeroFactura,
                        txtMotivo.Text.Trim(),
                        detalleSeleccionado,
                        UsuarioActual
                    );
                Notificaciones.CerrarProceso(this);

                if (!resultado.Exito)
                {
                    Notificaciones.Show(
                        this,
                        resultado.Mensaje,
                        "error",
                        UsuarioActual,
                        IPActual
                    );
                    return;
                }

                Notificaciones.Show(
                    this,
                    "Vista previa generada correctamente.",
                    "exito",
                    UsuarioActual,
                    IPActual
                );

                System.Diagnostics.Process.Start(resultado.RutaPdf);
            }
            catch (Exception ex)
            {
                Notificaciones.CerrarProceso(this);
                Notificaciones.Show(
                    this,
                    "Error generando vista previa:\n" + ex.Message,
                    "error",
                    UsuarioActual,
                    IPActual
                );
            }
        }
    }
}
