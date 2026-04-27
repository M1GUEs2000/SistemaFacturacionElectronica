using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Facturacion.api.App_Start;
using Facturacion.api.Factories;
using Facturacion.api.Models.Solicitudes;
using LogicaNegocios;

namespace Facturacion.api.Controllers
{
    [RoutePrefix("api/facturas")]
    public class FacturasController : ApiController
    {
        private IServicioFactura _servicioFactura;
        private FacturacionPaths _paths;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            string empresa = null;
            IEnumerable<string> valores;
            if (controllerContext.Request.Headers.TryGetValues("X-Empresa", out valores))
                empresa = valores.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(empresa))
            {
                var query = controllerContext.Request.RequestUri.ParseQueryString();
                empresa = query["empresa"];
            }

            var appServices = AppServicesFactory.Create(empresa);
            _servicioFactura = ServiciosFactory.CreateServicioFactura(appServices);
            _paths = FacturacionConfig.GetFacturacionPaths(empresa);
        }


        // ==========================================================
        // 1) CREAR FACTURA (PROCESO COMPLETO)
        // POST: api/facturas
        // ==========================================================
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CrearFactura([FromBody] SolicitudFactura solicitud)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _servicioFactura.CrearFacturaAsync(solicitud);

            if (resultado == null)
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Respuesta nula del servicio."
                    )
                );
            }

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        // ==========================================================
        // 2) PREVIEW FACTURA (SIN FIRMAR / SIN INSERTAR)
        // POST: api/facturas/preview
        // ==========================================================
        [HttpPost]
        [Route("preview")]
        public async Task<IHttpActionResult> PreviewFactura([FromBody] SolicitudFactura solicitud)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _servicioFactura.PreviewFacturaAsync(solicitud);

            if (resultado == null)
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Respuesta nula del servicio."
                    )
                );
            }

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        // ==========================================================
        // 3) AUTORIZAR FACTURA PENDIENTE
        // POST: api/facturas/autorizar-pendiente
        // ==========================================================
        [HttpPost]
        [Route("autorizar-pendiente")]
        public async Task<IHttpActionResult> AutorizarPendiente([FromBody] SolicitudProcesarPendienteDocumento solicitud)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _servicioFactura.AutorizarFacturaPendienteAsync(solicitud);

            if (resultado == null)
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Respuesta nula del servicio."
                    )
                );
            }

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        // ==========================================================
        // 4) PROCESAR FACTURA CONSUMIDOR FINAL
        // POST: api/facturas/procesar-consumidor-final
        // ==========================================================
        [HttpPost]
        [Route("procesar-consumidor-final")]
        public async Task<IHttpActionResult> ProcesarFacturaConsumidorFinal([FromBody] SolicitudProcesarFacturaConsumidorFinal solicitud)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _servicioFactura.ProcesarFacturaConsumidorFinalAsync(solicitud);

            if (resultado == null)
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Respuesta nula del servicio."
                    )
                );
            }

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        // ==========================================================
        // 5) REPROCESAR FACTURA PENDIENTE NORMAL
        // POST: api/facturas/reprocesar-pendiente
        // ==========================================================
        [HttpPost]
        [Route("reprocesar-pendiente")]
        public async Task<IHttpActionResult> ReprocesarPendiente([FromBody] SolicitudProcesarFacturaConsumidorFinal solicitud)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _servicioFactura.ReprocesarFacturaPendienteAsync(solicitud);

            if (resultado == null)
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Respuesta nula del servicio."
                    )
                );
            }

            if (!resultado.Exito)
                return Content(HttpStatusCode.BadRequest, resultado);

            return Ok(resultado);
        }

        // ==========================================================
        // FACTURAS
        // ==========================================================

        [HttpGet]
        [Route("pdf-preview")]
        public IHttpActionResult AbrirFacturaPdfPreview(string nombreArchivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreArchivo))
                    return BadRequest("Nombre de archivo vacío.");

                string root = Path.GetFullPath(Path.Combine(_paths.Facturas, "PDFPREVIEW"))
                                  .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                string nombreFinal = Path.GetFileName(nombreArchivo);
                if (!nombreFinal.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    nombreFinal += ".pdf";

                string fullPath = Path.GetFullPath(Path.Combine(root, nombreFinal));
                if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    return StatusCode(HttpStatusCode.Forbidden);

                if (!File.Exists(fullPath))
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "No existe el PDF preview de factura."));

                return ResponderArchivo(fullPath, "application/pdf", true);
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error al abrir PDF preview de factura: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("pdf")]
        public IHttpActionResult AbrirFacturaPdf(string fecha, string numeroDocumento)
        {
            return AbrirDocumentoBuscado(
                _paths.Facturas,
                "PDF",
                "pdf",
                "application/pdf",
                fecha,
                numeroDocumento,
                "la factura"
            );
        }

        [HttpGet]
        [Route("xml-autorizado")]
        public IHttpActionResult AbrirFacturaXmlAutorizado(string fecha, string numeroDocumento)
        {
            return AbrirDocumentoBuscado(
                _paths.Facturas,
                "XMLAUTORIZADOS",
                "xml",
                "application/xml",
                fecha,
                numeroDocumento,
                "la factura"
            );
        }

        #region NOTAS DE CREDITO

        [HttpGet]
        [Route("notas-credito/pdf-preview")]
        public IHttpActionResult AbrirNotaCreditoPdfPreview(string nombreArchivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreArchivo))
                    return BadRequest("Nombre de archivo vacío.");

                string root = Path.GetFullPath(Path.Combine(_paths.NotasCredito, "PDFPREVIEW"))
                                  .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                string nombreFinal = Path.GetFileName(nombreArchivo);
                if (!nombreFinal.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    nombreFinal += ".pdf";

                string fullPath = Path.GetFullPath(Path.Combine(root, nombreFinal));
                if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    return StatusCode(HttpStatusCode.Forbidden);

                if (!File.Exists(fullPath))
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "No existe el PDF preview de nota de crédito."));

                return ResponderArchivo(fullPath, "application/pdf", true);
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error al abrir PDF preview de nota de crédito: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("notas-credito/pdf")]
        public IHttpActionResult AbrirNotaCreditoPdf(string fecha, string numeroDocumento)
        {
            return AbrirDocumentoBuscado(
                _paths.NotasCredito,
                "PDF",
                "pdf",
                "application/pdf",
                fecha,
                numeroDocumento,
                "la nota de crédito"
            );
        }

        [HttpGet]
        [Route("notas-credito/xml-autorizado")]
        public IHttpActionResult AbrirNotaCreditoXmlAutorizado(string fecha, string numeroDocumento)
        {
            return AbrirDocumentoBuscado(
                _paths.NotasCredito,
                "XMLAUTORIZADOS",
                "xml",
                "application/xml",
                fecha,
                numeroDocumento,
                "la nota de crédito"
            );
        }
        #endregion

        #region RETENCIONES

        [HttpGet]
        [Route("retenciones/pdf-preview")]
        public IHttpActionResult AbrirRetencionPdfPreview(string nombreArchivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreArchivo))
                    return BadRequest("Nombre de archivo vacío.");

                string root = Path.GetFullPath(Path.Combine(_paths.Retenciones, "PDFPREVIEW"))
                                  .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                string nombreFinal = Path.GetFileName(nombreArchivo);
                if (!nombreFinal.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    nombreFinal += ".pdf";

                string fullPath = Path.GetFullPath(Path.Combine(root, nombreFinal));
                if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    return StatusCode(HttpStatusCode.Forbidden);

                if (!File.Exists(fullPath))
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "No existe el PDF preview de retención."));

                return ResponderArchivo(fullPath, "application/pdf", true);
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error al abrir PDF preview de retención: " + ex.Message));
            }
        }

        [HttpGet]
        [Route("retenciones/pdf")]
        public IHttpActionResult AbrirRetencionPdf(string fecha, string numeroDocumento)
        {
            return AbrirDocumentoBuscado(
                _paths.Retenciones,
                "PDF",
                "pdf",
                "application/pdf",
                fecha,
                numeroDocumento,
                "la retención"
            );
        }

        [HttpGet]
        [Route("retenciones/xml-autorizado")]
        public IHttpActionResult AbrirRetencionXmlAutorizado(string fecha, string numeroDocumento)
        {
            return AbrirDocumentoBuscado(
                _paths.Retenciones,
                "XMLAUTORIZADOS",
                "xml",
                "application/xml",
                fecha,
                numeroDocumento,
                "la retención"
            );
        }

        #endregion

        // ==========================================================
        // HELPERS
        // ==========================================================

        private IHttpActionResult ResponderArchivo(string rutaArchivo, string contentType, bool inline = false)
        {
            byte[] bytes = File.ReadAllBytes(rutaArchivo);
            string nombreArchivo = Path.GetFileName(rutaArchivo);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(bytes);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(inline ? "inline" : "attachment")
            {
                FileName = nombreArchivo
            };

            return ResponseMessage(response);
        }
        private string BuscarRutaDocumento(
            string carpetaBase,
            string subcarpeta,
            string extension,
            string fechaTexto,
            string numeroDocumento
        )
        {
            if (string.IsNullOrWhiteSpace(fechaTexto))
                throw new Exception("Fecha vacía.");

            if (string.IsNullOrWhiteSpace(numeroDocumento))
                throw new Exception("Número de documento vacío.");

            DateTime fecha;
            if (!DateTime.TryParseExact(
                fechaTexto,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha))
            {
                throw new Exception("Fecha inválida para localizar el documento.");
            }

            string fechaFormato = fecha.ToString("ddMMyyyy");
            string patron = "*" + fechaFormato + "*" + numeroDocumento + "*." + extension;
            string carpeta = Path.Combine(carpetaBase, subcarpeta);

            if (!Directory.Exists(carpeta))
                throw new Exception("No existe la carpeta: " + carpeta);

            string[] archivos = Directory.GetFiles(carpeta, patron);

            if (archivos == null || archivos.Length == 0)
                return null;

            return archivos[0];
        }

        private IHttpActionResult AbrirDocumentoBuscado(
            string carpetaBase,
            string subcarpeta,
            string extension,
            string contentType,
            string fecha,
            string numeroDocumento,
            string nombreDocumentoUsuario
        )
        {
            try
            {
                string ruta = BuscarRutaDocumento(
                    carpetaBase,
                    subcarpeta,
                    extension,
                    fecha,
                    numeroDocumento
                );

                if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
                {
                    return ResponseMessage(
                        Request.CreateResponse(
                            HttpStatusCode.NotFound,
                            "No se encontró el " + extension.ToUpper() + " de " + nombreDocumentoUsuario + "."
                        )
                    );
                }

                return ResponderArchivo(ruta, contentType, contentType == "application/pdf");
            }
            catch (Exception ex)
            {
                return ResponseMessage(
                    Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        "Error al abrir " + extension.ToUpper() + " de " + nombreDocumentoUsuario + ": " + ex.Message
                    )
                );
            }
        }
    }
}