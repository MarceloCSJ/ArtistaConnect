using System.Text;
using System.Text.Json.Serialization;
using ArtistaConnect.Model;
using ArtistaConnect.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

/* não esquecer:
dotnet ef migrations add NomedaMigration - Para iniciar uma nova migration

dotnet ef migrations remove - se der ruim

dotnet ef database update NomeDaUltimaMigration - atualizar tudo
*/

var builder = WebApplication.CreateBuilder(args);


// Caso o objeto fique em loop infinito, causando erro no servidor:
builder.Services.ConfigureHttpJsonOptions(options => {
options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var jwtSettings = builder.Configuration.GetSection("JWTSetting");
var chaveSecretaBytes = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
}).AddJwtBearer("JwtBearer", options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(chaveSecretaBytes),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login/auth", ([FromBody] Artista artista) =>
{
    TokenService authService = new TokenService(builder.Configuration);
    return authService.GerarToken(artista);
});

var musicasGroup = app.MapGroup("/musicas").RequireAuthorization();

//End-point para listar todas as musicas
musicasGroup.MapGet("/", () => {
    List<Musica> musicas = new MusicaServicos().ListarMusicas();
    return musicas;
});
//End-point para Inserir/Postar a musica no Banco de Dados
musicasGroup.MapPost("/", ([FromBody] Musica musica) =>{
    new MusicaServicos().InserirMusica(musica);
    return "Música inserida com sucesso!";
});
//End-point para Editar a musica
musicasGroup.MapPut("/", ([FromBody] Musica musica) =>{
    new MusicaServicos().EditarMusica(musica);
    return "Música editada com sucesso!";
});
//End-point para Deletar a musica
musicasGroup.MapDelete("/", ([FromBody] Musica musica) =>{
    new MusicaServicos().RemoverMusica(musica);
    return "Música deletada com sucesso!";
});
app.Run();
