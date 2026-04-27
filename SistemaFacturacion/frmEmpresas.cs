using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmEmpresas : Form
    {
        private readonly AppServices _services;

        public frmEmpresas(
            AppServices services
        )
        {
            _services = services;
            InitializeComponent();

        }
        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }


        private void frmEmpresas_Load(object sender, EventArgs e)
        {
            CargarCombos();

            MostrarEmpresa("");
            btnNuevo.Visible = false;
            btnEliminar.Visible = false;
            btnModificar.Visible = false;
            btnGuardar.Visible = false;
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

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            Limpiar();
        }
        public void Limpiar()
        {
            btnEliminar.Visible = false;
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
        }


        private void btnBuscar_Click(object sender, EventArgs e)
        {
            MostrarEmpresa(txtNombreBuscar.Text);
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


                    txtNombre.Enabled = false;
                    btnEliminar.Visible = true;
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
            if (!btnNuevo.Visible)
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
            FormValidador.Requerido(txtUbicacion.Text,    "Ubicación del archivo P12",  errores);
            FormValidador.Requerido(txtContrasena.Text,   "Contraseña del certificado", errores);
            FormValidador.Requerido(cmbImpresion.Text,    "Impresión",                  errores);
            FormValidador.Requerido(cmbEstadoRuc.Text,    "Estado RUC",                 errores);

            // Validaciones de formato (solo si el campo no está vacío)
            string ruc = (txtRuc.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(ruc) && (ruc.Length != 13 || !ruc.All(char.IsDigit)))
                errores.Add("- El RUC debe tener exactamente 13 dígitos numéricos");

            string ubicacion = (txtUbicacion.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(ubicacion) && !ubicacion.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
                errores.Add("- La ubicación del certificado debe apuntar a un archivo .p12");

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

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (gvEmpresa.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Seleccione una empresa primero.", "Mensaje");
                    return;
                }

                if (DialogResult.Yes == MessageBox.Show(
                    "¿Desea cambiar el estado del RUC a INACTIVO?",
                    "Mensaje",
                    MessageBoxButtons.YesNo))
                {
                    // Estado actual desde el combo o desde el grid
                    string estadoActual = cmbEstadoRuc.Text.Trim();

                    // Solo cambiar si está activo
                    if (estadoActual != "ACTIVO")
                    {
                        MessageBox.Show("La empresa ya está INACTIVA.", "Mensaje");
                        return;
                    }

                    // ✅ Update lógico: ACTIVO -> INACTIVO
                    int fila = _services.Empresa.ActualizarEstadoRuc(
                        txtNombre.Text.Trim(),
                        "INACTIVO",
                        UsuarioActual,
                        IPActual
                    );

                    if (fila == 1)
                    {
                        MostrarEmpresa("");
                        Limpiar();

                        MessageBox.Show(
                            "Empresa marcada como INACTIVA correctamente.",
                            "Mensaje"
                        );
                    }
                    else
                    {
                        MessageBox.Show("No se pudo actualizar el estado.", "Mensaje");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Mensaje");
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
