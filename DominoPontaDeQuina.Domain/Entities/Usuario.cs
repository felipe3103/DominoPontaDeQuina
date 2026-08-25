using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("Usuarios")]
[Index(nameof(Email), IsUnique = true)]
public class Usuario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string HashSenha { get; set; } = string.Empty;

    [Required]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
}
