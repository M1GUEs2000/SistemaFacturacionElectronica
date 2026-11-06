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
        private readonly CasilleroParametrosManejador _parametros;
        private readonly Dictionary<int, CasilleroCardControl> _tarjetas = new Dictionary<int, CasilleroCardControl>();
        private Timer _timerRefresco;

        // Cada tarjeta EnAlerta corre su PROPIO reloj de 5s (ver CasilleroCardControl.
        // LeTocaEntrarEnCola, basado en AlertaDesde de la base de datos) - no hay turno
        // compartido ni indice rotativo. Este timer hace de "pulso": cada ~200ms junta en
        // _colaPendienteSonido los casilleros cuyo propio reloj ya cumplio, y tambien
        // sirve para refrescar el destello visual y el tiempo restante en vivo (sin
        // consultar la base de datos), en vez de saltar de golpe cada 5s junto con
        // CargarTablero().
        private Timer _timerAnimacion;

        // Con muchos casilleros activos, cada uno con su propio reloj, sonar uno por uno
        // se vuelve ruido insoportable (con 100 activos, en promedio suena ~1 cada 250ms).
        // Por eso el sonido tiene un piso minimo entre reproducciones: los que van
        // "cumpliendo turno" mientras tanto se acumulan aca sin sonar, y cuando se cumple
        // el piso se disparan TODOS juntos - un solo sonido, y esos bordes destellan a la
        // vez (ver PulsoAnimacionYAlertas). Ningun casillero se salta ni se prioriza sobre
        // otro, solo se agrupan los que quedaron pendientes en la ventana de espera.
        private readonly HashSet<int> _colaPendienteSonido = new HashSet<int>();
        private DateTime _ultimoSonido = DateTime.MinValue;
        private const double IntervaloMinimoSonidoSegundos = 2.5;

        // El sonido configurado casi nunca cambia (solo cuando el usuario lo edita en
        // Configuracion), asi que se cachea en memoria en vez de ir a la base de datos en
        // cada ciclo de 5s del hilo de la interfaz. Se refresca al abrir la pantalla y al
        // cerrar el dialogo de configuracion (ver btnConfiguracion_Click).
        private string _sonidoNotificacionCacheado;

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
            _parametros = new CasilleroParametrosManejador(services.Conexion, services.Log);

            HabilitarDobleBuffer(this);
            HabilitarDobleBuffer(pnlTablero);
            HabilitarDobleBuffer(pnlResumen);

            this.Load += FrmCasilleroPrincipal_Load;
            this.FormClosed += (s, e) =>
            {
                _timerRefresco?.Stop();
                _timerAnimacion?.Stop();
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
            RefrescarSonidoCacheado();

            _timerRefresco = new Timer { Interval = 5000 };
            _timerRefresco.Tick += (s, e2) => CargarTablero();
            _timerRefresco.Start();

            _timerAnimacion = new Timer { Interval = 200 };
            _timerAnimacion.Tick += (s, e2) => PulsoAnimacionYAlertas();
            _timerAnimacion.Start();
        }

        private void PulsoAnimacionYAlertas()
        {
            foreach (var tarjeta in _tarjetas.Values)
            {
                if (!tarjeta.EsDisponible)
                    tarjeta.Invalidate();

                // Se llama siempre (no solo con foco) para que el reloj de cada tarjeta no
                // se atrase. Solo acumula en la cola - el destello y el sonido se disparan
                // juntos, en lote, mas abajo.
                if (tarjeta.LeTocaEntrarEnCola())
                    _colaPendienteSonido.Add(tarjeta.IdCasillero);
            }

            if (_colaPendienteSonido.Count == 0)
                return;

            if ((DateTime.Now - _ultimoSonido).TotalSeconds < IntervaloMinimoSonidoSegundos)
                return;

            foreach (int idCasillero in _colaPendienteSonido)
                if (_tarjetas.TryGetValue(idCasillero, out var tarjeta))
                    tarjeta.Destellar();

            _colaPendienteSonido.Clear();
            _ultimoSonido = DateTime.Now;

            // Solo mientras la ventana de casilleros esta enfocada (no si esta minimizada,
            // detras de otra ventana, o con un dialogo modal de detalle abierto encima).
            if (!ContainsFocus)
                return;

            try
            {
                SonidosCasilleros.Reproducir(_sonidoNotificacionCacheado);
            }
            catch (Exception ex)
            {
                _services.Log.CrearLog("CASILLEROS", UsuarioActual, IPActual, "ERROR reproduciendo sonido de alerta: " + ex);
            }
        }

        private void RefrescarSonidoCacheado()
        {
            try
            {
                _sonidoNotificacionCacheado = _parametros.ObtenerSonidoNotificacion();
            }
            catch (Exception ex)
            {
                _services.Log.CrearLog("CASILLEROS", UsuarioActual, IPActual, "ERROR obteniendo sonido de notificacion: " + ex);
            }
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
                DateTime? alertaDesde = fila.Table.Columns.Contains("ALERTADESDE") && fila["ALERTADESDE"] != DBNull.Value
                    ? Convert.ToDateTime(fila["ALERTADESDE"]) : (DateTime?)null;

                idsVistos.Add(id);

                if (_tarjetas.TryGetValue(id, out var card))
                {
                    card.Actualizar(estado, idAsignacion, cliente, fechaInicio, fechaFin, minutosSolicitados, notificado, alertaDesde);
                }
                else
                {
                    card = new CasilleroCardControl(id, codigo, estado, idAsignacion, cliente, fechaInicio, fechaFin, minutosSolicitados, notificado, alertaDesde);
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

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            using (var frm = new frmHistorialCasilleros(_services))
            {
                frm.UsuarioActual = UsuarioActual;
                frm.IPActual = IPActual;
                frm.ShowDialog(this);
            }
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
            RefrescarSonidoCacheado();
        }
    }
}
