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

        // Solo se rellena en el login de admin (verificar): JWT con rol=admin.
        public string Token { get; set; }
    }
}
