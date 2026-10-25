namespace ArtistaConnect.Model;
 
public class Artista
{
    public string? ID { get; set; }
    public string? Nome { get; set; }
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