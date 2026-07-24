namespace SistemaFacturacion
{
    partial class frmReportePorFechas
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConsultar = new System.Windows.Forms.Button();
            this.dtpFechaDesde = new System.Windows.Forms.DateTimePicker();
            this.dtpFechaHasta = new System.Windows.Forms.DateTimePicker();
            this.gvReporteFecha = new System.Windows.Forms.DataGridView();
            this.lblTotales = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbProducto = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbCliente = new System.Windows.Forms.ComboBox();
            this.lblTotalCantidad = new System.Windows.Forms.Label();
            this.cmbFormaPago = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnPendientes = new System.Windows.Forms.Button();
            this.btnVerPendientes = new System.Windows.Forms.Button();
            this.btnVerConsumidor = new System.Windows.Forms.Button();
            this.chkVerTarjeta = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.gvReporteFecha)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(205, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Fecha Desde:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(552, 11);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(141, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "Fecha Hasta:";
            // 
            // btnConsultar
            // 
            this.btnConsultar.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.btnConsultar.Location = new System.Drawing.Point(951, 11);
            this.btnConsultar.Margin = new System.Windows.Forms.Padding(2);
            this.btnConsultar.Name = "btnConsultar";
            this.btnConsultar.Size = new System.Drawing.Size(221, 64);
            this.btnConsultar.TabIndex = 5;
            this.btnConsultar.Text = "CONSULTA";
            this.btnConsultar.UseVisualStyleBackColor = true;
            this.btnConsultar.Click += new System.EventHandler(this.btnConsultar_Click);
            // 
            // dtpFechaDesde
            // 
            this.dtpFechaDesde.CustomFormat = "yyyy/MM/dd";
            this.dtpFechaDesde.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpFechaDesde.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFechaDesde.Location = new System.Drawing.Point(363, 11);
            this.dtpFechaDesde.Margin = new System.Windows.Forms.Padding(2);
            this.dtpFechaDesde.Name = "dtpFechaDesde";
            this.dtpFechaDesde.Size = new System.Drawing.Size(163, 32);
            this.dtpFechaDesde.TabIndex = 0;
            // 
            // dtpFechaHasta
            // 
            this.dtpFechaHasta.CustomFormat = "yyyy/MM/dd";
            this.dtpFechaHasta.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpFechaHasta.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFechaHasta.Location = new System.Drawing.Point(695, 11);
            this.dtpFechaHasta.Margin = new System.Windows.Forms.Padding(2);
            this.dtpFechaHasta.Name = "dtpFechaHasta";
            this.dtpFechaHasta.Size = new System.Drawing.Size(167, 32);
            this.dtpFechaHasta.TabIndex = 1;
            // 
            // gvReporteFecha
            // 
            this.gvReporteFecha.AllowUserToAddRows = false;
            this.gvReporteFecha.AllowUserToResizeColumns = false;
            this.gvReporteFecha.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvReporteFecha.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gvReporteFecha.Location = new System.Drawing.Point(9, 211);
            this.gvReporteFecha.Margin = new System.Windows.Forms.Padding(2);
            this.gvReporteFecha.Name = "gvReporteFecha";
            this.gvReporteFecha.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gvReporteFecha.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.Format = "N2";
            this.gvReporteFecha.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.gvReporteFecha.RowTemplate.Height = 24;
            this.gvReporteFecha.Size = new System.Drawing.Size(1253, 351);
            this.gvReporteFecha.TabIndex = 5;
            // 
            // lblTotales
            // 
            this.lblTotales.AutoSize = true;
            this.lblTotales.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotales.Location = new System.Drawing.Point(446, 580);
            this.lblTotales.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTotales.Name = "lblTotales";
            this.lblTotales.Size = new System.Drawing.Size(64, 20);
            this.lblTotales.TabIndex = 6;
            this.lblTotales.Text = "TOTAL";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(235, 49);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 26);
            this.label3.TabIndex = 7;
            this.label3.Text = "Productos:";
            // 
            // cbProducto
            // 
            this.cbProducto.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbProducto.FormattingEnabled = true;
            this.cbProducto.Location = new System.Drawing.Point(363, 47);
            this.cbProducto.Margin = new System.Windows.Forms.Padding(2);
            this.cbProducto.Name = "cbProducto";
            this.cbProducto.Size = new System.Drawing.Size(498, 33);
            this.cbProducto.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(253, 88);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 26);
            this.label4.TabIndex = 9;
            this.label4.Text = "Clientes:";
            // 
            // cbCliente
            // 
            this.cbCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCliente.FormattingEnabled = true;
            this.cbCliente.Location = new System.Drawing.Point(363, 85);
            this.cbCliente.Margin = new System.Windows.Forms.Padding(2);
            this.cbCliente.Name = "cbCliente";
            this.cbCliente.Size = new System.Drawing.Size(498, 33);
            this.cbCliente.TabIndex = 3;
            // 
            // lblTotalCantidad
            // 
            this.lblTotalCantidad.AutoSize = true;
            this.lblTotalCantidad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalCantidad.Location = new System.Drawing.Point(193, 580);
            this.lblTotalCantidad.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTotalCantidad.Name = "lblTotalCantidad";
            this.lblTotalCantidad.Size = new System.Drawing.Size(99, 20);
            this.lblTotalCantidad.TabIndex = 11;
            this.lblTotalCantidad.Text = "CANTIDAD";
            // 
            // cmbFormaPago
            // 
            this.cmbFormaPago.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbFormaPago.FormattingEnabled = true;
            this.cmbFormaPago.Location = new System.Drawing.Point(363, 124);
            this.cmbFormaPago.Margin = new System.Windows.Forms.Padding(2);
            this.cmbFormaPago.Name = "cmbFormaPago";
            this.cmbFormaPago.Size = new System.Drawing.Size(498, 33);
            this.cmbFormaPago.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(182, 130);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(168, 26);
            this.label5.TabIndex = 12;
            this.label5.Text = "Forma de Pago:";
            // 
            // btnPendientes
            // 
            this.btnPendientes.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.btnPendientes.Location = new System.Drawing.Point(951, 88);
            this.btnPendientes.Margin = new System.Windows.Forms.Padding(2);
            this.btnPendientes.Name = "btnPendientes";
            this.btnPendientes.Size = new System.Drawing.Size(221, 64);
            this.btnPendientes.TabIndex = 16;
            this.btnPendientes.Text = "FACTURAR PENDIENTES";
            this.btnPendientes.UseVisualStyleBackColor = true;
            this.btnPendientes.Click += new System.EventHandler(this.btnPendientes_Click);
            // 
            // btnVerPendientes
            // 
            this.btnVerPendientes.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.btnVerPendientes.Location = new System.Drawing.Point(362, 163);
            this.btnVerPendientes.Margin = new System.Windows.Forms.Padding(2);
            this.btnVerPendientes.Name = "btnVerPendientes";
            this.btnVerPendientes.Size = new System.Drawing.Size(243, 35);
            this.btnVerPendientes.TabIndex = 17;
            this.btnVerPendientes.Text = "VER PENDIENTES";
            this.btnVerPendientes.UseVisualStyleBackColor = true;
            this.btnVerPendientes.Click += new System.EventHandler(this.btnVerPendientes_Click);
            // 
            // btnVerConsumidor
            // 
            this.btnVerConsumidor.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.btnVerConsumidor.Location = new System.Drawing.Point(619, 163);
            this.btnVerConsumidor.Margin = new System.Windows.Forms.Padding(2);
            this.btnVerConsumidor.Name = "btnVerConsumidor";
            this.btnVerConsumidor.Size = new System.Drawing.Size(243, 35);
            this.btnVerConsumidor.TabIndex = 18;
            this.btnVerConsumidor.Text = "VER CONSUMIDOR";
            this.btnVerConsumidor.UseVisualStyleBackColor = true;
            this.btnVerConsumidor.Click += new System.EventHandler(this.btnVerConsumidor_Click);
            // 
            // chkVerTarjeta
            // 
            this.chkVerTarjeta.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVerTarjeta.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkVerTarjeta.Location = new System.Drawing.Point(917, 163);
            this.chkVerTarjeta.Margin = new System.Windows.Forms.Padding(2);
            this.chkVerTarjeta.Name = "chkVerTarjeta";
            this.chkVerTarjeta.Size = new System.Drawing.Size(345, 35);
            this.chkVerTarjeta.TabIndex = 19;
            this.chkVerTarjeta.Text = "Ver columnas de tarjeta »";
            this.chkVerTarjeta.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkVerTarjeta.UseVisualStyleBackColor = true;
            this.chkVerTarjeta.CheckedChanged += new System.EventHandler(this.chkVerTarjeta_CheckedChanged);
            // 
            // frmReportePorFechas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1273, 609);
            this.Controls.Add(this.chkVerTarjeta);
            this.Controls.Add(this.btnVerConsumidor);
            this.Controls.Add(this.btnVerPendientes);
            this.Controls.Add(this.btnPendientes);
            this.Controls.Add(this.cmbFormaPago);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblTotalCantidad);
            this.Controls.Add(this.cbCliente);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbProducto);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTotales);
            this.Controls.Add(this.gvReporteFecha);
            this.Controls.Add(this.dtpFechaHasta);
            this.Controls.Add(this.dtpFechaDesde);
            this.Controls.Add(this.btnConsultar);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "frmReportePorFechas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "COSULTA POR FECHAS";
            this.Load += new System.EventHandler(this.frmReportePorFechas_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gvReporteFecha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConsultar;
        private System.Windows.Forms.DateTimePicker dtpFechaDesde;
        private System.Windows.Forms.DateTimePicker dtpFechaHasta;
        private System.Windows.Forms.DataGridView gvReporteFecha;
        private System.Windows.Forms.Label lblTotales;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbProducto;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbCliente;
        private System.Windows.Forms.Label lblTotalCantidad;
        private System.Windows.Forms.ComboBox cmbFormaPago;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnPendientes;
        private System.Windows.Forms.Button btnVerPendientes;
        private System.Windows.Forms.Button btnVerConsumidor;
        private System.Windows.Forms.CheckBox chkVerTarjeta;
    }
}