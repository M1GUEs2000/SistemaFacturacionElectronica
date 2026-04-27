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

        public static string Generar(string empresa)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[] { new Claim("empresa", empresa ?? "") },
                expires: DateTime.UtcNow.AddHours(HorasExpiracion),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static bool Validar(string tokenStr, out string empresa)
        {
            empresa = null;
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

                empresa = ((JwtSecurityToken)validated)
                    .Claims.FirstOrDefault(c => c.Type == "empresa")?.Value;

                return !string.IsNullOrEmpty(empresa);
            }
            catch
            {
                return false;
            }
        }
    }
}
