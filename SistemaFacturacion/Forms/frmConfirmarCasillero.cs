using SistemaFacturacion.Controls;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmConfirmarCasillero : Form
    {
        public frmConfirmarCasillero(string mensaje, string textoConfirmar)
        {
            InitializeComponent();

            lblMensaje.Text = mensaje;
            if (!string.IsNullOrEmpty(textoConfirmar))
                btnConfirmar.Text = textoConfirmar;

            VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
        }

        public static bool Preguntar(IWin32Window propietario, string mensaje, string textoConfirmar = "Sí, continuar")
        {
            using (var frm = new frmConfirmarCasillero(mensaje, textoConfirmar))
                return frm.ShowDialog(propietario) == DialogResult.Yes;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parametros = base.CreateParams;
                parametros.ClassStyle |= VentanaPersonalizada.CS_DROPSHADOW;
                return parametros;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (VentanaPersonalizada.TryObtenerHitTest(this, m, 56, permitirRedimension: false, resultado: out var resultado))
            {
                m.Result = resultado;
                return;
            }

            base.WndProc(ref m);
        }

        private void btnConfirmar_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnCancelar_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }
    }
}
