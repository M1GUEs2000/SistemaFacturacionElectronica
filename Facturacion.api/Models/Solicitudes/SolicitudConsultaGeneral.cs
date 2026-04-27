namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudConsultaGeneral
    {
        public string TipoDocumento { get; set; } = "";
        public string FechaDesde { get; set; } = "";
        public string FechaHasta { get; set; } = "";

        public string Cliente { get; set; }
        public string Producto { get; set; }
        public string FormaPago { get; set; }

        public string NumeroFactura { get; set; }
        public string NumeroRetencion { get; set; }
        public string NumeroNota { get; set; }

        public string SujetoRetenido { get; set; }
        public string Estado { get; set; }
    }
}