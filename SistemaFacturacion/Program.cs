using AccesoDatos;
using AccesoDatos.Abstractions;
using LogicaNegocios;
using LogicaNegocios.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace SistemaFacturacion
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string base_dir = AppDomain.CurrentDomain.BaseDirectory;
            string facturacion_dir = Path.Combine(base_dir, "FACTURACION");

            var paths = new FacturacionPaths
            {
                General = Path.Combine(facturacion_dir, "GENERAL"),
                Facturas = Path.Combine(facturacion_dir, "FACTURAS"),
                NotasCredito = Path.Combine(facturacion_dir, "NOTASCREDITO"),
                Retenciones = Path.Combine(facturacion_dir, "RETENCIONES")
            };

            Directory.CreateDirectory(paths.General);
            Directory.CreateDirectory(paths.Facturas);
            Directory.CreateDirectory(paths.NotasCredito);
            Directory.CreateDirectory(paths.Retenciones);

            IConexionBD conexion = new ConexionBD();

            var services = new AppServices(paths, conexion);

            // Todo error que se le muestre al cajero queda en la tabla LOG.
            Notificaciones.Inicializar(services.Log);

            // Red de seguridad: lo que se escape de un try/catch. Sin esto una
            // excepción no controlada solo mostraba el diálogo genérico de .NET
            // (o tumbaba la app) sin dejar rastro en ningún lado.
            EngancharExcepcionesNoControladas(services);

            Application.Run(new frmLogin(services));
        }

        private static void EngancharExcepcionesNoControladas(AppServices services)
        {
            // Excepciones en eventos de UI (clics, timers): la app puede seguir viva.
            Application.ThreadException += (s, e) =>
            {
                RegistrarFatal(services, "EXCEPCION UI", e.Exception);

                MessageBox.Show(
                    "Ocurrió un error inesperado:\n\n" + e.Exception.Message +
                    "\n\nEl detalle quedó registrado en el log del sistema.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // Excepciones fuera del hilo de UI (Task.Run, hilos del pinpad/SRI).
            // Acá el CLR ya decidió terminar el proceso: solo se alcanza a registrar.
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                RegistrarFatal(services, "EXCEPCION FATAL", e.ExceptionObject as Exception);
        }

        private static void RegistrarFatal(AppServices services, string proceso, Exception ex)
        {
            string detalle = ex?.ToString() ?? "Excepción sin detalle.";

            // A archivo PRIMERO: si lo que falló es la conexión a la BD, el INSERT en
            // LOG también va a fallar y este sería el único registro que sobrevive.
            try
            {
                string archivo = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "logs", "fatal.log");
                Directory.CreateDirectory(Path.GetDirectoryName(archivo));
                File.AppendAllText(
                    archivo,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + proceso + " | " +
                    detalle + Environment.NewLine);
            }
            catch { /* logger de último recurso: no hay dónde registrar su propia falla */ }

            try
            {
                services.Log.CrearLog(proceso, "SISTEMA", "", detalle);
            }
            catch { /* ídem: la BD puede ser justamente lo que falló */ }
        }
    }
}