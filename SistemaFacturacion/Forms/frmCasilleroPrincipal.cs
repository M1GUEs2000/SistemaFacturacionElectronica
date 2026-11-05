using LogicaNegocios.Casilleros;
using LogicaNegocios.Services;
using SistemaFacturacion.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmCasilleroPrincipal : Form
    {
        private static frmCasilleroPrincipal _instanciaAbierta;

        private readonly AppServices _services;
        private readonly CasilleroManejador _casilleros;
        private readonly Dictionary<int, CasilleroCardControl> _tarjetas = new Dictionary<int, CasilleroCardControl>();
        private Timer _timerRefresco;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        public static void AbrirOEnfocar(AppServices services, string usuario, string ip)
        {
            if (_instanciaAbierta != null && !_instanciaAbierta.IsDisposed)
            {
                if (_instanciaAbierta.WindowState == FormWindowState.Minimized)
                    _instanciaAbierta.WindowState = FormWindowState.Normal;
                _instanciaAbierta.Activate();
                return;
            }

            var frm = new frmCasilleroPrincipal(services) { UsuarioActual = usuario, IPActual = ip };
            _instanciaAbierta = frm;
            frm.Show();
        }

        public frmCasilleroPrincipal(AppServices services)
        {
            InitializeComponent();

            _services = services;
            _casilleros = new CasilleroManejador(services.Conexion, services.Log);

            HabilitarDobleBuffer(this);
            HabilitarDobleBuffer(pnlTablero);
            HabilitarDobleBuffer(pnlResumen);

            this.Load += FrmCasilleroPrincipal_Load;
            this.FormClosed += (s, e) =>
            {
                _timerRefresco?.Stop();
                if (_instanciaAbierta == this)
                    _instanciaAbierta = null;
            };

            pnlTablero.Resize += (s, e) => RecalcularAnchoTarjetas();
            ConfigurarCajaFiltro();

            VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
            this.Resize += (s, e) => VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
        }

        private void ConfigurarCajaFiltro()
        {
            void ActualizarRegion()
            {
                if (pnlCajaFiltro.Width <= 0 || pnlCajaFiltro.Height <= 0) return;
                using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, pnlCajaFiltro.Width, pnlCajaFiltro.Height), 8))
                    pnlCajaFiltro.Region = new Region(ruta);
            }

            ActualizarRegion();
            pnlCajaFiltro.Resize += (s, e) => ActualizarRegion();
            pnlCajaFiltro.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool activo = txtFiltro.Focused;
                using (var lapiz = new Pen(activo ? Color.FromArgb(0, 123, 255) : Color.FromArgb(210, 213, 217), activo ? 1.6f : 1f))
                    g.DrawPath(lapiz, ConstruirRedondeado(new Rectangle(0, 0, pnlCajaFiltro.Width - 1, pnlCajaFiltro.Height - 1), 8));

                using (var fuenteIcono = IconosCasilleros.CrearFuente(12F))
                using (var brochaIcono = new SolidBrush(Color.FromArgb(148, 155, 162)))
                using (var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(IconosCasilleros.Glifo(IconosCasilleros.Buscar), fuenteIcono, brochaIcono,
                        new RectangleF(0, 0, 34, pnlCajaFiltro.Height), formato);
            };
            txtFiltro.Enter += (s, e) => pnlCajaFiltro.Invalidate();
            txtFiltro.Leave += (s, e) => pnlCajaFiltro.Invalidate();

            txtFiltro.TextChanged += (s, e) => AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string filtro = txtFiltro.Text.Trim();

            foreach (var tarjeta in _tarjetas.Values)
            {
                bool visible = string.IsNullOrEmpty(filtro)
                    || (tarjeta.NombreCliente ?? "").IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0;
                tarjeta.Visible = visible;
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

        private static void HabilitarDobleBuffer(Control control)
        {
            typeof(Control).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
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
            if (VentanaPersonalizada.TryObtenerHitTest(this, m, 44, permitirRedimension: true, resultado: out var resultado))
            {
                m.Result = resultado;
                return;
            }

            base.WndProc(ref m);
        }

        private void btnCerrarVentana_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMinimizarVentana_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void FrmCasilleroPrincipal_Load(object sender, EventArgs e)
        {
            try
            {
                CasilleroSchemaInstalador.AsegurarTablas(_services.Conexion, _services.Log);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "No se pudo preparar la base de datos de casilleros:\n\n" + ex.Message,
                    "Casilleros", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CargarTablero();

            _timerRefresco = new Timer { Interval = 5000 };
            _timerRefresco.Tick += (s, e2) => CargarTablero();
            _timerRefresco.Start();
        }

        private void CargarTablero()
        {
            DataSet ds = _casilleros.ConsultarConAsignacion();

            int disponibles = 0, ocupados = 0, porVencer = 0, vencidos = 0;
            var idsVistos = new HashSet<int>();

            foreach (DataRow fila in ds.Tables[0].Rows)
            {
                int id = Convert.ToInt32(fila["ID"]);
                string codigo = fila["CODIGO"].ToString();
                string estado = fila["ESTADO"].ToString();
                int? idAsignacion = fila["IDASIGNACION"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["IDASIGNACION"]);
                string cliente = fila["NOMBRECLIENTE"] == DBNull.Value ? null : fila["NOMBRECLIENTE"].ToString();
                DateTime? fechaInicio = fila["FECHAINICIO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHAINICIO"]);
                DateTime? fechaFin = fila["FECHAFIN"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(fila["FECHAFIN"]);
                int? minutosSolicitados = fila["MINUTOSSOLICITADOS"] == DBNull.Value ? (int?)null : Convert.ToInt32(fila["MINUTOSSOLICITADOS"]);
                bool notificado = fila["NOTIFICADO"] != DBNull.Value && Convert.ToBoolean(fila["NOTIFICADO"]);

                idsVistos.Add(id);

                if (_tarjetas.TryGetValue(id, out var card))
                {
                    card.Actualizar(estado, idAsignacion, cliente, fechaInicio, fechaFin, minutosSolicitados, notificado);
                }
                else
                {
                    card = new CasilleroCardControl(id, codigo, estado, idAsignacion, cliente, fechaInicio, fechaFin, minutosSolicitados, notificado);
                    card.Click += Card_Click;
                    _tarjetas[id] = card;
                    pnlTablero.Controls.Add(card);
                }

                if (estado == "DISPONIBLE") disponibles++;
                else if (estado == "VENCIDO") vencidos++;
                else if (notificado) porVencer++;
                else ocupados++;
            }

            foreach (int idViejo in _tarjetas.Keys.Where(id => !idsVistos.Contains(id)).ToList())
            {
                pnlTablero.Controls.Remove(_tarjetas[idViejo]);
                _tarjetas[idViejo].Dispose();
                _tarjetas.Remove(idViejo);
            }

            ActualizarResumen(disponibles, ocupados, porVencer, vencidos);
            RecalcularAnchoTarjetas();
            AplicarFiltro();
        }

        private void RecalcularAnchoTarjetas()
        {
            if (_tarjetas.Count == 0) return;

            const int anchoObjetivo = 184;
            const int anchoMinimo = 150;
            const int margenPorLado = 8;

            int anchoDisponible = pnlTablero.ClientSize.Width - pnlTablero.Padding.Horizontal
                - SystemInformation.VerticalScrollBarWidth;
            if (anchoDisponible <= 0) return;

            int anchoConMargen = anchoObjetivo + margenPorLado * 2;
            int columnas = Math.Max(1, anchoDisponible / anchoConMargen);
            int anchoTarjeta = Math.Max(anchoMinimo, anchoDisponible / columnas - margenPorLado * 2);

            pnlTablero.SuspendLayout();
            foreach (var tarjeta in _tarjetas.Values)
                tarjeta.Width = anchoTarjeta;
            pnlTablero.ResumeLayout();
        }

        private void ActualizarResumen(int disponibles, int ocupados, int porVencer, int vencidos)
        {
            pnlResumen.SuspendLayout();
            pnlResumen.Controls.Clear();
            pnlResumen.Controls.Add(CrearBadge("● Disponibles: " + disponibles, Color.FromArgb(40, 167, 69)));
            pnlResumen.Controls.Add(CrearBadge("● Ocupados: " + ocupados, Color.FromArgb(0, 123, 255)));
            pnlResumen.Controls.Add(CrearBadge("● Por vencer: " + porVencer, Color.FromArgb(255, 147, 41)));
            pnlResumen.Controls.Add(CrearBadge("● Vencidos: " + vencidos, Color.FromArgb(220, 53, 69)));
            pnlResumen.ResumeLayout();
        }

        private static Label CrearBadge(string texto, Color color)
        {
            return new Label
            {
                Text = texto,
                ForeColor = color,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 4, 28, 0)
            };
        }

        private void Card_Click(object sender, EventArgs e)
        {
            var card = (CasilleroCardControl)sender;

            if (card.EsDisponible)
            {
                AbrirAsignar(card.IdCasillero, card.Codigo);
            }
            else
            {
                using (var frm = new frmDetalleCasillero(
                    _services, card.IdAsignacion.Value, card.Codigo, card.NombreCliente,
                    card.FechaInicio, card.FechaFin, card.MinutosSolicitados,
                    card.EsVencido, card.Notificado, UsuarioActual, IPActual))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                        CargarTablero();
                }
            }
        }

        private void AbrirAsignar(int? idCasillero = null, string codigo = null)
        {
            using (var frm = new frmAsignarCasillero(_services, UsuarioActual, IPActual, idCasillero, codigo))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    Notificaciones.Show(this, "Casillero " + frm.Resultado.CodigoCasillero + " asignado.", "exito");
                    CargarTablero();
                }
            }
        }

        private void btnAsignar_Click(object sender, EventArgs e)
        {
            AbrirAsignar();
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarTablero();
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            using (var frm = new frmCasilleroConfig(_services))
            {
                frm.UsuarioActual = UsuarioActual;
                frm.IPActual = IPActual;
                frm.ShowDialog(this);
            }
            CargarTablero();
        }
    }
}
