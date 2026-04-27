using LogicaNegocios.Services;
using System;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmConsultas : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }
        public frmConsultas(AppServices services)
        {

            InitializeComponent();
            _services = services;
        }

        private void btnFacturas_Click(object sender, EventArgs e)
        {
            frmReportePorFechas frmReporteF = new frmReportePorFechas(_services);
            frmReporteF.UsuarioActual = this.UsuarioActual;
            frmReporteF.IPActual = this.IPActual;
            frmReporteF.Show();
        }

        private void btnNotas_Click(object sender, EventArgs e)
        {
            frmNotaCredito frmReporteF = new frmNotaCredito(_services);
            frmReporteF.UsuarioActual = this.UsuarioActual;
            frmReporteF.IPActual = this.IPActual;
            frmReporteF.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmConsultarRetenciones frmReporteF = new frmConsultarRetenciones(_services);
            frmReporteF.UsuarioActual = this.UsuarioActual;
            frmReporteF.IPActual = this.IPActual;
            frmReporteF.Show();
        }
    }
}
