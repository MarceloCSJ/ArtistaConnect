using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArtistaConnect.Model;

public class Usuario
{
    public int ID { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [JsonIgnore]
    public string SenhaHash { get; set; } = string.Empty;
}
