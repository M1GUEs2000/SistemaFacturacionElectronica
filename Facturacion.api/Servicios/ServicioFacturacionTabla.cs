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
    public class ServicioFacturacionTabla : IServicioFacturacionTabla
    {
        private readonly AppServices _services;

        public ServicioFacturacionTabla(
            AppServices services
        )
        {
            _services = services;
        }

        #region COMANDOS

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudFacturacion s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Facturacion.Insertar(
                        s.Fecha, s.FormaDePago, s.Producto,
                        s.Cantidad, s.Total, s.Cliente,
                        s.Hora, s.NumeroFactura,
                        s.Usuario, s.Ip
                    )
                );

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Factura registrada.")
                    : RespuestaGeneral<string>.Fail("No se pudo registrar.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail(ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> EliminarAsync(SolicitudFacturacion s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Facturacion.Eliminar(
                        s.Fecha, s.FormaDePago, s.Producto,
                        s.Cantidad, s.Total, s.Cliente,
                        s.Hora, s.NumeroFactura,
                        s.Usuario, s.Ip
                    )
                );

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Factura eliminada.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail(ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> EliminarPorSecuencialAsync(string numeroFactura, string usuario, string ip)
        {
            try
            {
                await Task.Run(() =>
                    _services.Facturacion.EliminarPorSecuencial(numeroFactura, usuario, ip)
                );

                return RespuestaGeneral<string>.Ok("Factura eliminada por secuencial.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail(ex.Message);
            }
        }

        #endregion

        #region CONSULTAS

        public async Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarPorNumeroAsync(string numeroFactura)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarPorNumeroFactura(numeroFactura));

            var lista = ds.Tables[0].AsEnumerable()
                .Select(r => new DtoFacturacion
                {
                    Fecha = r["FECHA"]?.ToString(),
                    Hora = r["HORA"]?.ToString(),
                    Cliente = r["CLIENTE"]?.ToString(),
                    Producto = r["PRODUCTO"]?.ToString(),
                    Cantidad = r["CANTIDAD"]?.ToString(),
                    Total = r["TOTAL"]?.ToString(),
                    NumeroFactura = r["NUMEROFACTURA"]?.ToString()
                }).ToList();

            return RespuestaGeneral<List<DtoFacturacion>>.Ok(lista);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarFacturaConsumidorFinalAsync(string fecha, string hora, string numeroFactura)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarFacturaConsumidorFinal(fecha, hora, numeroFactura));

            return MapearDetalle(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarFacturaNormalAsync(string fecha, string hora, string cedula, string numeroFactura)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarFacturaNormal(fecha, hora, cedula, numeroFactura));

            return MapearDetalle(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarTotalesAsync(string fecha)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarTotales(fecha));

            return MapearResumen(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarFechasAsync(string desde, string hasta)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarFechas(desde, hasta));

            return MapearResumen(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarFechasPorClienteAsync(string desde, string hasta, string cliente)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarFechasPorCliente(desde, hasta, cliente));

            return MapearResumen(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarPendientesAsync(string desde, string hasta)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarPendientesPorFecha(desde, hasta));

            return MapearResumen(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacionResumen>>> ConsultarConsumidorFinalPorFechaAsync(string desde, string hasta)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarConsumidorFinalPorFecha(desde, hasta));

            return MapearResumen(ds);
        }

        public async Task<RespuestaGeneral<List<DtoFacturacion>>> ConsultarFiltroAsync(string desde, string hasta, string producto, string cliente, string formaPago)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarFecha(desde, hasta, producto, cliente, formaPago));

            return MapearDetalle(ds);
        }

        #endregion

        #region COMBOS

        public async Task<RespuestaGeneral<List<DtoComboSimple>>> ConsultarClientesAsync()
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarCliente());

            return MapearCombo(ds, "NOMBRE", "CEDULA");
        }

        public async Task<RespuestaGeneral<List<DtoComboSimple>>> ConsultarProductosAsync()
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarProducto());

            return MapearCombo(ds, "PRODUCTO", "PRODUCTO");
        }

        public async Task<RespuestaGeneral<List<DtoComboSimple>>> ConsultarFormasPagoAsync()
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarFormaPago());

            return MapearCombo(ds, "FORMADEPAGO", "FORMADEPAGO");
        }

        #endregion

        #region SECUENCIALES

        public async Task<RespuestaGeneral<string>> ObtenerSecuencialPendienteAsync()
        {
            var sec = await Task.Run(() =>
                _services.Facturacion.ObtenerSecuencialError());

            return RespuestaGeneral<string>.Ok(sec);
        }

        public async Task<RespuestaGeneral<string>> ObtenerSecuencialConsumidorAsync()
        {
            var sec = await Task.Run(() =>
                _services.Facturacion.ObtenerSecuencialConsumidor());

            return RespuestaGeneral<string>.Ok(sec);
        }

        public async Task<RespuestaGeneral<List<DtoConsultaGeneral>>> ConsultarGeneralAsync(SolicitudConsultaGeneral s)
        {
            var ds = await Task.Run(() =>
                _services.Facturacion.ConsultarDocumentos(
                    s.TipoDocumento,
                    s.FechaDesde,
                    s.FechaHasta,
                    s.Cliente ?? "",
                    s.Producto ?? "",
                    s.FormaPago ?? "",
                    s.NumeroFactura ?? "",
                    s.NumeroRetencion ?? "",
                    s.NumeroNota ?? "",
                    s.SujetoRetenido ?? "",
                    s.Estado ?? "TODOS"
                ));

            var lista = MapperConsultaGeneral.ToList(ds.Tables[0], s.TipoDocumento);

            return RespuestaGeneral<List<DtoConsultaGeneral>>.Ok(lista);
        }

        #endregion

        #region MAPEOS PRIVADOS

        private RespuestaGeneral<List<DtoFacturacion>> MapearDetalle(DataSet ds)
        {
            var lista = ds.Tables[0].AsEnumerable()
                .Select(r => new DtoFacturacion
                {
                    Fecha = r["FECHA"]?.ToString(),
                    FormaDePago = r.Table.Columns.Contains("FORMADEPAGO") ? r["FORMADEPAGO"]?.ToString() : "",
                    Producto = r["PRODUCTO"]?.ToString(),
                    Cantidad = r["CANTIDAD"]?.ToString(),
                    Total = r["TOTAL"]?.ToString(),
                    Cliente = r["CLIENTE"]?.ToString(),
                    Hora = r["HORA"]?.ToString(),
                    NumeroFactura = r["NUMEROFACTURA"]?.ToString()
                }).ToList();

            return RespuestaGeneral<List<DtoFacturacion>>.Ok(lista);
        }

        private RespuestaGeneral<List<DtoFacturacionResumen>> MapearResumen(DataSet ds)
        {
            var lista = ds.Tables[0].AsEnumerable()
                .Select(r => new DtoFacturacionResumen
                {
                    Fecha = r["FECHA"]?.ToString(),
                    FormaDePago = r.Table.Columns.Contains("FORMA DE PAGO") ? r["FORMA DE PAGO"]?.ToString() : "",
                    Cantidades = r["CANTIDADES"]?.ToString(),
                    Totales = r["TOTALES"]?.ToString(),
                    Cliente = r.Table.Columns.Contains("CLIENTE") ? r["CLIENTE"]?.ToString() : "",
                    Cedula = r.Table.Columns.Contains("CEDULA") ? r["CEDULA"]?.ToString() : "",
                    Hora = r["HORA"]?.ToString(),
                    NumeroFactura = r["NUMEROFACTURA"]?.ToString()
                }).ToList();

            return RespuestaGeneral<List<DtoFacturacionResumen>>.Ok(lista);
        }

        private RespuestaGeneral<List<DtoComboSimple>> MapearCombo(DataSet ds, string textoCol, string valorCol)
        {
            var lista = ds.Tables[0].AsEnumerable()
                .Select(r => new DtoComboSimple
                {
                    Texto = r[textoCol]?.ToString(),
                    Valor = r[valorCol]?.ToString()
                }).ToList();

            return RespuestaGeneral<List<DtoComboSimple>>.Ok(lista);
        }

        #endregion
    }
}