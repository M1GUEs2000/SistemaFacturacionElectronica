using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/empresas")]
    public class EmpresasController : BaseController
    {
        private IServicioEmpresa _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioEmpresa(AppServices);
        }
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodas([FromUri] string nombre = null)
        {
            return HandleResponse(await _servicio.ObtenerTodasAsync(nombre));
        }

        [HttpGet]
        [Route("{nombre}")]
        public async Task<IHttpActionResult> ObtenerPorNombre(string nombre)
        {
            return HandleResponse(await _servicio.ObtenerPorNombreAsync(nombre));
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear([FromBody] SolicitudEmpresa solicitud)
        {
            return HandleResponse(await _servicio.CrearAsync(solicitud));
        }

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudEmpresa solicitud)
        {
            return HandleResponse(await _servicio.ActualizarAsync(solicitud));
        }

        [HttpDelete]
        [Route("{nombre}")]
        public async Task<IHttpActionResult> Eliminar(
            string nombre,
            [FromUri] string usuario = null,
            [FromUri] string ip = null)
        {
            return HandleResponse(await _servicio.EliminarAsync(nombre, usuario, ip));
        }

        [HttpPatch]
        [Route("estado-ruc")]
        public async Task<IHttpActionResult> ActualizarEstadoRuc(
            [FromUri] string nombreEmpresa,
            [FromUri] string nuevoEstado,
            [FromUri] string usuario = null,
            [FromUri] string ip = null)
        {
            return HandleResponse(await _servicio
                .ActualizarEstadoRucAsync(nombreEmpresa, nuevoEstado, usuario, ip));
        }

        // =========================================
        // CAMBIAR CREDENCIALES
        // PUT /api/empresas/credenciales
        // =========================================
        [HttpPut]
        [Route("credenciales")]
        public async Task<IHttpActionResult> CambiarCredenciales(
            [FromBody] SolicitudCambiarCredenciales solicitud)
        {
            return HandleResponse(await _servicio
                .CambiarCredencialesAsync(solicitud?.NuevoUsuario, solicitud?.NuevaClave));
        }
    }
}
