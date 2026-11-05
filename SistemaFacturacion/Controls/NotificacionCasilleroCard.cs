using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion.Controls
{
    public class NotificacionCasilleroCard : Panel
    {
        public const int AnchoTarjeta = 300;
        private const int DiametroIcono = 34;

        private const int AltoHora = 16;

        private readonly string _mensaje;
        private readonly string _hora;
        private readonly int _altoMensaje;
        private readonly int _anchoTexto;
        private readonly Font _fuenteMensaje;
        private readonly Color _colorTipo;
        private readonly int _codigoIcono;
        private bool _hover;

        public int IdNotificacion { get; }
        public string Tipo { get; }
        public DateTime FechaCreacion { get; }

        public NotificacionCasilleroCard(int id, string mensaje, string tipo, DateTime fechaCreacion)
        {
            IdNotificacion = id;
            Tipo = tipo;
            FechaCreacion = fechaCreacion;
            _mensaje = mensaje;
            _hora = fechaCreacion.ToString("HH:mm");
            _colorTipo = tipo == "VENCIDO" ? Color.FromArgb(220, 53, 69) : Color.FromArgb(255, 147, 41);
            _codigoIcono = tipo == "VENCIDO" ? IconosCasilleros.Advertencia : IconosCasilleros.Reloj;
            _fuenteMensaje = new Font("Segoe UI", 8.75F);

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _anchoTexto = AnchoTarjeta - 14 - DiametroIcono - 12 - 14;
            Size tamanoTexto = TextRenderer.MeasureText(mensaje, _fuenteMensaje, new Size(_anchoTexto, 0), TextFormatFlags.WordBreak);
            _altoMensaje = Math.Max(DiametroIcono - AltoHora, tamanoTexto.Height);

            Width = AnchoTarjeta;
            Height = 14 + _altoMensaje + AltoHora + 14;
            Margin = new Padding(0, 0, 0, 8);
            Cursor = Cursors.Hand;

            Resize += (s, e) => AplicarBordeRedondeado();
            AplicarBordeRedondeado();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width, Height);
            using (var ruta = ConstruirRedondeado(rect, 12))
            using (var brochaFondo = new SolidBrush(_hover ? Color.FromArgb(238, 241, 244) : Color.FromArgb(249, 250, 251)))
                g.FillPath(brochaFondo, ruta);

            var rectInsignia = new RectangleF(14, (Height - DiametroIcono) / 2f, DiametroIcono, DiametroIcono);
            using (var brochaInsignia = new SolidBrush(Color.FromArgb(26, _colorTipo.R, _colorTipo.G, _colorTipo.B)))
                g.FillEllipse(brochaInsignia, rectInsignia);
            using (var fuenteIcono = IconosCasilleros.CrearFuente(13F))
            using (var brochaIcono = new SolidBrush(_colorTipo))
            using (var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(IconosCasilleros.Glifo(_codigoIcono), fuenteIcono, brochaIcono, rectInsignia, formato);

            int xTexto = 14 + DiametroIcono + 12;
            var rectTexto = new Rectangle(xTexto, 12, _anchoTexto, _altoMensaje);
            TextRenderer.DrawText(g, _mensaje, _fuenteMensaje, rectTexto, Color.FromArgb(33, 37, 41),
                TextFormatFlags.WordBreak | TextFormatFlags.Top | TextFormatFlags.Left | TextFormatFlags.NoPadding);

            var rectHora = new Rectangle(xTexto, rectTexto.Bottom, _anchoTexto, AltoHora);
            using (var fuenteHora = new Font("Segoe UI", 7.5F))
                TextRenderer.DrawText(g, _hora, fuenteHora, rectHora, Color.FromArgb(148, 155, 162),
                    TextFormatFlags.Top | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        private void AplicarBordeRedondeado()
        {
            if (Width <= 0 || Height <= 0) return;
            using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, Width, Height), 12))
                Region = new Region(ruta);
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
    }
}
