using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmClientesDatos : Form
    {

        private readonly AppServices _services;

        private bool modoNuevo = false;
        public string CedulaRegistrada { get; private set; }
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private bool modoDesdePrincipal = false;

        public frmClientesDatos( AppServices services, bool desdePrincipal = false )
        {
            InitializeComponent();   
            _services = services;
            modoDesdePrincipal = desdePrincipal;
            ConfigurarEventos();
            ConfigurarValidaciones();
            InicializarBotones();
        }


        // 🔹 Configurar eventos

        private void ConfigurarEventos()
        {
            btnGuardar.Click += BtnGuardar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
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
            if (FormValidador.EsVacio(this, txtCedula.Text,    "Cédula",    txtCedula))    return false;
            if (txtCedula.Text.Trim().Length < 10 || txtCedula.Text.Trim().Length > 13)
            {
                MessageBox.Show("La identificación debe tener entre 10 y 13 dígitos.");
                txtCedula.Focus();
                return false;
            }
            if (FormValidador.EsVacio(this, txtNombre.Text,    "Nombre",    txtNombre))    return false;
            if (FormValidador.EsVacio(this, txtCorreo.Text,    "Correo",    txtCorreo))    return false;
            if (FormValidador.EsVacio(this, txtDireccion.Text, "Dirección", txtDireccion)) return false;
            if (FormValidador.EsVacio(this, txtTelefono.Text,  "Teléfono",  txtTelefono))  return false;
            return true;
        }

        private void InicializarBotones()
        {
            btnModificar.Visible = false;
            btnEliminar.Visible = false;
            btnGuardar.Text = "Guardar";
            modoNuevo = false;
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
                    Notificaciones.Show(this, "El cliente ya existe.", "advertencia");
                    return;
                }

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
                    Notificaciones.Show(this, "Cliente guardado correctamente.", "exito");
                    CedulaRegistrada = txtCedula.Text.Trim();

                    if (modoDesdePrincipal)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }

                    LimpiarCampos();
                }
                else
                {
                    Notificaciones.Show(this, "No se pudo guardar el cliente.", "error");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error al guardar cliente: " + ex.Message, "error");
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
                Notificaciones.Show(this, "Cliente modificado correctamente.", "exito");
                LimpiarCampos();
                InicializarBotones();
            }
            else
            {
                Notificaciones.Show(this, "No se pudo modificar el cliente.", "error");
            }
        }


        // ❌ Eliminar

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                Notificaciones.Show(this, "Seleccione un cliente para eliminar.", "advertencia");
                return;
            }

            bool confirmar = Notificaciones.Show(
                this,
                "¿Está seguro de eliminar este cliente?",
                "confirmacion"
            );

            if (confirmar)
            {
                int filas = _services.Cliente.Eliminar(
                    txtCedula.Text.Trim(),
                    txtNombre.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (filas > 0)
                {
                    Notificaciones.Show(this, "Cliente eliminado correctamente.", "exito");
                    LimpiarCampos();
                    InicializarBotones();
                }
                else
                {
                    Notificaciones.Show(this, "No se pudo eliminar el cliente.", "error");
                }
            }
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
