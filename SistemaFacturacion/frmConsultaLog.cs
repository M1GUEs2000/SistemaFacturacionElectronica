using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmConsultaLog : Form
    {

        private readonly AppServices _services;
        public frmConsultaLog(
                      AppServices services
)
        {
            _services = services;

            InitializeComponent();
            CargarProcesos();
            ConfigurarControles();
        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }


        // ===========================
        //   CONFIGURACIONES INICIALES
        // ===========================
        private void ConfigurarControles()
        {
            dtpDesde.Format = DateTimePickerFormat.Custom;
            dtpDesde.CustomFormat = "yyyy/MM/dd";

            dtpHasta.Format = DateTimePickerFormat.Custom;
            dtpHasta.CustomFormat = "yyyy/MM/dd";

            dtpDesde.Value = DateTime.Now;
            dtpHasta.Value = DateTime.Now;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void CargarProcesos()
        {
            try
            {
                DataSet ds = _services.Log.MostrarProceso();
                cbProceso.DataSource = ds.Tables[0];
                cbProceso.DisplayMember = "PROCESO";
                cbProceso.ValueMember = "PROCESO";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los procesos: " + ex.Message);
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {

            try
            {
                string fechaDesde = dtpDesde.Value.ToString("yyyy/MM/dd");
                string fechaHasta = dtpHasta.Value.ToString("yyyy/MM/dd");

                if (dtpHasta.Value < dtpDesde.Value)
                {
                    MessageBox.Show("La fecha HASTA no puede ser menor a la fecha DESDE.");
                    return;
                }

                string proceso = cbProceso.Text.Trim();
                string texto = txtTexto?.Text.Trim() ?? "";

                DataSet ds = _services.Log.ConsultarLog(
                    proceso,
                    texto,
                    fechaDesde,
                    fechaHasta
                );

                dataGridView1.DataSource = ds.Tables[0];

                if (ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron registros.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar logs: " + ex.Message);
            }

        }

        private void btnTodo_Click(object sender, EventArgs e)
        {
            try
            {
                string fechaDesde = dtpDesde.Value.ToString("yyyy/MM/dd");
                string fechaHasta = dtpHasta.Value.ToString("yyyy/MM/dd");
                string Proceso = "Seleccione";
                string Texto = "";

                // Consulta que muestra TODO por fechas, sin filtros
                DataSet dsDatos = _services.Log.ConsultarLog(Proceso, Texto, fechaDesde, fechaHasta);

                if (dsDatos.Tables.Count > 0 && dsDatos.Tables[0].Rows.Count > 0)
                {
                    dataGridView1.DataSource = dsDatos.Tables[0];
                }
                else
                {
                    dataGridView1.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar todas las facturas: " + ex.Message);
            }
        }



    }
}
