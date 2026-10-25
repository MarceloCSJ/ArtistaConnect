using ArtistaConnect.Model;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;

namespace ArtistaConnect.Data;

public class ArtistaContext : DbContext
{
    // Será criado a tabela Diarios do tipo DbSet
    public DbSet<Musica> Musicas {get; set;}
    // Esse será o primeiro atributo da tabela Diarios
    private readonly string stringConexao = "Server=localhost;Port=3306;Database=ArtistaConnect;Uid=root;Pwd=toor;";

    // Método de conexão através do Entity Framework
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(stringConexao, 
            ServerVersion.AutoDetect(stringConexao));
    }
}
