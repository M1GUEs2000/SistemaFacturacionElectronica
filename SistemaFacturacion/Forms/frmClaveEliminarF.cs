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
    public partial class frmClaveEliminarF : Form
    {
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }
        public frmClaveEliminarF()
        {
            InitializeComponent();
        }

        private void frmClaveEliminarF_Load(object sender, EventArgs e)
        {

        }
        public string getText()
        {
            return textBox1.Text;
        }
    }
}
