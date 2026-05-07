using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Controllers;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/notascredito")]
    public class NotasCreditoController : BaseController
    {
        private IServicioNotasCredito _servicio;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioNotasCredito(AppServices);
        }

        // ======================================================
        // CRUD
        // ======================================================

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Insertar([FromBody] SolicitudNotaCredito solicitud)
        {
            return Ok(await _servicio.InsertarAsync(solicitud));
        }

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Actualizar([FromBody] SolicitudNotaCredito solicitud)
        {
            return Ok(await _servicio.ActualizarAsync(solicitud));
        }

        [HttpDelete]
        [Route("{numeroNota}")]
        public async Task<IHttpActionResult> Eliminar(string numeroNota, [FromUri] string usuario = null, [FromUri] string ip = null)
        {
            return Ok(await _servicio.EliminarAsync(numeroNota, usuario, ip));
        }

        // ======================================================
        // CONSULTAS
        // ======================================================

        [HttpGet]
        [Route("{numeroNota}")]
        public async Task<IHttpActionResult> Consultar(string numeroNota)
        {
            return Ok(await _servicio.ConsultarPorNumeroAsync(numeroNota));
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Listar([FromUri] int top = 20)
        {
            return Ok(await _servicio.ListarAsync(top));
        }

        [HttpGet]
        [Route("fechas")]
        public async Task<IHttpActionResult> PorFechas([FromUri] string fd = null, [FromUri] string fh = null)
        {
            return Ok(await _servicio.ConsultarPorFechasAsync(fd, fh));
        }

        [HttpGet]
        [Route("cliente")]
        public async Task<IHttpActionResult> PorCliente([FromUri] string fd = null, [FromUri] string fh = null, [FromUri] string cedula = null)
        {
            return Ok(await _servicio.ConsultarPorClienteAsync(fd, fh, cedula));
        }

        [HttpGet]
        [Route("numero")]
        public async Task<IHttpActionResult> PorNumeroFechas([FromUri] string fd = null, [FromUri] string fh = null, [FromUri] string numeroNota = null)
        {
            return Ok(await _servicio.ConsultarPorNumeroNotaFechasAsync(fd, fh, numeroNota));
        }

        [HttpGet]
        [Route("cliente-numero")]
        public async Task<IHttpActionResult> PorClienteNumero([FromUri] string fd = null, [FromUri] string fh = null, [FromUri] string cedula = null, [FromUri] string numeroNota = null)
        {
            return Ok(await _servicio.ConsultarPorClienteYNumeroNotaAsync(fd, fh, cedula, numeroNota));
        }

        // ======================================================
        // DETALLE
        // ======================================================

        [HttpPost]
        [Route("detalle")]
        public async Task<IHttpActionResult> InsertarDetalle([FromBody] SolicitudNotaCreditoDetalle solicitud)
        {
            return Ok(await _servicio.InsertarDetalleAsync(solicitud));
        }

        [HttpGet]
        [Route("detalle/{numeroNota}")]
        public async Task<IHttpActionResult> ConsultarDetalle(string numeroNota)
        {
            return Ok(await _servicio.ConsultarDetalleAsync(numeroNota));
        }

        [HttpDelete]
        [Route("detalle/{numeroNota}")]
        public async Task<IHttpActionResult> EliminarDetalle(string numeroNota)
        {
            return Ok(await _servicio.EliminarDetalleAsync(numeroNota));
        }

        // ======================================================
        // UTILIDADES
        // ======================================================

        [HttpPut]
        [Route("renombrar")]
        public async Task<IHttpActionResult> Renombrar([FromUri] string vieja, [FromUri] string nueva)
        {
            return Ok(await _servicio.RenombrarNotaAsync(vieja, nueva));
        }

        [HttpGet]
        [Route("facturas-unicas")]
        public async Task<IHttpActionResult> FacturasUnicas()
        {
            return Ok(await _servicio.ListarFacturasUnicasAsync());
        }
    }
}
