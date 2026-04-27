using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenerarXml.Dto
{
    public class Modulo11DTO
    {
        public String Fecha { get; set; }
        public string TipoComprobante { get; set; }
        public string RucEmpresa { get; set; }
        public string Ambiente { get; set; }
        public string PtoEmision { get; set; }
        public string Sucursal { get; set; }
        public string Secuencial { get; set; }
        public string Digito8 { get; set; }

    }


    public class ViewMod11Dto
    {
        public string ClaveAcceso { get; set; }
    }


}
