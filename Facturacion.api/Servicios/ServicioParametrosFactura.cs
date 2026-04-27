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
    public class ServicioParametrosFacturas : IServicioParametrosFacturas
    {
        private readonly AppServices _services;

        public ServicioParametrosFacturas(
            AppServices services
        )
        {
            _services = services;
        }
        // ======================================================
        // MOSTRAR
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoParametrosFactura>>> MostrarAsync()
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.ParamFactura.Mostrar());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoParametrosFactura>>
                        .Ok(new List<DtoParametrosFactura>());

                var lista = MapperParametrosFactura.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoParametrosFactura>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoParametrosFactura>>
                    .Fail("Error obteniendo parámetros: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAR POR NOMBRE
        // ======================================================

        public async Task<RespuestaGeneral<DtoParametrosFactura>> ConsultarPorNombreAsync(string nombre)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.ParamFactura.ConsultarNombre(nombre));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoParametrosFactura>
                        .Fail("Parámetros no encontrados.");

                var dto = MapperParametrosFactura.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoParametrosFactura>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoParametrosFactura>
                    .Fail("Error consultando parámetros: " + ex.Message);
            }
        }

        // ======================================================
        // VALIDACIONES
        // ======================================================

        public async Task<RespuestaGeneral<bool>> EsProduccionAsync(string nombre)
        {
            try
            {
                bool esProd = await Task.Run(() =>
                    _services.ParamFactura.EsProduccion(nombre));

                return RespuestaGeneral<bool>.Ok(esProd);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<bool>
                    .Fail("Error verificando ambiente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> CambiarAProduccionAsync(string nombre)
        {
            try
            {
                await Task.Run(() =>
                    _services.ParamFactura.CambiarAProduccion(nombre,nombre, nombre));

                return RespuestaGeneral<string>.Ok("Ambiente cambiado a producción.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error cambiando ambiente: " + ex.Message);
            }
        }

        // ======================================================
        // INSERTAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudParametrosFactura s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.ParamFactura.Insertar(
                        s.Nombre,
                        s.Ambiente,
                        s.TipoEmision,
                        s.AgenteRetencion,
                        s.ContribuyenteRimpe,
                        s.CodDoc,
                        s.Estab,
                        s.PuntoEmision,
                        s.NumeroDigitos,
                        s.ContribuyenteEspecial,
                        s.ObligadoContabilidad,
                        s.TipoIdentComprador,
                        s.Moneda,
                        s.CodigoImpuesto,
                        s.CodigoPorcentaje,
                        s.FechaActualizacion,
                        s.SmtpServer,
                        s.SmtpPort,
                        s.SmtpUser,
                        s.SmtpPass,
                        s.Usuario,
                        s.Ip
                    )
                );

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Parámetros insertados correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudieron insertar los parámetros.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando parámetros: " + ex.Message);
            }
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudParametrosFactura s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.ParamFactura.Actualizar(
                        s.Nombre,
                        s.Ambiente,
                        s.TipoEmision,
                        s.AgenteRetencion,
                        s.ContribuyenteRimpe,
                        s.CodDoc,
                        s.Estab,
                        s.PuntoEmision,
                        s.NumeroDigitos,
                        s.ContribuyenteEspecial,
                        s.ObligadoContabilidad,
                        s.TipoIdentComprador,
                        s.Moneda,
                        s.CodigoImpuesto,
                        s.CodigoPorcentaje,
                        s.FechaActualizacion,
                        s.SmtpServer,
                        s.SmtpPort,
                        s.SmtpUser,
                        s.SmtpPass,
                        s.Usuario,
                        s.Ip
                    )
                );

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Parámetros actualizados correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudieron actualizar los parámetros.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando parámetros: " + ex.Message);
            }
        }

        // ======================================================
        // ELIMINAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> EliminarAsync(string nombre, string usuario, string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.ParamFactura.Eliminar(nombre, usuario, ip));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Parámetros eliminados correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudieron eliminar los parámetros.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando parámetros: " + ex.Message);
            }
        }
    }
}