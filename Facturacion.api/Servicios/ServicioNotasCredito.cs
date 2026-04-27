using Facturacion.api.Mappers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using LogicaNegocios.Services;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Facturacion.api.Servicios
{
    public class ServicioNotasCredito : IServicioNotasCredito
    {
        private readonly AppServices _services;

        public ServicioNotasCredito(
            AppServices services
        )
        {
            _services = services;
        }

        // ======================================================
        // CRUD ENCABEZADO
        // ======================================================

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudNotaCredito s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.NotaCredito.Insertar(
                        s.NumeroNota,
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
                        s.Motivo,
                        s.Cliente,
                        s.TotalSinImpuestos,
                        s.TotalConImpuestos,
                        s.CreditoUsado,
                        s.Usuario,
                        s.Ip
                    )
                );

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Nota de crédito registrada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo registrar la nota.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error insertando nota: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudNotaCredito s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.NotaCredito.Actualizar(
                        s.NumeroNota,
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
                        s.Motivo,
                        s.Cliente,
                        s.TotalSinImpuestos,
                        s.TotalConImpuestos,
                        s.Usuario,
                        s.Ip
                    )
                );

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Nota actualizada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar la nota.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error actualizando nota: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> EliminarAsync(string numeroNota, string usuario, string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.NotaCredito.Eliminar(numeroNota, usuario, ip));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Nota eliminada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar la nota.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error eliminando nota: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAS
        // ======================================================

        public async Task<RespuestaGeneral<DtoNotaCredito>> ConsultarPorNumeroAsync(string numeroNota)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.NotaCredito.ConsultarPorNumeroNota(numeroNota));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoNotaCredito>.Fail("Nota no encontrada.");

                var dto = MapperNotaCredito.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoNotaCredito>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoNotaCredito>.Fail("Error consultando nota: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<List<DtoNotaCredito>>> ListarAsync(int top, string filtro)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.NotaCredito.Listar(top, filtro ?? ""));

                var lista = MapperNotaCredito.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoNotaCredito>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoNotaCredito>>.Fail("Error listando notas: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAS TABLA
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorFechasAsync(string fd, string fh)
        {
            var ds = await Task.Run(() =>
                _services.NotaCredito.ConsultarPorFechas(fd, fh));

            return RespuestaGeneral<List<DtoNotaCreditoListado>>
                .Ok(MapperNotaCredito.ToListadoList(ds.Tables[0]));
        }

        public async Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorClienteAsync(string fd, string fh, string cedula)
        {
            var ds = await Task.Run(() =>
                _services.NotaCredito.ConsultarPorCliente(fd, fh, cedula));

            return RespuestaGeneral<List<DtoNotaCreditoListado>>
                .Ok(MapperNotaCredito.ToListadoList(ds.Tables[0]));
        }

        public async Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorNumeroNotaFechasAsync(string fd, string fh, string numeroNota)
        {
            var ds = await Task.Run(() =>
                _services.NotaCredito.ConsultarPorNumeroNotaFechas(fd, fh, numeroNota));

            return RespuestaGeneral<List<DtoNotaCreditoListado>>
                .Ok(MapperNotaCredito.ToListadoList(ds.Tables[0]));
        }

        public async Task<RespuestaGeneral<List<DtoNotaCreditoListado>>> ConsultarPorClienteYNumeroNotaAsync(string fd, string fh, string cedula, string numeroNota)
        {
            var ds = await Task.Run(() =>
                _services.NotaCredito.ConsultarPorClienteYNumeroNota(fd, fh, cedula, numeroNota));

            return RespuestaGeneral<List<DtoNotaCreditoListado>>
                .Ok(MapperNotaCredito.ToListadoList(ds.Tables[0]));
        }

        // ======================================================
        // DETALLE
        // ======================================================

        public async Task<RespuestaGeneral<string>> InsertarDetalleAsync(SolicitudNotaCreditoDetalle s)
        {
            int r = await Task.Run(() =>
                _services.NotaCredito.InsertarDetalle(
                    s.NumeroNota,
                    s.Producto,
                    s.Cantidad,
                    s.Precio,
                    s.Iva,
                    s.NumeroFactura
                )
            );

            return r > 0
                ? RespuestaGeneral<string>.Ok("Detalle insertado.")
                : RespuestaGeneral<string>.Fail("No se pudo insertar el detalle.");
        }

        public async Task<RespuestaGeneral<List<DtoNotaCreditoDetalle>>> ConsultarDetalleAsync(string numeroNota)
        {
            var ds = await Task.Run(() =>
                _services.NotaCredito.ConsultarDetallePorNumeroNota(numeroNota));

            return RespuestaGeneral<List<DtoNotaCreditoDetalle>>
                .Ok(MapperNotaCredito.ToDetalleList(ds.Tables[0]));
        }

        public async Task<RespuestaGeneral<string>> EliminarDetalleAsync(string numeroNota)
        {
            int r = await Task.Run(() =>
                _services.NotaCredito.EliminarDetallePorNumeroNota(numeroNota));

            return r > 0
                ? RespuestaGeneral<string>.Ok("Detalle eliminado.")
                : RespuestaGeneral<string>.Fail("No se pudo eliminar detalle.");
        }

        // ======================================================
        // UTILIDADES
        // ======================================================

        public async Task<RespuestaGeneral<string>> RenombrarNotaAsync(string vieja, string nueva)
        {
            await Task.Run(() =>
                _services.NotaCredito.ActualizarNumeroNota_EncabezadoYDetalle(vieja, nueva));

            return RespuestaGeneral<string>.Ok("Nota renombrada correctamente.");
        }

        public async Task<RespuestaGeneral<List<string>>> ListarFacturasUnicasAsync()
        {
            var ds = await Task.Run(() =>
                _services.NotaCredito.ListarFacturasUnicas());

            var lista = ds.Tables[0]
                .AsEnumerable()
                .Select(r => r["NUMEROFACTURA"].ToString())
                .ToList();

            return RespuestaGeneral<List<string>>.Ok(lista);
        }
    }
}