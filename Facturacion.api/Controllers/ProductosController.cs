using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/v1/productos")]
    public class ProductosController : BaseController
    {
        private IServicioProductos _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioProductos(AppServices);
        }

        // ======================================================
        // GET ACTIVOS
        // ======================================================

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> MostrarTodos()
        {
            return Ok(await _servicio.MostrarActivosAsync());
        }

        // ======================================================
        // GET ORDEN >= 9001
        // ======================================================

        [HttpGet]
        [Route("orden")]
        public async Task<IHttpActionResult> MostrarOrden()
        {
            return Ok(await _servicio.MostrarOrdenAsync());
        }

        // ======================================================
        // GET ESTADOS
        // ======================================================

        [HttpGet]
        [Route("estados")]
        public async Task<IHttpActionResult> MostrarEstados()
        {
            return Ok(await _servicio.MostrarEstadosAsync());
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
        // POST
        // ======================================================

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudProducto solicitud)
        {
            return Ok(await _servicio.InsertarAsync(solicitud));
        }

        // ======================================================
        // PUT
        // ======================================================

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudProducto solicitud)
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
