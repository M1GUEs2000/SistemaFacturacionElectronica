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
    public class ServicioProductos : IServicioProductos
    {
        private readonly AppServices _services;

        public ServicioProductos(
            AppServices services
        )
        {
            _services = services;
        }
        // ======================================================
        // MOSTRAR ACTIVOS (ORDEN < 9001)
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoProducto>>> MostrarActivosAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Producto.MostrarProductos());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoProducto>>
                        .Ok(new List<DtoProducto>());

                var lista = MapperProducto.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoProducto>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoProducto>>
                    .Fail("Error obteniendo productos: " + ex.Message);
            }
        }

        // ======================================================
        // MOSTRAR ORDEN >= 9001
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoProducto>>> MostrarOrdenAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Producto.MostrarProductosOrden());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoProducto>>
                        .Ok(new List<DtoProducto>());

                var lista = MapperProducto.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoProducto>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoProducto>>
                    .Fail("Error obteniendo productos ordenados: " + ex.Message);
            }
        }

        // ======================================================
        // MOSTRAR TODOS + FILTRO
        // ======================================================

        public async Task<RespuestaGeneral<List<DtoProducto>>> MostrarTodoAsync(string nombre)
        {
            try
            {
                var ds = await Task.Run(() => _services.Producto.MostrarTodoProductos(nombre));

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoProducto>>
                        .Ok(new List<DtoProducto>());

                var lista = MapperProducto.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoProducto>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoProducto>>
                    .Fail("Error obteniendo productos: " + ex.Message);
            }
        }

        // ======================================================
        // MOSTRAR ESTADOS
        // ======================================================

        public async Task<RespuestaGeneral<List<string>>> MostrarEstadosAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Producto.MostrarEstados());

                var lista = new List<string>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        lista.Add(row["ESTATUS"]?.ToString());
                    }
                }

                return RespuestaGeneral<List<string>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<string>>
                    .Fail("Error obteniendo estados: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAR POR NOMBRE
        // ======================================================

        public async Task<RespuestaGeneral<DtoProducto>> ConsultarPorNombreAsync(string nombre)
        {
            try
            {
                var ds = await Task.Run(() => _services.Producto.ConsultaNombre(nombre));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoProducto>
                        .Fail("Producto no encontrado.");

                var dto = MapperProducto.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoProducto>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoProducto>
                    .Fail("Error consultando producto: " + ex.Message);
            }
        }

        // ======================================================
        // INSERTAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudProducto s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Producto.Insertar(
                        s.Nombre,
                        s.Valor,
                        s.Estatus,
                        s.Orden,
                        s.Codigo,
                        s.Iva,
                        s.AplicaIce     ?? "",
                        s.IceCodigo     ?? "",
                        s.IceTipo       ?? "",
                        s.IcePorcentaje ?? "",
                        s.IceValor      ?? "",
                        s.IceUnidad     ?? "",
                        s.AzucarGramos  ?? "",
                        s.Volumen       ?? "",
                        s.Pvp           ?? "",
                        s.TipoVehiculo  ?? "",
                        s.Usuario,
                        s.Ip
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Producto insertado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo insertar el producto.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando producto: " + ex.Message);
            }
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudProducto s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Producto.Actualizar(
                        s.Nombre,
                        s.Valor,
                        s.Estatus,
                        s.Orden,
                        s.Codigo,
                        s.Iva,
                        s.AplicaIce     ?? "",
                        s.IceCodigo     ?? "",
                        s.IceTipo       ?? "",
                        s.IcePorcentaje ?? "",
                        s.IceValor      ?? "",
                        s.IceUnidad     ?? "",
                        s.AzucarGramos  ?? "",
                        s.Volumen       ?? "",
                        s.Pvp           ?? "",
                        s.TipoVehiculo  ?? "",
                        s.Usuario,
                        s.Ip
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Producto actualizado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar el producto.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando producto: " + ex.Message);
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
                    _services.Producto.Eliminar(nombre, usuario, ip));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Producto eliminado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar el producto.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando producto: " + ex.Message);
            }
        }
    }
}