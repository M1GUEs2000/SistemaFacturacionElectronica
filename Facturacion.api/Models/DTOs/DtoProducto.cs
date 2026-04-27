namespace Facturacion.api.DTOs
{
    public class DtoProducto
    {
        public string Nombre { get; set; }
        public string Valor { get; set; }
        public string Estatus { get; set; }
        public string Orden { get; set; }
        public string Codigo { get; set; }
        public string Iva { get; set; }

        // ICE
        public string AplicaIce { get; set; }
        public string IceCodigo { get; set; }
        public string IceTipo { get; set; }
        public string IcePorcentaje { get; set; }
        public string IceValor { get; set; }
        public string IceUnidad { get; set; }
        public string AzucarGramos { get; set; }
        public string Volumen { get; set; }
        public string Pvp { get; set; }
        public string TipoVehiculo { get; set; }
    }
}
