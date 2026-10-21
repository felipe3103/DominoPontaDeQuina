using DominoPontaDeQuina.Core.Models;
using DominoPontaDeQuina.Core.Regras;

namespace DominoPontaDeQuina.UnitTests.Regras;

public class RegraPontuacaoTests
{
    [Fact]
    public void CalcularPontos_Soma10_Retorna2()
    {
        // Arrange
        const int soma = 10;

        // Act
        var pontos = RegraPontuacao.CalcularPontos(soma);

        // Assert
        pontos.Should().Be(2);
    }

    [Fact]
    public void CalcularPontos_Soma15_Retorna3()
    {
        // Arrange
        const int soma = 15;

        // Act
        var pontos = RegraPontuacao.CalcularPontos(soma);

        // Assert
        pontos.Should().Be(3);
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(10, 2)]
    [InlineData(15, 3)]
    [InlineData(20, 4)]
    [InlineData(25, 5)]
    public void CalcularPontos_SomaMultiplaDe5_RetornaSomaDivididaPor5(int soma, int pontosEsperados)
    {
        // Arrange (dados vindos do InlineData)

        // Act
        var pontos = RegraPontuacao.CalcularPontos(soma);

        // Assert
        pontos.Should().Be(pontosEsperados);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(11)]
    [InlineData(12)]
    public void CalcularPontos_SomaNaoMultiplaDe5_RetornaZero(int soma)
    {
        // Arrange (dados vindos do InlineData)

        // Act
        var pontos = RegraPontuacao.CalcularPontos(soma);

        // Assert
        pontos.Should().Be(0);
    }

    [Fact]
    public void CalcularPontos_SomaZero_RetornaZero()
    {
        // Arrange
        const int soma = 0;

        // Act
        var pontos = RegraPontuacao.CalcularPontos(soma);

        // Assert
        pontos.Should().Be(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void CalcularPontos_SomaNegativa_LancaArgumentOutOfRangeException(int soma)
    {
        // Arrange
        var acao = () => RegraPontuacao.CalcularPontos(soma);

        // Act + Assert
        acao.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("somaDasPontas");
    }

    [Fact]
    public void CalcularPontos_TabuleiroComPontasSomando10_Retorna2()
    {
        // Arrange
        var tabuleiro = new Tabuleiro();
        tabuleiro.Pecas.Add(new Peca(6, 3));
        tabuleiro.Pecas.Add(new Peca(3, 4));

        // Act
        var pontos = RegraPontuacao.CalcularPontos(tabuleiro);

        // Assert
        tabuleiro.SomarPontasExternas().Should().Be(10);
        pontos.Should().Be(2);
    }

    [Fact]
    public void CalcularPontos_TabuleiroComPontasNaoMultiplas_RetornaZero()
    {
        // Arrange
        var tabuleiro = new Tabuleiro();
        tabuleiro.Pecas.Add(new Peca(6, 2));
        tabuleiro.Pecas.Add(new Peca(2, 1));

        // Act
        var pontos = RegraPontuacao.CalcularPontos(tabuleiro);

        // Assert
        pontos.Should().Be(0);
    }

    [Fact]
    public void CalcularPontos_TabuleiroVazio_RetornaZero()
    {
        // Arrange
        var tabuleiro = new Tabuleiro();

        // Act
        var pontos = RegraPontuacao.CalcularPontos(tabuleiro);

        // Assert
        pontos.Should().Be(0);
    }

    [Fact]
    public void CalcularPontos_TabuleiroNulo_LancaArgumentNullException()
    {
        // Arrange
        Tabuleiro tabuleiro = null!;
        var acao = () => RegraPontuacao.CalcularPontos(tabuleiro);

        // Act + Assert
        acao.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("tabuleiro");
    }
}
