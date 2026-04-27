namespace Facturacion.api.DTOs
{
    public class DtoConsultaGeneral
    {
        public string TipoDocumento { get; set; }

        public string Fecha { get; set; }
        public string FechaEmision { get; set; }
        public string FechaFactura { get; set; }

        public string NumeroFactura { get; set; }
        public string NumeroRetencion { get; set; }
        public string NumeroNota { get; set; }

        public string Cliente { get; set; }
        public string Cedula { get; set; }

        public string SujetoRetenido { get; set; }
        public string Identificacion { get; set; }

        public string Producto { get; set; }
        public string FormaPago { get; set; }
        public string Cantidad { get; set; }

        public string Total { get; set; }
        public string TotalBaseImponible { get; set; }
        public string TotalRetencionRenta { get; set; }
        public string TotalRetencionIVA { get; set; }
        public string TotalRetenido { get; set; }

        public string CreditoUsado { get; set; }
        public string Motivo { get; set; }

        public string Hora { get; set; }

        //Documentos Pendientes
        public string Estado { get; set; }
        public string Accion { get; set; }
        public string ClaveAcceso { get; set; }
        public string RutaXmlFirmado { get; set; }
        public string FechaRegistro { get; set; }
        public string Intentos { get; set; }
    }
}