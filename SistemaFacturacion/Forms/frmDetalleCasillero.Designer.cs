namespace SistemaFacturacion
{
    partial class frmDetalleCasillero
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
            this.btnCerrarVentana = new SistemaFacturacion.Controls.IconButton();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlCuerpo = new System.Windows.Forms.Panel();
            this.lblClienteEtiqueta = new System.Windows.Forms.Label();
            this.lblClienteValor = new System.Windows.Forms.Label();
            this.lblInicioEtiqueta = new System.Windows.Forms.Label();
            this.lblInicioValor = new System.Windows.Forms.Label();
            this.lblDuracionEtiqueta = new System.Windows.Forms.Label();
            this.lblDuracionValor = new System.Windows.Forms.Label();
            this.lblTiempoEtiqueta = new System.Windows.Forms.Label();
            this.lblTiempoValor = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnLiberar = new SistemaFacturacion.Controls.IconButton();
            this.btnCerrar = new SistemaFacturacion.Controls.IconButton();
            this.pnlHeader.SuspendLayout();
            this.pnlCuerpo.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Controls.Add(this.lblIconoTitulo);
            this.pnlHeader.Controls.Add(this.btnCerrarVentana);
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
            this.lblIconoTitulo.TabIndex = 2;
            this.lblIconoTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblIconoTitulo.UseCompatibleTextRendering = true;
            //
            // btnCerrarVentana
            //
            this.btnCerrarVentana.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnCerrarVentana.CodigoIcono = 0xf00d;
            this.btnCerrarVentana.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCerrarVentana.FlatAppearance.BorderSize = 0;
            this.btnCerrarVentana.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnCerrarVentana.ForeColor = System.Drawing.Color.White;
            this.btnCerrarVentana.Name = "btnCerrarVentana";
            this.btnCerrarVentana.RadioEsquinas = 0;
            this.btnCerrarVentana.Size = new System.Drawing.Size(46, 56);
            this.btnCerrarVentana.TabIndex = 9;
            this.btnCerrarVentana.UseVisualStyleBackColor = false;
            this.btnCerrarVentana.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // lblTitulo
            //
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(290, 56);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Casillero";
            //
            // pnlCuerpo
            //
            this.pnlCuerpo.BackColor = System.Drawing.Color.White;
            this.pnlCuerpo.Controls.Add(this.lblClienteEtiqueta);
            this.pnlCuerpo.Controls.Add(this.lblClienteValor);
            this.pnlCuerpo.Controls.Add(this.lblInicioEtiqueta);
            this.pnlCuerpo.Controls.Add(this.lblInicioValor);
            this.pnlCuerpo.Controls.Add(this.lblDuracionEtiqueta);
            this.pnlCuerpo.Controls.Add(this.lblDuracionValor);
            this.pnlCuerpo.Controls.Add(this.lblTiempoEtiqueta);
            this.pnlCuerpo.Controls.Add(this.lblTiempoValor);
            this.pnlCuerpo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCuerpo.Location = new System.Drawing.Point(0, 56);
            this.pnlCuerpo.Name = "pnlCuerpo";
            this.pnlCuerpo.Size = new System.Drawing.Size(380, 364);
            this.pnlCuerpo.TabIndex = 1;
            //
            // lblClienteEtiqueta
            //
            this.lblClienteEtiqueta.AutoSize = true;
            this.lblClienteEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblClienteEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblClienteEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblClienteEtiqueta.Location = new System.Drawing.Point(72, 152);
            this.lblClienteEtiqueta.Name = "lblClienteEtiqueta";
            this.lblClienteEtiqueta.Size = new System.Drawing.Size(45, 13);
            this.lblClienteEtiqueta.TabIndex = 1;
            this.lblClienteEtiqueta.Text = "CLIENTE";
            //
            // lblClienteValor
            //
            this.lblClienteValor.AutoEllipsis = true;
            this.lblClienteValor.AutoSize = false;
            this.lblClienteValor.BackColor = System.Drawing.Color.Transparent;
            this.lblClienteValor.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblClienteValor.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this.lblClienteValor.Location = new System.Drawing.Point(70, 167);
            this.lblClienteValor.Name = "lblClienteValor";
            this.lblClienteValor.Size = new System.Drawing.Size(232, 22);
            this.lblClienteValor.TabIndex = 2;
            this.lblClienteValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblInicioEtiqueta
            //
            this.lblInicioEtiqueta.AutoSize = true;
            this.lblInicioEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblInicioEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblInicioEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblInicioEtiqueta.Location = new System.Drawing.Point(72, 204);
            this.lblInicioEtiqueta.Name = "lblInicioEtiqueta";
            this.lblInicioEtiqueta.Size = new System.Drawing.Size(66, 13);
            this.lblInicioEtiqueta.TabIndex = 3;
            this.lblInicioEtiqueta.Text = "HORA INICIO";
            //
            // lblInicioValor
            //
            this.lblInicioValor.AutoEllipsis = true;
            this.lblInicioValor.AutoSize = false;
            this.lblInicioValor.BackColor = System.Drawing.Color.Transparent;
            this.lblInicioValor.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblInicioValor.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this.lblInicioValor.Location = new System.Drawing.Point(70, 219);
            this.lblInicioValor.Name = "lblInicioValor";
            this.lblInicioValor.Size = new System.Drawing.Size(232, 22);
            this.lblInicioValor.TabIndex = 4;
            this.lblInicioValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblDuracionEtiqueta
            //
            this.lblDuracionEtiqueta.AutoSize = true;
            this.lblDuracionEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblDuracionEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblDuracionEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblDuracionEtiqueta.Location = new System.Drawing.Point(72, 256);
            this.lblDuracionEtiqueta.Name = "lblDuracionEtiqueta";
            this.lblDuracionEtiqueta.Size = new System.Drawing.Size(115, 13);
            this.lblDuracionEtiqueta.TabIndex = 5;
            this.lblDuracionEtiqueta.Text = "TIEMPO SOLICITADO";
            //
            // lblDuracionValor
            //
            this.lblDuracionValor.AutoEllipsis = true;
            this.lblDuracionValor.AutoSize = false;
            this.lblDuracionValor.BackColor = System.Drawing.Color.Transparent;
            this.lblDuracionValor.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblDuracionValor.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this.lblDuracionValor.Location = new System.Drawing.Point(70, 271);
            this.lblDuracionValor.Name = "lblDuracionValor";
            this.lblDuracionValor.Size = new System.Drawing.Size(232, 22);
            this.lblDuracionValor.TabIndex = 6;
            this.lblDuracionValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblTiempoEtiqueta
            //
            this.lblTiempoEtiqueta.AutoSize = true;
            this.lblTiempoEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblTiempoEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblTiempoEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblTiempoEtiqueta.Location = new System.Drawing.Point(72, 308);
            this.lblTiempoEtiqueta.Name = "lblTiempoEtiqueta";
            this.lblTiempoEtiqueta.Size = new System.Drawing.Size(56, 13);
            this.lblTiempoEtiqueta.TabIndex = 7;
            this.lblTiempoEtiqueta.Text = "HORA FIN";
            //
            // lblTiempoValor
            //
            this.lblTiempoValor.AutoEllipsis = true;
            this.lblTiempoValor.AutoSize = false;
            this.lblTiempoValor.BackColor = System.Drawing.Color.Transparent;
            this.lblTiempoValor.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTiempoValor.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this.lblTiempoValor.Location = new System.Drawing.Point(70, 323);
            this.lblTiempoValor.Name = "lblTiempoValor";
            this.lblTiempoValor.Size = new System.Drawing.Size(232, 22);
            this.lblTiempoValor.TabIndex = 8;
            this.lblTiempoValor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(247, 248, 249);
            this.pnlFooter.Controls.Add(this.btnLiberar);
            this.pnlFooter.Controls.Add(this.btnCerrar);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 420);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(380, 68);
            this.pnlFooter.TabIndex = 2;
            //
            // btnLiberar
            //
            this.btnLiberar.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnLiberar.CodigoIcono = 0xf084;
            this.btnLiberar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnLiberar.ForeColor = System.Drawing.Color.White;
            this.btnLiberar.Location = new System.Drawing.Point(24, 16);
            this.btnLiberar.Name = "btnLiberar";
            this.btnLiberar.RadioEsquinas = 8;
            this.btnLiberar.Size = new System.Drawing.Size(160, 38);
            this.btnLiberar.TabIndex = 7;
            this.btnLiberar.Text = "Liberar";
            this.btnLiberar.Click += new System.EventHandler(this.btnLiberar_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnCerrar.CodigoIcono = 0xf00d;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(196, 16);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.RadioEsquinas = 8;
            this.btnCerrar.Size = new System.Drawing.Size(160, 38);
            this.btnCerrar.TabIndex = 8;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            //
            // frmDetalleCasillero
            //
            this.AcceptButton = this.btnCerrar;
            this.CancelButton = this.btnCerrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(380, 488);
            this.Controls.Add(this.pnlCuerpo);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDetalleCasillero";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Detalle del casillero";
            this.pnlHeader.ResumeLayout(false);
            this.pnlCuerpo.ResumeLayout(false);
            this.pnlCuerpo.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblIconoTitulo;
        private SistemaFacturacion.Controls.IconButton btnCerrarVentana;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlCuerpo;
        private System.Windows.Forms.Label lblClienteEtiqueta;
        private System.Windows.Forms.Label lblClienteValor;
        private System.Windows.Forms.Label lblInicioEtiqueta;
        private System.Windows.Forms.Label lblInicioValor;
        private System.Windows.Forms.Label lblDuracionEtiqueta;
        private System.Windows.Forms.Label lblDuracionValor;
        private System.Windows.Forms.Label lblTiempoEtiqueta;
        private System.Windows.Forms.Label lblTiempoValor;
        private System.Windows.Forms.Panel pnlFooter;
        private SistemaFacturacion.Controls.IconButton btnLiberar;
        private SistemaFacturacion.Controls.IconButton btnCerrar;
    }
}
