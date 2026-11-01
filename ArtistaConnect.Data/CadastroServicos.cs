using ArtistaConnect.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtistaConnect.Data;

public class CadastroServicos
{
    // Retorna null quando o nome já existe.
    public Usuario? CriarUsuario(string nome, string senhaHash)
    {
        using var db = new ArtistaContext();
        if (db.Usuarios.Any(u => u.Nome == nome)) return null;

        var usuario = new Usuario { Nome = nome, SenhaHash = senhaHash };
        db.Usuarios.Add(usuario);
        return Salvar(db) ? usuario : null;
    }

    public Usuario? BuscarUsuario(string nome)
    {
        using var db = new ArtistaContext();
        return db.Usuarios.AsNoTracking().FirstOrDefault(u => u.Nome == nome);
    }

    public List<Artista> ListarArtistas()
    {
        using var db = new ArtistaContext();
        return db.Artistas.AsNoTracking().OrderBy(a => a.Nome).ToList();
    }

    public Artista? BuscarArtista(string id)
    {
        using var db = new ArtistaContext();
        return db.Artistas.AsNoTracking().FirstOrDefault(a => a.ID == id);
    }

    public Artista CriarArtista(string nome, string? bio, string? genero)
    {
        using var db = new ArtistaContext();
        var artista = new Artista
        {
            ID = Guid.NewGuid().ToString(),
            Nome = nome,
            Bio = bio,
            GeneroMusical = genero
        };
        db.Artistas.Add(artista);
        db.SaveChanges();
        return artista;
    }

    public Artista? EditarArtista(string id, string nome, string? bio, string? genero)
    {
        using var db = new ArtistaContext();
        var artista = db.Artistas.FirstOrDefault(a => a.ID == id);
        if (artista is null) return null;

        artista.Nome = nome;
        artista.Bio = bio;
        artista.GeneroMusical = genero;
        db.SaveChanges();
        return artista;
    }

    public bool RemoverArtista(string id)
    {
        using var db = new ArtistaContext();
        var artista = db.Artistas.FirstOrDefault(a => a.ID == id);
        if (artista is null) return false;

        db.Artistas.Remove(artista);
        db.SaveChanges();
        return true;
    }

    public List<GeneroMusical> ListarGeneros()
    {
        using var db = new ArtistaContext();
        return db.GenerosMusicais.AsNoTracking().OrderBy(g => g.Nome).ToList();
    }

    public GeneroMusical? BuscarGenero(string id)
    {
        using var db = new ArtistaContext();
        return db.GenerosMusicais.AsNoTracking().FirstOrDefault(g => g.ID == id);
    }

    // Retorna null quando o nome já existe.
    public GeneroMusical? CriarGenero(string nome)
    {
        using var db = new ArtistaContext();
        if (db.GenerosMusicais.Any(g => g.Nome == nome)) return null;

        var genero = new GeneroMusical { ID = Guid.NewGuid().ToString(), Nome = nome };
        db.GenerosMusicais.Add(genero);
        return Salvar(db) ? genero : null;
    }

    // Retorna (encontrado, conflito).
    public (bool Encontrado, bool Conflito) EditarGenero(string id, string nome)
    {
        using var db = new ArtistaContext();
        var genero = db.GenerosMusicais.FirstOrDefault(g => g.ID == id);
        if (genero is null) return (false, false);
        if (db.GenerosMusicais.Any(g => g.Nome == nome && g.ID != id)) return (true, true);

        genero.Nome = nome;
        return (true, !Salvar(db));
    }

    public bool RemoverGenero(string id)
    {
        using var db = new ArtistaContext();
        var genero = db.GenerosMusicais.FirstOrDefault(g => g.ID == id);
        if (genero is null) return false;

        db.GenerosMusicais.Remove(genero);
        db.SaveChanges();
        return true;
    }

    // Trata corrida entre a checagem de duplicidade e o índice único.
    private static bool Salvar(ArtistaContext db)
    {
        try
        {
            db.SaveChanges();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
