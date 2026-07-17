using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Controllers;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/v1/proveedores")]
    public class ProveedoresController : BaseController
    {
        private IServicioProveedores _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioProveedores(AppServices);
        }

        // ======================================================
        // GET: api/proveedores
        // ======================================================
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Listar()
        {
            var r = await _servicio.ListarAsync();
            return Ok(r);
        }

        // ======================================================
        // GET: api/proveedores/activos
        // ======================================================
        [HttpGet]
        [Route("activos")]
        public async Task<IHttpActionResult> ListarActivos()
        {
            var r = await _servicio.ListarActivosAsync();
            return Ok(r);
        }

        // ======================================================
        // GET: api/proveedores/{id}
        // ======================================================
        [HttpGet]
        [Route("{id}")]
        public async Task<IHttpActionResult> ConsultarPorId(int id)
        {
            var r = await _servicio.ConsultarPorIdAsync(id);
            return Ok(r);
        }

        // ======================================================
        // GET: api/proveedores/identificacion/{identificacion}
        // ======================================================
        [HttpGet]
        [Route("identificacion/{identificacion}")]
        public async Task<IHttpActionResult> ConsultarPorIdentificacion(string identificacion)
        {
            var r = await _servicio.ConsultarPorIdentificacionAsync(identificacion);
            return Ok(r);
        }

        // ======================================================
        // POST: api/proveedores
        // ======================================================
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudProveedor solicitud)
        {
            var r = await _servicio.InsertarAsync(solicitud);
            return Ok(r);
        }

        // ======================================================
        // PUT: api/proveedores
        // ======================================================
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudProveedor solicitud)
        {
            var r = await _servicio.ActualizarAsync(solicitud);
            return Ok(r);
        }

        // ======================================================
        // DELETE: api/proveedores/{id}
        // ======================================================
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Eliminar(int id, [FromUri] string usuario, [FromUri] string ip)
        {
            var r = await _servicio.EliminarAsync(id, usuario, ip);
            return Ok(r);
        }
    }
}
