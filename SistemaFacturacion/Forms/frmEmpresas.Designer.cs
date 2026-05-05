namespace SistemaFacturacion
{
    partial class frmEmpresas
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnNuevo = new System.Windows.Forms.Button();
            this.pnlDatos = new System.Windows.Forms.Panel();
            this.cmbEstadoRuc = new System.Windows.Forms.ComboBox();
            this.cmbImpresion = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtImagen = new System.Windows.Forms.TextBox();
            this.txtContrasena = new System.Windows.Forms.TextBox();
            this.txtUbicacion = new System.Windows.Forms.TextBox();
            this.txtRuc = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtPropietario = new System.Windows.Forms.TextBox();
            this.txtTelefono = new System.Windows.Forms.TextBox();
            this.txtClaveTablas = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtClaveConsulta = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtClaveEliminar = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtClaveTotales = new System.Windows.Forms.TextBox();
            this.txtClaveIngreso = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDireccion = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnModificar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.txtNombreBuscar = new System.Windows.Forms.TextBox();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.gvEmpresa = new System.Windows.Forms.DataGridView();
            this.NOMBRE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DIRECCION = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CLAVEINGRESO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CLAVETOTALES = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CLAVEELIMINACION = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CLAVECONSULTA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CLAVETABLAS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RUC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FACTURACION = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IMPRESION = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TELEFONO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PROPIETARIO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EMAIL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UBICACIONARCHIVOP12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CONTRASENA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IMAGEN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ESTADORUC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvEmpresa)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNuevo
            // 
            this.btnNuevo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNuevo.Location = new System.Drawing.Point(410, 0);
            this.btnNuevo.Margin = new System.Windows.Forms.Padding(2);
            this.btnNuevo.Name = "btnNuevo";
            this.btnNuevo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnNuevo.Size = new System.Drawing.Size(92, 32);
            this.btnNuevo.TabIndex = 11;
            this.btnNuevo.Text = "NUEVO";
            this.btnNuevo.UseVisualStyleBackColor = true;
            this.btnNuevo.Click += new System.EventHandler(this.btnNuevo_Click);
            // 
            // pnlDatos
            // 
            this.pnlDatos.AutoScroll = true;
            this.pnlDatos.AutoScrollMinSize = new System.Drawing.Size(50, 0);
            this.pnlDatos.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlDatos.Controls.Add(this.cmbEstadoRuc);
            this.pnlDatos.Controls.Add(this.cmbImpresion);
            this.pnlDatos.Controls.Add(this.label19);
            this.pnlDatos.Controls.Add(this.label18);
            this.pnlDatos.Controls.Add(this.label17);
            this.pnlDatos.Controls.Add(this.label16);
            this.pnlDatos.Controls.Add(this.label14);
            this.pnlDatos.Controls.Add(this.label13);
            this.pnlDatos.Controls.Add(this.label12);
            this.pnlDatos.Controls.Add(this.label11);
            this.pnlDatos.Controls.Add(this.label10);
            this.pnlDatos.Controls.Add(this.txtImagen);
            this.pnlDatos.Controls.Add(this.txtContrasena);
            this.pnlDatos.Controls.Add(this.txtUbicacion);
            this.pnlDatos.Controls.Add(this.txtRuc);
            this.pnlDatos.Controls.Add(this.txtEmail);
            this.pnlDatos.Controls.Add(this.txtPropietario);
            this.pnlDatos.Controls.Add(this.txtTelefono);
            this.pnlDatos.Controls.Add(this.txtClaveTablas);
            this.pnlDatos.Controls.Add(this.label8);
            this.pnlDatos.Controls.Add(this.txtClaveConsulta);
            this.pnlDatos.Controls.Add(this.label7);
            this.pnlDatos.Controls.Add(this.txtClaveEliminar);
            this.pnlDatos.Controls.Add(this.label6);
            this.pnlDatos.Controls.Add(this.txtClaveTotales);
            this.pnlDatos.Controls.Add(this.txtClaveIngreso);
            this.pnlDatos.Controls.Add(this.label5);
            this.pnlDatos.Controls.Add(this.label4);
            this.pnlDatos.Controls.Add(this.txtDireccion);
            this.pnlDatos.Controls.Add(this.label3);
            this.pnlDatos.Controls.Add(this.txtNombre);
            this.pnlDatos.Controls.Add(this.label2);
            this.pnlDatos.Controls.Add(this.btnEliminar);
            this.pnlDatos.Controls.Add(this.btnModificar);
            this.pnlDatos.Controls.Add(this.btnGuardar);
            this.pnlDatos.Location = new System.Drawing.Point(9, 256);
            this.pnlDatos.Margin = new System.Windows.Forms.Padding(2);
            this.pnlDatos.Name = "pnlDatos";
            this.pnlDatos.Size = new System.Drawing.Size(977, 278);
            this.pnlDatos.TabIndex = 10;
            // 
            // cmbEstadoRuc
            // 
            this.cmbEstadoRuc.FormattingEnabled = true;
            this.cmbEstadoRuc.Items.AddRange(new object[] {
            "ACTIVO",
            "INACTIVO"});
            this.cmbEstadoRuc.Location = new System.Drawing.Point(111, 203);
            this.cmbEstadoRuc.Name = "cmbEstadoRuc";
            this.cmbEstadoRuc.Size = new System.Drawing.Size(110, 21);
            this.cmbEstadoRuc.TabIndex = 48;
            // 
            // cmbImpresion
            // 
            this.cmbImpresion.FormattingEnabled = true;
            this.cmbImpresion.Items.AddRange(new object[] {
            "SI",
            "NO"});
            this.cmbImpresion.Location = new System.Drawing.Point(562, 84);
            this.cmbImpresion.Name = "cmbImpresion";
            this.cmbImpresion.Size = new System.Drawing.Size(186, 21);
            this.cmbImpresion.TabIndex = 47;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(108, 185);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(69, 13);
            this.label19.TabIndex = 46;
            this.label19.Text = "Estado RUC:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(3, 185);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(45, 13);
            this.label18.TabIndex = 45;
            this.label18.Text = "Imagen:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(340, 185);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(64, 13);
            this.label17.TabIndex = 44;
            this.label17.Text = "Contraseña:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(474, 185);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(80, 13);
            this.label16.TabIndex = 43;
            this.label16.Text = "Ubicacion P12:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(233, 185);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(27, 13);
            this.label14.TabIndex = 41;
            this.label14.Text = "Ruc";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(346, 67);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(35, 13);
            this.label13.TabIndex = 40;
            this.label13.Text = "Email:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(159, 67);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 39;
            this.label12.Text = "Propietario:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(2, 67);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(52, 13);
            this.label11.TabIndex = 38;
            this.label11.Text = "Telefono:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(559, 67);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 13);
            this.label10.TabIndex = 37;
            this.label10.Text = "Impresion:";
            // 
            // txtImagen
            // 
            this.txtImagen.Location = new System.Drawing.Point(5, 204);
            this.txtImagen.Name = "txtImagen";
            this.txtImagen.Size = new System.Drawing.Size(100, 20);
            this.txtImagen.TabIndex = 35;
            // 
            // txtContrasena
            // 
            this.txtContrasena.Location = new System.Drawing.Point(343, 204);
            this.txtContrasena.Name = "txtContrasena";
            this.txtContrasena.Size = new System.Drawing.Size(126, 20);
            this.txtContrasena.TabIndex = 34;
            // 
            // txtUbicacion
            // 
            this.txtUbicacion.Location = new System.Drawing.Point(474, 204);
            this.txtUbicacion.Name = "txtUbicacion";
            this.txtUbicacion.Size = new System.Drawing.Size(274, 20);
            this.txtUbicacion.TabIndex = 33;
            // 
            // txtRuc
            // 
            this.txtRuc.Location = new System.Drawing.Point(227, 204);
            this.txtRuc.Name = "txtRuc";
            this.txtRuc.Size = new System.Drawing.Size(113, 20);
            this.txtRuc.TabIndex = 31;
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(349, 83);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(208, 20);
            this.txtEmail.TabIndex = 30;
            // 
            // txtPropietario
            // 
            this.txtPropietario.Location = new System.Drawing.Point(162, 84);
            this.txtPropietario.Name = "txtPropietario";
            this.txtPropietario.Size = new System.Drawing.Size(181, 20);
            this.txtPropietario.TabIndex = 28;
            // 
            // txtTelefono
            // 
            this.txtTelefono.Location = new System.Drawing.Point(5, 82);
            this.txtTelefono.Name = "txtTelefono";
            this.txtTelefono.Size = new System.Drawing.Size(151, 20);
            this.txtTelefono.TabIndex = 27;
            // 
            // txtClaveTablas
            // 
            this.txtClaveTablas.Location = new System.Drawing.Point(583, 144);
            this.txtClaveTablas.Margin = new System.Windows.Forms.Padding(2);
            this.txtClaveTablas.Name = "txtClaveTablas";
            this.txtClaveTablas.Size = new System.Drawing.Size(166, 20);
            this.txtClaveTablas.TabIndex = 23;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(590, 128);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Clave Tablas:";
            // 
            // txtClaveConsulta
            // 
            this.txtClaveConsulta.Location = new System.Drawing.Point(401, 144);
            this.txtClaveConsulta.Margin = new System.Windows.Forms.Padding(2);
            this.txtClaveConsulta.Name = "txtClaveConsulta";
            this.txtClaveConsulta.Size = new System.Drawing.Size(178, 20);
            this.txtClaveConsulta.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(398, 128);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(81, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Clave Consulta:";
            // 
            // txtClaveEliminar
            // 
            this.txtClaveEliminar.Location = new System.Drawing.Point(275, 143);
            this.txtClaveEliminar.Margin = new System.Windows.Forms.Padding(2);
            this.txtClaveEliminar.Name = "txtClaveEliminar";
            this.txtClaveEliminar.Size = new System.Drawing.Size(122, 20);
            this.txtClaveEliminar.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(272, 128);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Clave Eliminar:";
            // 
            // txtClaveTotales
            // 
            this.txtClaveTotales.Location = new System.Drawing.Point(127, 143);
            this.txtClaveTotales.Margin = new System.Windows.Forms.Padding(2);
            this.txtClaveTotales.Name = "txtClaveTotales";
            this.txtClaveTotales.Size = new System.Drawing.Size(144, 20);
            this.txtClaveTotales.TabIndex = 17;
            // 
            // txtClaveIngreso
            // 
            this.txtClaveIngreso.Location = new System.Drawing.Point(3, 143);
            this.txtClaveIngreso.Margin = new System.Windows.Forms.Padding(2);
            this.txtClaveIngreso.Name = "txtClaveIngreso";
            this.txtClaveIngreso.Size = new System.Drawing.Size(115, 20);
            this.txtClaveIngreso.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(124, 128);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Clave Total:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 128);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Clave Ingreso:";
            // 
            // txtDireccion
            // 
            this.txtDireccion.Location = new System.Drawing.Point(354, 25);
            this.txtDireccion.Margin = new System.Windows.Forms.Padding(2);
            this.txtDireccion.Name = "txtDireccion";
            this.txtDireccion.Size = new System.Drawing.Size(395, 20);
            this.txtDireccion.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(352, 10);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Direccion:";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(4, 26);
            this.txtNombre.Margin = new System.Windows.Forms.Padding(2);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(344, 20);
            this.txtNombre.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Nombre:";
            // 
            // btnEliminar
            // 
            this.btnEliminar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEliminar.Location = new System.Drawing.Point(796, 103);
            this.btnEliminar.Margin = new System.Windows.Forms.Padding(2);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(136, 43);
            this.btnEliminar.TabIndex = 8;
            this.btnEliminar.Text = "ELIMINAR";
            this.btnEliminar.UseVisualStyleBackColor = true;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            // 
            // btnModificar
            // 
            this.btnModificar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModificar.Location = new System.Drawing.Point(796, 164);
            this.btnModificar.Margin = new System.Windows.Forms.Padding(2);
            this.btnModificar.Name = "btnModificar";
            this.btnModificar.Size = new System.Drawing.Size(136, 43);
            this.btnModificar.TabIndex = 7;
            this.btnModificar.Text = "MODIFICAR";
            this.btnModificar.UseVisualStyleBackColor = true;
            this.btnModificar.Click += new System.EventHandler(this.btnModificar_Click);
            // 
            // btnGuardar
            // 
            this.btnGuardar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.Location = new System.Drawing.Point(796, 41);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(2);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(133, 43);
            this.btnGuardar.TabIndex = 6;
            this.btnGuardar.Text = "GUARDAR";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // txtNombreBuscar
            // 
            this.txtNombreBuscar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNombreBuscar.Location = new System.Drawing.Point(79, 6);
            this.txtNombreBuscar.Margin = new System.Windows.Forms.Padding(2);
            this.txtNombreBuscar.Name = "txtNombreBuscar";
            this.txtNombreBuscar.Size = new System.Drawing.Size(232, 26);
            this.txtNombreBuscar.TabIndex = 9;
            // 
            // btnBuscar
            // 
            this.btnBuscar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscar.Location = new System.Drawing.Point(314, 2);
            this.btnBuscar.Margin = new System.Windows.Forms.Padding(2);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnBuscar.Size = new System.Drawing.Size(92, 32);
            this.btnBuscar.TabIndex = 7;
            this.btnBuscar.Text = "BUSCAR";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // gvEmpresa
            // 
            this.gvEmpresa.AllowUserToAddRows = false;
            this.gvEmpresa.AllowUserToResizeRows = false;
            this.gvEmpresa.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvEmpresa.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NOMBRE,
            this.DIRECCION,
            this.CLAVEINGRESO,
            this.CLAVETOTALES,
            this.CLAVEELIMINACION,
            this.CLAVECONSULTA,
            this.CLAVETABLAS,
            this.RUC,
            this.FACTURACION,
            this.IMPRESION,
            this.TELEFONO,
            this.PROPIETARIO,
            this.EMAIL,
            this.UBICACIONARCHIVOP12,
            this.CONTRASENA,
            this.IMAGEN,
            this.ESTADORUC});
            this.gvEmpresa.Location = new System.Drawing.Point(9, 36);
            this.gvEmpresa.Margin = new System.Windows.Forms.Padding(2);
            this.gvEmpresa.Name = "gvEmpresa";
            this.gvEmpresa.RowTemplate.Height = 24;
            this.gvEmpresa.Size = new System.Drawing.Size(977, 212);
            this.gvEmpresa.TabIndex = 6;
            this.gvEmpresa.SelectionChanged += new System.EventHandler(this.gvEmpresa_SelectionChanged);
            // 
            // NOMBRE
            // 
            this.NOMBRE.DataPropertyName = "NOMBRE";
            this.NOMBRE.HeaderText = "NOMBRE";
            this.NOMBRE.Name = "NOMBRE";
            this.NOMBRE.Width = 200;
            // 
            // DIRECCION
            // 
            this.DIRECCION.DataPropertyName = "DIRECCION";
            dataGridViewCellStyle2.Format = "N2";
            this.DIRECCION.DefaultCellStyle = dataGridViewCellStyle2;
            this.DIRECCION.HeaderText = "DIRECCION";
            this.DIRECCION.Name = "DIRECCION";
            // 
            // CLAVEINGRESO
            // 
            this.CLAVEINGRESO.DataPropertyName = "CLAVEINGRESO";
            this.CLAVEINGRESO.HeaderText = "CLAVE INGRESO";
            this.CLAVEINGRESO.Name = "CLAVEINGRESO";
            this.CLAVEINGRESO.Width = 70;
            // 
            // CLAVETOTALES
            // 
            this.CLAVETOTALES.DataPropertyName = "CLAVETOTALES";
            this.CLAVETOTALES.HeaderText = "CLAVE TOTALES";
            this.CLAVETOTALES.Name = "CLAVETOTALES";
            this.CLAVETOTALES.Width = 70;
            // 
            // CLAVEELIMINACION
            // 
            this.CLAVEELIMINACION.DataPropertyName = "CLAVEELIMINACION";
            this.CLAVEELIMINACION.HeaderText = "CLAVE ELIMINACION";
            this.CLAVEELIMINACION.Name = "CLAVEELIMINACION";
            this.CLAVEELIMINACION.Width = 70;
            // 
            // CLAVECONSULTA
            // 
            this.CLAVECONSULTA.DataPropertyName = "CLAVECONSULTA";
            this.CLAVECONSULTA.HeaderText = "CLAVE CONSULTA";
            this.CLAVECONSULTA.Name = "CLAVECONSULTA";
            this.CLAVECONSULTA.Width = 70;
            // 
            // CLAVETABLAS
            // 
            this.CLAVETABLAS.DataPropertyName = "CLAVETABLAS";
            this.CLAVETABLAS.HeaderText = "CLAVE TABLAS";
            this.CLAVETABLAS.Name = "CLAVETABLAS";
            this.CLAVETABLAS.Width = 70;
            // 
            // RUC
            // 
            this.RUC.DataPropertyName = "RUC";
            this.RUC.HeaderText = "RUC";
            this.RUC.Name = "RUC";
            // 
            // FACTURACION
            // 
            this.FACTURACION.DataPropertyName = "FACTURACION";
            this.FACTURACION.HeaderText = "FACTURACION";
            this.FACTURACION.Name = "FACTURACION";
            // 
            // IMPRESION
            // 
            this.IMPRESION.DataPropertyName = "IMPRESION";
            this.IMPRESION.HeaderText = "IMPRESION";
            this.IMPRESION.Name = "IMPRESION";
            // 
            // TELEFONO
            // 
            this.TELEFONO.DataPropertyName = "TELEFONO";
            this.TELEFONO.HeaderText = "TELEFONO";
            this.TELEFONO.Name = "TELEFONO";
            // 
            // PROPIETARIO
            // 
            this.PROPIETARIO.DataPropertyName = "PROPIETARIO";
            this.PROPIETARIO.HeaderText = "PROPIETARIO";
            this.PROPIETARIO.Name = "PROPIETARIO";
            // 
            // EMAIL
            // 
            this.EMAIL.DataPropertyName = "EMAIL";
            this.EMAIL.HeaderText = "EMAIL";
            this.EMAIL.Name = "EMAIL";
            // 
            // UBICACIONARCHIVOP12
            // 
            this.UBICACIONARCHIVOP12.DataPropertyName = "UBICACIONARCHIVOP12";
            this.UBICACIONARCHIVOP12.HeaderText = "UBICACION ARCHIVO P12";
            this.UBICACIONARCHIVOP12.Name = "UBICACIONARCHIVOP12";
            // 
            // CONTRASENA
            // 
            this.CONTRASENA.DataPropertyName = "CONTRASENA";
            this.CONTRASENA.HeaderText = "CONTRASEÑA";
            this.CONTRASENA.Name = "CONTRASENA";
            // 
            // IMAGEN
            // 
            this.IMAGEN.DataPropertyName = "IMAGEN";
            this.IMAGEN.HeaderText = "IMAGEN";
            this.IMAGEN.Name = "IMAGEN";
            // 
            // ESTADORUC
            // 
            this.ESTADORUC.DataPropertyName = "ESTADORUC";
            this.ESTADORUC.HeaderText = "ESTADO RUC";
            this.ESTADORUC.Name = "ESTADORUC";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "Nombre:";
            // 
            // frmEmpresas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1013, 556);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.pnlDatos);
            this.Controls.Add(this.txtNombreBuscar);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.gvEmpresa);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "frmEmpresas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EMPRESAS";
            this.Load += new System.EventHandler(this.frmEmpresas_Load);
            this.pnlDatos.ResumeLayout(false);
            this.pnlDatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gvEmpresa)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnNuevo;
        private System.Windows.Forms.Panel pnlDatos;
        private System.Windows.Forms.TextBox txtClaveIngreso;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDireccion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Button btnModificar;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.TextBox txtNombreBuscar;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.DataGridView gvEmpresa;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtClaveConsulta;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtClaveEliminar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtClaveTotales;
        private System.Windows.Forms.TextBox txtClaveTablas;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtRuc;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtPropietario;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtImagen;
        private System.Windows.Forms.TextBox txtContrasena;
        private System.Windows.Forms.TextBox txtUbicacion;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.DataGridViewTextBoxColumn NOMBRE;
        private System.Windows.Forms.DataGridViewTextBoxColumn DIRECCION;
        private System.Windows.Forms.DataGridViewTextBoxColumn CLAVEINGRESO;
        private System.Windows.Forms.DataGridViewTextBoxColumn CLAVETOTALES;
        private System.Windows.Forms.DataGridViewTextBoxColumn CLAVEELIMINACION;
        private System.Windows.Forms.DataGridViewTextBoxColumn CLAVECONSULTA;
        private System.Windows.Forms.DataGridViewTextBoxColumn CLAVETABLAS;
        private System.Windows.Forms.DataGridViewTextBoxColumn RUC;
        private System.Windows.Forms.DataGridViewTextBoxColumn FACTURACION;
        private System.Windows.Forms.DataGridViewTextBoxColumn IMPRESION;
        private System.Windows.Forms.DataGridViewTextBoxColumn TELEFONO;
        private System.Windows.Forms.DataGridViewTextBoxColumn PROPIETARIO;
        private System.Windows.Forms.DataGridViewTextBoxColumn EMAIL;
        private System.Windows.Forms.DataGridViewTextBoxColumn UBICACIONARCHIVOP12;
        private System.Windows.Forms.DataGridViewTextBoxColumn CONTRASENA;
        private System.Windows.Forms.DataGridViewTextBoxColumn IMAGEN;
        private System.Windows.Forms.DataGridViewTextBoxColumn ESTADORUC;
        private System.Windows.Forms.ComboBox cmbEstadoRuc;
        private System.Windows.Forms.ComboBox cmbImpresion;
    }
}