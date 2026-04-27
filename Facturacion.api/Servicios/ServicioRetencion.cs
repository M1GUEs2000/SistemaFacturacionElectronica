using Facturacion.api.Models.Respuestas;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Servicios.Interfaces;
using LogicaNegocios.Services;
using System.Data;
using static LogicaNegocios.Procesos.ProcesosRetenciones;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Facturacion.api.Servicios
{
    public class ServicioRetencion : IServicioRetencion
    {
        private readonly AppServices _services;

        public ServicioRetencion(
            AppServices services
        )
        {
            _services = services;
        }
        public async Task<RespuestaRetencion> CrearRetencionAsync(
            SolicitudCrearRetencion solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (solicitud.Conceptos == null ||
                    solicitud.Conceptos.Count == 0)
                    return Error("No existen conceptos de retención.");

                // ==========================================
                // 1) Convertir a DTO backend
                // ==========================================
                var dto = new DtoRetencionManual
                {
                    TipoIdentificacion = solicitud.TipoIdentificacion,
                    Identificacion = solicitud.Identificacion,
                    RazonSocial = solicitud.RazonSocial,
                    Direccion = solicitud.Direccion,
                    Telefono = solicitud.Telefono,
                    Correo = solicitud.Correo,
                    TipoPersona = solicitud.TipoPersona,
                    EsRimpe = solicitud.EsRimpe,
                    TipoRimpe = solicitud.TipoRimpe,
                    EsProfesional = solicitud.EsProfesional,
                    EsArrendador = solicitud.EsArrendador,
                    NumeroFactura = solicitud.NumeroFactura,
                    NumeroRetencion = solicitud.NumeroRetencion,
                    FechaFactura = solicitud.FechaFactura,
                    BaseImponible = solicitud.BaseImponible,
                    Iva = solicitud.Iva,
                    Total = solicitud.Total,
                    TipoOperacion = solicitud.TipoOperacion
                };

                DataTable conceptos = ConvertirConceptos(solicitud.Conceptos);

                // ==========================================
                // 2) Ejecutar backend real
                // ==========================================
                var resultado = await _services.ProcesosRetenciones.ProcesarRetencionElectronicaCompleta(
                        dto,
                        conceptos,
                        solicitud.Usuario,
                        solicitud.Ip
                    );

                return new RespuestaRetencion
                {
                    Exito = resultado.Exito,
                    Mensaje = resultado.Mensaje,
                    NumeroRetencion = resultado.NumeroRetencion,
                    NumeroFactura = resultado.NumeroFactura,
                    ClaveAcceso = resultado.ClaveAcceso,
                    RutaXmlGenerado = resultado.RutaXmlGenerado,
                    RutaXmlFirmado = resultado.RutaXmlFirmado,
                    RutaXmlAutorizado = resultado.RutaXmlAutorizado,
                    RutaPdf = resultado.RutaPdf,
                    EnvioCorreoExitoso = resultado.EnvioCorreoExitoso,
                    SecuencialRepetido = resultado.SecuencialRepetido,
                    Autorizado = resultado.Autorizado
                };
            }
            catch (Exception ex)
            {
                return Error("Error interno API: " + ex.Message);
            }
        }

        public async Task<RespuestaRetencion> ProcesarRetencionDesdeAutorizacionAsync(
            SolicitudProcesarPendienteDocumento solicitud
        )
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.NumeroDocumento))
                    return Error("Número de retención inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Fecha))
                    return Error("Fecha inválida.");

                if (string.IsNullOrWhiteSpace(solicitud.Usuario))
                    return Error("Usuario inválido.");

                if (string.IsNullOrWhiteSpace(solicitud.Ip))
                    return Error("IP inválida.");

                var resultado = await Task.Run(() =>
                    _services.ProcesosRetenciones.RetencionPendienteAutorizacionDesdeApi(
                        solicitud.NumeroDocumento,
                        solicitud.Fecha,
                        solicitud.Usuario,
                        solicitud.Ip,
                        solicitud.Proceso
                    )
                );

                return new RespuestaRetencion
                {
                    Exito = resultado.Exito,
                    Mensaje = resultado.Mensaje,
                    NumeroRetencion = resultado.NumeroRetencion,
                    ClaveAcceso = resultado.ClaveAcceso,
                    RutaXmlAutorizado = resultado.RutaXmlAutorizado,
                    RutaPdf = resultado.RutaPdf,
                    EnvioCorreoExitoso = resultado.EnvioCorreoExitoso,
                    Autorizado = resultado.Autorizado
                };
            }
            catch (Exception ex)
            {
                return Error("Error procesando retención desde autorización: " + ex.Message);
            }
        }

        private DataTable ConvertirConceptos(
            List<ConceptoRetencionSolicitud> conceptos
        )
        {
            var dt = new DataTable();

            dt.Columns.Add("TIPOIMPUESTO", typeof(string));
            dt.Columns.Add("CODIGOIMPUESTO", typeof(string));
            dt.Columns.Add("BASEIMPONIBLE", typeof(decimal));
            dt.Columns.Add("PORCENTAJERETENCION", typeof(decimal));
            dt.Columns.Add("VALORRETENIDO", typeof(decimal));

            foreach (var item in conceptos)
            {
                dt.Rows.Add(
                    item.TipoImpuesto ?? "",
                    item.Codigo ?? "",
                    item.BaseImponible,
                    item.Porcentaje,
                    item.ValorRetenido
                );
            }

            return dt;
        }

        public async Task<RespuestaRetencion> PreviewRetencionAsync(
    SolicitudCrearRetencion solicitud
)
        {
            try
            {
                if (solicitud == null)
                    return Error("Solicitud inválida.");

                if (solicitud.Conceptos == null || solicitud.Conceptos.Count == 0)
                    return Error("No existen conceptos de retención.");

                // ==========================================
                // Convertir DTO backend
                // ==========================================
                var dto = new DtoRetencionManual
                {
                    TipoIdentificacion = solicitud.TipoIdentificacion,
                    Identificacion = solicitud.Identificacion,
                    RazonSocial = solicitud.RazonSocial,
                    Direccion = solicitud.Direccion,
                    Telefono = solicitud.Telefono,
                    Correo = solicitud.Correo,
                    TipoPersona = solicitud.TipoPersona,
                    EsRimpe = solicitud.EsRimpe,
                    TipoRimpe = solicitud.TipoRimpe,
                    EsProfesional = solicitud.EsProfesional,
                    EsArrendador = solicitud.EsArrendador,
                    NumeroFactura = solicitud.NumeroFactura,
                    NumeroRetencion = solicitud.NumeroRetencion,
                    FechaFactura = solicitud.FechaFactura,
                    BaseImponible = solicitud.BaseImponible,
                    Iva = solicitud.Iva,
                    Total = solicitud.Total,
                    TipoOperacion = solicitud.TipoOperacion
                };

                DataTable conceptos = ConvertirConceptos(solicitud.Conceptos);

                var resultado = await _services.ProcesosRetenciones.ProcesarRetencionPreviewAsync(
                        dto,
                        conceptos,
                        solicitud.Usuario
                    );

                return new RespuestaRetencion
                {
                    Exito = resultado.Exito,
                    RutaPdf = resultado.RutaPdf,
                    Mensaje = resultado.Mensaje
                };
            }
            catch (Exception ex)
            {
                return Error("Error generando preview de retención: " + ex.Message);
            }
        }

        private RespuestaRetencion Error(string mensaje)
        {
            return new RespuestaRetencion
            {
                Exito = false,
                Mensaje = mensaje
            };
        }
    }
}