using LogicaNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogicaNegocios.Procesos
{
    // ================================================================
    // DTOs públicos
    // ================================================================

    /// <summary>
    /// Datos mínimos que el formulario pasa por cada fila pendiente.
    /// </summary>
    public class SolicitudItemLote
    {
        public string NumeroViejo { get; set; }   // ej. "PENDIENTE_FACTURA_001"
        public string Cedula      { get; set; }
        public string Fecha       { get; set; }
        public string Hora        { get; set; }
        public string Total       { get; set; }
    }

    /// <summary>
    /// Resultado individual por documento dentro del lote.
    /// </summary>
    public class ResultadoItemLote
    {
        public string NumeroViejo          { get; set; }
        public string NumeroNuevo          { get; set; }
        public bool   Exito                { get; set; }
        public bool   Autorizado           { get; set; }
        public bool   EnvioCorreoExitoso   { get; set; }
        public string Mensaje              { get; set; }
    }

    /// <summary>
    /// Resumen global del lote.
    /// </summary>
    public class ResultadoLote
    {
        public int                       Total               { get; set; }
        public int                       Autorizados         { get; set; }
        public int                       PendienteAutorizacion { get; set; }
        public int                       Errores             { get; set; }
        public List<ResultadoItemLote>   Items               { get; set; } = new List<ResultadoItemLote>();
    }

    // ================================================================
    // Clase principal
    // ================================================================
    public class ProcesosLote
    {
        private readonly AppServices _services;

        public ProcesosLote(AppServices services)
        {
            _services = services;
        }

        // ── Estado intermedio por documento ──────────────────────────
        private class ItemPreparado
        {
            public SolicitudItemLote Solicitud    { get; set; }
            public string            NumeroFactura { get; set; }
            public string            ClaveAcceso   { get; set; }
            public string            RutaXmlFirmado { get; set; }
            public DataRow           RowCliente    { get; set; }
            public DataTable         Detalle       { get; set; }
            public string            FechaPago     { get; set; }
            public string            FormaPago     { get; set; }
            public string            ClienteTexto  { get; set; }
            public string            TotalVisual   { get; set; }
            public DataSet           Em            { get; set; }
            public DataSet           Pfm           { get; set; }
        }

        // ================================================================
        // MÉTODO PRINCIPAL
        // ================================================================
        public async Task<ResultadoLote> ProcesarPendientesEnLote(
            List<SolicitudItemLote> solicitudes,
            string usuarioActual,
            string ipActual,
            Action<string> onProgreso = null
        )
        {
            solicitudes = (solicitudes ?? new List<SolicitudItemLote>())
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.NumeroViejo))
                .GroupBy(s => s.NumeroViejo.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            ResultadoLote resultado = new ResultadoLote { Total = solicitudes.Count };

            // ─── Cargar parámetros una sola vez para todo el lote ────────
            DataSet em  = _services.Empresa.MostrarEmpresa(usuarioActual);
            DataSet pfm = _services.ParamFactura.ConsultarNombre(usuarioActual);
            DataSet pm  = _services.Param.ConsultarPorTipo("01");

            if (em.Tables[0].Rows.Count == 0 || pfm.Tables[0].Rows.Count == 0 || pm.Tables[0].Rows.Count == 0)
            {
                foreach (var s in solicitudes)
                    resultado.Items.Add(new ResultadoItemLote
                    {
                        NumeroViejo = s.NumeroViejo,
                        Exito = false,
                        Mensaje = "Error: no se pudieron cargar parámetros de empresa/factura."
                    });

                resultado.Errores = solicitudes.Count;
                return resultado;
            }

            string ruc = em.Tables[0].Rows[0]["RUC"].ToString();

            // ─── FASE 1: preparar → XML → firmar cada documento ──────────
            onProgreso?.Invoke("Preparando documentos para el lote...");

            List<ItemPreparado>  preparados = new List<ItemPreparado>();
            List<ResultadoItemLote> erroresPreparacion = new List<ResultadoItemLote>();

            foreach (var solicitud in solicitudes)
            {
                onProgreso?.Invoke("Preparando: " + solicitud.NumeroViejo);

                var itemResultado = await Task.Run(() =>
                    PrepararUnItem(solicitud, em, pfm, pm, usuarioActual, ipActual)
                );

                if (itemResultado == null)
                {
                    erroresPreparacion.Add(new ResultadoItemLote
                    {
                        NumeroViejo = solicitud.NumeroViejo,
                        Exito = false,
                        Mensaje = "Error al preparar el documento."
                    });
                    continue;
                }

                if (!string.IsNullOrEmpty(itemResultado.ClaveAcceso))
                {
                    preparados.Add(itemResultado);
                    // Incrementar secuencial después de cada documento preparado
                    _services.ProcesosFacturacion.IncrementarSecuencialFactura("01");
                }
                else
                {
                    erroresPreparacion.Add(new ResultadoItemLote
                    {
                        NumeroViejo = solicitud.NumeroViejo,
                        Exito = false,
                        Mensaje = itemResultado.FormaPago // se usa como mensaje de error temporal
                    });
                }
            }

            resultado.Items.AddRange(erroresPreparacion);
            resultado.Errores += erroresPreparacion.Count;

            if (preparados.Count == 0)
            {
                // Todos fallaron en preparación
                return resultado;
            }

            // ─── FASE 2: enviar lote al SRI ───────────────────────────────
            onProgreso?.Invoke($"Enviando lote de {preparados.Count} documentos al SRI...");

            var itemsParaLote = preparados
                .Select(p => (p.ClaveAcceso, p.RutaXmlFirmado))
                .ToList();

            RespuestaRecepcionSriLote respuestaLote = null;

            try
            {
                respuestaLote = await Task.Run(() =>
                {
                    var consulta = new Consultas();
                    var resp = consulta.recepcionLote(ruc, itemsParaLote);
                    return new RespuestaRecepcionSriLote
                    {
                        Recibida  = resp.RespuestaRecepcion != null &&
                                    resp.RespuestaRecepcion.Equals("RECIBIDA", StringComparison.OrdinalIgnoreCase),
                        Error     = resp.Error
                    };
                });
            }
            catch (Exception ex)
            {
                respuestaLote = new RespuestaRecepcionSriLote
                {
                    Recibida = false,
                    Error    = "Error técnico enviando lote: " + ex.Message
                };
            }

            // ─── FASE 3: procesar resultado por documento ─────────────────
            if (!respuestaLote.Recibida)
            {
                // Lote rechazado → registrar todos como PENDIENTE en FACTURACION
                onProgreso?.Invoke("Lote no recibido por el SRI. Registrando como pendientes...");

                foreach (var item in preparados)
                {
                    string estadoPendiente = _services.Pendientes
                        .ObtenerEstadoPendientePorTipo("FACTURA");

                    try
                    {
                        var insertado = _services.ProcesosFacturacion.InsertarFactura(
                            item.Detalle,
                            item.FechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            item.FormaPago,
                            item.ClienteTexto,
                            estadoPendiente,
                            usuarioActual,
                            ipActual
                        );

                        if (insertado.Exito)
                        {
                            _services.Pendientes.Insertar(
                                estadoPendiente,
                                item.ClaveAcceso,
                                item.RutaXmlFirmado,
                                DateTime.Now,
                                0,
                                estadoPendiente,
                                "FACTURA",
                                usuarioActual,
                                ipActual
                            );

                            try
                            {
                                _services.Facturacion.EliminarPorSecuencial(
                                    item.Solicitud.NumeroViejo,
                                    usuarioActual,
                                    ipActual
                                );
                            }
                            catch { }

                            try
                            {
                                _services.Pendientes.Eliminar(
                                    item.Solicitud.NumeroViejo,
                                    "FACTURA",
                                    usuarioActual,
                                    ipActual
                                );
                            }
                            catch { }
                        }
                    }
                    catch { }

                    resultado.Items.Add(new ResultadoItemLote
                    {
                        NumeroViejo = item.Solicitud.NumeroViejo,
                        NumeroNuevo = estadoPendiente,
                        Exito       = false,
                        Mensaje     = "Lote rechazado por SRI. Registrado como " + estadoPendiente +
                                      ". " + (respuestaLote.Error ?? "")
                    });

                    resultado.Errores++;
                }

                return resultado;
            }

            // Lote RECIBIDA → autorizar cada documento individualmente
            onProgreso?.Invoke("Lote RECIBIDO. Consultando autorización por documento...");

            int indice = 0;
            foreach (var item in preparados)
            {
                indice++;
                onProgreso?.Invoke($"Autorizando {indice} de {preparados.Count}: {item.NumeroFactura}");

                var itemResult = await Task.Run(() =>
                    FinalizarItem(item, em, usuarioActual, ipActual)
                );

                // Eliminar el registro viejo si el proceso fue exitoso
                if (itemResult.Exito)
                {
                    try
                    {
                        _services.Facturacion.EliminarPorSecuencial(
                            item.Solicitud.NumeroViejo,
                            usuarioActual,
                            ipActual
                        );
                    }
                    catch { }
                }

                resultado.Items.Add(itemResult);

                if (itemResult.Exito && itemResult.Autorizado)
                    resultado.Autorizados++;
                else if (itemResult.Exito && !itemResult.Autorizado)
                    resultado.PendienteAutorizacion++;
                else
                    resultado.Errores++;
            }

            return resultado;
        }

        // ================================================================
        // PREPARAR UN ITEM: cargar datos → XML → firmar
        // Retorna null si hay excepción no controlada.
        // Retorna un ItemPreparado con ClaveAcceso vacío si hay error de negocio
        // (en ese caso FormaPago contiene el mensaje de error).
        // ================================================================
        private ItemPreparado PrepararUnItem(
            SolicitudItemLote solicitud,
            DataSet em,
            DataSet pfm,
            DataSet pm,
            string usuarioActual,
            string ipActual
        )
        {
            try
            {
                // ── Cargar detalle de la factura ──────────────────────────
                var dsFactura = _services.Facturacion.ConsultarFacturaNormal(
                    solicitud.Fecha,
                    solicitud.Hora,
                    solicitud.Cedula,
                    solicitud.NumeroViejo
                );

                if (dsFactura.Tables[0].Rows.Count == 0)
                    return ErrorItem(solicitud, "Factura sin detalle en BD.");

                DataTable detalle = new DataTable();
                detalle.Columns.Add("CANTIDAD");
                detalle.Columns.Add("PRODUCTO");
                detalle.Columns.Add("VALOR");
                detalle.Columns.Add("TOTAL");

                string formaPago = "";
                decimal totalFactura = 0m;

                foreach (DataRow r in dsFactura.Tables[0].Rows)
                {
                    detalle.Rows.Add(
                        r["CANTIDAD"].ToString(),
                        r["PRODUCTO"].ToString(),
                        r["TOTAL"].ToString(),
                        r["TOTAL"].ToString()
                    );
                    formaPago = r["FORMADEPAGO"].ToString();
                    if (decimal.TryParse(r["TOTAL"].ToString(), out decimal t))
                        totalFactura += t;
                }

                // ── Cargar cliente ────────────────────────────────────────
                var dsCli = _services.Cliente.ConsultarCedula(solicitud.Cedula);
                if (dsCli.Tables[0].Rows.Count == 0)
                    return ErrorItem(solicitud, "Cliente no encontrado: " + solicitud.Cedula);

                DataRow rowCliente = dsCli.Tables[0].Rows[0];

                // ── Preparar factura (obtiene el nuevo número secuencial) ──
                var prep = _services.ProcesosFacturacion.PrepararFactura(
                    detalle,
                    solicitud.Cedula,
                    formaPago,
                    "SI",
                    DateTime.Now.ToString("yyyy/MM/dd"),
                    DateTime.Now.ToString("HH:mm:ss")
                );

                if (!prep.Exito)
                    return ErrorItem(solicitud, "PrepararFactura: " + prep.Mensaje);

                if (!prep.EsElectronica)
                    return ErrorItem(solicitud, "El documento no es electrónico.");

                // ── Generar XML ───────────────────────────────────────────
                var factura = _services.ProcesosFacturacion.GenerarXmlFactura(
                    em, pfm, pm,
                    rowCliente,
                    prep.DetalleOriginal,
                    prep.NumeroFactura,
                    DateTime.Now.ToString("yyyy/MM/dd"),
                    totalFactura.ToString("0.00"),
                    formaPago,
                    "Gracias por su compra!"
                );

                if (factura == null)
                    return ErrorItem(solicitud, "Error generando XML.");

                // ── Firmar ────────────────────────────────────────────────
                var firma = _services.ProcesosFacturacion.GuardarYFirmarFacturaXml(
                    factura,
                    em.Tables[0].Rows[0]
                );

                if (!firma.Exito)
                    return ErrorItem(solicitud, "Error firmando: " + firma.Mensaje);

                return new ItemPreparado
                {
                    Solicitud      = solicitud,
                    NumeroFactura  = prep.NumeroFactura,
                    ClaveAcceso    = firma.ClaveAcceso,
                    RutaXmlFirmado = firma.RutaXmlFirmado,
                    RowCliente     = rowCliente,
                    Detalle        = prep.DetalleOriginal,
                    FechaPago      = DateTime.Now.ToString("yyyy/MM/dd"),
                    FormaPago      = formaPago,
                    ClienteTexto   = solicitud.Cedula,
                    TotalVisual    = totalFactura.ToString("0.00"),
                    Em             = em,
                    Pfm            = pfm
                };
            }
            catch (Exception ex)
            {
                return ErrorItem(solicitud, "Excepción preparando documento: " + ex.Message);
            }
        }

        // ================================================================
        // FINALIZAR UN ITEM: autorizar → insertar → PDF → correo
        // ================================================================
        private ResultadoItemLote FinalizarItem(
            ItemPreparado item,
            DataSet em,
            string usuarioActual,
            string ipActual
        )
        {
            var r = new ResultadoItemLote
            {
                NumeroViejo = item.Solicitud.NumeroViejo,
                NumeroNuevo = item.NumeroFactura
            };

            try
            {
                // ── Consultar autorización SRI ────────────────────────────
                var sri = _services.ProcesosGenerales.ConsultarAutorizacion(
                    item.RutaXmlFirmado,
                    item.ClaveAcceso,
                    "FACTURA",
                    item.NumeroFactura,
                    Path.Combine(_services.Paths.Facturas, "XMLAUTORIZADOS"),
                    1,
                    (i, m) => { },
                    (i, m) => { },
                    usuarioActual,
                    ipActual
                );

                if (!sri.Exito)
                {
                    // Pendiente de autorización — insertar con número real para que
                    // quede rastreable
                    try
                    {
                        _services.ProcesosFacturacion.InsertarFactura(
                            item.Detalle,
                            item.FechaPago,
                            DateTime.Now.ToString("HH:mm:ss"),
                            item.FormaPago,
                            item.ClienteTexto,
                            item.NumeroFactura,
                            usuarioActual,
                            ipActual
                        );

                        string estadoAut = _services.Pendientes
                            .ObtenerEstadoPendienteAutorizacionPorTipo("FACTURA");

                        _services.Pendientes.Insertar(
                            item.NumeroFactura,
                            item.ClaveAcceso,
                            item.RutaXmlFirmado,
                            DateTime.Now,
                            1,
                            estadoAut,
                            "FACTURA",
                            usuarioActual,
                            ipActual
                        );
                    }
                    catch { }

                    r.Exito     = true;
                    r.Autorizado = false;
                    r.Mensaje   = "Enviado al SRI. Pendiente de autorización: " + item.NumeroFactura;
                    return r;
                }

                // ── Insertar factura definitiva en FACTURACION ────────────
                _services.ProcesosFacturacion.InsertarFactura(
                    item.Detalle,
                    item.FechaPago,
                    DateTime.Now.ToString("HH:mm:ss"),
                    item.FormaPago,
                    item.ClienteTexto,
                    item.NumeroFactura,
                    usuarioActual,
                    ipActual
                );

                // ── Generar PDF ───────────────────────────────────────────
                var pdf = _services.ProcesosGenerales.GenerarPdfDesdeXmlAutorizado(
                    sri.XmlAutorizado,
                    em.Tables[0].Rows[0],
                    Path.Combine(_services.Paths.Facturas, "PDF"),
                    (gen, carpeta, xml, logo) =>
                        gen.GeneracionRideFacturaSRI(carpeta, xml, logo)
                );

                if (!pdf.Exito)
                {
                    r.Exito      = false;
                    r.Autorizado = true;
                    r.Mensaje    = "AUTORIZADO pero error generando PDF: " + pdf.Mensaje;
                    return r;
                }

                // ── Enviar correo ─────────────────────────────────────────
                var solicitudCorreo = new SolicitudCorreoDocumento
                {
                    TipoDocumento   = "FACTURA",
                    NumeroDocumento = item.NumeroFactura,
                    RutaPdf         = pdf.RutaPdf,
                    RutaXmlAutorizado = sri.RutaXmlAutorizado
                };

                var correo = _services.ProcesosGenerales.EnviarCorreoDocumento(
                    solicitudCorreo,
                    item.RowCliente,
                    "FACTURA",
                    usuarioActual,
                    ipActual
                );

                r.Exito              = true;
                r.Autorizado         = true;
                r.EnvioCorreoExitoso = correo.Exito;
                r.Mensaje            = correo.Exito
                    ? "Autorizada y correo enviado: " + item.NumeroFactura
                    : "AUTORIZADA. Correo pendiente: " + item.NumeroFactura;

                return r;
            }
            catch (Exception ex)
            {
                r.Exito   = false;
                r.Mensaje = "Error finalizando " + item.NumeroFactura + ": " + ex.Message;
                return r;
            }
        }

        // ── Helper ────────────────────────────────────────────────────
        private static ItemPreparado ErrorItem(SolicitudItemLote solicitud, string mensaje)
        {
            // ClaveAcceso vacío indica error; FormaPago se reutiliza como portador del mensaje
            return new ItemPreparado
            {
                Solicitud   = solicitud,
                ClaveAcceso = "",
                FormaPago   = mensaje
            };
        }
    }

    // ── DTO interno para la respuesta de recepción del lote ──────────
    internal class RespuestaRecepcionSriLote
    {
        public bool   Recibida { get; set; }
        public string Error    { get; set; }
    }
}
