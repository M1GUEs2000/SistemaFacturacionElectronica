namespace LogicaNegocios.Services
{
    using System.Configuration;
    using System.IO;
    using AccesoDatos.Abstractions;
    using DF_PinPad.Wrapper.Config;
    using DF_PinPad.Wrapper.Services;
    using LogicaNegocios;
    using LogicaNegocios.Procesos;
    using static LogicaNegocios.Procesos.ProcesosFacturacion;

    public class AppServices
    {
        public FacturacionPaths Paths { get; }
        public IConexionBD Conexion { get; }
        public LogManejador Log { get; }
        public ClienteManejador Cliente { get; }
        public EmpresaManejador Empresa { get; }
        public FacturacionManejador Facturacion { get; }
        public FacturasPendientesManejador Pendientes { get; }
        public FormaPagoManejador FormaPago { get; }
        public LoginManejador Login { get; }
        public NotasCreditoManejador NotaCredito { get; }
        public ParametrosFacturasManejador ParamFactura { get; }
        public ParametrosTransaccionesManejador ParamTransaccion { get; }
        public ParametrosManejador Param { get; }
        public ProductoManejador Producto { get; }
        public ProveedoresManejador Proveedor { get; }
        public RetencionesManejador Retencion { get; }

        public ProcesosFacturacion ProcesosFacturacion { get; }
        public ProcesosNotaCredito ProcesosNotaCredito { get; }
        public ProcesosRetenciones ProcesosRetenciones { get; }
        public ProcesosGenerales ProcesosGenerales { get; }
        public ProcesosLote ProcesosLote { get; }
        public FacturacionQueueAsync FacturacionQueue { get; }

        /// <summary>Servicio de cobro con pinpad (Datafast). Auditoría en Access via PinPadLogManejador.</summary>
        public IPinPadService PinPad { get; private set; }

        public decimal TarifaIva { get; private set; } = 0m;

        public AppServices(FacturacionPaths paths, IConexionBD conexion)
        {
            Paths = paths;
            Conexion = conexion;

            InicializarCarpetas();

            Log = new LogManejador(Conexion);

            Cliente = new ClienteManejador(Conexion, Log);
            Empresa = new EmpresaManejador(Conexion, Log);
            Facturacion = new FacturacionManejador(Conexion, Log);
            Pendientes = new FacturasPendientesManejador(Conexion, Log);
            FormaPago = new FormaPagoManejador(Conexion, Log);
            Login = new LoginManejador(Conexion);
            NotaCredito = new NotasCreditoManejador(Conexion, Log);
            ParamFactura = new ParametrosFacturasManejador(Conexion, Log);
            ParamTransaccion = new ParametrosTransaccionesManejador(Conexion, Log);
            Param = new ParametrosManejador(Conexion, Log);
            Producto = new ProductoManejador(Conexion, Log);
            Proveedor = new ProveedoresManejador(Conexion, Log);
            Retencion = new RetencionesManejador(Conexion, Log);

            ProcesosFacturacion = new ProcesosFacturacion(this);
            ProcesosNotaCredito = new ProcesosNotaCredito(this);
            ProcesosRetenciones = new ProcesosRetenciones(this);
            ProcesosGenerales = new ProcesosGenerales(this);
            ProcesosLote = new ProcesosLote(this);
            FacturacionQueue = new FacturacionQueueAsync();

            PinPad = ConstruirPinPad();
        }

        // PinPadSettings se arma a mano desde appSettings (NO se usa
        // PinPadSettings.LoadFromAppConfig(): ese exige la connection string SQL
        // 'PinPadLogsDb' y lanza. Aquí la auditoría va a Access via PinPadLogManejador,
        // así que SqlConnectionString queda null a propósito).
        /// <summary>
        /// Reconstruye el servicio de pinpad releyendo appSettings. Llamar tras guardar
        /// la configuración de conexión (frmDatafast) para aplicarla sin reiniciar la app.
        /// </summary>
        public void RecargarPinPad()
        {
            ConfigurationManager.RefreshSection("appSettings");
            PinPad = ConstruirPinPad();
        }

        private IPinPadService ConstruirPinPad()
        {
            var settings = new PinPadSettings
            {
                IP = ConfigurationManager.AppSettings["PinPad.IP"],
                Puerto = ParseInt(ConfigurationManager.AppSettings["PinPad.Puerto"], 0),
                TimeOutMs = ParseInt(ConfigurationManager.AppSettings["PinPad.TimeOutMs"], 60000),
                MID = ConfigurationManager.AppSettings["PinPad.MID"],
                TID = ConfigurationManager.AppSettings["PinPad.TID"],
                CajaID = ConfigurationManager.AppSettings["PinPad.CajaID"],
                Version = ParseInt(ConfigurationManager.AppSettings["PinPad.Version"], 1), // 1 = V4_4
                SHA = ParseInt(ConfigurationManager.AppSettings["PinPad.SHA"], 1)          // 1 = SHA1
                // SqlConnectionString: null a propósito (auditoría en Access)
            };

            return new PinPadService(settings, new PinPadLogManejador(Conexion));
        }

        private static int ParseInt(string valor, int porDefecto)
        {
            return int.TryParse(valor, out int r) ? r : porDefecto;
        }

        public void CargarTarifaIva(string nombreEmpresa)
        {
            try
            {
                System.Data.DataSet pfm = ParamFactura.ConsultarNombre(nombreEmpresa);
                if (pfm != null && pfm.Tables.Count > 0 && pfm.Tables[0].Rows.Count > 0)
                {
                    string codigo = pfm.Tables[0].Rows[0]["CODIGOPORCENTAJE"].ToString();
                    TarifaIva = Procesos.HelperIva.TarifaDesdeCodigoPorcentaje(codigo);
                }
            }
            catch { }
        }

        private void InicializarCarpetas()
        {
            Directory.CreateDirectory(Path.Combine(Paths.General, "FIRMAELECTRONICA"));
            Directory.CreateDirectory(Path.Combine(Paths.General, "LOGOFACTURA"));
            Directory.CreateDirectory(Path.Combine(Paths.General, "ELECTRONICA"));

            Directory.CreateDirectory(Path.Combine(Paths.Facturas, "XML"));
            Directory.CreateDirectory(Path.Combine(Paths.Facturas, "XMLFIRMADOS"));
            Directory.CreateDirectory(Path.Combine(Paths.Facturas, "XMLAUTORIZADOS"));
            Directory.CreateDirectory(Path.Combine(Paths.Facturas, "PDF"));
            Directory.CreateDirectory(Path.Combine(Paths.Facturas, "PDFPREVIEW"));

            Directory.CreateDirectory(Path.Combine(Paths.NotasCredito, "XML"));
            Directory.CreateDirectory(Path.Combine(Paths.NotasCredito, "XMLFIRMADOS"));
            Directory.CreateDirectory(Path.Combine(Paths.NotasCredito, "XMLAUTORIZADOS"));
            Directory.CreateDirectory(Path.Combine(Paths.NotasCredito, "PDF"));
            Directory.CreateDirectory(Path.Combine(Paths.NotasCredito, "PDFPREVIEW"));

            Directory.CreateDirectory(Path.Combine(Paths.Retenciones, "XML"));
            Directory.CreateDirectory(Path.Combine(Paths.Retenciones, "XMLFIRMADOS"));
            Directory.CreateDirectory(Path.Combine(Paths.Retenciones, "XMLAUTORIZADOS"));
            Directory.CreateDirectory(Path.Combine(Paths.Retenciones, "PDF"));
            Directory.CreateDirectory(Path.Combine(Paths.Retenciones, "PDFPREVIEW"));
        }
    }
}   