using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmClaveConsulta : Form
    {

        private readonly AppServices _services;

        public frmClaveConsulta(
        AppServices services
        )
        {

            InitializeComponent();
            _services = services;
        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }


        private void btnAceptar_Click(object sender, EventArgs e)
        {
            DataSet dsDato = new DataSet();
            if (txtClaveFecha.Text != "")
            {
                dsDato = _services.Login.ConsultarClaveConsultaFecha(txtClaveFecha.Text);
                if (dsDato.Tables[0].Rows.Count > 0)
                {
                    frmConsultas frmReporteF = new frmConsultas(_services);
                    frmReporteF.UsuarioActual = this.UsuarioActual;
                    frmReporteF.IPActual = this.IPActual;
                    frmReporteF.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Clave incorrecta", "Mensaje");
                }
            }
            else
            {
                MessageBox.Show("Ingrese una Clave", "Mensaje");
            }
        }

        private void txtClaveFecha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataSet dsDato = new DataSet();
                if (txtClaveFecha.Text != "")
                {
                    dsDato = _services.Login.ConsultarClaveConsultaFecha(txtClaveFecha.Text);
                    if (dsDato.Tables[0].Rows.Count > 0)
                    {
                        frmConsultas frmReporteF = new frmConsultas(_services);
                        frmReporteF.UsuarioActual = this.UsuarioActual;
                        frmReporteF.IPActual = this.IPActual;
                        frmReporteF.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Calave incorrecta ", "Mensaje");
                    }
                }
                else
                {
                    MessageBox.Show("Ingrese una Clave", "Mensaje");
                }
            }
        }
    }
}
