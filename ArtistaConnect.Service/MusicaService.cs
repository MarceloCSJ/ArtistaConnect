using ArtistaConnect.Model;

namespace ArtistaConnect.Service;

public class MusicaService
{
    public void NomeNaoEstaVazio(Musica musica)
    {
        if (string.IsNullOrWhiteSpace(musica.Titulo))
        {
            throw new ArgumentException("O nome não pode estar vazio.");
        }
    }
}