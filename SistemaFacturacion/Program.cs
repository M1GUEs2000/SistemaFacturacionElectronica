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

            Application.Run(new frmLogin(services));
        }
    }
}