using System.ComponentModel.DataAnnotations;

namespace ArtistaConnect.Model;

public class Artista
{
    [Key]
    [StringLength(36)]
    public string ID { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    public string? Bio { get; set; }
    public string? GeneroMusical { get; set; }

    public Artista() { }

    public void ExibirDados()
    {
        Console.WriteLine($"Artista: {Nome}");
        Console.WriteLine($"Bio: {Bio}");
        Console.WriteLine($"Genêro Musical: {GeneroMusical}");
    }
}