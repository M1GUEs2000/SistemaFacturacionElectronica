using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/v1/secuenciales")]
    public class SecuencialesController : BaseController
    {
        private IServicioSecuenciales _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioSecuenciales(AppServices);
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Mostrar()
        {
            var resultado = await _servicio.MostrarAsync();
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("{tipo}")]
        public async Task<IHttpActionResult> Consultar(string tipo)
        {
            var resultado = await _servicio.ConsultarPorTipoAsync(tipo);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("{tipo}/real")]
        public async Task<IHttpActionResult> ObtenerReal(string tipo)
        {
            var resultado = await _servicio.ObtenerSecuencialRealAsync(tipo);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("{tipo}/formateado")]
        public async Task<IHttpActionResult> ObtenerFormateado(string tipo)
        {
            var resultado = await _servicio.ObtenerSecuencialFormateadoAsync(tipo);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("{tipo}/codigo")]
        public async Task<IHttpActionResult> ObtenerCodigo(string tipo)
        {
            var resultado = await _servicio.ObtenerCodigoNumericoAsync(tipo);
            return HandleResponse(resultado);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudSecuencial solicitud)
        {
            var resultado = await _servicio.InsertarAsync(solicitud);
            return HandleResponse(resultado);
        }

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudSecuencial solicitud)
        {
            var resultado = await _servicio.ActualizarAsync(solicitud);
            return HandleResponse(resultado);
        }

        [HttpPut]
        [Route("{tipo}/incrementar")]
        public async Task<IHttpActionResult> Incrementar(string tipo)
        {
            var resultado = await _servicio.IncrementarAsync(tipo);
            return HandleResponse(resultado);
        }

        [HttpDelete]
        [Route("{tipo}")]
        public async Task<IHttpActionResult> Eliminar(string tipo, [FromUri] string usuario, [FromUri] string ip)
        {
            var resultado = await _servicio.EliminarAsync(tipo, usuario, ip);
            return HandleResponse(resultado);
        }
    }
}