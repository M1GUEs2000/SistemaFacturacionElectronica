namespace Facturacion.api.Models.Respuestas
{
    public class RespuestaNota
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public string NumeroNota { get; set; }
        public string NumeroFactura { get; set; }

        public string ClaveAcceso { get; set; }
        public string RutaXmlGenerado { get; set; }
        public string RutaXmlFirmado { get; set; }
        public string RutaXmlAutorizado { get; set; }
        public string RutaPdf { get; set; }

        public bool EnvioCorreoExitoso { get; set; }
        public bool SecuencialRepetido { get; set; }
        public bool Autorizado { get; set; }
    }
}