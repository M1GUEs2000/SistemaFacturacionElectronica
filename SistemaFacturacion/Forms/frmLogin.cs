using LogicaNegocios.Services;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace SistemaFacturacion
{
    public partial class frmLogin : Form
    {
        private readonly AppServices _services;

        public frmLogin(AppServices services
         )
        {

            InitializeComponent();
            _services = services;

            CargarBackgroundEmpresa();

        }

        public string UsuarioActual { get; set; }
        public string IPActual { get; set; }

        private void bntAceptar_Click(object sender, EventArgs e)
        {
            if (txtClave.Text != "")
            {
                DataSet dsDatos = _services.Login.ConsultarUsuario(txtClave.Text);

                if (dsDatos.Tables[0].Rows.Count > 0)
                {
                    string Nombre = dsDatos.Tables[0].Rows[0]["NOMBRE"].ToString();
                    string IP = _services.Login.ObtenerIP();
                    _services.Log.CrearLog(
                                             "Ingreso al sistema",
                                             Nombre,
                                             IP,
                                             "El usuario ingresó al sistema"
                                         );

                    // Para que los errores que se loguean solos (toast rojo) sepan quién
                    // los provocó: la mayoría de las llamadas a Show() no pasan usuario.
                    Notificaciones.EstablecerUsuario(Nombre, IP);

                    frmPrincipal frmPrincipal = new frmPrincipal(Nombre, IP, _services);
                    frmPrincipal.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Clave Incorrecta", "Error");
                }
            }
            else
            {
                MessageBox.Show("Ingrese una Clave en el Login.");
            }
        }

        private void txtClave_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataSet dsDatos = new DataSet();
                if (txtClave.Text != "")
                {
                    dsDatos = _services.Login.ConsultarUsuario(txtClave.Text);
                    if (dsDatos.Tables[0].Rows.Count > 0)
                    {
                        string Nombre = dsDatos.Tables[0].Rows[0]["NOMBRE"].ToString();
                        string IP = _services.Login.ObtenerIP();   // ✔ obtenemos la IP aquí
                        _services.Log.CrearLog(
                            "Ingreso al sistema",
                            Nombre,
                            IP,
                            "El usuario ingresó al sistema"
                        );

                        // Ídem bntAceptar_Click: atribución de los errores auto-logueados.
                        Notificaciones.EstablecerUsuario(Nombre, IP);


                        // string Fecha = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                        ////string HostName = Environment.UserName;
                        //LogicaNegocios._services.Log.Insertar("Ingreso Sistema", Usuario.ToUpper(), HostName, "Ingreso al sistema", Fecha);

                        frmPrincipal frmPrincipal = new frmPrincipal(Nombre, IP, _services);
                        frmPrincipal.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Clave Incorrecta", "Mensaje");
                    }
                }
                else
                {
                    MessageBox.Show("Ingrese una Clave", "Mensaje");
                }

            }
        }

        private void CargarBackgroundEmpresa()
        {
            try
            {
                // ==========================================
                // CONSULTAR EMPRESA
                // ==========================================
                DataSet dsEmpresa = _services.Empresa.MostrarEmpresa("");

                if (dsEmpresa == null ||
                    dsEmpresa.Tables.Count == 0 ||
                    dsEmpresa.Tables[0].Rows.Count == 0)
                    return;

                DataRow rowEm = dsEmpresa.Tables[0].Rows[0];

                string nombreImagen = rowEm["IMAGEN"]?.ToString();

                if (string.IsNullOrWhiteSpace(nombreImagen))
                    return;

                // ==========================================
                // RUTA LOGO (CONFIGURACIÓN CENTRAL)
                // ==========================================
                string rutaLogo = Path.Combine(
                    _services.Paths.General,
                    "LOGOFACTURA",
                    nombreImagen
                );

                if (!File.Exists(rutaLogo))
                    return;

                // ==========================================
                // CARGAR IMAGEN
                // ==========================================
                using (var stream = new MemoryStream(File.ReadAllBytes(rutaLogo)))
                {
                    this.BackgroundImage = Image.FromStream(stream);
                }

                this.BackgroundImageLayout = ImageLayout.Stretch;
                // Puedes cambiar a Zoom / Center si deseas
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar imagen de fondo: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

    }
}
