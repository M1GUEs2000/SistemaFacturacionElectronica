using System.Collections.Generic;

namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudRetenciones
    {
        public string NumeroRetencion { get; set; }
        public string ClaveAcceso { get; set; }
        public string FechaEmision { get; set; }
        public string HoraEmision { get; set; }
        public string Ambiente { get; set; }
        public string Estado { get; set; }
        public string Codigo { get; set; }
        public string TipoEmision { get; set; }

        public string NumeroFactura { get; set; }
        public string ClaveAccesoFactura { get; set; }
        public string FechaFactura { get; set; }

        public string SujetoRetenido { get; set; }
        public string IdentificacionSujeto { get; set; }
        public string TipoIdentificacionSujeto { get; set; }
        public string DireccionSujeto { get; set; }
        public string RegimenSujeto { get; set; }

        public string TotalBaseImponible { get; set; }
        public string TotalRetencionRenta { get; set; }
        public string TotalRetencionIva { get; set; }
        public string TotalRetenido { get; set; }

        public string Observaciones { get; set; }

        public string Usuario { get; set; }
        public string IP { get; set; }

        public List<SolicitudRetencionesDetalle> Detalles { get; set; } = new List<SolicitudRetencionesDetalle>();
    }
}