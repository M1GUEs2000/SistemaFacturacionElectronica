using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Controllers;
using Facturacion.api.Servicios;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Factories;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/parametrosfacturas")]
    public class ParametrosFacturasController : BaseController
    {
        private IServicioParametrosFacturas _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioParametrosFacturas(AppServices);
        }

        // ======================================================
        // GET TODOS
        // ======================================================

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Mostrar()
        {
            return Ok(await _servicio.MostrarAsync());
        }

        // ======================================================
        // GET POR NOMBRE
        // ======================================================

        [HttpGet]
        [Route("{nombre}")]
        public async Task<IHttpActionResult> Consultar(string nombre)
        {
            return Ok(await _servicio.ConsultarPorNombreAsync(nombre));
        }

        // ======================================================
        // VALIDACIÓN AMBIENTE
        // ======================================================

        [HttpGet]
        [Route("es-produccion/{nombre}")]
        public async Task<IHttpActionResult> EsProduccion(string nombre)
        {
            return Ok(await _servicio.EsProduccionAsync(nombre));
        }

        [HttpPut]
        [Route("cambiar-produccion/{nombre}")]
        public async Task<IHttpActionResult> CambiarAProduccion(string nombre)
        {
            return Ok(await _servicio.CambiarAProduccionAsync(nombre));
        }

        // ======================================================
        // POST
        // ======================================================

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudParametrosFactura solicitud)
        {
            return Ok(await _servicio.InsertarAsync(solicitud));
        }

        // ======================================================
        // PUT
        // ======================================================

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudParametrosFactura solicitud)
        {
            return Ok(await _servicio.ActualizarAsync(solicitud));
        }

        // ======================================================
        // DELETE
        // ======================================================

        [HttpDelete]
        [Route("{nombre}")]
        public async Task<IHttpActionResult> Eliminar(
            string nombre,
            [FromUri] string usuario = null,
            [FromUri] string ip = null)
        {
            return Ok(await _servicio.EliminarAsync(nombre, usuario, ip));
        }
    }
}
