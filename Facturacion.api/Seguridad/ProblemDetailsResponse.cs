namespace Facturacion.api.Seguridad
{
    /// <summary>
    /// Respuesta segura y uniforme para errores no controlados de la API.
    /// No debe contener detalles internos, consultas SQL ni trazas de pila.
    /// </summary>
    public sealed class ProblemDetailsResponse
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
    }
}
