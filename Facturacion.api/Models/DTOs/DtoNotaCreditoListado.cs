namespace Facturacion.api.DTOs
{
    public class DtoNotaCreditoListado
    {
        public string NumeroNota { get; set; }
        public string NumeroFactura { get; set; }
        public string FechaEmision { get; set; }

        public string Cliente { get; set; }
        public string Cedula { get; set; }

        public string Total { get; set; }
        public string CreditoUsado { get; set; }
        public string Motivo { get; set; }
    }
}