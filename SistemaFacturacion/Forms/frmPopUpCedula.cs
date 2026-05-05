using System;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmPopUpCedula : Form
    {

        public string CedulaIngresada { get; private set; } = "";
        public bool UsuarioConfirmo { get; private set; } = false;

        public frmPopUpCedula()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            string cedula = txtCedula.Text.Trim();

            CedulaIngresada = cedula;
            UsuarioConfirmo = true;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
