using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmEmpresas : Form
    {
        private readonly AppServices _services;

        // Antes se usaba btnNuevo.Visible (siempre false) como bandera; el botón ya no existe.
        private readonly bool PermitirCrearEmpresa = false;

        public frmEmpresas(
            AppServices services
        )
        {
            _services = services;
            InitializeComponent();
            txtUbicacion.TextChanged += CamposFirma_TextChanged;
            txtContrasena.TextChanged += CamposFirma_TextChanged;
        }
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }


        private void frmEmpresas_Load(object sender, EventArgs e)
        {
            CargarCombos();

            MostrarEmpresa("");
            CargarRucProveedor();
            btnModificar.Visible = false;
            btnGuardar.Visible = false;
            ActualizarVisibilidadVerificarFirma();
        }

        private void CargarRucProveedor()
        {
            try
            {
                txtRucProveedor.Text = _services.Empresa.ObtenerRucProveedor();
            }
            catch (Exception ex)
            {
                txtRucProveedor.Text = "";
                MessageBox.Show("Error al cargar el RUC del proveedor: " + ex.Message, "Mensaje");
            }
        }
        public void MostrarEmpresa(string Nombre)
        {
            DataSet dsDatos = new DataSet();
            try
            {
                dsDatos = _services.Empresa.MostrarEmpresa(Nombre);

                gvEmpresa.AutoGenerateColumns = false;
                gvEmpresa.DataSource = dsDatos.Tables[0];

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message.ToString(), "Mensaje ");
            }
        }

        public void Limpiar()
        {
            btnModificar.Visible = false;
            btnGuardar.Visible = false;
            txtNombre.Enabled = true;

            txtNombre.Text = "";
            txtDireccion.Text = "";
            txtClaveIngreso.Text = "";
            txtClaveTotales.Text = "";
            txtClaveEliminar.Text = "";
            txtClaveConsulta.Text = "";
            txtClaveTablas.Text = "";
            txtTelefono.Text = "";
            txtPropietario.Text = "";
            txtEmail.Text = "";
            txtRuc.Text = "";
            txtUbicacion.Text = "";
            txtContrasena.Text = "";
            txtImagen.Text = "";
            cmbImpresion.SelectedIndex = -1;
            cmbEstadoRuc.SelectedIndex = -1;
            ActualizarVisibilidadVerificarFirma();
        }


        private void btnBuscar_Click(object sender, EventArgs e)
        {
            MostrarEmpresa(txtNombreBuscar.Text);
        }

        private void btnSeleccionarFirma_Click(object sender, EventArgs e)
        {
            SeleccionarYCopiarArchivo(
                "Firma electrónica",
                "Certificados P12 (*.p12)|*.p12",
                "FIRMAELECTRONICA",
                txtUbicacion);
        }

        private void CamposFirma_TextChanged(object sender, EventArgs e)
        {
            ActualizarVisibilidadVerificarFirma();
        }

        private void ActualizarVisibilidadVerificarFirma()
        {
            btnVerificarFirma.Visible =
                !string.IsNullOrWhiteSpace(txtUbicacion.Text) &&
                !string.IsNullOrWhiteSpace(txtContrasena.Text);
        }

        private void btnVerificarFirma_Click(object sender, EventArgs e)
        {
            string rutaP12 = Path.Combine(
                _services.Paths.General,
                "FIRMAELECTRONICA",
                Path.GetFileName(txtUbicacion.Text.Trim()));

            try
            {
                var certificado = new FirmadorNativo().ValidarCertificado(
                    rutaP12,
                    txtContrasena.Text);

                Notificaciones.Show(
                    this,
                    "Firma válida. Vence el " + certificado.VigenteHasta.ToString("dd/MM/yyyy") + ".",
                    "exito");
            }
            catch (CryptographicException)
            {
                Notificaciones.Show(
                    this,
                    "No se pudo abrir el P12. Verifique que la contraseña corresponda al certificado.",
                    "error");
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "No se pudo validar la firma: " + ex.Message, "error");
            }
        }

        private void btnSeleccionarLogo_Click(object sender, EventArgs e)
        {
            SeleccionarYCopiarArchivo(
                "Logo de la empresa",
                "Imágenes (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                "LOGOFACTURA",
                txtImagen);
        }

        private void SeleccionarYCopiarArchivo(
            string titulo,
            string filtro,
            string carpetaDestino,
            TextBox campoDestino)
        {
            using (var selector = new OpenFileDialog
            {
                Title = titulo,
                Filter = filtro,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            })
            {
                if (selector.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    string nombreArchivo = Path.GetFileName(selector.FileName);
                    string carpeta = Path.Combine(_services.Paths.General, carpetaDestino);
                    string destino = Path.Combine(carpeta, nombreArchivo);

                    Directory.CreateDirectory(carpeta);

                    if (!string.Equals(
                        Path.GetFullPath(selector.FileName),
                        Path.GetFullPath(destino),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        File.Copy(selector.FileName, destino, true);
                    }

                    // La base de datos guarda solo el nombre: los procesos de facturación
                    // ya resuelven la ruta desde GENERAL\\FIRMAELECTRONICA o LOGOFACTURA.
                    campoDestino.Text = nombreArchivo;
                    Notificaciones.Show(this, titulo + " cargado correctamente.", "exito");
                }
                catch (IOException ex)
                {
                    Notificaciones.Show(this, "No se pudo copiar el archivo: " + ex.Message, "error");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Notificaciones.Show(this, "No tiene permisos para guardar el archivo: " + ex.Message, "error");
                }
            }
        }

        private void gvEmpresa_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (gvEmpresa.DataSource != null && gvEmpresa.Rows.Count > 0 && gvEmpresa.SelectedRows.Count > 0)
                {
                    var row = gvEmpresa.SelectedRows[0];

                    txtNombre.Text = row.Cells["NOMBRE"].Value?.ToString();
                    txtDireccion.Text = row.Cells["DIRECCION"].Value?.ToString();
                    txtClaveIngreso.Text = row.Cells["CLAVEINGRESO"].Value?.ToString();
                    txtClaveTotales.Text = row.Cells["CLAVETOTALES"].Value?.ToString();
                    txtClaveEliminar.Text = row.Cells["CLAVEELIMINACION"].Value?.ToString();
                    txtClaveConsulta.Text = row.Cells["CLAVECONSULTA"].Value?.ToString();
                    txtClaveTablas.Text = row.Cells["CLAVETABLAS"].Value?.ToString();
                    cmbImpresion.Text = row.Cells["IMPRESION"].Value?.ToString();

                    txtTelefono.Text = row.Cells["TELEFONO"].Value?.ToString();
                    txtPropietario.Text = row.Cells["PROPIETARIO"].Value?.ToString();
                    txtEmail.Text = row.Cells["EMAIL"].Value?.ToString();
                    txtRuc.Text = row.Cells["RUC"].Value?.ToString();
                    txtUbicacion.Text = row.Cells["UBICACIONARCHIVOP12"].Value?.ToString();
                    txtContrasena.Text = row.Cells["CONTRASENA"].Value?.ToString();
                    txtImagen.Text = row.Cells["IMAGEN"].Value?.ToString();
                    cmbEstadoRuc.Text = row.Cells["ESTADORUC"].Value?.ToString();
                    ActualizarVisibilidadVerificarFirma();


                    txtNombre.Enabled = false;
                    btnModificar.Visible = true;
                    btnGuardar.Visible = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error" + ex.Message.ToString(), "Mensaje");
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!PermitirCrearEmpresa)
            {
                Notificaciones.Show(this, "No está permitido crear una nueva empresa desde este formulario.", "advertencia");
                return;
            }

            if (!Validar()) return;
            try
            {
                DataSet dsDatos = _services.Empresa.ConsultaNombre(txtNombre.Text.Trim());
                if (dsDatos.Tables[0].Rows.Count > 0)
                {
                    Notificaciones.Show(this, "Ya existe una empresa con el nombre: " + txtNombre.Text, "advertencia");
                    return;
                }

                int fila = _services.Empresa.Insertar(
                    txtNombre.Text,
                    txtDireccion.Text,
                    UsuarioActual,
                    txtClaveIngreso.Text,
                    txtClaveTotales.Text,
                    txtClaveEliminar.Text,
                    txtClaveConsulta.Text,
                    txtClaveTablas.Text,
                    "SI",
                    cmbImpresion.Text.Trim(),
                    txtTelefono.Text,
                    txtPropietario.Text,
                    txtEmail.Text,
                    txtRuc.Text,
                    txtUbicacion.Text,
                    txtContrasena.Text,
                    txtImagen.Text,
                    cmbEstadoRuc.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (fila == 1)
                {
                    MostrarEmpresa(txtNombre.Text.Trim());
                    Limpiar();
                    Notificaciones.Show(this, "Empresa registrada correctamente.", "exito");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        public bool Validar()
        {
            var errores = new List<string>();

            FormValidador.Requerido(txtNombre.Text,       "Nombre",                     errores);
            FormValidador.Requerido(txtDireccion.Text,    "Dirección",                  errores);
            FormValidador.Requerido(txtClaveIngreso.Text, "Clave de Ingreso",           errores);
            FormValidador.Requerido(txtClaveTotales.Text, "Clave de Totales",           errores);
            FormValidador.Requerido(txtClaveEliminar.Text,"Clave de Eliminación",       errores);
            FormValidador.Requerido(txtClaveConsulta.Text,"Clave de Consulta",          errores);
            FormValidador.Requerido(txtClaveTablas.Text,  "Clave de Tablas",            errores);
            FormValidador.Requerido(txtTelefono.Text,     "Teléfono",                   errores);
            FormValidador.Requerido(txtPropietario.Text,  "Propietario",                errores);
            FormValidador.Requerido(txtEmail.Text,        "Email",                      errores);
            FormValidador.Requerido(txtRuc.Text,          "RUC",                        errores);
            FormValidador.Requerido(txtUbicacion.Text,    "Archivo P12",               errores);
            FormValidador.Requerido(txtContrasena.Text,   "Contraseña del certificado", errores);
            FormValidador.Requerido(cmbImpresion.Text,    "Impresión",                  errores);
            FormValidador.Requerido(cmbEstadoRuc.Text,    "Estado RUC",                 errores);

            // Validaciones de formato (solo si el campo no está vacío)
            string ruc = (txtRuc.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(ruc) && (ruc.Length != 13 || !ruc.All(char.IsDigit)))
                errores.Add("- El RUC debe tener exactamente 13 dígitos numéricos");

            string ubicacion = (txtUbicacion.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(ubicacion) && !ubicacion.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
                errores.Add("- El certificado debe ser un archivo .p12");

            if (!string.IsNullOrWhiteSpace(ubicacion) && Path.GetFileName(ubicacion) != ubicacion)
                errores.Add("- El certificado debe guardarse solo con su nombre de archivo");

            string imagen = (txtImagen.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(imagen) && Path.GetFileName(imagen) != imagen)
                errores.Add("- El logo debe guardarse solo con su nombre de archivo");

            return !FormValidador.MostrarErrores(this, errores);
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (!Validar()) return;
            try
            {
                int fila = _services.Empresa.Actualizar(
                    txtNombre.Text,
                    txtDireccion.Text,
                    UsuarioActual,
                    txtClaveIngreso.Text,
                    txtClaveTotales.Text,
                    txtClaveEliminar.Text,
                    txtClaveConsulta.Text,
                    txtClaveTablas.Text,
                    "SI",
                    cmbImpresion.Text.Trim(),
                    txtTelefono.Text,
                    txtPropietario.Text,
                    txtEmail.Text,
                    txtRuc.Text,
                    txtUbicacion.Text,
                    txtContrasena.Text,
                    txtImagen.Text,
                    cmbEstadoRuc.Text.Trim(),
                    UsuarioActual,
                    IPActual
                );

                if (fila == 1)
                {
                    MostrarEmpresa(txtNombre.Text.Trim());
                    Limpiar();
                    Notificaciones.Show(this, "Empresa actualizada correctamente.", "exito");
                }
            }
            catch (Exception ex)
            {
                Notificaciones.Show(this, "Error: " + ex.Message, "error");
            }
        }

        private void CargarCombos()
        {
            // Impresión: SI/NO
            cmbImpresion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbImpresion.Items.Clear();
            cmbImpresion.Items.Add("SI");
            cmbImpresion.Items.Add("NO");

            // Estado RUC: ajusta a tus valores reales
            cmbEstadoRuc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstadoRuc.Items.Clear();
            cmbEstadoRuc.Items.Add("ACTIVO");
            cmbEstadoRuc.Items.Add("INACTIVO");
        }


    }
}
