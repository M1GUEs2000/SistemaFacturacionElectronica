using Facturacion.api.Mappers;
using Facturacion.api.Models.Solicitudes;
using Facturacion.api.Models.DTOs;
using Facturacion.api.Models.Respuestas;
using LogicaNegocios.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Facturacion.api.Servicios
{
    public class ServicioEmpresa : IServicioEmpresa
    {
        private readonly AppServices _services;

        public ServicioEmpresa(
            AppServices services
        )
        {
            _services = services;
        }
        public async Task<RespuestaGeneral<List<DtoEmpresa>>> ObtenerTodasAsync(string nombre)
        {
            try
            {
                var ds = await Task.Run(() => _services.Empresa.MostrarEmpresa(nombre));

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<List<DtoEmpresa>>.Fail("No existen empresas.");

                return RespuestaGeneral<List<DtoEmpresa>>
                    .Ok(MapperEmpresa.ToList(ds.Tables[0]));
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoEmpresa>>
                    .Fail("Error obteniendo empresas: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<DtoEmpresa>> ObtenerPorNombreAsync(string nombre)
        {
            try
            {
                var ds = await Task.Run(() => _services.Empresa.ConsultaNombre(nombre));

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoEmpresa>.Fail("Empresa no encontrada.");

                return RespuestaGeneral<DtoEmpresa>
                    .Ok(MapperEmpresa.ToDto(ds.Tables[0].Rows[0]));
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoEmpresa>
                    .Fail("Error consultando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> CrearAsync(SolicitudEmpresa s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Empresa.Insertar(
                        s.Nombre, s.Direccion, s.UsuarioLogin,
                        s.ClaveIngreso, s.ClaveTotales,
                        s.ClaveEliminar, s.ClaveConsulta, s.ClaveTabla,
                        s.Facturacion, s.Impresion, s.Telefono, s.Propietario,
                        s.Email, s.UbicacionArchivoP12, s.Contrasena,
                        s.Imagen, s.EstadoRuc, s.Ruc,
                        s.Usuario, s.Ip
                    )
                );

                if (r <= 0)
                    return RespuestaGeneral<string>.Fail("No se pudo crear la empresa.");

                await Task.Run(() => _services.Empresa.SincronizarNombreParametros(s.Nombre));

                return RespuestaGeneral<string>.Ok("Empresa creada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error creando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudEmpresa s)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Empresa.Actualizar(
                        s.Nombre, s.Direccion, s.UsuarioLogin,
                        s.ClaveIngreso, s.ClaveTotales,
                        s.ClaveEliminar, s.ClaveConsulta, s.ClaveTabla,
                        s.Facturacion, s.Impresion, s.Telefono, s.Propietario,
                        s.Email, s.Ruc,
                        s.UbicacionArchivoP12, s.Contrasena,
                        s.Imagen, s.EstadoRuc,
                        s.Usuario, s.Ip
                    )
                );

                if (r <= 0)
                    return RespuestaGeneral<string>.Fail("No se pudo actualizar la empresa.");

                await Task.Run(() => _services.Empresa.SincronizarNombreParametros(s.Nombre));

                return RespuestaGeneral<string>.Ok("Empresa actualizada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error actualizando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> EliminarAsync(string nombre, string usuario, string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Empresa.Eliminar(nombre, usuario, ip)
                );

                if (r <= 0)
                    return RespuestaGeneral<string>.Fail("No se pudo eliminar la empresa.");

                return RespuestaGeneral<string>.Ok("Empresa eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error eliminando empresa: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ActualizarEstadoRucAsync(
            string nombreEmpresa,
            string nuevoEstado,
            string usuario,
            string ip)
        {
            try
            {
                int r = await Task.Run(() =>
                    _services.Empresa.ActualizarEstadoRuc(
                        nombreEmpresa,
                        nuevoEstado,
                        usuario,
                        ip
                    )
                );

                if (r <= 0)
                    return RespuestaGeneral<string>.Fail("No se pudo actualizar el estado.");

                return RespuestaGeneral<string>.Ok("Estado RUC actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error actualizando estado RUC: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> CambiarCredencialesAsync(string nuevoUsuario, string nuevaClave)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nuevoUsuario) || string.IsNullOrWhiteSpace(nuevaClave))
                    return RespuestaGeneral<string>.Fail("El usuario y la clave son requeridos.");

                var r = await Task.Run(() =>
                    _services.Empresa.CambiarCredenciales(nuevoUsuario, nuevaClave)
                );

                if (r <= 0)
                    return RespuestaGeneral<string>.Fail("No se pudieron actualizar las credenciales.");

                return RespuestaGeneral<string>.Ok("Credenciales actualizadas correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>.Fail("Error actualizando credenciales: " + ex.Message);
            }
        }
    }
}