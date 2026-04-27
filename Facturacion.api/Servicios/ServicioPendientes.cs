using Facturacion.api.Mappers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using LogicaNegocios.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Facturacion.api.Servicios
{
    public class ServicioPendientes : IServicioPendientes
    {
        private readonly AppServices _services;

        public ServicioPendientes(
            AppServices services
        )
        {
            _services = services;
        }
        // ==========================================================
        // LISTADOS
        // ==========================================================

        public async Task<RespuestaGeneral<List<DtoPendiente>>> MostrarTodosAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Pendientes.Mostrar());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoPendiente>>.Ok(new List<DtoPendiente>());

                var lista = MapperPendiente.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoPendiente>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoPendiente>>
                    .Fail("Error obteniendo pendientes: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<DtoPendiente>> ConsultarAsync(string numeroDocumento)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Pendientes.Consultar(numeroDocumento));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoPendiente>.Fail("Documento no encontrado.");

                var dto = MapperPendiente.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoPendiente>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoPendiente>
                    .Fail("Error consultando pendiente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<DtoPendiente>> ConsultarPorNumeroYTipoAsync(string numeroDocumento, string tipo)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Pendientes.ConsultarPorNumeroYTipo(numeroDocumento, tipo));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoPendiente>.Fail("Documento no encontrado.");

                var dto = MapperPendiente.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoPendiente>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoPendiente>
                    .Fail("Error consultando pendiente: " + ex.Message);
            }
        }

        // ==========================================================
        // COMANDOS
        // ==========================================================

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudPendiente solicitud)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.Pendientes.Insertar(
                        solicitud.NumeroDocumento,
                        solicitud.ClaveAcceso,
                        solicitud.RutaXmlFirmado,
                        solicitud.FechaRegistro,
                        solicitud.Intentos,
                        solicitud.Estado,
                        solicitud.Tipo,
                        solicitud.Usuario,
                        solicitud.Ip
                    )
                );

                return resultado > 0
                    ? RespuestaGeneral<string>.Ok("Pendiente registrado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo registrar el pendiente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando pendiente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ActualizarEstadoAsync(
            string numeroDocumento, string estado, string tipo, string usuario, string ip)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.Pendientes.ActualizarEstado(
                        numeroDocumento, estado, tipo, usuario, ip));

                return resultado > 0
                    ? RespuestaGeneral<string>.Ok("Estado actualizado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar el estado.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando estado: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> EliminarAsync(
            string numeroDocumento, string tipo, string usuario, string ip)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.Pendientes.Eliminar(
                        numeroDocumento, tipo, usuario, ip));

                return resultado > 0
                    ? RespuestaGeneral<string>.Ok("Pendiente eliminado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar el pendiente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando pendiente: " + ex.Message);
            }
        }

        // ==========================================================
        // GENERADORES DE ESTADO
        // ==========================================================

        public async Task<RespuestaGeneral<string>> ObtenerEstadoPendienteAsync(string tipo)
        {
            try
            {
                var estado = await Task.Run(() =>
                    _services.Pendientes.ObtenerEstadoPendientePorTipo(tipo));

                return RespuestaGeneral<string>.Ok(estado);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error generando estado pendiente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ObtenerEstadoPendienteAutorizacionAsync(string tipo)
        {
            try
            {
                var estado = await Task.Run(() =>
                    _services.Pendientes.ObtenerEstadoPendienteAutorizacionPorTipo(tipo));

                return RespuestaGeneral<string>.Ok(estado);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error generando estado autorización: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ObtenerEstadoPendienteCorreoAsync(string tipo)
        {
            try
            {
                var estado = await Task.Run(() =>
                    _services.Pendientes.ObtenerEstadoPendienteCorreoPorTipo(tipo));

                return RespuestaGeneral<string>.Ok(estado);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error generando estado correo: " + ex.Message);
            }
        }

        // ==========================================================
        // ACCION DOCUMENTO
        // ==========================================================

        public async Task<RespuestaGeneral<DtoAccionPendienteDocumento>>
            ConsultarAccionDocumentoAsync(string numeroDocumento, string tipoDocumento)
        {
            try
            {
                var accion = await Task.Run(() =>
                    _services.Pendientes
                        .ConsultarAccionPendienteDocumento(numeroDocumento, tipoDocumento));

                var dto = new DtoAccionPendienteDocumento
                {
                    Existe = accion.Existe,
                    Tipo = accion.Tipo,
                    NumeroDocumento = accion.NumeroDocumento,
                    EstadoPendiente = accion.EstadoPendiente,
                    TextoBoton = accion.TextoBoton,
                    Accion = accion.Accion,
                    MostrarPdf = accion.MostrarPdf,
                    MostrarXml = accion.MostrarXml
                };

                return RespuestaGeneral<DtoAccionPendienteDocumento>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoAccionPendienteDocumento>
                    .Fail("Error consultando acción: " + ex.Message);
            }
        }
    }
}