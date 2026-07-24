using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

public static class Notificaciones
{


    // =======================================
    //  CONTROL DE TOASTS (MÁXIMO 2)
    // =======================================
    private static int toastIndex = 0;
    private static Panel toastProceso = null;
    private static Panel overlayBloqueo = null;
    private static FormClosingEventHandler _handlerCierreBloqueado = null;
    private static Form _formBloqueado = null;


    // =======================================
    //  MOSTRAR TOAST
    // =======================================
    public static bool Show(Form destino, string mensaje, string tipo = "exito",
                            string usuario = null, string ip = null)
    {
        tipo = tipo.Trim().ToLower();

        // ===================================
        // LOG solo para "error"
        // ===================================
        if (tipo == "error")
        {
            try
            {

            }
            catch { }
        }

        // ======================================================================
        // CONFIRMACION (GRIS) – BLOQUEANTE
        // ======================================================================
        if (tipo == "confirmacion")
        {
            bool resultado = false;
            bool respondido = false;

            BloquearUI(destino, 60);

            Color fondo = Color.FromArgb(108, 117, 125);
            Color borde = Color.FromArgb(90, 98, 104);

            Panel container = CrearContenedorToast(fondo, borde);

            Label lblTitulo = CrearTitulo("Confirmación", fondo);
            Panel header = CrearHeader(lblTitulo, fondo);

            Label lblMensaje = CrearMensaje(mensaje);
            Panel body = CrearBody(lblMensaje);

            Panel panelBotones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55
            };

            Button btnSi = new Button
            {
                Text = "Sí",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(110, 10)
            };
            btnSi.FlatAppearance.BorderSize = 0;

            Button btnNo = new Button
            {
                Text = "No",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(220, 10)
            };
            btnNo.FlatAppearance.BorderSize = 0;

            btnSi.Click += (s, e) =>
            {
                resultado = true;
                respondido = true;
                container.Dispose();
                DesbloquearUI(destino);
            };

            btnNo.Click += (s, e) =>
            {
                resultado = false;
                respondido = true;
                container.Dispose();
                DesbloquearUI(destino);
            };

            panelBotones.Controls.Add(btnSi);
            panelBotones.Controls.Add(btnNo);

            Panel footer = CrearFooter(fondo);

            container.Controls.Add(panelBotones);
            container.Controls.Add(body);
            container.Controls.Add(footer);
            container.Controls.Add(header);

            destino.Controls.Add(container);
            container.PerformLayout();

            container.Location = new Point(
                (destino.ClientSize.Width - container.Width) / 2,
                (destino.ClientSize.Height - container.Height) / 2
            );

            overlayBloqueo.BringToFront();
            container.BringToFront();

            // BringToFront solo cambia el z-order: el foco de teclado se queda en el
            // control que lo tenía antes (p.ej. el reportViewer del recibo, que además
            // se come el primer clic al soltarlo). Sin esto el cajero tiene que hacer
            // un clic "en vacío" para poder responder. Enfocar btnNo mete el panel en
            // la conversación — a partir de ahí Sí y No responden al primer clic — y
            // deja el Enter en la opción segura: estas confirmaciones también anulan
            // notas de crédito y retenciones.
            try { destino.ActiveControl = btnNo; }
            catch { /* si el form no puede dar foco, el panel igual funciona con mouse */ }

            while (!respondido)
            {
                Application.DoEvents();
            }
            CerrarProceso(destino);
            return resultado;
        }

        // ======================================================================
        // ÉXITO (VERDE)
        // ======================================================================
        if (tipo == "exito")
        {
            Panel toast = new Panel
            {
                BackColor = Color.FromArgb(35, 160, 70),
                Width = 420,
                Height = 45,
                Tag = "toast"
            };

            toast.Click += (s, e) =>
            {
                destino.Controls.Remove(toast);
                toast.Dispose();
            };

            Label lbl = new Label
            {
                Text = mensaje,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            toast.Controls.Add(lbl);
            destino.Controls.Add(toast);
            toast.BringToFront();

            int x = (destino.ClientSize.Width - toast.Width) / 2;
            toast.Location = new Point(x, 20);

            LimitarToasts(destino, 2);

            Timer t = new Timer { Interval = 2500 };
            t.Tick += (s, e) =>
            {
                Timer fade = new Timer { Interval = 25 };
                int alpha = 255;
                fade.Tick += (s2, e2) =>
                {
                    alpha -= 15;
                    if (alpha <= 0)
                    {
                        fade.Stop();
                        destino.Controls.Remove(toast);
                        toast.Dispose();
                    }
                    else
                    {
                        toast.BackColor = Color.FromArgb(alpha, toast.BackColor);
                        lbl.ForeColor = Color.FromArgb(alpha, lbl.ForeColor);
                    }
                };
                fade.Start();
                t.Stop();
            };
            t.Start();

            return false;
        }

        // ======================================================================
        // PROCESO (AZUL)
        // ======================================================================
        if (tipo == "proceso")
        {
            BloquearUI(destino);

            Color colorFondo = Color.FromArgb(0, 123, 255);
            Color colorBorde = Color.FromArgb(0, 80, 180);

            Panel container = CrearContenedorToast(colorFondo, colorBorde);

            Label lblTitulo = CrearTitulo("Procesando...", colorFondo);
            Panel header = CrearHeader(lblTitulo, colorFondo);

            Label lblMensaje = CrearMensaje(mensaje);
            Panel body = CrearBody(lblMensaje);

            Panel footer = CrearFooter(colorFondo);

            container.Controls.Add(body);
            container.Controls.Add(footer);
            container.Controls.Add(header);

            destino.Controls.Add(container);
            container.PerformLayout();

            container.Location = new Point(
                (destino.ClientSize.Width - container.Width) / 2,
                (destino.ClientSize.Height - container.Height) / 2
            );

            overlayBloqueo.BringToFront();
            container.BringToFront();

            toastProceso = container;

            _formBloqueado = destino;
            _handlerCierreBloqueado = (s, e) =>
            {
                if (!HayProcesoActivo) return;

                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    MessageBox.Show(
                        destino,
                        "No se puede cerrar mientras hay un proceso en curso.\nEspere a que el proceso finalice.",
                        "Proceso activo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            };
            destino.FormClosing += _handlerCierreBloqueado;

            return false;
        }

        // ======================================================================
        // ERROR / ADVERTENCIA
        // ======================================================================
        Color fondoToast, bordeToast;
        string titulo;
        int duracion;

        if (tipo == "error")
        {
            fondoToast = Color.FromArgb(220, 53, 69);
            bordeToast = Color.FromArgb(185, 40, 50);
            titulo = "Error";
            duracion = 12000;
        }
        else
        {
            fondoToast = Color.FromArgb(255, 147, 41);
            bordeToast = Color.FromArgb(230, 120, 20);
            titulo = "Advertencia";
            duracion = 10000;
        }

        Panel cont = CrearContenedorToast(fondoToast, bordeToast);

        Label lblT = CrearTitulo(titulo, fondoToast);
        Panel headerToast = CrearHeader(lblT, fondoToast);

        Button btnCerrar = new Button
        {
            Text = "X",
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Width = 32,
            Height = 32,
            Location = new Point(cont.Width - 40, 2),
            Cursor = Cursors.Hand
        };
        btnCerrar.FlatAppearance.BorderSize = 0;
        btnCerrar.Click += (s, e) => cont.Dispose();

        headerToast.Controls.Add(btnCerrar);

        Label lblM = CrearMensaje(mensaje);
        Panel bodyArea = CrearBody(lblM);
        Panel foot = CrearFooter(fondoToast);

        cont.Controls.Add(bodyArea);
        cont.Controls.Add(foot);
        cont.Controls.Add(headerToast);

        destino.Controls.Add(cont);
        cont.BringToFront();

        cont.Location = new Point(
            (destino.ClientSize.Width - cont.Width) / 2,
            (destino.ClientSize.Height - cont.Height) / 2
        );

        LimitarToasts(destino, 2);

        Timer tm = new Timer { Interval = duracion };
        tm.Tick += (s, e) =>
        {
            Timer fade = new Timer { Interval = 20 };
            int alpha = 255;
            fade.Tick += (s2, e2) =>
            {
                alpha -= 25;
                if (alpha <= 0)
                {
                    fade.Stop();
                    cont.Dispose();
                }
                else
                {
                    cont.BackColor = Color.FromArgb(alpha, Color.White);
                }
            };
            fade.Start();
            tm.Stop();
        };
        tm.Start();

        return false;
    }


    // ==========================================================
    //  HELPERS: CONTENEDOR, HEADER, BODY, FOOTER
    // ==========================================================

    private static Panel CrearContenedorToast(Color colorFondo, Color colorBorde)
    {
        Panel container = new Panel
        {
            Width = 420,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(420, 60),
            BackColor = Color.White,
            Tag = "toast"
        };

        container.Paint += (s, e) =>
        {
            using (Pen p = new Pen(colorBorde, 3))
            {
                e.Graphics.DrawRectangle(
                    p,
                    new Rectangle(1, 1, container.Width - 3, container.Height - 3)
                );
            }
        };

        container.Resize += (s, e) =>
        {
            int r = 12;
            GraphicsPath gp = new GraphicsPath();

            gp.AddArc(0, 0, r, r, 180, 90);
            gp.AddArc(container.Width - r, 0, r, r, 270, 90);
            gp.AddArc(container.Width - r, container.Height - r, r, r, 0, 90);
            gp.AddArc(0, container.Height - r, r, r, 90, 90);

            gp.CloseAllFigures();
            container.Region = new Region(gp);
        };

        return container;
    }

    private static Panel CrearBody(Label lbl)
    {
        Panel body = new Panel
        {
            Dock = DockStyle.Top,                  // ← ya NO Fill
            Padding = new Padding(12, 6, 12, 6),
            AutoSize = true,                       // ← que mida su altura real
            AutoSizeMode = AutoSizeMode.GrowAndShrink
            // Width NO hace falta: hereda el ancho del container
        };

        body.Controls.Add(lbl);
        return body;
    }

    private static Label CrearMensaje(string mensaje)
    {
        return new Label
        {
            Text = mensaje,
            AutoSize = true,
            MaximumSize = new Size(380, 0),
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10),
            Dock = DockStyle.Top
        };
    }


    private static Panel CrearHeader(Label lbl, Color colorFondo)
    {
        Panel header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = colorFondo
        };
        header.Controls.Add(lbl);
        return header;
    }

    private static Label CrearTitulo(string texto, Color fondo)
    {
        return new Label
        {
            Text = texto,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Dock = DockStyle.Left,
            Width = 300,
            Padding = new Padding(12, 8, 0, 0)
        };
    }



    private static Panel CrearFooter(Color fondo)
    {
        return new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 6,
            BackColor = fondo
        };
    }


    // =======================================
    //   LIMPIAR TOASTS MÁS ANTIGUAS
    // =======================================
    private static void LimitarToasts(Form destino, int maximo)
    {
        var lista = destino.Controls.OfType<Panel>()
            .Where(p => p.Tag != null && p.Tag.ToString() == "toast")
            .OrderBy(p => destino.Controls.GetChildIndex(p))
            .ToList();

        while (lista.Count > maximo)
        {
            var viejo = lista.First();
            destino.Controls.Remove(viejo);
            viejo.Dispose();
            lista.RemoveAt(0);
        }
    }

    public static bool HayProcesoActivo =>
        toastProceso != null && !toastProceso.IsDisposed;

    public static void ActualizarProceso(Form destino, string mensaje)
    {
        if (destino == null || destino.IsDisposed) return;

        // Si no existe la notificación azul, salir
        if (toastProceso == null || toastProceso.IsDisposed)
            return;

        try
        {
            destino.Invoke(new Action(() =>
            {
                // Buscar el label del proceso
                var lbl = toastProceso.Controls
                    .OfType<Panel>()        // body
                    .SelectMany(p => p.Controls.OfType<Label>())
                    .FirstOrDefault();

                if (lbl != null)
                {
                    lbl.Text = mensaje;
                    toastProceso.BringToFront();
                }
            }));
        }
        catch { }
    }

    public static void CerrarProceso(Form destino)
    {
        if (toastProceso == null || toastProceso.IsDisposed) return;

        try
        {
            destino.Invoke(new Action(() =>
            {
                if (_handlerCierreBloqueado != null && _formBloqueado != null)
                {
                    _formBloqueado.FormClosing -= _handlerCierreBloqueado;
                    _handlerCierreBloqueado = null;
                    _formBloqueado = null;
                }

                toastProceso.Dispose();
                toastProceso = null;
                DesbloquearUI(destino);
            }));
        }
        catch { }
    }

    /// <param name="alpha">
    /// Intensidad del velo gris (0-255). El de "proceso" va oscuro a propósito: ahí el
    /// cajero NO puede hacer nada y conviene que se note. El de "confirmacion" va más
    /// claro porque suele necesitar leer lo que hay detrás para decidir.
    /// </param>
    private static void BloquearUI(Form destino, int alpha = 120)
    {
        if (overlayBloqueo != null && !overlayBloqueo.IsDisposed)
            return;

        // WinForms NO permite que un control vea a sus hermanos de abajo: un Panel con
        // BackColor semitransparente solo mezcla contra SU PROPIO fondo, y sale un gris
        // plano por más que se baje el alpha. El truco estándar es fotografiar el form
        // ANTES de taparlo y pintar esa foto dentro del overlay; el velo negro va encima
        // de la foto, y ahí sí se ve lo de atrás atenuado.
        //
        // La foto queda congelada, pero da igual: mientras el overlay está puesto la UI
        // está bloqueada y nada de lo que hay debajo puede cambiar.
        Bitmap fondo = null;
        Point origenCliente = Point.Empty;
        try
        {
            // DrawToBitmap de un Form incluye el área NO-CLIENTE (borde + barra de título)
            // y la dibuja desde el pixel 0,0. Como el overlay vive en coordenadas de
            // cliente, se captura la ventana COMPLETA y se guarda dónde empieza el área
            // cliente dentro de esa foto; el OnPaint la desplaza para que calce.
            Size tam = destino.Size;
            if (tam.Width > 0 && tam.Height > 0)
            {
                fondo = new Bitmap(tam.Width, tam.Height);
                destino.DrawToBitmap(fondo, new Rectangle(Point.Empty, tam));

                Point cliente = destino.PointToScreen(Point.Empty);
                origenCliente = new Point(cliente.X - destino.Left, cliente.Y - destino.Top);
            }
        }
        catch
        {
            // Algún control puede negarse a renderizarse a bitmap. Sin foto el overlay
            // se comporta como antes (gris opaco): feo, pero sigue bloqueando.
            if (fondo != null) { fondo.Dispose(); fondo = null; }
        }

        overlayBloqueo = new OverlayTransparente
        {
            Dock = DockStyle.Fill,
            Alpha = alpha,
            ColorOverlay = Color.Black,
            Fondo = fondo,
            OrigenCliente = origenCliente,
            Tag = "bloqueo"
        };

        // Captura todos los clics
        overlayBloqueo.Click += (s, e) => { };

        destino.Controls.Add(overlayBloqueo);
        overlayBloqueo.BringToFront();
    }

    private static void DesbloquearUI(Form destino)
    {
        if (overlayBloqueo != null && !overlayBloqueo.IsDisposed)
        {
            destino.Controls.Remove(overlayBloqueo);
            overlayBloqueo.Dispose();
            overlayBloqueo = null;
        }
    }

    public class OverlayTransparente : Panel
    {
        public int Alpha { get; set; } = 120;
        public Color ColorOverlay { get; set; } = Color.Black;

        /// <summary>
        /// Foto de lo que había debajo, tomada por BloquearUI antes de tapar el form.
        /// Si va en null el overlay pinta gris opaco (el comportamiento viejo).
        /// </summary>
        public Image Fondo { get; set; }

        /// <summary>
        /// Dónde arranca el área cliente dentro de <see cref="Fondo"/>. Sirve para
        /// descartar el borde y la barra de título que DrawToBitmap incluye siempre.
        /// </summary>
        public Point OrigenCliente { get; set; }

        public OverlayTransparente()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);

            this.UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Primero la foto, después el velo: al revés el velo quedaría debajo y no
            // atenuaría nada.
            if (Fondo != null)
                e.Graphics.DrawImageUnscaled(Fondo, -OrigenCliente.X, -OrigenCliente.Y);

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(Alpha, ColorOverlay)))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Fondo != null)
            {
                Fondo.Dispose();
                Fondo = null;
            }

            base.Dispose(disposing);
        }
    }




}
