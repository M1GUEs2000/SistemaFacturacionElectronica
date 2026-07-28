using System;
using System.Configuration;
using System.Data;
using System.Drawing;
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
        private TextBox _txtLogTarjeta;
        private ListBox _lstLogTarjetas;
        private ComboBox _cmbLogTipoOperacion;
        private Button _btnConsultarLog;
        private DataGridView _gvLogPinPad;
        private bool _seleccionandoLogTarjeta;

        public frmDatafast(AppServices services)
        {
            _services = services;
            InicializarUI();
            CargarDesdeConfig();
        }

        private void InicializarUI()
        {
            Text = "CONFIGURACIÓN DATAFAST";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1080, 620);

            var tabs = new TabControl { Dock = DockStyle.Fill };

            var tabConexion = new TabPage("Conexión");
            ConstruirTabConexion(tabConexion);
            tabs.TabPages.Add(tabConexion);

            var tabReinicio = new TabPage("Reinicio");
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
            var panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = SystemColors.ControlDarkDark
            };

            panelFiltros.Controls.Add(new Label
            {
                Text = "Fecha desde:",
                Location = new Point(18, 18),
                AutoSize = true,
                ForeColor = Color.White
            });
            _dtpLogDesde = new DateTimePicker
            {
                Location = new Point(108, 14),
                Size = new Size(128, 24),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd",
                Value = DateTime.Today
            };
            panelFiltros.Controls.Add(_dtpLogDesde);

            panelFiltros.Controls.Add(new Label
            {
                Text = "Fecha hasta:",
                Location = new Point(258, 18),
                AutoSize = true,
                ForeColor = Color.White
            });
            _dtpLogHasta = new DateTimePicker
            {
                Location = new Point(342, 14),
                Size = new Size(128, 24),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy/MM/dd",
                Value = DateTime.Today
            };
            panelFiltros.Controls.Add(_dtpLogHasta);

            panelFiltros.Controls.Add(new Label
            {
                Text = "Tarjeta:",
                Location = new Point(492, 18),
                AutoSize = true,
                ForeColor = Color.White
            });
            _txtLogTarjeta = new TextBox { Location = new Point(550, 14), Size = new Size(170, 24) };
            _txtLogTarjeta.TextChanged += TxtLogTarjeta_TextChanged;
            panelFiltros.Controls.Add(_txtLogTarjeta);

            _lstLogTarjetas = new ListBox
            {
                Location = new Point(550, 38),
                Size = new Size(170, 96),
                IntegralHeight = false,
                Visible = false
            };
            _lstLogTarjetas.SelectedIndexChanged += LstLogTarjetas_SelectedIndexChanged;
            panelFiltros.Controls.Add(_lstLogTarjetas);

            panelFiltros.Controls.Add(new Label
            {
                Text = "Tipo de operación:",
                Location = new Point(18, 62),
                AutoSize = true,
                ForeColor = Color.White
            });
            _cmbLogTipoOperacion = new ComboBox
            {
                Location = new Point(138, 58),
                Size = new Size(230, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panelFiltros.Controls.Add(_cmbLogTipoOperacion);

            _btnConsultarLog = new Button
            {
                Text = "Consultar",
                Location = new Point(748, 13),
                Size = new Size(145, 38),
                BackColor = Color.White,
                UseVisualStyleBackColor = false
            };
            _btnConsultarLog.Click += BtnConsultarLog_Click;
            panelFiltros.Controls.Add(_btnConsultarLog);

            _gvLogPinPad = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            tab.Controls.Add(_gvLogPinPad);
            tab.Controls.Add(panelFiltros);
            CargarTiposOperacionPinPad();
        }

        private void TxtLogTarjeta_TextChanged(object sender, EventArgs e)
        {
            if (_seleccionandoLogTarjeta)
                return;

            CargarSugerenciasTarjeta();
        }

        private void CargarSugerenciasTarjeta()
        {
            string texto = _txtLogTarjeta.Text.Trim();
            _lstLogTarjetas.Items.Clear();

            if (texto.Length == 0)
            {
                _lstLogTarjetas.Visible = false;
                return;
            }

            try
            {
                DataSet ds = _services.PinPadLog.ConsultarTarjetas(texto);
                if (ds == null || ds.Tables.Count == 0)
                {
                    _lstLogTarjetas.Visible = false;
                    return;
                }

                foreach (DataRow fila in ds.Tables[0].Rows)
                    _lstLogTarjetas.Items.Add(fila["NUMEROTARJETA"].ToString());

                _lstLogTarjetas.Visible = _lstLogTarjetas.Items.Count > 0;
                _lstLogTarjetas.BringToFront();
            }
            catch
            {
                _lstLogTarjetas.Visible = false;
            }
        }

        private void LstLogTarjetas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lstLogTarjetas.SelectedItem == null)
                return;

            _seleccionandoLogTarjeta = true;
            _txtLogTarjeta.Text = _lstLogTarjetas.SelectedItem.ToString();
            _seleccionandoLogTarjeta = false;
            _lstLogTarjetas.Visible = false;
            _txtLogTarjeta.Focus();
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
                    tipoOperacion);

                _gvLogPinPad.DataSource = ds.Tables[0];

                if (ds.Tables[0].Rows.Count == 0)
                    MessageBox.Show("No se encontraron registros.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar el log de pinpad: " + ex.Message);
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

            _btnReinicio = new Button { Text = "Enviar al aparato", Location = new Point(x1, y), Size = new Size(160, 32) };
            _btnReinicio.Click += BtnReinicio_Click;
            tab.Controls.Add(_btnReinicio); y += 40;

            _lblEstadoReinicio = new Label { Location = new Point(x1, y), AutoSize = true, MaximumSize = new Size(420, 0), Font = new Font("Microsoft Sans Serif", 9F) };
            tab.Controls.Add(_lblEstadoReinicio);
        }

        private async void BtnReinicio_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtDevIp.Text))
            {
                MostrarEstadoEn(_lblEstadoReinicio, "La IP del aparato es obligatoria.", true);
                return;
            }

            var confirmar = MessageBox.Show(
                "Esto reescribe la configuración de RED del pinpad físico. Si los valores " +
                "son incorrectos el aparato puede quedar inalcanzable.\n\n¿Enviar de todos modos?",
                "Confirmar reinicio de red", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes) return;

            var req = new ConfiguracionRedRequest
            {
                DireccionIP = _txtDevIp.Text.Trim(),
                Mascara = _txtDevMascara.Text.Trim(),
                Gateway = _txtDevGateway.Text.Trim(),
                PrincipalHost = _txtDevHostPrin.Text.Trim(),
                PrincipalPuerto = _txtDevPuertoPrin.Text.Trim(),
                AlternoHost = _txtDevHostAlt.Text.Trim(),
                AlternoPuerto = _txtDevPuertoAlt.Text.Trim(),
                PuertoEscucha = _txtDevPuertoEscucha.Text.Trim(),
                UsuarioSistema = UsuarioActual
            };

            _btnReinicio.Enabled = false;
            MostrarEstadoEn(_lblEstadoReinicio, "Enviando configuración al aparato…", false);
            try
            {
                ConfiguracionRedResult r = await Task.Run(() => _services.PinPad.ConfigurarRedPinPad(req));
                if (r != null && r.Exitoso)
                    MostrarEstadoEn(_lblEstadoReinicio, "✔ Configuración aplicada. Código: " + (r.CodigoRespuesta ?? "00"), false);
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
            // Los hosts de Datafast se dejan vacíos cuando no aplican, según la guía.
            _txtDevIp.Text = _txtIp.Text;
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
    }
}
