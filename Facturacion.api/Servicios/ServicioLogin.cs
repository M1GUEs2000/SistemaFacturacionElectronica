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
    public class ServicioLogin : IServicioLogin
    {
        private readonly AppServices _services;

        public ServicioLogin(AppServices services)
        {
            _services = services;
        }

        // ==========================================
        // LOGIN POR CLAVE
        // ==========================================

        public async Task<RespuestaGeneral<DtoLogin>> LoginAsync(string clave)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Login.ConsultarUsuario(clave)
                );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoLogin>
                        .Fail("Clave incorrecta.");

                var login = MapperLogin.ToDto(ds.Tables[0].Rows[0]);

                return RespuestaGeneral<DtoLogin>
                    .Ok(login);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoLogin>
                    .Fail("Error en login: " + ex.Message);
            }
        }

        // ==========================================
        // LOGIN CON USUARIO + CLAVE
        // ==========================================

        public async Task<RespuestaGeneral<DtoLogin>> LoginConUsuarioAsync(
            SolicitudLogin solicitud)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Login.ConsultarUsuario(solicitud.ClaveIngreso)
                );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<DtoLogin>
                        .Fail("Usuario o clave incorrectos.");

                var login = MapperLogin.ToDto(ds.Tables[0].Rows[0]);

                login.DebeCambiarCredenciales = await VerificarCredencialesPorDefecto();

                return RespuestaGeneral<DtoLogin>
                    .Ok(login);
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<DtoLogin>
                    .Fail("Error en login: " + ex.Message);
            }
        }

        private async Task<bool> VerificarCredencialesPorDefecto()
        {
            try
            {
                bool modificadas = await Task.Run(() =>
                    _services.Empresa.ObtenerCredencialesModificadas()
                );
                return !modificadas;
            }
            catch
            {
                return false;
            }
        }

        // ==========================================
        // CLAVE ELIMINAR
        // ==========================================

        public async Task<RespuestaGeneral<string>> ValidarClaveEliminarAsync(string clave)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Login.ConsultarClaveEliminar(clave)
                );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<string>
                        .Fail("Clave incorrecta.");

                return RespuestaGeneral<string>
                    .Ok("Clave válida.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error validando clave: " + ex.Message);
            }
        }

        // ==========================================
        // CLAVE TOTALES
        // ==========================================

        public async Task<RespuestaGeneral<string>> ValidarClaveTotalesAsync(string clave)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Login.ConsultarClaveTotal(clave)
                );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<string>
                        .Fail("Clave incorrecta.");

                return RespuestaGeneral<string>
                    .Ok("Clave válida.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error validando clave: " + ex.Message);
            }
        }

        // ==========================================
        // CLAVE CONSULTA
        // ==========================================

        public async Task<RespuestaGeneral<string>> ValidarClaveConsultaAsync(string clave)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Login.ConsultarClaveConsultaFecha(clave)
                );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<string>
                        .Fail("Clave incorrecta.");

                return RespuestaGeneral<string>
                    .Ok("Clave válida.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error validando clave: " + ex.Message);
            }
        }

        // ==========================================
        // CLAVE TABLAS
        // ==========================================

        public async Task<RespuestaGeneral<string>> ValidarClaveTablasAsync(string clave)
        {
            try
            {
                var ds = await Task.Run(() =>
                    _services.Login.ConsultarClaveTabla(clave)
                );

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return RespuestaGeneral<string>
                        .Fail("Clave incorrecta.");

                return RespuestaGeneral<string>
                    .Ok("Clave válida.");
            }
            catch (Exception ex)
            {
                return RespuestaGeneral<string>
                    .Fail("Error validando clave: " + ex.Message);
            }
        }
    }
}