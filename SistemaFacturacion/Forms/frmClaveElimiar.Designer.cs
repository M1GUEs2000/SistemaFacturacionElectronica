namespace SistemaFacturacion
{
    partial class frmClaveElimiar
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtClaveEliminar = new System.Windows.Forms.TextBox();
            this.btnAceptarEli = new System.Windows.Forms.Button();
            this.lblCadena = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "CLAVE:";
            // 
            // txtClaveEliminar
            // 
            this.txtClaveEliminar.Location = new System.Drawing.Point(108, 24);
            this.txtClaveEliminar.Name = "txtClaveEliminar";
            this.txtClaveEliminar.Size = new System.Drawing.Size(167, 22);
            this.txtClaveEliminar.TabIndex = 1;
            this.txtClaveEliminar.UseSystemPasswordChar = true;
            this.txtClaveEliminar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtClaveEliminar_KeyDown);
            // 
            // btnAceptarEli
            // 
            this.btnAceptarEli.Location = new System.Drawing.Point(108, 52);
            this.btnAceptarEli.Name = "btnAceptarEli";
            this.btnAceptarEli.Size = new System.Drawing.Size(167, 41);
            this.btnAceptarEli.TabIndex = 2;
            this.btnAceptarEli.Text = "ACEPTAR";
            this.btnAceptarEli.UseVisualStyleBackColor = true;
            this.btnAceptarEli.Click += new System.EventHandler(this.btnAceptarEli_Click);
            // 
            // lblCadena
            // 
            this.lblCadena.AutoSize = true;
            this.lblCadena.Location = new System.Drawing.Point(62, 110);
            this.lblCadena.Name = "lblCadena";
            this.lblCadena.Size = new System.Drawing.Size(46, 17);
            this.lblCadena.TabIndex = 3;
            this.lblCadena.Text = "label2";
            this.lblCadena.Visible = false;
            // 
            // frmClaveElimiar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 135);
            this.Controls.Add(this.lblCadena);
            this.Controls.Add(this.btnAceptarEli);
            this.Controls.Add(this.txtClaveEliminar);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "frmClaveElimiar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ELIMINAR FACTURACION";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtClaveEliminar;
        private System.Windows.Forms.Button btnAceptarEli;
        private System.Windows.Forms.Label lblCadena;
    }
}