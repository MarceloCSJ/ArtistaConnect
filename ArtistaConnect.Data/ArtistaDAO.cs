using System.Linq.Expressions;
using ArtistaConnect.Model;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
namespace ArtistaConnect.Data;

public class ArtistaDAO
{
    private ArtistaContext conexao = new ArtistaContext();

    public void InserirMusica(Musica musica)
    {
        try
        {
            conexao.Musicas.Add(musica);
            conexao.SaveChanges();
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.InnerException);
            throw;
        }
        
    }
    public List<Musica> ListarMusicas()
    {
        return conexao.Musicas.ToList();
    }
    public void EditarMusica(Musica musica)
    {
        conexao.Musicas.Update(musica);
        conexao.SaveChanges();
    }
    public void RemoverMusica(Musica musica)
    {
        conexao.Musicas.Remove(musica);
        conexao.SaveChanges();
    }
}