using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Controllers;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/formapago")]
    public class FormaPagoController : BaseController
    {
        private IServicioFormaPago _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioFormaPago(AppServices);
        }

        // ==========================================================
        // GET LISTADO
        // ==========================================================

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Mostrar([FromUri] string filtro = "")
        {
            var resultado = await _servicio.MostrarAsync(filtro);
            return Ok(resultado);
        }

        // ==========================================================
        // GET UNA
        // ==========================================================

        [HttpGet]
        [Route("{formas}")]
        public async Task<IHttpActionResult> Consultar(string formas)
        {
            var resultado = await _servicio.ConsultarAsync(formas);
            return Ok(resultado);
        }

        // ==========================================================
        // GET TOTALES
        // ==========================================================

        [HttpGet]
        [Route("totales/{fecha}")]
        public async Task<IHttpActionResult> ConsultarTotales(string fecha)
        {
            var resultado = await _servicio.ConsultarTotalesAsync(fecha);
            return Ok(resultado);
        }

        // ==========================================================
        // POST
        // ==========================================================

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudFormaPago solicitud)
        {
            var resultado = await _servicio.InsertarAsync(solicitud);
            return Ok(resultado);
        }

        // ==========================================================
        // PUT
        // ==========================================================

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudFormaPago solicitud)
        {
            var resultado = await _servicio.ActualizarAsync(solicitud);
            return Ok(resultado);
        }

        // ==========================================================
        // DELETE
        // ==========================================================

        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> Eliminar(
            [FromUri] string formas,
            [FromUri] string usuario,
            [FromUri] string ip)
        {
            var resultado = await _servicio.EliminarAsync(formas, usuario, ip);
            return Ok(resultado);
        }
    }
}
