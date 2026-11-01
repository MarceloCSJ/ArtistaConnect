using ArtistaConnect.Data;
using ArtistaConnect.Model;
using Microsoft.AspNetCore.Identity;

public record CredenciaisRequest(string? Nome, string? Senha);
public record ArtistaRequest(string? Nome, string? Bio, string? GeneroMusical);
public record GeneroRequest(string? Nome);

public static class CadastroEndpoints
{
    private const int SenhaMinima = 12;
    private const int SenhaMaxima = 128;
    private const int NomeUsuarioMaximo = 100;
    private const int NomeArtistaMaximo = 150;
    private const int BioMaxima = 1000;
    private const int NomeGeneroMaximo = 100;

    private static readonly PasswordHasher<Usuario> Hasher = new();
    // Hash descartável para igualar o tempo de resposta quando o usuário não existe.
    private static readonly string HashFalso = Hasher.HashPassword(new Usuario(), "senha-descartavel-invalida");

    public static void MapCadastroEndpoints(this WebApplication app)
    {
        var servicos = new CadastroServicos();

        app.MapPost("/login/auth", (CredenciaisRequest req, IConfiguration config) =>
        {
            var nome = req.Nome?.Trim();
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(req.Senha) || req.Senha.Length > SenhaMaxima)
                return Results.Unauthorized();

            var usuario = servicos.BuscarUsuario(nome);
            var resultado = Hasher.VerifyHashedPassword(
                usuario ?? new Usuario(), usuario?.SenhaHash ?? HashFalso, req.Senha);

            if (usuario is null || resultado == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            return Results.Text(new TokenService(config).GerarToken(usuario.Nome));
        }).RequireRateLimiting("autenticacao");

        app.MapPost("/usuarios", (CredenciaisRequest req) =>
        {
            var nome = req.Nome?.Trim();
            var erros = new Dictionary<string, string[]>();
            if (string.IsNullOrEmpty(nome) || nome.Length > NomeUsuarioMaximo)
                erros["nome"] = [$"Informe um nome de usuário de até {NomeUsuarioMaximo} caracteres."];
            if (req.Senha is null || req.Senha.Length < SenhaMinima || req.Senha.Length > SenhaMaxima)
                erros["senha"] = [$"A senha deve ter entre {SenhaMinima} e {SenhaMaxima} caracteres."];
            if (erros.Count > 0) return Results.ValidationProblem(erros);

            var usuario = servicos.CriarUsuario(nome!, Hasher.HashPassword(new Usuario(), req.Senha!));
            return usuario is null
                ? Results.Problem("Nome de usuário já cadastrado.", statusCode: StatusCodes.Status409Conflict)
                : Results.Created($"/usuarios/{usuario.Id}", new { id = usuario.Id, nome = usuario.Nome });
        }).RequireRateLimiting("autenticacao");

        MapArtistas(app, servicos);
        MapGeneros(app, servicos);
    }

    private static void MapArtistas(WebApplication app, CadastroServicos servicos)
    {
        var grupo = app.MapGroup("/artistas").RequireAuthorization();

        grupo.MapGet("/", () => servicos.ListarArtistas());

        grupo.MapGet("/{id}", (string id) =>
            servicos.BuscarArtista(id) is { } artista ? Results.Ok(artista) : Results.NotFound());

        grupo.MapPost("/", (ArtistaRequest req) =>
        {
            if (ValidarArtista(req) is { } erros) return Results.ValidationProblem(erros);
            var artista = servicos.CriarArtista(req.Nome!.Trim(), Limpar(req.Bio), Limpar(req.GeneroMusical));
            return Results.Created($"/artistas/{artista.ID}", artista);
        });

        grupo.MapPut("/{id}", (string id, ArtistaRequest req) =>
        {
            if (ValidarArtista(req) is { } erros) return Results.ValidationProblem(erros);
            return servicos.EditarArtista(id, req.Nome!.Trim(), Limpar(req.Bio), Limpar(req.GeneroMusical))
                is { } artista ? Results.Ok(artista) : Results.NotFound();
        });

        grupo.MapDelete("/{id}", (string id) =>
            servicos.RemoverArtista(id) ? Results.NoContent() : Results.NotFound());
    }

    private static void MapGeneros(WebApplication app, CadastroServicos servicos)
    {
        var grupo = app.MapGroup("/generos-musicais").RequireAuthorization();

        grupo.MapGet("/", () => servicos.ListarGeneros());

        grupo.MapGet("/{id}", (string id) =>
            servicos.BuscarGenero(id) is { } genero ? Results.Ok(genero) : Results.NotFound());

        grupo.MapPost("/", (GeneroRequest req) =>
        {
            if (ValidarGenero(req) is { } erros) return Results.ValidationProblem(erros);
            var genero = servicos.CriarGenero(req.Nome!.Trim());
            return genero is null
                ? Results.Problem("Gênero musical já cadastrado.", statusCode: StatusCodes.Status409Conflict)
                : Results.Created($"/generos-musicais/{genero.ID}", genero);
        });

        grupo.MapPut("/{id}", (string id, GeneroRequest req) =>
        {
            if (ValidarGenero(req) is { } erros) return Results.ValidationProblem(erros);
            var (encontrado, conflito) = servicos.EditarGenero(id, req.Nome!.Trim());
            if (!encontrado) return Results.NotFound();
            if (conflito)
                return Results.Problem("Gênero musical já cadastrado.", statusCode: StatusCodes.Status409Conflict);
            return Results.Ok(servicos.BuscarGenero(id));
        });

        grupo.MapDelete("/{id}", (string id) =>
            servicos.RemoverGenero(id) ? Results.NoContent() : Results.NotFound());
    }

    private static Dictionary<string, string[]>? ValidarArtista(ArtistaRequest req)
    {
        var erros = new Dictionary<string, string[]>();
        var nome = req.Nome?.Trim();
        if (string.IsNullOrEmpty(nome) || nome.Length > NomeArtistaMaximo)
            erros["nome"] = [$"Informe o nome do artista (até {NomeArtistaMaximo} caracteres)."];
        if (req.Bio is { Length: > BioMaxima })
            erros["bio"] = [$"A biografia deve ter no máximo {BioMaxima} caracteres."];
        if (req.GeneroMusical is { Length: > NomeGeneroMaximo })
            erros["generoMusical"] = [$"O gênero deve ter no máximo {NomeGeneroMaximo} caracteres."];
        return erros.Count > 0 ? erros : null;
    }

    private static Dictionary<string, string[]>? ValidarGenero(GeneroRequest req)
    {
        var nome = req.Nome?.Trim();
        return string.IsNullOrEmpty(nome) || nome.Length > NomeGeneroMaximo
            ? new() { ["nome"] = [$"Informe o nome do gênero (até {NomeGeneroMaximo} caracteres)."] }
            : null;
    }

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
