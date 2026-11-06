using LogicaNegocios.Casilleros;
using LogicaNegocios.Services;
using SistemaFacturacion.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    // Consulta del historial de casilleros con filtros y envío del reporte por correo.
    public class frmHistorialCasilleros : Form
    {
        private const string FormatoFecha = "dd/MM/yyyy";
        private const string FormatoFechaHora = "dd/MM/yyyy HH:mm";

        private static readonly Color ColorAccento = Color.FromArgb(0, 123, 255);
        private static readonly Color ColorFondo = Color.FromArgb(245, 247, 250);
        private static readonly Color ColorBorde = Color.FromArgb(210, 213, 217);
        private static readonly Color ColorTextoSuave = Color.FromArgb(108, 117, 125);

        private readonly CasilleroHistorialManejador _historial;
        private readonly CasilleroReporteServicio _reporte;

        private readonly DateTimePicker dtpDesde = new DateTimePicker();
        private readonly DateTimePicker dtpHasta = new DateTimePicker();
        private readonly ComboBox cmbPersona = new ComboBox();
        private readonly ComboBox cmbTiempo = new ComboBox();
        private readonly TextBox txtCorreo = new TextBox();
        private IconButton btnConsultar;
        private IconButton btnEnviar;
        private readonly DataGridView gridDetalle = new DataGridView();
        private readonly FlowLayoutPanel pnlConsolidado = new FlowLayoutPanel();
        private readonly Label lblResumen = new Label();

        private DataTable _detalle;
        private List<FilaConsolidado> _consolidado;
        private FiltrosReporteCasilleros _filtros;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        public frmHistorialCasilleros(AppServices services)
        {
            _historial = new CasilleroHistorialManejador(services.Conexion, services.Log);
            _reporte = new CasilleroReporteServicio(services);

            ConstruirInterfaz();
            CargarFiltros();

            VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
            Resize += (s, e) => VentanaPersonalizada.AplicarEsquinasRedondeadas(this, 12);
        }

        // ---------------------------------------------------------------- interfaz

        private void ConstruirInterfaz()
        {
            SuspendLayout();

            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ColorFondo;
            ClientSize = new Size(960, 640);
            MinimumSize = new Size(880, 560);
            Font = new Font("Segoe UI", 9.5F);
            Text = "Historial de casilleros";
            DoubleBuffered = true;

            // Cabecera
            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = ColorAccento };
            var btnCerrarVentana = new IconButton
            {
                Dock = DockStyle.Right, Width = 46, BackColor = ColorAccento, ForeColor = Color.White,
                CodigoIcono = IconosCasilleros.Cerrar, RadioEsquinas = 0,
                ColorHoverPersonalizado = Color.FromArgb(220, 53, 69),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), UseVisualStyleBackColor = false
            };
            btnCerrarVentana.FlatAppearance.BorderSize = 0;
            btnCerrarVentana.Click += (s, e) => Close();
            var lblIcono = new Label
            {
                Dock = DockStyle.Left, Width = 52, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter,
                Font = IconosCasilleros.CrearFuente(18F), Text = IconosCasilleros.Glifo(IconosCasilleros.Reloj),
                UseCompatibleTextRendering = true
            };
            var lblTitulo = new Label
            {
                Dock = DockStyle.Fill, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold), Text = "Historial de casilleros"
            };
            header.Controls.Add(lblTitulo);
            header.Controls.Add(lblIcono);
            header.Controls.Add(btnCerrarVentana);

            // Pie con envío por correo
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.FromArgb(247, 248, 249) };
            var lblCorreo = new Label
            {
                Text = "Enviar reporte a", AutoSize = true, ForeColor = ColorTextoSuave, Location = new Point(24, 8)
            };
            var cajaCorreo = CrearCaja(txtCorreo, new Rectangle(24, 26, 340, 30));
            btnEnviar = CrearBoton("Enviar por correo", 0xf0e0, Color.FromArgb(40, 167, 69), new Rectangle(376, 24, 190, 34));
            btnEnviar.Enabled = false;
            btnEnviar.Click += async (s, e) => await EnviarAsync();
            var btnCerrar = CrearBoton("Cerrar", IconosCasilleros.Cerrar, Color.FromArgb(108, 117, 125), new Rectangle(0, 24, 120, 34));
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Left = footer.Width - 120 - 24;
            btnCerrar.Click += (s, e) => Close();
            footer.Controls.Add(lblCorreo);
            footer.Controls.Add(cajaCorreo);
            footer.Controls.Add(btnEnviar);
            footer.Controls.Add(btnCerrar);
            footer.Resize += (s, e) => btnCerrar.Left = footer.Width - btnCerrar.Width - 24;

            // Tarjeta de filtros
            var tarjetaFiltros = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Color.White, Margin = Padding.Empty };
            ConfigurarDtp(dtpDesde);
            ConfigurarDtp(dtpHasta);
            cmbPersona.DropDownStyle = cmbTiempo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPersona.FlatStyle = cmbTiempo.FlatStyle = FlatStyle.Flat;

            AgregarCampo(tarjetaFiltros, "Desde", dtpDesde, 24, 150);
            AgregarCampo(tarjetaFiltros, "Hasta", dtpHasta, 190, 150);
            AgregarCampo(tarjetaFiltros, "Persona", cmbPersona, 356, 250);
            AgregarCampo(tarjetaFiltros, "Tiempo", cmbTiempo, 622, 130);
            btnConsultar = CrearBoton("Consultar", IconosCasilleros.Buscar, ColorAccento, new Rectangle(0, 40, 150, 34));
            btnConsultar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConsultar.Left = tarjetaFiltros.Width - 150 - 24;
            btnConsultar.Click += (s, e) => Consultar();
            tarjetaFiltros.Controls.Add(btnConsultar);
            tarjetaFiltros.Resize += (s, e) => btnConsultar.Left = tarjetaFiltros.Width - btnConsultar.Width - 24;

            // Resumen (consolidado en tarjetas)
            lblResumen.Dock = DockStyle.Top;
            lblResumen.Height = 28;
            lblResumen.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumen.ForeColor = Color.FromArgb(52, 58, 64);
            lblResumen.Text = "Consolidado por tiempo";
            lblResumen.TextAlign = ContentAlignment.BottomLeft;

            pnlConsolidado.Dock = DockStyle.Top;
            pnlConsolidado.Height = 78;
            pnlConsolidado.AutoScroll = true;
            pnlConsolidado.WrapContents = false;
            pnlConsolidado.BackColor = ColorFondo;
            pnlConsolidado.Padding = new Padding(0, 6, 0, 6);

            ConfigurarGrid();
            var lblDetalle = new Label
            {
                Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64), Text = "Detalle", TextAlign = ContentAlignment.BottomLeft
            };

            var cuerpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 0, 24, 8), BackColor = ColorFondo };
            // El orden de Controls.Add con Dock: lo último agregado queda arriba.
            cuerpo.Controls.Add(gridDetalle);
            cuerpo.Controls.Add(lblDetalle);
            cuerpo.Controls.Add(pnlConsolidado);
            cuerpo.Controls.Add(lblResumen);

            Controls.Add(cuerpo);
            Controls.Add(footer);
            Controls.Add(tarjetaFiltros);
            Controls.Add(header);

            ResumeLayout(false);
        }

        private static void ConfigurarDtp(DateTimePicker dtp)
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = FormatoFecha;
            dtp.Font = new Font("Segoe UI", 10F);
        }

        private void AgregarCampo(Panel padre, string etiqueta, Control control, int x, int ancho)
        {
            padre.Controls.Add(new Label
            {
                Text = etiqueta, AutoSize = true, ForeColor = ColorTextoSuave, Location = new Point(x, 14)
            });
            Control caja = CrearCaja(control, new Rectangle(x, 40, ancho, 34));
            padre.Controls.Add(caja);
        }

        // Caja con borde redondeado y foco azul, igual que los campos de Configuración.
        private Panel CrearCaja(Control control, Rectangle bounds)
        {
            var panel = new Panel { BackColor = Color.White, Bounds = bounds, Padding = new Padding(10, 6, 10, 4) };

            if (control is TextBox tb)
            {
                tb.BorderStyle = BorderStyle.None;
                tb.Font = new Font("Segoe UI", 10.5F);
            }
            control.Dock = DockStyle.Fill;
            control.BackColor = Color.White;
            panel.Controls.Add(control);

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
                using (var lapiz = new Pen(activo ? ColorAccento : ColorBorde, activo ? 1.6f : 1f))
                using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 8))
                    e.Graphics.DrawPath(lapiz, ruta);
            };
            control.Enter += (s, e) => panel.Invalidate();
            control.Leave += (s, e) => panel.Invalidate();
            return panel;
        }

        private static IconButton CrearBoton(string texto, int icono, Color fondo, Rectangle bounds)
        {
            var b = new IconButton
            {
                Text = texto, CodigoIcono = icono, BackColor = fondo, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), RadioEsquinas = 8, Bounds = bounds
            };
            return b;
        }

        private void ConfigurarGrid()
        {
            var g = gridDetalle;
            g.Dock = DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;
            g.AutoGenerateColumns = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(235, 237, 240);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.RowTemplate.Height = 32;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(52, 58, 64);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 242, 245);
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 237, 255);
            g.DefaultCellStyle.SelectionForeColor = Color.FromArgb(33, 37, 41);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);

            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FECHA", HeaderText = "Fecha", FillWeight = 24,
                DefaultCellStyle = { Format = FormatoFechaHora }
            });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CASILLERO", HeaderText = "Casillero", FillWeight = 14 });
            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TIEMPO", HeaderText = "Tiempo", FillWeight = 14,
                DefaultCellStyle = { Format = "0' min'" }
            });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PERSONA", HeaderText = "Persona", FillWeight = 32 });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ESTADO", HeaderText = "Estado", FillWeight = 16 });
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

        // Tarjeta pequeña: número grande + "N min".
        private static Control CrearTarjetaConsolidado(string titulo, int cantidad, Color acento)
        {
            var panel = new Panel { Size = new Size(120, 60), BackColor = Color.White, Margin = new Padding(0, 0, 10, 0) };
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var lapiz = new Pen(Color.FromArgb(230, 232, 235)))
                using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 10))
                    e.Graphics.DrawPath(lapiz, ruta);
                using (var brocha = new SolidBrush(acento))
                    e.Graphics.FillRectangle(brocha, 0, 14, 4, panel.Height - 28);
            };
            using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, panel.Width, panel.Height), 10))
                panel.Region = new Region(ruta);

            panel.Controls.Add(new Label
            {
                Text = cantidad.ToString(), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(14, 4), AutoSize = true
            });
            panel.Controls.Add(new Label
            {
                Text = titulo, Font = new Font("Segoe UI", 9F), ForeColor = ColorTextoSuave,
                Location = new Point(16, 36), AutoSize = true
            });
            return panel;
        }

        // ---------------------------------------------------------------- lógica

        private void CargarFiltros()
        {
            dtpDesde.Value = DateTime.Today;
            dtpHasta.Value = DateTime.Today;

            var personas = new List<string> { "Todas" };
            var dsClientes = _historial.ListarClientes();
            if (dsClientes.Tables.Count > 0)
                foreach (DataRow r in dsClientes.Tables[0].Rows)
                    personas.Add(Convert.ToString(r["NOMBRECLIENTE"]));
            cmbPersona.DataSource = personas;

            var tiempos = new List<string> { "Todos" };
            var dsMin = _historial.ListarMinutos();
            if (dsMin.Tables.Count > 0)
                foreach (DataRow r in dsMin.Tables[0].Rows)
                    tiempos.Add(Convert.ToString(r["MINUTOS"]));
            cmbTiempo.DataSource = tiempos;
        }

        private void Consultar()
        {
            try
            {
                var filtros = new FiltrosReporteCasilleros
                {
                    Desde = dtpDesde.Value.Date,
                    Hasta = dtpHasta.Value.Date,
                    Cliente = cmbPersona.SelectedIndex > 0 ? (string)cmbPersona.SelectedItem : null,
                    Minutos = cmbTiempo.SelectedIndex > 0 ? int.Parse((string)cmbTiempo.SelectedItem) : (int?)null
                };

                var detalle = _historial.Consultar(filtros.Desde, filtros.Hasta, filtros.Cliente, filtros.Minutos);
                var consolidado = CasilleroHistorialManejador.Consolidar(detalle);

                _detalle = detalle;
                _consolidado = consolidado;
                _filtros = filtros;

                pnlConsolidado.SuspendLayout();
                pnlConsolidado.Controls.Clear();
                pnlConsolidado.Controls.Add(CrearTarjetaConsolidado("Total", detalle.Rows.Count, Color.FromArgb(40, 167, 69)));
                foreach (var f in consolidado)
                    pnlConsolidado.Controls.Add(CrearTarjetaConsolidado(f.Minutos + " min", f.Cantidad, ColorAccento));
                pnlConsolidado.ResumeLayout();

                gridDetalle.DataSource = detalle;
                lblResumen.Text = "Consolidado por tiempo  ·  " + filtros.Desde.ToString(FormatoFecha)
                                  + " al " + filtros.Hasta.ToString(FormatoFecha);
                btnEnviar.Enabled = detalle.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Historial de casilleros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async System.Threading.Tasks.Task EnviarAsync()
        {
            if (_detalle == null || _detalle.Rows.Count == 0)
            {
                MessageBox.Show(this, "Primero realice una consulta con resultados.", "Historial de casilleros",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string destino = txtCorreo.Text.Trim();
            if (destino.Length == 0)
            {
                MessageBox.Show(this, "Escriba el correo del destinatario.", "Historial de casilleros",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCorreo.Focus();
                return;
            }

            btnEnviar.Enabled = false;
            UseWaitCursor = true;
            try
            {
                var detalle = _detalle; var consolidado = _consolidado; var filtros = _filtros;
                await System.Threading.Tasks.Task.Run(() =>
                    _reporte.EnviarPorCorreo(destino, detalle, consolidado, filtros, UsuarioActual));

                MessageBox.Show(this, "Reporte enviado a " + destino + ".", "Historial de casilleros",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo enviar el correo: " + ex.Message, "Historial de casilleros",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                btnEnviar.Enabled = true;
            }
        }

        // ---------------------------------------------------------------- ventana

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
    }
}
