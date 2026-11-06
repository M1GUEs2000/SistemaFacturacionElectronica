namespace SistemaFacturacion
{
    partial class frmCasilleroConfig
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
            this.lblCantidadEtiqueta = new System.Windows.Forms.Label();
            this.pnlCajaCantidad = new System.Windows.Forms.Panel();
            this.numCantidad = new System.Windows.Forms.NumericUpDown();
            this.lblCantidadActual = new System.Windows.Forms.Label();
            this.lblMinutosEtiqueta = new System.Windows.Forms.Label();
            this.pnlCajaMinutos = new System.Windows.Forms.Panel();
            this.numMinutos = new System.Windows.Forms.NumericUpDown();
            this.lblSonidoEtiqueta = new System.Windows.Forms.Label();
            this.pnlCajaSonido = new System.Windows.Forms.Panel();
            this.cmbSonido = new System.Windows.Forms.ComboBox();
            this.btnAgregarSonido = new SistemaFacturacion.Controls.IconButton();
            this.btnProbarSonido = new SistemaFacturacion.Controls.IconButton();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnGuardar = new SistemaFacturacion.Controls.IconButton();
            this.btnCerrar = new SistemaFacturacion.Controls.IconButton();
            this.pnlHeader.SuspendLayout();
            this.pnlCuerpo.SuspendLayout();
            this.pnlCajaCantidad.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).BeginInit();
            this.pnlCajaMinutos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinutos)).BeginInit();
            this.pnlCajaSonido.SuspendLayout();
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
            this.pnlHeader.Size = new System.Drawing.Size(400, 56);
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
            this.btnCerrarVentana.ColorHoverPersonalizado = System.Drawing.Color.FromArgb(220, 53, 69);
            this.btnCerrarVentana.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCerrarVentana.FlatAppearance.BorderSize = 0;
            this.btnCerrarVentana.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnCerrarVentana.ForeColor = System.Drawing.Color.White;
            this.btnCerrarVentana.Name = "btnCerrarVentana";
            this.btnCerrarVentana.RadioEsquinas = 0;
            this.btnCerrarVentana.Size = new System.Drawing.Size(46, 56);
            this.btnCerrarVentana.TabIndex = 9;
            this.btnCerrarVentana.UseVisualStyleBackColor = false;
            this.btnCerrarVentana.Click += new System.EventHandler(this.btnCerrarVentana_Click);
            //
            // lblTitulo
            //
            this.lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(310, 56);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Configuración de Casilleros";
            //
            // pnlCuerpo
            //
            this.pnlCuerpo.BackColor = System.Drawing.Color.White;
            this.pnlCuerpo.Controls.Add(this.lblCantidadEtiqueta);
            this.pnlCuerpo.Controls.Add(this.pnlCajaCantidad);
            this.pnlCuerpo.Controls.Add(this.lblCantidadActual);
            this.pnlCuerpo.Controls.Add(this.lblMinutosEtiqueta);
            this.pnlCuerpo.Controls.Add(this.pnlCajaMinutos);
            this.pnlCuerpo.Controls.Add(this.lblSonidoEtiqueta);
            this.pnlCuerpo.Controls.Add(this.pnlCajaSonido);
            this.pnlCuerpo.Controls.Add(this.btnAgregarSonido);
            this.pnlCuerpo.Controls.Add(this.btnProbarSonido);
            this.pnlCuerpo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCuerpo.Location = new System.Drawing.Point(0, 56);
            this.pnlCuerpo.Name = "pnlCuerpo";
            this.pnlCuerpo.Size = new System.Drawing.Size(400, 274);
            this.pnlCuerpo.TabIndex = 1;
            //
            // lblCantidadEtiqueta
            //
            this.lblCantidadEtiqueta.AutoSize = true;
            this.lblCantidadEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblCantidadEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCantidadEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblCantidadEtiqueta.Location = new System.Drawing.Point(72, 28);
            this.lblCantidadEtiqueta.Name = "lblCantidadEtiqueta";
            this.lblCantidadEtiqueta.Size = new System.Drawing.Size(140, 13);
            this.lblCantidadEtiqueta.TabIndex = 0;
            this.lblCantidadEtiqueta.Text = "CANTIDAD DE CASILLEROS";
            //
            // pnlCajaCantidad
            //
            this.pnlCajaCantidad.BackColor = System.Drawing.Color.White;
            this.pnlCajaCantidad.Controls.Add(this.numCantidad);
            this.pnlCajaCantidad.Location = new System.Drawing.Point(72, 44);
            this.pnlCajaCantidad.Name = "pnlCajaCantidad";
            this.pnlCajaCantidad.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.pnlCajaCantidad.Size = new System.Drawing.Size(100, 33);
            this.pnlCajaCantidad.TabIndex = 1;
            //
            // numCantidad
            //
            this.numCantidad.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numCantidad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numCantidad.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numCantidad.Location = new System.Drawing.Point(10, 4);
            this.numCantidad.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this.numCantidad.Name = "numCantidad";
            this.numCantidad.Size = new System.Drawing.Size(80, 23);
            this.numCantidad.TabIndex = 0;
            this.numCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // lblCantidadActual
            //
            this.lblCantidadActual.AutoSize = true;
            this.lblCantidadActual.BackColor = System.Drawing.Color.Transparent;
            this.lblCantidadActual.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            this.lblCantidadActual.ForeColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.lblCantidadActual.Location = new System.Drawing.Point(72, 82);
            this.lblCantidadActual.Name = "lblCantidadActual";
            this.lblCantidadActual.Size = new System.Drawing.Size(0, 15);
            this.lblCantidadActual.TabIndex = 2;
            //
            // lblMinutosEtiqueta
            //
            this.lblMinutosEtiqueta.AutoSize = true;
            this.lblMinutosEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblMinutosEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblMinutosEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblMinutosEtiqueta.Location = new System.Drawing.Point(72, 118);
            this.lblMinutosEtiqueta.Name = "lblMinutosEtiqueta";
            this.lblMinutosEtiqueta.Size = new System.Drawing.Size(190, 13);
            this.lblMinutosEtiqueta.TabIndex = 3;
            this.lblMinutosEtiqueta.Text = "MINUTOS DE AVISO PREVIO";
            //
            // pnlCajaMinutos
            //
            this.pnlCajaMinutos.BackColor = System.Drawing.Color.White;
            this.pnlCajaMinutos.Controls.Add(this.numMinutos);
            this.pnlCajaMinutos.Location = new System.Drawing.Point(72, 134);
            this.pnlCajaMinutos.Name = "pnlCajaMinutos";
            this.pnlCajaMinutos.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.pnlCajaMinutos.Size = new System.Drawing.Size(100, 33);
            this.pnlCajaMinutos.TabIndex = 4;
            //
            // numMinutos
            //
            this.numMinutos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numMinutos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numMinutos.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numMinutos.Location = new System.Drawing.Point(10, 4);
            this.numMinutos.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            this.numMinutos.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMinutos.Name = "numMinutos";
            this.numMinutos.Size = new System.Drawing.Size(80, 23);
            this.numMinutos.TabIndex = 0;
            this.numMinutos.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMinutos.Value = new decimal(new int[] { 10, 0, 0, 0 });
            //
            // lblSonidoEtiqueta
            //
            this.lblSonidoEtiqueta.AutoSize = true;
            this.lblSonidoEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblSonidoEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblSonidoEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblSonidoEtiqueta.Location = new System.Drawing.Point(72, 188);
            this.lblSonidoEtiqueta.Name = "lblSonidoEtiqueta";
            this.lblSonidoEtiqueta.Size = new System.Drawing.Size(160, 13);
            this.lblSonidoEtiqueta.TabIndex = 5;
            this.lblSonidoEtiqueta.Text = "SONIDO DE NOTIFICACIÓN";
            //
            // pnlCajaSonido
            //
            this.pnlCajaSonido.BackColor = System.Drawing.Color.White;
            this.pnlCajaSonido.Controls.Add(this.cmbSonido);
            this.pnlCajaSonido.Location = new System.Drawing.Point(72, 204);
            this.pnlCajaSonido.Name = "pnlCajaSonido";
            this.pnlCajaSonido.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.pnlCajaSonido.Size = new System.Drawing.Size(196, 33);
            this.pnlCajaSonido.TabIndex = 6;
            //
            // cmbSonido
            //
            this.cmbSonido.BackColor = System.Drawing.Color.White;
            this.cmbSonido.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbSonido.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSonido.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbSonido.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbSonido.FormattingEnabled = true;
            this.cmbSonido.Location = new System.Drawing.Point(10, 4);
            this.cmbSonido.Name = "cmbSonido";
            this.cmbSonido.Size = new System.Drawing.Size(176, 25);
            this.cmbSonido.TabIndex = 0;
            //
            // btnAgregarSonido
            //
            this.btnAgregarSonido.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnAgregarSonido.CodigoIcono = SistemaFacturacion.Controls.IconosCasilleros.Agregar;
            this.btnAgregarSonido.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAgregarSonido.ForeColor = System.Drawing.Color.White;
            this.btnAgregarSonido.Location = new System.Drawing.Point(276, 204);
            this.btnAgregarSonido.Name = "btnAgregarSonido";
            this.btnAgregarSonido.RadioEsquinas = 8;
            this.btnAgregarSonido.Size = new System.Drawing.Size(33, 33);
            this.btnAgregarSonido.TabIndex = 7;
            this.btnAgregarSonido.UseVisualStyleBackColor = false;
            this.btnAgregarSonido.Click += new System.EventHandler(this.btnAgregarSonido_Click);
            //
            // btnProbarSonido
            //
            this.btnProbarSonido.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnProbarSonido.CodigoIcono = SistemaFacturacion.Controls.IconosCasilleros.Reproducir;
            this.btnProbarSonido.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnProbarSonido.ForeColor = System.Drawing.Color.White;
            this.btnProbarSonido.Location = new System.Drawing.Point(313, 204);
            this.btnProbarSonido.Name = "btnProbarSonido";
            this.btnProbarSonido.RadioEsquinas = 8;
            this.btnProbarSonido.Size = new System.Drawing.Size(33, 33);
            this.btnProbarSonido.TabIndex = 8;
            this.btnProbarSonido.UseVisualStyleBackColor = false;
            this.btnProbarSonido.Click += new System.EventHandler(this.btnProbarSonido_Click);
            //
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(247, 248, 249);
            this.pnlFooter.Controls.Add(this.btnGuardar);
            this.pnlFooter.Controls.Add(this.btnCerrar);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 330);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(400, 68);
            this.pnlFooter.TabIndex = 2;
            //
            // btnGuardar
            //
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnGuardar.CodigoIcono = 0xf0c7;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(24, 16);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.RadioEsquinas = 8;
            this.btnGuardar.Size = new System.Drawing.Size(168, 38);
            this.btnGuardar.TabIndex = 0;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // btnCerrar
            //
            this.btnCerrar.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnCerrar.CodigoIcono = 0xf00d;
            this.btnCerrar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCerrar.ForeColor = System.Drawing.Color.White;
            this.btnCerrar.Location = new System.Drawing.Point(208, 16);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.RadioEsquinas = 8;
            this.btnCerrar.Size = new System.Drawing.Size(168, 38);
            this.btnCerrar.TabIndex = 1;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrarVentana_Click);
            //
            // frmCasilleroConfig
            //
            this.AcceptButton = this.btnGuardar;
            this.CancelButton = this.btnCerrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(400, 398);
            this.Controls.Add(this.pnlCuerpo);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCasilleroConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configuración de Casilleros";
            this.pnlHeader.ResumeLayout(false);
            this.pnlCuerpo.ResumeLayout(false);
            this.pnlCuerpo.PerformLayout();
            this.pnlCajaCantidad.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numCantidad)).EndInit();
            this.pnlCajaMinutos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMinutos)).EndInit();
            this.pnlCajaSonido.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblIconoTitulo;
        private SistemaFacturacion.Controls.IconButton btnCerrarVentana;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Panel pnlCuerpo;
        private System.Windows.Forms.Label lblCantidadEtiqueta;
        private System.Windows.Forms.Panel pnlCajaCantidad;
        private System.Windows.Forms.NumericUpDown numCantidad;
        private System.Windows.Forms.Label lblCantidadActual;
        private System.Windows.Forms.Label lblMinutosEtiqueta;
        private System.Windows.Forms.Panel pnlCajaMinutos;
        private System.Windows.Forms.NumericUpDown numMinutos;
        private System.Windows.Forms.Label lblSonidoEtiqueta;
        private System.Windows.Forms.Panel pnlCajaSonido;
        private System.Windows.Forms.ComboBox cmbSonido;
        private SistemaFacturacion.Controls.IconButton btnAgregarSonido;
        private SistemaFacturacion.Controls.IconButton btnProbarSonido;
        private System.Windows.Forms.Panel pnlFooter;
        private SistemaFacturacion.Controls.IconButton btnGuardar;
        private SistemaFacturacion.Controls.IconButton btnCerrar;
    }
}
