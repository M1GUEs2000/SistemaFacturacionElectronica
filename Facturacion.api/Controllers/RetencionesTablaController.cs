using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Servicios;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Controllers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Factories;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/retencionestabla")]
    public class RetencionesTablaController : BaseController
    {
        private IServicioRetenciones _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioRetenciones(AppServices);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudRetenciones solicitud)
        {
            var r = await _servicio.InsertarAsync(solicitud);
            return Ok(r);
        }

        [HttpPut]
        [Route("{numero}")]
        public async Task<IHttpActionResult> Actualizar(string numero, [FromBody] SolicitudRetenciones solicitud)
        {
            var r = await _servicio.ActualizarAsync(
                numero,
                solicitud.Estado,
                solicitud.Codigo,
                solicitud.ClaveAcceso,
                solicitud.Ambiente,
                solicitud.TipoEmision,
                solicitud.FechaEmision,
                solicitud.HoraEmision,
                solicitud.Usuario ?? "",
                solicitud.IP ?? ""
            );

            return Ok(r);
        }

        [HttpDelete]
        [Route("{numero}")]
        public async Task<IHttpActionResult> Eliminar(string numero, [FromUri] string usuario, [FromUri] string ip)
        {
            var r = await _servicio.EliminarAsync(numero, usuario, ip);
            return Ok(r);
        }

        [HttpGet]
        [Route("{numero}")]
        public async Task<IHttpActionResult> Consultar(string numero)
        {
            var r = await _servicio.ConsultarAsync(numero);
            return Ok(r);
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Listar([FromUri] int top = 50)
        {
            var r = await _servicio.ListarAsync(top);
            return Ok(r);
        }

        [HttpGet]
        [Route("avanzado")]
        public async Task<IHttpActionResult> ConsultarAvanzado([FromUri] string fechaDesde = null, [FromUri] string fechaHasta = null, [FromUri] string sujetoRetenido = null, [FromUri] string numeroRetencion = null, [FromUri] string numeroFactura = null)
        {
            var r = await _servicio.ConsultarAvanzadoAsync(
                fechaDesde,
                fechaHasta,
                sujetoRetenido,
                numeroRetencion,
                numeroFactura
            );

            return Ok(r);
        }
    }
}
