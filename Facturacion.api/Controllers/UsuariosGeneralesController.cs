using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/usuariosgenerales")]
    public class UsuariosGeneralesController : ApiController
    {
        private readonly IServicioUsuariosGenerales _servicio;

        public UsuariosGeneralesController()
        {
            _servicio = new ServicioUsuariosGenerales();
        }

        // POST api/usuariosgenerales/verificar
        [HttpPost]
        [Route("verificar")]
        public async Task<IHttpActionResult> Verificar([FromBody] SolicitudLoginAdmin solicitud)
        {
            var resultado = await _servicio.VerificarAdminAsync(solicitud);

            if (resultado.exito)
                return Ok(resultado);

            return Content(HttpStatusCode.BadRequest, resultado);
        }

        // GET api/usuariosgenerales
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos()
        {
            var resultado = await _servicio.ObtenerTodosAsync();

            if (resultado.exito)
                return Ok(resultado);

            return Content(HttpStatusCode.BadRequest, resultado);
        }

        // GET api/usuariosgenerales/5
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> ObtenerPorId(int id)
        {
            var resultado = await _servicio.ObtenerPorIdAsync(id);

            if (resultado.exito)
                return Ok(resultado);

            return Content(HttpStatusCode.BadRequest, resultado);
        }

        // POST api/usuariosgenerales
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear([FromBody] SolicitudUsuarioGeneral solicitud)
        {
            var resultado = await _servicio.CrearAsync(solicitud);

            if (resultado.exito)
                return Ok(resultado);

            return Content(HttpStatusCode.BadRequest, resultado);
        }

        // PUT api/usuariosgenerales
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudUsuarioGeneral solicitud)
        {
            var resultado = await _servicio.ActualizarAsync(solicitud);

            if (resultado.exito)
                return Ok(resultado);

            return Content(HttpStatusCode.BadRequest, resultado);
        }

        // DELETE api/usuariosgenerales/5
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Eliminar(int id)
        {
            var resultado = await _servicio.EliminarAsync(id);

            if (resultado.exito)
                return Ok(resultado);

            return Content(HttpStatusCode.BadRequest, resultado);
        }
    }
}
