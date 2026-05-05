using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmFormaPago : Form
    {
        private readonly AppServices _services;

        public frmFormaPago(AppServices services

        )
        {
            _services = services;
            InitializeComponent();
        }
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }


        private void frmFormaPago_Load(object sender, EventArgs e)
        {
            MostrarFormaPago("");
            CargarCombos();
            btnEliminar.Visible = false;
            btnModificar.Visible = false;
            btnGuardar.Visible = true;
        }
        public void MostrarFormaPago(string Formas)
        {
            DataSet dsDatos = new DataSet();
            try
            {
                dsDatos = _services.FormaPago.MostrarFormaPago(Formas);
                gvFormaPago.DataSource = dsDatos.Tables[0];

            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            MostrarFormaPago(txtNombreBuscar.Text);
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            Limpiar();
        }
        public void Limpiar()
        {
            btnEliminar.Visible = false;
            btnModificar.Visible = false;
            btnGuardar.Visible = true;
            txtFormas.Enabled = true;
            txtFormas.Text = "";
            txtImagen.Text = "";

        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            try
            {
                DataSet dsDatos = _services.FormaPago.ConsultaFormas(txtFormas.Text.Trim());
                if (dsDatos.Tables[0].Rows.Count > 0)
                {
                    Notificaciones.Show(this, "Ya existe la Forma de Pago: " + txtFormas.Text, "advertencia");
                    return;
                }

                ComboItem item = (ComboItem)cmbCodigo.SelectedItem;
                string codigo = item?.Value ?? "";
                int fila = _services.FormaPago.Insertar(txtFormas.Text, txtImagen.Text, codigo, UsuarioActual, IPActual);
                if (fila == 1)
                {
                    string formaGuardada = txtFormas.Text.Trim();
                    MostrarFormaPago("");
                    SeleccionarFila(formaGuardada);
                    Limpiar();
                    Notificaciones.Show(this, "Forma de pago registrada correctamente.", "exito");
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
            FormValidador.Requerido(txtFormas.Text, "Nombre de forma de pago", errores);
            return !FormValidador.MostrarErrores(this, errores);
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            try
            {
                ComboItem item = (ComboItem)cmbCodigo.SelectedItem;
                string codigo = item?.Value ?? "";
                int fila = _services.FormaPago.Actualizar(txtFormas.Text, txtImagen.Text, codigo, UsuarioActual, IPActual);
                if (fila == 1)
                {
                    string formaGuardada = txtFormas.Text.Trim();
                    MostrarFormaPago("");
                    SeleccionarFila(formaGuardada);
                    Limpiar();
                    Notificaciones.Show(this, "Forma de pago actualizada correctamente.", "exito");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (DialogResult.Yes == MessageBox.Show("¿Realmente desea elminar Forma de Pago?", "Mensaje", MessageBoxButtons.YesNo))
                {
                    int fila = _services.FormaPago.Eliminar(txtFormas.Text, UsuarioActual, IPActual);
                    if (fila == 1)
                    { //log
                        string Fechas = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                        string HostName = Environment.UserName;
                        // LogicaNegocios._services.Log.Insertar("Eliminar Contribuyente ", frmPrincipal.NombreUsuario, HostName, " Eliminar  contribuyente con celula " + txtCedula.Text + " nombre  " + txtNombre.Text + " apellido " + txtApellido.Text, Fechas);

                        MostrarFormaPago("");
                        Limpiar();
                        Notificaciones.Show(this, "Eliminado correctamente.", "exito");
                    }
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void gvFormaPago_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (gvFormaPago.DataSource != null && gvFormaPago.Rows.Count > 0 && gvFormaPago.SelectedRows.Count > 0)
                {
                    txtFormas.Text = gvFormaPago.SelectedRows[0].Cells[0].Value.ToString();
                    txtImagen.Text = gvFormaPago.SelectedRows[0].Cells[1].Value.ToString();
                    string codigo = gvFormaPago.SelectedRows[0].Cells[2].Value.ToString();
                    SeleccionarCombo(cmbCodigo, codigo);

                    txtFormas.Enabled = false;
                    btnEliminar.Visible = true;
                    btnModificar.Visible = true;
                    btnGuardar.Visible = false;

                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void SeleccionarCombo(ComboBox combo, string value)
        {
            foreach (ComboItem item in combo.Items)
            {
                if (item.Value == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private void CargarCombos()
        {
            cmbCodigo.Items.Clear();

            cmbCodigo.Items.Add(new ComboItem("SIN UTILIZACION DEL SISTEMA FINANCIERO", "01"));
            cmbCodigo.Items.Add(new ComboItem("COMPENSACIÓN DE DEUDAS", "15"));
            cmbCodigo.Items.Add(new ComboItem("TARJETA DE DÉBITO", "16"));
            cmbCodigo.Items.Add(new ComboItem("DINERO ELECTRÓNICO", "17"));
            cmbCodigo.Items.Add(new ComboItem("TARJETA PREPAGO", "18"));
            cmbCodigo.Items.Add(new ComboItem("TARJETA DE CRÉDITO", "19"));
            cmbCodigo.Items.Add(new ComboItem("OTROS CON UTILIZACIÓN DEL SISTEMA FINANCIERO", "20"));
            cmbCodigo.Items.Add(new ComboItem("ENDOSO DE TÍTULOS", "21"));
        }


        private void SeleccionarFila(string forma)
        {
            foreach (DataGridViewRow row in gvFormaPago.Rows)
            {
                if (string.Equals(row.Cells[0].Value?.ToString().Trim(), forma, StringComparison.OrdinalIgnoreCase))
                {
                    gvFormaPago.ClearSelection();
                    gvFormaPago.Rows[row.Index].Selected = true;
                    gvFormaPago.FirstDisplayedScrollingRowIndex = row.Index;
                    return;
                }
            }
        }

        private class ComboItem
        {
            public string Text { get; }
            public string Value { get; }

            public ComboItem(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() => Text;
        }

    }
}
