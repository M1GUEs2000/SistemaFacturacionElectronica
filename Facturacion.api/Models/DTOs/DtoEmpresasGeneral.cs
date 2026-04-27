using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facturacion.api.Models.DTOs
{
    public class DtoVerificarEmpresa
    {
        public string Empresa { get; set; }
        public string Usuario { get; set; }
        public string ClaveIngreso { get; set; }
    }
}