using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmRecibo : Form
    {
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }
        public frmRecibo()
        {
            InitializeComponent();
        }

        private void frmRecibo_Load(object sender, EventArgs e)
        {

            this.reportViewer1.RefreshReport();
            try
            {
                DataSet dsDatosventa = new DataSet();
            }
            catch (Exception ex)
            {
               MessageBox.Show("Error: " + ex.Message.ToString(), "Mensaje");
            }
        }
    }
}
