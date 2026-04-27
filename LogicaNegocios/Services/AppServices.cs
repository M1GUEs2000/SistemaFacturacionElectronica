namespace LogicaNegocios.Services
{
    using System.IO;
    using AccesoDatos.Abstractions;
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