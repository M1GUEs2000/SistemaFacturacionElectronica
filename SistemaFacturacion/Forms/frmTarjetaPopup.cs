using LogicaNegocios.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    // =====================================================================
    //  POPUP DE TARJETA — Tipo de Pago / Tipo de Diferido / Nº de Cuotas
    //  Ventana NO modal (Show(owner)) que flota sobre la grilla de productos.
    //  Se alimenta de PARAMETROS_TRANSACCIONES (ver ParametrosTransaccionesManejador).
    //
    //  NO tiene botones a propósito: la selección queda expuesta en las
    //  propiedades públicas y frmPrincipal la consume cuando el cajero da
    //  Registrar. Cada cambio de combo dispara SeleccionCambiada para que el
    //  form principal cachee los valores y no dependa de que el popup siga vivo.
    // =====================================================================
    public class frmTarjetaPopup : Form
    {
        private readonly AppServices _services;

        private ComboBox _cmbTipoPago;
        private ComboBox _cmbDiferido;
        private ComboBox _cmbCuotas;
        private ComboBox _cmbMesesGracia;
        private Label _lblDiferido;
        private Label _lblCuotas;
        private Label _lblMesesGracia;

        // Selección resultante — disponible para la emisión de la factura
        public string TipoPago { get; private set; }        // "C" = corriente / "D" = diferido
        public string DiferidoCodigo { get; private set; }  // T1..T7
        public string DiferidoNombre { get; private set; }
        public int Cuotas { get; private set; }
        public int MesesGracia { get; private set; }

        public event EventHandler SeleccionCambiada;

        // El popup se abre sin robarle el foco al form principal: el cajero
        // puede seguir tipeando el código de producto sin tener que volver a
        // hacer clic en la grilla.
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public frmTarjetaPopup(AppServices services)
        {
            _services = services;

            ConstruirUI();
            CargarTipoPago();
        }

        private void ConstruirUI()
        {
            Text = "PAGO CON TARJETA";
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ClientSize = new Size(264, 250);
            StartPosition = FormStartPosition.Manual;
            // Con la X el cajero puede deshacer la elección de TARJETA; el form
            // principal escucha FormClosed y limpia la forma de pago.
            ControlBox = true;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;

            var lblTipoPago = new Label
            {
                Text = "Tipo de Pago:",
                Location = new Point(12, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular)
            };

            _cmbTipoPago = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 36),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            _cmbTipoPago.SelectedIndexChanged += CmbTipoPago_Changed;

            _lblDiferido = new Label
            {
                Text = "Tipo de Diferido:",
                Location = new Point(12, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Visible = false
            };
            _cmbDiferido = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 90),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = false
            };
            _cmbDiferido.SelectedIndexChanged += CmbDiferido_Changed;

            _lblCuotas = new Label
            {
                Text = "Nº de Cuotas:",
                Location = new Point(12, 124),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Visible = false
            };
            _cmbCuotas = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 144),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = false
            };
            _cmbCuotas.SelectedIndexChanged += (s, e) => GuardarSeleccion();

            _lblMesesGracia = new Label
            {
                Text = "Meses de Gracia:",
                Location = new Point(12, 178),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Visible = false
            };
            _cmbMesesGracia = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 198),
                Size = new Size(240, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Visible = false
            };
            for (int i = 1; i <= 24; i++)
                _cmbMesesGracia.Items.Add(i.ToString("00"));
            _cmbMesesGracia.SelectedIndexChanged += (s, e) => GuardarSeleccion();

            Controls.Add(lblTipoPago);
            Controls.Add(_cmbTipoPago);
            Controls.Add(_lblDiferido);
            Controls.Add(_cmbDiferido);
            Controls.Add(_lblCuotas);
            Controls.Add(_cmbCuotas);
            Controls.Add(_lblMesesGracia);
            Controls.Add(_cmbMesesGracia);
        }

        private const int MargenPopup = 8;

        // Ancla el popup en la esquina SUPERIOR DERECHA del control indicado
        // (la grilla de productos), en coordenadas de PANTALLA y sin salirse
        // del monitor.
        public void UbicarSobre(Control referencia)
        {
            if (referencia == null || !referencia.IsHandleCreated) return;

            Point origen = referencia.PointToScreen(Point.Empty);

            int x = origen.X + referencia.Width - Width - MargenPopup;
            int y = origen.Y + MargenPopup;

            Rectangle area = Screen.FromControl(referencia).WorkingArea;
            x = Math.Max(area.Left, Math.Min(x, area.Right - Width));
            y = Math.Max(area.Top, Math.Min(y, area.Bottom - Height));

            Location = new Point(x, y);
        }

        // TIPO DE PAGO: CORRIENTE es SIEMPRE el default. DIFERIDO solo se ofrece
        // cuando en los parámetros está activo AMBOS (CODIGO 'A', ACTIVO=1).
        // Si no está AMBOS, el combo queda bloqueado en CORRIENTE.
        private void CargarTipoPago()
        {
            _cmbTipoPago.Items.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposPago();

            string valorCorriente = "CORRIENTE";
            string valorDiferido = "DIFERIDO";
            bool ambosActivo = false;

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string cod = row["CODIGO"].ToString().Trim().ToUpper();
                    string val = row["VALOR"].ToString().Trim();

                    if (cod == "C") valorCorriente = val;
                    else if (cod == "D") valorDiferido = val;

                    if (cod == "A" && Convert.ToBoolean(row["ACTIVO"]))
                        ambosActivo = true;
                }
            }

            // CORRIENTE siempre presente y seleccionado por default
            _cmbTipoPago.Items.Add(new ItemCombo(valorCorriente, "C"));

            // DIFERIDO únicamente si AMBOS está activo
            if (ambosActivo)
                _cmbTipoPago.Items.Add(new ItemCombo(valorDiferido, "D"));

            // Solo se puede cambiar si existe la opción DIFERIDO
            _cmbTipoPago.Enabled = ambosActivo;

            _cmbTipoPago.SelectedIndex = 0; // CORRIENTE => combos de diferido ocultos
        }

        private void CmbTipoPago_Changed(object sender, EventArgs e)
        {
            var seleccionado = _cmbTipoPago.SelectedItem as ItemCombo;
            bool esDiferido = seleccionado != null && seleccionado.Value == "D";

            // Tipo de diferido: visible solo cuando el pago es DIFERIDO
            _lblDiferido.Visible = esDiferido;
            _cmbDiferido.Visible = esDiferido;

            // Cuotas: ocultas hasta que se escoja un tipo de diferido
            _lblCuotas.Visible = false;
            _cmbCuotas.Visible = false;
            _cmbCuotas.Items.Clear();
            _lblMesesGracia.Visible = false;
            _cmbMesesGracia.Visible = false;
            _cmbMesesGracia.SelectedIndex = -1;

            if (esDiferido)
                CargarDiferidos();
            else
                _cmbDiferido.Items.Clear();

            GuardarSeleccion();
        }

        // TIPO DE DIFERIDO: solo los activos. Se traen todos y se filtra en C# con
        // Convert.ToBoolean, porque en Access el campo Sí/No guarda True como -1 y la
        // consulta 'ACTIVO = 1' de ObtenerTiposDiferidoActivos no matchea nada.
        private void CargarDiferidos()
        {
            _cmbDiferido.Items.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposDiferido();
            if (ds != null && ds.Tables.Count > 0)
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (!Convert.ToBoolean(row["ACTIVO"])) continue;

                    _cmbDiferido.Items.Add(new ItemCombo(
                        row["VALOR"].ToString().Trim(),
                        row["CODIGO"].ToString().Trim()));
                }

            // Default: SIN INTERESES (si está activo); si no, el primero disponible
            int idxDefault = -1;
            for (int i = 0; i < _cmbDiferido.Items.Count; i++)
                if (((ItemCombo)_cmbDiferido.Items[i]).Text
                        .Equals("SIN INTERESES", StringComparison.OrdinalIgnoreCase))
                {
                    idxDefault = i;
                    break;
                }

            if (idxDefault < 0 && _cmbDiferido.Items.Count > 0)
                idxDefault = 0;

            _cmbDiferido.SelectedIndex = idxDefault; // dispara la carga de cuotas
        }

        private void CmbDiferido_Changed(object sender, EventArgs e)
        {
            bool hayDiferido = _cmbDiferido.SelectedItem is ItemCombo;

            // Cuotas: aparecen solo cuando ya hay un tipo de diferido escogido
            _lblCuotas.Visible = hayDiferido;
            _cmbCuotas.Visible = hayDiferido;

            if (hayDiferido)
                CargarCuotas();
            else
                _cmbCuotas.Items.Clear();

            bool aplicaMesesGracia = hayDiferido
                && EsDiferidoConMesesGracia(_cmbDiferido.SelectedItem as ItemCombo);
            _lblMesesGracia.Visible = aplicaMesesGracia;
            _cmbMesesGracia.Visible = aplicaMesesGracia;
            if (aplicaMesesGracia && _cmbMesesGracia.SelectedIndex < 0)
                _cmbMesesGracia.SelectedIndex = 0; // 01 por defecto
            else if (!aplicaMesesGracia)
                _cmbMesesGracia.SelectedIndex = -1;

            GuardarSeleccion();
        }

        // Nº DE CUOTAS: solo múltiplos de tres (03..24), limitados por la cuota
        // configurada para el tipo de diferido.
        private void CargarCuotas()
        {
            _cmbCuotas.Items.Clear();

            var dif = _cmbDiferido.SelectedItem as ItemCombo;
            if (dif == null) return;

            DataSet ds = _services.ParamTransaccion.ObtenerCuotasPorDiferido(dif.Value);

            int maxCuotas = 0;
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                int.TryParse(ds.Tables[0].Rows[0]["VALOR"].ToString().Trim(), out maxCuotas);

            int limiteCuotas = Math.Min(maxCuotas, 24);
            for (int i = 3; i <= limiteCuotas; i += 3)
                _cmbCuotas.Items.Add(i.ToString("00"));

            if (_cmbCuotas.Items.Count > 0)
                _cmbCuotas.SelectedIndex = 0;
        }

        private void GuardarSeleccion()
        {
            var tipo = _cmbTipoPago.SelectedItem as ItemCombo;
            TipoPago = tipo != null ? tipo.Value : "";

            if (TipoPago == "D")
            {
                var dif = _cmbDiferido.SelectedItem as ItemCombo;
                DiferidoCodigo = dif != null ? dif.Value : "";
                DiferidoNombre = dif != null ? dif.Text : "";

                int cuotas;
                Cuotas = int.TryParse(
                    _cmbCuotas.SelectedItem == null ? null : _cmbCuotas.SelectedItem.ToString(),
                    out cuotas) ? cuotas : 0;

                int mesesGracia;
                MesesGracia = EsDiferidoConMesesGracia(dif)
                    && int.TryParse(
                        _cmbMesesGracia.SelectedItem == null ? null : _cmbMesesGracia.SelectedItem.ToString(),
                        out mesesGracia)
                    ? mesesGracia
                    : 0;
            }
            else
            {
                DiferidoCodigo = "";
                DiferidoNombre = "";
                Cuotas = 0;
                MesesGracia = 0;
            }

            var handler = SeleccionCambiada;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private static bool EsDiferidoConMesesGracia(ItemCombo diferido)
        {
            if (diferido == null) return false;

            return diferido.Value == "07"
                || diferido.Value == "09"
                || diferido.Text.IndexOf("GRACIA", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class ItemCombo
        {
            public string Text { get; private set; }
            public string Value { get; private set; }

            public ItemCombo(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() { return Text; }
        }
    }
}
