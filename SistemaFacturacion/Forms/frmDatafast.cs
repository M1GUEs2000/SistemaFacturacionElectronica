using System;
using System.Configuration;
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
    /// Configuración del pinpad Datafast. Por ahora solo implementa la pestaña
    /// CONEXIÓN (los datos de appSettings PinPad.*). Las pestañas Reinicio, Prueba
    /// de Tarjeta y Anulación quedan como placeholder para implementarse después.
    ///
    /// La conexión se lee/escribe en el .exe.config (appSettings) en caliente:
    /// al Guardar se llama a AppServices.RecargarPinPad() para que el cobro use los
    /// nuevos valores sin reiniciar la aplicación.
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
            ClientSize = new Size(460, 420);

            var tabs = new TabControl { Dock = DockStyle.Fill };

            var tabConexion = new TabPage("Conexión");
            ConstruirTabConexion(tabConexion);
            tabs.TabPages.Add(tabConexion);

            tabs.TabPages.Add(CrearTabPlaceholder("Reinicio"));
            tabs.TabPages.Add(CrearTabPlaceholder("Prueba de Tarjeta"));
            tabs.TabPages.Add(CrearTabPlaceholder("Anulación"));

            Controls.Add(tabs);
        }

        private static TabPage CrearTabPlaceholder(string titulo)
        {
            var tab = new TabPage(titulo);
            tab.Controls.Add(new Label
            {
                Text = "En construcción.",
                AutoSize = true,
                ForeColor = Color.Gray,
                Location = new Point(20, 20),
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Italic)
            });
            return tab;
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
            _lblEstado.ForeColor = esError ? Color.FromArgb(185, 40, 50) : Color.FromArgb(0, 110, 40);
            _lblEstado.Text = mensaje;
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
