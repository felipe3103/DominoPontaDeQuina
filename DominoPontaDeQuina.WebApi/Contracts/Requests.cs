using System.ComponentModel.DataAnnotations;

namespace DominoPontaDeQuina.WebApi.Contracts;

/// <summary>Dados para iniciar uma nova partida.</summary>
/// <param name="PontuacaoAlvo">Pontuação necessária para vencer a partida.</param>
public sealed record IniciarPartidaRequest(
    [property: Range(1, int.MaxValue, ErrorMessage = "A pontuação alvo deve ser maior que zero.")]
    int PontuacaoAlvo = 50);

/// <summary>Dados para registrar um jogador em uma partida.</summary>
/// <param name="Nome">Nome exibido do jogador.</param>
/// <param name="UsuarioId">Conta opcional associada ao jogador.</param>
public sealed record RegistrarJogadorRequest(
    [property: Required(AllowEmptyStrings = false, ErrorMessage = "O nome do jogador é obrigatório.")]
    [property: StringLength(150, MinimumLength = 1)]
    string Nome,
    Guid? UsuarioId = null);

/// <summary>Dados para registrar um lance em uma partida.</summary>
/// <param name="JogadorId">Identificador do jogador que realizou o lance.</param>
public sealed record RegistrarLanceRequest(
    [property: Required]
    Guid JogadorId);
