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
    // Consulta del historial de casilleros con filtros, totales por tiempo y envío del reporte por correo.
    public class frmHistorialCasilleros : Form
    {
        private const string FormatoFecha = "dd/MM/yyyy";
        private const string FormatoFechaHora = "dd/MM/yyyy HH:mm";

        private static readonly Color ColorAccento = Color.FromArgb(0, 123, 255);
        private static readonly Color ColorVerde = Color.FromArgb(40, 167, 69);
        private static readonly Color ColorNaranja = Color.FromArgb(255, 153, 0);
        private static readonly Color ColorFondo = Color.FromArgb(245, 247, 250);
        private static readonly Color ColorBorde = Color.FromArgb(210, 213, 217);
        private static readonly Color ColorTexto = Color.FromArgb(33, 37, 41);
        private static readonly Color ColorTextoSuave = Color.FromArgb(108, 117, 125);
        private static readonly Color ColorSubtotal = Color.FromArgb(232, 241, 252);
        private static readonly Color ColorTotal = Color.FromArgb(0, 123, 255);

        private readonly CasilleroHistorialManejador _historial;
        private readonly CasilleroReporteServicio _reporte;

        private readonly DateTimePicker dtpDesde = new DateTimePicker();
        private readonly DateTimePicker dtpHasta = new DateTimePicker();
        private readonly ComboBox cmbPersona = new ComboBox();
        private readonly ComboBox cmbTiempo = new ComboBox();
        private IconButton btnConsultar;
        private IconButton btnEnviar;
        private readonly DataGridView gridDetalle = new DataGridView();
        private readonly DataGridView gridConsolidado = new DataGridView();
        private readonly FlowLayoutPanel pnlKpi = new FlowLayoutPanel();
        private readonly Label lblTotalConsolidado = new Label();
        private readonly Label lblTotalDetalle = new Label();

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
            ClientSize = new Size(1040, 680);
            MinimumSize = new Size(940, 600);
            Font = new Font("Segoe UI", 9.5F);
            Text = "Historial de casilleros";
            DoubleBuffered = true;

            // ---- Cabecera
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

            // ---- Pie: envío por correo
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 68, BackColor = Color.White };
            footer.Paint += (s, e) =>
            {
                using (var lapiz = new Pen(Color.FromArgb(230, 232, 235)))
                    e.Graphics.DrawLine(lapiz, 0, 0, footer.Width, 0);
            };
            var lblCorreo = new Label
            {
                Text = "El reporte se envía al correo registrado de la empresa.",
                AutoSize = true, ForeColor = ColorTextoSuave, Location = new Point(220, 24)
            };
            btnEnviar = CrearBoton("Enviar por correo", 0xf0e0, ColorVerde, new Rectangle(24, 18, 180, 34));
            btnEnviar.Enabled = false;
            btnEnviar.Click += async (s, e) => await EnviarAsync();
            var btnCerrar = CrearBoton("Cerrar", IconosCasilleros.Cerrar, Color.FromArgb(108, 117, 125), new Rectangle(0, 18, 120, 34));
            btnCerrar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrar.Click += (s, e) => Close();
            footer.Controls.Add(lblCorreo);
            footer.Controls.Add(btnEnviar);
            footer.Controls.Add(btnCerrar);
            footer.Resize += (s, e) => btnCerrar.Left = footer.Width - btnCerrar.Width - 24;
            btnCerrar.Left = ClientSize.Width - 120 - 24;

            // ---- Filtros
            var barraFiltros = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Color.White };
            barraFiltros.Paint += (s, e) =>
            {
                using (var lapiz = new Pen(Color.FromArgb(230, 232, 235)))
                    e.Graphics.DrawLine(lapiz, 0, barraFiltros.Height - 1, barraFiltros.Width, barraFiltros.Height - 1);
            };
            ConfigurarDtp(dtpDesde);
            ConfigurarDtp(dtpHasta);
            cmbPersona.DropDownStyle = cmbTiempo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPersona.FlatStyle = cmbTiempo.FlatStyle = FlatStyle.Flat;
            AgregarCampo(barraFiltros, "Desde", dtpDesde, 24, 150);
            AgregarCampo(barraFiltros, "Hasta", dtpHasta, 190, 150);
            AgregarCampo(barraFiltros, "Persona", cmbPersona, 356, 260);
            AgregarCampo(barraFiltros, "Tiempo", cmbTiempo, 632, 140);
            btnConsultar = CrearBoton("Consultar", IconosCasilleros.Buscar, ColorAccento, new Rectangle(0, 44, 150, 36));
            btnConsultar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConsultar.Click += (s, e) => Consultar();
            barraFiltros.Controls.Add(btnConsultar);
            barraFiltros.Resize += (s, e) => btnConsultar.Left = barraFiltros.Width - btnConsultar.Width - 24;
            btnConsultar.Left = ClientSize.Width - 150 - 24;

            // ---- Tarjetas de resumen (KPI)
            pnlKpi.Dock = DockStyle.Top;
            pnlKpi.Height = 88;
            pnlKpi.WrapContents = false;
            pnlKpi.AutoScroll = false;
            pnlKpi.BackColor = ColorFondo;
            pnlKpi.Padding = new Padding(0, 12, 0, 8);

            // ---- Consolidado (izquierda) y detalle (derecha) en tarjetas blancas
            ConfigurarGrid(gridConsolidado);
            // MINUTOS y SUBTOTAL son numéricos (ordenan bien); el texto se arma en CellFormatting.
            gridConsolidado.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MINUTOS", Name = "colTiempo", HeaderText = "Tiempo", FillWeight = 36 });
            gridConsolidado.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CANTIDAD", HeaderText = "Cantidad", FillWeight = 26,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            gridConsolidado.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SUBTOTAL", Name = "colSubtotal", HeaderText = "Subtotal", FillWeight = 38,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            gridConsolidado.Columns[1].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridConsolidado.Columns[2].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            ConfigurarGrid(gridDetalle);
            gridDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FECHA", HeaderText = "Fecha", FillWeight = 24,
                DefaultCellStyle = { Format = FormatoFechaHora }
            });
            gridDetalle.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CASILLERO", HeaderText = "Casillero", FillWeight = 14 });
            gridDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TIEMPO", HeaderText = "Tiempo", FillWeight = 14,
                DefaultCellStyle = { Format = "0' min'" }
            });
            gridDetalle.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PERSONA", HeaderText = "Persona", FillWeight = 32 });
            gridDetalle.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ESTADO", HeaderText = "Estado", FillWeight = 16 });

            var tarjetaConsolidado = CrearTarjeta("Consolidado por tiempo", gridConsolidado, lblTotalConsolidado);
            var tarjetaDetalle = CrearTarjeta("Historial", gridDetalle, lblTotalDetalle);
            ConfigurarBarraTotal(lblTotalConsolidado, "Total");
            ConfigurarBarraTotal(lblTotalDetalle, "Total de la consulta");

            var contenido = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = ColorFondo, Margin = Padding.Empty
            };
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tarjetaConsolidado.Dock = tarjetaDetalle.Dock = DockStyle.Fill;
            tarjetaConsolidado.Margin = new Padding(0, 0, 12, 0);
            tarjetaDetalle.Margin = Padding.Empty;
            contenido.Controls.Add(tarjetaConsolidado, 0, 0);
            contenido.Controls.Add(tarjetaDetalle, 1, 0);

            var cuerpo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 0, 24, 14), BackColor = ColorFondo };
            cuerpo.Controls.Add(contenido);
            cuerpo.Controls.Add(pnlKpi);

            Controls.Add(cuerpo);
            Controls.Add(footer);
            Controls.Add(barraFiltros);
            Controls.Add(header);

            gridConsolidado.CellFormatting += GridConsolidado_CellFormatting;
            MostrarKpi(0, 0, 0);

            ResumeLayout(false);
        }

        private static void ConfigurarDtp(DateTimePicker dtp)
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = FormatoFecha;
            dtp.Font = new Font("Segoe UI", 10.5F);
        }

        private void AgregarCampo(Panel padre, string etiqueta, Control control, int x, int ancho)
        {
            padre.Controls.Add(new Label
            {
                Text = etiqueta, AutoSize = true, ForeColor = ColorTextoSuave, Location = new Point(x, 14)
            });
            padre.Controls.Add(CrearCaja(control, new Rectangle(x, 44, ancho, 36)));
        }

        // Caja con borde redondeado y foco azul, igual que los campos de Configuración.
        private Panel CrearCaja(Control control, Rectangle bounds)
        {
            var panel = new Panel { BackColor = Color.White, Bounds = bounds, Padding = new Padding(10, 7, 10, 4) };

            if (control is TextBox tb)
            {
                tb.BorderStyle = BorderStyle.None;
                tb.Font = new Font("Segoe UI", 10.5F);
            }
            if (control is ComboBox cb)
                cb.Font = new Font("Segoe UI", 10.5F);

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
            return new IconButton
            {
                Text = texto, CodigoIcono = icono, BackColor = fondo, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), RadioEsquinas = 8, Bounds = bounds
            };
        }

        // Tarjeta blanca redondeada con título y un control que ocupa el resto.
        // Barra azul fija (fuera de la tabla) para que el ordenamiento nunca mezcle el total.
        private static void ConfigurarBarraTotal(Label barra, string prefijo)
        {
            barra.Tag = prefijo;
            barra.Dock = DockStyle.Bottom;
            barra.Height = 40;
            barra.BackColor = ColorTotal;
            barra.ForeColor = Color.White;
            barra.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            barra.TextAlign = ContentAlignment.MiddleLeft;
            barra.Padding = new Padding(14, 0, 14, 0);
            barra.Text = prefijo;
        }

        private static Panel CrearTarjeta(string titulo, Control contenido, Control pie)
        {
            var tarjeta = new Panel { BackColor = Color.White, Padding = new Padding(1, 46, 1, 1) };
            var lbl = new Label
            {
                Text = titulo, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = ColorTexto,
                Location = new Point(18, 12), AutoSize = true, BackColor = Color.Transparent
            };
            contenido.Dock = DockStyle.Fill;
            tarjeta.Controls.Add(contenido);
            tarjeta.Controls.Add(pie);
            tarjeta.Controls.Add(lbl);

            void ActualizarRegion()
            {
                if (tarjeta.Width <= 0 || tarjeta.Height <= 0) return;
                using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, tarjeta.Width, tarjeta.Height), 12))
                    tarjeta.Region = new Region(ruta);
            }
            tarjeta.Resize += (s, e) => ActualizarRegion();
            tarjeta.HandleCreated += (s, e) => ActualizarRegion();
            return tarjeta;
        }

        private static void ConfigurarGrid(DataGridView g)
        {
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.AllowUserToResizeColumns = false;
            g.RowHeadersVisible = false;
            g.AutoGenerateColumns = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(238, 240, 243);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.RowTemplate.Height = 34;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 248, 250);
            g.ColumnHeadersDefaultCellStyle.ForeColor = ColorTextoSuave;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 248, 250);
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorTextoSuave;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            g.DefaultCellStyle.ForeColor = ColorTexto;
            g.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 237, 255);
            g.DefaultCellStyle.SelectionForeColor = ColorTexto;
        }

        // Muestra el tiempo y el subtotal del consolidado como texto ("1 hora", "120 minutos")
        // sin perder el valor numérico, para que el ordenamiento sea correcto.
        private void GridConsolidado_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null || e.Value == DBNull.Value) return;
            string columna = gridConsolidado.Columns[e.ColumnIndex].Name;
            if (columna == "colTiempo")
            {
                e.Value = CasilleroHistorialManejador.TextoTiempo(Convert.ToInt32(e.Value));
                e.FormattingApplied = true;
            }
            else if (columna == "colSubtotal")
            {
                e.Value = CasilleroHistorialManejador.TextoMinutos(Convert.ToInt32(e.Value));
                e.FormattingApplied = true;
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

        // Tarjeta KPI: etiqueta pequeña + número grande, con franja de color.
        private static Control CrearKpi(string titulo, string valor, Color acento)
        {
            var panel = new Panel { Size = new Size(210, 64), BackColor = Color.White, Margin = new Padding(0, 0, 12, 0) };
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brocha = new SolidBrush(acento))
                using (var ruta = ConstruirRedondeado(new Rectangle(0, 16, 5, panel.Height - 32), 2))
                    e.Graphics.FillPath(brocha, ruta);
            };
            using (var ruta = ConstruirRedondeado(new Rectangle(0, 0, panel.Width, panel.Height), 12))
                panel.Region = new Region(ruta);

            panel.Controls.Add(new Label
            {
                Text = titulo, Font = new Font("Segoe UI", 9F), ForeColor = ColorTextoSuave,
                Location = new Point(18, 8), AutoSize = true
            });
            panel.Controls.Add(new Label
            {
                Text = valor, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = ColorTexto,
                Location = new Point(16, 24), AutoSize = true
            });
            return panel;
        }

        private void MostrarKpi(int total, int tiempos, int personas)
        {
            pnlKpi.SuspendLayout();
            pnlKpi.Controls.Clear();
            pnlKpi.Controls.Add(CrearKpi("Total asignaciones", total.ToString(), ColorAccento));
            pnlKpi.Controls.Add(CrearKpi("Tiempos distintos", tiempos.ToString(), ColorVerde));
            pnlKpi.Controls.Add(CrearKpi("Personas", personas.ToString(), ColorNaranja));
            pnlKpi.ResumeLayout();
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

        private static DataTable ConstruirTablaConsolidado(List<FilaConsolidado> consolidado)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("MINUTOS", typeof(int));
            tabla.Columns.Add("CANTIDAD", typeof(int));
            tabla.Columns.Add("SUBTOTAL", typeof(int));
            foreach (var f in consolidado)
                tabla.Rows.Add(f.Minutos, f.Cantidad, f.SubtotalMinutos);
            return tabla;
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

                int personas = detalle.AsEnumerable().Select(r => Convert.ToString(r["PERSONA"])).Distinct().Count();
                MostrarKpi(detalle.Rows.Count, consolidado.Count, personas);

                gridConsolidado.DataSource = ConstruirTablaConsolidado(consolidado);
                gridDetalle.DataSource = detalle;

                int totalUsos = consolidado.Sum(f => f.Cantidad);
                int totalMinutos = consolidado.Sum(f => f.SubtotalMinutos);
                string totalTexto = CasilleroHistorialManejador.TextoUsos(totalUsos) + "   ·   "
                                    + CasilleroHistorialManejador.TextoMinutos(totalMinutos);
                lblTotalConsolidado.Text = "Total   " + totalTexto;
                lblTotalDetalle.Text = "Total de la consulta   " + totalTexto;
                btnEnviar.Enabled = detalle.Rows.Count > 0;

                if (detalle.Rows.Count == 0)
                    MessageBox.Show(this, "No hay asignaciones en el rango y filtros seleccionados.", "Historial de casilleros",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            string destino;
            try
            {
                destino = _reporte.ObtenerCorreoEmpresa(UsuarioActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo leer el correo de la empresa: " + ex.Message, "Historial de casilleros",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(destino))
            {
                MessageBox.Show(this, "La empresa no tiene un correo registrado. Regístrelo en los datos de la empresa.",
                    "Historial de casilleros", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmar = MessageBox.Show(this,
                "Se enviará el reporte de casilleros del " + _filtros.Desde.ToString(FormatoFecha)
                + " al " + _filtros.Hasta.ToString(FormatoFecha) + " al correo:\n\n" + destino + "\n\n¿Desea continuar?",
                "Confirmar envío", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes) return;

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
