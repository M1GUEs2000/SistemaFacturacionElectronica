namespace Facturacion.api.Models.DTOs
{
    public class DtoEmpresa
    {
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string UsuarioLogin { get; set; }

        public string ClaveIngreso { get; set; }
        public string ClaveTotales { get; set; }
        public string ClaveEliminar { get; set; }
        public string ClaveConsulta { get; set; }
        public string ClaveTabla { get; set; }

        public string Facturacion { get; set; }
        public string Impresion { get; set; }

        public string Telefono { get; set; }
        public string Propietario { get; set; }
        public string Email { get; set; }

        public string UbicacionArchivoP12 { get; set; }
        public string Contrasena { get; set; }

        public string Imagen { get; set; }
        public string EstadoRuc { get; set; }
        public string Ruc { get; set; }
    }
}