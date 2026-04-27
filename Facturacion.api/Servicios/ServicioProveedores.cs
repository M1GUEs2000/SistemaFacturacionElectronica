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
    public class ServicioProveedores : IServicioProveedores
    {
        private readonly AppServices _services;

        public ServicioProveedores(
            AppServices services
        )
        {
            _services = services;
        }
        // ======================================================
        // LISTAR TODOS
        // ======================================================
        public async Task<RespuestaGeneral<List<DtoProveedor>>> ListarAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Proveedor.Listar());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoProveedor>>
                        .Ok(new List<DtoProveedor>());

                var lista = MapperProveedor.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoProveedor>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoProveedor>>
                    .Fail("Error obteniendo proveedores: " + ex.Message);
            }
        }

        // ======================================================
        // LISTAR ACTIVOS
        // ======================================================
        public async Task<RespuestaGeneral<List<DtoProveedor>>> ListarActivosAsync()
        {
            try
            {
                var ds = await Task.Run(() => _services.Proveedor.ListarActivos());

                if (ds == null || ds.Tables.Count == 0)
                    return RespuestaGeneral<List<DtoProveedor>>
                        .Ok(new List<DtoProveedor>());

                var lista = MapperProveedor.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoProveedor>>.Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoProveedor>>
                    .Fail("Error obteniendo proveedores activos: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAR POR ID
        // ======================================================
        public async Task<RespuestaGeneral<DtoProveedor>> ConsultarPorIdAsync(int idProveedor)
        {
            try
            {
                var ds = await Task.Run(() => _services.Proveedor.ConsultarPorId(idProveedor));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoProveedor>
                        .Fail("Proveedor no encontrado.");

                var dto = MapperProveedor.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoProveedor>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoProveedor>
                    .Fail("Error consultando proveedor: " + ex.Message);
            }
        }

        // ======================================================
        // CONSULTAR POR IDENTIFICACIÓN
        // ======================================================
        public async Task<RespuestaGeneral<DtoProveedor>> ConsultarPorIdentificacionAsync(string identificacion)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Proveedor.ConsultarPorIdentificacion(identificacion));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoProveedor>
                        .Fail("Proveedor no encontrado.");

                var dto = MapperProveedor.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoProveedor>.Ok(dto);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoProveedor>
                    .Fail("Error consultando proveedor: " + ex.Message);
            }
        }

        // ======================================================
        // INSERTAR
        // ======================================================
        public async Task<RespuestaGeneral<string>> InsertarAsync(SolicitudProveedor s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Proveedor.Insertar(
                        s.TipoIdentificacion,
                        s.Identificacion,
                        s.RazonSocial,
                        s.Correo,
                        s.Direccion,
                        s.Telefono,
                        s.TipoPersona,
                        s.EsRimpe,
                        s.TipoRimpe,
                        s.EsProfesional,
                        s.EsArrendador,
                        s.Estado,
                        s.Usuario ?? "",
                        s.IP ?? ""
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Proveedor insertado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo insertar el proveedor.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error insertando proveedor: " + ex.Message);
            }
        }

        // ======================================================
        // ACTUALIZAR
        // ======================================================
        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudProveedor s)
        {
            try
            {
                if (s.IdProveedor == null)
                    return RespuestaGeneral<string>
                        .Fail("IdProveedor es requerido para actualizar.");

                int r = await Task.Run(() =>
                    _services.Proveedor.Actualizar(
                        s.IdProveedor.Value,
                        s.TipoIdentificacion,
                        s.Identificacion,
                        s.RazonSocial,
                        s.Correo,
                        s.Direccion,
                        s.Telefono,
                        s.TipoPersona,
                        s.EsRimpe,
                        s.TipoRimpe,
                        s.EsProfesional,
                        s.EsArrendador,
                        s.Estado,
                        s.Usuario ?? "",
                        s.IP ?? ""
                    ));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Proveedor actualizado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo actualizar el proveedor.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando proveedor: " + ex.Message);
            }
        }

        // ======================================================
        // ELIMINAR
        // ======================================================
        public async Task<RespuestaGeneral<string>> EliminarAsync(int idProveedor, string usuario, string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Proveedor.Eliminar(idProveedor, usuario, ip));

                return r > 0
                    ? RespuestaGeneral<string>.Ok("Proveedor eliminado correctamente.")
                    : RespuestaGeneral<string>.Fail("No se pudo eliminar el proveedor.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando proveedor: " + ex.Message);
            }
        }
    }
}