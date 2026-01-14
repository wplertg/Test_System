using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Tools.JWT
{
    public class JwtTokenHelper
    {
        private readonly IConfiguration _configuration;

        public JwtTokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(
     int userId,
     string userName,
     string? role = null)
        {
            var jwt = _configuration.GetSection("Jwt");

            var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new(JwtRegisteredClaimNames.UniqueName, userName),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwt["ExpireMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
