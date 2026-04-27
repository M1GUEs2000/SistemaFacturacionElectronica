namespace Facturacion.api.Models.Solicitudes
{
    public class SolicitudProveedor
    {
        public int? IdProveedor { get; set; } // null cuando es insert

        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string RazonSocial { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }

        public string TipoPersona { get; set; }

        public bool EsRimpe { get; set; }
        public string TipoRimpe { get; set; }

        public bool EsProfesional { get; set; }
        public bool EsArrendador { get; set; }

        public string Estado { get; set; }

        public string Usuario { get; set; }
        public string IP { get; set; }
    }
}