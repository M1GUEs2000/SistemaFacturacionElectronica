using LogicaNegocios.Services;
using System;
using System.Data;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmParametrosFactura : Form
    {

        private readonly AppServices _services;

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }


        public frmParametrosFactura(AppServices services)
        {
            InitializeComponent();
            _services = services;
            ConfigurarEventos();
            CargarCombos();
            btnModificar.Visible = false;
            CargarParametros();

            txtFechaActualizacion.ReadOnly = true;
            txtNumeroDigitos.ReadOnly = true;

        }

        // =========================================================
        // 1️⃣ EVENTO LOAD
        // =========================================================
        private void frmParametrosFactura_Load(object sender, EventArgs e)
        {
            txtFechaActualizacion.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        }

        private void CargarParametros()
        {
            try
            {
                DataSet ds = _services.ParamFactura.Mostrar();
                dataGridView1.DataSource = ds.Tables[0];
                btnModificar.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar parámetros: " + ex.Message);
            }
        }

        // =========================================================
        // 2️⃣ CARGAR COMBOS (DESCRIPCIÓN MOSTRADA, CÓDIGO GUARDADO)
        // =========================================================
        private void CargarCombos()
        {
            cmbAmbiente.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmision.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAgenteR.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbContribuyenteR.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCodDoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbContabilidad.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoIdentificacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMoneda.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoImpuesto.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPorcentaje.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbContribuyenteE.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstablecimiento.DropDownStyle = ComboBoxStyle.DropDown;
            cmbPuntoE.DropDownStyle = ComboBoxStyle.DropDown;

            cmbAmbiente.Items.Clear();
            cmbEmision.Items.Clear();
            cmbAgenteR.Items.Clear();
            cmbContribuyenteR.Items.Clear();
            cmbCodDoc.Items.Clear();
            cmbContribuyenteE.Items.Clear();
            cmbEstablecimiento.Items.Clear();
            cmbPuntoE.Items.Clear();
            cmbContabilidad.Items.Clear();
            cmbTipoIdentificacion.Items.Clear();
            cmbMoneda.Items.Clear();
            cmbTipoImpuesto.Items.Clear();
            cmbPorcentaje.Items.Clear();

            cmbAmbiente.Items.Add(new ComboItem("Pruebas", "1"));
            cmbAmbiente.Items.Add(new ComboItem("Producción", "2"));

            cmbEmision.Items.Add(new ComboItem("Emisión Normal", "1"));

            cmbAgenteR.Items.Add(new ComboItem("SI", "SI"));
            cmbAgenteR.Items.Add(new ComboItem("NO", "NO"));

            cmbContribuyenteR.Items.Add(new ComboItem("(Vacío / no aplica)", ""));
            cmbContribuyenteR.Items.Add(new ComboItem("CONTRIBUYENTE RÉGIMEN RIMPE", "CONTRIBUYENTE RÉGIMEN RIMPE"));
            cmbContribuyenteR.Items.Add(new ComboItem("CONTRIBUYENTE NEGOCIO POPULAR - RÉGIMEN RIMPE", "CONTRIBUYENTE NEGOCIO POPULAR - RÉGIMEN RIMPE"));

            cmbCodDoc.Items.Add(new ComboItem("Factura", "01"));

            cmbContribuyenteE.Items.Add("NO");
            cmbContribuyenteE.Items.Add("SI");

            cmbEstablecimiento.Items.Add(new ComboItem("001", "001"));

            cmbPuntoE.Items.Add(new ComboItem("002", "002"));

            cmbContabilidad.Items.Add(new ComboItem("SI", "SI"));
            cmbContabilidad.Items.Add(new ComboItem("NO", "NO"));

            cmbTipoIdentificacion.Items.Add(new ComboItem("RUC", "04"));
            cmbTipoIdentificacion.Items.Add(new ComboItem("Cédula", "05"));
            cmbTipoIdentificacion.Items.Add(new ComboItem("Pasaporte", "06"));
            cmbTipoIdentificacion.Items.Add(new ComboItem("Consumidor Final", "07"));
            cmbTipoIdentificacion.Items.Add(new ComboItem("Exterior", "08"));

            cmbMoneda.Items.Add(new ComboItem("Dólares (USD)", "USD"));

            // Tipo de impuesto: fijo en IVA
            cmbTipoImpuesto.Items.Add(new ComboItem("IVA", "2"));
            cmbTipoImpuesto.SelectedIndex = 0;
            cmbTipoImpuesto.Enabled = false;

            // Porcentajes
            cmbPorcentaje.Items.Add(new ComboItem("IVA 0%", "0"));
            cmbPorcentaje.Items.Add(new ComboItem("IVA 12%", "2"));
            cmbPorcentaje.Items.Add(new ComboItem("IVA 14%", "3"));
            cmbPorcentaje.Items.Add(new ComboItem("IVA 15%", "4"));
            cmbPorcentaje.Items.Add(new ComboItem("IVA 5%", "5"));
            cmbPorcentaje.Items.Add(new ComboItem("No Objeto", "6"));
            cmbPorcentaje.Items.Add(new ComboItem("Exento", "7"));
            cmbPorcentaje.Items.Add(new ComboItem("IVA diferenciado (8%)", "8"));
            cmbPorcentaje.Items.Add(new ComboItem("IVA 13%", "10"));

            cmbContribuyenteE.SelectedIndex = 0;
            AplicarEstadoContribuyenteEspecial();
        }

        // =========================================================
        // 3️⃣ CONFIGURAR EVENTOS
        // =========================================================
        private void ConfigurarEventos()
        {
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            dataGridView1.CellClick += DataGridView1_CellClick;
            cmbContribuyenteE.SelectedIndexChanged += CmbContribuyenteE_SelectedIndexChanged;
            this.Load += frmParametrosFactura_Load;
        }

        // =========================================================
        // 4️⃣ CUANDO EL USUARIO SELECCIONA UNA FILA
        // =========================================================
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

            SeleccionarCombo(cmbAmbiente, row.Cells["AMBIENTE"].Value.ToString());
            SeleccionarCombo(cmbEmision, row.Cells["TIPOEMISION"].Value.ToString());
            SeleccionarCombo(cmbAgenteR, row.Cells["AGENTERETENCION"].Value.ToString());
            string contribuyenteRimpe = row.Cells["CONTRIBUYENTERIMPE"].Value.ToString();
            if (string.Equals(contribuyenteRimpe, "GENERAL", StringComparison.OrdinalIgnoreCase))
                contribuyenteRimpe = "";

            SeleccionarCombo(cmbContribuyenteR, contribuyenteRimpe);
            SeleccionarCombo(cmbCodDoc, "01");
            SeleccionarCombo(cmbEstablecimiento, row.Cells["ESTAB"].Value.ToString());
            SeleccionarCombo(cmbPuntoE, row.Cells["PUNTOEMISION"].Value.ToString());

            txtNumeroDigitos.Text = row.Cells["NUMERODIGITOS"].Value.ToString();

            string contribuyenteEspecial = row.Cells["CONTRIBUYENTEESPECIAL"].Value.ToString();
            bool tieneContribuyenteEspecial = !string.IsNullOrWhiteSpace(contribuyenteEspecial);
            cmbContribuyenteE.SelectedItem = tieneContribuyenteEspecial ? "SI" : "NO";
            txtContribuyenteEspecial.Text = tieneContribuyenteEspecial
                ? contribuyenteEspecial
                : "VACIO / NO APLICA";
            AplicarEstadoContribuyenteEspecial();
            SeleccionarCombo(cmbContabilidad, row.Cells["OBLIGADOCONTABILIDAD"].Value.ToString());
            SeleccionarCombo(cmbTipoIdentificacion, row.Cells["TIPOIDENTIFICADORCOMPRADOR"].Value.ToString());
            SeleccionarCombo(cmbMoneda, row.Cells["MONEDA"].Value.ToString());
            SeleccionarCombo(cmbTipoImpuesto, "2");
            SeleccionarCombo(cmbPorcentaje, row.Cells["CODIGOPORCENTAJE"].Value.ToString());

            txtServidorCorreos.Text = row.Cells["SMTPSERVER"].Value.ToString();
            txtPuertoCorreos.Text = row.Cells["SMTPPORT"].Value.ToString();
            txtUsuarioCorreos.Text = row.Cells["SMTPUSER"].Value.ToString();
            txtClaveCorreos.Text = row.Cells["SMTPPASS"].Value.ToString();


            txtFechaActualizacion.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            btnModificar.Visible = true;
        }

        private void CmbContribuyenteE_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarEstadoContribuyenteEspecial();
        }

        private void AplicarEstadoContribuyenteEspecial()
        {
            bool aplica = string.Equals(cmbContribuyenteE.Text, "SI", StringComparison.OrdinalIgnoreCase);

            txtContribuyenteEspecial.Enabled = aplica;

            if (aplica)
            {
                if (string.Equals(txtContribuyenteEspecial.Text.Trim(), "VACIO / NO APLICA", StringComparison.OrdinalIgnoreCase))
                    txtContribuyenteEspecial.Clear();
            }
            else
            {
                txtContribuyenteEspecial.Text = "VACIO / NO APLICA";
            }
        }

        private void SeleccionarCombo(ComboBox combo, string value)
        {
            foreach (ComboItem item in combo.Items)
            {
                if (item.Value == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        // =========================================================
        // 5️⃣ BOTÓN MODIFICAR
        // =========================================================
        private void BtnModificar_Click(object sender, EventArgs e)
        {
            if (!ValidarObligatorios()) return;
            int filas = _services.ParamFactura.Actualizar(
                UsuarioActual,
                ((ComboItem)cmbAmbiente.SelectedItem).Value,
                ((ComboItem)cmbEmision.SelectedItem).Value,
                ((ComboItem)cmbAgenteR.SelectedItem).Value,
                ((ComboItem)cmbContribuyenteR.SelectedItem).Value,
                ((ComboItem)cmbCodDoc.SelectedItem).Value,
                cmbEstablecimiento.Text.Trim(),
                cmbPuntoE.Text.Trim(),
                txtNumeroDigitos.Text.Trim(),
                ObtenerContribuyenteEspecial(),
                ((ComboItem)cmbContabilidad.SelectedItem).Value,
                ((ComboItem)cmbTipoIdentificacion.SelectedItem).Value,
                ((ComboItem)cmbMoneda.SelectedItem).Value,
                ((ComboItem)cmbTipoImpuesto.SelectedItem).Value,
                ((ComboItem)cmbPorcentaje.SelectedItem).Value,
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                txtServidorCorreos.Text.Trim(),
                txtPuertoCorreos.Text.Trim(),
                txtUsuarioCorreos.Text.Trim(),
                txtClaveCorreos.Text.Trim(),
                UsuarioActual,
                IPActual
            );

            if (filas > 0)
            {
                MessageBox.Show("Parámetros modificados correctamente.");
                CargarParametros();
            }
            else
            {
                MessageBox.Show("No se pudo modificar.");
            }
        }

        // =========================================================
        // 6️⃣ BOTÓN ELIMINAR
        // =========================================================
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Eliminar parámetros?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            int filas = _services.ParamFactura.Eliminar(UsuarioActual, UsuarioActual, IPActual);

            if (filas > 0)
            {
                MessageBox.Show("Eliminado.");
                CargarParametros();
            }
            else
            {
                MessageBox.Show("No se pudo eliminar.");
            }
        }

        // =========================================================
        // 7️⃣ VALIDAR CAMPOS
        // =========================================================
        private bool ValidarObligatorios()
        {
            // Combo obligatorios
            if (cmbAmbiente.SelectedItem == null) { MessageBox.Show("Seleccione el Ambiente."); return false; }
            if (cmbEmision.SelectedItem == null) { MessageBox.Show("Seleccione Tipo de Emisión."); return false; }
            if (cmbAgenteR.SelectedItem == null) { MessageBox.Show("Seleccione Agente de Retención."); return false; }
            if (cmbContribuyenteR.SelectedItem == null) { MessageBox.Show("Seleccione Contribuyente RIMPE."); return false; }
            if (cmbCodDoc.SelectedItem == null) { MessageBox.Show("Seleccione Documento de Emisión."); return false; }
            if (string.IsNullOrWhiteSpace(cmbEstablecimiento.Text)) { MessageBox.Show("Ingrese Establecimiento."); return false; }
            if (string.IsNullOrWhiteSpace(cmbPuntoE.Text)) { MessageBox.Show("Ingrese Punto de Emisión."); return false; }
            if (cmbContabilidad.SelectedItem == null) { MessageBox.Show("Seleccione Obligado a Contabilidad."); return false; }
            if (cmbTipoIdentificacion.SelectedItem == null) { MessageBox.Show("Seleccione Tipo de Identificación del Comprador."); return false; }
            if (cmbMoneda.SelectedItem == null) { MessageBox.Show("Seleccione la Moneda."); return false; }
            if (cmbTipoImpuesto.SelectedItem == null) { MessageBox.Show("Seleccione Tipo de Impuesto."); return false; }
            if (cmbPorcentaje.SelectedItem == null) { MessageBox.Show("Seleccione Porcentaje."); return false; }

            // Validar establecimiento
            if (cmbEstablecimiento.Text.Length != 3 || !int.TryParse(cmbEstablecimiento.Text, out _))
            {
                MessageBox.Show("El Establecimiento debe tener exactamente 3 dígitos numéricos.");
                return false;
            }

            // Validar punto de emisión
            if (cmbPuntoE.Text.Length != 3 || !int.TryParse(cmbPuntoE.Text, out _))
            {
                MessageBox.Show("El Punto de Emisión debe tener exactamente 3 dígitos numéricos.");
                return false;
            }

            bool aplicaContribuyenteEspecial = string.Equals(cmbContribuyenteE.Text, "SI", StringComparison.OrdinalIgnoreCase);
            string contribuyenteEspecial = ObtenerContribuyenteEspecial();

            if (aplicaContribuyenteEspecial && string.IsNullOrWhiteSpace(contribuyenteEspecial))
            {
                MessageBox.Show("Ingrese el numero de contribuyente especial.");
                return false;
            }

            if (!aplicaContribuyenteEspecial)
            {
                contribuyenteEspecial = "";
            }

            if (!string.IsNullOrWhiteSpace(contribuyenteEspecial))
            {
                if (!int.TryParse(contribuyenteEspecial, out _))
                {
                    MessageBox.Show("El Contribuyente Especial debe ser numérico cuando aplique.");
                    return false;
                }

                if (contribuyenteEspecial.Length < 3 || contribuyenteEspecial.Length > 13)
                {
                    MessageBox.Show("El Contribuyente Especial debe tener entre 3 y 13 dígitos cuando aplique.");
                    return false;
                }
            }

            // Validar número de dígitos del secuencial
            if (!int.TryParse(txtNumeroDigitos.Text.Trim(), out int digitos) || digitos < 1 || digitos > 9)
            {
                MessageBox.Show("Número de dígitos inválido. Debe ser entre 1 y 9.");
                return false;
            }

            // Validar fecha
            if (!DateTime.TryParse(txtFechaActualizacion.Text, out _))
            {
                MessageBox.Show("Fecha de Actualización inválida.");
                return false;
            }

            return true;
        }

        private string ObtenerContribuyenteEspecial()
        {
            if (!string.Equals(cmbContribuyenteE.Text, "SI", StringComparison.OrdinalIgnoreCase))
                return "";

            string texto = txtContribuyenteEspecial.Text.Trim();
            if (string.Equals(texto, "VACIO / NO APLICA", StringComparison.OrdinalIgnoreCase))
                return "";

            return texto;
        }


        // =========================================================
        // 8️⃣ CLASE PARA COMBO ITEM
        // =========================================================
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
