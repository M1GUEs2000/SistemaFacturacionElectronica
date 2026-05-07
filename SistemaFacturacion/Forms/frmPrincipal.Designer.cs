namespace SistemaFacturacion
{
    partial class frmPrincipal
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPrincipal));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource2 = new Microsoft.Reporting.WinForms.ReportDataSource();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            this.TB_VENTABindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dsVenta = new SistemaFacturacion.dsVenta();
            this.lblNombreEmpresa = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.lbletiqueta = new System.Windows.Forms.Label();
            this.lblHora = new System.Windows.Forms.Label();
            this.gvProductos = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NOMBRE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtFormaPago = new System.Windows.Forms.TextBox();
            this.panelFormasPago = new System.Windows.Forms.Panel();
            this.bntTarjeta = new System.Windows.Forms.Button();
            this.bntDeUna = new System.Windows.Forms.Button();
            this.btnEfectivo = new System.Windows.Forms.Button();
            this.pnlVariosProductos = new System.Windows.Forms.Panel();
            this.lstClientes = new System.Windows.Forms.ListBox();
            this.button5 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.txtComentario = new System.Windows.Forms.TextBox();
            this.btnConsumidor = new System.Windows.Forms.Button();
            this.lblNumeroFactura = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnClientes = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblImpresion = new System.Windows.Forms.Label();
            this.lblFactura = new System.Windows.Forms.Label();
            this.gvTransacccionesFacturadas = new System.Windows.Forms.DataGridView();
            this.CNT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VLR = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PROD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TOT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FORMA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HORA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.gvDetalleFactura = new System.Windows.Forms.DataGridView();
            this.CANTIDAD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VALOR = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PRODUCTO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TOTAL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ELIMINAR = new System.Windows.Forms.DataGridViewImageColumn();
            this.label9 = new System.Windows.Forms.Label();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTotalVP = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button3 = new System.Windows.Forms.Button();
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.button4 = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnProcesar = new System.Windows.Forms.Button();
            this.bntTablas = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRetenciones = new System.Windows.Forms.Button();
            this.btnPDF = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.txtBuscarProductos = new System.Windows.Forms.TextBox();
            this.btnLimpiarProducto = new System.Windows.Forms.Button();
            this.pnlLabel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.TB_VENTABindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsVenta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvProductos)).BeginInit();
            this.panelFormasPago.SuspendLayout();
            this.pnlVariosProductos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvTransacccionesFacturadas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDetalleFactura)).BeginInit();
            this.panel1.SuspendLayout();
            this.pnlLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TB_VENTABindingSource
            // 
            this.TB_VENTABindingSource.DataMember = "TB_VENTA";
            this.TB_VENTABindingSource.DataSource = this.dsVenta;
            // 
            // dsVenta
            // 
            this.dsVenta.DataSetName = "dsVenta";
            this.dsVenta.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // lblNombreEmpresa
            // 
            this.lblNombreEmpresa.AutoSize = true;
            this.lblNombreEmpresa.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreEmpresa.ForeColor = System.Drawing.Color.Orange;
            this.lblNombreEmpresa.Location = new System.Drawing.Point(35, 3);
            this.lblNombreEmpresa.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblNombreEmpresa.Name = "lblNombreEmpresa";
            this.lblNombreEmpresa.Size = new System.Drawing.Size(132, 46);
            this.lblNombreEmpresa.TabIndex = 1;
            this.lblNombreEmpresa.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label1.Location = new System.Drawing.Point(2, 50);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 31);
            this.label1.TabIndex = 3;
            this.label1.Text = "FECHA:";
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFecha.Location = new System.Drawing.Point(118, 45);
            this.lblFecha.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(115, 39);
            this.lblFecha.TabIndex = 4;
            this.lblFecha.Text = "label2";
            // 
            // lbletiqueta
            // 
            this.lbletiqueta.AutoSize = true;
            this.lbletiqueta.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbletiqueta.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lbletiqueta.Location = new System.Drawing.Point(352, 50);
            this.lbletiqueta.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbletiqueta.Name = "lbletiqueta";
            this.lbletiqueta.Size = new System.Drawing.Size(106, 31);
            this.lbletiqueta.TabIndex = 7;
            this.lbletiqueta.Text = "HORA:";
            // 
            // lblHora
            // 
            this.lblHora.AutoSize = true;
            this.lblHora.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHora.Location = new System.Drawing.Point(462, 42);
            this.lblHora.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHora.Name = "lblHora";
            this.lblHora.Size = new System.Drawing.Size(115, 39);
            this.lblHora.TabIndex = 8;
            this.lblHora.Text = "label3";
            // 
            // gvProductos
            // 
            this.gvProductos.AllowUserToAddRows = false;
            this.gvProductos.AllowUserToResizeColumns = false;
            dataGridViewCellStyle16.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvProductos.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle16;
            this.gvProductos.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle17.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvProductos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle17;
            this.gvProductos.ColumnHeadersHeight = 40;
            this.gvProductos.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.NOMBRE});
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle18.Format = "N2";
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gvProductos.DefaultCellStyle = dataGridViewCellStyle18;
            this.gvProductos.EnableHeadersVisualStyles = false;
            this.gvProductos.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.gvProductos.Location = new System.Drawing.Point(8, 234);
            this.gvProductos.Margin = new System.Windows.Forms.Padding(2);
            this.gvProductos.Name = "gvProductos";
            this.gvProductos.ReadOnly = true;
            this.gvProductos.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvProductos.RowHeadersDefaultCellStyle = dataGridViewCellStyle19;
            this.gvProductos.RowHeadersWidth = 30;
            dataGridViewCellStyle20.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvProductos.RowsDefaultCellStyle = dataGridViewCellStyle20;
            this.gvProductos.RowTemplate.Height = 40;
            this.gvProductos.Size = new System.Drawing.Size(603, 420);
            this.gvProductos.TabIndex = 9;
            this.gvProductos.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvProductos_CellClick);
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "NOMBRE";
            this.Column1.HeaderText = "NOMBRE";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 300;
            // 
            // NOMBRE
            // 
            this.NOMBRE.DataPropertyName = "VALOR";
            this.NOMBRE.HeaderText = "VALOR";
            this.NOMBRE.Name = "NOMBRE";
            this.NOMBRE.ReadOnly = true;
            // 
            // txtFormaPago
            // 
            this.txtFormaPago.Enabled = false;
            this.txtFormaPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFormaPago.Location = new System.Drawing.Point(463, 63);
            this.txtFormaPago.Margin = new System.Windows.Forms.Padding(2);
            this.txtFormaPago.Name = "txtFormaPago";
            this.txtFormaPago.Size = new System.Drawing.Size(135, 29);
            this.txtFormaPago.TabIndex = 2;
            // 
            // panelFormasPago
            // 
            this.panelFormasPago.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.panelFormasPago.Controls.Add(this.bntTarjeta);
            this.panelFormasPago.Controls.Add(this.bntDeUna);
            this.panelFormasPago.Controls.Add(this.btnEfectivo);
            this.panelFormasPago.Location = new System.Drawing.Point(8, 91);
            this.panelFormasPago.Margin = new System.Windows.Forms.Padding(2);
            this.panelFormasPago.Name = "panelFormasPago";
            this.panelFormasPago.Size = new System.Drawing.Size(605, 115);
            this.panelFormasPago.TabIndex = 12;
            // 
            // bntTarjeta
            // 
            this.bntTarjeta.Location = new System.Drawing.Point(0, 0);
            this.bntTarjeta.Name = "bntTarjeta";
            this.bntTarjeta.Size = new System.Drawing.Size(75, 23);
            this.bntTarjeta.TabIndex = 22;
            // 
            // bntDeUna
            // 
            this.bntDeUna.Location = new System.Drawing.Point(0, 0);
            this.bntDeUna.Name = "bntDeUna";
            this.bntDeUna.Size = new System.Drawing.Size(75, 23);
            this.bntDeUna.TabIndex = 23;
            // 
            // btnEfectivo
            // 
            this.btnEfectivo.Location = new System.Drawing.Point(0, 0);
            this.btnEfectivo.Name = "btnEfectivo";
            this.btnEfectivo.Size = new System.Drawing.Size(75, 23);
            this.btnEfectivo.TabIndex = 24;
            // 
            // pnlVariosProductos
            // 
            this.pnlVariosProductos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlVariosProductos.Controls.Add(this.lstClientes);
            this.pnlVariosProductos.Controls.Add(this.button5);
            this.pnlVariosProductos.Controls.Add(this.label10);
            this.pnlVariosProductos.Controls.Add(this.txtComentario);
            this.pnlVariosProductos.Controls.Add(this.btnConsumidor);
            this.pnlVariosProductos.Controls.Add(this.lblNumeroFactura);
            this.pnlVariosProductos.Controls.Add(this.label7);
            this.pnlVariosProductos.Controls.Add(this.btnClientes);
            this.pnlVariosProductos.Controls.Add(this.label6);
            this.pnlVariosProductos.Controls.Add(this.label5);
            this.pnlVariosProductos.Controls.Add(this.lblImpresion);
            this.pnlVariosProductos.Controls.Add(this.lblFactura);
            this.pnlVariosProductos.Controls.Add(this.gvTransacccionesFacturadas);
            this.pnlVariosProductos.Controls.Add(this.label4);
            this.pnlVariosProductos.Controls.Add(this.reportViewer1);
            this.pnlVariosProductos.Controls.Add(this.gvDetalleFactura);
            this.pnlVariosProductos.Controls.Add(this.label9);
            this.pnlVariosProductos.Controls.Add(this.txtCliente);
            this.pnlVariosProductos.Controls.Add(this.label8);
            this.pnlVariosProductos.Controls.Add(this.label2);
            this.pnlVariosProductos.Controls.Add(this.txtFormaPago);
            this.pnlVariosProductos.ForeColor = System.Drawing.Color.Black;
            this.pnlVariosProductos.Location = new System.Drawing.Point(627, 10);
            this.pnlVariosProductos.Margin = new System.Windows.Forms.Padding(2);
            this.pnlVariosProductos.Name = "pnlVariosProductos";
            this.pnlVariosProductos.Size = new System.Drawing.Size(618, 653);
            this.pnlVariosProductos.TabIndex = 18;
            // 
            // lstClientes
            // 
            this.lstClientes.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstClientes.FormattingEnabled = true;
            this.lstClientes.ItemHeight = 21;
            this.lstClientes.Location = new System.Drawing.Point(83, 93);
            this.lstClientes.Name = "lstClientes";
            this.lstClientes.Size = new System.Drawing.Size(361, 214);
            this.lstClientes.TabIndex = 36;
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Location = new System.Drawing.Point(92, 3);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(72, 57);
            this.button5.TabIndex = 34;
            this.button5.Text = "C.F\r\nSRI";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(332, 259);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(84, 17);
            this.label10.TabIndex = 33;
            this.label10.Text = "Comentario:";
            // 
            // txtComentario
            // 
            this.txtComentario.Location = new System.Drawing.Point(413, 259);
            this.txtComentario.Name = "txtComentario";
            this.txtComentario.Size = new System.Drawing.Size(185, 20);
            this.txtComentario.TabIndex = 32;
            // 
            // btnConsumidor
            // 
            this.btnConsumidor.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConsumidor.Location = new System.Drawing.Point(3, 3);
            this.btnConsumidor.Name = "btnConsumidor";
            this.btnConsumidor.Size = new System.Drawing.Size(72, 57);
            this.btnConsumidor.TabIndex = 31;
            this.btnConsumidor.Text = "C.F";
            this.btnConsumidor.UseVisualStyleBackColor = true;
            this.btnConsumidor.Click += new System.EventHandler(this.btnConsumidor_Click);
            // 
            // lblNumeroFactura
            // 
            this.lblNumeroFactura.AutoSize = true;
            this.lblNumeroFactura.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumeroFactura.Location = new System.Drawing.Point(183, 259);
            this.lblNumeroFactura.Name = "lblNumeroFactura";
            this.lblNumeroFactura.Size = new System.Drawing.Size(60, 20);
            this.lblNumeroFactura.TabIndex = 30;
            this.lblNumeroFactura.Text = "label10";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(19, 259);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(168, 20);
            this.label7.TabIndex = 29;
            this.label7.Text = "Número de Factura:";
            // 
            // btnClientes
            // 
            this.btnClientes.BackColor = System.Drawing.Color.Transparent;
            this.btnClientes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnClientes.Image = ((System.Drawing.Image)(resources.GetObject("btnClientes.Image")));
            this.btnClientes.Location = new System.Drawing.Point(240, 61);
            this.btnClientes.Name = "btnClientes";
            this.btnClientes.Size = new System.Drawing.Size(57, 39);
            this.btnClientes.TabIndex = 28;
            this.btnClientes.UseVisualStyleBackColor = false;
            this.btnClientes.Click += new System.EventHandler(this.btnClientes_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Blue;
            this.label6.Location = new System.Drawing.Point(460, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 17);
            this.label6.TabIndex = 27;
            this.label6.Text = "IMPRIMIR:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(460, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 17);
            this.label5.TabIndex = 26;
            this.label5.Text = "Facturacion:";
            // 
            // lblImpresion
            // 
            this.lblImpresion.AutoSize = true;
            this.lblImpresion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImpresion.ForeColor = System.Drawing.Color.Blue;
            this.lblImpresion.Location = new System.Drawing.Point(555, 31);
            this.lblImpresion.Name = "lblImpresion";
            this.lblImpresion.Size = new System.Drawing.Size(52, 17);
            this.lblImpresion.TabIndex = 25;
            this.lblImpresion.Text = "label6";
            // 
            // lblFactura
            // 
            this.lblFactura.AutoSize = true;
            this.lblFactura.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFactura.ForeColor = System.Drawing.Color.Blue;
            this.lblFactura.Location = new System.Drawing.Point(555, 12);
            this.lblFactura.Name = "lblFactura";
            this.lblFactura.Size = new System.Drawing.Size(43, 17);
            this.lblFactura.TabIndex = 24;
            this.lblFactura.Text = "label";
            // 
            // gvTransacccionesFacturadas
            // 
            this.gvTransacccionesFacturadas.AllowUserToAddRows = false;
            this.gvTransacccionesFacturadas.BackgroundColor = System.Drawing.SystemColors.ControlLight;
            this.gvTransacccionesFacturadas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvTransacccionesFacturadas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CNT,
            this.VLR,
            this.PROD,
            this.TOT,
            this.FORMA,
            this.HORA});
            this.gvTransacccionesFacturadas.GridColor = System.Drawing.SystemColors.Control;
            this.gvTransacccionesFacturadas.Location = new System.Drawing.Point(23, 126);
            this.gvTransacccionesFacturadas.Margin = new System.Windows.Forms.Padding(2);
            this.gvTransacccionesFacturadas.Name = "gvTransacccionesFacturadas";
            dataGridViewCellStyle23.BackColor = System.Drawing.Color.Gainsboro;
            this.gvTransacccionesFacturadas.RowsDefaultCellStyle = dataGridViewCellStyle23;
            this.gvTransacccionesFacturadas.RowTemplate.Height = 24;
            this.gvTransacccionesFacturadas.Size = new System.Drawing.Size(575, 122);
            this.gvTransacccionesFacturadas.TabIndex = 23;
            // 
            // CNT
            // 
            this.CNT.DataPropertyName = "CNT";
            this.CNT.HeaderText = "CNT";
            this.CNT.Name = "CNT";
            this.CNT.Width = 30;
            // 
            // VLR
            // 
            this.VLR.DataPropertyName = "VALOR";
            dataGridViewCellStyle21.Format = "N2";
            this.VLR.DefaultCellStyle = dataGridViewCellStyle21;
            this.VLR.HeaderText = "VALOR";
            this.VLR.Name = "VLR";
            this.VLR.Width = 50;
            // 
            // PROD
            // 
            this.PROD.DataPropertyName = "PRODUCTO";
            this.PROD.HeaderText = "PRODUCTO";
            this.PROD.Name = "PROD";
            this.PROD.Width = 180;
            // 
            // TOT
            // 
            this.TOT.DataPropertyName = "TOTAL";
            dataGridViewCellStyle22.Format = "N2";
            this.TOT.DefaultCellStyle = dataGridViewCellStyle22;
            this.TOT.HeaderText = "TOTAL";
            this.TOT.Name = "TOT";
            this.TOT.Width = 50;
            // 
            // FORMA
            // 
            this.FORMA.DataPropertyName = "FORMADEPAGO";
            this.FORMA.HeaderText = "FORMA DE PAGO";
            this.FORMA.Name = "FORMA";
            // 
            // HORA
            // 
            this.HORA.DataPropertyName = "HORA";
            this.HORA.HeaderText = "HORA";
            this.HORA.Name = "HORA";
            this.HORA.Width = 50;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(98, 103);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(366, 20);
            this.label4.TabIndex = 22;
            this.label4.Text = "ULTIMAS TRANSACCIONES FACTURADAS";
            // 
            // reportViewer1
            // 
            reportDataSource2.Name = "DataSet1";
            reportDataSource2.Value = this.TB_VENTABindingSource;
            this.reportViewer1.LocalReport.DataSources.Add(reportDataSource2);
            this.reportViewer1.LocalReport.ReportEmbeddedResource = "SistemaFacturacion.rptRecibo.rdlc";
            this.reportViewer1.Location = new System.Drawing.Point(52, 335);
            this.reportViewer1.Margin = new System.Windows.Forms.Padding(2);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.ServerReport.BearerToken = null;
            this.reportViewer1.Size = new System.Drawing.Size(515, 228);
            this.reportViewer1.TabIndex = 21;
            this.reportViewer1.Visible = false;
            // 
            // gvDetalleFactura
            // 
            this.gvDetalleFactura.AllowUserToAddRows = false;
            this.gvDetalleFactura.AllowUserToResizeColumns = false;
            dataGridViewCellStyle24.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvDetalleFactura.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle24;
            this.gvDetalleFactura.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle25.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle25.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle25.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle25.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle25.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle25.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvDetalleFactura.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle25;
            this.gvDetalleFactura.ColumnHeadersHeight = 40;
            this.gvDetalleFactura.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CANTIDAD,
            this.VALOR,
            this.PRODUCTO,
            this.TOTAL,
            this.ELIMINAR});
            dataGridViewCellStyle28.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle28.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle28.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle28.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle28.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle28.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle28.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gvDetalleFactura.DefaultCellStyle = dataGridViewCellStyle28;
            this.gvDetalleFactura.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.gvDetalleFactura.Location = new System.Drawing.Point(23, 294);
            this.gvDetalleFactura.Margin = new System.Windows.Forms.Padding(2);
            this.gvDetalleFactura.Name = "gvDetalleFactura";
            dataGridViewCellStyle29.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle29.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle29.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle29.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle29.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle29.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle29.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvDetalleFactura.RowHeadersDefaultCellStyle = dataGridViewCellStyle29;
            this.gvDetalleFactura.RowHeadersWidth = 30;
            dataGridViewCellStyle30.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle30.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvDetalleFactura.RowsDefaultCellStyle = dataGridViewCellStyle30;
            this.gvDetalleFactura.RowTemplate.Height = 40;
            this.gvDetalleFactura.Size = new System.Drawing.Size(575, 349);
            this.gvDetalleFactura.TabIndex = 5;
            this.gvDetalleFactura.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvDetalleFactura_CellClick);
            this.gvDetalleFactura.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvDetalleFactura_CellEndEdit);
            // 
            // CANTIDAD
            // 
            this.CANTIDAD.DataPropertyName = "CANTIDAD";
            this.CANTIDAD.HeaderText = "CANTIDAD";
            this.CANTIDAD.Name = "CANTIDAD";
            this.CANTIDAD.Width = 90;
            // 
            // VALOR
            // 
            this.VALOR.DataPropertyName = "VALOR";
            dataGridViewCellStyle26.Format = "N2";
            this.VALOR.DefaultCellStyle = dataGridViewCellStyle26;
            this.VALOR.HeaderText = "VALOR";
            this.VALOR.Name = "VALOR";
            this.VALOR.Width = 70;
            // 
            // PRODUCTO
            // 
            this.PRODUCTO.DataPropertyName = "PRODUCTO";
            this.PRODUCTO.HeaderText = "PRODUCTO";
            this.PRODUCTO.Name = "PRODUCTO";
            this.PRODUCTO.ReadOnly = true;
            this.PRODUCTO.Width = 150;
            // 
            // TOTAL
            // 
            this.TOTAL.DataPropertyName = "TOTAL";
            dataGridViewCellStyle27.Format = "N2";
            this.TOTAL.DefaultCellStyle = dataGridViewCellStyle27;
            this.TOTAL.HeaderText = "TOTAL";
            this.TOTAL.Name = "TOTAL";
            this.TOTAL.ReadOnly = true;
            // 
            // ELIMINAR
            // 
            this.ELIMINAR.HeaderText = "ELIMINAR";
            this.ELIMINAR.Image = ((System.Drawing.Image)(resources.GetObject("ELIMINAR.Image")));
            this.ELIMINAR.Name = "ELIMINAR";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label9.Location = new System.Drawing.Point(299, 66);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(160, 24);
            this.label9.TabIndex = 3;
            this.label9.Text = "Forma de Pago:";
            // 
            // txtCliente
            // 
            this.txtCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCliente.Location = new System.Drawing.Point(83, 65);
            this.txtCliente.Margin = new System.Windows.Forms.Padding(2);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.Size = new System.Drawing.Size(149, 29);
            this.txtCliente.TabIndex = 2;
            this.txtCliente.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCliente_KeyDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label8.Location = new System.Drawing.Point(8, 68);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 24);
            this.label8.TabIndex = 1;
            this.label8.Text = "Cliente:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Orange;
            this.label2.Location = new System.Drawing.Point(159, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(285, 39);
            this.label2.TabIndex = 0;
            this.label2.Text = "Detalle de Venta";
            // 
            // lblTotalVP
            // 
            this.lblTotalVP.BackColor = System.Drawing.Color.Yellow;
            this.lblTotalVP.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalVP.ForeColor = System.Drawing.Color.Black;
            this.lblTotalVP.Location = new System.Drawing.Point(819, 670);
            this.lblTotalVP.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTotalVP.Name = "lblTotalVP";
            this.lblTotalVP.Size = new System.Drawing.Size(185, 55);
            this.lblTotalVP.TabIndex = 7;
            this.lblTotalVP.Text = "label10";
            this.lblTotalVP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label3.Location = new System.Drawing.Point(11, 208);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 24);
            this.label3.TabIndex = 19;
            this.label3.Text = "PRODUCTOS:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(535, 10);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(76, 23);
            this.button3.TabIndex = 21;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.HeaderText = "ELIMINAR";
            this.dataGridViewImageColumn1.Image = ((System.Drawing.Image)(resources.GetObject("dataGridViewImageColumn1.Image")));
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            // 
            // button4
            // 
            this.button4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button4.BackgroundImage")));
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(326, 0);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(94, 67);
            this.button4.TabIndex = 22;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLimpiar.BackgroundImage")));
            this.btnLimpiar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLimpiar.Location = new System.Drawing.Point(1009, 670);
            this.btnLimpiar.Margin = new System.Windows.Forms.Padding(2);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(118, 55);
            this.btnLimpiar.TabIndex = 20;
            this.btnLimpiar.UseVisualStyleBackColor = true;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            // 
            // btnProcesar
            // 
            this.btnProcesar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnProcesar.BackgroundImage")));
            this.btnProcesar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnProcesar.Location = new System.Drawing.Point(630, 667);
            this.btnProcesar.Margin = new System.Windows.Forms.Padding(2);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(177, 61);
            this.btnProcesar.TabIndex = 9;
            this.btnProcesar.UseVisualStyleBackColor = true;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click);
            // 
            // bntTablas
            // 
            this.bntTablas.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("bntTablas.BackgroundImage")));
            this.bntTablas.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bntTablas.Location = new System.Drawing.Point(218, 1);
            this.bntTablas.Margin = new System.Windows.Forms.Padding(2);
            this.bntTablas.Name = "bntTablas";
            this.bntTablas.Size = new System.Drawing.Size(94, 67);
            this.bntTablas.TabIndex = 16;
            this.bntTablas.UseVisualStyleBackColor = true;
            this.bntTablas.Click += new System.EventHandler(this.bntTablas_Click);
            // 
            // button2
            // 
            this.button2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button2.BackgroundImage")));
            this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button2.Location = new System.Drawing.Point(116, 0);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(84, 67);
            this.button2.TabIndex = 15;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.Location = new System.Drawing.Point(12, 1);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 67);
            this.button1.TabIndex = 14;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSalir
            // 
            this.btnSalir.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSalir.BackgroundImage")));
            this.btnSalir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSalir.Location = new System.Drawing.Point(8, 658);
            this.btnSalir.Margin = new System.Windows.Forms.Padding(2);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(75, 67);
            this.btnSalir.TabIndex = 2;
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnRetenciones);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.button4);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.bntTablas);
            this.panel1.Location = new System.Drawing.Point(88, 660);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(534, 68);
            this.panel1.TabIndex = 23;
            // 
            // btnRetenciones
            // 
            this.btnRetenciones.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRetenciones.BackgroundImage")));
            this.btnRetenciones.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRetenciones.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRetenciones.Location = new System.Drawing.Point(429, 1);
            this.btnRetenciones.Margin = new System.Windows.Forms.Padding(2);
            this.btnRetenciones.Name = "btnRetenciones";
            this.btnRetenciones.Size = new System.Drawing.Size(94, 67);
            this.btnRetenciones.TabIndex = 23;
            this.btnRetenciones.UseVisualStyleBackColor = true;
            this.btnRetenciones.Click += new System.EventHandler(this.btnRetenciones_Click);
            // 
            // btnPDF
            // 
            this.btnPDF.Font = new System.Drawing.Font("Microsoft Tai Le", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPDF.Location = new System.Drawing.Point(1133, 670);
            this.btnPDF.Name = "btnPDF";
            this.btnPDF.Size = new System.Drawing.Size(111, 55);
            this.btnPDF.TabIndex = 24;
            this.btnPDF.Text = "Visualizar PDF";
            this.btnPDF.UseVisualStyleBackColor = true;
            this.btnPDF.Click += new System.EventHandler(this.btnPDF_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRefresh.BackgroundImage")));
            this.btnRefresh.Location = new System.Drawing.Point(575, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(36, 36);
            this.btnRefresh.TabIndex = 25;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click_1);
            // 
            // txtBuscarProductos
            // 
            this.txtBuscarProductos.Font = new System.Drawing.Font("Microsoft Tai Le", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBuscarProductos.Location = new System.Drawing.Point(153, 207);
            this.txtBuscarProductos.Name = "txtBuscarProductos";
            this.txtBuscarProductos.Size = new System.Drawing.Size(392, 26);
            this.txtBuscarProductos.TabIndex = 26;
            this.txtBuscarProductos.TextChanged += new System.EventHandler(this.txtBuscarProductos_TextChanged);
            // 
            // btnLimpiarProducto
            // 
            this.btnLimpiarProducto.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiarProducto.ForeColor = System.Drawing.Color.Red;
            this.btnLimpiarProducto.Location = new System.Drawing.Point(575, 207);
            this.btnLimpiarProducto.Name = "btnLimpiarProducto";
            this.btnLimpiarProducto.Size = new System.Drawing.Size(36, 26);
            this.btnLimpiarProducto.TabIndex = 27;
            this.btnLimpiarProducto.Text = "X";
            this.btnLimpiarProducto.UseVisualStyleBackColor = true;
            this.btnLimpiarProducto.Click += new System.EventHandler(this.btnLimpiarProducto_Click);
            // 
            // pnlLabel
            // 
            this.pnlLabel.Controls.Add(this.lblNombreEmpresa);
            this.pnlLabel.Location = new System.Drawing.Point(45, 4);
            this.pnlLabel.Name = "pnlLabel";
            this.pnlLabel.Size = new System.Drawing.Size(474, 46);
            this.pnlLabel.TabIndex = 28;
            // 
            // frmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1256, 736);
            this.Controls.Add(this.pnlLabel);
            this.Controls.Add(this.btnLimpiarProducto);
            this.Controls.Add(this.txtBuscarProductos);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnPDF);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.lblTotalVP);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnProcesar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pnlVariosProductos);
            this.Controls.Add(this.panelFormasPago);
            this.Controls.Add(this.gvProductos);
            this.Controls.Add(this.lblHora);
            this.Controls.Add(this.lbletiqueta);
            this.Controls.Add(this.lblFecha);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSalir);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SISTEMA DE FACTURACION";
            this.Load += new System.EventHandler(this.frmPrincipal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.TB_VENTABindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsVenta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvProductos)).EndInit();
            this.panelFormasPago.ResumeLayout(false);
            this.pnlVariosProductos.ResumeLayout(false);
            this.pnlVariosProductos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvTransacccionesFacturadas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDetalleFactura)).EndInit();
            this.panel1.ResumeLayout(false);
            this.pnlLabel.ResumeLayout(false);
            this.pnlLabel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNombreEmpresa;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lbletiqueta;
        private System.Windows.Forms.Label lblHora;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.DataGridView gvProductos;
        private System.Windows.Forms.Panel panelFormasPago;
        private System.Windows.Forms.Button btnEfectivo;
        private System.Windows.Forms.TextBox txtFormaPago;
        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.Button bntTarjeta;
        private System.Windows.Forms.Button bntDeUna;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button bntTablas;
        private System.Windows.Forms.Panel pnlVariosProductos;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCliente;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView gvDetalleFactura;
        private System.Windows.Forms.Label lblTotalVP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnLimpiar;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private System.Windows.Forms.BindingSource TB_VENTABindingSource;
        private dsVenta dsVenta;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridView gvTransacccionesFacturadas;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblFactura;
        private System.Windows.Forms.Label lblImpresion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnClientes;
        private System.Windows.Forms.Label lblNumeroFactura;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnConsumidor;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtComentario;
        private System.Windows.Forms.DataGridViewTextBoxColumn CANTIDAD;
        private System.Windows.Forms.DataGridViewTextBoxColumn VALOR;
        private System.Windows.Forms.DataGridViewTextBoxColumn PRODUCTO;
        private System.Windows.Forms.DataGridViewTextBoxColumn TOTAL;
        private System.Windows.Forms.DataGridViewImageColumn ELIMINAR;
        private System.Windows.Forms.DataGridViewTextBoxColumn CNT;
        private System.Windows.Forms.DataGridViewTextBoxColumn VLR;
        private System.Windows.Forms.DataGridViewTextBoxColumn PROD;
        private System.Windows.Forms.DataGridViewTextBoxColumn TOT;
        private System.Windows.Forms.DataGridViewTextBoxColumn FORMA;
        private System.Windows.Forms.DataGridViewTextBoxColumn HORA;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRetenciones;
        private System.Windows.Forms.Button btnPDF;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox txtBuscarProductos;
        private System.Windows.Forms.Button btnLimpiarProducto;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn NOMBRE;
        private System.Windows.Forms.ListBox lstClientes;
        private System.Windows.Forms.Panel pnlLabel;
    }
}