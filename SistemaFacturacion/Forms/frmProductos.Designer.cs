namespace SistemaFacturacion
{
    partial class frmProductos
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.gvProductos = new System.Windows.Forms.DataGridView();
            this.NOMBRE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNombreBuscar = new System.Windows.Forms.TextBox();
            this.pnlDatos = new System.Windows.Forms.Panel();
            this.label23 = new System.Windows.Forms.Label();
            this.txtValorSinIVA = new System.Windows.Forms.TextBox();
            this.cmbTipoICE = new System.Windows.Forms.ComboBox();
            this.cmbTipoVehiculoICE = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txtPVPICE = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtVolumenICE = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtAzucarICE = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtUnidadICE = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtValorICE = new System.Windows.Forms.TextBox();
            this.txtPorcentajeICE = new System.Windows.Forms.TextBox();
            this.txtCodigoICE = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbAplicaICE = new System.Windows.Forms.ComboBox();
            this.cmbIva = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtCodigo = new System.Windows.Forms.TextBox();
            this.btnModificar = new System.Windows.Forms.Button();
            this.cmbEstado = new System.Windows.Forms.ComboBox();
            this.txtOrden = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtValor = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.btnLimpiar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gvProductos)).BeginInit();
            this.pnlDatos.SuspendLayout();
            this.SuspendLayout();
            // 
            // gvProductos
            // 
            this.gvProductos.AllowUserToAddRows = false;
            this.gvProductos.AllowUserToResizeRows = false;
            this.gvProductos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvProductos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NOMBRE,
            this.Column2,
            this.Column3,
            this.Column4});
            this.gvProductos.Location = new System.Drawing.Point(9, 57);
            this.gvProductos.Margin = new System.Windows.Forms.Padding(2);
            this.gvProductos.Name = "gvProductos";
            this.gvProductos.RowTemplate.Height = 24;
            this.gvProductos.Size = new System.Drawing.Size(859, 212);
            this.gvProductos.TabIndex = 0;
            this.gvProductos.SelectionChanged += new System.EventHandler(this.gvProductos_SelectionChanged);
            // 
            // NOMBRE
            // 
            this.NOMBRE.DataPropertyName = "NOMBRE";
            this.NOMBRE.HeaderText = "NOMBRE";
            this.NOMBRE.Name = "NOMBRE";
            this.NOMBRE.Width = 260;
            // 
            // Column2
            // 
            this.Column2.DataPropertyName = "VALOR";
            dataGridViewCellStyle2.Format = "N2";
            this.Column2.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column2.HeaderText = "VALOR";
            this.Column2.Name = "Column2";
            this.Column2.Width = 50;
            // 
            // Column3
            // 
            this.Column3.DataPropertyName = "ESTATUS";
            this.Column3.HeaderText = "ESTATUS";
            this.Column3.Name = "Column3";
            this.Column3.Width = 80;
            // 
            // Column4
            // 
            this.Column4.DataPropertyName = "ORDEN";
            this.Column4.HeaderText = "ORDEN";
            this.Column4.Name = "Column4";
            this.Column4.Width = 50;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(144, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Nombre:";
            // 
            // txtNombreBuscar
            // 
            this.txtNombreBuscar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombreBuscar.Location = new System.Drawing.Point(214, 23);
            this.txtNombreBuscar.Margin = new System.Windows.Forms.Padding(2);
            this.txtNombreBuscar.Name = "txtNombreBuscar";
            this.txtNombreBuscar.Size = new System.Drawing.Size(392, 26);
            this.txtNombreBuscar.TabIndex = 3;
            // 
            // pnlDatos
            // 
            this.pnlDatos.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlDatos.Controls.Add(this.label23);
            this.pnlDatos.Controls.Add(this.txtValorSinIVA);
            this.pnlDatos.Controls.Add(this.cmbTipoICE);
            this.pnlDatos.Controls.Add(this.cmbTipoVehiculoICE);
            this.pnlDatos.Controls.Add(this.label22);
            this.pnlDatos.Controls.Add(this.label21);
            this.pnlDatos.Controls.Add(this.label20);
            this.pnlDatos.Controls.Add(this.label19);
            this.pnlDatos.Controls.Add(this.label17);
            this.pnlDatos.Controls.Add(this.label18);
            this.pnlDatos.Controls.Add(this.txtPVPICE);
            this.pnlDatos.Controls.Add(this.label15);
            this.pnlDatos.Controls.Add(this.txtVolumenICE);
            this.pnlDatos.Controls.Add(this.label16);
            this.pnlDatos.Controls.Add(this.txtAzucarICE);
            this.pnlDatos.Controls.Add(this.label14);
            this.pnlDatos.Controls.Add(this.txtUnidadICE);
            this.pnlDatos.Controls.Add(this.label13);
            this.pnlDatos.Controls.Add(this.label12);
            this.pnlDatos.Controls.Add(this.label11);
            this.pnlDatos.Controls.Add(this.label10);
            this.pnlDatos.Controls.Add(this.txtValorICE);
            this.pnlDatos.Controls.Add(this.txtPorcentajeICE);
            this.pnlDatos.Controls.Add(this.txtCodigoICE);
            this.pnlDatos.Controls.Add(this.label9);
            this.pnlDatos.Controls.Add(this.cmbAplicaICE);
            this.pnlDatos.Controls.Add(this.cmbIva);
            this.pnlDatos.Controls.Add(this.label7);
            this.pnlDatos.Controls.Add(this.label6);
            this.pnlDatos.Controls.Add(this.txtCodigo);
            this.pnlDatos.Controls.Add(this.btnModificar);
            this.pnlDatos.Controls.Add(this.cmbEstado);
            this.pnlDatos.Controls.Add(this.txtOrden);
            this.pnlDatos.Controls.Add(this.label5);
            this.pnlDatos.Controls.Add(this.label4);
            this.pnlDatos.Controls.Add(this.txtValor);
            this.pnlDatos.Controls.Add(this.label3);
            this.pnlDatos.Controls.Add(this.txtNombre);
            this.pnlDatos.Controls.Add(this.label2);
            this.pnlDatos.Controls.Add(this.btnEliminar);
            this.pnlDatos.Controls.Add(this.btnGuardar);
            this.pnlDatos.Location = new System.Drawing.Point(9, 274);
            this.pnlDatos.Margin = new System.Windows.Forms.Padding(2);
            this.pnlDatos.Name = "pnlDatos";
            this.pnlDatos.Size = new System.Drawing.Size(859, 328);
            this.pnlDatos.TabIndex = 4;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(334, 83);
            this.label23.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(70, 13);
            this.label23.TabIndex = 51;
            this.label23.Text = "Valor sin IVA:";
            // 
            // txtValorSinIVA
            // 
            this.txtValorSinIVA.Location = new System.Drawing.Point(337, 98);
            this.txtValorSinIVA.Margin = new System.Windows.Forms.Padding(2);
            this.txtValorSinIVA.Name = "txtValorSinIVA";
            this.txtValorSinIVA.Size = new System.Drawing.Size(103, 20);
            this.txtValorSinIVA.TabIndex = 50;
            // 
            // cmbTipoICE
            // 
            this.cmbTipoICE.FormattingEnabled = true;
            this.cmbTipoICE.Location = new System.Drawing.Point(210, 144);
            this.cmbTipoICE.Margin = new System.Windows.Forms.Padding(2);
            this.cmbTipoICE.Name = "cmbTipoICE";
            this.cmbTipoICE.Size = new System.Drawing.Size(138, 21);
            this.cmbTipoICE.TabIndex = 49;
            // 
            // cmbTipoVehiculoICE
            // 
            this.cmbTipoVehiculoICE.FormattingEnabled = true;
            this.cmbTipoVehiculoICE.Location = new System.Drawing.Point(586, 235);
            this.cmbTipoVehiculoICE.Margin = new System.Windows.Forms.Padding(2);
            this.cmbTipoVehiculoICE.Name = "cmbTipoVehiculoICE";
            this.cmbTipoVehiculoICE.Size = new System.Drawing.Size(143, 21);
            this.cmbTipoVehiculoICE.TabIndex = 48;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(429, 191);
            this.label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(258, 21);
            this.label22.TabIndex = 47;
            this.label22.Text = "Datos Adicionales Vehiculos ICE:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(4, 191);
            this.label21.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(243, 21);
            this.label21.TabIndex = 46;
            this.label21.Text = "Datos Adicionales Bebidas ICE:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(4, 98);
            this.label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(114, 21);
            this.label20.TabIndex = 45;
            this.label20.Text = "Datos del ICE:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(5, 7);
            this.label19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(160, 21);
            this.label19.TabIndex = 44;
            this.label19.Text = "Datos del Producto:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(581, 221);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(75, 13);
            this.label17.TabIndex = 43;
            this.label17.Text = "Tipo Vehiculo:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(432, 221);
            this.label18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(120, 13);
            this.label18.TabIndex = 41;
            this.label18.Text = "Precio Venta al Publico:";
            // 
            // txtPVPICE
            // 
            this.txtPVPICE.Location = new System.Drawing.Point(433, 237);
            this.txtPVPICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtPVPICE.Name = "txtPVPICE";
            this.txtPVPICE.Size = new System.Drawing.Size(141, 20);
            this.txtPVPICE.TabIndex = 40;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(115, 222);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(92, 13);
            this.label15.TabIndex = 39;
            this.label15.Text = "Volumen Bebidas:";
            // 
            // txtVolumenICE
            // 
            this.txtVolumenICE.Location = new System.Drawing.Point(118, 236);
            this.txtVolumenICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtVolumenICE.Name = "txtVolumenICE";
            this.txtVolumenICE.Size = new System.Drawing.Size(103, 20);
            this.txtVolumenICE.TabIndex = 38;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(2, 221);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(84, 13);
            this.label16.TabIndex = 37;
            this.label16.Text = "Azúcar Bebidas:";
            // 
            // txtAzucarICE
            // 
            this.txtAzucarICE.Location = new System.Drawing.Point(3, 237);
            this.txtAzucarICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtAzucarICE.Name = "txtAzucarICE";
            this.txtAzucarICE.Size = new System.Drawing.Size(103, 20);
            this.txtAzucarICE.TabIndex = 36;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(583, 130);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(64, 13);
            this.label14.TabIndex = 35;
            this.label14.Text = "Unidad ICE:";
            // 
            // txtUnidadICE
            // 
            this.txtUnidadICE.Location = new System.Drawing.Point(586, 144);
            this.txtUnidadICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtUnidadICE.Name = "txtUnidadICE";
            this.txtUnidadICE.Size = new System.Drawing.Size(103, 20);
            this.txtUnidadICE.TabIndex = 34;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(470, 129);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(54, 13);
            this.label13.TabIndex = 33;
            this.label13.Text = "Valor ICE:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(350, 130);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(81, 13);
            this.label12.TabIndex = 32;
            this.label12.Text = "Porcentaje ICE:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(225, 129);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(66, 13);
            this.label11.TabIndex = 31;
            this.label11.Text = "Tipo de ICE:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(101, 129);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(63, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Codigo ICE:";
            // 
            // txtValorICE
            // 
            this.txtValorICE.Location = new System.Drawing.Point(471, 145);
            this.txtValorICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtValorICE.Name = "txtValorICE";
            this.txtValorICE.Size = new System.Drawing.Size(103, 20);
            this.txtValorICE.TabIndex = 29;
            // 
            // txtPorcentajeICE
            // 
            this.txtPorcentajeICE.Location = new System.Drawing.Point(351, 145);
            this.txtPorcentajeICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtPorcentajeICE.Name = "txtPorcentajeICE";
            this.txtPorcentajeICE.Size = new System.Drawing.Size(103, 20);
            this.txtPorcentajeICE.TabIndex = 28;
            // 
            // txtCodigoICE
            // 
            this.txtCodigoICE.Location = new System.Drawing.Point(101, 145);
            this.txtCodigoICE.Margin = new System.Windows.Forms.Padding(2);
            this.txtCodigoICE.Name = "txtCodigoICE";
            this.txtCodigoICE.Size = new System.Drawing.Size(103, 20);
            this.txtCodigoICE.TabIndex = 26;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 129);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Aplica ICE:";
            // 
            // cmbAplicaICE
            // 
            this.cmbAplicaICE.FormattingEnabled = true;
            this.cmbAplicaICE.Location = new System.Drawing.Point(2, 144);
            this.cmbAplicaICE.Margin = new System.Windows.Forms.Padding(2);
            this.cmbAplicaICE.Name = "cmbAplicaICE";
            this.cmbAplicaICE.Size = new System.Drawing.Size(77, 21);
            this.cmbAplicaICE.TabIndex = 24;
            // 
            // cmbIva
            // 
            this.cmbIva.FormattingEnabled = true;
            this.cmbIva.Location = new System.Drawing.Point(782, 54);
            this.cmbIva.Margin = new System.Windows.Forms.Padding(2);
            this.cmbIva.Name = "cmbIva";
            this.cmbIva.Size = new System.Drawing.Size(45, 21);
            this.cmbIva.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(779, 41);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "IVA";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(657, 38);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Código:";
            // 
            // txtCodigo
            // 
            this.txtCodigo.Location = new System.Drawing.Point(658, 55);
            this.txtCodigo.Margin = new System.Windows.Forms.Padding(2);
            this.txtCodigo.Name = "txtCodigo";
            this.txtCodigo.Size = new System.Drawing.Size(108, 20);
            this.txtCodigo.TabIndex = 20;
            // 
            // btnModificar
            // 
            this.btnModificar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModificar.Location = new System.Drawing.Point(373, 270);
            this.btnModificar.Margin = new System.Windows.Forms.Padding(2);
            this.btnModificar.Name = "btnModificar";
            this.btnModificar.Size = new System.Drawing.Size(132, 43);
            this.btnModificar.TabIndex = 18;
            this.btnModificar.Text = "MODIFICAR";
            this.btnModificar.UseVisualStyleBackColor = true;
            this.btnModificar.Click += new System.EventHandler(this.btnModificar_Click_1);
            // 
            // cmbEstado
            // 
            this.cmbEstado.FormattingEnabled = true;
            this.cmbEstado.Location = new System.Drawing.Point(444, 56);
            this.cmbEstado.Margin = new System.Windows.Forms.Padding(2);
            this.cmbEstado.Name = "cmbEstado";
            this.cmbEstado.Size = new System.Drawing.Size(116, 21);
            this.cmbEstado.TabIndex = 17;
            // 
            // txtOrden
            // 
            this.txtOrden.Location = new System.Drawing.Point(564, 56);
            this.txtOrden.Margin = new System.Windows.Forms.Padding(2);
            this.txtOrden.Name = "txtOrden";
            this.txtOrden.Size = new System.Drawing.Size(81, 20);
            this.txtOrden.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(561, 40);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Orden:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(441, 40);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Estatus:";
            // 
            // txtValor
            // 
            this.txtValor.Location = new System.Drawing.Point(337, 56);
            this.txtValor.Margin = new System.Windows.Forms.Padding(2);
            this.txtValor.Name = "txtValor";
            this.txtValor.Size = new System.Drawing.Size(103, 20);
            this.txtValor.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(334, 41);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Valor:";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(3, 57);
            this.txtNombre.Margin = new System.Windows.Forms.Padding(2);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(325, 20);
            this.txtNombre.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 41);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Nombre:";
            // 
            // btnEliminar
            // 
            this.btnEliminar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEliminar.Location = new System.Drawing.Point(509, 270);
            this.btnEliminar.Margin = new System.Windows.Forms.Padding(2);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(116, 43);
            this.btnEliminar.TabIndex = 8;
            this.btnEliminar.Text = "ELIMINAR";
            this.btnEliminar.UseVisualStyleBackColor = true;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(256, 270);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(2);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(113, 43);
            this.btnGuardar.TabIndex = 6;
            this.btnGuardar.Text = "GUARDAR";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnNuevo
            // 
            this.btnNuevo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNuevo.Location = new System.Drawing.Point(714, 20);
            this.btnNuevo.Margin = new System.Windows.Forms.Padding(2);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnNuevo.Size = new System.Drawing.Size(92, 32);
            this.btnNuevo.TabIndex = 5;
            this.btnNuevo.Text = "NUEVO";
            this.btnNuevo.UseVisualStyleBackColor = true;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(144, 26);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(69, 20);
            this.label8.TabIndex = 2;
            this.label8.Text = "Nombre:";
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.ForeColor = System.Drawing.Color.Red;
            this.btnLimpiar.Location = new System.Drawing.Point(623, 23);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(33, 29);
            this.btnLimpiar.TabIndex = 6;
            this.btnLimpiar.Text = "X";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            // 
            // frmProductos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 613);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.pnlDatos);
            this.Controls.Add(this.txtNombreBuscar);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gvProductos);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmProductos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PRODUCTOS";
            this.Load += new System.EventHandler(this.frmProductos_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gvProductos)).EndInit();
            this.pnlDatos.ResumeLayout(false);
            this.pnlDatos.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gvProductos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNombreBuscar;
        private System.Windows.Forms.Panel pnlDatos;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOrden;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtValor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.ComboBox cmbEstado;
        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.DataGridViewTextBoxColumn NOMBRE;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.Button btnModificar;
        private System.Windows.Forms.TextBox txtCodigo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbIva;
        private System.Windows.Forms.ComboBox cmbAplicaICE;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtPVPICE;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtVolumenICE;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtAzucarICE;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtUnidadICE;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtValorICE;
        private System.Windows.Forms.TextBox txtPorcentajeICE;
        private System.Windows.Forms.TextBox txtCodigoICE;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbTipoVehiculoICE;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ComboBox cmbTipoICE;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox txtValorSinIVA;
        private System.Windows.Forms.Button btnLimpiar;
    }
}