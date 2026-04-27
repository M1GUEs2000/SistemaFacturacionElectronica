namespace Facturacion.api.Models.Respuestas
{
    public class RespuestaFactura
    {
        public bool Exito { get; set; }
        public bool Autorizado { get; set; }
        public bool SecuencialRepetido { get; set; }
        public bool EsElectronica { get; set; }

        public bool EnvioCorreoExitoso { get; set; }

        public string NumeroDocumento { get; set; }
        public string ClaveAcceso { get; set; }
        public string RutaPdf { get; set; }
        public string RutaXmlAutorizado { get; set; }
        public string Mensaje { get; set; }
    }
}