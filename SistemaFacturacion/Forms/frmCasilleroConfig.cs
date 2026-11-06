using LogicaNegocios.Casilleros;
using LogicaNegocios.Services;
using SistemaFacturacion.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmCasilleroConfig : Form
    {
        private static readonly Color ColorAccento = Color.FromArgb(0, 123, 255);

        private readonly CasilleroManejador _casilleros;
        private readonly CasilleroParametrosManejador _parametros;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        public frmCasilleroConfig(AppServices services)
        {
            InitializeComponent();

            _casilleros = new CasilleroManejador(services.Conexion, services.Log);
            _parametros = new CasilleroParametrosManejador(services.Conexion, services.Log);

            int actuales = _casilleros.ContarActivos();
            numCantidad.Value = actuales;
            lblCantidadActual.Text = "Actualmente hay " + actuales + " casillero(s) activo(s).";

            numMinutos.Value = _parametros.ObtenerMinutosNotificacion();

            lblIconoTitulo.Text = IconosCasilleros.Glifo(IconosCasilleros.Configuracion);
            lblIconoTitulo.UseCompatibleTextRendering = true;

            pnlCuerpo.Paint += PnlCuerpo_Paint;
            ConfigurarCajaTexto(pnlCajaCantidad, numCantidad);
            ConfigurarCajaTexto(pnlCajaMinutos, numMinutos);
            ConfigurarCajaTexto(pnlCajaSonido, cmbSonido);

            CargarSonidos(_parametros.ObtenerSonidoNotificacion());

            VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
        }

        private void CargarSonidos(string claveSeleccionada)
        {
            cmbSonido.DisplayMember = "Value";
            cmbSonido.ValueMember = "Key";
            cmbSonido.DataSource = SonidosCasilleros.ObtenerDisponibles()
                .Select(s => new KeyValuePair<string, string>(s.Clave, s.Etiqueta))
                .ToList();

            cmbSonido.SelectedValue = claveSeleccionada;
            if (cmbSonido.SelectedIndex < 0 && cmbSonido.Items.Count > 0)
                cmbSonido.SelectedIndex = 0;
        }

        private void PnlCuerpo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const int altoFilaCantidad = 90;
            const int altoFilaMinutos = 70;
            const int altoFilaSonido = 70;
            var rectTarjeta = new Rectangle(24, 20, pnlCuerpo.Width - 48, altoFilaCantidad + altoFilaMinutos + altoFilaSonido);
            using (var rutaTarjeta = ConstruirRedondeado(rectTarjeta, 12))
            using (var brochaTarjeta = new SolidBrush(Color.FromArgb(247, 248, 249)))
                g.FillPath(brochaTarjeta, rutaTarjeta);

            using (var lapizDivisor = new Pen(Color.FromArgb(230, 232, 235)))
            {
                int y1 = rectTarjeta.Top + altoFilaCantidad + 4;
                g.DrawLine(lapizDivisor, rectTarjeta.Left + 16, y1, rectTarjeta.Right - 16, y1);
                int y2 = rectTarjeta.Top + altoFilaCantidad + altoFilaMinutos + 4;
                g.DrawLine(lapizDivisor, rectTarjeta.Left + 16, y2, rectTarjeta.Right - 16, y2);
            }

            using (var fuenteIconoChico = IconosCasilleros.CrearFuente(13F))
            using (var brochaIconoChico = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var formatoIconoChico = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Casilleros), fuenteIconoChico, brochaIconoChico,
                    new RectangleF(40, rectTarjeta.Top + 18, 24, 24), formatoIconoChico);
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Reloj), fuenteIconoChico, brochaIconoChico,
                    new RectangleF(40, rectTarjeta.Top + altoFilaCantidad + 18, 24, 24), formatoIconoChico);
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Volumen), fuenteIconoChico, brochaIconoChico,
                    new RectangleF(40, rectTarjeta.Top + altoFilaCantidad + altoFilaMinutos + 18, 24, 24), formatoIconoChico);
            }
        }

        private void ConfigurarCajaTexto(Panel panel, Control control)
        {
            void ActualizarRegion()
            {
                if (panel.Width <= 0 || panel.Height <= 0) return;
                using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, panel.Width, panel.Height), 8))
                    panel.Region = new Region(ruta);
            }

            ActualizarRegion();
            panel.Resize += (s, e) => ActualizarRegion();
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                bool activo = control.Focused;
                using (var lapiz = new Pen(activo ? ColorAccento : Color.FromArgb(210, 213, 217), activo ? 1.6f : 1f))
                    e.Graphics.DrawPath(lapiz, ConstruirRedondeado(
                        new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8));
            };
            control.Enter += (s, e) => panel.Invalidate();
            control.Leave += (s, e) => panel.Invalidate();
        }

        private static GraphicsPath ConstruirRedondeado(Rectangle rect, int radio)
        {
            var gp = new GraphicsPath();
            int d = Math.Min(radio * 2, Math.Min(rect.Width, rect.Height));
            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
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

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var pendientes = _casilleros.FijarCantidad((int)numCantidad.Value, UsuarioActual, IPActual);
                _parametros.GuardarMinutosNotificacion((int)numMinutos.Value, UsuarioActual, IPActual);
                _parametros.GuardarSonidoNotificacion((string)cmbSonido.SelectedValue, UsuarioActual, IPActual);

                int actuales = _casilleros.ContarActivos();
                lblCantidadActual.Text = "Actualmente hay " + actuales + " casillero(s) activo(s).";

                if (pendientes.Count > 0)
                {
                    MessageBox.Show(this,
                        "Se guardó la configuración, pero no se pudieron quitar estos casilleros porque están en uso:\n\n" +
                        string.Join(", ", pendientes) + "\n\nLibéralos primero y vuelve a intentar.",
                        "Casilleros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(this, "Configuración guardada.", "Casilleros",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Casilleros", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrarVentana_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnAgregarSonido_Click(object sender, EventArgs e)
        {
            using (var dialogo = new OpenFileDialog { Filter = "Archivos de sonido (*.wav;*.mp3)|*.wav;*.mp3", Title = "Agregar sonido de notificación" })
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    string clave = SonidosCasilleros.AgregarPersonalizado(dialogo.FileName);
                    CargarSonidos(clave);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "No se pudo agregar el sonido: " + ex.Message, "Casilleros",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnProbarSonido_Click(object sender, EventArgs e)
        {
            try
            {
                SonidosCasilleros.Reproducir((string)cmbSonido.SelectedValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo reproducir el sonido: " + ex.Message, "Casilleros",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
