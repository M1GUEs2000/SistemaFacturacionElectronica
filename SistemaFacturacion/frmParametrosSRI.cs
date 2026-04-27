using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmParametrosSRI : Form
    {

        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }
        public frmParametrosSRI(AppServices services
)
        {
            InitializeComponent();
            _services = services;

            ConfigurarEventos();
            btnModificar.Visible = false;
        }
        // =========================================================
        // 🔹 1️ Cargar datos al iniciar
        // =========================================================
        private void frmParametrosSRI_Load(object sender, EventArgs e)
        {
            CargarParametros();
        }
        private void CargarParametros()
        {
            try
            {
                DataSet ds = _services.Param.Mostrar();
                dataGridView1.DataSource = ds.Tables[0];
                btnModificar.Visible = false;

                // Ocultar IDPARAMETRO si existe
                if (dataGridView1.Columns.Contains("IDPARAMETRO"))
                {
                    dataGridView1.Columns["IDPARAMETRO"].Visible = false;
                }

                // 👇 OCULTAR IDFACTURA (ES INT, NO LO TOCAMOS)
                if (dataGridView1.Columns.Contains("IDFACTURA"))
                {
                    dataGridView1.Columns["IDFACTURA"].Visible = false;
                }

                // 👇 CONVERTIR VISUALMENTE TIPOCOMPROBANTE
                if (dataGridView1.Columns.Contains("TIPOCOMPROBANTE"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string tipo = row.Cells["TIPOCOMPROBANTE"].Value?.ToString().Trim();

                        // 🔐 GUARDAMOS EL VALOR REAL
                        row.Cells["TIPOCOMPROBANTE"].Tag = tipo;

                        // 👁 MOSTRAMOS TEXTO AMIGABLE
                        row.Cells["TIPOCOMPROBANTE"].Value = ObtenerDescripcionComprobante(tipo);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los parámetros: " + ex.Message);
            }
        }
        private string ObtenerDescripcionComprobante(string tipo)
        {
            switch (tipo)
            {
                case "01":
                    return "FACTURA";
                case "04":
                    return "NOTA DE CRÉDITO";
                case "03":
                    return "LIQUIDACIÓN DE COMPRA";
                case "05":
                    return "NOTA DE DÉBITO";
                case "06":
                    return "GUÍA DE REMISIÓN";
                case "07":
                    return "RETENCIÓN";
                default:
                    return tipo; // por si viene algo raro, no lo rompas
            }
        }
        // =========================================================
        // 🔹 2️ Configurar eventos
        // =========================================================
        private void ConfigurarEventos()
        {
            this.Load += frmParametrosSRI_Load;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }
        // =========================================================
        // ✏️ 3 Modificar un parámetro existente
        // =========================================================
        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (CamposVacios())
            {
                MessageBox.Show("Complete todos los campos antes de modificar.");
                return;
            }

            try
            {
                string tipoComprobante = txtTipo.Text.Trim();
                if (!long.TryParse(txtSecuencia.Text.Trim(), out long secuencial))
                {
                    MessageBox.Show("El secuencial debe ser numérico.");
                    return;
                }

                string codigoNumerico = txtCodigoNumerico.Text.Trim();
                string fecha = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                int filas = _services.Param.Actualizar(
                    tipoComprobante,
                    secuencial,
                    codigoNumerico,
                    fecha,
                    UsuarioActual,
                    IPActual
                );

                if (filas > 0)
                {
                    MessageBox.Show("Parámetro modificado correctamente ✅");
                    CargarParametros();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se pudo modificar el registro ❌");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al modificar: " + ex.Message);
            }
        }
        // =========================================================
        // ❌ 4 Eliminar un parámetro
        // =========================================================
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSecuencia.Text))
            {
                MessageBox.Show("Seleccione un tipo de comprobante para eliminar.");
                return;
            }

            DialogResult r = MessageBox.Show(
                "¿Está seguro de eliminar este registro?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (r == DialogResult.Yes)
            {
                try
                {
                    string tipoComprobante = txtTipo.Text.Trim();
                    int filas = _services.Param.EliminarPorTipo(tipoComprobante, UsuarioActual, IPActual);

                    if (filas > 0)
                    {
                        MessageBox.Show("Registro eliminado correctamente ✅");
                        CargarParametros();
                        LimpiarCampos();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo eliminar el registro ❌");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar: " + ex.Message);
                }
            }
        }
        // =========================================================
        // 📋 5 Seleccionar registro desde la tabla
        // =========================================================
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Rows[e.RowIndex].Cells.Count > 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtTipo.Text = row.Cells["TIPOCOMPROBANTE"].Tag?.ToString();
                txtSecuencia.Text = row.Cells["SECUENCIAL"].Value.ToString();
                txtCodigoNumerico.Text = row.Cells["CODIGONUMERICO"].Value.ToString();
                txtFecha.Text = row.Cells["FECHAACTUALIZACION"].Value.ToString();

                txtTipo.Enabled = false;
                btnModificar.Visible = true;
            }
        }
        // =========================================================
        // 🧹  Utilidades
        // =========================================================
        private bool CamposVacios()
        {
            return string.IsNullOrWhiteSpace(txtSecuencia.Text)
                || string.IsNullOrWhiteSpace(txtCodigoNumerico.Text)
                || string.IsNullOrWhiteSpace(txtFecha.Text);
        }
        private void LimpiarCampos()
        {
            txtSecuencia.Clear();
            txtCodigoNumerico.Clear();
            txtFecha.Clear();
            txtTipo.Clear();
            btnModificar.Visible = false;
        }
    }
}
