namespace SistemaFacturacion
{
    partial class frmCasilleroPrincipal
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
            this.btnCerrarVentana = new SistemaFacturacion.Controls.IconButton();
            this.btnMinimizarVentana = new SistemaFacturacion.Controls.IconButton();
            this.lblIconoTitulo = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.pnlResumen = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnAsignar = new SistemaFacturacion.Controls.IconButton();
            this.btnActualizar = new SistemaFacturacion.Controls.IconButton();
            this.pnlCajaFiltro = new System.Windows.Forms.Panel();
            this.txtFiltro = new System.Windows.Forms.TextBox();
            this.btnConfiguracion = new SistemaFacturacion.Controls.IconButton();
            this.pnlTablero = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlHeader.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlCajaFiltro.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.pnlHeader.Controls.Add(this.lblTitulo);
            this.pnlHeader.Controls.Add(this.lblIconoTitulo);
            this.pnlHeader.Controls.Add(this.btnMinimizarVentana);
            this.pnlHeader.Controls.Add(this.btnCerrarVentana);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(960, 44);
            this.pnlHeader.TabIndex = 0;
            //
            // btnCerrarVentana
            //
            this.btnCerrarVentana.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnCerrarVentana.CodigoIcono = 0xf00d;
            this.btnCerrarVentana.ColorHoverPersonalizado = System.Drawing.Color.FromArgb(220, 53, 69);
            this.btnCerrarVentana.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCerrarVentana.FlatAppearance.BorderSize = 0;
            this.btnCerrarVentana.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnCerrarVentana.ForeColor = System.Drawing.Color.White;
            this.btnCerrarVentana.Name = "btnCerrarVentana";
            this.btnCerrarVentana.RadioEsquinas = 0;
            this.btnCerrarVentana.Size = new System.Drawing.Size(46, 44);
            this.btnCerrarVentana.TabIndex = 3;
            this.btnCerrarVentana.UseVisualStyleBackColor = false;
            this.btnCerrarVentana.Click += new System.EventHandler(this.btnCerrarVentana_Click);
            //
            // btnMinimizarVentana
            //
            this.btnMinimizarVentana.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnMinimizarVentana.CodigoIcono = SistemaFacturacion.Controls.IconosCasilleros.Minimizar;
            this.btnMinimizarVentana.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnMinimizarVentana.FlatAppearance.BorderSize = 0;
            this.btnMinimizarVentana.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnMinimizarVentana.ForeColor = System.Drawing.Color.White;
            this.btnMinimizarVentana.Name = "btnMinimizarVentana";
            this.btnMinimizarVentana.RadioEsquinas = 0;
            this.btnMinimizarVentana.Size = new System.Drawing.Size(46, 44);
            this.btnMinimizarVentana.TabIndex = 4;
            this.btnMinimizarVentana.UseVisualStyleBackColor = false;
            this.btnMinimizarVentana.Click += new System.EventHandler(this.btnMinimizarVentana_Click);
            //
            // lblIconoTitulo
            //
            this.lblIconoTitulo.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblIconoTitulo.Font = SistemaFacturacion.Controls.IconosCasilleros.CrearFuente(18F);
            this.lblIconoTitulo.ForeColor = System.Drawing.Color.White;
            this.lblIconoTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblIconoTitulo.Name = "lblIconoTitulo";
            this.lblIconoTitulo.Size = new System.Drawing.Size(52, 44);
            this.lblIconoTitulo.TabIndex = 1;
            this.lblIconoTitulo.Text = SistemaFacturacion.Controls.IconosCasilleros.Glifo(SistemaFacturacion.Controls.IconosCasilleros.Candado);
            this.lblIconoTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblIconoTitulo.UseCompatibleTextRendering = true;
            //
            // lblTitulo
            //
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.lblTitulo.Location = new System.Drawing.Point(0, 0);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(960, 44);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Casilleros";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlResumen
            //
            this.pnlResumen.BackColor = System.Drawing.Color.White;
            this.pnlResumen.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlResumen.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlResumen.Location = new System.Drawing.Point(0, 104);
            this.pnlResumen.Name = "pnlResumen";
            this.pnlResumen.Padding = new System.Windows.Forms.Padding(20, 12, 0, 12);
            this.pnlResumen.Size = new System.Drawing.Size(960, 46);
            this.pnlResumen.TabIndex = 1;
            this.pnlResumen.WrapContents = false;
            //
            // pnlToolbar
            //
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlToolbar.Controls.Add(this.btnAsignar);
            this.pnlToolbar.Controls.Add(this.btnActualizar);
            this.pnlToolbar.Controls.Add(this.pnlCajaFiltro);
            this.pnlToolbar.Controls.Add(this.btnConfiguracion);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 44);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(960, 60);
            this.pnlToolbar.TabIndex = 2;
            //
            // btnAsignar
            //
            this.btnAsignar.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnAsignar.CodigoIcono = 0xf067;
            this.btnAsignar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnAsignar.ForeColor = System.Drawing.Color.White;
            this.btnAsignar.Location = new System.Drawing.Point(20, 12);
            this.btnAsignar.Name = "btnAsignar";
            this.btnAsignar.RadioEsquinas = 8;
            this.btnAsignar.Size = new System.Drawing.Size(180, 36);
            this.btnAsignar.TabIndex = 0;
            this.btnAsignar.Text = "Asignar casillero";
            this.btnAsignar.Click += new System.EventHandler(this.btnAsignar_Click);
            //
            // btnActualizar
            //
            this.btnActualizar.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnActualizar.CodigoIcono = 0xf021;
            this.btnActualizar.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnActualizar.ForeColor = System.Drawing.Color.White;
            this.btnActualizar.Location = new System.Drawing.Point(210, 12);
            this.btnActualizar.Name = "btnActualizar";
            this.btnActualizar.RadioEsquinas = 8;
            this.btnActualizar.Size = new System.Drawing.Size(130, 36);
            this.btnActualizar.TabIndex = 1;
            this.btnActualizar.Text = "Actualizar";
            this.btnActualizar.Click += new System.EventHandler(this.btnActualizar_Click);
            //
            // pnlCajaFiltro
            //
            this.pnlCajaFiltro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCajaFiltro.BackColor = System.Drawing.Color.White;
            this.pnlCajaFiltro.Controls.Add(this.txtFiltro);
            this.pnlCajaFiltro.Location = new System.Drawing.Point(356, 12);
            this.pnlCajaFiltro.Name = "pnlCajaFiltro";
            this.pnlCajaFiltro.Padding = new System.Windows.Forms.Padding(38, 5, 10, 5);
            this.pnlCajaFiltro.Size = new System.Drawing.Size(422, 36);
            this.pnlCajaFiltro.TabIndex = 2;
            //
            // txtFiltro
            //
            this.txtFiltro.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFiltro.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFiltro.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtFiltro.Location = new System.Drawing.Point(38, 5);
            this.txtFiltro.Name = "txtFiltro";
            this.txtFiltro.Size = new System.Drawing.Size(374, 25);
            this.txtFiltro.TabIndex = 0;
            //
            // btnConfiguracion
            //
            this.btnConfiguracion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfiguracion.BackColor = System.Drawing.Color.FromArgb(73, 80, 87);
            this.btnConfiguracion.CodigoIcono = 0xf013;
            this.btnConfiguracion.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnConfiguracion.ForeColor = System.Drawing.Color.White;
            this.btnConfiguracion.Location = new System.Drawing.Point(798, 12);
            this.btnConfiguracion.Name = "btnConfiguracion";
            this.btnConfiguracion.RadioEsquinas = 8;
            this.btnConfiguracion.Size = new System.Drawing.Size(142, 36);
            this.btnConfiguracion.TabIndex = 3;
            this.btnConfiguracion.Text = "Configuración";
            this.btnConfiguracion.Click += new System.EventHandler(this.btnConfiguracion_Click);
            //
            // pnlTablero
            //
            this.pnlTablero.AutoScroll = true;
            this.pnlTablero.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlTablero.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTablero.Location = new System.Drawing.Point(0, 150);
            this.pnlTablero.Name = "pnlTablero";
            this.pnlTablero.Padding = new System.Windows.Forms.Padding(16);
            this.pnlTablero.Size = new System.Drawing.Size(960, 452);
            this.pnlTablero.TabIndex = 3;
            //
            // frmCasilleroPrincipal
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(960, 602);
            this.Controls.Add(this.pnlTablero);
            this.Controls.Add(this.pnlResumen);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(700, 480);
            this.Name = "frmCasilleroPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Casilleros";
            this.pnlHeader.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlCajaFiltro.ResumeLayout(false);
            this.pnlCajaFiltro.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private SistemaFacturacion.Controls.IconButton btnCerrarVentana;
        private SistemaFacturacion.Controls.IconButton btnMinimizarVentana;
        private System.Windows.Forms.Label lblIconoTitulo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.FlowLayoutPanel pnlResumen;
        private System.Windows.Forms.Panel pnlToolbar;
        private SistemaFacturacion.Controls.IconButton btnAsignar;
        private SistemaFacturacion.Controls.IconButton btnActualizar;
        private System.Windows.Forms.Panel pnlCajaFiltro;
        private System.Windows.Forms.TextBox txtFiltro;
        private SistemaFacturacion.Controls.IconButton btnConfiguracion;
        private System.Windows.Forms.FlowLayoutPanel pnlTablero;
    }
}
