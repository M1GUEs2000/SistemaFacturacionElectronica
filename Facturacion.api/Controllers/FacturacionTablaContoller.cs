using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Servicios;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Controllers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Factories;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/facturacion-tabla")]
    public class FacturacionTablaController : BaseController
    {
        private IServicioFacturacionTabla _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioFacturacionTabla(AppServices);
        }

        [HttpPost]
        [Route("insertar")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudFacturacion s)
            => HandleResponse(await _servicio.InsertarAsync(s));

        [HttpDelete]
        [Route("eliminar")]
        public async Task<IHttpActionResult> Eliminar([FromBody] SolicitudFacturacion s)
            => HandleResponse(await _servicio.EliminarAsync(s));

        [HttpDelete]
        [Route("secuencial/{numeroFactura}")]
        public async Task<IHttpActionResult> EliminarPorSecuencial(string numeroFactura, [FromUri] string usuario = null, [FromUri] string ip = null)
            => HandleResponse(await _servicio.EliminarPorSecuencialAsync(numeroFactura, usuario, ip));

        [HttpGet]
        [Route("numero/{numeroFactura}")]
        public async Task<IHttpActionResult> PorNumero(string numeroFactura)
            => HandleResponse(await _servicio.ConsultarPorNumeroAsync(numeroFactura));

        [HttpGet]
        [Route("fechas")]
        public async Task<IHttpActionResult> Fechas([FromUri] string desde = null, [FromUri] string hasta = null)
            => HandleResponse(await _servicio.ConsultarFechasAsync(desde, hasta));

        [HttpGet]
        [Route("fechas-cliente")]
        public async Task<IHttpActionResult> FechasCliente([FromUri] string desde = null, [FromUri] string hasta = null, [FromUri] string cliente = null)
            => HandleResponse(await _servicio.ConsultarFechasPorClienteAsync(desde, hasta, cliente));

        [HttpGet]
        [Route("pendientes")]
        public async Task<IHttpActionResult> Pendientes([FromUri] string desde = null, [FromUri] string hasta = null)
            => HandleResponse(await _servicio.ConsultarPendientesAsync(desde, hasta));

        [HttpGet]
        [Route("consumidor-final")]
        public async Task<IHttpActionResult> ConsumidorFinal([FromUri] string desde = null, [FromUri] string hasta = null)
            => HandleResponse(await _servicio.ConsultarConsumidorFinalPorFechaAsync(desde, hasta));

        [HttpGet]
        [Route("clientes")]
        public async Task<IHttpActionResult> Clientes()
            => HandleResponse(await _servicio.ConsultarClientesAsync());

        [HttpGet]
        [Route("productos")]
        public async Task<IHttpActionResult> Productos()
            => HandleResponse(await _servicio.ConsultarProductosAsync());

        [HttpGet]
        [Route("formas-pago")]
        public async Task<IHttpActionResult> FormasPago()
            => HandleResponse(await _servicio.ConsultarFormasPagoAsync());

        [HttpGet]
        [Route("secuencial-pendiente")]
        public async Task<IHttpActionResult> SecPendiente()
            => HandleResponse(await _servicio.ObtenerSecuencialPendienteAsync());

        [HttpGet]
        [Route("secuencial-consumidor")]
        public async Task<IHttpActionResult> SecConsumidor()
            => HandleResponse(await _servicio.ObtenerSecuencialConsumidorAsync());

        //ConsultaGeneral

        [HttpPost]
        [Route("consulta-general")]
        public async Task<IHttpActionResult> ConsultarGeneral([FromBody] SolicitudConsultaGeneral s)
        {
            var r = await _servicio.ConsultarGeneralAsync(s);
            return Ok(r);
        }
    }
}
