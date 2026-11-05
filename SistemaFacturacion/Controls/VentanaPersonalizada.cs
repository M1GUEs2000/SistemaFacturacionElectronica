using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion.Controls
{
    internal static class VentanaPersonalizada
    {
        private const int WM_NCHITTEST = 0x84;
        internal const int CS_DROPSHADOW = 0x00020000;

        public static bool TryObtenerHitTest(Form form, Message m, int altoArrastre, bool permitirRedimension, out IntPtr resultado)
        {
            resultado = IntPtr.Zero;
            if (m.Msg != WM_NCHITTEST || form.WindowState != FormWindowState.Normal)
                return false;

            const int grip = 6;
            int lp = m.LParam.ToInt32();
            var punto = form.PointToClient(new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF)));

            if (permitirRedimension)
            {
                bool izquierda = punto.X <= grip;
                bool derecha = punto.X >= form.ClientSize.Width - grip;
                bool arriba = punto.Y <= grip;
                bool abajo = punto.Y >= form.ClientSize.Height - grip;

                if (arriba && izquierda) { resultado = (IntPtr)13; return true; }
                if (arriba && derecha) { resultado = (IntPtr)14; return true; }
                if (abajo && izquierda) { resultado = (IntPtr)16; return true; }
                if (abajo && derecha) { resultado = (IntPtr)17; return true; }
                if (izquierda) { resultado = (IntPtr)10; return true; }
                if (derecha) { resultado = (IntPtr)11; return true; }
                if (arriba) { resultado = (IntPtr)12; return true; }
                if (abajo) { resultado = (IntPtr)15; return true; }
            }

            if (punto.Y >= 0 && punto.Y <= altoArrastre)
            {
                resultado = (IntPtr)2;
                return true;
            }

            return false;
        }

        public static void AplicarEsquinasRedondeadas(Form form, int radio)
        {
            if (form.Width <= 0 || form.Height <= 0) return;

            var gp = new GraphicsPath();
            int d = Math.Min(radio * 2, Math.Min(form.Width, form.Height));
            var rect = new Rectangle(0, 0, form.Width, form.Height);

            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();

            form.Region = new Region(gp);
        }
    }
}
