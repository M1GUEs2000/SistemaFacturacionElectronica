using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion.Controls
{
    public class IconButton : Button
    {
        private int _codigoIcono;
        private bool _hover;
        private bool _presionado;

        public int CodigoIcono
        {
            get => _codigoIcono;
            set { _codigoIcono = value; Invalidate(); }
        }

        public int RadioEsquinas { get; set; } = 8;

        public bool IconoArriba { get; set; }

        public Color? ColorHoverPersonalizado { get; set; }

        public IconButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AplicarBordeRedondeado();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AplicarBordeRedondeado();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); _presionado = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _presionado = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool tieneIcono = _codigoIcono != 0;
            string texto = Text ?? "";

            if (IconoArriba)
            {
                PintarTarjetaAcceso(g, tieneIcono, texto);
                return;
            }

            Color fondo;
            if (!Enabled) fondo = Color.FromArgb(200, 200, 200);
            else if (_presionado) fondo = Oscurecer(ColorHoverPersonalizado ?? BackColor, 0.18f);
            else if (_hover) fondo = ColorHoverPersonalizado ?? Oscurecer(BackColor, 0.08f);
            else fondo = BackColor;

            g.Clear(Parent?.BackColor ?? SystemColors.Control);
            using (var brocha = new SolidBrush(fondo))
                g.FillPath(brocha, ConstruirRuta(ClientRectangle, RadioEsquinas));

            if (!tieneIcono)
            {
                TextRenderer.DrawText(g, texto, Font, ClientRectangle, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                return;
            }

            using (var fuenteIcono = IconosCasilleros.CrearFuente(Font.Size + 2.5F))
            using (var brochaTexto = new SolidBrush(ForeColor))
            {
                string glifo = IconosCasilleros.Glifo(_codigoIcono);

                if (string.IsNullOrEmpty(texto))
                {
                    using (var formatoCentro = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(glifo, fuenteIcono, brochaTexto, ClientRectangle, formatoCentro);
                    return;
                }

                using (var formato = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                {
                    SizeF tamanoIcono = g.MeasureString(glifo, fuenteIcono, PointF.Empty, StringFormat.GenericTypographic);
                    Size tamanoTexto = TextRenderer.MeasureText(g, texto, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

                    const int espacio = 8;
                    int anchoTotal = (int)tamanoIcono.Width + espacio + tamanoTexto.Width;
                    int x0 = Math.Max(6, (Width - anchoTotal) / 2);

                    var rectIcono = new RectangleF(x0, (Height - tamanoIcono.Height) / 2, tamanoIcono.Width + 2, tamanoIcono.Height);
                    g.DrawString(glifo, fuenteIcono, brochaTexto, rectIcono, formato);

                    var rectTexto = new Rectangle(x0 + (int)tamanoIcono.Width + espacio, 0, tamanoTexto.Width + 4, Height);
                    TextRenderer.DrawText(g, texto, Font, rectTexto, ForeColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                }
            }
        }

        private void PintarTarjetaAcceso(Graphics g, bool tieneIcono, string texto)
        {
            var ruta = ConstruirRuta(ClientRectangle, RadioEsquinas);

            Color baseColor = !Enabled ? Color.FromArgb(190, 190, 190) : BackColor;
            Color colorSuperior = Oscurecer(baseColor, _presionado ? 0.12f : -0.12f);
            Color colorInferior = Oscurecer(baseColor, _presionado ? 0.30f : (_hover ? 0.14f : 0.20f));

            g.Clear(Parent?.BackColor ?? SystemColors.Control);

            using (var brochaFondo = new LinearGradientBrush(ClientRectangle, colorSuperior, colorInferior, LinearGradientMode.Vertical))
                g.FillPath(brochaFondo, ruta);

            var rectBrillo = new Rectangle(0, 0, Width, Math.Max(1, Height / 2));
            using (var rutaBrillo = ConstruirRuta(rectBrillo, RadioEsquinas))
            using (var brochaBrillo = new SolidBrush(Color.FromArgb(_hover ? 38 : 26, 255, 255, 255)))
            {
                var clipAnterior = g.Clip;
                g.SetClip(ruta, CombineMode.Replace);
                g.FillPath(brochaBrillo, rutaBrillo);
                g.Clip = clipAnterior;
            }

            if (_hover)
            {
                using (var rutaAnillo = ConstruirRuta(Rectangle.Inflate(ClientRectangle, -1, -1), Math.Max(0, RadioEsquinas - 1)))
                using (var lapizAnillo = new Pen(Color.FromArgb(_presionado ? 90 : 140, 255, 255, 255), 1.6f))
                    g.DrawPath(lapizAnillo, rutaAnillo);
            }

            if (!tieneIcono)
            {
                TextRenderer.DrawText(g, texto, Font, ClientRectangle, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                return;
            }

            float diametroInsignia = Math.Min(Width, Height) * (_hover && !_presionado ? 0.54f : 0.5f);
            var rectInsignia = new RectangleF((Width - diametroInsignia) / 2f, 5f - (_presionado ? 1 : (_hover ? 1 : 0)), diametroInsignia, diametroInsignia);
            using (var brochaInsignia = new SolidBrush(Color.FromArgb(_presionado ? 26 : (_hover ? 58 : 40), 255, 255, 255)))
                g.FillEllipse(brochaInsignia, rectInsignia);

            using (var fuenteIcono = IconosCasilleros.CrearFuente(Font.Size + 4.5F))
            using (var brochaIcono = new SolidBrush(ForeColor))
            using (var formatoIcono = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                string glifo = IconosCasilleros.Glifo(_codigoIcono);
                g.DrawString(glifo, fuenteIcono, brochaIcono, rectInsignia, formatoIcono);
            }

            var rectTexto = new Rectangle(2, (int)(rectInsignia.Bottom) + 2, Width - 4, Height - (int)rectInsignia.Bottom - 4);
            using (var fuenteTexto = new Font(Font, FontStyle.Bold))
                TextRenderer.DrawText(g, texto, fuenteTexto, rectTexto, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            if (_hover)
            {
                int anchoBarra = (int)(Width * 0.34f);
                var rectBarra = new Rectangle((Width - anchoBarra) / 2, Height - 5, anchoBarra, 2);
                using (var rutaBarra = ConstruirRuta(rectBarra, 1))
                using (var brochaBarra = new SolidBrush(Color.FromArgb(_presionado ? 130 : 200, 255, 255, 255)))
                    g.FillPath(brochaBarra, rutaBarra);
            }
        }

        private void AplicarBordeRedondeado()
        {
            if (Width <= 0 || Height <= 0) return;
            Region = new Region(ConstruirRuta(ClientRectangle, RadioEsquinas));
        }

        private static GraphicsPath ConstruirRuta(Rectangle rect, int radio)
        {
            var ruta = new GraphicsPath();
            if (radio <= 0) { ruta.AddRectangle(rect); return ruta; }

            int d = Math.Min(radio * 2, Math.Min(rect.Width, rect.Height));
            ruta.AddArc(rect.X, rect.Y, d, d, 180, 90);
            ruta.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            ruta.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            ruta.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }

        private static Color Oscurecer(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                Math.Min(255, Math.Max(0, (int)(color.R * (1 - factor)))),
                Math.Min(255, Math.Max(0, (int)(color.G * (1 - factor)))),
                Math.Min(255, Math.Max(0, (int)(color.B * (1 - factor)))));
        }
    }
}
