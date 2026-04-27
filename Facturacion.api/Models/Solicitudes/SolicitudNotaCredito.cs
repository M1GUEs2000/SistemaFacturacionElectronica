namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudNotaCredito
    {
        public string NumeroNota { get; set; }
        public string ClaveAcceso { get; set; }
        public string FechaEmision { get; set; }
        public string HoraEmision { get; set; }
        public string Ambiente { get; set; }
        public string Estado { get; set; }
        public string Codigo { get; set; }
        public string TipoEmision { get; set; }

        public string NumeroFactura { get; set; }
        public string ClaveAccesoFactura { get; set; }
        public string FechaFactura { get; set; }

        public string Motivo { get; set; }
        public string Cliente { get; set; }

        public string TotalSinImpuestos { get; set; }
        public string TotalConImpuestos { get; set; }
        public string CreditoUsado { get; set; }

        public string Usuario { get; set; }
        public string Ip { get; set; }
    }
}