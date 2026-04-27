using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmClaveTabla : Form
    {
        private readonly AppServices _services;
        public frmClaveTabla(AppServices services

)
        {
            _services = services;

            InitializeComponent();

        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            DataSet dsDato = new DataSet();
            if (txtClaveTabla.Text != "")
            {
                dsDato = _services.Login.ConsultarClaveTabla(txtClaveTabla.Text);
                if (dsDato.Tables[0].Rows.Count > 0)
                {
                    frmMantenimientoTablas frmReporteT = new frmMantenimientoTablas(_services);
                    frmReporteT.UsuarioActual = this.UsuarioActual;
                    frmReporteT.IPActual = this.IPActual;
                    frmReporteT.Show();
                    this.Close();
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

        private void txtClaveTabla_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                DataSet dsDato = new DataSet();
                if (txtClaveTabla.Text != "")
                {
                    dsDato = _services.Login.ConsultarClaveTabla(txtClaveTabla.Text);
                    if (dsDato.Tables[0].Rows.Count > 0)
                    {
                        frmMantenimientoTablas frmReporteT = new frmMantenimientoTablas(_services);
                        frmReporteT.UsuarioActual = this.UsuarioActual;
                        frmReporteT.IPActual = this.IPActual;
                        frmReporteT.Show();
                        this.Close();
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
    }
}
