using System.ComponentModel.DataAnnotations;

namespace ArtistaConnect.Model;

public sealed class ArtistaRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? GeneroMusical { get; set; }
}

public sealed class GeneroMusicalRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;
}
