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
    public class ServicioFormaPago : IServicioFormaPago
    {
        private readonly AppServices _services;

        public ServicioFormaPago(
            AppServices services
        )
        {
            _services = services;
        }

        // ==========================================================
        // MOSTRAR
        // ==========================================================

        public async Task<RespuestaGeneral<List<DtoFormaPago>>> MostrarAsync(string filtro)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.FormaPago.MostrarFormaPago(filtro ?? ""));

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoFormaPago>>
                        .Ok(new List<DtoFormaPago>());

                var lista = MapperFormaPago.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoFormaPago>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoFormaPago>>
                    .Fail("Error obteniendo formas de pago: " + ex.Message);
            }
        }

        // ==========================================================
        // CONSULTAR UNA
        // ==========================================================

        public async Task<RespuestaGeneral<DtoFormaPago>> ConsultarAsync(string formas)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.FormaPago.ConsultaFormas(formas));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoFormaPago>
                        .Fail("Forma de pago no encontrada.");

                var dto = MapperFormaPago.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoFormaPago>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoFormaPago>
                    .Fail("Error consultando forma de pago: " + ex.Message);
            }
        }

        // ==========================================================
        // CONSULTAR TOTALES
        // ==========================================================

        public async Task<RespuestaGeneral<List<DtoTotalesFormaPago>>> ConsultarTotalesAsync(string fecha)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.FormaPago.ConsultarTotales(fecha));

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoTotalesFormaPago>>
                        .Ok(new List<DtoTotalesFormaPago>());

                var lista = MapperFormaPago.ToTotalesList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoTotalesFormaPago>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoTotalesFormaPago>>
                    .Fail("Error consultando totales: " + ex.Message);
            }
        }

        // ==========================================================
        // INSERTAR
        // ==========================================================

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudFormaPago solicitud)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.FormaPago.Insertar(
                        solicitud.Formas,
                        solicitud.Imagen,
                        solicitud.Codigo,
                        solicitud.Usuario,
                        solicitud.Ip
                    )
                );

                return resultado > 0
                    ? RespuestaGeneral<string>.Ok("Forma de pago insertada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo insertar la forma de pago.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando forma de pago: " + ex.Message);
            }
        }

        // ==========================================================
        // ACTUALIZAR
        // ==========================================================

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudFormaPago solicitud)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.FormaPago.Actualizar(
                        solicitud.Formas,
                        solicitud.Imagen,
                        solicitud.Codigo,
                        solicitud.Usuario,
                        solicitud.Ip
                    )
                );

                return resultado > 0
                    ? RespuestaGeneral<string>.Ok("Forma de pago actualizada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar la forma de pago.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando forma de pago: " + ex.Message);
            }
        }

        // ==========================================================
        // ELIMINAR
        // ==========================================================

        public async Task<RespuestaGeneral<string>> EliminarAsync(string formas, string usuario, string ip)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.FormaPago.Eliminar(formas, usuario, ip));

                return resultado > 0
                    ? RespuestaGeneral<string>.Ok("Forma de pago eliminada correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar la forma de pago.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando forma de pago: " + ex.Message);
            }
        }
    }
}