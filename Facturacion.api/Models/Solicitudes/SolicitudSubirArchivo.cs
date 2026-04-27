namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudSubirArchivo
    {
        public string Empresa { get; set; }
        public string Tipo { get; set; }        // "logo" | "p12"
        public string NombreArchivo { get; set; }
        public string ContenidoBase64 { get; set; }
    }
}
