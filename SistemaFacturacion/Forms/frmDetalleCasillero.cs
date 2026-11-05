using LogicaNegocios.Casilleros;
using LogicaNegocios.Services;
using SistemaFacturacion.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmDetalleCasillero : Form
    {
        private readonly CasilleroAsignacionManejador _asignaciones;
        private readonly int _idAsignacion;
        private readonly string _usuario;
        private readonly string _ip;
        private readonly Color _colorEstado;
        private readonly string _estadoTexto;

        public bool SeLiberoElCasillero { get; private set; }

        public frmDetalleCasillero(
            AppServices services, int idAsignacion, string codigo, string nombreCliente,
            DateTime? fechaInicio, DateTime? fechaFin, int? minutosSolicitados,
            bool vencido, bool avisoEnviado, string usuario, string ip)
        {
            InitializeComponent();

            _asignaciones = new CasilleroAsignacionManejador(
                services.Conexion, services.Log, new CasilleroManejador(services.Conexion, services.Log),
                new CasilleroNotificacionManejador(services.Conexion, services.Log));
            _idAsignacion = idAsignacion;
            _usuario = usuario;
            _ip = ip;

            _colorEstado = vencido
                ? Color.FromArgb(220, 53, 69)
                : avisoEnviado ? Color.FromArgb(255, 147, 41) : Color.FromArgb(0, 123, 255);
            _estadoTexto = vencido ? "VENCIDO" : avisoEnviado ? "POR VENCER" : "OCUPADO";

            lblTitulo.Text = "Casillero " + codigo;
            lblClienteValor.Text = nombreCliente;
            lblInicioValor.Text = fechaInicio?.ToString("HH:mm") ?? "";
            lblDuracionValor.Text = minutosSolicitados.HasValue ? minutosSolicitados.Value + " minutos" : "";
            lblTiempoEtiqueta.Text = vencido ? "DEBIÓ LIBERARSE A LAS" : "HORA FIN";
            lblTiempoValor.Text = fechaFin?.ToString("HH:mm") ?? "";

            pnlHeader.BackColor = _colorEstado;
            btnCerrarVentana.BackColor = _colorEstado;
            btnCerrarVentana.ColorHoverPersonalizado = Color.FromArgb(
                Math.Max(0, _colorEstado.R - 30), Math.Max(0, _colorEstado.G - 30), Math.Max(0, _colorEstado.B - 30));

            pnlCuerpo.Paint += PnlCuerpo_Paint;

            VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
        }

        private void PnlCuerpo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            const float diametroBadge = 64F;
            var rectBadge = new RectangleF((pnlCuerpo.Width - diametroBadge) / 2f, 20F, diametroBadge, diametroBadge);
            using (var brochaFondo = new SolidBrush(Color.FromArgb(26, _colorEstado.R, _colorEstado.G, _colorEstado.B)))
                g.FillEllipse(brochaFondo, rectBadge);
            using (var lapiz = new Pen(_colorEstado, 2.4f))
                g.DrawEllipse(lapiz, rectBadge);
            using (var fuenteIcono = IconosCasilleros.CrearFuente(26F))
            using (var brochaIcono = new SolidBrush(_colorEstado))
            using (var formatoIcono = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Candado), fuenteIcono, brochaIcono, rectBadge, formatoIcono);

            using (var fuentePill = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                Size tamanoPill = TextRenderer.MeasureText(_estadoTexto, fuentePill, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                var rectPill = new Rectangle((pnlCuerpo.Width - tamanoPill.Width - 28) / 2, (int)rectBadge.Bottom + 12, tamanoPill.Width + 28, 26);
                using (var rutaPill = ConstruirRedondeado(rectPill, 13))
                using (var brochaPill = new SolidBrush(_colorEstado))
                    g.FillPath(brochaPill, rutaPill);
                TextRenderer.DrawText(g, _estadoTexto, fuentePill, rectPill, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            const int filas = 4;
            const int altoFila = 52;
            var rectTarjeta = new Rectangle(24, 140, pnlCuerpo.Width - 48, altoFila * filas);
            using (var rutaTarjeta = ConstruirRedondeado(rectTarjeta, 12))
            using (var brochaTarjeta = new SolidBrush(Color.FromArgb(247, 248, 249)))
                g.FillPath(brochaTarjeta, rutaTarjeta);

            using (var lapizDivisor = new Pen(Color.FromArgb(230, 232, 235)))
            {
                for (int i = 0; i < filas - 1; i++)
                {
                    int y = rectTarjeta.Top + (i + 1) * altoFila + 4;
                    g.DrawLine(lapizDivisor, rectTarjeta.Left + 16, y, rectTarjeta.Right - 16, y);
                }
            }

            using (var fuenteIconoChico = IconosCasilleros.CrearFuente(13F))
            using (var brochaIconoChico = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var formatoIconoChico = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                int[] iconosFila = { IconosCasilleros.Usuario, IconosCasilleros.Reloj, IconosCasilleros.Duracion, IconosCasilleros.Reloj };
                for (int i = 0; i < filas; i++)
                {
                    int yIcono = rectTarjeta.Top + i * altoFila + 19;
                    g.DrawString(IconosCasilleros.Glifo(iconosFila[i]), fuenteIconoChico, brochaIconoChico,
                        new RectangleF(40, yIcono, 24, 24), formatoIconoChico);
                }
            }
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

        private void btnLiberar_Click(object sender, EventArgs e)
        {
            bool confirmado = frmConfirmarCasillero.Preguntar(this,
                "¿Confirma que el cliente retiró sus pertenencias y se libera el casillero?",
                "Sí, liberar");

            if (!confirmado)
                return;

            _asignaciones.FinalizarManual(_idAsignacion, _usuario, _ip);
            SeLiberoElCasillero = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
