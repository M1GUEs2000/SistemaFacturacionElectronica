namespace SistemaFacturacion
{
    partial class frmNuevaNotaCredito
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lstNotas = new System.Windows.Forms.ListBox();
            this.txtNota = new System.Windows.Forms.TextBox();
            this.lblNumeroNota = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.txtMotivo = new System.Windows.Forms.TextBox();
            this.lblFecha = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblFactura = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblCedula = new System.Windows.Forms.Label();
            this.lblNombre = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnGenerar = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblTotalOriginal = new System.Windows.Forms.Label();
            this.lblCantidadOriginal = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvOriginal = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblTotalNota = new System.Windows.Forms.Label();
            this.lblCantidadNota = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.dgvNota = new System.Windows.Forms.DataGridView();
            this.btnPDF = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOriginal)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNota)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.lstNotas);
            this.panel1.Controls.Add(this.txtNota);
            this.panel1.Controls.Add(this.lblNumeroNota);
            this.panel1.Controls.Add(this.label18);
            this.panel1.Controls.Add(this.label17);
            this.panel1.Controls.Add(this.txtMotivo);
            this.panel1.Controls.Add(this.lblFecha);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.lblFactura);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.lblCedula);
            this.panel1.Controls.Add(this.lblNombre);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(888, 180);
            this.panel1.TabIndex = 0;
            // 
            // lstNotas
            // 
            this.lstNotas.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstNotas.FormattingEnabled = true;
            this.lstNotas.ItemHeight = 16;
            this.lstNotas.Location = new System.Drawing.Point(201, 55);
            this.lstNotas.Name = "lstNotas";
            this.lstNotas.Size = new System.Drawing.Size(499, 116);
            this.lstNotas.TabIndex = 29;
            // 
            // txtNota
            // 
            this.txtNota.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNota.Location = new System.Drawing.Point(200, 31);
            this.txtNota.Name = "txtNota";
            this.txtNota.Size = new System.Drawing.Size(499, 24);
            this.txtNota.TabIndex = 28;
            // 
            // lblNumeroNota
            // 
            this.lblNumeroNota.AutoSize = true;
            this.lblNumeroNota.Font = new System.Drawing.Font("MS UI Gothic", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumeroNota.Location = new System.Drawing.Point(778, 16);
            this.lblNumeroNota.Name = "lblNumeroNota";
            this.lblNumeroNota.Size = new System.Drawing.Size(28, 18);
            this.lblNumeroNota.TabIndex = 27;
            this.lblNumeroNota.Text = "Nº";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("MS UI Gothic", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(756, 16);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(28, 18);
            this.label18.TabIndex = 26;
            this.label18.Text = "Nº";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(648, 101);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(180, 16);
            this.label17.TabIndex = 25;
            this.label17.Text = "Motivo de la Nota de Crédito:";
            // 
            // txtMotivo
            // 
            this.txtMotivo.Location = new System.Drawing.Point(651, 131);
            this.txtMotivo.Name = "txtMotivo";
            this.txtMotivo.Size = new System.Drawing.Size(216, 20);
            this.txtMotivo.TabIndex = 24;
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFecha.Location = new System.Drawing.Point(465, 135);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(51, 16);
            this.lblFecha.TabIndex = 23;
            this.lblFecha.Text = "label11";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(360, 135);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 16);
            this.label9.TabIndex = 22;
            this.label9.Text = "Fecha Emisión:";
            // 
            // lblFactura
            // 
            this.lblFactura.AutoSize = true;
            this.lblFactura.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFactura.Location = new System.Drawing.Point(466, 101);
            this.lblFactura.Name = "lblFactura";
            this.lblFactura.Size = new System.Drawing.Size(51, 16);
            this.lblFactura.TabIndex = 21;
            this.lblFactura.Text = "label11";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(334, 101);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(125, 16);
            this.label12.TabIndex = 20;
            this.label12.Text = "Número de Factura:";
            // 
            // lblCedula
            // 
            this.lblCedula.AutoSize = true;
            this.lblCedula.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCedula.Location = new System.Drawing.Point(128, 135);
            this.lblCedula.Name = "lblCedula";
            this.lblCedula.Size = new System.Drawing.Size(44, 16);
            this.lblCedula.TabIndex = 19;
            this.lblCedula.Text = "label9";
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombre.Location = new System.Drawing.Point(128, 101);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(44, 16);
            this.lblNombre.TabIndex = 18;
            this.lblNombre.Text = "label8";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(69, 135);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 16);
            this.label6.TabIndex = 17;
            this.label6.Text = "Cédula:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(56, 101);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(66, 16);
            this.label10.TabIndex = 16;
            this.label10.Text = "Nombres:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(41, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(153, 18);
            this.label7.TabIndex = 4;
            this.label7.Text = "Número de Factura";
            // 
            // btnGenerar
            // 
            this.btnGenerar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerar.Location = new System.Drawing.Point(301, 665);
            this.btnGenerar.Name = "btnGenerar";
            this.btnGenerar.Size = new System.Drawing.Size(300, 71);
            this.btnGenerar.TabIndex = 22;
            this.btnGenerar.Text = "Generar Nueva Nota de Crédito";
            this.btnGenerar.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel3.Controls.Add(this.lblTotalOriginal);
            this.panel3.Controls.Add(this.lblCantidadOriginal);
            this.panel3.Controls.Add(this.label15);
            this.panel3.Controls.Add(this.label13);
            this.panel3.Controls.Add(this.label8);
            this.panel3.Controls.Add(this.dgvOriginal);
            this.panel3.Location = new System.Drawing.Point(13, 210);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(437, 449);
            this.panel3.TabIndex = 23;
            // 
            // lblTotalOriginal
            // 
            this.lblTotalOriginal.AutoSize = true;
            this.lblTotalOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalOriginal.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTotalOriginal.Location = new System.Drawing.Point(238, 415);
            this.lblTotalOriginal.Name = "lblTotalOriginal";
            this.lblTotalOriginal.Size = new System.Drawing.Size(12, 18);
            this.lblTotalOriginal.TabIndex = 7;
            this.lblTotalOriginal.Text = "l";
            // 
            // lblCantidadOriginal
            // 
            this.lblCantidadOriginal.AutoSize = true;
            this.lblCantidadOriginal.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCantidadOriginal.ForeColor = System.Drawing.SystemColors.Control;
            this.lblCantidadOriginal.Location = new System.Drawing.Point(100, 415);
            this.lblCantidadOriginal.Name = "lblCantidadOriginal";
            this.lblCantidadOriginal.Size = new System.Drawing.Size(12, 18);
            this.lblCantidadOriginal.TabIndex = 6;
            this.lblCantidadOriginal.Text = "l";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.SystemColors.Control;
            this.label15.Location = new System.Drawing.Point(181, 415);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(51, 18);
            this.label15.TabIndex = 5;
            this.label15.Text = "Total:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.SystemColors.Control;
            this.label13.Location = new System.Drawing.Point(15, 415);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(79, 18);
            this.label13.TabIndex = 4;
            this.label13.Text = "Cantidad:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(3, 8);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(424, 18);
            this.label8.TabIndex = 2;
            this.label8.Text = "Seleccione los productos que iran en la nota de crédito";
            // 
            // dgvOriginal
            // 
            this.dgvOriginal.BackgroundColor = System.Drawing.Color.Silver;
            this.dgvOriginal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOriginal.Location = new System.Drawing.Point(3, 29);
            this.dgvOriginal.Name = "dgvOriginal";
            this.dgvOriginal.ShowRowErrors = false;
            this.dgvOriginal.Size = new System.Drawing.Size(430, 372);
            this.dgvOriginal.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel2.Controls.Add(this.lblTotalNota);
            this.panel2.Controls.Add(this.lblCantidadNota);
            this.panel2.Controls.Add(this.label16);
            this.panel2.Controls.Add(this.label14);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.dgvNota);
            this.panel2.Location = new System.Drawing.Point(463, 210);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(438, 449);
            this.panel2.TabIndex = 24;
            // 
            // lblTotalNota
            // 
            this.lblTotalNota.AutoSize = true;
            this.lblTotalNota.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalNota.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTotalNota.Location = new System.Drawing.Point(255, 415);
            this.lblTotalNota.Name = "lblTotalNota";
            this.lblTotalNota.Size = new System.Drawing.Size(12, 18);
            this.lblTotalNota.TabIndex = 9;
            this.lblTotalNota.Text = "l";
            // 
            // lblCantidadNota
            // 
            this.lblCantidadNota.AutoSize = true;
            this.lblCantidadNota.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCantidadNota.ForeColor = System.Drawing.SystemColors.Control;
            this.lblCantidadNota.Location = new System.Drawing.Point(101, 415);
            this.lblCantidadNota.Name = "lblCantidadNota";
            this.lblCantidadNota.Size = new System.Drawing.Size(12, 18);
            this.lblCantidadNota.TabIndex = 8;
            this.lblCantidadNota.Text = "l";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.ForeColor = System.Drawing.SystemColors.Control;
            this.label16.Location = new System.Drawing.Point(198, 415);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(51, 18);
            this.label16.TabIndex = 6;
            this.label16.Text = "Total:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.ForeColor = System.Drawing.SystemColors.Control;
            this.label14.Location = new System.Drawing.Point(16, 415);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(79, 18);
            this.label14.TabIndex = 5;
            this.label14.Text = "Cantidad:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.Control;
            this.label11.Location = new System.Drawing.Point(16, 8);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(233, 18);
            this.label11.TabIndex = 3;
            this.label11.Text = "Productos en Nota de Crédito";
            // 
            // dgvNota
            // 
            this.dgvNota.BackgroundColor = System.Drawing.Color.Silver;
            this.dgvNota.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNota.Location = new System.Drawing.Point(3, 29);
            this.dgvNota.Name = "dgvNota";
            this.dgvNota.ShowRowErrors = false;
            this.dgvNota.Size = new System.Drawing.Size(430, 372);
            this.dgvNota.TabIndex = 1;
            // 
            // btnPDF
            // 
            this.btnPDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPDF.Location = new System.Drawing.Point(721, 665);
            this.btnPDF.Name = "btnPDF";
            this.btnPDF.Size = new System.Drawing.Size(180, 71);
            this.btnPDF.TabIndex = 25;
            this.btnPDF.Text = "Visualizar PDF";
            this.btnPDF.UseVisualStyleBackColor = true;
            this.btnPDF.Click += new System.EventHandler(this.btnPDF_Click);
            // 
            // frmNuevaNotaCredito
            // 
            this.AcceptButton = this.btnGenerar;
            this.ClientSize = new System.Drawing.Size(913, 748);
            this.Controls.Add(this.btnPDF);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.btnGenerar);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "frmNuevaNotaCredito";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notas de Crédito";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOriginal)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNota)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblFactura;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblCedula;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnGenerar;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView dgvOriginal;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgvNota;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblCantidadNota;
        private System.Windows.Forms.Label lblTotalOriginal;
        private System.Windows.Forms.Label lblCantidadOriginal;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblTotalNota;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtMotivo;
        private System.Windows.Forms.Label lblNumeroNota;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button btnPDF;
        private System.Windows.Forms.TextBox txtNota;
        private System.Windows.Forms.ListBox lstNotas;
    }
}