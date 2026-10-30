using ArtistaConnect.Model;
using Microsoft.EntityFrameworkCore;

namespace ArtistaConnect.Data;

public class ArtistaContext(DbContextOptions<ArtistaContext> options) : DbContext(options)
{
    public DbSet<Musica> Musicas => Set<Musica>();
    public DbSet<Artista> Artistas => Set<Artista>();
    public DbSet<GeneroMusical> GenerosMusicais => Set<GeneroMusical>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Musica>().HasKey(musica => musica.Titulo);

        modelBuilder.Entity<Artista>()
            .HasKey(artista => artista.ID);
        modelBuilder.Entity<Artista>()
            .Property(artista => artista.ID)
            .HasMaxLength(36);
        modelBuilder.Entity<Artista>()
            .Property(artista => artista.Nome)
            .HasMaxLength(120)
            .IsRequired();

        modelBuilder.Entity<GeneroMusical>()
            .HasKey(genero => genero.ID);
        modelBuilder.Entity<GeneroMusical>()
            .Property(genero => genero.ID)
            .HasMaxLength(36);
        modelBuilder.Entity<GeneroMusical>()
            .Property(genero => genero.Nome)
            .HasMaxLength(80)
            .IsRequired();
        modelBuilder.Entity<GeneroMusical>()
            .HasIndex(genero => genero.Nome)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasKey(usuario => usuario.ID);
        modelBuilder.Entity<Usuario>()
            .Property(usuario => usuario.Nome)
            .HasMaxLength(120)
            .IsRequired();
        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Nome)
            .IsUnique();
        modelBuilder.Entity<Usuario>()
            .Property(usuario => usuario.SenhaHash)
            .HasMaxLength(512)
            .IsRequired();
    }
}
