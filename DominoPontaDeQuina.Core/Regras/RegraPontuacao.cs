using DominoPontaDeQuina.Core.Models;

namespace DominoPontaDeQuina.Core.Regras;

/// <summary>
/// Concentra a regra de pontuacao do Domino Ponta de Quina.
/// Depois de cada jogada, a soma das pontas externas do tabuleiro e verificada:
/// quando ela for multiplo de 5, o time do jogador recebe <c>soma / 5</c> pontos.
/// </summary>
public static class RegraPontuacao
{
    /// <summary>
    /// Valor do qual a soma das pontas precisa ser multipla para gerar pontos.
    /// </summary>
    public const int Divisor = 5;

    /// <summary>
    /// Calcula os pontos gerados por uma soma de pontas externas.
    /// </summary>
    /// <param name="somaDasPontas">A soma das pontas externas do tabuleiro.</param>
    /// <returns><c>soma / 5</c> quando a soma for multipla de 5; caso contrario, 0.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Quando a soma for negativa.</exception>
    public static int CalcularPontos(int somaDasPontas)
    {
        if (somaDasPontas < 0)
            throw new ArgumentOutOfRangeException(nameof(somaDasPontas), somaDasPontas, "A soma das pontas nao pode ser negativa.");

        return somaDasPontas % Divisor == 0 ? somaDasPontas / Divisor : 0;
    }

    /// <summary>
    /// Calcula os pontos gerados pelas pontas externas atualmente expostas no tabuleiro.
    /// </summary>
    /// <param name="tabuleiro">O tabuleiro da rodada.</param>
    /// <returns>Os pontos gerados pela soma das pontas externas.</returns>
    /// <exception cref="ArgumentNullException">Quando o tabuleiro for nulo.</exception>
    public static int CalcularPontos(Tabuleiro tabuleiro)
    {
        ArgumentNullException.ThrowIfNull(tabuleiro);
        return CalcularPontos(tabuleiro.SomarPontasExternas());
    }
}
