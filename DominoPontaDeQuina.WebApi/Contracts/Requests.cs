using System.ComponentModel.DataAnnotations;

namespace DominoPontaDeQuina.WebApi.Contracts;

/// <summary>Dados para iniciar uma nova partida.</summary>
/// <param name="PontuacaoAlvo">Pontuação necessária para vencer a partida.</param>
public sealed record IniciarPartidaRequest(
    [Range(1, int.MaxValue, ErrorMessage = "A pontuação alvo deve ser maior que zero.")]
    int PontuacaoAlvo = 50);

/// <summary>Dados para registrar um jogador em uma partida.</summary>
/// <param name="Nome">Nome exibido do jogador.</param>
/// <param name="UsuarioId">Conta opcional associada ao jogador.</param>
public sealed record RegistrarJogadorRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "O nome do jogador é obrigatório.")]
    [StringLength(150, MinimumLength = 1)]
    string Nome,
    Guid? UsuarioId = null);

/// <summary>Dados para registrar um lance em uma partida.</summary>
/// <param name="JogadorId">Identificador do jogador que realizou o lance.</param>
public sealed record RegistrarLanceRequest(
    [Required]
    Guid JogadorId);

/// <summary>Dados para cadastrar uma nova conta de usuário.</summary>
/// <param name="Email">E-mail que identifica a conta.</param>
/// <param name="Senha">Senha em texto puro, armazenada apenas como hash.</param>
public sealed record RegistrarUsuarioRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
    [StringLength(320)]
    string Email,
    [Required(AllowEmptyStrings = false, ErrorMessage = "A senha é obrigatória.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
    string Senha);

/// <summary>Credenciais para autenticar uma conta e obter o token JWT.</summary>
/// <param name="Email">E-mail da conta.</param>
/// <param name="Senha">Senha em texto puro.</param>
public sealed record LoginRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "O e-mail é obrigatório.")]
    string Email,
    [Required(AllowEmptyStrings = false, ErrorMessage = "A senha é obrigatória.")]
    string Senha);
