using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facturacion.api.Servicios.Interfaces
{
    public interface IServicioCrearBase
    {
        // ===============================
        // BASE TENANT (SCRIPT)
        // ===============================
        Task<RespuestaGeneral<string>> EjecutarScriptAsync(SolicitudCrearBase solicitud);

        // ===============================
        // BASE GENERAL (EMPRESAS)
        // ===============================
        Task<RespuestaGeneral<string>> InsertarEmpresaAsync(SolicitudEmpresaGeneral solicitud);

        Task<RespuestaGeneral<string>> ActualizarEmpresaAsync(SolicitudEmpresaGeneral solicitud);

        Task<RespuestaGeneral<string>> DesactivarEmpresaAsync(string nombreEmpresa);

        Task<RespuestaGeneral<bool>> ExisteBaseAsync(string databaseName);
        Task<RespuestaGeneral<List<DtoEmpresaGeneral>>> ListarEmpresasAsync();

        // ===============================
        // CARPETAS + ARCHIVOS POR EMPRESA
        // ===============================
        Task<RespuestaGeneral<string>> CrearCarpetasEmpresaAsync(string nombreEmpresa);
        Task<RespuestaGeneral<string>> SubirArchivoEmpresaAsync(SolicitudSubirArchivo solicitud);
        Task<RespuestaGeneral<string>> EliminarEmpresaCompletaAsync(string nombreEmpresa);
    }
}