//instalado neste projeto o Microsoft.AspNetCore.Authenticator.JwtBearer através do NuGet
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtistaConnect.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
//using Microsoft.Extensions.Configuration;

public class TokenService
{
    // injeção de dependência:
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GerarToken(Artista artista)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwtSettings = _configuration.GetSection("JWTSetting");
        var chaveSecretaBytes = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);
        // var chaveSecretaBytes = Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["JWTSetting:SecretKey"]!);

        var credenciais = new SigningCredentials
        (
            new SymmetricSecurityKey(chaveSecretaBytes), 
            SecurityAlgorithms.HmacSha256Signature
        );

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, artista.Nome!)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credenciais,
            Expires = DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpirationInHours"]!))
        };

        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

}