using LogicaNegocios.Casilleros;
using LogicaNegocios.Services;
using SistemaFacturacion.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmAsignarCasillero : Form
    {
        private static readonly Color ColorAccento = Color.FromArgb(40, 167, 69);

        private readonly CasilleroAsignacionManejador _asignaciones;
        private readonly string _usuario;
        private readonly string _ip;
        private readonly int? _idCasilleroFijo;
        private readonly string _codigoFijo;
        private readonly string _textoPill;

        public ResultadoAsignacion Resultado { get; private set; }

        public frmAsignarCasillero(AppServices services, string usuario, string ip)
            : this(services, usuario, ip, null, null)
        {
        }

        public frmAsignarCasillero(AppServices services, string usuario, string ip, int? idCasillero, string codigo)
        {
            InitializeComponent();

            _asignaciones = new CasilleroAsignacionManejador(
                services.Conexion, services.Log, new CasilleroManejador(services.Conexion, services.Log),
                new CasilleroNotificacionManejador(services.Conexion, services.Log));
            _usuario = usuario;
            _ip = ip;
            _idCasilleroFijo = idCasillero;
            _codigoFijo = codigo;

            lblTitulo.Text = _idCasilleroFijo.HasValue ? "Asignar casillero " + _codigoFijo : "Asignar casillero";
            _textoPill = _idCasilleroFijo.HasValue ? "CASILLERO " + _codigoFijo : "ASIGNACIÓN AUTOMÁTICA";

            lblIconoTitulo.Text = IconosCasilleros.Glifo(IconosCasilleros.CandadoAbierto);
            lblIconoTitulo.UseCompatibleTextRendering = true;

            pnlCuerpo.Paint += PnlCuerpo_Paint;
            ConfigurarCajaTexto(pnlCajaCliente, txtCliente);
            ConfigurarCajaTexto(pnlCajaMinutos, numMinutos);
            ConfigurarChipsMinutos();

            txtCliente.Focus();

            VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
        }

        private IconButton _chipSeleccionado;

        private void ConfigurarChipsMinutos()
        {
            btnChip30.Tag = 30;
            btnChip60.Tag = 60;
            btnChip90.Tag = 90;
            btnChip120.Tag = 120;
            btnChipOtro.Tag = null;

            _chipSeleccionado = btnChip60;
            ActualizarSeleccionChips();
        }

        private void BtnChip_Click(object sender, EventArgs e)
        {
            _chipSeleccionado = (IconButton)sender;
            if (_chipSeleccionado.Tag is int minutos)
                numMinutos.Value = minutos;

            ActualizarSeleccionChips();
        }

        private void ActualizarSeleccionChips()
        {
            foreach (var chip in new[] { btnChip30, btnChip60, btnChip90, btnChip120, btnChipOtro })
            {
                bool seleccionado = chip == _chipSeleccionado;
                chip.BackColor = seleccionado ? ColorAccento : Color.FromArgb(241, 243, 245);
                chip.ForeColor = seleccionado ? Color.White : Color.FromArgb(73, 80, 87);
            }

            bool personalizado = _chipSeleccionado == btnChipOtro;
            pnlCajaMinutos.Visible = personalizado;
            lblMinutosPersonalizadoEtiqueta.Visible = personalizado;
            if (personalizado) numMinutos.Focus();
        }

        private void PnlCuerpo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            const float diametroBadge = 64F;
            var rectBadge = new RectangleF((pnlCuerpo.Width - diametroBadge) / 2f, 20F, diametroBadge, diametroBadge);
            using (var brochaFondo = new SolidBrush(Color.FromArgb(26, ColorAccento.R, ColorAccento.G, ColorAccento.B)))
                g.FillEllipse(brochaFondo, rectBadge);
            using (var lapiz = new Pen(ColorAccento, 2.4f))
                g.DrawEllipse(lapiz, rectBadge);
            using (var fuenteIcono = IconosCasilleros.CrearFuente(26F))
            using (var brochaIcono = new SolidBrush(ColorAccento))
            using (var formatoIcono = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.CandadoAbierto), fuenteIcono, brochaIcono, rectBadge, formatoIcono);

            using (var fuentePill = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                Size tamanoPill = TextRenderer.MeasureText(_textoPill, fuentePill, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                var rectPill = new Rectangle((pnlCuerpo.Width - tamanoPill.Width - 28) / 2, (int)rectBadge.Bottom + 12, tamanoPill.Width + 28, 26);
                using (var rutaPill = ConstruirRedondeado(rectPill, 13))
                using (var brochaPill = new SolidBrush(ColorAccento))
                    g.FillPath(brochaPill, rutaPill);
                TextRenderer.DrawText(g, _textoPill, fuentePill, rectPill, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            const int altoFilaCliente = 64;
            const int altoFilaMinutos = 104;
            var rectTarjeta = new Rectangle(24, 140, pnlCuerpo.Width - 48, altoFilaCliente + altoFilaMinutos);
            using (var rutaTarjeta = ConstruirRedondeado(rectTarjeta, 12))
            using (var brochaTarjeta = new SolidBrush(Color.FromArgb(247, 248, 249)))
                g.FillPath(brochaTarjeta, rutaTarjeta);

            using (var lapizDivisor = new Pen(Color.FromArgb(230, 232, 235)))
            {
                int y = rectTarjeta.Top + altoFilaCliente + 4;
                g.DrawLine(lapizDivisor, rectTarjeta.Left + 16, y, rectTarjeta.Right - 16, y);
            }

            using (var fuenteIconoChico = IconosCasilleros.CrearFuente(13F))
            using (var brochaIconoChico = new SolidBrush(Color.FromArgb(108, 117, 125)))
            using (var formatoIconoChico = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Usuario), fuenteIconoChico, brochaIconoChico,
                    new RectangleF(40, rectTarjeta.Top + 17, 24, 24), formatoIconoChico);
                g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Reloj), fuenteIconoChico, brochaIconoChico,
                    new RectangleF(40, rectTarjeta.Top + altoFilaCliente + 17, 24, 24), formatoIconoChico);
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

        private void btnAsignar_Click(object sender, EventArgs e)
        {
            try
            {
                Resultado = _idCasilleroFijo.HasValue
                    ? _asignaciones.AsignarACasillero(
                        _idCasilleroFijo.Value, _codigoFijo, txtCliente.Text.Trim(), (int)numMinutos.Value, _usuario, _ip)
                    : _asignaciones.AsignarAutomatico(
                        txtCliente.Text.Trim(), (int)numMinutos.Value, _usuario, _ip);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
