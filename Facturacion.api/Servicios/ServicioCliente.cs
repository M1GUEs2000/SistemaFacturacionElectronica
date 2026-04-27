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
    public class ServicioCliente : IServicioCliente
    {
        private readonly AppServices _services;

        public ServicioCliente(
            AppServices services
        )
        {
            _services = services;
        }
        public async Task<RespuestaGeneral<List<DtoCliente>>> ObtenerTodosAsync()
        {

            try
            {
                var ds = await Task.Run(() => _services.Cliente.Mostrar());

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<List<DtoCliente>>
                        .Fail("No existen clientes.");

                var lista = MapperCliente.ToList(ds.Tables[0]);

                return RespuestaGeneral<List<DtoCliente>>
                    .Ok(lista);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<List<DtoCliente>>
                    .Fail("Error obteniendo clientes: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<DtoCliente>> ObtenerPorCedulaAsync(string cedula)
        {
            try
            {
                var ds = await Task.Run(() => _services.Cliente.ConsultarCedula(cedula));

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoCliente>
                        .Fail("Cliente no encontrado.");

                var cliente = MapperCliente.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoCliente>
                    .Ok(cliente);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoCliente>
                    .Fail("Error consultando cliente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> CrearAsync(SolicitudCliente solicitud)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.Cliente.Insertar(
                        solicitud.Cedula,
                        solicitud.Nombre,
                        solicitud.Correo,
                        solicitud.Direccion,
                        solicitud.Telefono,
                        solicitud.Usuario,
                        solicitud.Ip
                    )
                );

                if (resultado <= 0)
                    return RespuestaGeneral<string>
                        .Fail("No se pudo crear el cliente.");

                return RespuestaGeneral<string>
                    .Ok("Cliente creado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error creando cliente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> ActualizarAsync(SolicitudCliente solicitud)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.Cliente.Actualizar(
                        solicitud.Cedula,
                        solicitud.Nombre,
                        solicitud.Correo,
                        solicitud.Direccion,
                        solicitud.Telefono,
                        solicitud.Usuario,
                        solicitud.Ip
                    )
                );

                if (resultado <= 0)
                    return RespuestaGeneral<string>
                        .Fail("No se pudo actualizar el cliente.");

                return RespuestaGeneral<string>
                    .Ok("Cliente actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error actualizando cliente: " + ex.Message);
            }
        }

        public async Task<RespuestaGeneral<string>> EliminarAsync(
            string cedula,
            string nombre,
            string usuario,
            string ip)
        {
            try
            {
                int resultado = await Task.Run(() =>
                    _services.Cliente.Eliminar(
                        cedula,
                        nombre,
                        usuario,
                        ip
                    )
                );

                if (resultado <= 0)
                    return RespuestaGeneral<string>
                        .Fail("No se pudo eliminar el cliente.");

                return RespuestaGeneral<string>
                    .Ok("Cliente eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error eliminando cliente: " + ex.Message);
            }
        }
    }
}