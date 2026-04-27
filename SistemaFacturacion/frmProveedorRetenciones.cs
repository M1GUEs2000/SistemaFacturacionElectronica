using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static LogicaNegocios.Procesos.ProcesosGenerales;
using static LogicaNegocios.Procesos.ProcesosRetenciones;
using Exception = System.Exception;

namespace SistemaFacturacion
{
    public partial class frmProveedorRetenciones : Form
    {

        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private bool ProveedorExiste = false;

        public frmProveedorRetenciones(AppServices services
)
        {
            InitializeComponent();
            InicializarFormulario();
            _services = services;

        }

        // ======================================================
        // INICIALIZACIÓN
        // ======================================================
        private void InicializarFormulario()
        {
            panelFormulario.Visible = false;
            CargarCombos();
            ConfigurarValidaciones();
            txtIdentificacion.Text = "";
            cmbTipoId.SelectedIndex = -1;

            txtIdentificacion.Focus();
        }

        private void frmProveedorRetenciones_Load(object sender, EventArgs e)
        {
            InicializarFormulario();
        }

        // ======================================================
        // VALIDAR IDENTIFICACIÓN / BUSCAR PROVEEDOR
        // ======================================================
        private void btnValidar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbTipoId.SelectedIndex < 0)
                {
                    Notificaciones.Show(this, "Seleccione el tipo de identificación.", "advertencia");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtIdentificacion.Text))
                {
                    Notificaciones.Show(this, "Ingrese la identificación.", "advertencia");
                    return;
                }

                BuscarProveedor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuscarProveedor()
        {
            string identificacion = txtIdentificacion.Text.Trim();

            DataSet ds = _services.Proveedor.ConsultarPorIdentificacion(identificacion);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                ProveedorExiste = true;

                DataRow r = ds.Tables[0].Rows[0];

                txtRazonSocial.Text = r["RAZONSOCIAL"].ToString();
                txtDireccion.Text = r["DIRECCION"].ToString();
                txtTelefono.Text = r["TELEFONO"].ToString();
                txtCorreo.Text = r["CORREO"].ToString();

                cmbTipoPersona.SelectedValue = r["TIPOPERSONA"].ToString();
                cmbRimpe.SelectedItem = Convert.ToBoolean(r["ESRIMPE"]) ? "SI" : "NO";
                cmbTipoRimpe.SelectedValue = r["TIPORIMPE"].ToString();
                cmbProfesional.SelectedItem = Convert.ToBoolean(r["ESPROFESIONAL"]) ? "SI" : "NO";
                cmbArrendador.SelectedItem = Convert.ToBoolean(r["ESARRENDADOR"]) ? "SI" : "NO";

                lblVerificacion.Text = "✔ Proveedor encontrado";
                lblVerificacion.ForeColor = Color.LightGreen;
            }
            else
            {
                ProveedorExiste = false;
                LimpiarProveedor();

                lblVerificacion.Text = "⚠ Proveedor no existe, ingrese los datos";
                lblVerificacion.ForeColor = Color.LightPink;
            }

            panelFormulario.Visible = true;
        }

        private void LimpiarProveedor()
        {
            txtRazonSocial.Text = "";
            txtDireccion.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";

            cmbTipoPersona.SelectedIndex = -1;
            cmbRimpe.SelectedIndex = -1;
            cmbTipoRimpe.SelectedIndex = -1;
            cmbProfesional.SelectedIndex = -1;
            cmbArrendador.SelectedIndex = -1;
        }

        // ======================================================
        // CONTINUAR → PASAR DTO
        // ======================================================
        private void btnContinuar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarFormulario(out string mensaje))
                {
                    Notificaciones.Show(this, mensaje, "advertencia");
                    return;
                }

                string numeroRetencion =
                    _services.Param.ObtenerSecuencialFormateado("07");

                string numeroFacturaCompleto =
                        txtPunto.Text.Trim().PadLeft(3, '0') + "-" +
                        txtCaja.Text.Trim().PadLeft(3, '0') + "-" +
                        txtNumeroFactura.Text.Trim().PadLeft(9, '0');

                DtoRetencionManual dto = new DtoRetencionManual
                {
                    // PROVEEDOR
                    TipoIdentificacion = cmbTipoId.SelectedValue.ToString(),
                    Identificacion = txtIdentificacion.Text.Trim(),
                    RazonSocial = txtRazonSocial.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Correo = txtCorreo.Text.Trim(),

                    TipoPersona = cmbTipoPersona.SelectedValue?.ToString(),
                    EsRimpe = cmbRimpe.SelectedItem?.ToString() == "SI",
                    TipoRimpe = cmbTipoRimpe.SelectedValue?.ToString(),
                    EsProfesional = cmbProfesional.SelectedItem?.ToString() == "SI",
                    EsArrendador = cmbArrendador.SelectedItem?.ToString() == "SI",

                    // FACTURA
                    NumeroFactura = numeroFacturaCompleto,
                    NumeroRetencion = numeroRetencion,
                    FechaFactura = dtpFechaFactura.Value.Date,
                    BaseImponible = NumericHelper.ParseUserDecimal(txtBaseImponible.Text),
                    Iva = NumericHelper.ParseUserDecimal(txtIva.Text),
                    Total = NumericHelper.ParseUserDecimal(txtTotal.Text)
                };

                if (!ProveedorExiste)
                {
                    _services.Proveedor.Insertar(
                        dto.TipoIdentificacion,
                        dto.Identificacion,
                        dto.RazonSocial,
                        dto.Correo,
                        dto.Direccion,
                        dto.Telefono,
                        dto.TipoPersona,
                        dto.EsRimpe,
                        dto.TipoRimpe,
                        dto.EsProfesional,
                        dto.EsArrendador,
                        "ACTIVO",
                        UsuarioActual,
                        IPActual
                    );
                }

                frmNuevaRetencion frm = new frmNuevaRetencion(dto, _services);
                frm.UsuarioActual = UsuarioActual;
                frm.IPActual = IPActual;
                frm.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // VALIDACIONES
        // ======================================================
        private bool ValidarFormulario(out string mensaje)
        {
            mensaje = "";
            var errores = new List<string>();

            if (cmbTipoId.SelectedIndex < 0)
                errores.Add("- Tipo de identificación es obligatorio");

            FormValidador.Requerido(txtIdentificacion.Text, "Identificación",    errores);
            FormValidador.Requerido(txtRazonSocial.Text,    "Razón social",      errores);
            FormValidador.Requerido(txtNumeroFactura.Text,  "Número de factura", errores);

            if (!NumericHelper.TryParseUserDecimal(txtBaseImponible.Text))
                errores.Add("- Base imponible inválida");
            if (!NumericHelper.TryParseUserDecimal(txtIva.Text))
                errores.Add("- IVA inválido");
            if (!NumericHelper.TryParseUserDecimal(txtTotal.Text))
                errores.Add("- Total inválido");

            if (errores.Count > 0)
                mensaje = string.Join("\n", errores);

            return errores.Count == 0;
        }

        // ======================================================
        // COMBOS
        // ======================================================
        private void CargarCombos()
        {
            // ===============================
            // TIPO IDENTIFICACIÓN
            // ===============================
            cmbTipoId.DataSource = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("04", "RUC"),
        new KeyValuePair<string, string>("05", "CÉDULA"),
        new KeyValuePair<string, string>("06", "PASAPORTE")
    };
            cmbTipoId.DisplayMember = "Value";
            cmbTipoId.ValueMember = "Key";
            cmbTipoId.SelectedIndex = -1;

            // ===============================
            // TIPO PERSONA
            // ===============================
            cmbTipoPersona.DataSource = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("PN", "PERSONA NATURAL"),
        new KeyValuePair<string, string>("PJ", "PERSONA JURÍDICA")
    };
            cmbTipoPersona.DisplayMember = "Value";
            cmbTipoPersona.ValueMember = "Key";
            cmbTipoPersona.SelectedIndex = -1;

            // ===============================
            // RIMPE
            // ===============================
            cmbRimpe.Items.Clear();
            cmbRimpe.Items.Add("SI");
            cmbRimpe.Items.Add("NO");
            cmbRimpe.SelectedIndex = -1;

            // ===============================
            // TIPO RIMPE
            // ===============================
            cmbTipoRimpe.DataSource = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("RIMPE-NE", "NEGOCIO POPULAR"),
        new KeyValuePair<string, string>("RIMPE-EM", "EMPRENDEDOR")
    };
            cmbTipoRimpe.DisplayMember = "Value";
            cmbTipoRimpe.ValueMember = "Key";
            cmbTipoRimpe.SelectedIndex = -1;
            cmbTipoRimpe.Enabled = false;

            // ===============================
            // PROFESIONAL
            // ===============================
            cmbProfesional.Items.Clear();
            cmbProfesional.Items.Add("SI");
            cmbProfesional.Items.Add("NO");
            cmbProfesional.SelectedIndex = -1;

            // ===============================
            // ARRENDADOR
            // ===============================
            cmbArrendador.Items.Clear();
            cmbArrendador.Items.Add("SI");
            cmbArrendador.Items.Add("NO");
            cmbArrendador.SelectedIndex = -1;

            // ===============================
            // LÓGICA RIMPE → TIPO RIMPE
            // ===============================
            cmbRimpe.SelectedIndexChanged -= CmbRimpe_SelectedIndexChanged;
            cmbRimpe.SelectedIndexChanged += CmbRimpe_SelectedIndexChanged;
        }

        private void CmbRimpe_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esRimpe = cmbRimpe.SelectedItem != null &&
                           cmbRimpe.SelectedItem.ToString() == "SI";

            cmbTipoRimpe.Enabled = esRimpe;

            if (!esRimpe)
                cmbTipoRimpe.SelectedIndex = -1;
        }

        private void ConfigurarValidaciones()
        {
            // -------------------------
            // SOLO NÚMEROS
            // -------------------------
            txtPunto.KeyPress += SoloNumeros_KeyPress;
            txtCaja.KeyPress += SoloNumeros_KeyPress;
            txtNumeroFactura.KeyPress += SoloNumeros_KeyPress;

            txtPunto.MaxLength = 3;
            txtCaja.MaxLength = 3;
            txtNumeroFactura.MaxLength = 9;

            // -------------------------
            // DECIMALES CON COMA
            // -------------------------
            txtBaseImponible.KeyPress += SoloDecimalConComa_KeyPress;
            txtIva.KeyPress += SoloDecimalConComa_KeyPress;
            txtTotal.KeyPress += SoloDecimalConComa_KeyPress;
        }

        private void SoloNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;

                Notificaciones.Show(
                    this,
                    "Solo se permiten números.",
                    "advertencia",
                    UsuarioActual,
                    IPActual
                );
            }
        }

        private void SoloDecimalConComa_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = sender as TextBox;

            // Convertir punto en coma
            if (e.KeyChar == '.')
            {
                e.Handled = true;

                if (!txt.Text.Contains(","))
                {
                    txt.Text += ",";
                    txt.SelectionStart = txt.Text.Length;
                }

                Notificaciones.Show(
                    this,
                    "Use coma (,) para separar decimales.",
                    "advertencia",
                    UsuarioActual,
                    IPActual
                );

                return;
            }

            // Permitir números
            if (char.IsDigit(e.KeyChar))
                return;

            // Permitir una sola coma
            if (e.KeyChar == ',' && !txt.Text.Contains(","))
                return;

            // Permitir borrar
            if (char.IsControl(e.KeyChar))
                return;

            e.Handled = true;

            Notificaciones.Show(
                this,
                "Solo se permiten números y coma para decimales.",
                "advertencia",
                UsuarioActual,
                IPActual
            );
        }


    }

}
