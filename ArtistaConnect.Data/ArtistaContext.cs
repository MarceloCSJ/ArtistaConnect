using ArtistaConnect.Model;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;

namespace ArtistaConnect.Data;

public class ArtistaContext : DbContext
{
    // Será criado a tabela Diarios do tipo DbSet
    public DbSet<Musica> Musicas {get; set;}
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Artista> Artistas { get; set; }
    public DbSet<GeneroMusical> GenerosMusicais { get; set; }
    // Esse será o primeiro atributo da tabela Diarios
    private readonly string stringConexao = "Server=localhost;Port=3306;Database=ArtistaConnect;Uid=root;Pwd=198422;";

    // Método de conexão através do Entity Framework
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(stringConexao, 
            ServerVersion.AutoDetect(stringConexao));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Nome).IsRequired().HasMaxLength(100);
            e.Property(u => u.SenhaHash).IsRequired().HasMaxLength(500);
            e.HasIndex(u => u.Nome).IsUnique();
        });

        modelBuilder.Entity<Artista>(e =>
        {
            e.HasKey(a => a.ID);
            e.Property(a => a.ID).HasMaxLength(36);
            e.Property(a => a.Nome).IsRequired().HasMaxLength(150);
            e.Property(a => a.Bio).HasMaxLength(1000);
            e.Property(a => a.GeneroMusical).HasMaxLength(100);
        });

        modelBuilder.Entity<GeneroMusical>(e =>
        {
            e.HasKey(g => g.ID);
            e.Property(g => g.ID).HasMaxLength(36);
            e.Property(g => g.Nome).IsRequired().HasMaxLength(100);
            e.HasIndex(g => g.Nome).IsUnique();
        });
    }
}
