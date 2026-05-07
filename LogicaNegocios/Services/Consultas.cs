using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using es.mityc.javasign.pkstore;
using Firmador;
using GenerarXml.Dto;


namespace LogicaNegocios.Services
{
    public class Consultas
    {
            public bool Firma(string archivop12, string contrasena, string RutaXml, string RutaFi)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(archivop12))
                        throw new Exception("La ruta del archivo P12 está vacía.");

                    if (string.IsNullOrWhiteSpace(contrasena))
                        throw new Exception("La contraseña del certificado está vacía.");

                    if (string.IsNullOrWhiteSpace(RutaXml))
                        throw new Exception("La ruta del XML sin firmar está vacía.");

                    if (string.IsNullOrWhiteSpace(RutaFi))
                        throw new Exception("La ruta del XML firmado está vacía.");

                    if (!File.Exists(archivop12))
                        throw new Exception("No existe el archivo P12: " + archivop12);

                    if (!File.Exists(RutaXml))
                        throw new Exception("No existe el XML a firmar: " + RutaXml);

                    string carpetaSalida = Path.GetDirectoryName(RutaFi);
                    if (string.IsNullOrWhiteSpace(carpetaSalida))
                        throw new Exception("No se pudo determinar la carpeta de salida del XML firmado.");

                    Directory.CreateDirectory(carpetaSalida);

                    var signer = new FirmadorNativo();
                    signer.Sign(RutaXml, RutaFi, archivop12, contrasena);

                    if (!File.Exists(RutaFi))
                        throw new Exception("La firma terminó, pero no se generó el XML firmado: " + RutaFi);

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error en firma integrada DLL: " + ex.ToString(), ex);
                }
            }

    private string LeerAmbienteDeXml(string rutaXml)
        {
            try
            {
                if (!File.Exists(rutaXml)) return "1";
                var doc = XDocument.Load(rutaXml);
                var val = doc.Descendants("ambiente").FirstOrDefault()?.Value?.Trim();
                return val == "2" ? "2" : "1";
            }
            catch { return "1"; }
        }

        public RespuestaRecepcionSri recepcion(string Clave, string RutaFirmado)
        {
            CFuncionesComprobantesElectronicos cFuncionesComprobantes = new CFuncionesComprobantesElectronicos();

            string ambiente = LeerAmbienteDeXml(RutaFirmado);
            var recepcion = ambiente == "2"
                ? cFuncionesComprobantes.RecepcionComprobante(RutaFirmado)
                : cFuncionesComprobantes.RecepcionComprobantePrueba(RutaFirmado);

            // ==========================================
            // CASO ÉXITO: RECIBIDA
            // ==========================================
            if (recepcion.Estado != null && recepcion.Estado.Equals("RECIBIDA", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaRecepcionSri
                {
                    RespuestaRecepcion = recepcion.Estado,
                    EstadoSri = recepcion.Estado,
                    MensajesSri = ConvertirErroresSri(recepcion.Errores)
                };
            }

            // ==========================================
            // CASO ERROR (DEVUELTA / NO RECIBIDA)
            // LEER ERRORES CAPTURADOS DEL SOAP
            // ==========================================
            string mensajeError = "";

            var mensajesRecepcion = ConvertirErroresSri(recepcion.Errores);
            mensajeError = FormatearMensajesSri(recepcion.Estado, mensajesRecepcion);

            return new RespuestaRecepcionSri
            {
                Error = mensajeError,
                EstadoSri = recepcion.Estado,
                MensajesSri = mensajesRecepcion
            };
        }

        // ==========================================
        // RECEPCIÓN POR LOTE (TABLA 8 SRI)
        // ==========================================
        public RespuestaRecepcionSri recepcionLote(
            string ruc,
            List<(string ClaveAcceso, string RutaFirmado)> documentos
        )
        {
            if (documentos == null || documentos.Count == 0)
                return new RespuestaRecepcionSri { Error = "No hay documentos para el lote." };

            // ─── Construir XML lote ───────────────────────────────
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<lote version=\"1.0.0\">");
            sb.Append("<claveAcceso>").Append(documentos[0].ClaveAcceso).Append("</claveAcceso>");
            sb.Append("<ruc>").Append(ruc).Append("</ruc>");
            sb.Append("<comprobantes>");

            foreach (var doc in documentos)
            {
                string contenido = File.ReadAllText(doc.RutaFirmado, Encoding.UTF8);
                sb.Append("<comprobante><![CDATA[").Append(contenido).Append("]]></comprobante>");
            }

            sb.Append("</comprobantes>");
            sb.Append("</lote>");

            string loteXml = sb.ToString();

            // ─── Llamar al SRI ────────────────────────────────────
            CFuncionesComprobantesElectronicos cFunciones = new CFuncionesComprobantesElectronicos();

            string ambienteLote = LeerAmbienteDeXml(documentos[0].RutaFirmado);
            var recepcion = ambienteLote == "2"
                ? cFunciones.RecepcionLoteOffline(loteXml)
                : cFunciones.RecepcionLotePrueba(loteXml);

            if (recepcion == null)
                return new RespuestaRecepcionSri { Error = "Sin respuesta del SRI para el lote." };

            if (recepcion.Estado != null &&
                recepcion.Estado.Equals("RECIBIDA", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaRecepcionSri
                {
                    RespuestaRecepcion = recepcion.Estado,
                    EstadoSri = recepcion.Estado,
                    MensajesSri = ConvertirErroresSri(recepcion.Errores)
                };
            }

            var mensajesLote = ConvertirErroresSri(recepcion.Errores);
            string mensajeError = FormatearMensajesSri(recepcion.Estado, mensajesLote);

            return new RespuestaRecepcionSri
            {
                Error = mensajeError,
                EstadoSri = recepcion.Estado,
                MensajesSri = mensajesLote
            };
        }

        public RespuestaAutorizacionSri autorizacion(string clave, string rutaFirmado, string rutaAutorizado)
        {
            var respuesta = new RespuestaAutorizacionSri();
            var cFunciones = new CFuncionesComprobantesElectronicos();

            string ambiente = LeerAmbienteDeXml(rutaFirmado);
            var autorizacion = ambiente == "2"
                ? cFunciones.AutorizacionComprobante(clave)
                : cFunciones.AutorizacionComprobantePrueba(clave);

            if (autorizacion == null)
            {
                respuesta.Error = "No se pudo consultar autorización";
                respuesta.EstadoSri = "SIN_RESPUESTA";
                return respuesta;
            }

            respuesta.EstadoSri = autorizacion.Estado;
            respuesta.ClaveAccesoConsultada = autorizacion.ClaveAcceso;
            respuesta.NumeroComprobantes = autorizacion.NumeroComprobantes;

            if (autorizacion.Comprobantes == null || autorizacion.Comprobantes.Count == 0)
            {
                // Igual que en recepción: si no hay comprobantes, intenta mostrar errores del SOAP
                var mensajesSinComprobantes = ConvertirErroresSri(autorizacion.Errores);
                string mensajeError = FormatearMensajesSri("SIN COMPROBANTES", mensajesSinComprobantes);

                respuesta.Error = mensajeError;
                respuesta.MensajesSri = mensajesSinComprobantes;
                return respuesta;
            }

            var comp = autorizacion.Comprobantes[0];

            respuesta.Estado = comp.Estado?.Trim().ToUpper();

            if (comp.Estado != null && comp.Estado.Equals("AUTORIZADO", StringComparison.OrdinalIgnoreCase))
            {
                cFunciones.xmlAutorizado(
                    rutaFirmado,
                    rutaAutorizado,
                    comp.Estado,
                    autorizacion.ClaveAcceso,
                    comp.FechaAutorizacion
                );

                respuesta.RespuestaAutorizacion = comp.Estado;
                respuesta.EstadoSri = comp.Estado;
                respuesta.MensajesSri = ConvertirMensajesSri(comp.Mensajes);
                return respuesta;
            }

            // ==========================================
            // CASO ERROR (NO AUTORIZADO / OTRO ESTADO)
            // ==========================================
            string mensajeErrorNoAut = "";

            var mensajesNoAut = ConvertirMensajesSri(comp.Mensajes);
            if (mensajesNoAut.Count == 0)
                mensajesNoAut = ConvertirErroresSri(autorizacion.Errores);

            mensajeErrorNoAut = FormatearMensajesSri(comp.Estado, mensajesNoAut);

            respuesta.Error = mensajeErrorNoAut;
            respuesta.EstadoSri = comp.Estado;
            respuesta.MensajesSri = mensajesNoAut;
            return respuesta;
        }

        private static List<SriMensajeDto> ConvertirErroresSri(IEnumerable<CErrorSri> errores)
        {
            if (errores == null)
                return new List<SriMensajeDto>();

            return errores
                .Where(e => e != null)
                .Select(e => new SriMensajeDto
                {
                    Identificador = e.Identificador ?? "",
                    Mensaje = e.Mensaje ?? "",
                    InformacionAdicional = e.InformacionAdicional ?? "",
                    Tipo = e.Tipo ?? ""
                })
                .Where(e =>
                    !string.IsNullOrWhiteSpace(e.Identificador) ||
                    !string.IsNullOrWhiteSpace(e.Mensaje) ||
                    !string.IsNullOrWhiteSpace(e.InformacionAdicional) ||
                    !string.IsNullOrWhiteSpace(e.Tipo))
                .ToList();
        }

        private static List<SriMensajeDto> ConvertirMensajesSri(IEnumerable<Mensajes> mensajes)
        {
            if (mensajes == null)
                return new List<SriMensajeDto>();

            return mensajes
                .Where(m => m != null)
                .Select(m => new SriMensajeDto
                {
                    Identificador = m.Identificador ?? "",
                    Mensaje = m.mensajes ?? "",
                    InformacionAdicional = m.InformacionAdicional ?? "",
                    Tipo = m.Tipo ?? ""
                })
                .Where(m =>
                    !string.IsNullOrWhiteSpace(m.Identificador) ||
                    !string.IsNullOrWhiteSpace(m.Mensaje) ||
                    !string.IsNullOrWhiteSpace(m.InformacionAdicional) ||
                    !string.IsNullOrWhiteSpace(m.Tipo))
                .ToList();
        }

        private static string FormatearMensajesSri(string estado, List<SriMensajeDto> mensajes)
        {
            string estadoFinal = string.IsNullOrWhiteSpace(estado) ? "SIN ESTADO" : estado;
            if (mensajes == null || mensajes.Count == 0)
                return "Estado: " + estadoFinal + " (No existen mensajes detallados del SRI)";

            var sb = new StringBuilder();
            sb.Append("Estado: ").Append(estadoFinal);

            foreach (var msg in mensajes)
            {
                sb.AppendLine();
                sb.Append("Identificador: ").Append(string.IsNullOrWhiteSpace(msg.Identificador) ? "N/A" : msg.Identificador).AppendLine();
                sb.Append("Mensaje: ").Append(msg.Mensaje ?? "").AppendLine();
                sb.Append("Detalle: ").Append(msg.InformacionAdicional ?? "").AppendLine();
                sb.Append("Tipo: ").Append(msg.Tipo ?? "");
            }

            return sb.ToString();
        }

    }
}

