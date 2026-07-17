using AccesoDatosWeb.Configuracion;
using Facturacion.api.App_Start;
using Facturacion.api.Mappers;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios.Services;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/v1/loginempresasgeneral")]
    public class LoginEmpresasGeneralController : ApiController
    {
        private readonly IServicioEmpresasGeneral servicio_empresas_general;
        private readonly BuilderAppServices builder_app_services;

        public LoginEmpresasGeneralController()
        {
            servicio_empresas_general = new ServicioEmpresasGeneral();
            builder_app_services = new BuilderAppServices();
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult login(DtoVerificarEmpresa dto_verificar_empresa)
        {
            try
            {
                if (dto_verificar_empresa == null)
                    return Ok(RespuestaGeneral<DtoLogin>.Fail("Solicitud inválida."));

                if (string.IsNullOrWhiteSpace(dto_verificar_empresa.Empresa))
                    return Ok(RespuestaGeneral<DtoLogin>.Fail("Debe ingresar la empresa."));

                var dto_empresas_general =
                    servicio_empresas_general.obtenerPorNombreOCodigo(dto_verificar_empresa.Empresa);

                if (dto_empresas_general == null)
                    return Ok(RespuestaGeneral<DtoLogin>.Fail("La empresa no existe o está inactiva."));

                DatabaseSettings tenant_db =
                    MapperEmpresasGeneral.toDatabaseSettings(dto_empresas_general);

                AppServices app_services =
                    builder_app_services.construir(tenant_db);

                if (app_services == null)
                    return Ok(RespuestaGeneral<DtoLogin>.Fail("No se pudo construir la configuración de la empresa."));

                return Ok(new
                {
                    exito = true,
                    mensaje = "Empresa verificada correctamente.",
                    empresa = dto_empresas_general.CodigoEmpresa
                });
            }
            catch (Exception ex)
            {
                return Ok(RespuestaGeneral<DtoLogin>.Fail(
                    "Error verificando empresa: " + ex.Message
                ));
            }
        }
    }
}