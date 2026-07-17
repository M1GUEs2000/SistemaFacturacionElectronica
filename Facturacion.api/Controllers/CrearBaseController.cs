using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Seguridad;
using Facturacion.api.Servicios.Interfaces;

namespace Facturacion.api.Controllers
{
    // Aprovisionamiento (crear/eliminar empresas, scripts, archivos): solo operador.
    [SoloAdmin]
    [RoutePrefix("api/v1/crearbase")]
    public class CrearBaseController : BaseController
    {
        private IServicioCrearBase _servicio;

        // El admin no está atado a un tenant: como su token trae rol=admin y sin
        // empresa, BaseController deja AppServices en la BD general, que es el
        // contexto de estas operaciones. La autorización la impone [SoloAdmin].

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioCrearBase(AppServices);
        }

        // =========================================
        // EJECUTAR SCRIPT EN BASE TENANT
        // =========================================    
        [HttpPost]
        [Route("script")]
        public async Task<IHttpActionResult> EjecutarScript([FromBody] SolicitudCrearBase solicitud)
        {
            solicitud = solicitud ?? new SolicitudCrearBase();

            var resultado = await _servicio.EjecutarScriptAsync(solicitud);
            return HandleResponse(resultado);
        }

        // =========================================
        // INSERTAR EMPRESA (BASE GENERAL)
        // =========================================
        [HttpPost]
        [Route("empresa")]
        public async Task<IHttpActionResult> InsertarEmpresa([FromBody] SolicitudEmpresaGeneral solicitud)
        {
            var resultado = await _servicio.InsertarEmpresaAsync(solicitud);
            return HandleResponse(resultado);
        }

        // =========================================
        // ACTUALIZAR EMPRESA
        // =========================================
        [HttpPut]
        [Route("empresa")]
        public async Task<IHttpActionResult> ActualizarEmpresa([FromBody] SolicitudEmpresaGeneral solicitud)
        {
            var resultado = await _servicio.ActualizarEmpresaAsync(solicitud);
            return HandleResponse(resultado);
        }

        // =========================================
        // DESACTIVAR EMPRESA
        // =========================================
        [HttpDelete]
        [Route("empresa/{nombreEmpresa}")]
        public async Task<IHttpActionResult> DesactivarEmpresa(string nombreEmpresa)
        {
            var resultado = await _servicio.DesactivarEmpresaAsync(nombreEmpresa);
            return HandleResponse(resultado);
        }

        // =========================================
        // VALIDAR EXISTENCIA DE BASE
        // =========================================
        [HttpGet]
        [Route("existe-base")]
        public async Task<IHttpActionResult> ExisteBase([FromUri] string databaseName)
        {
            var resultado = await _servicio.ExisteBaseAsync(databaseName);
            return HandleResponse(resultado);
        }

        // =========================================
        // LISTAR EMPRESAS (BASE GENERAL)
        // GET /api/crearbase/empresas
        // =========================================
        [HttpGet]
        [Route("empresas")]
        public async Task<IHttpActionResult> ListarEmpresas()
        {
            var resultado = await _servicio.ListarEmpresasAsync();
            return HandleResponse(resultado);
        }

        // =========================================
        // CREAR CARPETAS POR EMPRESA
        // POST /api/crearbase/carpetas/{nombreEmpresa}
        // =========================================
        [HttpPost]
        [Route("carpetas/{nombreEmpresa}")]
        public async Task<IHttpActionResult> CrearCarpetas(string nombreEmpresa)
        {
            var resultado = await _servicio.CrearCarpetasEmpresaAsync(nombreEmpresa);
            return HandleResponse(resultado);
        }

        // =========================================
        // ELIMINAR EMPRESA COMPLETA (carpetas + BD General)
        // DELETE /api/crearbase/empresa-completa/{nombreEmpresa}
        // =========================================
        [HttpDelete]
        [Route("empresa-completa/{nombreEmpresa}")]
        public async Task<IHttpActionResult> EliminarEmpresaCompleta(string nombreEmpresa)
        {
            var resultado = await _servicio.EliminarEmpresaCompletaAsync(nombreEmpresa);
            return HandleResponse(resultado);
        }

        // =========================================
        // SUBIR ARCHIVO (LOGO O P12) POR EMPRESA
        // POST /api/crearbase/subir-archivo
        // =========================================
        [HttpPost]
        [Route("subir-archivo")]
        public async Task<IHttpActionResult> SubirArchivo([FromBody] SolicitudSubirArchivo solicitud)
        {
            var resultado = await _servicio.SubirArchivoEmpresaAsync(solicitud);
            return HandleResponse(resultado);
        }
    }
}