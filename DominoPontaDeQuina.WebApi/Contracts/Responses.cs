using DominoPontaDeQuina.Application.Security;
using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.WebApi.Contracts;

/// <summary>Representa uma partida devolvida pela API.</summary>
/// <param name="Id">Identificador da partida.</param>
/// <param name="PontuacaoAlvo">Pontuação necessária para vencer.</param>
/// <param name="Status">Status atual da partida.</param>
/// <param name="CriadaEm">Data de criação em UTC.</param>
public sealed record PartidaResponse(Guid Id, int PontuacaoAlvo, string Status, DateTime CriadaEm)
{
    /// <summary>Converte a entidade de domínio no contrato da API.</summary>
    /// <param name="partida">Partida persistida.</param>
    /// <returns>Contrato correspondente.</returns>
    public static PartidaResponse De(Partida partida) =>
        new(partida.Id, partida.PontuacaoAlvo, partida.Status, partida.CriadaEm);
}

/// <summary>Representa um jogador devolvido pela API.</summary>
/// <param name="Id">Identificador do jogador.</param>
/// <param name="Nome">Nome exibido do jogador.</param>
public sealed record JogadorResponse(Guid Id, string Nome)
{
    /// <summary>Converte a entidade de domínio no contrato da API.</summary>
    /// <param name="jogador">Jogador persistido.</param>
    /// <returns>Contrato correspondente.</returns>
    public static JogadorResponse De(Jogador jogador) => new(jogador.Id, jogador.Nome);
}

/// <summary>Representa um lance devolvido pela API.</summary>
/// <param name="Id">Identificador do lance.</param>
/// <param name="Timestamp">Instante do lance em UTC.</param>
public sealed record LanceResponse(Guid Id, DateTime Timestamp)
{
    /// <summary>Converte a entidade de domínio no contrato da API.</summary>
    /// <param name="lance">Lance persistido.</param>
    /// <returns>Contrato correspondente.</returns>
    public static LanceResponse De(Lance lance) => new(lance.Id, lance.Timestamp);
}

/// <summary>Representa uma posição do ranking devolvida pela API.</summary>
/// <param name="JogadorId">Identificador do jogador, quando carregado.</param>
/// <param name="JogadorNome">Nome do jogador, quando carregado.</param>
/// <param name="Vitorias">Total de vitórias acumuladas.</param>
public sealed record RankingResponse(Guid? JogadorId, string? JogadorNome, int Vitorias)
{
    /// <summary>Converte a entidade de domínio no contrato da API.</summary>
    /// <param name="ranking">Posição de ranking persistida.</param>
    /// <returns>Contrato correspondente.</returns>
    public static RankingResponse De(Ranking ranking) =>
        new(ranking.Jogador?.Id, ranking.Jogador?.Nome, ranking.Vitorias);
}

/// <summary>Representa uma conta de usuário devolvida pela API.</summary>
/// <param name="Id">Identificador da conta.</param>
/// <param name="Email">E-mail da conta.</param>
/// <param name="CriadoEm">Data de criação em UTC.</param>
public sealed record UsuarioResponse(Guid Id, string Email, DateTime CriadoEm)
{
    /// <summary>Converte a entidade de domínio no contrato da API.</summary>
    /// <param name="usuario">Conta persistida.</param>
    /// <returns>Contrato correspondente, sem o hash da senha.</returns>
    public static UsuarioResponse De(Usuario usuario) => new(usuario.Id, usuario.Email, usuario.CriadoEm);
}

/// <summary>Representa o token de acesso devolvido no login.</summary>
/// <param name="AccessToken">JWT a enviar em <c>Authorization: Bearer &lt;token&gt;</c>.</param>
/// <param name="TokenType">Esquema de autenticação do token.</param>
/// <param name="ExpiraEm">Instante de expiração em UTC.</param>
/// <param name="UsuarioId">Identificador da conta autenticada.</param>
/// <param name="Email">E-mail da conta autenticada.</param>
public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiraEm,
    Guid UsuarioId,
    string Email)
{
    /// <summary>Converte o token emitido pela aplicação no contrato da API.</summary>
    /// <param name="token">Token emitido.</param>
    /// <returns>Contrato correspondente.</returns>
    public static TokenResponse De(TokenAcesso token) =>
        new(token.Token, "Bearer", token.ExpiraEm, token.UsuarioId, token.Email);
}
