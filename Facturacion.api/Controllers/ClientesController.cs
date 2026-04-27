using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/clientes")]
    public class ClientesController : BaseController
    {
        private IServicioCliente _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioCliente(AppServices);
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ObtenerTodos()
        {
            var resultado = await _servicio.ObtenerTodosAsync();
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("{cedula}")]
        public async Task<IHttpActionResult> ObtenerPorCedula(string cedula)
        {
            var resultado = await _servicio.ObtenerPorCedulaAsync(cedula);
            return HandleResponse(resultado);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Crear([FromBody] SolicitudCliente solicitud)
        {
            var resultado = await _servicio.CrearAsync(solicitud);
            return HandleResponse(resultado);
        }

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudCliente solicitud)
        {
            var resultado = await _servicio.ActualizarAsync(solicitud);
            return HandleResponse(resultado);
        }

        [HttpDelete]
        [Route("{cedula}")]
        public async Task<IHttpActionResult> Eliminar(
            string cedula,
            [FromUri] string nombre = null,
            [FromUri] string usuario = null,
            [FromUri] string ip = null)
        {
            var resultado = await _servicio.EliminarAsync(
                cedula,
                nombre,
                usuario,
                ip
            );

            return HandleResponse(resultado);
        }
    }
}