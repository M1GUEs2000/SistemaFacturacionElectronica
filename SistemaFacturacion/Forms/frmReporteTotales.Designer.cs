namespace SistemaFacturacion
{
    partial class frmReporteTotales
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
            this.gvFacturasTotales = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSumaTotal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gvFacturasTotales)).BeginInit();
            this.SuspendLayout();
            // 
            // gvFacturasTotales
            // 
            this.gvFacturasTotales.AllowUserToAddRows = false;
            this.gvFacturasTotales.AllowUserToResizeColumns = false;
            this.gvFacturasTotales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvFacturasTotales.Location = new System.Drawing.Point(9, 33);
            this.gvFacturasTotales.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gvFacturasTotales.Name = "gvFacturasTotales";
            this.gvFacturasTotales.ReadOnly = true;
            dataGridViewCellStyle1.Format = "N2";
            this.gvFacturasTotales.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.gvFacturasTotales.RowTemplate.Height = 24;
            this.gvFacturasTotales.Size = new System.Drawing.Size(592, 180);
            this.gvFacturasTotales.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label1.Location = new System.Drawing.Point(185, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(242, 24);
            this.label1.TabIndex = 1;
            this.label1.Text = "REPORTE DE TOTALES";
            // 
            // lblSumaTotal
            // 
            this.lblSumaTotal.AutoSize = true;
            this.lblSumaTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSumaTotal.Location = new System.Drawing.Point(275, 215);
            this.lblSumaTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSumaTotal.Name = "lblSumaTotal";
            this.lblSumaTotal.Size = new System.Drawing.Size(76, 20);
            this.lblSumaTotal.TabIndex = 2;
            this.lblSumaTotal.Text = "Mensaje";
            // 
            // frmReporteTotales
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 240);
            this.Controls.Add(this.lblSumaTotal);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gvFacturasTotales);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "frmReporteTotales";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "REPORTE TOTALES";
            this.Load += new System.EventHandler(this.frmReporteTotales_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gvFacturasTotales)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gvFacturasTotales;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSumaTotal;
    }
}