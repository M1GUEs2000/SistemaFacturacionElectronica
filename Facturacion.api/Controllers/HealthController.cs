using System;
using System.Net;
using System.Reflection;
using System.Web.Http;
using AccesoDatos.Abstractions;
using AccesoDatosWeb;
using Facturacion.api.App_Start;
using Serilog;

namespace Facturacion.api.Controllers
{
    /// <summary>
    /// Sondas de salud para balanceadores y monitoreo (Plesk, uptime checks).
    /// Hereda ApiController directamente: no requiere token ni contexto de empresa.
    /// </summary>
    [RoutePrefix("api/health")]
    public class HealthController : ApiController
    {
        // =========================================================
        // LIVENESS — GET api/health
        // Confirma que el proceso responde. No toca la base de datos.
        // =========================================================
        [HttpGet]
        [Route("")]
        public IHttpActionResult Liveness()
        {
            return Ok(new
            {
                estado = "vivo",
                utc = DateTime.UtcNow,
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            });
        }

        // =========================================================
        // READINESS — GET api/health/db
        // Comprobacion de conectividad de solo lectura (SELECT 1).
        // No lee tablas del negocio. 200 si conecta, 503 si no.
        // =========================================================
        [HttpGet]
        [Route("db")]
        public IHttpActionResult BaseDeDatos()
        {
            try
            {
                IConexionBD conexion = new ConexionBD(FacturacionConfig.GetDatabaseSettings());
                conexion.Seleccionar("SELECT 1");

                return Ok(new { estado = "vivo", baseDeDatos = "conectada" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Health check de base de datos fallido.");

                return Content(HttpStatusCode.ServiceUnavailable, new
                {
                    estado = "degradado",
                    baseDeDatos = "inaccesible"
                });
            }
        }
    }
}
