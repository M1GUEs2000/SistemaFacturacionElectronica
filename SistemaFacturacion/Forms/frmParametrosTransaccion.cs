using LogicaNegocios;
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

        // Valor guardado de cada unidad del plazo de anulación. Se conservan aparte para que,
        // al cambiar de DÍAS a MINUTOS y volver, se reponga lo que hay en la BD y no lo que
        // quedó seleccionado en pantalla.
        private string _plazoAnulacionDias = "";
        private string _plazoAnulacionMinutos = "";
        private bool _cargandoAnulacion;

        public frmParametrosTransaccion(AppServices services)
        {
            InitializeComponent();
            _services = services;
            ConfigurarEventos();
            CargarTipoPago();
            CargarPublicidad();
            CargarDiferidos();
            CargarMinimoFirma();
            CargarPlazoAnulacion();

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
            txtPublicidad.TextChanged += TxtPublicidad_TextChanged;
            cmbUnidadAnulacion.SelectedIndexChanged += CmbUnidadAnulacion_SelectedIndexChanged;
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
        // TIPO DE PAGO (CORRIENTE / DIFERIDO / AMBOS) — la fila ACTIVO=-1 es el modo vigente
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
        // PUBLICIDAD DEL BAUCHER (NOMBRE='PUBLICIDAD', CODIGO='P')
        // Es la última línea del baucher de tarjeta. El checkbox es el ACTIVO de la
        // fila: si se desmarca, ObtenerPublicidad() devuelve "" y el baucher no la
        // imprime, pero el texto queda guardado en la BD.
        // =========================================================
        private void CargarPublicidad()
        {
            // No se usa ObtenerPublicidad(): ese filtra por ACTIVO=-1 y aquí hay que
            // mostrar el texto aunque la publicidad esté apagada.
            DataSet ds = _services.ParamTransaccion.ConsultarPorCodigo("P");

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                txtPublicidad.Text = row["VALOR"].ToString().Trim();
                chkPublicidadActiva.Checked = Convert.ToBoolean(row["ACTIVO"]);
            }
            else
            {
                txtPublicidad.Text = "";
                chkPublicidadActiva.Checked = false;
            }

            ActualizarContadorPublicidad();
        }

        private void TxtPublicidad_TextChanged(object sender, EventArgs e)
        {
            ActualizarContadorPublicidad();
        }

        private void ActualizarContadorPublicidad()
        {
            int restantes = txtPublicidad.MaxLength - txtPublicidad.Text.Length;
            lblContadorPublicidad.Text = "Te quedan " + restantes + " caracteres";
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
                    Text = "Cuota Máxima del Diferido:",
                    Location = new Point(colLabel, y + 8),
                    AutoSize = true,
                    Font = new Font("Microsoft Sans Serif", 8F),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var cmbCuotas = new ComboBox
                {
                    Location = new Point(colNumero, y + 4),
                    Size = new Size(100, 20),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Tag = codigo
                };
                for (int c = 3; c <= 96; c += 3)
                    cmbCuotas.Items.Add(c.ToString("00"));

                DataSet dsCuota = _services.ParamTransaccion.ObtenerCuotasPorDiferido(codigo);
                if (dsCuota != null && dsCuota.Tables.Count > 0 && dsCuota.Tables[0].Rows.Count > 0)
                {
                    if (decimal.TryParse(dsCuota.Tables[0].Rows[0]["VALOR"].ToString(), out decimal cuotas))
                    {
                        int cuotaNormalizada = Math.Max(3, Math.Min(96, ((int)cuotas / 3) * 3));
                        cmbCuotas.SelectedItem = cuotaNormalizada.ToString("00");
                    }
                }
                if (cmbCuotas.SelectedIndex < 0)
                    cmbCuotas.SelectedIndex = 0;

                panelDiferidos.Controls.Add(chk);
                panelDiferidos.Controls.Add(lbl);
                panelDiferidos.Controls.Add(cmbCuotas);

                _filasDiferido.Add(new FilaDiferido { Codigo = codigo, CheckActivo = chk, CmbCuotas = cmbCuotas });

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
        // PLAZO MÁXIMO PARA ANULACIONES
        // Dos filas excluyentes en BD (MAXIMOANULACIONDIAS / MAXIMOANULACIONMINUTOS):
        // el combo de unidad decide cuál queda activa y el otro combo su valor.
        // =========================================================
        private void CargarPlazoAnulacion()
        {
            _cargandoAnulacion = true;

            _plazoAnulacionDias = "";
            _plazoAnulacionMinutos = "";
            string unidadActiva = "";

            DataSet ds = _services.ParamTransaccion.ObtenerPlazosAnulacion();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string nombre = row["NOMBRE"].ToString().Trim();
                    string valor = row["VALOR"].ToString().Trim();

                    if (nombre == ParametrosTransaccionesManejador.AnulacionDias)
                        _plazoAnulacionDias = valor;
                    else
                        _plazoAnulacionMinutos = valor;

                    if (Convert.ToBoolean(row["ACTIVO"]))
                        unidadActiva = nombre;
                }
            }

            cmbUnidadAnulacion.Items.Clear();
            cmbUnidadAnulacion.Items.Add(new ComboItem("Días", ParametrosTransaccionesManejador.AnulacionDias));
            cmbUnidadAnulacion.Items.Add(new ComboItem("Minutos", ParametrosTransaccionesManejador.AnulacionMinutos));

            // Sin unidad activa en BD se asume minutos, que es el plazo más restrictivo.
            cmbUnidadAnulacion.SelectedIndex =
                unidadActiva == ParametrosTransaccionesManejador.AnulacionDias ? 0 : 1;

            _cargandoAnulacion = false;
            AplicarUnidadPlazo();
        }

        private void CmbUnidadAnulacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cargandoAnulacion) return;
            AplicarUnidadPlazo();
        }

        /// <summary>Ajusta el tope del plazo según la unidad elegida y repone el valor guardado de esa unidad.</summary>
        private void AplicarUnidadPlazo()
        {
            bool enDias = cmbUnidadAnulacion.SelectedItem is ComboItem unidad
                && unidad.Value == ParametrosTransaccionesManejador.AnulacionDias;

            // 3 días o 1440 minutos (24 h) — el tope se aplica antes del valor
            // porque NumericUpDown recorta cualquier Value que exceda el Maximum.
            numPlazoAnulacion.Maximum = enDias ? 3 : 1440;

            string guardado = enDias ? _plazoAnulacionDias : _plazoAnulacionMinutos;
            numPlazoAnulacion.Value = int.TryParse(guardado, out int valor)
                && valor >= numPlazoAnulacion.Minimum
                && valor <= numPlazoAnulacion.Maximum
                    ? valor
                    : numPlazoAnulacion.Minimum;
        }

        // =========================================================
        // BOTÓN MODIFICAR — guarda tipo de pago vigente, activos/cuotas de cada diferido y mínimo de firma
        // =========================================================
        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (!(cmbTipoPago.SelectedItem is ComboItem tipoPago))
            {
                Notificaciones.Show(this, "Seleccione el Tipo de Pago.", "advertencia");
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

            _services.ParamTransaccion.ActualizarPublicidad(
                txtPublicidad.Text.Trim(),
                UsuarioActual,
                IPActual
            );

            _services.ParamTransaccion.ActualizarActivo(
                "P",
                "PUBLICIDAD",
                chkPublicidadActiva.Checked,
                UsuarioActual,
                IPActual
            );

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
                    fila.CmbCuotas.SelectedItem?.ToString() ?? "03",
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

            if (cmbUnidadAnulacion.SelectedItem is ComboItem unidadAnulacion)
            {
                _services.ParamTransaccion.GuardarPlazoAnulacion(
                    unidadAnulacion.Value,
                    ((int)numPlazoAnulacion.Value).ToString(),
                    UsuarioActual,
                    IPActual
                );
            }

            Notificaciones.Show(this, "Parámetros modificados correctamente.", "exito");
            CargarTipoPago();
            CargarPublicidad();
            CargarDiferidos();
            CargarMinimoFirma();
            CargarPlazoAnulacion();
        }

        private class FilaDiferido
        {
            public string Codigo { get; set; }
            public CheckBox CheckActivo { get; set; }
            public ComboBox CmbCuotas { get; set; }
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
