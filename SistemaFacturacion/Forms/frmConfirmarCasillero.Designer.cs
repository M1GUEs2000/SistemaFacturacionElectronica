namespace SistemaFacturacion
{
    partial class frmConfirmarCasillero
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblIconoTitulo = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblMensaje = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnConfirmar = new SistemaFacturacion.Controls.IconButton();
            this.btnCancelar = new SistemaFacturacion.Controls.IconButton();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(255, 147, 41);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Controls.Add(this.lblIconoTitulo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(380, 56);
            this.pnlHeader.TabIndex = 0;
            //
            // lblIconoTitulo
            //
            this.lblIconoTitulo.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblIconoTitulo.Font = SistemaFacturacion.Controls.IconosCasilleros.CrearFuente(17F);
            this.lblIconoTitulo.ForeColor = System.Drawing.Color.White;
            this.lblIconoTitulo.Name = "lblIconoTitulo";
            this.lblIconoTitulo.Size = new System.Drawing.Size(44, 56);
            this.lblIconoTitulo.TabIndex = 0;
            this.lblIconoTitulo.Text = SistemaFacturacion.Controls.IconosCasilleros.Glifo(SistemaFacturacion.Controls.IconosCasilleros.Pregunta);
            this.lblIconoTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblIconoTitulo.UseCompatibleTextRendering = true;
            //
            // lblTitulo
            //
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(336, 56);
            this.lblTitulo.TabIndex = 1;
            this.lblTitulo.Text = "Confirmar";
            //
            // lblMensaje
            //
            this.lblMensaje.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblMensaje.ForeColor = System.Drawing.Color.FromArgb(52, 58, 64);
            this.lblMensaje.Location = new System.Drawing.Point(24, 76);
            this.lblMensaje.Name = "lblMensaje";
            this.lblMensaje.Size = new System.Drawing.Size(332, 80);
            this.lblMensaje.TabIndex = 1;
            this.lblMensaje.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(247, 248, 249);
            this.pnlFooter.Controls.Add(this.btnConfirmar);
            this.pnlFooter.Controls.Add(this.btnCancelar);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 160);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(380, 68);
            this.pnlFooter.TabIndex = 2;
            //
            // btnConfirmar
            //
            this.btnConfirmar.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnConfirmar.CodigoIcono = 0xf00c;
            this.btnConfirmar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnConfirmar.ForeColor = System.Drawing.Color.White;
            this.btnConfirmar.Location = new System.Drawing.Point(24, 16);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.RadioEsquinas = 8;
            this.btnConfirmar.Size = new System.Drawing.Size(168, 38);
            this.btnConfirmar.TabIndex = 0;
            this.btnConfirmar.Text = "Sí, continuar";
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnCancelar.CodigoIcono = 0xf00d;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(208, 16);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.RadioEsquinas = 8;
            this.btnCancelar.Size = new System.Drawing.Size(148, 38);
            this.btnCancelar.TabIndex = 1;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // frmConfirmarCasillero
            //
            this.AcceptButton = this.btnConfirmar;
            this.CancelButton = this.btnCancelar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(380, 228);
            this.Controls.Add(this.lblMensaje);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConfirmarCasillero";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Confirmar";
            this.pnlHeader.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblIconoTitulo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblMensaje;
        private System.Windows.Forms.Panel pnlFooter;
        private SistemaFacturacion.Controls.IconButton btnConfirmar;
        private SistemaFacturacion.Controls.IconButton btnCancelar;
    }
}
