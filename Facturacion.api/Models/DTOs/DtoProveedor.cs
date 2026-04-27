using System;

namespace Facturacion.api.DTOs
{
    public class DtoProveedor
    {
        public int IdProveedor { get; set; }

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
        public DateTime? FechaRegistro { get; set; }
    }
}