using AccesoDatosWeb.Configuracion;
using Facturacion.api.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios.Interfaces
{
    public interface IServicioEmpresasGeneral
    {
        DtoEmpresasGeneral obtenerPorNombreOCodigo(string empresa);
        bool existeEmpresa(string empresa);
        DatabaseSettings obtenerDatabaseSettings(string empresa);
        List<string> listarNombresActivas();
    }
}
