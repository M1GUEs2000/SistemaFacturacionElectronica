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

        // Compartido por las filas de diferido: explica el combo de plazos, que no
        // es un selector normal sino el que va agregando a la lista de al lado.
        private readonly ToolTip _tooltipPlazos = new ToolTip();

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
            CargarPuntoVenta();
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
            chkPuntoVentaActivo.CheckedChanged += ChkPuntoVentaActivo_CheckedChanged;
            cmbTipoPago.SelectedIndexChanged += CmbTipoPago_SelectedIndexChanged;
            txtPublicidad.TextChanged += TxtPublicidad_TextChanged;
            cmbUnidadAnulacion.SelectedIndexChanged += CmbUnidadAnulacion_SelectedIndexChanged;
            this.Load += frmParametrosTransaccion_Load;
        }

        // =========================================================
        // PUNTO DE VENTA — interruptor maestro del formulario
        // Es el primer parámetro porque de él dependen todos los demás: el resto
        // de esta pantalla configura el cobro con tarjeta, y sin punto de venta
        // no hay cobro con tarjeta (en frmPrincipal TARJETA se comporta como
        // cualquier otra forma de pago, sin popup ni datafast).
        // =========================================================
        private void CargarPuntoVenta()
        {
            chkPuntoVentaActivo.Checked = _services.ParamTransaccion.PuntoVentaActivo();
            AplicarEstadoPuntoVenta();
        }

        private void ChkPuntoVentaActivo_CheckedChanged(object sender, EventArgs e)
        {
            AplicarEstadoPuntoVenta();
        }

        /// <summary>
        /// Habilita o deshabilita los parámetros de tarjeta según el punto de venta.
        /// btnModificar queda siempre activo: es el que guarda el propio interruptor.
        /// </summary>
        private void AplicarEstadoPuntoVenta()
        {
            bool activo = chkPuntoVentaActivo.Checked;

            cmbTipoPago.Enabled = activo;
            cmbUnidadAnulacion.Enabled = activo;
            numPlazoAnulacion.Enabled = activo;
            txtPublicidad.Enabled = activo;
            chkPublicidadActiva.Enabled = activo;
            numVoucher.Enabled = activo;
            panelDiferidos.Enabled = activo;
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
        // PUBLICIDAD DEL VOUCHER (NOMBRE='PUBLICIDAD', CODIGO='P')
        // Es la última línea del voucher de tarjeta. El checkbox es el ACTIVO de la
        // fila: si se desmarca, ObtenerPublicidad() devuelve "" y el voucher no la
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
        // TIPOS DE DIFERIDO: por cada uno, un checkbox (activo) + el combo de
        // plazos y la lista de los habilitados.
        //
        // El combo no deja un valor seleccionado: cada plazo que se elige se
        // AGREGA a la lista de al lado (y si ya estaba, se quita), porque los
        // plazos no son un tope sino el conjunto exacto que el popup de tarjeta
        // le ofrece al cajero. Así se puede habilitar 1, 3, 9 y 12 saltándose el 6.
        // =========================================================
        private void CargarDiferidos()
        {
            panelDiferidos.Controls.Clear();
            _filasDiferido.Clear();

            DataSet ds = _services.ParamTransaccion.ObtenerTiposDiferido();
            if (ds == null || ds.Tables.Count == 0) return;

            // Todo en una sola línea por diferido: nombre, combo de plazos, lista
            // y Limpiar. colCheck..colLimpiar son las columnas de esa línea.
            const int filaAlto = 28;
            const int colCheck = 12;
            const int colEtiqueta = 292;
            const int colCombo = 393;
            const int colLista = 457;
            const int colLimpiar = 641;

            var fuenteChica = new Font("Microsoft Sans Serif", 8F);
            var fuenteNegrita = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold);

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
                    Location = new Point(colCheck, y + 3),
                    Size = new Size(272, 20),
                    Font = fuenteNegrita,
                    Tag = codigo
                };

                var lbl = new Label
                {
                    Text = "Plazos (meses):",
                    Location = new Point(colEtiqueta, y + 6),
                    AutoSize = true,
                    Font = fuenteNegrita,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var cmbPlazo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(colCombo, y + 2),
                    Size = new Size(58, 21),
                    Font = fuenteChica
                };
                foreach (int plazo in ParametrosTransaccionesManejador.PlazosCuotaPosibles)
                    cmbPlazo.Items.Add(plazo.ToString("00"));

                // Ancho justo para los nueve plazos ("01, 03, ... 24"); no más,
                // porque sobrante aquí es ancho de formulario desperdiciado.
                var txtPlazos = new TextBox
                {
                    ReadOnly = true,
                    Location = new Point(colLista, y + 3),
                    Size = new Size(178, 20),
                    Font = fuenteChica,
                    BackColor = SystemColors.Control,
                    TabStop = false
                };

                var btnLimpiar = new Button
                {
                    Text = "Limpiar",
                    Location = new Point(colLimpiar, y + 2),
                    Size = new Size(66, 22),
                    Font = fuenteChica,
                    UseVisualStyleBackColor = true
                };

                var fila = new FilaDiferido
                {
                    Codigo = codigo,
                    CheckActivo = chk,
                    TxtPlazos = txtPlazos,
                    Plazos = _services.ParamTransaccion.ObtenerPlazosCuota(codigo)
                };

                // SelectionChangeCommitted y no SelectedIndexChanged: solo reacciona a
                // lo que elige el administrador, así el SelectedIndex = -1 de después
                // (que deja el combo en blanco, listo para el siguiente plazo) no se
                // dispara a sí mismo.
                cmbPlazo.SelectionChangeCommitted += (s, e) =>
                {
                    if (cmbPlazo.SelectedItem != null
                        && int.TryParse(cmbPlazo.SelectedItem.ToString(), out int plazo))
                        fila.AlternarPlazo(plazo);

                    cmbPlazo.SelectedIndex = -1;
                };

                btnLimpiar.Click += (s, e) => fila.LimpiarPlazos();

                _tooltipPlazos.SetToolTip(cmbPlazo,
                    "Elija un plazo para agregarlo a la lista. Si ya está en la lista, se quita.");
                _tooltipPlazos.SetToolTip(txtPlazos,
                    "Plazos habilitados para este diferido: son los únicos que se le ofrecen al cajero.");

                panelDiferidos.Controls.Add(chk);
                panelDiferidos.Controls.Add(lbl);
                panelDiferidos.Controls.Add(cmbPlazo);
                panelDiferidos.Controls.Add(txtPlazos);
                panelDiferidos.Controls.Add(btnLimpiar);

                fila.RefrescarLista();
                _filasDiferido.Add(fila);

                y += filaAlto;
            }
        }

        private void CargarMinimoFirma()
        {
            string valor = _services.ParamTransaccion.ObtenerMinimoFirma();
            if (decimal.TryParse(valor, out decimal minimo))
                numVoucher.Value = minimo;
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

            // Un diferido habilitado sin plazos dejaría al popup de tarjeta sin
            // cuotas que ofrecer y la venta saldría con 0 cuotas al pinpad. Solo
            // aplica con punto de venta activo: apagado, nadie lee estos parámetros.
            if (chkPuntoVentaActivo.Checked)
            {
                foreach (var fila in _filasDiferido)
                {
                    if (!fila.CheckActivo.Checked || fila.Plazos.Count > 0)
                        continue;

                    Notificaciones.Show(
                        this,
                        "Seleccione al menos un plazo para el diferido: " + fila.CheckActivo.Text.Trim(),
                        "advertencia"
                    );
                    return;
                }
            }

            // El punto de venta se guarda primero: si quedó apagado, los demás
            // parámetros igual se persisten (conservan su valor para cuando se
            // vuelva a encender), simplemente nadie los consume.
            _services.ParamTransaccion.GuardarPuntoVenta(
                chkPuntoVentaActivo.Checked,
                UsuarioActual,
                IPActual
            );

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

                _services.ParamTransaccion.GuardarPlazosCuota(
                    fila.Codigo,
                    fila.Plazos,
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
                numVoucher.Value.ToString("0.##"),
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
            CargarPuntoVenta();
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
            public TextBox TxtPlazos { get; set; }

            /// <summary>
            /// Plazos habilitados, ascendentes. Esta lista es el estado real de la fila:
            /// el TextBox solo la muestra, así que nunca se lee de vuelta desde el texto.
            /// </summary>
            public List<int> Plazos { get; set; }

            /// <summary>Agrega el plazo, o lo quita si ya estaba (el combo funciona como interruptor).</summary>
            public void AlternarPlazo(int Plazo)
            {
                if (!Plazos.Remove(Plazo))
                    Plazos.Add(Plazo);

                Plazos.Sort();
                RefrescarLista();
            }

            public void LimpiarPlazos()
            {
                Plazos.Clear();
                RefrescarLista();
            }

            public void RefrescarLista()
            {
                var textos = new List<string>();

                foreach (int plazo in Plazos)
                    textos.Add(plazo.ToString("00"));

                TxtPlazos.Text = string.Join(", ", textos);
            }
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
