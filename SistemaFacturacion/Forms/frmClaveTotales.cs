using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmClaveTotales : Form
    {
        private readonly AppServices _services;

        public frmClaveTotales(
        AppServices services

           )
        {
            _services = services;

            InitializeComponent();

        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private void btnAcetarCT_Click(object sender, EventArgs e)
        {
            DataSet dsDato = new DataSet();
            if (txtClaveTotales.Text != "")
            {
                dsDato = _services.Login.ConsultarClaveTotal(txtClaveTotales.Text);
                if (dsDato.Tables[0].Rows.Count > 0)
                {
                    frmReporteTotales frmReporteT = new frmReporteTotales(_services);
                    frmReporteT.Show();
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

        private void txtClaveTotales_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataSet dsDato = new DataSet();
                if (txtClaveTotales.Text != "")
                {
                    dsDato = _services.Login.ConsultarClaveTotal(txtClaveTotales.Text);
                    if (dsDato.Tables[0].Rows.Count > 0)
                    {
                        frmReporteTotales frmReporteT = new frmReporteTotales(_services);
                        frmReporteT.Show();
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
        }

        private void frmClaveTotales_Load(object sender, EventArgs e)
        {
            txtClaveTotales.Focus();
        }


    }
}
