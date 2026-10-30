using ArtistaConnect.Data;
using ArtistaConnect.Model;

public class MusicaServicos
{
    private readonly ArtistaDAO artistaDAO;

    public MusicaServicos(ArtistaDAO artistaDAO)
    {
        this.artistaDAO = artistaDAO;
    }
    public void InserirMusica(Musica musica)
    {
        artistaDAO.InserirMusica(musica);
    }
    public List<Musica> ListarMusicas()
    {
        try
        {
            return artistaDAO.ListarMusicas();
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"§| {e.Message}");
            throw;
        }
    }
    public void EditarMusica(Musica musica)
    {
        artistaDAO.EditarMusica(musica);
    }
    public void RemoverMusica(Musica musica)
    {
        artistaDAO.RemoverMusica(musica);
    }
}