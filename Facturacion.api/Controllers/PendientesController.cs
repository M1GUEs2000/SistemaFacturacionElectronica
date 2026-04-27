using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Controllers;
using Facturacion.api.Servicios;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Factories;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/pendientes")]
    public class PendientesController : BaseController
    {
        private IServicioPendientes _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioPendientes(AppServices);
        }

        // ==========================================================
        // GET
        // ==========================================================

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> MostrarTodos()
        {
            var resultado = await _servicio.MostrarTodosAsync();
            return Ok(resultado);
        }

        [HttpGet]
        [Route("{numeroDocumento}")]
        public async Task<IHttpActionResult> Consultar(string numeroDocumento)
        {
            var resultado = await _servicio.ConsultarAsync(numeroDocumento);
            return Ok(resultado);
        }

        [HttpGet]
        [Route("{numeroDocumento}/{tipo}")]
        public async Task<IHttpActionResult> ConsultarPorNumeroYTipo(string numeroDocumento, string tipo)
        {
            var resultado = await _servicio.ConsultarPorNumeroYTipoAsync(numeroDocumento, tipo);
            return Ok(resultado);
        }

        // ==========================================================
        // POST
        // ==========================================================

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudPendiente solicitud)
        {
            var resultado = await _servicio.InsertarAsync(solicitud);
            return Ok(resultado);
        }

        // ==========================================================
        // PUT
        // ==========================================================

        [HttpPut]
        [Route("estado")]
        public async Task<IHttpActionResult> ActualizarEstado(
            [FromUri] string numeroDocumento,
            [FromUri] string estado,
            [FromUri] string tipo,
            [FromUri] string usuario = null,
            [FromUri] string ip = null)
        {
            var resultado = await _servicio
                .ActualizarEstadoAsync(numeroDocumento, estado, tipo, usuario, ip);

            return Ok(resultado);
        }

        // ==========================================================
        // DELETE
        // ==========================================================

        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Eliminar(
            [FromUri] string numeroDocumento,
            [FromUri] string tipo,
            [FromUri] string usuario = null,
            [FromUri] string ip = null)
        {
            var resultado = await _servicio
                .EliminarAsync(numeroDocumento, tipo, usuario, ip);

            return Ok(resultado);
        }

        // ==========================================================
        // ESTADOS GENERADOS
        // ==========================================================

        [HttpGet]
        [Route("estado/pendiente/{tipo}")]
        public async Task<IHttpActionResult> ObtenerEstadoPendiente(string tipo)
        {
            var resultado = await _servicio.ObtenerEstadoPendienteAsync(tipo);
            return Ok(resultado);
        }

        [HttpGet]
        [Route("estado/autorizacion/{tipo}")]
        public async Task<IHttpActionResult> ObtenerEstadoAutorizacion(string tipo)
        {
            var resultado = await _servicio.ObtenerEstadoPendienteAutorizacionAsync(tipo);
            return Ok(resultado);
        }

        [HttpGet]
        [Route("estado/correo/{tipo}")]
        public async Task<IHttpActionResult> ObtenerEstadoCorreo(string tipo)
        {
            var resultado = await _servicio.ObtenerEstadoPendienteCorreoAsync(tipo);
            return Ok(resultado);
        }

        // ==========================================================
        // ACCION BOTON UI
        // ==========================================================

        [HttpGet]
        [Route("accion/{numeroDocumento}/{tipo}")]
        public async Task<IHttpActionResult> ConsultarAccion(string numeroDocumento, string tipo)
        {
            var resultado = await _servicio
                .ConsultarAccionDocumentoAsync(numeroDocumento, tipo);

            return Ok(resultado);
        }
    }
}
