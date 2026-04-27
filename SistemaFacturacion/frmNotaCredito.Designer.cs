namespace SistemaFacturacion
{
    partial class frmNotaCredito
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
            this.btnConsultar = new System.Windows.Forms.Button();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.label9 = new System.Windows.Forms.Label();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbNota = new System.Windows.Forms.ComboBox();
            this.cmbCliente = new System.Windows.Forms.ComboBox();
            this.dgvNotas = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNotas)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.btnConsultar);
            this.panel1.Controls.Add(this.dtpHasta);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.dtpDesde);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.cmbNota);
            this.panel1.Controls.Add(this.cmbCliente);
            this.panel1.Location = new System.Drawing.Point(13, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1056, 173);
            this.panel1.TabIndex = 0;
            // 
            // btnConsultar
            // 
            this.btnConsultar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConsultar.Location = new System.Drawing.Point(731, 76);
            this.btnConsultar.Name = "btnConsultar";
            this.btnConsultar.Size = new System.Drawing.Size(181, 58);
            this.btnConsultar.TabIndex = 4;
            this.btnConsultar.Text = "Consultar";
            this.btnConsultar.UseVisualStyleBackColor = true;
            this.btnConsultar.Click += new System.EventHandler(this.btnConsultar_Click_1);
            // 
            // dtpHasta
            // 
            this.dtpHasta.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpHasta.Location = new System.Drawing.Point(539, 30);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(141, 24);
            this.dtpHasta.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(430, 32);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(103, 18);
            this.label9.TabIndex = 7;
            this.label9.Text = "Fecha Hasta";
            // 
            // dtpDesde
            // 
            this.dtpDesde.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpDesde.Location = new System.Drawing.Point(261, 30);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(141, 24);
            this.dtpDesde.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(148, 32);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 18);
            this.label8.TabIndex = 5;
            this.label8.Text = "Fecha Desde";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(123, 122);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(137, 18);
            this.label7.TabIndex = 4;
            this.label7.Text = "Número de Nota:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(195, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 18);
            this.label6.TabIndex = 3;
            this.label6.Text = "Cliente:";
            // 
            // cmbNota
            // 
            this.cmbNota.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNota.FormattingEnabled = true;
            this.cmbNota.Location = new System.Drawing.Point(261, 119);
            this.cmbNota.Name = "cmbNota";
            this.cmbNota.Size = new System.Drawing.Size(419, 26);
            this.cmbNota.TabIndex = 3;
            // 
            // cmbCliente
            // 
            this.cmbCliente.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbCliente.FormattingEnabled = true;
            this.cmbCliente.Location = new System.Drawing.Point(261, 73);
            this.cmbCliente.Name = "cmbCliente";
            this.cmbCliente.Size = new System.Drawing.Size(419, 26);
            this.cmbCliente.TabIndex = 2;
            // 
            // dgvNotas
            // 
            this.dgvNotas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNotas.Location = new System.Drawing.Point(12, 203);
            this.dgvNotas.Name = "dgvNotas";
            this.dgvNotas.ShowRowErrors = false;
            this.dgvNotas.Size = new System.Drawing.Size(1057, 363);
            this.dgvNotas.TabIndex = 1;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(618, 581);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(42, 13);
            this.lblTotal.TabIndex = 2;
            this.lblTotal.Text = "Totales";
            // 
            // frmNotaCredito
            // 
            this.AcceptButton = this.btnConsultar;
            this.ClientSize = new System.Drawing.Size(1081, 611);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.dgvNotas);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "frmNotaCredito";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notas de Crédito";
            this.Load += new System.EventHandler(this.frmNotaCredito_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNotas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbNota;
        private System.Windows.Forms.ComboBox cmbCliente;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnConsultar;
        private System.Windows.Forms.DataGridView dgvNotas;
        private System.Windows.Forms.Label lblTotal;
    }
}