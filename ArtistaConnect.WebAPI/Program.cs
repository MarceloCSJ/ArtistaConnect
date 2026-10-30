using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ArtistaConnect.Data;
using ArtistaConnect.Model;
using ArtistaConnect.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("ArtistaConnect")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings:ArtistaConnect antes de iniciar a API.");
var jwtSettings = builder.Configuration.GetSection("JWTSetting");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException(
        "Configure JWTSetting:SecretKey fora do código-fonte.");
var issuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("Configure JWTSetting:Issuer.");
var audience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("Configure JWTSetting:Audience.");
var expirationInHours = jwtSettings.GetValue<double>("ExpirationInHours");

if (Encoding.UTF8.GetByteCount(secretKey) < 32)
{
    throw new InvalidOperationException(
        "JWTSetting:SecretKey deve possuir ao menos 32 bytes.");
}
if (expirationInHours <= 0)
{
    throw new InvalidOperationException(
        "JWTSetting:ExpirationInHours deve ser maior que zero.");
}

builder.Services.AddDbContext<ArtistaContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddAuthentication("JwtBearer")
    .AddJwtBearer("JwtBearer", options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArtistaContext>();
    await context.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/usuarios", async (
    CadastroUsuarioRequest request,
    ArtistaContext context,
    IPasswordHasher<Usuario> passwordHasher,
    CancellationToken cancellationToken) =>
{
    request.Nome = request.Nome?.Trim() ?? string.Empty;
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    if (await context.Usuarios.AnyAsync(
            usuario => usuario.Nome == request.Nome,
            cancellationToken))
    {
        return Results.Conflict(new { message = "Já existe um usuário com esse nome." });
    }

    var usuario = new Usuario { Nome = request.Nome };
    usuario.SenhaHash = passwordHasher.HashPassword(usuario, request.Senha);
    context.Usuarios.Add(usuario);
    await context.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/usuarios/{usuario.ID}",
        new UsuarioResponse { ID = usuario.ID, Nome = usuario.Nome });
});

app.MapPost("/login/auth", async (
    LoginRequest request,
    ArtistaContext context,
    IPasswordHasher<Usuario> passwordHasher,
    TokenService tokenService,
    CancellationToken cancellationToken) =>
{
    request.Nome = request.Nome?.Trim() ?? string.Empty;
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var usuario = await context.Usuarios
        .SingleOrDefaultAsync(
            item => item.Nome == request.Nome,
            cancellationToken);
    if (usuario is null ||
        passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.SenhaHash,
            request.Senha) == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    return Results.Text(tokenService.GerarToken(usuario), "text/plain");
});

var musicas = app.MapGroup("/musicas").RequireAuthorization();

musicas.MapGet("/", async (ArtistaContext context, CancellationToken cancellationToken) =>
    Results.Ok(await context.Musicas.AsNoTracking().ToListAsync(cancellationToken)));

musicas.MapPost("/", async (
    Musica musica,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(musica.Titulo))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(musica.Titulo)] = ["O título é obrigatório."],
        });
    }

    musica.Titulo = musica.Titulo.Trim();
    context.Musicas.Add(musica);
    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok("Música inserida com sucesso!");
});

musicas.MapPut("/", async (
    Musica musica,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(musica.Titulo))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(musica.Titulo)] = ["O título é obrigatório."],
        });
    }

    var current = await context.Musicas.FindAsync(
        [musica.Titulo.Trim()],
        cancellationToken);
    if (current is null)
    {
        return Results.NotFound();
    }
    current.ArtistaResponsavel = musica.ArtistaResponsavel;
    current.Duracao = musica.Duracao;
    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok("Música editada com sucesso!");
});

musicas.MapDelete("/", async (
    [FromBody] Musica musica,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(musica.Titulo))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(musica.Titulo)] = ["O título é obrigatório."],
        });
    }

    var current = await context.Musicas.FindAsync(
        [musica.Titulo.Trim()],
        cancellationToken);
    if (current is null)
    {
        return Results.NotFound();
    }
    context.Musicas.Remove(current);
    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok("Música deletada com sucesso!");
});

var artistas = app.MapGroup("/artistas").RequireAuthorization();

artistas.MapGet("/", async (ArtistaContext context, CancellationToken cancellationToken) =>
    Results.Ok(await context.Artistas.AsNoTracking().OrderBy(item => item.Nome)
        .ToListAsync(cancellationToken)));

artistas.MapGet("/{id}", async (
    string id,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    var artista = await context.Artistas.AsNoTracking()
        .SingleOrDefaultAsync(item => item.ID == id, cancellationToken);
    return artista is null ? Results.NotFound() : Results.Ok(artista);
});

artistas.MapPost("/", async (
    ArtistaRequest request,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    request.Nome = request.Nome?.Trim() ?? string.Empty;
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var artista = new Artista
    {
        Nome = request.Nome,
        Bio = request.Bio,
        GeneroMusical = request.GeneroMusical,
    };
    context.Artistas.Add(artista);
    await context.SaveChangesAsync(cancellationToken);
    return Results.Created($"/artistas/{artista.ID}", artista);
});

artistas.MapPut("/{id}", async (
    string id,
    ArtistaRequest request,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    request.Nome = request.Nome?.Trim() ?? string.Empty;
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var artista = await context.Artistas.FindAsync([id], cancellationToken);
    if (artista is null)
    {
        return Results.NotFound();
    }
    artista.Nome = request.Nome;
    artista.Bio = request.Bio;
    artista.GeneroMusical = request.GeneroMusical;
    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok(artista);
});

artistas.MapDelete("/{id}", async (
    string id,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    var artista = await context.Artistas.FindAsync([id], cancellationToken);
    if (artista is null)
    {
        return Results.NotFound();
    }
    context.Artistas.Remove(artista);
    await context.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
});

var generos = app.MapGroup("/generos-musicais").RequireAuthorization();

generos.MapGet("/", async (ArtistaContext context, CancellationToken cancellationToken) =>
    Results.Ok(await context.GenerosMusicais.AsNoTracking().OrderBy(item => item.Nome)
        .ToListAsync(cancellationToken)));

generos.MapGet("/{id}", async (
    string id,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    var genero = await context.GenerosMusicais.AsNoTracking()
        .SingleOrDefaultAsync(item => item.ID == id, cancellationToken);
    return genero is null ? Results.NotFound() : Results.Ok(genero);
});

generos.MapPost("/", async (
    GeneroMusicalRequest request,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    request.Nome = request.Nome?.Trim() ?? string.Empty;
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }
    if (await context.GenerosMusicais.AnyAsync(
            genero => genero.Nome == request.Nome,
            cancellationToken))
    {
        return Results.Conflict(new { message = "Já existe um gênero com esse nome." });
    }

    var genero = new GeneroMusical { Nome = request.Nome };
    context.GenerosMusicais.Add(genero);
    await context.SaveChangesAsync(cancellationToken);
    return Results.Created($"/generos-musicais/{genero.ID}", genero);
});

generos.MapPut("/{id}", async (
    string id,
    GeneroMusicalRequest request,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    request.Nome = request.Nome?.Trim() ?? string.Empty;
    var validationErrors = Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var genero = await context.GenerosMusicais.FindAsync([id], cancellationToken);
    if (genero is null)
    {
        return Results.NotFound();
    }
    if (await context.GenerosMusicais.AnyAsync(
            item => item.ID != id && item.Nome == request.Nome,
            cancellationToken))
    {
        return Results.Conflict(new { message = "Já existe um gênero com esse nome." });
    }

    genero.Nome = request.Nome;
    await context.SaveChangesAsync(cancellationToken);
    return Results.Ok(genero);
});

generos.MapDelete("/{id}", async (
    string id,
    ArtistaContext context,
    CancellationToken cancellationToken) =>
{
    var genero = await context.GenerosMusicais.FindAsync([id], cancellationToken);
    if (genero is null)
    {
        return Results.NotFound();
    }
    context.GenerosMusicais.Remove(genero);
    await context.SaveChangesAsync(cancellationToken);
    return Results.NoContent();
});

app.Run();

static Dictionary<string, string[]> Validate(object request)
{
    var validationResults = new List<ValidationResult>();
    Validator.TryValidateObject(
        request,
        new ValidationContext(request),
        validationResults,
        validateAllProperties: true);

    return validationResults
        .SelectMany(result =>
            (result.MemberNames.Any() ? result.MemberNames : [string.Empty])
            .Select(member => new { member, result.ErrorMessage }))
        .GroupBy(result => result.member)
        .ToDictionary(
            group => group.Key,
            group => group.Select(result => result.ErrorMessage ?? "Valor inválido.")
                .ToArray());
}

public partial class Program;
