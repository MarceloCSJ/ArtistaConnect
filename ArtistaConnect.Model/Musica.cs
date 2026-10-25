using System.ComponentModel.DataAnnotations;

namespace ArtistaConnect.Model;
 
public class Musica
{
    [Key]
    public string? Titulo { get; set; }
    public string? ArtistaResponsavel { get; set; }
    public string? Duracao { get; set; }
 
    public Musica() { }
 
    public void ExibirDados()
    {
        Console.WriteLine($"Música: {Titulo} | Artista: {ArtistaResponsavel} | Duração: {Duracao}");
    }
}