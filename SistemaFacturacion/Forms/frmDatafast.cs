using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DF_PinPad.Wrapper.Config;
using DF_PinPad.Wrapper.Models;
using DF_PinPad.Wrapper.Services;
using LogicaNegocios;
using LogicaNegocios.Services;

namespace SistemaFacturacion
{
    /// <summary>
    /// Configuración y operación del pinpad Datafast. Tres pestañas:
    ///   • Conexión  — parámetros PinPad.* (appSettings). Se leen/escriben en el
    ///     .exe.config en caliente: al Guardar se llama a AppServices.RecargarPinPad()
    ///     para que el cobro use los nuevos valores sin reiniciar la app.
    ///   • Reinicio  — reconfigura la RED del aparato físico (ConfigurarRedPinPad).
    ///   • Prueba de Tarjeta — lee una tarjeta sin cobrar (LeerTarjeta).
    ///
    /// Las dos últimas usan la conexión YA GUARDADA (_services.PinPad); configure y
    /// guarde primero la pestaña Conexión para poder alcanzar el aparato.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    public class frmDatafast : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        // Claves de appSettings (mismas que lee PinPadSettings / AppServices)
        private const string K_IP = "PinPad.IP";
        private const string K_PUERTO = "PinPad.Puerto";
        private const string K_TIMEOUT = "PinPad.TimeOutMs";
        private const string K_MID = "PinPad.MID";
        private const string K_TID = "PinPad.TID";
        private const string K_CAJA = "PinPad.CajaID";
        private const string K_VERSION = "PinPad.Version";
        private const string K_SHA = "PinPad.SHA";
        private const string K_RED_MASCARA = "PinPad.Red.Mascara";
        private const string K_RED_GATEWAY = "PinPad.Red.Gateway";
        private const string K_RED_HOST_PRINCIPAL = "PinPad.Red.HostPrincipal";
        private const string K_RED_PUERTO_PRINCIPAL = "PinPad.Red.PuertoPrincipal";
        private const string K_RED_HOST_ALTERNO = "PinPad.Red.HostAlterno";
        private const string K_RED_PUERTO_ALTERNO = "PinPad.Red.PuertoAlterno";

        private TextBox _txtIp, _txtMid, _txtTid, _txtCaja;
        private NumericUpDown _numPuerto, _numTimeout;
        private ComboBox _cmbVersion, _cmbSha;
        private Button _btnGuardar, _btnProbar;
        private Label _lblEstado;

        // Reinicio (ConfigurarRedPinPad — reconfigura la RED del aparato físico)
        private TextBox _txtDevIp, _txtDevMascara, _txtDevGateway,
                        _txtDevHostPrin, _txtDevPuertoPrin,
                        _txtDevHostAlt, _txtDevPuertoAlt, _txtDevPuertoEscucha;
        private Button _btnReinicio;
        private Label _lblEstadoReinicio;

        // Prueba de Tarjeta (LeerTarjeta — solo lectura, no cobra)
        private Button _btnLeerTarjeta;
        private Label _lblEstadoTarjeta;
        private TextBox _txtTjNumero, _txtTjBin, _txtTjVence, _txtTjRedCorr, _txtTjRedDif;

        // Consulta de auditoría del pinpad.
        private DateTimePicker _dtpLogDesde, _dtpLogHasta;
        private TextBox _txtLogTarjeta, _txtLogFactura, _txtLogAutorizacion, _txtLogReferencia;
        private ComboBox _cmbLogTipoOperacion;
        private Button _btnConsultarLog;
        private DataGridView _gvLogPinPad;
        private Label _lblLogResultado;

        // Listas de autocompletado del panel de filtros; se registran aquí para poder
        // ocultar las demás cuando se despliega una.
        private readonly List<ListBox> _listasSugerenciasLog = new List<ListBox>();

        /// <summary>
        /// Constructor sin servicios para que el diseñador de Windows Forms pueda
        /// crear y mostrar el formulario sin conectarse a la base ni al PinPad.
        /// </summary>
        public frmDatafast()
        {
            InicializarUI();
        }

        public frmDatafast(AppServices services)
            : this()
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            CargarTiposOperacionPinPad();
            CargarDesdeConfig();
        }

        private void InicializarUI()
        {
            Text = "CONFIGURACIÓN DATAFAST";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ClientSize = new Size(1152, 576);
            MinimumSize = new Size(920, 500);

            var tabs = new TabControl { Dock = DockStyle.Fill };

            var tabConexion = new TabPage("Conexión");
            ConstruirTabConexion(tabConexion);
            tabs.TabPages.Add(tabConexion);

            var tabReinicio = new TabPage("Configurar red");
            ConstruirTabReinicio(tabReinicio);
            tabs.TabPages.Add(tabReinicio);

            var tabTarjeta = new TabPage("Prueba de Tarjeta");
            ConstruirTabPruebaTarjeta(tabTarjeta);
            tabs.TabPages.Add(tabTarjeta);

            var tabLog = new TabPage("Log de Pinpad");
            ConstruirTabLogPinPad(tabLog);
            tabs.TabPages.Add(tabLog);

            Controls.Add(tabs);
        }

        private void ConstruirTabConexion(TabPage tab)
        {
            int x1 = 20, x2 = 160, y = 20, alto = 26, sep = 34, ancho = 260;

            AgregarLabel(tab, "IP del pinpad:", x1, y);
            _txtIp = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtIp);
            y += sep;

            AgregarLabel(tab, "Puerto (escucha):", x1, y);
            _numPuerto = new NumericUpDown { Location = new Point(x2, y - 2), Size = new Size(120, alto), Minimum = 0, Maximum = 65535 };
            tab.Controls.Add(_numPuerto);
            y += sep;

            AgregarLabel(tab, "Timeout (ms):", x1, y);
            _numTimeout = new NumericUpDown { Location = new Point(x2, y - 2), Size = new Size(120, alto), Minimum = 1000, Maximum = 600000, Increment = 1000 };
            tab.Controls.Add(_numTimeout);
            y += sep;

            AgregarLabel(tab, "MID:", x1, y);
            _txtMid = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtMid);
            y += sep;

            AgregarLabel(tab, "TID:", x1, y);
            _txtTid = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtTid);
            y += sep;

            AgregarLabel(tab, "Caja ID:", x1, y);
            _txtCaja = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtCaja);
            y += sep;

            AgregarLabel(tab, "Versión:", x1, y);
            _cmbVersion = new ComboBox { Location = new Point(x2, y - 2), Size = new Size(200, alto), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbVersion.Items.AddRange(new object[] { "0 - V4_2", "1 - V4_4", "2 - VFastTrack" });
            tab.Controls.Add(_cmbVersion);
            y += sep;

            AgregarLabel(tab, "SHA:", x1, y);
            _cmbSha = new ComboBox { Location = new Point(x2, y - 2), Size = new Size(200, alto), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbSha.Items.AddRange(new object[] { "0 - NoSha", "1 - SHA1", "2 - SHA2" });
            tab.Controls.Add(_cmbSha);
            y += sep + 6;

            _btnProbar = new Button { Text = "Probar conexión", Location = new Point(x1, y), Size = new Size(130, 34) };
            _btnProbar.Click += BtnProbar_Click;
            tab.Controls.Add(_btnProbar);

            _btnGuardar = new Button { Text = "Guardar", Location = new Point(x1 + 140, y), Size = new Size(130, 34) };
            _btnGuardar.Click += BtnGuardar_Click;
            tab.Controls.Add(_btnGuardar);
            y += 44;

            _lblEstado = new Label { Location = new Point(x1, y), AutoSize = true, MaximumSize = new Size(410, 0), Font = new Font("Microsoft Sans Serif", 9F) };
            tab.Controls.Add(_lblEstado);
        }

        private static void AgregarLabel(TabPage tab, string texto, int x, int y)
        {
            tab.Controls.Add(new Label
            {
                Text = texto,
                Location = new Point(x, y + 2),
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 9F)
            });
        }

        // =========================================================
        // PESTAÑA LOG DE PINPAD
        // =========================================================
        private void ConstruirTabLogPinPad(TabPage tab)
        {
            // Rejilla de filtros: tres filas y dos columnas fijas (la primera fila usa una
            // tercera para el tipo de operación). Las columnas comparten X y ancho para que
            // los campos de abajo queden alineados entre sí.
            const int colALabel = 18, colACampo = 130;
            const int colBLabel = 330, colBCampo = 452;
            const int colCLabel = 652, colCCampo = 770;
            const int anchoCampo = 180;
            const int fila1 = 16, fila2 = 56, fila3 = 96;
            const int altoCampo = 24;

            var panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 186,
                BackColor = SystemColors.ControlDarkDark
            };

            AgregarLabelFiltro(panelFiltros, "Fecha desde:", colALabel, fila1);
            _dtpLogDesde = new DateTimePicker
            {
                Location = new Point(colACampo, fila1),
                Size = new Size(anchoCampo, altoCampo),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd",
                Value = DateTime.Today
            };
            panelFiltros.Controls.Add(_dtpLogDesde);

            AgregarLabelFiltro(panelFiltros, "Fecha hasta:", colBLabel, fila1);
            _dtpLogHasta = new DateTimePicker
            {
                Location = new Point(colBCampo, fila1),
                Size = new Size(anchoCampo, altoCampo),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd",
                Value = DateTime.Today
            };
            panelFiltros.Controls.Add(_dtpLogHasta);

            AgregarLabelFiltro(panelFiltros, "Tipo de operación:", colCLabel, fila1);
            _cmbLogTipoOperacion = new ComboBox
            {
                Location = new Point(colCCampo, fila1),
                Size = new Size(anchoCampo, altoCampo),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panelFiltros.Controls.Add(_cmbLogTipoOperacion);

            AgregarLabelFiltro(panelFiltros, "Núm. tarjeta:", colALabel, fila2);
            _txtLogTarjeta = new TextBox
            {
                Location = new Point(colACampo, fila2),
                Size = new Size(anchoCampo, altoCampo)
            };
            panelFiltros.Controls.Add(_txtLogTarjeta);

            AgregarLabelFiltro(panelFiltros, "Núm. autorización:", colBLabel, fila2);
            _txtLogAutorizacion = new TextBox
            {
                Location = new Point(colBCampo, fila2),
                Size = new Size(anchoCampo, altoCampo)
            };
            panelFiltros.Controls.Add(_txtLogAutorizacion);

            AgregarLabelFiltro(panelFiltros, "Núm. factura:", colALabel, fila3);
            _txtLogFactura = new TextBox
            {
                Location = new Point(colACampo, fila3),
                Size = new Size(anchoCampo, altoCampo)
            };
            panelFiltros.Controls.Add(_txtLogFactura);

            AgregarLabelFiltro(panelFiltros, "Núm. referencia:", colBLabel, fila3);
            _txtLogReferencia = new TextBox
            {
                Location = new Point(colBCampo, fila3),
                Size = new Size(anchoCampo, altoCampo)
            };
            panelFiltros.Controls.Add(_txtLogReferencia);

            // Autocompletado de los cuatro campos de texto. Las listas se despliegan sobre
            // la fila siguiente y se traen al frente al mostrarse.
            AgregarSugerencias(panelFiltros, _txtLogTarjeta,
                new Rectangle(colACampo, fila2 + altoCampo + 2, anchoCampo, 90),
                texto => _services.PinPadLog.ConsultarTarjetas(texto), "NUMEROTARJETA");

            AgregarSugerencias(panelFiltros, _txtLogAutorizacion,
                new Rectangle(colBCampo, fila2 + altoCampo + 2, anchoCampo, 90),
                texto => _services.PinPadLog.ConsultarAutorizaciones(texto), "AUTORIZACION");

            AgregarSugerencias(panelFiltros, _txtLogFactura,
                new Rectangle(colACampo, fila3 + altoCampo + 2, anchoCampo, 60),
                texto => _services.PinPadLog.ConsultarFacturas(texto), "NUMEROFACTURA");

            AgregarSugerencias(panelFiltros, _txtLogReferencia,
                new Rectangle(colBCampo, fila3 + altoCampo + 2, anchoCampo, 60),
                texto => _services.PinPadLog.ConsultarReferencias(texto), "REFERENCIA");

            // Botón justo debajo del tipo de operación (columna C, fila 2) y la leyenda
            // bajo él, alineada con la fila 3.
            _btnConsultarLog = new Button
            {
                Text = "Consultar",
                Location = new Point(colCCampo, fila2 - 3),
                Size = new Size(anchoCampo, 34),
                BackColor = Color.White,
                UseVisualStyleBackColor = false
            };
            _btnConsultarLog.Click += BtnConsultarLog_Click;
            panelFiltros.Controls.Add(_btnConsultarLog);

            _lblLogResultado = new Label
            {
                Text = "Doble clic en una fila para verla completa.",
                Location = new Point(colCCampo, fila3 + 4),
                Size = new Size(300, 40),
                ForeColor = Color.White
            };
            panelFiltros.Controls.Add(_lblLogResultado);

            _gvLogPinPad = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
            };
            _gvLogPinPad.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
            _gvLogPinPad.CellDoubleClick += GvLogPinPad_CellDoubleClick;
            ProcesosGeneralesUI.HabilitarDobleBuffer(_gvLogPinPad);

            tab.Controls.Add(_gvLogPinPad);
            tab.Controls.Add(panelFiltros);
        }

        /// <summary>Etiqueta blanca sobre el panel oscuro, centrada respecto a su campo.</summary>
        private static void AgregarLabelFiltro(Panel panel, string texto, int x, int y)
        {
            panel.Controls.Add(new Label
            {
                Text = texto,
                Location = new Point(x, y + 4),
                AutoSize = true,
                ForeColor = Color.White
            });
        }

        /// <summary>
        /// Engancha un campo de texto con su lista de sugerencias: al escribir consulta los
        /// valores distintos de la columna y los despliega; al elegir uno lo copia al campo.
        /// El indicador `seleccionando` evita que el TextChanged que dispara esa copia
        /// vuelva a lanzar la consulta.
        /// </summary>
        private void AgregarSugerencias(Panel panel, TextBox campo, Rectangle area,
            Func<string, DataSet> consulta, string columna)
        {
            var lista = new ListBox
            {
                Bounds = area,
                IntegralHeight = false,
                Visible = false
            };

            bool seleccionando = false;

            campo.TextChanged += (s, e) =>
            {
                if (seleccionando)
                    return;

                OcultarSugerenciasLog(lista);
                lista.Items.Clear();

                string texto = campo.Text.Trim();
                if (texto.Length == 0)
                {
                    lista.Visible = false;
                    return;
                }

                try
                {
                    DataSet ds = consulta(texto);
                    if (ds == null || ds.Tables.Count == 0)
                    {
                        lista.Visible = false;
                        return;
                    }

                    foreach (DataRow fila in ds.Tables[0].Rows)
                        lista.Items.Add(fila[columna].ToString());

                    lista.Visible = lista.Items.Count > 0;
                    lista.BringToFront();
                }
                catch
                {
                    lista.Visible = false;
                }
            };

            lista.SelectedIndexChanged += (s, e) =>
            {
                if (lista.SelectedItem == null)
                    return;

                seleccionando = true;
                campo.Text = lista.SelectedItem.ToString();
                seleccionando = false;
                lista.Visible = false;
                campo.Focus();
            };

            _listasSugerenciasLog.Add(lista);
            panel.Controls.Add(lista);
        }

        /// <summary>Solo una lista de sugerencias visible a la vez: se pisan entre filas.</summary>
        private void OcultarSugerenciasLog(ListBox excepto)
        {
            foreach (ListBox lista in _listasSugerenciasLog)
            {
                if (lista != excepto)
                    lista.Visible = false;
            }
        }

        private void CargarTiposOperacionPinPad()
        {
            try
            {
                DataSet ds = _services.PinPadLog.ConsultarTiposOperacion();
                _cmbLogTipoOperacion.DataSource = ds.Tables[0];
                _cmbLogTipoOperacion.DisplayMember = "TIPOOPERACION";
                _cmbLogTipoOperacion.ValueMember = "TIPOOPERACION";
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los tipos de operación: " + ex.Message);
            }
        }

        private void BtnConsultarLog_Click(object sender, EventArgs e)
        {
            if (_dtpLogHasta.Value.Date < _dtpLogDesde.Value.Date)
            {
                MessageBox.Show("La fecha HASTA no puede ser menor a la fecha DESDE.");
                return;
            }

            try
            {
                string tipoOperacion = _cmbLogTipoOperacion.SelectedValue?.ToString()
                    ?? _cmbLogTipoOperacion.Text;

                DataSet ds = _services.PinPadLog.ConsultarLog(
                    _dtpLogDesde.Value,
                    _dtpLogHasta.Value,
                    _txtLogTarjeta.Text,
                    _txtLogFactura.Text,
                    tipoOperacion,
                    _txtLogAutorizacion.Text,
                    _txtLogReferencia.Text);

                _gvLogPinPad.SuspendLayout();
                try
                {
                    _gvLogPinPad.DataSource = ds.Tables[0];
                    ConfigurarGridLogPinPad();
                    _lblLogResultado.Text = ds.Tables[0].Rows.Count +
                        " operación(es). Doble clic para ver todos los campos.";
                }
                finally
                {
                    _gvLogPinPad.ResumeLayout(true);
                }

                if (ds.Tables[0].Rows.Count == 0)
                    MessageBox.Show("No se encontraron registros.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar el log de pinpad: " + ex.Message);
            }
        }

        private void ConfigurarGridLogPinPad()
        {
            if (_gvLogPinPad.Columns.Count == 0)
                return;

            if (_gvLogPinPad.Columns.Contains("NUMERO FACTURA"))
                _gvLogPinPad.Columns["NUMERO FACTURA"].Frozen = true;

            string[] columnasLargas =
            {
                "EXCEPCION",
                "PAGO TARJETA ENCRIPTADA",
                "TRAMA ENVIADA",
                "TRAMA RESPUESTA",
                "EVENTOS"
            };

            foreach (string nombre in columnasLargas)
            {
                if (!_gvLogPinPad.Columns.Contains(nombre))
                    continue;

                DataGridViewColumn columna = _gvLogPinPad.Columns[nombre];
                columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                columna.Width = nombre == "EVENTOS" ? 380 : 280;
            }

            foreach (DataGridViewColumn columna in _gvLogPinPad.Columns)
            {
                if (columna.ValueType == typeof(DateTime))
                    columna.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
            }

            if (_gvLogPinPad.Columns.Contains("PAGO MONTO"))
                _gvLogPinPad.Columns["PAGO MONTO"].DefaultCellStyle.Format = "N2";
            if (_gvLogPinPad.Columns.Contains("PAGO VALOR INTERES"))
                _gvLogPinPad.Columns["PAGO VALOR INTERES"].DefaultCellStyle.Format = "N2";
        }

        private void GvLogPinPad_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow fila = _gvLogPinPad.Rows[e.RowIndex];
            var detalle = new StringBuilder();

            foreach (DataGridViewColumn columna in _gvLogPinPad.Columns)
            {
                object valor = fila.Cells[columna.Index].Value;
                detalle.Append(columna.HeaderText)
                    .Append(": ")
                    .Append(valor == null || valor == DBNull.Value ? "" : valor.ToString())
                    .AppendLine()
                    .AppendLine();
            }

            using (var ventana = new Form
            {
                Text = "DETALLE COMPLETO DE TRANSACCIÓN PINPAD",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(920, 680),
                MinimumSize = new Size(700, 500)
            })
            {
                var texto = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    WordWrap = false,
                    Font = new Font("Consolas", 9F),
                    Text = detalle.ToString()
                };

                var copiar = new Button
                {
                    Text = "Copiar todo",
                    Dock = DockStyle.Bottom,
                    Height = 38
                };
                copiar.Click += (s, args) =>
                {
                    if (texto.TextLength > 0)
                        Clipboard.SetText(texto.Text);
                };

                ventana.Controls.Add(texto);
                ventana.Controls.Add(copiar);
                ventana.ShowDialog(this);
            }
        }

        // =========================================================
        // PESTAÑA REINICIO — reconfigura la RED del pinpad físico
        // (ConfigurarRedPinPad: IP, máscara, gateway, host/puerto
        //  principal y alterno, puerto de escucha del aparato).
        // Usa la conexión ya GUARDADA (_services.PinPad); configure y
        // guarde primero la pestaña Conexión para poder alcanzar el aparato.
        // =========================================================
        private void ConstruirTabReinicio(TabPage tab)
        {
            int x1 = 20, x2 = 175, y = 16, alto = 24, sep = 31, ancho = 250;

            tab.Controls.Add(new Label
            {
                Text = "Los valores se cargan automáticamente. Presione Configurar para aplicarlos al PinPad.",
                Location = new Point(x1, y),
                AutoSize = true,
                MaximumSize = new Size(650, 0),
                ForeColor = Color.FromArgb(65, 65, 65)
            });
            y += 34;

            AgregarLabel(tab, "IP del aparato:", x1, y);
            _txtDevIp = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtDevIp); y += sep;

            AgregarLabel(tab, "Máscara:", x1, y);
            _txtDevMascara = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtDevMascara); y += sep;

            AgregarLabel(tab, "Gateway:", x1, y);
            _txtDevGateway = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtDevGateway); y += sep;

            AgregarLabel(tab, "Host principal:", x1, y);
            _txtDevHostPrin = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtDevHostPrin); y += sep;

            AgregarLabel(tab, "Puerto principal:", x1, y);
            _txtDevPuertoPrin = new TextBox { Location = new Point(x2, y - 2), Size = new Size(120, alto) };
            tab.Controls.Add(_txtDevPuertoPrin); y += sep;

            AgregarLabel(tab, "Host alterno:", x1, y);
            _txtDevHostAlt = new TextBox { Location = new Point(x2, y - 2), Size = new Size(ancho, alto) };
            tab.Controls.Add(_txtDevHostAlt); y += sep;

            AgregarLabel(tab, "Puerto alterno:", x1, y);
            _txtDevPuertoAlt = new TextBox { Location = new Point(x2, y - 2), Size = new Size(120, alto) };
            tab.Controls.Add(_txtDevPuertoAlt); y += sep;

            AgregarLabel(tab, "Puerto de escucha:", x1, y);
            _txtDevPuertoEscucha = new TextBox { Location = new Point(x2, y - 2), Size = new Size(120, alto) };
            tab.Controls.Add(_txtDevPuertoEscucha); y += sep + 6;

            _btnReinicio = new Button { Text = "Configurar", Location = new Point(x1, y), Size = new Size(160, 32) };
            _btnReinicio.Click += BtnReinicio_Click;
            tab.Controls.Add(_btnReinicio); y += 40;

            _lblEstadoReinicio = new Label { Location = new Point(x1, y), AutoSize = true, MaximumSize = new Size(420, 0), Font = new Font("Microsoft Sans Serif", 9F) };
            tab.Controls.Add(_lblEstadoReinicio);
        }

        private async void BtnReinicio_Click(object sender, EventArgs e)
        {
            ConfiguracionRedRequest req;
            string errorValidacion;
            if (!TryCrearConfiguracionRed(out req, out errorValidacion))
            {
                MostrarEstadoEn(_lblEstadoReinicio, errorValidacion, true);
                return;
            }

            _btnReinicio.Enabled = false;
            MostrarEstadoEn(_lblEstadoReinicio, "Configurando el PinPad…", false);
            try
            {
                ConfiguracionRedResult r = await Task.Run(() => _services.PinPad.ConfigurarRedPinPad(req));
                if (r != null && r.Exitoso)
                {
                    try
                    {
                        GuardarConexionConfigurada(req);
                        MostrarEstadoEn(_lblEstadoReinicio,
                            "✔ Configuración aplicada. Conexión actualizada a " +
                            req.DireccionIP + ":" + req.PuertoEscucha +
                            ". Código: " + (r.CodigoRespuesta ?? "00"), false);
                    }
                    catch (Exception ex)
                    {
                        MostrarEstadoEn(_lblEstadoReinicio,
                            "⚠ El PinPad aceptó la configuración, pero no se pudo guardar la nueva conexión: " +
                            ex.Message, true);
                    }
                }
                else
                    MostrarEstadoEn(_lblEstadoReinicio, "✖ No aplicada. " +
                        (r?.MensajeRespuesta ?? r?.ExcepcionMensaje ?? "Verifique conexión con el aparato."), true);
            }
            catch (Exception ex)
            {
                MostrarEstadoEn(_lblEstadoReinicio, "Error: " + ex.Message, true);
            }
            finally
            {
                _btnReinicio.Enabled = true;
            }
        }

        private void GuardarConexionConfigurada(ConfiguracionRedRequest request)
        {
            int puertoEscucha = ParseInt(request.PuertoEscucha, 0);

            // Reflejar inmediatamente en la pestaña Conexión los datos que acaba
            // de aceptar el aparato.
            _txtIp.Text = request.DireccionIP;
            _numPuerto.Value = Clamp(puertoEscucha, _numPuerto.Minimum, _numPuerto.Maximum);

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            EscribirApp(config, K_IP, request.DireccionIP);
            EscribirApp(config, K_PUERTO, request.PuertoEscucha);
            EscribirApp(config, K_RED_MASCARA, request.Mascara);
            EscribirApp(config, K_RED_GATEWAY, request.Gateway);
            EscribirApp(config, K_RED_HOST_PRINCIPAL, request.PrincipalHost);
            EscribirApp(config, K_RED_PUERTO_PRINCIPAL, request.PrincipalPuerto);
            EscribirApp(config, K_RED_HOST_ALTERNO, request.AlternoHost);
            EscribirApp(config, K_RED_PUERTO_ALTERNO, request.AlternoPuerto);
            config.Save(ConfigurationSaveMode.Modified);

            // El siguiente cobro ya utilizará la IP y el puerto de escucha nuevos.
            _services.RecargarPinPad();
        }

        private bool TryCrearConfiguracionRed(out ConfiguracionRedRequest request, out string error)
        {
            request = null;
            error = null;

            string ip = _txtDevIp.Text.Trim();
            string mascara = _txtDevMascara.Text.Trim();
            string gateway = _txtDevGateway.Text.Trim();
            string hostPrincipal = _txtDevHostPrin.Text.Trim();
            string puertoPrincipal = _txtDevPuertoPrin.Text.Trim();
            string hostAlterno = _txtDevHostAlt.Text.Trim();
            string puertoAlterno = _txtDevPuertoAlt.Text.Trim();
            string puertoEscucha = _txtDevPuertoEscucha.Text.Trim();

            if (!EsIPv4(ip))
                error = "La IP del aparato no es una dirección IPv4 válida.";
            else if (!EsIPv4(mascara))
                error = "La máscara no es una dirección IPv4 válida.";
            else if (!EsIPv4(gateway))
                error = "El gateway no es una dirección IPv4 válida.";
            else if (!EsIPv4(hostPrincipal))
                error = "El host principal no es una dirección IPv4 válida.";
            else if (!EsPuertoValido(puertoPrincipal))
                error = "El puerto principal debe estar entre 1 y 65535.";
            else if (!EsIPv4(hostAlterno))
                error = "El host alterno no es una dirección IPv4 válida.";
            else if (!EsPuertoValido(puertoAlterno))
                error = "El puerto alterno debe estar entre 1 y 65535.";
            else if (!EsPuertoValido(puertoEscucha))
                error = "El puerto de escucha debe estar entre 1 y 65535.";

            if (error != null)
                return false;

            request = new ConfiguracionRedRequest
            {
                DireccionIP = ip,
                Mascara = mascara,
                Gateway = gateway,
                PrincipalHost = hostPrincipal,
                PrincipalPuerto = puertoPrincipal,
                AlternoHost = hostAlterno,
                AlternoPuerto = puertoAlterno,
                PuertoEscucha = puertoEscucha,
                UsuarioSistema = UsuarioActual
            };
            return true;
        }

        // =========================================================
        // PESTAÑA PRUEBA DE TARJETA — LeerTarjeta (solo lectura, no cobra).
        // Sirve para verificar que el lector responde y ver los datos de la
        // tarjeta (número enmascarado, bin, vencimiento, red corriente/diferido).
        // =========================================================
        private void ConstruirTabPruebaTarjeta(TabPage tab)
        {
            int x1 = 20, x2 = 175, y = 16, alto = 24, sep = 33, ancho = 250;

            AgregarLabel(tab, "Número (enmascarado):", x1, y);
            _txtTjNumero = CrearReadOnly(x2, y, ancho, alto); tab.Controls.Add(_txtTjNumero); y += sep;

            AgregarLabel(tab, "BIN:", x1, y);
            _txtTjBin = CrearReadOnly(x2, y, ancho, alto); tab.Controls.Add(_txtTjBin); y += sep;

            AgregarLabel(tab, "Vencimiento:", x1, y);
            _txtTjVence = CrearReadOnly(x2, y, ancho, alto); tab.Controls.Add(_txtTjVence); y += sep;

            AgregarLabel(tab, "Red corriente:", x1, y);
            _txtTjRedCorr = CrearReadOnly(x2, y, ancho, alto); tab.Controls.Add(_txtTjRedCorr); y += sep;

            AgregarLabel(tab, "Red diferido:", x1, y);
            _txtTjRedDif = CrearReadOnly(x2, y, ancho, alto); tab.Controls.Add(_txtTjRedDif); y += sep + 8;

            _btnLeerTarjeta = new Button { Text = "Leer tarjeta", Location = new Point(x1, y), Size = new Size(150, 32) };
            _btnLeerTarjeta.Click += BtnLeerTarjeta_Click;
            tab.Controls.Add(_btnLeerTarjeta); y += 40;

            _lblEstadoTarjeta = new Label { Location = new Point(x1, y), AutoSize = true, MaximumSize = new Size(420, 0), Font = new Font("Microsoft Sans Serif", 9F) };
            tab.Controls.Add(_lblEstadoTarjeta);
        }

        private async void BtnLeerTarjeta_Click(object sender, EventArgs e)
        {
            _btnLeerTarjeta.Enabled = false;
            LimpiarDatosTarjeta();
            MostrarEstadoEn(_lblEstadoTarjeta, "Inserte / acerque la tarjeta en el datafast…", false);
            try
            {
                LecturaTarjetaResult r = await Task.Run(() => _services.PinPad.LeerTarjeta());
                if (r != null && r.Exitoso)
                {
                    _txtTjNumero.Text = r.NumeroTarjeta ?? "";
                    _txtTjBin.Text = r.BinTarjeta ?? "";
                    _txtTjVence.Text = r.FechaVencimiento ?? "";
                    _txtTjRedCorr.Text = r.RedAdquirienteCorriente ?? "";
                    _txtTjRedDif.Text = r.RedAdquirienteDiferido ?? "";
                    MostrarEstadoEn(_lblEstadoTarjeta, "✔ Tarjeta leída.", false);
                }
                else
                {
                    MostrarEstadoEn(_lblEstadoTarjeta, "✖ No se pudo leer. " +
                        (r?.MensajeRespuesta ?? r?.ExcepcionMensaje ?? "Reintente la lectura."), true);
                }
            }
            catch (Exception ex)
            {
                MostrarEstadoEn(_lblEstadoTarjeta, "Error: " + ex.Message, true);
            }
            finally
            {
                _btnLeerTarjeta.Enabled = true;
            }
        }

        private void LimpiarDatosTarjeta()
        {
            _txtTjNumero.Text = _txtTjBin.Text = _txtTjVence.Text =
                _txtTjRedCorr.Text = _txtTjRedDif.Text = "";
        }

        private static TextBox CrearReadOnly(int x, int y, int ancho, int alto)
        {
            return new TextBox
            {
                Location = new Point(x, y - 2),
                Size = new Size(ancho, alto),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };
        }

        // =========================================================
        // CARGAR / GUARDAR appSettings
        // =========================================================
        private void CargarDesdeConfig()
        {
            _txtIp.Text = LeerApp(K_IP, "");
            _numPuerto.Value = Clamp(ParseInt(LeerApp(K_PUERTO, "9999"), 9999), _numPuerto.Minimum, _numPuerto.Maximum);
            _numTimeout.Value = Clamp(ParseInt(LeerApp(K_TIMEOUT, "90000"), 90000), _numTimeout.Minimum, _numTimeout.Maximum);
            _txtMid.Text = LeerApp(K_MID, "");
            _txtTid.Text = LeerApp(K_TID, "");
            _txtCaja.Text = LeerApp(K_CAJA, "");
            _cmbVersion.SelectedIndex = Clamp(ParseInt(LeerApp(K_VERSION, "1"), 1), 0, 2);
            _cmbSha.SelectedIndex = Clamp(ParseInt(LeerApp(K_SHA, "1"), 1), 0, 2);

            // La reconfiguración de red parte de la conexión actual del equipo.
            _txtDevIp.Text = _txtIp.Text;
            _txtDevMascara.Text = LeerApp(K_RED_MASCARA, "255.255.255.0");
            _txtDevGateway.Text = LeerApp(K_RED_GATEWAY, DerivarGateway(_txtDevIp.Text));
            _txtDevHostPrin.Text = LeerApp(K_RED_HOST_PRINCIPAL, "200.0.67.188");
            _txtDevPuertoPrin.Text = LeerApp(K_RED_PUERTO_PRINCIPAL, "3000");
            _txtDevHostAlt.Text = LeerApp(K_RED_HOST_ALTERNO, _txtDevHostPrin.Text);
            _txtDevPuertoAlt.Text = LeerApp(K_RED_PUERTO_ALTERNO, _txtDevPuertoPrin.Text);
            _txtDevPuertoEscucha.Text = ((int)_numPuerto.Value).ToString();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtIp.Text))
            {
                MostrarEstado("La IP del pinpad es obligatoria.", true);
                return;
            }

            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                EscribirApp(config, K_IP, _txtIp.Text.Trim());
                EscribirApp(config, K_PUERTO, ((int)_numPuerto.Value).ToString());
                EscribirApp(config, K_TIMEOUT, ((int)_numTimeout.Value).ToString());
                EscribirApp(config, K_MID, _txtMid.Text.Trim());
                EscribirApp(config, K_TID, _txtTid.Text.Trim());
                EscribirApp(config, K_CAJA, _txtCaja.Text.Trim());
                EscribirApp(config, K_VERSION, _cmbVersion.SelectedIndex.ToString());
                EscribirApp(config, K_SHA, _cmbSha.SelectedIndex.ToString());
                config.Save(ConfigurationSaveMode.Modified);

                // Aplicar en caliente al servicio de cobro
                _services.RecargarPinPad();

                // Mantener sincronizados los valores que se enviarán desde Configurar red.
                _txtDevIp.Text = _txtIp.Text.Trim();
                _txtDevGateway.Text = DerivarGateway(_txtDevIp.Text);
                _txtDevPuertoEscucha.Text = ((int)_numPuerto.Value).ToString();

                MostrarEstado("Configuración guardada y aplicada.", false);
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al guardar: " + ex.Message, true);
            }
        }

        private async void BtnProbar_Click(object sender, EventArgs e)
        {
            var settings = LeerSettingsDePantalla();
            if (string.IsNullOrWhiteSpace(settings.IP))
            {
                MostrarEstado("Ingrese la IP del pinpad antes de probar.", true);
                return;
            }

            _btnProbar.Enabled = false;
            _btnGuardar.Enabled = false;
            MostrarEstado("Probando conexión con el pinpad…", false);

            try
            {
                var servicio = new PinPadService(settings, new PinPadLogManejador(_services.Conexion));
                ConfiguracionBasicaResult r = await Task.Run(() => servicio.ConsultarConfiguracionBasica());

                if (r != null && r.Exitoso)
                    MostrarEstado("✔ Pinpad accesible. Código: " + (r.CodigoRespuesta ?? "00"), false);
                else
                    MostrarEstado("✖ Sin respuesta correcta. " +
                        (r?.MensajeRespuesta ?? r?.ExcepcionMensaje ?? "Verifique IP/puerto y que el aparato esté encendido."), true);
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al probar: " + ex.Message, true);
            }
            finally
            {
                _btnProbar.Enabled = true;
                _btnGuardar.Enabled = true;
            }
        }

        private PinPadSettings LeerSettingsDePantalla()
        {
            return new PinPadSettings
            {
                IP = _txtIp.Text.Trim(),
                Puerto = (int)_numPuerto.Value,
                TimeOutMs = (int)_numTimeout.Value,
                MID = _txtMid.Text.Trim(),
                TID = _txtTid.Text.Trim(),
                CajaID = _txtCaja.Text.Trim(),
                Version = _cmbVersion.SelectedIndex,
                SHA = _cmbSha.SelectedIndex
                // SqlConnectionString: null a propósito (auditoría en Access)
            };
        }

        private void MostrarEstado(string mensaje, bool esError)
        {
            MostrarEstadoEn(_lblEstado, mensaje, esError);
        }

        private static void MostrarEstadoEn(Label destino, string mensaje, bool esError)
        {
            destino.ForeColor = esError ? Color.FromArgb(185, 40, 50) : Color.FromArgb(0, 110, 40);
            destino.Text = mensaje;
        }

        // =========================================================
        // Helpers de config
        // =========================================================
        private static string LeerApp(string clave, string porDefecto)
        {
            string v = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrEmpty(v) ? porDefecto : v;
        }

        private static void EscribirApp(Configuration config, string clave, string valor)
        {
            if (config.AppSettings.Settings[clave] != null)
                config.AppSettings.Settings[clave].Value = valor ?? "";
            else
                config.AppSettings.Settings.Add(clave, valor ?? "");
        }

        private static int ParseInt(string v, int porDefecto)
        {
            return int.TryParse(v, out int r) ? r : porDefecto;
        }

        private static decimal Clamp(int valor, decimal min, decimal max)
        {
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }

        private static int Clamp(int valor, int min, int max)
        {
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }

        private static bool EsIPv4(string valor)
        {
            IPAddress ip;
            return IPAddress.TryParse(valor, out ip) &&
                   ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        private static bool EsPuertoValido(string valor)
        {
            int puerto;
            return int.TryParse(valor, out puerto) && puerto >= 1 && puerto <= 65535;
        }

        private static string DerivarGateway(string direccionIp)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(direccionIp, out ip) ||
                ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return string.Empty;

            byte[] octetos = ip.GetAddressBytes();
            octetos[3] = 1;
            return new IPAddress(octetos).ToString();
        }
    }
}
