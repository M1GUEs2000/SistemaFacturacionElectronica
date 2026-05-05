using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmReporteTotales : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }
        public frmReporteTotales(AppServices services)
        {
            InitializeComponent();
            _services = services;
        }

        private void frmReporteTotales_Load(object sender, EventArgs e)
        {
            DataSet dsDatos = new DataSet();
            string Fecha = DateTime.Now.ToString("yyyy/MM/dd");
            dsDatos = _services.Facturacion.ConsultarTotales(Fecha);
            if (dsDatos.Tables[0].Rows.Count > 0)
            {
                gvFacturasTotales.DataSource = dsDatos.Tables[0];

                DataTable Tabla;
                Tabla = dsDatos.Tables[0];
                DataTable Valor = dsDatos.Tables[0];
                object SumaValor = Tabla.Compute("SUM(TOTALES)", "");
                decimal TotalValor = Convert.ToDecimal(SumaValor);
                string SumaTotalValor = string.Format("{0:0.00}", TotalValor);
                lblSumaTotal.Text = "TOTAL DIARIO($): " + SumaTotalValor;
            }
        }
    }
}
