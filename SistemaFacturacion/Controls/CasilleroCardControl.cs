using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion.Controls
{
    public class CasilleroCardControl : Panel
    {
        private bool _hover;

        public int IdCasillero { get; }
        public string Codigo { get; }
        public int? IdAsignacion { get; private set; }
        public string Estado { get; private set; }
        public string NombreCliente { get; private set; }
        public DateTime? FechaInicio { get; private set; }
        public DateTime? FechaFin { get; private set; }
        public int? MinutosSolicitados { get; private set; }
        public bool Notificado { get; private set; }

        public bool EsDisponible => Estado == "DISPONIBLE";
        public bool EsVencido => Estado == "VENCIDO";

        public CasilleroCardControl(
            int idCasillero, string codigo, string estado, int? idAsignacion, string nombreCliente,
            DateTime? fechaInicio, DateTime? fechaFin, int? minutosSolicitados, bool notificado)
        {
            IdCasillero = idCasillero;
            Codigo = codigo;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Size = new Size(184, 172);
            Margin = new Padding(8);
            Cursor = Cursors.Hand;

            Resize += (s, e) => AplicarBordeRedondeado();
            AplicarBordeRedondeado();

            Actualizar(estado, idAsignacion, nombreCliente, fechaInicio, fechaFin, minutosSolicitados, notificado);
        }

        public void Actualizar(
            string estado, int? idAsignacion, string nombreCliente,
            DateTime? fechaInicio, DateTime? fechaFin, int? minutosSolicitados, bool notificado)
        {
            Estado = estado;
            IdAsignacion = idAsignacion;
            NombreCliente = nombreCliente;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            MinutosSolicitados = minutosSolicitados;
            Notificado = notificado;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool vencido = Estado == "VENCIDO";
            bool ocupado = Estado == "OCUPADO";
            bool avisoEnviado = ocupado && Notificado;
            bool disponible = Estado == "DISPONIBLE";

            Color colorEstado =
                disponible ? Color.FromArgb(40, 167, 69) :
                vencido ? Color.FromArgb(220, 53, 69) :
                avisoEnviado ? Color.FromArgb(255, 147, 41) :
                Color.FromArgb(0, 123, 255);

            string estadoTexto =
                disponible ? "DISPONIBLE" :
                vencido ? "VENCIDO" :
                avisoEnviado ? "POR VENCER" :
                "OCUPADO";

            int codigoIcono = disponible ? IconosCasilleros.CandadoAbierto : IconosCasilleros.Candado;

            var rectTotal = new Rectangle(0, 0, Width, Height);

            using (var rutaTarjeta = ConstruirRedondeado(rectTotal, 16, false))
            using (var brochaFondo = new LinearGradientBrush(rectTotal, Color.FromArgb(240, 242, 245), Color.FromArgb(220, 224, 229), LinearGradientMode.Vertical))
                g.FillPath(brochaFondo, rutaTarjeta);

            var rectCabecera = new Rectangle(0, 0, Width, 36);
            using (var rutaCabecera = ConstruirRedondeado(rectCabecera, 16, true))
            using (var brochaCabecera = new LinearGradientBrush(rectCabecera, ClarearColor(colorEstado, 0.10f), colorEstado, LinearGradientMode.Vertical))
                g.FillPath(brochaCabecera, rutaCabecera);

            using (var fuenteCodigo = new Font("Segoe UI", 13F, FontStyle.Bold))
                TextRenderer.DrawText(g, Codigo, fuenteCodigo, rectCabecera, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

            using (var lapizTextura = new Pen(Color.FromArgb(24, 0, 0, 0), 1f))
            {
                for (int i = 0; i < 3; i++)
                {
                    int y = 45 + i * 5;
                    g.DrawLine(lapizTextura, 16, y, Width - 16, y);
                }
                g.DrawLine(lapizTextura, Width / 2, 64, Width / 2, Height - 10);
            }

            const float diametro = 52f;
            var rectInsignia = new RectangleF((Width - diametro) / 2f, 50f, diametro, diametro);
            using (var brochaInsignia = new SolidBrush(Color.FromArgb(28, colorEstado.R, colorEstado.G, colorEstado.B)))
                g.FillEllipse(brochaInsignia, rectInsignia);
            using (var lapizInsignia = new Pen(colorEstado, 2.4f))
                g.DrawEllipse(lapizInsignia, rectInsignia);

            using (var fuenteIcono = IconosCasilleros.CrearFuente(19F))
            using (var brochaIcono = new SolidBrush(colorEstado))
            using (var formatoIcono = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                string glifo = IconosCasilleros.Glifo(codigoIcono);
                g.DrawString(glifo, fuenteIcono, brochaIcono, rectInsignia, formatoIcono);
            }

            using (var fuenteChip = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            {
                Size tamanoChip = TextRenderer.MeasureText(g, estadoTexto, fuenteChip, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                var rectChip = new Rectangle((Width - tamanoChip.Width - 18) / 2, (int)rectInsignia.Bottom + 6, tamanoChip.Width + 18, 19);
                using (var rutaChip = ConstruirRedondeado(rectChip, 9, false))
                using (var brochaChip = new SolidBrush(colorEstado))
                    g.FillPath(brochaChip, rutaChip);
                TextRenderer.DrawText(g, estadoTexto, fuenteChip, rectChip, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

                string cuerpo;
                if (disponible)
                {
                    cuerpo = "Click para asignar";
                }
                else if (vencido)
                {
                    cuerpo = (NombreCliente ?? "") + "\n¡Debe liberarse!";
                }
                else
                {
                    TimeSpan restante = (FechaFin ?? DateTime.Now) - DateTime.Now;
                    string tiempoTexto = restante.TotalSeconds > 0 ? restante.ToString(@"hh\:mm\:ss") : "00:00:00";
                    cuerpo = (NombreCliente ?? "") + "\n" + tiempoTexto;
                }

                var rectCuerpo = new Rectangle(4, rectChip.Bottom + 6, Width - 8, Height - rectChip.Bottom - 10);
                using (var fuenteCuerpo = new Font("Segoe UI", 8.75F, disponible ? FontStyle.Italic : FontStyle.Bold))
                    TextRenderer.DrawText(g, cuerpo, fuenteCuerpo, rectCuerpo, Color.FromArgb(52, 58, 64),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            }

            if (_hover)
            {
                using (var rutaAnillo = ConstruirRedondeado(Rectangle.Inflate(rectTotal, -1, -1), 15, false))
                using (var lapizAnillo = new Pen(Color.FromArgb(180, colorEstado), 2.2f))
                    g.DrawPath(lapizAnillo, rutaAnillo);
            }
        }

        private void AplicarBordeRedondeado()
        {
            if (Width <= 0 || Height <= 0) return;
            using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, Width, Height), 16, false))
                Region = new Region(ruta);
        }

        private static GraphicsPath ConstruirRedondeado(Rectangle rect, int radio, bool soloArriba)
        {
            var gp = new GraphicsPath();
            int d = Math.Min(radio * 2, Math.Min(rect.Width, rect.Height));

            if (soloArriba)
            {
                gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
                gp.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                gp.AddLine(rect.Right, rect.Y + d / 2, rect.Right, rect.Bottom);
                gp.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
                gp.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + d / 2);
                gp.CloseFigure();
                return gp;
            }

            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        private static Color ClarearColor(Color c, float factor)
        {
            return Color.FromArgb(c.A,
                Math.Min(255, (int)(c.R + (255 - c.R) * factor)),
                Math.Min(255, (int)(c.G + (255 - c.G) * factor)),
                Math.Min(255, (int)(c.B + (255 - c.B) * factor)));
        }
    }
}
