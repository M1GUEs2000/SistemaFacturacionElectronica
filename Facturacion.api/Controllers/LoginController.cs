using System.Threading.Tasks;
using System.Web.Http;
using Facturacion.api.Auth;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/v1/login")]
    public class LoginController : BaseController
    {
        private IServicioLogin _servicio;

        protected override bool RequiereAutenticacion() => false;

        protected override void InicializarServicios()
        {
            _servicio = ServiciosFactory.CreateServicioLogin(AppServices);
        }

        [HttpGet]
        [Route("{clave}")]
        public async Task<IHttpActionResult> Login(string clave)
        {
            var resultado = await _servicio.LoginAsync(clave);
            if (resultado?.exito == true && resultado.data != null)
                resultado.data.Token = JwtHelper.Generar(EmpresaActual);
            return HandleResponse(resultado);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> LoginConUsuario([FromBody] SolicitudLogin solicitud)
        {
            var resultado = await _servicio.LoginConUsuarioAsync(solicitud);
            if (resultado?.exito == true && resultado.data != null)
                resultado.data.Token = JwtHelper.Generar(EmpresaActual);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("eliminar/{clave}")]
        public async Task<IHttpActionResult> ValidarClaveEliminar(string clave)
        {
            var resultado = await _servicio.ValidarClaveEliminarAsync(clave);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("totales/{clave}")]
        public async Task<IHttpActionResult> ValidarClaveTotales(string clave)
        {
            var resultado = await _servicio.ValidarClaveTotalesAsync(clave);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("consulta/{clave}")]
        public async Task<IHttpActionResult> ValidarClaveConsulta(string clave)
        {
            var resultado = await _servicio.ValidarClaveConsultaAsync(clave);
            return HandleResponse(resultado);
        }

        [HttpGet]
        [Route("tablas/{clave}")]
        public async Task<IHttpActionResult> ValidarClaveTablas(string clave)
        {
            var resultado = await _servicio.ValidarClaveTablasAsync(clave);
            return HandleResponse(resultado);
        }
    }
}
