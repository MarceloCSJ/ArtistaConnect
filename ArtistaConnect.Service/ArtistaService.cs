using ArtistaConnect.Model;

namespace ArtistaConnect.Service;

public class ArtistaService
{
    public void NomeNaoEstaVazio(Artista artista)
    {
        if (string.IsNullOrWhiteSpace(artista.Nome))
        {
            throw new ArgumentException("O nome não pode estar vazio.");
        }
    }
}