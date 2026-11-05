namespace SistemaFacturacion
{
    partial class frmAsignarCasillero
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
            this.pnlCajaCliente = new System.Windows.Forms.Panel();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.lblMinutosEtiqueta = new System.Windows.Forms.Label();
            this.btnChip30 = new SistemaFacturacion.Controls.IconButton();
            this.btnChip60 = new SistemaFacturacion.Controls.IconButton();
            this.btnChip90 = new SistemaFacturacion.Controls.IconButton();
            this.btnChip120 = new SistemaFacturacion.Controls.IconButton();
            this.btnChipOtro = new SistemaFacturacion.Controls.IconButton();
            this.lblMinutosPersonalizadoEtiqueta = new System.Windows.Forms.Label();
            this.pnlCajaMinutos = new System.Windows.Forms.Panel();
            this.numMinutos = new System.Windows.Forms.NumericUpDown();
            this.lblError = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnAsignar = new SistemaFacturacion.Controls.IconButton();
            this.btnCancelar = new SistemaFacturacion.Controls.IconButton();
            this.pnlHeader.SuspendLayout();
            this.pnlCuerpo.SuspendLayout();
            this.pnlCajaCliente.SuspendLayout();
            this.pnlCajaMinutos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinutos)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
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
            this.btnCerrarVentana.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
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
            this.btnCerrarVentana.Click += new System.EventHandler(this.btnCancelar_Click);
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
            this.lblTitulo.Text = "Asignar casillero";
            //
            // pnlCuerpo
            //
            this.pnlCuerpo.BackColor = System.Drawing.Color.White;
            this.pnlCuerpo.Controls.Add(this.lblClienteEtiqueta);
            this.pnlCuerpo.Controls.Add(this.pnlCajaCliente);
            this.pnlCuerpo.Controls.Add(this.lblMinutosEtiqueta);
            this.pnlCuerpo.Controls.Add(this.btnChip30);
            this.pnlCuerpo.Controls.Add(this.btnChip60);
            this.pnlCuerpo.Controls.Add(this.btnChip90);
            this.pnlCuerpo.Controls.Add(this.btnChip120);
            this.pnlCuerpo.Controls.Add(this.btnChipOtro);
            this.pnlCuerpo.Controls.Add(this.lblMinutosPersonalizadoEtiqueta);
            this.pnlCuerpo.Controls.Add(this.pnlCajaMinutos);
            this.pnlCuerpo.Controls.Add(this.lblError);
            this.pnlCuerpo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCuerpo.Location = new System.Drawing.Point(0, 56);
            this.pnlCuerpo.Name = "pnlCuerpo";
            this.pnlCuerpo.Size = new System.Drawing.Size(380, 374);
            this.pnlCuerpo.TabIndex = 1;
            //
            // lblClienteEtiqueta
            //
            this.lblClienteEtiqueta.AutoSize = true;
            this.lblClienteEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblClienteEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblClienteEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblClienteEtiqueta.Location = new System.Drawing.Point(72, 148);
            this.lblClienteEtiqueta.Name = "lblClienteEtiqueta";
            this.lblClienteEtiqueta.Size = new System.Drawing.Size(45, 13);
            this.lblClienteEtiqueta.TabIndex = 1;
            this.lblClienteEtiqueta.Text = "CLIENTE";
            //
            // pnlCajaCliente
            //
            this.pnlCajaCliente.BackColor = System.Drawing.Color.White;
            this.pnlCajaCliente.Controls.Add(this.txtCliente);
            this.pnlCajaCliente.Location = new System.Drawing.Point(70, 160);
            this.pnlCajaCliente.Name = "pnlCajaCliente";
            this.pnlCajaCliente.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.pnlCajaCliente.Size = new System.Drawing.Size(270, 33);
            this.pnlCajaCliente.TabIndex = 2;
            //
            // txtCliente
            //
            this.txtCliente.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCliente.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCliente.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtCliente.Location = new System.Drawing.Point(10, 5);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.Size = new System.Drawing.Size(250, 23);
            this.txtCliente.TabIndex = 0;
            //
            // lblMinutosEtiqueta
            //
            this.lblMinutosEtiqueta.AutoSize = true;
            this.lblMinutosEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblMinutosEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblMinutosEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblMinutosEtiqueta.Location = new System.Drawing.Point(72, 212);
            this.lblMinutosEtiqueta.Name = "lblMinutosEtiqueta";
            this.lblMinutosEtiqueta.Size = new System.Drawing.Size(96, 13);
            this.lblMinutosEtiqueta.TabIndex = 3;
            this.lblMinutosEtiqueta.Text = "MINUTOS DE USO";
            //
            // btnChip30
            //
            this.btnChip30.BackColor = System.Drawing.Color.FromArgb(241, 243, 245);
            this.btnChip30.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnChip30.ForeColor = System.Drawing.Color.FromArgb(73, 80, 87);
            this.btnChip30.Location = new System.Drawing.Point(70, 228);
            this.btnChip30.Name = "btnChip30";
            this.btnChip30.RadioEsquinas = 13;
            this.btnChip30.Size = new System.Drawing.Size(50, 26);
            this.btnChip30.TabIndex = 4;
            this.btnChip30.Text = "30m";
            this.btnChip30.Click += new System.EventHandler(this.BtnChip_Click);
            //
            // btnChip60
            //
            this.btnChip60.BackColor = System.Drawing.Color.FromArgb(241, 243, 245);
            this.btnChip60.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnChip60.ForeColor = System.Drawing.Color.FromArgb(73, 80, 87);
            this.btnChip60.Location = new System.Drawing.Point(125, 228);
            this.btnChip60.Name = "btnChip60";
            this.btnChip60.RadioEsquinas = 13;
            this.btnChip60.Size = new System.Drawing.Size(50, 26);
            this.btnChip60.TabIndex = 5;
            this.btnChip60.Text = "1h";
            this.btnChip60.Click += new System.EventHandler(this.BtnChip_Click);
            //
            // btnChip90
            //
            this.btnChip90.BackColor = System.Drawing.Color.FromArgb(241, 243, 245);
            this.btnChip90.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnChip90.ForeColor = System.Drawing.Color.FromArgb(73, 80, 87);
            this.btnChip90.Location = new System.Drawing.Point(180, 228);
            this.btnChip90.Name = "btnChip90";
            this.btnChip90.RadioEsquinas = 13;
            this.btnChip90.Size = new System.Drawing.Size(50, 26);
            this.btnChip90.TabIndex = 6;
            this.btnChip90.Text = "1h30";
            this.btnChip90.Click += new System.EventHandler(this.BtnChip_Click);
            //
            // btnChip120
            //
            this.btnChip120.BackColor = System.Drawing.Color.FromArgb(241, 243, 245);
            this.btnChip120.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnChip120.ForeColor = System.Drawing.Color.FromArgb(73, 80, 87);
            this.btnChip120.Location = new System.Drawing.Point(235, 228);
            this.btnChip120.Name = "btnChip120";
            this.btnChip120.RadioEsquinas = 13;
            this.btnChip120.Size = new System.Drawing.Size(50, 26);
            this.btnChip120.TabIndex = 7;
            this.btnChip120.Text = "2h";
            this.btnChip120.Click += new System.EventHandler(this.BtnChip_Click);
            //
            // btnChipOtro
            //
            this.btnChipOtro.BackColor = System.Drawing.Color.FromArgb(241, 243, 245);
            this.btnChipOtro.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnChipOtro.ForeColor = System.Drawing.Color.FromArgb(73, 80, 87);
            this.btnChipOtro.Location = new System.Drawing.Point(290, 228);
            this.btnChipOtro.Name = "btnChipOtro";
            this.btnChipOtro.RadioEsquinas = 13;
            this.btnChipOtro.Size = new System.Drawing.Size(50, 26);
            this.btnChipOtro.TabIndex = 8;
            this.btnChipOtro.Text = "Otro";
            this.btnChipOtro.Click += new System.EventHandler(this.BtnChip_Click);
            //
            // lblMinutosPersonalizadoEtiqueta
            //
            this.lblMinutosPersonalizadoEtiqueta.AutoSize = true;
            this.lblMinutosPersonalizadoEtiqueta.BackColor = System.Drawing.Color.Transparent;
            this.lblMinutosPersonalizadoEtiqueta.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblMinutosPersonalizadoEtiqueta.ForeColor = System.Drawing.Color.FromArgb(148, 155, 162);
            this.lblMinutosPersonalizadoEtiqueta.Location = new System.Drawing.Point(72, 260);
            this.lblMinutosPersonalizadoEtiqueta.Name = "lblMinutosPersonalizadoEtiqueta";
            this.lblMinutosPersonalizadoEtiqueta.Size = new System.Drawing.Size(140, 13);
            this.lblMinutosPersonalizadoEtiqueta.TabIndex = 9;
            this.lblMinutosPersonalizadoEtiqueta.Text = "MINUTOS PERSONALIZADOS";
            this.lblMinutosPersonalizadoEtiqueta.Visible = false;
            //
            // pnlCajaMinutos
            //
            this.pnlCajaMinutos.BackColor = System.Drawing.Color.White;
            this.pnlCajaMinutos.Controls.Add(this.numMinutos);
            this.pnlCajaMinutos.Location = new System.Drawing.Point(70, 274);
            this.pnlCajaMinutos.Name = "pnlCajaMinutos";
            this.pnlCajaMinutos.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.pnlCajaMinutos.Size = new System.Drawing.Size(120, 31);
            this.pnlCajaMinutos.TabIndex = 10;
            this.pnlCajaMinutos.Visible = false;
            //
            // numMinutos
            //
            this.numMinutos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numMinutos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numMinutos.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.numMinutos.Location = new System.Drawing.Point(10, 4);
            this.numMinutos.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            this.numMinutos.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMinutos.Name = "numMinutos";
            this.numMinutos.Size = new System.Drawing.Size(100, 23);
            this.numMinutos.TabIndex = 0;
            this.numMinutos.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMinutos.Value = new decimal(new int[] { 60, 0, 0, 0 });
            //
            // lblError
            //
            this.lblError.AutoSize = true;
            this.lblError.BackColor = System.Drawing.Color.Transparent;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblError.Location = new System.Drawing.Point(24, 322);
            this.lblError.MaximumSize = new System.Drawing.Size(332, 0);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(0, 15);
            this.lblError.TabIndex = 11;
            //
            // pnlFooter
            //
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(247, 248, 249);
            this.pnlFooter.Controls.Add(this.btnAsignar);
            this.pnlFooter.Controls.Add(this.btnCancelar);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 430);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(380, 68);
            this.pnlFooter.TabIndex = 2;
            //
            // btnAsignar
            //
            this.btnAsignar.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnAsignar.CodigoIcono = 0xf067;
            this.btnAsignar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAsignar.ForeColor = System.Drawing.Color.White;
            this.btnAsignar.Location = new System.Drawing.Point(24, 16);
            this.btnAsignar.Name = "btnAsignar";
            this.btnAsignar.RadioEsquinas = 8;
            this.btnAsignar.Size = new System.Drawing.Size(160, 38);
            this.btnAsignar.TabIndex = 6;
            this.btnAsignar.Text = "Asignar";
            this.btnAsignar.Click += new System.EventHandler(this.btnAsignar_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnCancelar.CodigoIcono = 0xf00d;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(196, 16);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.RadioEsquinas = 8;
            this.btnCancelar.Size = new System.Drawing.Size(160, 38);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // frmAsignarCasillero
            //
            this.AcceptButton = this.btnAsignar;
            this.CancelButton = this.btnCancelar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(380, 498);
            this.Controls.Add(this.pnlCuerpo);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAsignarCasillero";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Asignar casillero";
            this.pnlHeader.ResumeLayout(false);
            this.pnlCuerpo.ResumeLayout(false);
            this.pnlCuerpo.PerformLayout();
            this.pnlCajaCliente.ResumeLayout(false);
            this.pnlCajaCliente.PerformLayout();
            this.pnlCajaMinutos.ResumeLayout(false);
            this.pnlCajaMinutos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinutos)).EndInit();
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
        private System.Windows.Forms.Panel pnlCajaCliente;
        private System.Windows.Forms.TextBox txtCliente;
        private System.Windows.Forms.Label lblMinutosEtiqueta;
        private SistemaFacturacion.Controls.IconButton btnChip30;
        private SistemaFacturacion.Controls.IconButton btnChip60;
        private SistemaFacturacion.Controls.IconButton btnChip90;
        private SistemaFacturacion.Controls.IconButton btnChip120;
        private SistemaFacturacion.Controls.IconButton btnChipOtro;
        private System.Windows.Forms.Label lblMinutosPersonalizadoEtiqueta;
        private System.Windows.Forms.Panel pnlCajaMinutos;
        private System.Windows.Forms.NumericUpDown numMinutos;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Panel pnlFooter;
        private SistemaFacturacion.Controls.IconButton btnAsignar;
        private SistemaFacturacion.Controls.IconButton btnCancelar;
    }
}
