namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudParametrosFactura
    {
        public string Nombre { get; set; }
        public string Ambiente { get; set; }
        public string TipoEmision { get; set; }
        public string AgenteRetencion { get; set; }
        public string ContribuyenteRimpe { get; set; }
        public string CodDoc { get; set; }
        public string Estab { get; set; }
        public string PuntoEmision { get; set; }
        public string NumeroDigitos { get; set; }
        public string ContribuyenteEspecial { get; set; }
        public string ObligadoContabilidad { get; set; }
        public string TipoIdentComprador { get; set; }
        public string Moneda { get; set; }
        public string CodigoImpuesto { get; set; }
        public string CodigoPorcentaje { get; set; }
        public string FechaActualizacion { get; set; }

        public string SmtpServer { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }

        public string Usuario { get; set; }
        public string Ip { get; set; }
    }
}