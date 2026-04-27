using System;

namespace Facturacion.api.Models.DTOs
{
    public class DtoUsuarioGeneral
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
