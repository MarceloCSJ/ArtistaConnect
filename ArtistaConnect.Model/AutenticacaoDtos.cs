using System.ComponentModel.DataAnnotations;

namespace ArtistaConnect.Model;

public sealed class CadastroUsuarioRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    [StringLength(128)]
    public string Senha { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Senha { get; set; } = string.Empty;
}

public sealed class UsuarioResponse
{
    public int ID { get; init; }
    public string Nome { get; init; } = string.Empty;
}

public sealed class TokenResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
