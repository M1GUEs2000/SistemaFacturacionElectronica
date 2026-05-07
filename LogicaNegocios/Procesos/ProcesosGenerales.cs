using LogicaNegocios.Services;
using GenerarXml.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Xml.Linq;

namespace LogicaNegocios.Procesos
{
    public class ProcesosGenerales
    {

        private readonly AppServices _services;


        public ProcesosGenerales(AppServices services)
        {
            _services = services;
        }

        // ==========================================================
        // DEBUG CORREO
        // ==========================================================
        private void LogCorreo(string mensaje)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string carpeta = Path.Combine(baseDir, "logs");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                string archivo = Path.Combine(carpeta, "correo_debug.log");
                File.AppendAllText(
                    archivo,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + mensaje + Environment.NewLine
                );
            }
            catch
            {
            }
        }

        // ==========================================================
        // MÉTODO ÚNICO Y REUTILIZABLE (FACTURA / NC / RETENCIÓN)
        // ==========================================================
        public ResultadoCorreo EnviarCorreoDocumento(
            SolicitudCorreoDocumento solicitud,
            DataRow rowCliente,
            string tipo,
            string usuario,
            string ip
        )
        {
            ResultadoCorreo r = new ResultadoCorreo();

            try
            {
                LogCorreo("==================================================");
                LogCorreo("INICIO EnviarCorreoDocumento");
                LogCorreo($"TIPO={tipo}");
                LogCorreo($"USUARIO={usuario}");
                LogCorreo($"IP={ip}");

                if (solicitud == null)
                    throw new Exception("La solicitud de correo es nula.");

                LogCorreo($"Solicitud.TipoDocumento={solicitud.TipoDocumento}");
                LogCorreo($"Solicitud.NumeroDocumento={solicitud.NumeroDocumento}");
                LogCorreo($"Solicitud.RutaPdf={solicitud.RutaPdf}");
                LogCorreo($"Solicitud.RutaXmlAutorizado={solicitud.RutaXmlAutorizado}");

                // ------------------------------
                // Validaciones mínimas
                // ------------------------------
                string correoCliente = null;

                LogCorreo("Validando rowCliente y campo CORREO...");

                if (rowCliente != null &&
                    rowCliente.Table != null &&
                    rowCliente.Table.Columns.Contains("CORREO"))
                {
                    correoCliente = rowCliente["CORREO"]?.ToString()?.Trim();
                }

                LogCorreo($"Correo cliente detectado: [{correoCliente}]");

                if (string.IsNullOrWhiteSpace(correoCliente))
                    throw new Exception("El cliente no tiene correo registrado.");

                if (string.IsNullOrWhiteSpace(solicitud.RutaPdf) ||
                    string.IsNullOrWhiteSpace(solicitud.RutaXmlAutorizado))
                    throw new Exception("No se encontraron los archivos PDF o XML.");

                LogCorreo("Validando existencia física de adjuntos...");

                if (!File.Exists(solicitud.RutaPdf))
                    throw new Exception("No existe el PDF: " + solicitud.RutaPdf);

                if (!File.Exists(solicitud.RutaXmlAutorizado))
                    throw new Exception("No existe el XML autorizado: " + solicitud.RutaXmlAutorizado);

                LogCorreo("PDF y XML existen físicamente.");

                // ------------------------------
                // Leer XML autorizado (CDATA)
                // ------------------------------
                LogCorreo("Cargando XML autorizado...");
                XDocument xmlAut = XDocument.Load(solicitud.RutaXmlAutorizado);
                LogCorreo("XML autorizado cargado correctamente.");

                XCData cdata = xmlAut.DescendantNodes().OfType<XCData>().FirstOrDefault();

                if (cdata == null)
                    throw new Exception("El XML autorizado no contiene CDATA.");

                LogCorreo("CDATA encontrada correctamente.");

                XDocument xmlDoc = XDocument.Parse(cdata.Value);
                LogCorreo("XML interno parseado correctamente.");

                // ------------------------------
                // Armar contenido según tipo
                // ------------------------------
                LogCorreo("Armando correo...");
                CorreoArmado correo = ArmarCorreo(
                    solicitud.TipoDocumento,
                    xmlDoc,
                    correoCliente,
                    usuario
                );

                LogCorreo($"Correo.CorreoCliente={correo.CorreoCliente}");
                LogCorreo($"Correo.CorreoEmpresa={correo.CorreoEmpresa}");
                LogCorreo($"Correo.AsuntoCliente={correo.AsuntoCliente}");
                LogCorreo("Correo armado correctamente.");

                // ------------------------------
                // Obtener datos de empresa para mostrar nombre
                // y usar reply-to
                // ------------------------------
                string nombreEmpresa = "";
                string correoEmpresaReplyTo = "";

                LogCorreo("Consultando datos de empresa para From/Reply-To...");

                DataSet dsEmpresa = _services.Empresa.MostrarEmpresa(usuario);

                if (dsEmpresa != null &&
                    dsEmpresa.Tables != null &&
                    dsEmpresa.Tables.Count > 0 &&
                    dsEmpresa.Tables[0].Rows.Count > 0)
                {
                    DataRow emp = dsEmpresa.Tables[0].Rows[0];

                    if (dsEmpresa.Tables[0].Columns.Contains("NOMBRE"))
                        nombreEmpresa = emp["NOMBRE"]?.ToString()?.Trim() ?? "";

                    if (dsEmpresa.Tables[0].Columns.Contains("EMAIL"))
                        correoEmpresaReplyTo = emp["EMAIL"]?.ToString()?.Trim() ?? "";
                }

                LogCorreo($"Nombre empresa para display: [{nombreEmpresa}]");
                LogCorreo($"Reply-To empresa: [{correoEmpresaReplyTo}]");

                // Fallback por si no hubiese nombre
                if (string.IsNullOrWhiteSpace(nombreEmpresa))
                    nombreEmpresa = "Facturación";

                // ------------------------------
                // SMTP
                // ------------------------------
                LogCorreo("Consultando parámetros SMTP...");
                DataSet dsSMTP = _services.ParamFactura.ConsultarNombre(usuario);

                if (dsSMTP == null)
                    throw new Exception("ConsultarNombre devolvió null.");

                if (dsSMTP.Tables == null || dsSMTP.Tables.Count == 0)
                    throw new Exception("ConsultarNombre no devolvió tablas.");

                if (dsSMTP.Tables[0].Rows.Count == 0)
                    throw new Exception("No se encontraron parámetros SMTP.");

                DataRow p = dsSMTP.Tables[0].Rows[0];

                string smtpServer = p["SMTPSERVER"]?.ToString()?.Trim();
                string smtpPortTexto = p["SMTPPORT"]?.ToString()?.Trim();
                string smtpUser = p["SMTPUSER"]?.ToString()?.Trim();
                string smtpPass = p["SMTPPASS"]?.ToString() ?? "";

                LogCorreo($"SMTP SERVER={smtpServer}");
                LogCorreo($"SMTP PORT={smtpPortTexto}");
                LogCorreo($"SMTP USER={smtpUser}");
                LogCorreo($"SMTP PASS VACIA={(string.IsNullOrWhiteSpace(smtpPass) ? "SI" : "NO")}");

                if (string.IsNullOrWhiteSpace(smtpServer))
                    throw new Exception("SMTPSERVER está vacío.");

                if (string.IsNullOrWhiteSpace(smtpPortTexto))
                    throw new Exception("SMTPPORT está vacío.");

                if (!int.TryParse(smtpPortTexto, out int smtpPort))
                    throw new Exception("SMTPPORT no es numérico: " + smtpPortTexto);

                if (string.IsNullOrWhiteSpace(smtpUser))
                    throw new Exception("SMTPUSER está vacío.");

                SmtpClient client = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(
                        smtpUser,
                        smtpPass
                    ),
                    EnableSsl = true
                };

                LogCorreo("SmtpClient creado correctamente.");
                LogCorreo($"CONFIG SMTP FINAL => HOST={smtpServer} | PORT={smtpPort} | SSL=true");

                // ------------------------------
                // Envío cliente
                // ------------------------------
                LogCorreo("Creando MailMessage cliente...");

                using (MailMessage msgCliente = CrearMail(
                    smtpUser,               // correo real que autentica SMTP
                    nombreEmpresa,          // nombre visible
                    correoEmpresaReplyTo,   // reply-to de la empresa
                    correo.CorreoCliente,   // destinatario final
                    correo.AsuntoCliente,
                    correo.CuerpoCliente,
                    solicitud.RutaPdf,
                    solicitud.RutaXmlAutorizado
                ))
                {
                    LogCorreo("MailMessage cliente creado correctamente.");
                    LogCorreo($"FROM REAL={smtpUser}");
                    LogCorreo($"FROM DISPLAY={nombreEmpresa}");
                    LogCorreo($"REPLY-TO={correoEmpresaReplyTo}");
                    LogCorreo($"TO={correo.CorreoCliente}");
                    LogCorreo($"SUBJECT={correo.AsuntoCliente}");
                    LogCorreo("Enviando correo cliente...");

                    client.Send(msgCliente);

                    LogCorreo("Correo cliente enviado correctamente.");
                }

                // ==================================================
                // ELIMINAR DE s.Pendientes (CORREO ENVIADO)
                // ==================================================
                try
                {
                    LogCorreo("Intentando eliminar pendiente...");
                    _services.Pendientes.Eliminar(
                        solicitud.NumeroDocumento,
                        tipo,
                        usuario,
                        ip
                    );
                    LogCorreo("Pendiente eliminado correctamente.");
                }
                catch (Exception exEliminar)
                {
                    LogCorreo("ERROR al eliminar pendiente (no bloqueante): " + exEliminar);
                }

                // ------------------------------
                // Envío empresa (opcional)
                // Fallo aquí NO afecta el resultado — el cliente ya recibió su correo.
                // ------------------------------
                if (!string.IsNullOrWhiteSpace(correo.CorreoEmpresa))
                {
                    LogCorreo("Existe correo empresa configurado.");
                    LogCorreo($"Correo empresa={correo.CorreoEmpresa}");

                    try
                    {
                        using (MailMessage msgEmpresa = CrearMail(
                            smtpUser,
                            nombreEmpresa,
                            correoEmpresaReplyTo,
                            correo.CorreoEmpresa,
                            correo.AsuntoEmpresa,
                            correo.CuerpoEmpresa,
                            solicitud.RutaPdf,
                            solicitud.RutaXmlAutorizado
                        ))
                        {
                            LogCorreo("MailMessage empresa creado correctamente.");
                            LogCorreo($"FROM REAL={smtpUser}");
                            LogCorreo($"FROM DISPLAY={nombreEmpresa}");
                            LogCorreo($"REPLY-TO={correoEmpresaReplyTo}");
                            LogCorreo("Enviando correo empresa...");
                            client.Send(msgEmpresa);
                        }
                    }
                    catch (Exception exEmpresa)
                    {
                        LogCorreo("ERROR enviando correo empresa (no bloqueante): " + exEmpresa.Message);
                    }
                }
                else
                {
                    LogCorreo("No existe correo empresa configurado.");
                }

                r.Exito = true;
                r.Mensaje = "Correo enviado correctamente.";

                LogCorreo("FIN OK EnviarCorreoDocumento");
                return r;
            }
            catch (Exception ex)
            {
                LogCorreo("ERROR EnviarCorreoDocumento");
                LogCorreo(ex.ToString());

                // ------------------------------
                // PENDIENTE DE CORREO (NO fiscal)
                // ------------------------------
                try
                {
                    LogCorreo("Intentando registrar pendiente de correo...");

                    RegistrarPendienteCorreo(
                        solicitud?.NumeroDocumento ?? "",
                        ObtenerClaveAccesoDesdeXml(solicitud?.RutaXmlAutorizado ?? ""),
                        tipo,
                        solicitud?.RutaXmlAutorizado ?? "",
                        usuario,
                        ip
                    );

                    LogCorreo("Pendiente de correo registrado/actualizado correctamente.");
                }
                catch (Exception exPendiente)
                {
                    LogCorreo("ERROR registrando pendiente de correo: " + exPendiente);
                }

                r.Exito = false;
                r.Mensaje = $"{solicitud?.TipoDocumento} AUTORIZADO. Correo pendiente de envío. ERROR: {ex.Message}";

                LogCorreo("FIN ERROR EnviarCorreoDocumento");
                return r;
            }
        }

        private static string ObtenerClaveAccesoDesdeXml(string rutaXmlAutorizado)
        {
            try
            {
                var xml = XDocument.Load(rutaXmlAutorizado);
                var cdata = xml.DescendantNodes().OfType<XCData>().FirstOrDefault();
                if (cdata == null) return "";

                var interno = XDocument.Parse(cdata.Value);
                return interno.Descendants("claveAcceso").FirstOrDefault()?.Value ?? "";
            }
            catch
            {
                return "";
            }
        }

        // ==========================================================
        // SELECTOR SIMPLE (NO SOBREINGENIERÍA)
        // ==========================================================
        private CorreoArmado ArmarCorreo(
            string tipo,
            XDocument xml,
            string correoCliente,
            string usuario
        )
        {
            DataSet dsEmpresa = _services.Empresa.MostrarEmpresa(usuario);
            DataRow emp = dsEmpresa.Tables[0].Rows[0];

            string estab = xml.Descendants("estab").FirstOrDefault()?.Value ?? "";
            string ptoEmi = xml.Descendants("ptoEmi").FirstOrDefault()?.Value ?? "";
            string sec = xml.Descendants("secuencial").FirstOrDefault()?.Value ?? "";

            string razonCliente =
                xml.Descendants("razonSocialComprador").FirstOrDefault()?.Value ??
                xml.Descendants("razonSocialSujetoRetenido").FirstOrDefault()?.Value ??
                "";

            string fecha = xml.Descendants("fechaEmision").FirstOrDefault()?.Value ?? "";
            string total =
                xml.Descendants("importeTotal").FirstOrDefault()?.Value ??
                xml.Descendants("valorModificacion").FirstOrDefault()?.Value ??
                xml.Descendants("totalRetenido").FirstOrDefault()?.Value ??
                "";

            string clave = ObtenerClaveAcceso(xml);

            string numero = $"{estab}-{ptoEmi}-{sec}";

            string nombreDocumento;
            string asuntoEmpresa;
            string etiquetaTotal = "Total";

            switch (tipo.ToUpperInvariant())
            {
                case "NOTADECREDITO":
                    nombreDocumento = "Nota de Crédito Electrónica";
                    asuntoEmpresa = "[NC GENERADA]";
                    break;

                case "FACTURA":
                    nombreDocumento = "Factura Electrónica";
                    asuntoEmpresa = "[FACTURA GENERADA]";
                    break;

                case "RETENCION":
                    nombreDocumento = "Retención Electronica";
                    asuntoEmpresa = "[RETENCIÓN GENERADA]";
                    etiquetaTotal = "Total Retenido";
                    break;

                default:
                    throw new Exception("Tipo de documento no soportado: " + tipo);
            }

            return new CorreoArmado
            {
                CorreoCliente = correoCliente,
                CorreoEmpresa = emp["EMAIL"].ToString().Trim(),

                AsuntoCliente = $"{nombreDocumento} {numero}",
                CuerpoCliente =
                    $"Estimado(a) cliente:\n\n{razonCliente}\n\n" +
                    $"Adjunto encontrará su {nombreDocumento}.\n\n" +
                    $"Número: {numero}\n" +
                    $"Fecha: {fecha}\n" +
                    $"{etiquetaTotal}: {total}\n\n" +
                    $"Clave de Acceso SRI:\n{clave}",

                AsuntoEmpresa = $"{asuntoEmpresa} {numero}",
                CuerpoEmpresa =
                    $"Empresa: {emp["NOMBRE"]}\n" +
                    $"Clave de Acceso:\n{clave}"
            };
        }

        // ==========================================================
        // HELPERS
        // ==========================================================
        private static MailMessage CrearMail(
            string from,
            string fromDisplayName,
            string replyTo,
            string to,
            string subject,
            string body,
            string rutaPdf,
            string rutaXml
        )
        {
            MailMessage msg = new MailMessage();

            // Se verá como: Nombre Empresa <facturacion@gmail.com>
            msg.From = new MailAddress(from, fromDisplayName);

            // Al responder, irá al correo de la empresa
            if (!string.IsNullOrWhiteSpace(replyTo))
                msg.ReplyToList.Add(new MailAddress(replyTo));

            msg.To.Add(to);
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = false;

            msg.Attachments.Add(new Attachment(rutaPdf));
            msg.Attachments.Add(new Attachment(rutaXml));

            return msg;
        }

        private void RegistrarPendienteCorreo(
            string numeroDocumento,
            string claveAcceso,
            string tipo,
            string rutaXml,
            string usuario,
            string ip
        )
        {
            string estadoPendienteCorreo =
                _services.Pendientes.ObtenerEstadoPendienteCorreoPorTipo(tipo);

            DataSet ds = _services.Pendientes.Consultar(numeroDocumento);

            bool existeRegistro = ds != null &&
                                  ds.Tables.Count > 0 &&
                                  ds.Tables[0].Rows.Count > 0;

            if (existeRegistro)
            {
                _services.Pendientes.ActualizarEstado(
                    numeroDocumento,
                    estadoPendienteCorreo,
                    tipo,
                    usuario,
                    ip
                );
            }
            else
            {
                _services.Pendientes.Insertar(
                    numeroDocumento,
                    claveAcceso,
                    rutaXml,
                    DateTime.Now,
                    0,
                    estadoPendienteCorreo,
                    tipo,
                    usuario,
                    ip
                );
            }
        }


        //Procesos SRI 


        private static string ObtenerClaveAcceso(XDocument xml)
        {
            return xml.Descendants("claveAcceso").FirstOrDefault()?.Value ?? "";
        }
        public ResultadoSRI EnviarRecepcion(
            string rutaXmlFirmado,
            string claveAcceso,
            string numeroDocumento,
            string tipoDocumento,
            string usuarioActual,
            string ipActual
        )
        {
            ResultadoSRI r = new ResultadoSRI();

            try
            {
                Consultas consulta = new Consultas();
                var recepcion = consulta.recepcion("", rutaXmlFirmado);

                // ======================================================
                // 1) SIN RESPUESTA DEL SRI → PENDIENTE### (GENÉRICO)
                // ======================================================
                if (recepcion == null)
                {
                    string estadoPendiente =
                        _services.Pendientes.ObtenerEstadoPendientePorTipo(tipoDocumento);

                    _services.Pendientes.Insertar(
                        numeroDocumento,
                        claveAcceso,
                        rutaXmlFirmado,
                        DateTime.Now,
                        0,
                        estadoPendiente,
                        tipoDocumento,
                        usuarioActual,
                        ipActual
                    );

                    r.Exito = false;
                    r.EstadoEspecial = estadoPendiente;
                    r.Mensaje = tipoDocumento + ": SRI sin respuesta. Registrado como " + estadoPendiente;
                    return r;
                }

                // ======================================================
                // ESTADO DIRECTO
                // ======================================================
                string estadoRecepcion =
                    recepcion.RespuestaRecepcion != null
                        ? recepcion.RespuestaRecepcion.Trim().ToUpperInvariant()
                        : "";

                // ======================================================
                // MENSAJE SRI NORMALIZADO (TEXTO PLANO)
                // ======================================================
                string mensajeSri = (recepcion.Error ?? "").ToUpperInvariant();
                r.EstadoSri = string.IsNullOrWhiteSpace(recepcion.EstadoSri)
                    ? estadoRecepcion
                    : recepcion.EstadoSri;
                r.MensajesSri = recepcion.MensajesSri ?? new List<SriMensajeDto>();

                // ======================================================
                // FLAGS SRI
                // ======================================================
                bool claveAccesoRegistrada =
                    mensajeSri.Contains("CLAVE ACCESO REGISTRADA") ||
                    mensajeSri.Contains("CLAVE DE ACCESO REGISTRADA") ||
                    mensajeSri.Contains("SECUENCIAL REGISTRADO") ||
                    mensajeSri.Contains("ERROR SECUENCIAL REGISTRADO");

                bool claveAccesoEnProceso =
                    mensajeSri.Contains("CLAVE DE ACCESO EN PROCESO") ||
                    mensajeSri.Contains("CLAVE ACCESO EN PROCESO") ||
                    mensajeSri.Contains("EN PROCESAMIENTO") ||
                    mensajeSri.Contains("PROCESAMIENTO");

                // ======================================================
                // 2) RECIBIDA → OK
                // ======================================================
                if (estadoRecepcion == "RECIBIDA")
                {
                    r.Exito = true;
                    r.Autorizado = false;
                    r.Mensaje = tipoDocumento + ": RECEPCIÓN aceptada por el SRI.";
                    return r;
                }

                // ======================================================
                // 3) CLAVE / SECUENCIAL REGISTRADO
                // ======================================================
                if (claveAccesoRegistrada)
                {
                    r.Exito = false;
                    r.SecuencialRepetido = true;
                    r.Mensaje = tipoDocumento + ": Secuencial / clave ya registrada en el SRI.";
                    return r;
                }


                // ======================================================
                // 4) EN PROCESAMIENTO → PENDIENTE_AUTORIZACION###
                // ======================================================
                if (claveAccesoEnProceso)
                {

                    // --------------------------------------------------
                    // 2) INSERTAR PENDIENTE_AUTORIZACION
                    //    (CON EL SECUENCIAL REAL YA USADO)
                    // --------------------------------------------------
                    string estadoPendienteAut =
                        _services.Pendientes
                            .ObtenerEstadoPendienteAutorizacionPorTipo(tipoDocumento);

                    _services.Pendientes.Insertar(
                        numeroDocumento,      // ← SECUENCIAL REAL DEL XML
                        claveAcceso,
                        rutaXmlFirmado,
                        DateTime.Now,
                        1,
                        estadoPendienteAut,
                        tipoDocumento,
                        usuarioActual,
                        ipActual
                    );

                    r.Exito = true;
                    r.Autorizado = false;
                    r.EstadoEspecial = estadoPendienteAut;
                    r.Mensaje =
                        tipoDocumento +
                        ": Documento en procesamiento por el SRI.\n" +
                        "Registrado con secuencial " + numeroDocumento +
                        " y sistema habilitado para emitir el siguiente.";

                    return r;
                }


                // ======================================================
                // 5) OTRO ERROR → PENDIENTE### (GENÉRICO)
                // ======================================================
                string estadoPendienteFinal =
                    _services.Pendientes.ObtenerEstadoPendientePorTipo(tipoDocumento);

                _services.Pendientes.Insertar(
                    numeroDocumento,
                    claveAcceso,
                    rutaXmlFirmado,
                    DateTime.Now,
                    1,
                    estadoPendienteFinal,
                    tipoDocumento,
                    usuarioActual,
                    ipActual
                );

                r.Exito = false;
                r.EstadoEspecial = estadoPendienteFinal;
                r.Mensaje = tipoDocumento +
                    ": Error en recepción SRI. Registrado como " +
                    estadoPendienteFinal +
                    ". Detalle: " + mensajeSri;

                return r;
            }
            catch (Exception ex)
            {
                string estadoPendiente = tipoDocumento + "_PENDIENTE";

                try
                {
                    estadoPendiente =
                        _services.Pendientes.ObtenerEstadoPendientePorTipo(tipoDocumento);

                    _services.Pendientes.Insertar(
                        estadoPendiente,
                        claveAcceso,
                        rutaXmlFirmado,
                        DateTime.Now,
                        0,
                        estadoPendiente,
                        tipoDocumento,
                        usuarioActual,
                        ipActual
                    );
                }
                catch { }

                r.Exito = false;
                r.EstadoEspecial = estadoPendiente;
                r.Mensaje =
                    tipoDocumento +
                    ": Error técnico en recepción SRI. Registrado como " +
                    estadoPendiente +
                    ". Detalle: " + ex.Message;

                return r;
            }
        }

        public ResultadoSRI ConsultarAutorizacion(
            string rutaXmlFirmado,
            string claveAcceso,
            string tipoDocumento,
            string numeroDocumento,
            string carpetaAutorizados,
            int maxIntentos,
            Action<int, string> onPendiente,
            Action<int, string> onErrorFinal,
            string usuarioActual,
            string ipActual
        )
        {
            ResultadoSRI r = new ResultadoSRI();

            try
            {
                Directory.CreateDirectory(carpetaAutorizados);

                string rutaAutorizado =
                    Path.Combine(carpetaAutorizados, claveAcceso + ".xml");

                Consultas consulta = new Consultas();
                string ultimoDetalleSri = "";

                for (int intento = 1; intento <= maxIntentos; intento++)
                {
                    var auth = consulta.autorizacion(
                        claveAcceso,
                        rutaXmlFirmado,
                        rutaAutorizado
                    );

                    if (auth != null)
                    {
                        string estadoAuth = auth.Estado ?? "";
                        r.EstadoSri = string.IsNullOrWhiteSpace(auth.EstadoSri)
                            ? estadoAuth
                            : auth.EstadoSri;
                        r.MensajesSri = auth.MensajesSri ?? new List<SriMensajeDto>();
                        ultimoDetalleSri = FormatearDetalleAutorizacionPendiente(auth);

                        if (estadoAuth == "AUTORIZADO" && File.Exists(rutaAutorizado))
                        {
                            var xml = XDocument.Load(rutaAutorizado);
                            r.Exito = true;
                            r.Mensaje = $"{tipoDocumento} AUTORIZADO.";
                            r.RutaXmlAutorizado = rutaAutorizado;
                            r.XmlAutorizado = xml;
                            return r;
                        }

                        if (estadoAuth == "NO AUTORIZADO")
                        {
                            onErrorFinal?.Invoke(intento, "NO AUTORIZADO");

                            DataSet dsPend = _services.Pendientes.ConsultarPorNumeroYTipo(numeroDocumento, tipoDocumento);
                            bool existePend = dsPend != null &&
                                             dsPend.Tables.Count > 0 &&
                                             dsPend.Tables[0].Rows.Count > 0;

                            if (existePend)
                                _services.Pendientes.ActualizarEstado(numeroDocumento, "NO_AUTORIZADO", tipoDocumento, usuarioActual, ipActual);
                            else
                                _services.Pendientes.Insertar(numeroDocumento, claveAcceso, rutaXmlFirmado, DateTime.Now, 1, "NO_AUTORIZADO", tipoDocumento, usuarioActual, ipActual);

                            r.Exito = false;
                            r.Mensaje = $"{tipoDocumento} NO AUTORIZADO por el SRI." +
                                        (string.IsNullOrWhiteSpace(auth.Error) ? "" : Environment.NewLine + auth.Error);
                            return r;
                        }
                    }

                    // Sigue en procesamiento
                    onPendiente?.Invoke(intento, "EN PROCESAMIENTO");

                    Thread.Sleep(2000);
                }

                // Agotó los intentos sin respuesta definitiva del SRI
                GuardarPendienteAutorizacion(
                    numeroDocumento,
                    claveAcceso,
                    rutaXmlFirmado,
                    tipoDocumento,
                    esPendienteAutorizacion: true,
                    usuarioActual,
                    ipActual
                );

                r.Exito = false;
                r.Mensaje = $"{tipoDocumento} quedó PENDIENTE tras {maxIntentos} intento(s)." +
                            (string.IsNullOrWhiteSpace(ultimoDetalleSri)
                                ? ""
                                : Environment.NewLine + "Última respuesta SRI:" + Environment.NewLine + ultimoDetalleSri);

                return r;
            }
            catch (Exception ex)
            {
                // Error técnico durante el polling — puede reintentarse
                try
                {
                    GuardarPendienteAutorizacion(
                        numeroDocumento,
                        claveAcceso,
                        rutaXmlFirmado,
                        tipoDocumento,
                        esPendienteAutorizacion: true,
                        usuarioActual,
                        ipActual
                    );
                }
                catch { }

                r.Exito = false;
                r.Mensaje = "Error en AUTORIZACIÓN SRI: " + ex.Message;
                return r;
            }
        }

        private static string FormatearDetalleAutorizacionPendiente(RespuestaAutorizacionSri auth)
        {
            if (auth == null)
                return "";

            if (!string.IsNullOrWhiteSpace(auth.Error))
                return auth.Error.Trim();

            string estado = !string.IsNullOrWhiteSpace(auth.EstadoSri)
                ? auth.EstadoSri
                : auth.Estado;

            if (string.IsNullOrWhiteSpace(estado) &&
                (auth.MensajesSri == null || auth.MensajesSri.Count == 0))
            {
                return "";
            }

            if (auth.MensajesSri == null || auth.MensajesSri.Count == 0)
                return "Estado: " + (string.IsNullOrWhiteSpace(estado) ? "SIN ESTADO" : estado);

            var lineas = new List<string>
            {
                "Estado: " + (string.IsNullOrWhiteSpace(estado) ? "SIN ESTADO" : estado)
            };

            foreach (var msg in auth.MensajesSri)
            {
                lineas.Add("Identificador: " + (string.IsNullOrWhiteSpace(msg.Identificador) ? "N/A" : msg.Identificador));
                lineas.Add("Mensaje: " + (msg.Mensaje ?? ""));
                lineas.Add("Detalle: " + (msg.InformacionAdicional ?? ""));
                lineas.Add("Tipo: " + (msg.Tipo ?? ""));
            }

            return string.Join(Environment.NewLine, lineas);
        }

        private void GuardarPendienteAutorizacion(
            string numeroDocumento,
            string claveAcceso,
            string rutaXmlFirmado,
            string tipoDocumento,
            bool esPendienteAutorizacion,
            string usuarioActual,
            string ipActual
        )
        {
            string estado = esPendienteAutorizacion
                ? _services.Pendientes.ObtenerEstadoPendienteAutorizacionPorTipo(tipoDocumento)
                : _services.Pendientes.ObtenerEstadoPendientePorTipo(tipoDocumento);

            DataSet ds = _services.Pendientes.ConsultarPorNumeroYTipo(numeroDocumento, tipoDocumento);

            bool existe = ds != null &&
                          ds.Tables.Count > 0 &&
                          ds.Tables[0].Rows.Count > 0;

            if (existe)
            {
                _services.Pendientes.ActualizarEstado(
                    numeroDocumento,
                    estado,
                    tipoDocumento,
                    usuarioActual,
                    ipActual
                );
            }
            else
            {
                _services.Pendientes.Insertar(
                    numeroDocumento,
                    claveAcceso,
                    rutaXmlFirmado,
                    DateTime.Now,
                    1,
                    estado,
                    tipoDocumento,
                    usuarioActual,
                    ipActual
                );
            }
        }

        public ResultadoPDF GenerarPdfDesdeXmlAutorizado(
            XDocument xmlAutorizado,
            DataRow rowEm,
            string carpetaPdf,
            Func<CGenerarRide, string, XDocument, Stream, string> metodoRide
        )
        {
            ResultadoPDF r = new ResultadoPDF();

            try
            {
                // carpeta base de PDFs (FACTURAS / NOTASCREDITO / RETENCIONES)
                string carpeta = Path.Combine(_services.Paths.Facturas, carpetaPdf);
                Directory.CreateDirectory(carpeta);

                // logo desde GENERAL\LOGOFACTURA
                string rutaLogo = Path.Combine(
                    _services.Paths.General,
                    "LOGOFACTURA",
                    rowEm["IMAGEN"].ToString()
                );

                if (!File.Exists(rutaLogo))
                    throw new Exception("Logo no encontrado: " + rutaLogo);

                // obtener XML interno del CDATA
                var cdata = xmlAutorizado.DescendantNodes().OfType<XCData>().FirstOrDefault();
                if (cdata == null)
                    throw new Exception("XML no contiene CDATA");

                var xmlInterno = XDocument.Parse(cdata.Value);

                // cargar logo
                var logoStream = new MemoryStream(File.ReadAllBytes(rutaLogo));

                var generador = new CGenerarRide();

                // generar PDF
                string rutaPdf = metodoRide(
                    generador,
                    carpeta,
                    xmlInterno,
                    logoStream
                );

                r.Exito = true;
                r.RutaPdf = rutaPdf;
                r.Mensaje = "PDF generado correctamente.";

                return r;
            }
            catch (Exception ex)
            {
                r.Exito = false;
                r.Mensaje = ex.Message;
                return r;
            }
        }

        //Procesar Desde Autorizacion API

        public bool ObtenerXmlFirmado(
            string tipoDocumento,
            string fechaDocumento,
            string numeroDocumento,
            out string claveAcceso,
            out string rutaXmlFirmado
        )
        {
            claveAcceso = "";
            rutaXmlFirmado = "";

            try
            {
                if (string.IsNullOrWhiteSpace(tipoDocumento))
                    return false;

                if (string.IsNullOrWhiteSpace(fechaDocumento))
                    return false;

                if (string.IsNullOrWhiteSpace(numeroDocumento))
                    return false;

                DateTime fecha;
                bool fechaValida = DateTime.TryParseExact(
                    fechaDocumento.Trim(),
                    new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fecha
                );

                if (!fechaValida)
                    return false;

                string carpetaXmlFirmados;

                switch (tipoDocumento.Trim().ToUpperInvariant())
                {
                    case "FACTURA":
                        carpetaXmlFirmados = Path.Combine(_services.Paths.Facturas, "XMLFIRMADOS");
                        break;

                    case "NOTA_CREDITO":
                    case "NOTADECREDITO":
                        carpetaXmlFirmados = Path.Combine(_services.Paths.NotasCredito, "XMLFIRMADOS");
                        break;

                    case "RETENCION":
                        carpetaXmlFirmados = Path.Combine(_services.Paths.Retenciones, "XMLFIRMADOS");
                        break;

                    default:
                        return false;
                }

                if (!Directory.Exists(carpetaXmlFirmados))
                    return false;

                string fechaFormato = fecha.ToString("ddMMyyyy");
                string patron = $"*{fechaFormato}*{numeroDocumento.Trim()}*.xml";

                string[] archivos = Directory.GetFiles(carpetaXmlFirmados, patron);

                if (archivos == null || archivos.Length == 0)
                    return false;

                rutaXmlFirmado = archivos[0];
                claveAcceso = Path.GetFileNameWithoutExtension(rutaXmlFirmado);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PrepararPendienteDocumento(
            string tipoDocumento,
            string numeroDocumento,
            string fechaDocumento,
            string usuarioActual,
            out string claveAcceso,
            out string rutaXmlFirmado,
            out DataRow rowEmpresa,
            out DataRow rowCliente,
            out string mensajeError
        )
        {
            claveAcceso = "";
            rutaXmlFirmado = "";
            rowEmpresa = null;
            rowCliente = null;
            mensajeError = "";

            try
            {
                if (string.IsNullOrWhiteSpace(tipoDocumento))
                {
                    mensajeError = "Tipo de documento inválido.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    mensajeError = "Número de documento inválido.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(fechaDocumento))
                {
                    mensajeError = "Fecha inválida.";
                    return false;
                }

                if (!_services.ProcesosGenerales.ObtenerXmlFirmado(
                    tipoDocumento,
                    fechaDocumento,
                    numeroDocumento,
                    out claveAcceso,
                    out rutaXmlFirmado
                ))
                {
                    mensajeError = "No se pudo localizar el XML firmado del documento.";
                    return false;
                }

                var dsEmp = _services.Empresa.ConsultaNombre(usuarioActual);
                if (dsEmp == null || dsEmp.Tables.Count == 0 || dsEmp.Tables[0].Rows.Count == 0)
                {
                    mensajeError = "Empresa no encontrada.";
                    return false;
                }

                rowEmpresa = dsEmp.Tables[0].Rows[0];

                switch (tipoDocumento.Trim().ToUpperInvariant())
                {
                    case "FACTURA":
                        {
                            var dsFactura = _services.Facturacion.ConsultarPorNumeroFactura(numeroDocumento);
                            if (dsFactura == null || dsFactura.Tables.Count == 0 || dsFactura.Tables[0].Rows.Count == 0)
                            {
                                mensajeError = "Factura no encontrada.";
                                return false;
                            }

                            string cedula = dsFactura.Tables[0].Rows[0]["CLIENTE"]?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(cedula))
                            {
                                mensajeError = "No se encontró la cédula del cliente.";
                                return false;
                            }

                            var dsCli = _services.Cliente.ConsultarCedula(cedula);
                            if (dsCli == null || dsCli.Tables.Count == 0 || dsCli.Tables[0].Rows.Count == 0)
                            {
                                mensajeError = "Cliente no encontrado.";
                                return false;
                            }

                            rowCliente = dsCli.Tables[0].Rows[0];
                            return true;
                        }

                    case "NOTA_CREDITO":
                    case "NOTACREDITO":
                        {
                            var dsNota = _services.NotaCredito.ConsultarPorNumeroNota(numeroDocumento);
                            if (dsNota == null || dsNota.Tables.Count == 0 || dsNota.Tables[0].Rows.Count == 0)
                            {
                                mensajeError = "Nota de crédito no encontrada.";
                                return false;
                            }

                            string cedula = dsNota.Tables[0].Rows[0]["CLIENTE"]?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(cedula))
                            {
                                mensajeError = "No se encontró la cédula del cliente de la nota.";
                                return false;
                            }

                            var dsCli = _services.Cliente.ConsultarCedula(cedula);
                            if (dsCli == null || dsCli.Tables.Count == 0 || dsCli.Tables[0].Rows.Count == 0)
                            {
                                mensajeError = "Cliente de la nota no encontrado.";
                                return false;
                            }

                            rowCliente = dsCli.Tables[0].Rows[0];
                            return true;
                        }

                    case "RETENCION":
                        {
                            var dsRet = _services.Retencion.ConsultarPorNumero(numeroDocumento);
                            if (dsRet == null || dsRet.Tables.Count == 0 || dsRet.Tables[0].Rows.Count == 0)
                            {
                                mensajeError = "Retención no encontrada.";
                                return false;
                            }

                            string identificacion = dsRet.Tables[0].Rows[0]["IDENTIFICACIONSUJETO"]?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(identificacion))
                            {
                                mensajeError = "No se encontró la identificación del sujeto retenido.";
                                return false;
                            }

                            var dsProv = _services.Proveedor.ConsultarPorIdentificacion(identificacion);
                            if (dsProv == null || dsProv.Tables.Count == 0 || dsProv.Tables[0].Rows.Count == 0)
                            {
                                mensajeError = "Sujeto retenido no encontrado.";
                                return false;
                            }

                            rowCliente = dsProv.Tables[0].Rows[0];
                            return true;
                        }

                    default:
                        mensajeError = "Tipo de documento no soportado.";
                        return false;
                }
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                return false;
            }
        }


        // ==========================================================
        // LIMPIEZA DE ARCHIVOS TEMPORALES
        // ==========================================================

        /// <summary>
        /// Elimina XML y XMLFIRMADO solo si el XMLAUTORIZADO ya existe físicamente.
        /// Ignorar silenciosamente archivos inexistentes; registrar en log si falla.
        /// </summary>
        public void LimpiarXmlTransitorios(string rutaXml, string rutaXmlFirmado, string rutaXmlAutorizado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rutaXmlAutorizado) || !File.Exists(rutaXmlAutorizado))
                    return;

                EliminarArchivoSilencioso(rutaXml, "XML");
                EliminarArchivoSilencioso(rutaXmlFirmado, "XMLFIRMADO");
            }
            catch (Exception ex)
            {
                LogLimpieza("LimpiarXmlTransitorios ERROR: " + ex.Message);
            }
        }

        /// <summary>
        /// Elimina archivos .pdf de la carpeta PDFPREVIEW que superen la antigüedad máxima.
        /// No falla si la carpeta no existe o un archivo está bloqueado.
        /// </summary>
        public void LimpiarPreviewsAntiguos(string carpetaPdfPreview, TimeSpan antiguedadMaxima)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(carpetaPdfPreview) || !Directory.Exists(carpetaPdfPreview))
                    return;

                DateTime limite = DateTime.Now - antiguedadMaxima;

                foreach (string archivo in Directory.GetFiles(carpetaPdfPreview, "*.pdf"))
                {
                    try
                    {
                        if (File.GetLastWriteTime(archivo) < limite)
                        {
                            File.Delete(archivo);
                            LogLimpieza("Preview eliminado: " + archivo);
                        }
                    }
                    catch
                    {
                        // archivo bloqueado o ya borrado — ignorar
                    }
                }
            }
            catch (Exception ex)
            {
                LogLimpieza("LimpiarPreviewsAntiguos ERROR: " + ex.Message);
            }
        }

        private void EliminarArchivoSilencioso(string ruta, string etiqueta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
                    return;

                File.Delete(ruta);
                LogLimpieza("Eliminado " + etiqueta + ": " + ruta);
            }
            catch (Exception ex)
            {
                LogLimpieza("No se pudo eliminar " + etiqueta + " [" + ruta + "]: " + ex.Message);
            }
        }

        private void LogLimpieza(string mensaje)
        {
            try
            {
                string archivo = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "logs", "limpieza_debug.log");
                Directory.CreateDirectory(Path.GetDirectoryName(archivo));
                File.AppendAllText(
                    archivo,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + mensaje + Environment.NewLine);
            }
            catch { }
        }

        //INTERFAZ

        public static class NumericHelper
        {
            /// <summary>
            /// Convierte texto de usuario a decimal.
            /// Soporta formatos típicos: "1", "1,5", "1.234,56", "1234,56".
            /// </summary>
            public static decimal ParseUserDecimal(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return 0m;

                input = NormalizeUserDecimalInput(input);

                if (!decimal.TryParse(
                        input,
                        NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture,
                        out decimal value))
                {
                    throw new Exception("Valor numérico inválido: " + input);
                }

                return value;
            }

            /// <summary>
            /// Valida si el texto puede convertirse a decimal con la misma regla de ParseUserDecimal,
            /// sin lanzar excepción.
            /// </summary>
            public static bool TryParseUserDecimal(string input)
            {
                decimal _;
                return TryParseUserDecimal(input, out _);
            }

            /// <summary>
            /// Intenta convertir texto de usuario a decimal, sin lanzar excepción.
            /// </summary>
            public static bool TryParseUserDecimal(string input, out decimal value)
            {
                value = 0m;

                if (string.IsNullOrWhiteSpace(input))
                    return true; // vacío = 0 válido para tu flujo

                string normalized = NormalizeUserDecimalInput(input);

                return decimal.TryParse(
                    normalized,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out value
                );
            }

            /// <summary>
            /// Formato SRI: siempre punto decimal y 2 decimales.
            /// </summary>
            public static string ToSri(decimal value)
            {
                return value.ToString("0.00", CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Normaliza entrada del usuario:
            /// - quita separadores de miles (.)
            /// - convierte decimal coma a punto
            /// </summary>
            private static string NormalizeUserDecimalInput(string input)
            {
                input = input.Trim();

                // Si el usuario mete espacios internos, los quitamos
                input = input.Replace(" ", "");

                // Regla simple que ya estabas usando:
                // 1) quitar miles "."
                // 2) convertir decimal "," a "."
                input = input.Replace(".", "").Replace(",", ".");

                return input;
            }
        }



        // ==========================================================
        // DTO INTERNO
        // ==========================================================
        private class CorreoArmado
        {
            public string CorreoCliente;
            public string CorreoEmpresa;
            public string AsuntoCliente;
            public string CuerpoCliente;
            public string AsuntoEmpresa;
            public string CuerpoEmpresa;
        }
    }

    // ==============================================================
    // DTO EXTERNOS
    // ==============================================================
    public class SolicitudCorreoDocumento
    {
        public string TipoDocumento;      // FACTURA | RETENCION | NOTADECREDITO
        public string NumeroDocumento;
        public string RutaPdf;
        public string RutaXmlAutorizado;
    }

    public class ResultadoSRI
    {
        public bool Exito { get; set; }
        public bool Autorizado { get; set; }
        public bool SecuencialRepetido { get; set; }
        public string Mensaje { get; set; }
        public string EstadoSri { get; set; }
        public List<SriMensajeDto> MensajesSri { get; set; } = new List<SriMensajeDto>();
        public string EstadoEspecial { get; set; }
        public string RutaXmlAutorizado { get; set; }
        public XDocument XmlAutorizado { get; set; }
    }

    public class AccionPendienteDocumento
    {
        public bool Existe { get; set; }

        public string Tipo { get; set; }              // NOTA_CREDITO
        public string NumeroDocumento { get; set; }

        public string EstadoPendiente { get; set; }   // PENDIENTE / PENDIENTE_AUTORIZACION / PENDIENTE_CORREO

        public string TextoBoton { get; set; }         // PROCESAR / CONSULTAR SRI / ENVIAR CORREO
        public string Accion { get; set; }             // PROCESAR / AUTORIZAR / CORREO

        public bool MostrarPdf { get; set; }
        public bool MostrarXml { get; set; }
    }

    // ==============================================================
    // HELPER IVA — cálculos reutilizables en cualquier proceso
    // ==============================================================
    public static class HelperIva
    {
        /// <summary>
        /// Convierte el código SRI de porcentaje al decimal de tarifa.
        /// Códigos: 0=0%, 2=12%, 3=14%, 4=15%, 5=5%, 6=No Objeto,
        ///          7=Exento, 8=8%, 10=13%
        /// </summary>
        public static decimal TarifaDesdeCodigoPorcentaje(string codigoPorcentaje)
        {
            switch ((codigoPorcentaje ?? "").Trim())
            {
                case "2":  return 0.12m;
                case "3":  return 0.14m;
                case "4":  return 0.15m;
                case "5":  return 0.05m;
                case "8":  return 0.08m;
                case "10": return 0.13m;
                default:   return 0m;   // 0%, No Objeto, Exento
            }
        }

        /// <summary>
        /// Calcula la base imponible y el IVA de una línea.
        /// El totalLinea se trata como valor neto (sin IVA); el IVA se suma encima.
        /// </summary>
        public static (decimal Base, decimal Iva) CalcularBaseEIvaLinea(decimal totalLinea, decimal tarifa, decimal iceValor = 0m)
        {
            decimal ivaLinea = tarifa > 0m ? Math.Round((totalLinea + iceValor) * tarifa, 2) : 0m;
            return (totalLinea, ivaLinea);
        }

        // ----------------------------------------------------------
        // ICE
        // ----------------------------------------------------------

        public struct ResultadoIceLinea
        {
            public bool    Aplica;
            public string  Codigo;
            public decimal Valor;          // valor ICE de la línea
            public decimal BaseImponible;  // base para el XML <baseImponible>
            public decimal Tarifa;         // tarifa numérica para el XML <tarifa>
        }

        /// <summary>
        /// Calcula el ICE de una línea a partir del DataRow del producto.
        /// subtotalSinImp = precio neto × cantidad (base antes de ICE e IVA).
        /// cantidad       = unidades (para ESPECIFICO/AZUCAR).
        /// </summary>
        public static ResultadoIceLinea CalcularIceLinea(DataRow prod, decimal cantidad, decimal subtotalSinImp)
        {
            var result = new ResultadoIceLinea();
            if (prod == null) return result;

            bool aplicaIce = false;
            var vIce = prod["APLICA_ICE"];
            if (vIce != null && vIce != DBNull.Value)
            {
                if (vIce is bool bv) aplicaIce = bv;
                else { string s = vIce.ToString().Trim().ToUpperInvariant(); aplicaIce = (s == "SI" || s == "1" || s == "TRUE" || s == "YES"); }
            }
            if (!aplicaIce) return result;

            string  iceCodigo  = GetStr(prod, "ICE_CODIGO");
            string  iceTipo    = GetStr(prod, "ICE_TIPO").ToUpperInvariant();
            decimal icePct     = GetDec(prod, "ICE_PORCENTAJE");
            decimal iceVlr     = GetDec(prod, "ICE_VALOR");
            decimal azucar     = GetDec(prod, "AZUCAR_GRAMOS");
            decimal volumen    = GetDec(prod, "VOLUMEN");
            string  iceUnidad  = GetStr(prod, "ICE_UNIDAD").ToUpperInvariant();

            decimal iceValor  = 0m;
            decimal iceBase   = subtotalSinImp;
            decimal iceTarifa = 0m;

            if (iceTipo == "AD_VALOREM")
            {
                iceValor  = Math.Round(subtotalSinImp * (icePct / 100m), 2);
                iceBase   = subtotalSinImp;
                iceTarifa = icePct;
            }
            else if (iceTipo == "ESPECIFICO")
            {
                decimal unids = (iceUnidad == "LITRO") ? (volumen > 0m ? cantidad * volumen : cantidad) : cantidad;
                iceValor  = Math.Round(unids * iceVlr, 2);
                iceBase   = unids;
                iceTarifa = iceVlr;
            }
            else if (iceTipo == "AZUCAR")
            {
                decimal unidades100g = (azucar / 100m) * cantidad;
                iceValor  = Math.Round(unidades100g * iceVlr, 2);
                iceBase   = unidades100g;
                iceTarifa = iceVlr;
            }
            else if (iceTipo == "MIXTO")
            {
                decimal iceAdVal = Math.Round(subtotalSinImp * (icePct / 100m), 2);
                decimal litros   = volumen > 0m ? cantidad * volumen : cantidad;
                decimal iceEsp   = Math.Round(litros * iceVlr, 2);
                iceValor  = iceAdVal + iceEsp;
                iceBase   = subtotalSinImp;
                iceTarifa = icePct;
            }

            result.Aplica        = true;
            result.Codigo        = iceCodigo;
            result.Valor         = iceValor;
            result.BaseImponible = iceBase;
            result.Tarifa        = iceTarifa;
            return result;
        }

        private static string GetStr(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col)) return "";
            var v = row[col];
            return (v == null || v == DBNull.Value) ? "" : v.ToString().Trim();
        }

        private static decimal GetDec(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col)) return 0m;
            var v = row[col];
            if (v == null || v == DBNull.Value) return 0m;
            decimal d = 0m;
            decimal.TryParse(v.ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
            return d;
        }
    }

}
