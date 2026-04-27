using Facturacion.api.Mappers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using LogicaNegocios.Services;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Facturacion.api.Servicios
{
    public class ServicioRetenciones : IServicioRetenciones
    {
        private readonly AppServices _services;

        public ServicioRetenciones(
            AppServices services
        )
        {
            _services = services;
        }
        // ======================================================
        // INSERTAR (ENCABEZADO + DETALLE)
        // ======================================================
        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudRetenciones s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Retencion.Insertar(
                        s.NumeroRetencion,
                        s.ClaveAcceso,
                        s.FechaEmision,
                        s.HoraEmision,
                        s.Ambiente,
                        s.Estado,
                        s.Codigo,
                        s.TipoEmision,
                        s.NumeroFactura,
                        s.ClaveAccesoFactura,
                        s.FechaFactura,
                        s.SujetoRetenido,
                        s.IdentificacionSujeto,
                        s.TipoIdentificacionSujeto,
                        s.DireccionSujeto,
                        s.RegimenSujeto,
                        s.TotalBaseImponible,
                        s.TotalRetencionRenta,
                        s.TotalRetencionIva,
                        s.TotalRetenido,
                        s.Observaciones,
                        s.Usuario ?? "",
                        s.IP ?? ""
                    ));

                if (r <= 0)
                    return RespuestaGeneral<string>.Fail("No se pudo insertar la retención.");

                // Insertar detalles
                foreach (var det in s.Detalles)
                {
                    _services.Retencion.InsertarDetalle(
                        s.NumeroRetencion,
                        det.TipoImpuesto,
                        det.CodigoImpuesto,
                        det.BaseImponible,
                        det.PorcentajeRetencion,
                        det.ValorRetenido,
                        det.TipoOperacion
                    );
                }

                return RespuestaGeneral<string>.Ok("Retención insertada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando retención: " + ex.Message);
            }
        }

        // ======================================================
        // ACTUALIZAR ENCABEZADO
        // ======================================================
        public async Task<RespuestaGeneral<string>> ActualizarAsync(
            string numeroRetencion,
            string estado,
            string codigo,
            string claveAcceso,
            string ambiente,
            string tipoEmision,
            string fechaEmision,
            string horaEmision,
            string usuario,
            string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Retencion.Actualizar(
                        numeroRetencion,
                        estado,
                        codigo,
                        claveAcceso,
                        ambiente,
                        tipoEmision,
                        fechaEmision,
                        horaEmision,
                        usuario,
                        ip
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Retención actualizada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar la retención.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando retención: " + ex.Message);
            }
        }

        // ======================================================
        // ELIMINAR
        // ======================================================
        public async Task<RespuestaGeneral<string>> EliminarAsync(string numeroRetencion, string usuario, string ip)
        {
            try
            {
                await Task.Run(() =>
                {
                    _services.Retencion.EliminarDetalle(numeroRetencion);
                    _services.Retencion.Eliminar(numeroRetencion, usuario, ip);
                });

                return RespuestaGeneral<string>.Ok("Retención eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando retención: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAR (ENCABEZADO + DETALLE)
        // ======================================================
        public async Task<RespuestaGeneral<DtoRetencion>> ConsultarAsync(string numeroRetencion)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Retencion.ConsultarPorNumero(numeroRetencion));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoRetencion>.Fail("Retención no encontrada.");

                var dto = MapperRetencion.ToDto(ds.Tables[0].Rows[0]);

                // Cargar detalle
                var dsDet = _services.Retencion.ConsultarDetalle(numeroRetencion);

                if (dsDet != null && dsDet.Tables.Count > 0)
                {
                    foreach (DataRow row in dsDet.Tables[0].Rows)
                        dto.Detalles.Add(MapperRetencion.ToDetalle(row));
                }

                return RespuestaGeneral<DtoRetencion>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoRetencion>
                    .Fail("Error consultando retención: " + ex.Message);
            }
        }

        // ======================================================
        // LISTAR
        // ======================================================
        public async Task<RespuestaGeneral<List<DtoRetencion>>> ListarAsync(int top, string filtro)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Retencion.Listar(top, filtro));

                var lista = new List<DtoRetencion>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                        lista.Add(MapperRetencion.ToDto(row));
                }

                return RespuestaGeneral<List<DtoRetencion>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoRetencion>>
                    .Fail("Error listando retenciones: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTA AVANZADA
        // ======================================================
        public async Task<RespuestaGeneral<List<DtoRetencionConsulta>>> ConsultarAvanzadoAsync(
            string fechaDesde,
            string fechaHasta,
            string sujetoRetenido,
            string numeroRetencion,
            string numeroFactura)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Retencion.ConsultarAvanzado(
                        fechaDesde,
                        fechaHasta,
                        sujetoRetenido,
                        numeroRetencion,
                        numeroFactura
                    ));

                var lista = new List<DtoRetencionConsulta>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        lista.Add(new DtoRetencionConsulta
                        {
                            NumeroRetencion = row["NUMERORETENCION"]?.ToString(),
                            FechaEmision = row["FECHAEMISION"]?.ToString(),
                            NumeroFactura = row["NUMEROFACTURA"]?.ToString(),
                            FechaFactura = row["FECHAFACTURA"]?.ToString(),
                            SujetoRetenido = row["SUJETORETENIDO"]?.ToString(),
                            Identificacion = row["IDENTIFICACION"]?.ToString(),
                            TotalBaseImponible = row["TOTALBASEIMPONIBLE"]?.ToString(),
                            TotalRetencionRenta = row["TOTALRETENCIONRENTA"]?.ToString(),
                            TotalRetencionIva = row["TOTALRETENCIONIVA"]?.ToString(),
                            TotalRetenido = row["TOTALRETENIDO"]?.ToString()
                        });
                    }
                }

                return RespuestaGeneral<List<DtoRetencionConsulta>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoRetencionConsulta>>
                    .Fail("Error en consulta avanzada: " + ex.Message);
            }
        }
    }
}