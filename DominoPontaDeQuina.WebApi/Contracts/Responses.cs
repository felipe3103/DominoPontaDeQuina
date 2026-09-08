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
