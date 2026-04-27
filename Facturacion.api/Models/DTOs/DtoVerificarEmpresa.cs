using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facturacion.api.Models.DTOs
{
    public class DtoEmpresasGeneral
    {
        public string CodigoEmpresa { get; set; }
        public string NombreEmpresa { get; set; }
        public string Provider { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public bool TrustServerCertificate { get; set; }
        public bool Activa { get; set; }
    }
}