
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;
//using GenerarXml.Dto;

//namespace GenerarXml
//{
//    public class CLLamada
//    {
//        string Patch = @"C:\\Facturas";
//        public ViewXmlDto GenerarXml(GenerarXmlDTO generarXmlDTO)
//        {
//            string folderProcesado = Path.Combine(Patch, "facturaelectronicaprocesados");

//            if (!Directory.Exists(folderProcesado))
//            {
//                Directory.CreateDirectory(folderProcesado);
//            }

//            string folderFirmados = Path.Combine(Patch, "documentosfirmados");
//            if (!Directory.Exists(folderFirmados))
//            {
//                Directory.CreateDirectory(folderFirmados);
//            }

//            string folderRechazados = Path.Combine(Patch, "documentosrechazados");
//            if (!Directory.Exists(folderRechazados))
//            {
//                Directory.CreateDirectory(folderRechazados);
//            }

//            string folderAutorizados = Path.Combine(Patch, "documentosautorizados");
//            if (!Directory.Exists(folderAutorizados))
//            {
//                Directory.CreateDirectory(folderAutorizados);
//            }

//            string folderNoAutorizados = Path.Combine(Patch, "documentosnoautorizados");
//            if (!Directory.Exists(folderNoAutorizados))
//            {
//                Directory.CreateDirectory(folderNoAutorizados);
//            }
//            ////Empresa
//            var empresa = context.TB_INSTITUCION.FirstOrDefault(e => e.Ruc == generarXmlDTO.RucEmpresa);
//            if (empresa == null)
//            {
//                var error = new ViewXmlDto
//                {
//                    Error = "No existe Empresa"
//                };
//                return error;
//            }

//            ///Traer Xml
//            var FacturaXml = cFuncionesComprobantes.GetXmlFacturaAsync(generarXmlDTO.Id, false, generarXmlDTO.ComprobantevTipo, generarXmlDTO.RucEmpresa);
//            if (FacturaXml == null)
//            {
//                var error = new ViewXmlDto
//                {
//                    Error = "No existe xml"
//                };
//                return error;


//            }

//            var factura = FacturaXml.InnerXml;
//            factura = factura.Replace("&lt;", "<");
//            factura = factura.Replace("&gt;", ">");

//            var xmlByte = System.Text.Encoding.UTF8.GetBytes(factura);

//            var GetXml = context.TB_FACTURA.
//             FirstOrDefault(c => c.CodFactura == generarXmlDTO.Id );
//            AlmacenadorArchivosLocal almacenadorArchivos = new AlmacenadorArchivosLocal();
//            var ruta = almacenadorArchivos.GuardarArchivo(xmlByte, ".xml", folderProcesado, GetXml.Docsri);

//            var rutasxml = context.RutasXmls.FirstOrDefault(r => r.ClaveAcceso == GetXml.Docsri);
//            if (rutasxml == null)
//            {
//                var rutaXml = new RutasXmls
//                {
               
//                    RutaFirmado = null,
//                    RutaGenerado = ruta.Result
//                };
//                context.RutasXmls.Add(rutaXml);
//                context.SaveChangesAsync();

//                return new ViewXmlDto { Id = rutaXml.Id, RutaXml = rutaXml.RutaGenerado };
//            }
//            rutasxml.RutaGenerado = ruta.Result;
//            context.SaveChangesAsync();
//            return new ViewXmlDto { Id = rutasxml.Id, RutaXml = rutasxml.RutaGenerado };



//        }
//        public ViewXmlDto FirmaXml(FirmaXmlDto firmaXmlDto)
//        {

//            if (firmaXmlDto == null)
//            {
//                var error = new ViewXmlDto
//                {
//                    Error = "No existe xml"
//                };
//                return error;

//            }
//            try
//            {
//                var empresa = context.TB_INSTITUCION.FirstOrDefault(e => e.Ruc == firmaXmlDto.RucEmpresa);
//                if (empresa == null)
//                {
//                    var error = new ViewXmlDto
//                    {
//                        Error = "No existe empresa"
//                    };
//                    return error;

//                }
//                var comprobanteFirmar = context.RutasXmls.FirstOrDefault(r => r.Id == firmaXmlDto.Id);
//                if (comprobanteFirmar == null)
//                    if (empresa == null)
//                    {
//                        var error = new ViewXmlDto
//                        {
//                            Error = "No existe comprobante a firmar"
//                        };
//                        return error;

//                    }

         

  

//                var firma = new PassStoreKS.Signer();
//                firma.Sign(comprobanteFirmar.RutaGenerado, Patch + @"\\documentosfirmados\\" + comprobanteFirmar.ClaveAcceso + ".xml"
//                    , empresa.Ubicacionarchivop12, empresa.Contrasena);


//                comprobanteFirmar.RutaFirmado = Patch + @"\\documentosfirmados\\" + comprobanteFirmar.ClaveAcceso + ".xml";
//                context.SaveChanges();

//                return new ViewXmlDto { Id = comprobanteFirmar.Id, RutaXml = comprobanteFirmar.RutaFirmado };
//            }
//            catch (Exception ex)
//            {
//                var error = new ViewXmlDto
//                {
//                    Error = ex.Message
//                };
//                return error;

//            }


//        }

//        public RespuestaRecepcionSri RecepcionPrueba(RecepcionDto recepcionDto)
//        {
//            var ruta = context.RutasXmls.FirstOrDefault(c => c.ClaveAcceso == recepcionDto.ClaveAcceso);

//            if (ruta == null)
//            {
//                var error1 = new RespuestaRecepcionSri
//                {
//                    Error = "No existe datos Comprobante"
//                };
//                return error1;

//            }


//            var recepcion = cFuncionesComprobantes.RecepcionComprobantePrueba(ruta.RutaFirmado);
//            if (recepcion.Estado.Equals("RECIBIDA"))
//                return new RespuestaRecepcionSri { RespuestaRecepcion = recepcion.Estado };
//            recepcion.Comprobantes[0].Mensajes[0].mensaje.ToString();
//            var error = new RespuestaRecepcionSri
//            {
//                Error = recepcion.Estado + " Mensaje: " + recepcion.Comprobantes[0].Mensajes[0].mensaje.ToString()
//            };
//            return error;


//        }

//        public RespuestaAutorizacionSri AutorizacionPrueba(AutorizacionDto autorizacionDto)
//        {
//            var error = new RespuestaAutorizacionSri();
//            var comprobante = context.TB_FACTURA.FirstOrDefault(x => x.Docsri == autorizacionDto.ClaveAcceso);

//            if (comprobante == null)
//            {
//                error.Error = "No existe datos Comprobante";
//                return error;
//            }

//            var ruta = context.RutasXmls.FirstOrDefault(c => c.ClaveAcceso == comprobante.Docsri);
//            if (ruta == null)
//            {
//                error.Error = "No existe datos Comprobantefirmado";
//                return error;


//            }

//            var autorizacion = cFuncionesComprobantes.AutorizacionComprobantePrueba(comprobante.Docsri);
//            if (autorizacion.Comprobantes[0].Estado.Equals("AUTORIZADO"))
//            {
//                cFuncionesComprobantes.xmlAutorizado(ruta.RutaFirmado,
//                Patch + @"\\documentosautorizados\\" + comprobante.Docsri + ".xml", autorizacion.Comprobantes[0].Estado,
//                autorizacion.ClaveAcceso, autorizacion.Comprobantes[0].FechaAutorizacion);

//                return new RespuestaAutorizacionSri { RespuestaAutorizacion = autorizacion.Comprobantes[0].Estado };
//            }
//            error.Error = autorizacion.Comprobantes[0].Estado;
//            return error;
//        }

//        public RespuestaRecepcionSri Recepcion(RecepcionDto recepcionDto)
//        {
//            var error = new RespuestaRecepcionSri();
//            var ruta = context.RutasXmls.FirstOrDefault(c => c.ClaveAcceso == recepcionDto.ClaveAcceso);

//            if (ruta == null)
//            {
//                error.Error = "No existe datos Comprobante";
//                return error;
//            }

//            var recepcion = cFuncionesComprobantes.RecepcionComprobante(ruta.RutaFirmado);
//            if (recepcion.Estado.Equals("RECIBIDA"))
//                return new RespuestaRecepcionSri { RespuestaRecepcion = recepcion.Estado };
//            error.Error = recepcion.Estado;
//            return error;

//        }

//        public RespuestaAutorizacionSri Autorizacion(AutorizacionDto autorizacionDto)
//        {
//            var comprobante = context.TB_FACTURA.FirstOrDefault(x => x.Docsri == autorizacionDto.ClaveAcceso);
//            var error = new RespuestaAutorizacionSri();

//            if (comprobante == null)
//            {
//                error.Error = "No existe datos Comprobante";
//                return error;
//            }

//            var ruta = context.RutasXmls.FirstOrDefault(c => c.ClaveAcceso == comprobante.Docsri);
//            if (ruta == null)
//            {
//                error.Error = "No existe datos Comprobantefirmado";
//                return error;

//            }

//            var autorizacion = cFuncionesComprobantes.AutorizacionComprobante(comprobante.Docsri);
//            if (autorizacion.Comprobantes[0].Estado.Equals("AUTORIZADO"))
//            {
//                cFuncionesComprobantes.xmlAutorizado(ruta.RutaFirmado,
//                  Patch + @"\\documentosautorizados\\" + comprobante.Docsri + ".xml", autorizacion.Estado,
//                autorizacion.ClaveAcceso, autorizacion.Comprobantes[0].FechaAutorizacion);
//                ruta.RutaAutorizado = Patch + @"\\documentosautorizados\\" + comprobante.Docsri + ".xml";
//                context.SaveChangesAsync();
//                return new RespuestaAutorizacionSri { RespuestaAutorizacion = autorizacion.Comprobantes[0].Estado };
//            }
//            error.Error = autorizacion.Comprobantes[0].Estado;
//            return error;
//        }
//        public ViewPathPDFDTo GeneracionRide(PdfDTO pdfDTO)

//        {
//            var error = new ViewPathPDFDTo();

//            string folderPdf = Path.Combine(Patch, "pdf");

//            if (!Directory.Exists(folderPdf))
//            {
//                Directory.CreateDirectory(folderPdf);
//            }
//            var rutas = context.RutasXmls.FirstOrDefault(c => c.ClaveAcceso == pdfDTO.ClaveAcceso);

//            if (rutas == null)
//            {
//                error.Error = "No existe xml";
//                return error;

//            }


//            var xml = XDocument.Load(rutas.RutaFirmado);


//            var empresa = context.TB_INSTITUCION.FirstOrDefault(e => e.Ruc == pdfDTO.RucEmpresa);
//            if (rutas == null)
//            {
//                error.Error = "No existe empresa";
//                return error;

//            }



//            byte[] imagenStream = File.ReadAllBytes(empresa.Imagen);
//            Stream StreamImagen = new MemoryStream(imagenStream);

//            CGenerarRide cGenerarRide = new CGenerarRide();

//            var pdf = cGenerarRide.GeneracionRideFactura(folderPdf, xml, StreamImagen);

//            if (!string.IsNullOrEmpty(pdf))
//            {
//                rutas.RutaPdf = pdf;
//                 context.SaveChangesAsync();
//                return new ViewPathPDFDTo { RutaPDF = pdf };
//            }
//            error.Error = "No se genero PDF";
//            return error;

//        }



//    }
//}
