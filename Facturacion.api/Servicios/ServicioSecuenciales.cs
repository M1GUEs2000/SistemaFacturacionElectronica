using Facturacion.api.Mappers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.DTOs;
using Facturacion.api.Models.Respuestas;
using LogicaNegocios.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Facturacion.api.Servicios
{
    public class ServicioSecuenciales : IServicioSecuenciales
    {
        private readonly AppServices _services;

        public ServicioSecuenciales(
            AppServices services
        )
        {
            _services = services;
        }
        // ======================================================
        // MOSTRAR TODOS
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoSecuencial>>> MostrarAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Param.Mostrar());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoSecuencial>>
                        .Ok(new List<DtoSecuencial>());

                int digitos = _services.Param.ObtenerNumeroDigitosFactura();

                var lista = MapperSecuencial.ToList(ds.Tables[0], digitos);

                return RespuestaGeneral<List<DtoSecuencial>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoSecuencial>>
                    .Fail("Error obteniendo secuenciales: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAR POR TIPO
        // ======================================================

        public async Task<RespuestaGeneral<DtoSecuencial>> ConsultarPorTipoAsync(string tipoComprobante)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Param.ConsultarPorTipo(tipoComprobante));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoSecuencial>
                        .Fail("No existe secuencial para el tipo indicado.");

                int digitos = _services.Param.ObtenerNumeroDigitosFactura();

                var dto = MapperSecuencial.ToDto(ds.Tables[0].Rows[0], digitos);

                return RespuestaGeneral<DtoSecuencial>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoSecuencial>
                    .Fail("Error consultando secuencial: " + ex.Message);
            }
        }

        // ======================================================
        // OBTENER SECUENCIAL REAL
        // ======================================================

        public async Task<RespuestaGeneral<long>> ObtenerSecuencialRealAsync(string tipoComprobante)
        {
            try
            {
                long sec = await Task.Run(() =>
                    _services.Param.ObtenerSecuencialReal(tipoComprobante));

                return RespuestaGeneral<long>.Ok(sec);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<long>
                    .Fail("Error obteniendo secuencial real: " + ex.Message);
            }
        }

        // ======================================================
        // OBTENER SECUENCIAL FORMATEADO
        // ======================================================

        public async Task<RespuestaGeneral<string>> ObtenerSecuencialFormateadoAsync(string tipoComprobante)
        {
            try
            {
                string sec = await Task.Run(() =>
                    _services.Param.ObtenerSecuencialFormateado(tipoComprobante));

                return RespuestaGeneral<string>.Ok(sec);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error obteniendo secuencial formateado: " + ex.Message);
            }
        }

        // ======================================================
        // OBTENER CODIGO NUMERICO
        // ======================================================

        public async Task<RespuestaGeneral<string>> ObtenerCodigoNumericoAsync(string tipoComprobante)
        {
            try
            {
                string codigo = await Task.Run(() =>
                    _services.Param.ObtenerCodigoNumerico(tipoComprobante));

                return RespuestaGeneral<string>.Ok(codigo);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error obteniendo código numérico: " + ex.Message);
            }
        }

        // ======================================================
        // INSERTAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudSecuencial s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Param.Insertar(
                        s.TipoComprobante,
                        s.Secuencial,
                        s.CodigoNumerico,
                        s.FechaActualizacion,
                        s.Usuario,
                        s.Ip
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Secuencial insertado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo insertar el secuencial.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando secuencial: " + ex.Message);
            }
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudSecuencial s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Param.Actualizar(
                        s.TipoComprobante,
                        s.Secuencial,
                        s.CodigoNumerico,
                        s.FechaActualizacion,
                        s.Usuario,
                        s.Ip
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Secuencial actualizado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar el secuencial.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando secuencial: " + ex.Message);
            }
        }

        // ======================================================
        // INCREMENTAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> IncrementarAsync(string tipoComprobante)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Param.IncrementarSecuencial(tipoComprobante));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Secuencial incrementado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo incrementar el secuencial.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error incrementando secuencial: " + ex.Message);
            }
        }

        // ======================================================
        // ELIMINAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> EliminarAsync(string tipoComprobante, string usuario, string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Param.EliminarPorTipo(tipoComprobante, usuario, ip));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Secuencial eliminado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar el secuencial.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando secuencial: " + ex.Message);
            }
        }
    }
}