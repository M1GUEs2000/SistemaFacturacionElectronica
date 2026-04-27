namespace Facturacion.api.DTOs
{
    public class DtoAccionPendienteDocumento
    {
        public bool Existe { get; set; }

        public string Tipo { get; set; }
        public string NumeroDocumento { get; set; }

        public string EstadoPendiente { get; set; }

        public string TextoBoton { get; set; }
        public string Accion { get; set; }

        public bool MostrarPdf { get; set; }
        public bool MostrarXml { get; set; }
    }
}