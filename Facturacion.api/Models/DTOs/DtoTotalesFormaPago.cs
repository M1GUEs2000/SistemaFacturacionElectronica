namespace Facturacion.api.DTOs
{
    public class DtoTotalesFormaPago
    {
        public string Fecha { get; set; }
        public string FormaPago { get; set; }
        public decimal Cantidades { get; set; }
        public decimal Totales { get; set; }
    }
}