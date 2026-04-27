namespace Facturacion.api.Models.DTOs
{
    public class DtoLogin
    {
        public string Token { get; set; }

        public string Empresa { get; set; }

        public string ClaveIngreso { get; set; }

        public string ClaveEliminacion { get; set; }

        public string ClaveTotales { get; set; }

        public string ClaveConsulta { get; set; }

        public string ClaveTablas { get; set; }

        public string Usuario { get; set; }

        public bool DebeCambiarCredenciales { get; set; }
    }
}