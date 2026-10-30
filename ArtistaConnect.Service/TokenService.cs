using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtistaConnect.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ArtistaConnect.Service;

public class TokenService
{
    private readonly IConfiguration configuration;

    public TokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GerarToken(Usuario usuario)
    {
        var jwtSettings = configuration.GetSection("JWTSetting");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWTSetting:SecretKey não configurada.");
        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("JWTSetting:Issuer não configurado.");
        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("JWTSetting:Audience não configurado.");
        var expirationInHours = jwtSettings.GetValue<double>("ExpirationInHours");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.ID.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
            ],
            expires: DateTime.UtcNow.AddHours(expirationInHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
