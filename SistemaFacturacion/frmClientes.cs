using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmClientes : Form
    {
        private readonly AppServices _services;

        public frmClientes(
                  AppServices services
 )
        {
            _services = services;

            InitializeComponent();
            ConfigurarComboOpciones();
            CargarClientes();
            ConfigurarEventos();
            ConfigurarValidaciones();
            InicializarBotones();
        }

        private bool modoNuevo = false;
        public string CedulaRegistrada { get; private set; }
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private bool modoDesdePrincipal = false;
        public frmClientes(
           AppServices services,
            bool desdePrincipal = false   // ← EL OPCIONAL VA AL FINAL
        )
        {
            InitializeComponent();   // ← SIEMPRE LO PRIMERO


            _services = services;

            modoDesdePrincipal = desdePrincipal;

            ConfigurarComboOpciones();
            CargarClientes();
            ConfigurarEventos();
            ConfigurarValidaciones();
            InicializarBotones();

            panel2.Dispose();
            dataGridView1.Dispose();
        }


        // 🔹 Inicialización de combo

        private void ConfigurarComboOpciones()
        {
            cmbOpciones.Items.Clear();
            cmbOpciones.Items.Add("Cédula");
            cmbOpciones.Items.Add("Nombre");
            cmbOpciones.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOpciones.SelectedItem = "Nombre";
        }


        // 🔹 Mostrar todos los clientes al iniciar

        private void CargarClientes()
        {
            try
            {
                DataSet ds = _services.Cliente.Mostrar();
                dataGridView1.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message);
            }
        }


        // 🔹 Configurar eventos

        private void ConfigurarEventos()
        {
            txtBuscarCedula.TextChanged += TxtBuscarCedula_TextChanged;
            cmbOpciones.SelectedIndexChanged += CmbOpciones_SelectedIndexChanged;
            btnGuardar.Click += BtnGuardar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void ConfigurarValidaciones()
        {
            txtCedula.KeyPress += (s, e) =>
            {
                // Solo números
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            txtCedula.Leave += (s, e) =>
            {
                int largo = txtCedula.Text.Trim().Length;

                if (!string.IsNullOrWhiteSpace(txtCedula.Text) && (largo < 10 || largo > 13))
                {
                    Notificaciones.Show(this, "La identificación debe tener entre 10 y 13 dígitos.", "advertencia");
                    txtCedula.Focus();
                }
            };

            txtCorreo.Leave += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtCorreo.Text) && !EsCorreoValido(txtCorreo.Text))
                {
                    Notificaciones.Show(this, "Ingrese un correo electrónico válido.", "advertencia");
                    txtCorreo.Focus();
                }
            };

        }

        private bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidarCampos()
        {
            if (FormValidador.EsVacio(this, txtCedula.Text, "Cédula", txtCedula)) return false;
            if (txtCedula.Text.Trim().Length < 10 || txtCedula.Text.Trim().Length > 13)
            {
                MessageBox.Show("La identificación debe tener entre 10 y 13 dígitos.");
                txtCedula.Focus();
                return false;
            }

            if (FormValidador.EsVacio(this, txtNombre.Text, "Nombre", txtNombre)) return false;
            if (FormValidador.EsVacio(this, txtCorreo.Text, "Correo", txtCorreo)) return false;
            if (FormValidador.EsVacio(this, txtDireccion.Text, "Dirección", txtDireccion)) return false;
            if (FormValidador.EsVacio(this, txtTelefono.Text, "Teléfono", txtTelefono)) return false;
            return true;
        }

        private void InicializarBotones()
        {
            btnModificar.Visible = false;
            btnEliminar.Visible = false;
            btnGuardar.Text = "Guardar";
            modoNuevo = false;
        }



        // Búsqueda en vivo mientras se escribe

        private void TxtBuscarCedula_TextChanged(object sender, EventArgs e)
        {
            string opcion = cmbOpciones.SelectedItem?.ToString() ?? "";
            string termino = txtBuscarCedula.Text.Trim();

            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    CargarClientes();
                    return;
                }

                DataSet ds;

                if (opcion == "Nombre")
                {
                    ds = _services.Cliente.ConsultarNombre(termino);
                }
                else
                {
                    ds = _services.Cliente.ConsultarCedula(termino);
                }

                dataGridView1.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la búsqueda: " + ex.Message);
            }
        }

        private void CmbOpciones_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtBuscarCedula.Clear();
            txtBuscarCedula.Focus();
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtBuscarCedula.Clear();
            txtBuscarCedula.Focus();
        }


        // ➕ Insertar o modo nuevo

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (modoNuevo)
            {
                LimpiarCampos();
                InicializarBotones();
                return;
            }

            if (!ValidarCampos())
                return;

            try
            {
                DataSet dsExiste = _services.Cliente.ConsultarCedula(txtCedula.Text.Trim());

                if (dsExiste != null && dsExiste.Tables.Count > 0 && dsExiste.Tables[0].Rows.Count > 0)
                {
                    MessageBox.Show("El cliente ya existe ❌");
                    return;
                }

                // ✔ INSERTAR con Usuario + IP
                int filas = _services.Cliente.Insertar(
                    txtCedula.Text.Trim(),
                    txtNombre.Text.Trim(),
                    txtCorreo.Text.Trim(),
                    txtDireccion.Text.Trim(),
                    txtTelefono.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (filas > 0)
                {
                    MessageBox.Show("Cliente guardado correctamente ✅");

                    CedulaRegistrada = txtCedula.Text.Trim();

                    if (modoDesdePrincipal)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }

                    LimpiarCampos();
                    CargarClientes();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el cliente ❌");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cliente: " + ex.Message);
            }
        }


        // ✏️ Modificar

        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            int filas = _services.Cliente.Actualizar(
                txtCedula.Text.Trim(),
                txtNombre.Text.Trim(),
                txtCorreo.Text.Trim(),
                txtDireccion.Text.Trim(),
                txtTelefono.Text.Trim(),
                UsuarioActual,
                IPActual
            );

            if (filas > 0)
            {
                MessageBox.Show("Cliente modificado correctamente ✅");
                LimpiarCampos();
                CargarClientes();
                InicializarBotones();
            }
            else
            {
                MessageBox.Show("No se pudo modificar el cliente ❌");
            }
        }


        // ❌ Eliminar

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                MessageBox.Show("Seleccione un cliente para eliminar.");
                return;
            }

            DialogResult r = MessageBox.Show(
                "¿Está seguro de eliminar este cliente?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (r == DialogResult.Yes)
            {
                int filas = _services.Cliente.Eliminar(
                    txtCedula.Text.Trim(),
                    txtNombre.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (filas > 0)
                {
                    MessageBox.Show("Cliente eliminado correctamente ✅");
                    LimpiarCampos();
                    CargarClientes();
                    InicializarBotones();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el cliente ❌");
                }
            }
        }


        // 📋 Selección de fila

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                txtCedula.Text = row.Cells["CEDULA"].Value.ToString();
                txtNombre.Text = row.Cells["NOMBRE"].Value.ToString();
                txtCorreo.Text = row.Cells["CORREO"].Value.ToString();
                txtDireccion.Text = row.Cells["DIRECCION"].Value.ToString();
                txtTelefono.Text = row.Cells["TELEFONO"].Value.ToString();

                txtCedula.Enabled = false;
                btnModificar.Visible = true;
                btnEliminar.Visible = true;
                btnGuardar.Text = "NUEVO CLIENTE";
                modoNuevo = true;
            }
        }


        // 🧹 Utilidades

        private bool CamposVacios()
        {
            return string.IsNullOrWhiteSpace(txtCedula.Text)
                || string.IsNullOrWhiteSpace(txtNombre.Text)
                || string.IsNullOrWhiteSpace(txtCorreo.Text)
                || string.IsNullOrWhiteSpace(txtDireccion.Text);
        }

        private void LimpiarCampos()
        {
            txtCedula.Clear();
            txtNombre.Clear();
            txtCorreo.Clear();
            txtDireccion.Clear();
            txtTelefono.Clear();
        }
    }
}
