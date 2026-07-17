using System;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http;
using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;

namespace Facturacion.api.Controllers
{
    // NO extiende BaseController — no necesita X-Empresa ni BD, solo lógica pura ICE
    [RoutePrefix("api/v1/calculos")]
    public class CalculosController : ApiController
    {
        private readonly IServicioCalculos _servicio = new ServicioCalculos();

        // ======================================================
        // GET api/calculos/ice?nombre=gaseosa
        // ======================================================

        [HttpGet]
        [Route("ice")]
        public async Task<IHttpActionResult> DetectarICE([FromUri] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Content(HttpStatusCode.BadRequest,
                    new { exito = false, mensaje = "El parámetro 'nombre' es requerido." });

            var resultado = await _servicio.DetectarICEAsync(nombre);

            if (!resultado.exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        // ======================================================
        // GET api/calculos/ice-vehiculo?pvp=45000&tipoVehiculo=CONVENCIONAL
        // ======================================================

        [HttpGet]
        [Route("ice-vehiculo")]
        public async Task<IHttpActionResult> ICEVehiculo(
            [FromUri] double pvp,
            [FromUri] string tipoVehiculo = "CONVENCIONAL")
        {
            var resultado = await _servicio.ObtenerICEVehiculoPorPrecioAsync(pvp, tipoVehiculo);

            if (!resultado.exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }
    }
}
