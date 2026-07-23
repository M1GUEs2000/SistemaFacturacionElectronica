using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    public partial class frmParametrosTransaccion : Form
    {
        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private readonly List<FilaDiferido> _filasDiferido = new List<FilaDiferido>();

        public frmParametrosTransaccion(AppServices services)
        {
            InitializeComponent();
            _services = services;
            ConfigurarEventos();
            CargarTipoPago();
            CargarDiferidos();
            CargarMinimoFirma();

            txtFechaActualizacion.ReadOnly = true;
        }

        private void frmParametrosTransaccion_Load(object sender, EventArgs e)
        {
            txtFechaActualizacion.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void ConfigurarEventos()
        {
            btnModificar.Click += BtnModificar_Click;
            cmbTipoPago.SelectedIndexChanged += CmbTipoPago_SelectedIndexChanged;
            this.Load += frmParametrosTransaccion_Load;
        }

        // =========================================================
        // Si el tipo de pago es CORRIENTE, no se muestran los diferidos
        // =========================================================
        private void CmbTipoPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarEstadoTipoPago();
        }

        private void AplicarEstadoTipoPago()
        {
            bool aplicaDiferido = !(cmbTipoPago.SelectedItem is ComboItem tipoPago)
                || !string.Equals(tipoPago.Value, "C", StringComparison.OrdinalIgnoreCase);

            panelDiferidos.Visible = aplicaDiferido;
        }

        // =========================================================
        // TIPO DE PAGO (CORRIENTE / DIFERIDO / AMBOS) — la fila ACTIVO=1 es el modo vigente
        // =========================================================
        private void CargarTipoPago()
        {
            cmbTipoPago.Items.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposPago();
            if (ds == null || ds.Tables.Count == 0) return;

            ComboItem activo = null;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var item = new ComboItem(row["VALOR"].ToString(), row["CODIGO"].ToString());
                cmbTipoPago.Items.Add(item);

                if (Convert.ToBoolean(row["ACTIVO"]))
                    activo = item;
            }

            cmbTipoPago.SelectedItem = activo ?? (cmbTipoPago.Items.Count > 0 ? cmbTipoPago.Items[0] : null);
        }

        // =========================================================
        // TIPOS DE DIFERIDO: un checkbox (activo) + numeric (cuotas) por cada uno
        // =========================================================
        private void CargarDiferidos()
        {
            panelDiferidos.Controls.Clear();
            _filasDiferido.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposDiferido();
            if (ds == null || ds.Tables.Count == 0) return;

            const int filaAlto = 32;
            const int colCheck = 12;
            const int colLabel = 320;
            const int colNumero = 470;

            int y = 8;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string codigo = row["CODIGO"].ToString();
                string descripcion = row["VALOR"].ToString();
                bool activo = Convert.ToBoolean(row["ACTIVO"]);

                var chk = new CheckBox
                {
                    Text = descripcion,
                    Checked = activo,
                    Location = new Point(colCheck, y + 6),
                    Size = new Size(colLabel - colCheck - 10, 20),
                    Tag = codigo
                };

                var lbl = new Label
                {
                    Text = "Cuotas del diferido:",
                    Location = new Point(colLabel, y + 8),
                    AutoSize = true,
                    Font = new Font("Microsoft Sans Serif", 8F),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var num = new NumericUpDown
                {
                    Location = new Point(colNumero, y + 4),
                    Size = new Size(100, 20),
                    Maximum = 60,
                    TextAlign = HorizontalAlignment.Center,
                    Tag = codigo
                };

                DataSet dsCuota = _services.ParamTransaccion.ObtenerCuotasPorDiferido(codigo);
                if (dsCuota != null && dsCuota.Tables.Count > 0 && dsCuota.Tables[0].Rows.Count > 0)
                {
                    if (decimal.TryParse(dsCuota.Tables[0].Rows[0]["VALOR"].ToString(), out decimal cuotas))
                        num.Value = Math.Min(cuotas, num.Maximum);
                }

                panelDiferidos.Controls.Add(chk);
                panelDiferidos.Controls.Add(lbl);
                panelDiferidos.Controls.Add(num);

                _filasDiferido.Add(new FilaDiferido { Codigo = codigo, CheckActivo = chk, NumCuotas = num });

                y += filaAlto;
            }
        }

        private void CargarMinimoFirma()
        {
            string valor = _services.ParamTransaccion.ObtenerMinimoFirma();
            if (decimal.TryParse(valor, out decimal minimo))
                numBaucher.Value = minimo;
        }

        // =========================================================
        // BOTÓN MODIFICAR — guarda tipo de pago vigente, activos/cuotas de cada diferido y mínimo de firma
        // =========================================================
        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (!(cmbTipoPago.SelectedItem is ComboItem tipoPago))
            {
                MessageBox.Show("Seleccione el Tipo de Pago.");
                return;
            }

            foreach (ComboItem item in cmbTipoPago.Items)
            {
                _services.ParamTransaccion.ActualizarActivo(
                    item.Value,
                    "TIPOPAGO",
                    item == tipoPago,
                    UsuarioActual,
                    IPActual
                );
            }

            foreach (var fila in _filasDiferido)
            {
                _services.ParamTransaccion.ActualizarActivo(
                    fila.Codigo,
                    "TIPODIFERIDO",
                    fila.CheckActivo.Checked,
                    UsuarioActual,
                    IPActual
                );

                _services.ParamTransaccion.Actualizar(
                    fila.Codigo,
                    "CUOTA",
                    fila.NumCuotas.Value.ToString("0"),
                    UsuarioActual,
                    IPActual
                );

                _services.ParamTransaccion.ActualizarActivo(
                    fila.Codigo,
                    "CUOTA",
                    fila.CheckActivo.Checked,
                    UsuarioActual,
                    IPActual
                );
            }

            _services.ParamTransaccion.Actualizar(
                "F1",
                "MINIMOFIRMA",
                numBaucher.Value.ToString("0.##"),
                UsuarioActual,
                IPActual
            );

            MessageBox.Show("Parámetros modificados correctamente.");
            CargarTipoPago();
            CargarDiferidos();
            CargarMinimoFirma();
        }

        private class FilaDiferido
        {
            public string Codigo { get; set; }
            public CheckBox CheckActivo { get; set; }
            public NumericUpDown NumCuotas { get; set; }
        }

        private class ComboItem
        {
            public string Text { get; }
            public string Value { get; }

            public ComboItem(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() => Text;
        }
    }
}
