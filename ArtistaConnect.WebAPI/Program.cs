using System.Text;
using System.Threading.RateLimiting;
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

// Limita tentativas de login/cadastro por IP (força bruta).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("autenticacao", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapCadastroEndpoints();

var musicasGroup = app.MapGroup("/musicas").RequireAuthorization();

//End-point para listar todas as musicas
musicasGroup.MapGet("/", () => {
    List<Musica> musicas = new MusicaServicos().ListarMusicas();
    return musicas;
});
//End-point para Inserir/Postar a musica no Banco de Dados
musicasGroup.MapPost("/", ([FromBody] Musica musica) =>{
    if (!DuracaoValida(musica.Duracao))
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["duracao"] = ["Informe a duração no formato minutos:segundos (ex.: 3:45)."]
        });
    new MusicaServicos().InserirMusica(musica);
    return Results.Text("Música inserida com sucesso!");
});
//End-point para Editar a musica
musicasGroup.MapPut("/", ([FromBody] Musica musica) =>{
    if (!DuracaoValida(musica.Duracao))
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["duracao"] = ["Informe a duração no formato minutos:segundos (ex.: 3:45)."]
        });
    new MusicaServicos().EditarMusica(musica);
    return Results.Text("Música editada com sucesso!");
});
//End-point para Deletar a musica
musicasGroup.MapDelete("/", ([FromBody] Musica musica) =>{
    new MusicaServicos().RemoverMusica(musica);
    return "Música deletada com sucesso!";
});
static bool DuracaoValida(string? duracao) =>
    duracao is not null && System.Text.RegularExpressions.Regex.IsMatch(duracao, @"^\d{1,3}:[0-5]\d$");

app.Run();
