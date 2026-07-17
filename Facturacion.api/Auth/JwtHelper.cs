using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Facturacion.api.Auth
{
    public static class JwtHelper
    {
        private static string Secret =>
            WebConfigurationManager.AppSettings["JwtSecret"];

        private static int HorasExpiracion =>
            int.TryParse(WebConfigurationManager.AppSettings["JwtHorasExpiracion"], out int h) ? h : 8;

        // Rol por defecto de los tokens de tenant (login de empresa/usuario).
        public const string RolEmpresa = "empresa";
        // Rol de los operadores del sistema (login de administrador general).
        public const string RolAdmin = "admin";

        public static string Generar(string empresa) => Generar(empresa, RolEmpresa);

        public static string Generar(string empresa, string rol)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[]
                {
                    new Claim("empresa", empresa ?? ""),
                    new Claim("rol", string.IsNullOrWhiteSpace(rol) ? RolEmpresa : rol)
                },
                expires: DateTime.UtcNow.AddHours(HorasExpiracion),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida un token de tenant: firma correcta Y claim empresa presente.
        /// Mantiene el contrato histórico (BaseController, RateLimitingHandler).
        /// </summary>
        public static bool Validar(string tokenStr, out string empresa)
        {
            string rol;
            return ValidarConRol(tokenStr, out empresa, out rol) && !string.IsNullOrEmpty(empresa);
        }

        /// <summary>
        /// Valida solo la firma del token y extrae los claims empresa y rol.
        /// Devuelve true si la firma es válida aunque empresa venga vacía
        /// (el token de admin no tiene empresa). Lo usa el filtro [SoloAdmin].
        /// </summary>
        public static bool ValidarConRol(string tokenStr, out string empresa, out string rol)
        {
            empresa = null;
            rol = null;
            if (string.IsNullOrWhiteSpace(tokenStr)) return false;

            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
                var handler = new JwtSecurityTokenHandler();

                handler.ValidateToken(tokenStr, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validated);

                var claims = ((JwtSecurityToken)validated).Claims;
                empresa = claims.FirstOrDefault(c => c.Type == "empresa")?.Value;
                rol = claims.FirstOrDefault(c => c.Type == "rol")?.Value;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
