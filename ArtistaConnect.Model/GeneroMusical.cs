using System.ComponentModel.DataAnnotations;

namespace ArtistaConnect.Model;

public class GeneroMusical
{
    [Key]
    [StringLength(36)]
    public string ID { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;
}
