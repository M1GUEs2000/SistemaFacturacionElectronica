using LogicaNegocios.Procesos;
using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmProductos : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        // Label de feedback ICE (creado programáticamente)
        private Label lblICEDetectado;

        private bool _calculando = false;

        public frmProductos(AppServices services)
        {
            InitializeComponent();
            _services = services;
        }

        private void frmProductos_Load(object sender, EventArgs e)
        {
            MostrarProductos("");
            btnEliminar.Visible = false;
            btnModificar.Visible = false;

            cmbIva.Items.Clear();
            cmbIva.Items.Add("SI");
            cmbIva.Items.Add("NO");
            cmbIva.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbIva.SelectedItem = "NO";

            cmbAplicaICE.Items.Clear();
            cmbAplicaICE.Items.Add("NO");
            cmbAplicaICE.Items.Add("SI");
            cmbAplicaICE.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAplicaICE.SelectedIndex = 0;
            cmbAplicaICE.SelectedIndexChanged += cmbAplicaICE_SelectedIndexChanged;

            cmbTipoICE.Items.Clear();
            cmbTipoICE.Items.Add("");
            cmbTipoICE.Items.Add("AD_VALOREM");
            cmbTipoICE.Items.Add("ESPECIFICO");
            cmbTipoICE.Items.Add("AZUCAR");
            cmbTipoICE.Items.Add("MIXTO");
            cmbTipoICE.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoICE.SelectedIndex = 0;

            cmbTipoVehiculoICE.Items.Clear();
            cmbTipoVehiculoICE.Items.Add("NINGUNO");
            cmbTipoVehiculoICE.Items.Add("CONVENCIONAL");
            cmbTipoVehiculoICE.Items.Add("HIBRIDO");
            cmbTipoVehiculoICE.Items.Add("SENAE");
            cmbTipoVehiculoICE.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoVehiculoICE.SelectedIndex = 0;

            CargarEstado();

            // ── Label de feedback ICE (debajo del campo Nombre, sobre sección ICE)
            lblICEDetectado = new Label();
            lblICEDetectado.AutoSize = false;
            lblICEDetectado.Size = new Size(840, 36);
            lblICEDetectado.Location = new Point(3, 86);
            lblICEDetectado.Font = new Font("Microsoft Sans Serif", 8.5f, FontStyle.Regular);
            lblICEDetectado.ForeColor = Color.DarkSlateGray;
            lblICEDetectado.Text = "";
            pnlDatos.Controls.Add(lblICEDetectado);

            // ── Generar código y detectar ICE cuando el usuario termina de escribir el nombre
            txtNombre.Leave += txtNombre_Leave;

            // ── Recalcular código de vehículo cuando cambia el PVP
            txtPVPICE.Leave += txtPVPICE_Leave;
            ActualizarEstadoCamposICE(true);

            // ── Cálculo bidireccional de IVA
            txtValor.Leave       += txtValor_Leave;
            txtValorSinIVA.Leave += txtValorSinIVA_Leave;
            cmbIva.SelectedIndexChanged += cmbIva_IvaChanged;

            // ── Búsqueda en vivo por nombre
            txtNombreBuscar.TextChanged += TxtNombreBuscar_TextChanged;

            // ── Botón limpiar búsqueda
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        public void MostrarProductos(string Nombre)
        {
            try
            {
                DataSet dsDatos = _services.Producto.MostrarTodoProductos(Nombre);
                gvProductos.DataSource = dsDatos.Tables[0];
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void TxtNombreBuscar_TextChanged(object sender, EventArgs e)
        {
            string termino = txtNombreBuscar.Text.Trim();
            MostrarProductos(termino);
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtNombreBuscar.Clear();
            txtNombreBuscar.Focus();
        }

        private void gvProductos_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (gvProductos.DataSource == null ||
                    gvProductos.Rows.Count == 0 ||
                    gvProductos.SelectedRows.Count == 0)
                    return;

                DataGridViewRow row = gvProductos.SelectedRows[0];

                // Campos base
                txtNombre.Text = CeldaStr(row, "NOMBRE");
                txtValorSinIVA.Text = CeldaStr(row, "VALOR");
                txtOrden.Text       = CeldaStr(row, "ORDEN");
                txtCodigo.Text      = CeldaStr(row, "CODIGO");

                cmbEstado.SelectedItem = CeldaStr(row, "ESTATUS");
                cmbIva.SelectedItem    = CeldaStr(row, "IVA");
                if (cmbIva.SelectedItem == null)
                    cmbIva.SelectedItem = "NO";

                RecalcularConIVADesdeSin();

                // ICE (sólo si la columna ya existe en la BD)
                cmbAplicaICE.SelectedItem       = CeldaStr(row, "APLICA_ICE", "NO");
                if (cmbAplicaICE.SelectedItem == null)
                    cmbAplicaICE.SelectedItem = "NO";
                txtCodigoICE.Text               = CeldaStr(row, "ICE_CODIGO");
                cmbTipoICE.SelectedItem         = CeldaStr(row, "ICE_TIPO");
                txtPorcentajeICE.Text           = CeldaStr(row, "ICE_PORCENTAJE");
                txtValorICE.Text                = CeldaStr(row, "ICE_VALOR");
                txtUnidadICE.Text               = CeldaStr(row, "ICE_UNIDAD");
                txtAzucarICE.Text               = CeldaStr(row, "AZUCAR_GRAMOS");
                txtVolumenICE.Text              = CeldaStr(row, "VOLUMEN");
                txtPVPICE.Text                  = CeldaStr(row, "PVP");
                cmbTipoVehiculoICE.SelectedItem = CeldaStr(row, "TIPO_VEHICULO", "NINGUNO");

                txtNombre.Enabled    = false;
                btnEliminar.Visible  = true;
                btnModificar.Visible = true;
                btnGuardar.Visible   = false;

                // Limpiar mensaje ICE y desbloquear campos al cargar desde la tabla
                if (lblICEDetectado != null) lblICEDetectado.Text = "";
                ActualizarEstadoCamposICE(false);
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        // Lector seguro: busca por DataPropertyName (case-insensitive).
        // Devuelve defecto si la columna no existe o su valor es null.
        private string CeldaStr(DataGridViewRow row, string col, string defecto = "")
        {
            foreach (DataGridViewColumn c in row.DataGridView.Columns)
            {
                if (string.Equals(c.DataPropertyName, col, StringComparison.OrdinalIgnoreCase))
                    return row.Cells[c.Index].Value?.ToString() ?? defecto;
            }
            return defecto;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        public void Limpiar()
        {
            CargarEstado();

            btnEliminar.Visible  = false;
            btnModificar.Visible = false;
            btnGuardar.Visible   = true;
            txtNombre.Enabled    = true;

            // Campos base
            txtNombre.Text    = "";
            txtValor.Text     = "";
            txtValorSinIVA.Text = "";
            txtOrden.Text     = "";
            txtCodigo.Text = "";
            cmbIva.SelectedItem = "NO";

            // ICE
            cmbAplicaICE.SelectedIndex       = 0; // NO
            txtCodigoICE.Text                = "";
            cmbTipoICE.SelectedIndex         = 0; // vacío
            txtPorcentajeICE.Text            = "";
            txtValorICE.Text                 = "";
            txtUnidadICE.Text                = "";
            txtAzucarICE.Text                = "";
            txtVolumenICE.Text               = "";
            txtPVPICE.Text                   = "";
            cmbTipoVehiculoICE.SelectedIndex = 0; // NINGUNO
            ActualizarEstadoCamposICE(true);
        }

        private void CargarEstado()
        {
            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.Items.Clear();
            cmbEstado.Items.Add("ACTIVO");
            cmbEstado.Items.Add("INACTIVO");
            cmbEstado.SelectedIndex = 0;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            try
            {
                DataSet dsDatos = _services.Producto.ConsultaNombre(txtNombre.Text.Trim());
                if (dsDatos.Tables[0].Rows.Count != 0)
                {
                    Notificaciones.Show(this, "Ya existe un producto con el nombre: " + txtNombre.Text, "advertencia");
                    return;
                }

                int fila = _services.Producto.Insertar(
                    txtNombre.Text.Trim(),
                    txtValorSinIVA.Text.Trim(),
                    cmbEstado.Text.Trim(),
                    txtOrden.Text.Trim(),
                    txtCodigo.Text.Trim(),
                    cmbIva.Text.Trim(),
                    cmbAplicaICE.Text.Trim(),
                    txtCodigoICE.Text.Trim(),
                    cmbTipoICE.Text.Trim(),
                    txtPorcentajeICE.Text.Trim(),
                    txtValorICE.Text.Trim(),
                    txtUnidadICE.Text.Trim(),
                    txtAzucarICE.Text.Trim(),
                    txtVolumenICE.Text.Trim(),
                    txtPVPICE.Text.Trim(),
                    cmbTipoVehiculoICE.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (fila == 1)
                {
                    string nombreGuardado = txtNombre.Text.Trim();
                    MostrarProductos("");
                    SeleccionarFila(nombreGuardado);
                    Limpiar();
                    Notificaciones.Show(this, "Registrado correctamente.", "exito");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        public bool Validar()
        {
            var errores = new List<string>();

            FormValidador.Requerido(txtNombre.Text, "Nombre", errores);
            FormValidador.Requerido(txtValorSinIVA.Text, "Valor", errores);
            FormValidador.Requerido(txtOrden.Text,  "Orden",  errores);
            FormValidador.Requerido(cmbIva.Text, "IVA", errores);
            FormValidador.Requerido(cmbAplicaICE.Text, "Aplica ICE", errores);

            // Validaciones ICE (solo si aplica)
            if (cmbAplicaICE.Text == "SI")
            {
                FormValidador.Requerido(txtCodigoICE.Text, "ICE: Código ICE", errores);
                FormValidador.Requerido(cmbTipoICE.Text,   "ICE: Tipo ICE",   errores);

                if (cmbTipoICE.Text.Trim().ToUpperInvariant() == "AZUCAR")
                    FormValidador.Requerido(txtAzucarICE.Text, "ICE: Azúcar (gramos)", errores);

                string tipoVehiculo = cmbTipoVehiculoICE.Text;
                if (tipoVehiculo == "CONVENCIONAL" || tipoVehiculo == "HIBRIDO" || tipoVehiculo == "SENAE")
                    FormValidador.Requerido(txtPVPICE.Text, "ICE: PVP del vehículo", errores);
            }

            return !FormValidador.MostrarErrores(this, errores);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (DialogResult.Yes != MessageBox.Show(
                        "¿Realmente desea eliminar el Producto?", "Mensaje", MessageBoxButtons.YesNo))
                    return;

                int fila = _services.Producto.Eliminar(txtNombre.Text, UsuarioActual, IPActual);
                if (fila == 1)
                {
                    MostrarProductos("");
                    Limpiar();
                    Notificaciones.Show(this, "Eliminado correctamente.", "exito");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void btnModificar_Click_1(object sender, EventArgs e)
        {
            if (!Validar()) return;
            try
            {

                int fila = _services.Producto.Actualizar(
                    txtNombre.Text.Trim(),
                    txtValorSinIVA.Text.Trim(),
                    cmbEstado.Text.Trim(),
                    txtOrden.Text.Trim(),
                    txtCodigo.Text.Trim(),
                    cmbIva.Text.Trim(),
                    cmbAplicaICE.Text.Trim(),
                    txtCodigoICE.Text.Trim(),
                    cmbTipoICE.Text.Trim(),
                    txtPorcentajeICE.Text.Trim(),
                    txtValorICE.Text.Trim(),
                    txtUnidadICE.Text.Trim(),
                    txtAzucarICE.Text.Trim(),
                    txtVolumenICE.Text.Trim(),
                    txtPVPICE.Text.Trim(),
                    cmbTipoVehiculoICE.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (fila == 1)
                {
                    string nombreGuardado = txtNombre.Text.Trim();
                    MostrarProductos("");
                    SeleccionarFila(nombreGuardado);
                    Notificaciones.Show(this, "Actualizado correctamente.", "exito");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        // ====================================================================
        // DETECCIÓN AUTOMÁTICA DE ICE
        // ====================================================================

        // Dispara al salir del campo Nombre
        private void txtNombre_Leave(object sender, EventArgs e)
        {
            // Solo actuar cuando el formulario está en modo Nuevo (Guardar visible)
            if (!btnGuardar.Visible) return;

            // Generar código automático: primeras 11 letras + 3 dígitos aleatorios
            txtCodigo.Text = GenerarCodigoProducto(txtNombre.Text);

            // Detectar ICE
            ResultadoICE resultado = ProcesosICE.DetectarICE(txtNombre.Text);
            AplicarResultadoICE(resultado);
        }

        // Genera código: primeras 11 letras/números del nombre (sin espacios ni tildes)
        // más 3 dígitos aleatorios. Ej: "Pan con Queso" → "panconqueso472"
        private static string GenerarCodigoProducto(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "";

            // Normalizar: quitar tildes
            string sinTildes = nombre.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in sinTildes)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            // Solo letras y números, sin espacios, minúsculas
            var limpio = new StringBuilder();
            foreach (char c in sb.ToString())
            {
                if (char.IsLetterOrDigit(c))
                    limpio.Append(char.ToLowerInvariant(c));
            }

            // Primeras 11 caracteres + 3 dígitos aleatorios
            string texto = limpio.Length > 11 ? limpio.ToString().Substring(0, 11) : limpio.ToString();
            string sufijo = new Random().Next(100, 999).ToString();
            return texto + sufijo;
        }

        // Recalcula el código de vehículo cuando cambia el PVP
        private void txtPVPICE_Leave(object sender, EventArgs e)
        {
            string tipoVehiculo = cmbTipoVehiculoICE.Text;
            if (tipoVehiculo == "NINGUNO") return;

            if (!double.TryParse(
                    txtPVPICE.Text.Trim().Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double pvp) || pvp <= 0)
                return;

            ResultadoICE resultado = ProcesosICE.ObtenerICEVehiculoPorPrecio(pvp, tipoVehiculo);
            AplicarResultadoICE(resultado);
        }

        // Aplica un ResultadoICE al formulario
        private void AplicarResultadoICE(ResultadoICE resultado)
        {
            if (!resultado.Detectado)
            {
                if (lblICEDetectado != null)
                {
                    lblICEDetectado.Text = "";
                    lblICEDetectado.ForeColor = Color.DarkSlateGray;
                }
                ActualizarEstadoCamposICE(false);
                return;
            }

            // Llenar campos ICE auto-detectados
            cmbAplicaICE.SelectedItem = resultado.AplicaIce;
            txtCodigoICE.Text         = resultado.IceCodigo;
            if (cmbTipoICE.Items.Contains(resultado.IceTipo))
                cmbTipoICE.SelectedItem = resultado.IceTipo;
            txtPorcentajeICE.Text     = resultado.IcePorcentaje;
            txtValorICE.Text          = resultado.IceValor;
            txtUnidadICE.Text         = resultado.IceUnidad;

            // TipoVehiculo
            if (cmbTipoVehiculoICE.Items.Contains(resultado.TipoVehiculo))
                cmbTipoVehiculoICE.SelectedItem = resultado.TipoVehiculo;

            // Bloquear los campos que se llenaron automáticamente
            BloquearCamposICE(true);

            // Mostrar feedback
            if (lblICEDetectado != null)
            {
                lblICEDetectado.Text = "✔ ICE: " + resultado.Mensaje;
                lblICEDetectado.ForeColor = Color.DarkGreen;

                if (ProcesosICE.EsVehiculo(resultado))
                    lblICEDetectado.ForeColor = Color.DarkOrange;
            }
        }

        // Bloquea/desbloquea los campos ICE auto-llenados
        // Deja siempre editables: Azúcar g, Volumen, PVP (datos específicos del producto)
        private void cmbAplicaICE_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarEstadoCamposICE(true);
        }

        private void LimpiarCamposICE()
        {
            txtCodigoICE.Text = "";
            cmbTipoICE.SelectedIndex = 0;
            txtPorcentajeICE.Text = "";
            txtValorICE.Text = "";
            txtUnidadICE.Text = "";
            txtAzucarICE.Text = "";
            txtVolumenICE.Text = "";
            txtPVPICE.Text = "";
            cmbTipoVehiculoICE.SelectedIndex = 0;

            if (lblICEDetectado != null)
            {
                lblICEDetectado.Text = "";
                lblICEDetectado.ForeColor = Color.DarkSlateGray;
            }
        }

        private void ActualizarEstadoCamposICE(bool limpiarSiNoAplica)
        {
            bool aplicaICE = string.Equals(
                cmbAplicaICE.Text?.Trim(),
                "SI",
                StringComparison.OrdinalIgnoreCase
            );

            if (!aplicaICE && limpiarSiNoAplica)
                LimpiarCamposICE();

            txtCodigoICE.ReadOnly = !aplicaICE;
            cmbTipoICE.Enabled = aplicaICE;
            txtPorcentajeICE.ReadOnly = !aplicaICE;
            txtValorICE.ReadOnly = !aplicaICE;
            txtUnidadICE.ReadOnly = !aplicaICE;
            txtAzucarICE.ReadOnly = !aplicaICE;
            txtVolumenICE.ReadOnly = !aplicaICE;
            txtPVPICE.ReadOnly = !aplicaICE;
            cmbTipoVehiculoICE.Enabled = aplicaICE;

            Color fondo = aplicaICE ? SystemColors.Window : Color.WhiteSmoke;
            txtCodigoICE.BackColor = fondo;
            txtPorcentajeICE.BackColor = fondo;
            txtValorICE.BackColor = fondo;
            txtUnidadICE.BackColor = fondo;
            txtAzucarICE.BackColor = fondo;
            txtVolumenICE.BackColor = fondo;
            txtPVPICE.BackColor = fondo;
        }

        private void SeleccionarFila(string nombre)
        {
            foreach (DataGridViewRow row in gvProductos.Rows)
            {
                foreach (DataGridViewColumn col in gvProductos.Columns)
                {
                    if (!string.Equals(col.DataPropertyName, "NOMBRE", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (string.Equals(row.Cells[col.Index].Value?.ToString().Trim(), nombre, StringComparison.OrdinalIgnoreCase))
                    {
                        gvProductos.ClearSelection();
                        gvProductos.Rows[row.Index].Selected = true;
                        gvProductos.FirstDisplayedScrollingRowIndex = row.Index;
                        return;
                    }
                }
            }
        }

        private void BloquearCamposICE(bool bloquear)
        {
            cmbAplicaICE.Enabled       = !bloquear;
            txtCodigoICE.ReadOnly      = bloquear;
            cmbTipoICE.Enabled         = !bloquear;
            txtPorcentajeICE.ReadOnly  = bloquear;
            txtValorICE.ReadOnly       = bloquear;
            txtUnidadICE.ReadOnly      = bloquear;
            cmbTipoVehiculoICE.Enabled = !bloquear;

            Color fondo = bloquear ? Color.WhiteSmoke : SystemColors.Window;
            txtCodigoICE.BackColor     = fondo;
            txtPorcentajeICE.BackColor = fondo;
            txtValorICE.BackColor      = fondo;
            txtUnidadICE.BackColor     = fondo;
        }

        // ====================================================================
        // CÁLCULO BIDIRECCIONAL IVA
        // ====================================================================

        private bool AplicaIva() =>
            string.Equals(cmbIva.Text?.Trim(), "SI", StringComparison.OrdinalIgnoreCase)
            && _services.TarifaIva > 0m;

        private void txtValor_Leave(object sender, EventArgs e)
        {
            if (_calculando) return;
            RecalcularSinIVADesdeCon();
        }

        private void txtValorSinIVA_Leave(object sender, EventArgs e)
        {
            if (_calculando) return;
            RecalcularConIVADesdeSin();
        }

        private void cmbIva_IvaChanged(object sender, EventArgs e)
        {
            if (_calculando) return;
            if (!string.IsNullOrWhiteSpace(txtValorSinIVA.Text))
                RecalcularConIVADesdeSin();
            else if (!string.IsNullOrWhiteSpace(txtValor.Text))
                RecalcularSinIVADesdeCon();
        }

        private void RecalcularSinIVADesdeCon()
        {
            _calculando = true;
            try
            {
                if (!decimal.TryParse(
                        txtValor.Text.Replace(",", "."),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out decimal conIva))
                {
                    txtValorSinIVA.Text = "";
                    return;
                }

                decimal sinIva = AplicaIva()
                    ? Math.Round(conIva / (1 + _services.TarifaIva), 4)
                    : conIva;

                txtValorSinIVA.Text = sinIva.ToString(CultureInfo.InvariantCulture);
            }
            finally { _calculando = false; }
        }

        private void RecalcularConIVADesdeSin()
        {
            _calculando = true;
            try
            {
                if (!decimal.TryParse(
                        txtValorSinIVA.Text.Replace(",", "."),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out decimal sinIva))
                {
                    txtValor.Text = "";
                    return;
                }

                decimal conIva = AplicaIva()
                    ? Math.Round(sinIva * (1 + _services.TarifaIva), 2)
                    : sinIva;

                txtValor.Text = conIva.ToString("0.00", CultureInfo.InvariantCulture);
            }
            finally { _calculando = false; }
        }
    }
}
