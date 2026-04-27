namespace Facturacion.api.DTOs
{
    public class DtoResultadoICE
    {
        public bool Detectado { get; set; }
        public string Categoria { get; set; }
        public string AplicaIce { get; set; }
        public string IceCodigo { get; set; }

        // AD_VALOREM | ESPECIFICO | AZUCAR | MIXTO
        public string IceTipo { get; set; }

        // Porcentaje (ej: "75" para 75%)
        public string IcePorcentaje { get; set; }

        // Valor específico (ej: "0.16" por unidad)
        public string IceValor { get; set; }

        // UNIDAD | LITRO | 100G | ""
        public string IceUnidad { get; set; }

        // NINGUNO | CONVENCIONAL | HIBRIDO | SENAE
        public string TipoVehiculo { get; set; }

        // Mensaje legible para mostrar al usuario
        public string Mensaje { get; set; }
    }
}
