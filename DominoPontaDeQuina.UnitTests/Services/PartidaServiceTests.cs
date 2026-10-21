using DominoPontaDeQuina.Application.Services;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Repositories;

namespace DominoPontaDeQuina.UnitTests.Services;

/// <summary>
/// Testa as decisoes do <see cref="PartidaService"/> sobre quando consultar e quando salvar,
/// com os repositorios substituidos por mocks do Moq.
/// O xUnit cria uma instancia da classe por teste, entao cada teste recebe mocks novos.
/// </summary>
public class PartidaServiceTests
{
    readonly Mock<IPartidaRepository> _partidas = new();
    readonly Mock<IJogadorRepository> _jogadores = new();
    readonly Mock<IParticipacaoRepository> _participacoes = new();
    readonly Mock<ILanceRepository> _lances = new();
    readonly Mock<IRankingRepository> _ranking = new();
    readonly PartidaService _service;

    public PartidaServiceTests()
    {
        _service = new PartidaService(
            _partidas.Object,
            _jogadores.Object,
            _participacoes.Object,
            _lances.Object,
            _ranking.Object);
    }

    static Partida CriarPartida(string status = "EmAndamento") =>
        new() { Id = Guid.NewGuid(), PontuacaoAlvo = 50, Status = status };

    // ---------- IniciarPartidaAsync ----------

    [Fact]
    public async Task IniciarPartida_PontuacaoValida_SalvaPartidaEmAndamento()
    {
        // Arrange
        _partidas
            .Setup(r => r.AdicionarAsync(It.IsAny<Partida>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partida p, CancellationToken _) => p);

        // Act
        var partida = await _service.IniciarPartidaAsync(100);

        // Assert
        partida.PontuacaoAlvo.Should().Be(100);
        partida.Status.Should().Be("EmAndamento");
        partida.Id.Should().NotBeEmpty();
        _partidas.Verify(r => r.AdicionarAsync(
            It.Is<Partida>(p => p.PontuacaoAlvo == 100 && p.Status == "EmAndamento"),
            It.IsAny<CancellationToken>()), Times.Once);
        _partidas.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task IniciarPartida_PontuacaoInvalida_LancaExcecaoENaoSalva(int pontuacaoAlvo)
    {
        // Arrange
        var acao = () => _service.IniciarPartidaAsync(pontuacaoAlvo);

        // Act + Assert
        (await acao.Should().ThrowAsync<ArgumentOutOfRangeException>())
            .Which.ParamName.Should().Be("pontuacaoAlvo");
        _partidas.Verify(r => r.AdicionarAsync(It.IsAny<Partida>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- RegistrarJogadorAsync ----------

    [Fact]
    public async Task RegistrarJogador_PartidaExistente_ConsultaPartidaESalvaJogadorEParticipacao()
    {
        // Arrange
        var partida = CriarPartida();
        var usuarioId = Guid.NewGuid();
        _partidas.Setup(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>())).ReturnsAsync(partida);
        _jogadores
            .Setup(r => r.AdicionarComUsuarioAsync(It.IsAny<Jogador>(), usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Jogador j, Guid? _, CancellationToken _) => j);

        // Act
        var jogador = await _service.RegistrarJogadorAsync(partida.Id, "Ana", usuarioId);

        // Assert
        jogador.Nome.Should().Be("Ana");
        _partidas.Verify(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>()), Times.Once);
        _jogadores.Verify(r => r.AdicionarComUsuarioAsync(
            It.Is<Jogador>(j => j.Nome == "Ana"), usuarioId, It.IsAny<CancellationToken>()), Times.Once);
        _participacoes.Verify(r => r.AdicionarNaPartidaAsync(
            It.Is<ParticipacaoPartida>(p => p.Pontos == 0), partida.Id, jogador.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarJogador_PartidaInexistente_LancaKeyNotFoundENaoSalva()
    {
        // Arrange
        var partidaId = Guid.NewGuid();
        _partidas.Setup(r => r.ObterAsync(partidaId, It.IsAny<CancellationToken>())).ReturnsAsync((Partida?)null);
        var acao = () => _service.RegistrarJogadorAsync(partidaId, "Ana");

        // Act + Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Partida não encontrada.");
        _partidas.Verify(r => r.ObterAsync(partidaId, It.IsAny<CancellationToken>()), Times.Once);
        _jogadores.VerifyNoOtherCalls();
        _participacoes.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegistrarJogador_NomeInvalido_LancaArgumentExceptionSemConsultarNemSalvar(string? nome)
    {
        // Arrange
        var acao = () => _service.RegistrarJogadorAsync(Guid.NewGuid(), nome!);

        // Act + Assert
        (await acao.Should().ThrowAsync<ArgumentException>())
            .Which.ParamName.Should().Be("nome");
        _partidas.VerifyNoOtherCalls();
        _jogadores.VerifyNoOtherCalls();
        _participacoes.VerifyNoOtherCalls();
    }

    // ---------- RegistrarLanceAsync ----------

    [Fact]
    public async Task RegistrarLance_PartidaEmAndamentoEJogadorExistente_SalvaLance()
    {
        // Arrange
        var partida = CriarPartida();
        var jogadorId = Guid.NewGuid();
        _partidas.Setup(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>())).ReturnsAsync(partida);
        _jogadores.Setup(r => r.ExisteAsync(jogadorId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _lances
            .Setup(r => r.AdicionarNaPartidaAsync(It.IsAny<Lance>(), partida.Id, jogadorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lance l, Guid _, Guid _, CancellationToken _) => l);

        // Act
        var lance = await _service.RegistrarLanceAsync(partida.Id, jogadorId);

        // Assert
        lance.Id.Should().NotBeEmpty();
        _jogadores.Verify(r => r.ExisteAsync(jogadorId, It.IsAny<CancellationToken>()), Times.Once);
        _lances.Verify(r => r.AdicionarNaPartidaAsync(
            It.IsAny<Lance>(), partida.Id, jogadorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarLance_PartidaFinalizada_LancaInvalidOperationSemConsultarJogadorNemSalvar()
    {
        // Arrange
        var partida = CriarPartida(status: "Finalizada");
        _partidas.Setup(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>())).ReturnsAsync(partida);
        var acao = () => _service.RegistrarLanceAsync(partida.Id, Guid.NewGuid());

        // Act + Assert
        await acao.Should().ThrowAsync<InvalidOperationException>().WithMessage("A partida não está em andamento.");
        _jogadores.VerifyNoOtherCalls();
        _lances.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegistrarLance_JogadorInexistente_LancaKeyNotFoundENaoSalva()
    {
        // Arrange
        var partida = CriarPartida();
        var jogadorId = Guid.NewGuid();
        _partidas.Setup(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>())).ReturnsAsync(partida);
        _jogadores.Setup(r => r.ExisteAsync(jogadorId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var acao = () => _service.RegistrarLanceAsync(partida.Id, jogadorId);

        // Act + Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Jogador não encontrado.");
        _jogadores.Verify(r => r.ExisteAsync(jogadorId, It.IsAny<CancellationToken>()), Times.Once);
        _lances.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegistrarLance_PartidaInexistente_LancaKeyNotFoundSemConsultarJogador()
    {
        // Arrange
        var partidaId = Guid.NewGuid();
        _partidas.Setup(r => r.ObterAsync(partidaId, It.IsAny<CancellationToken>())).ReturnsAsync((Partida?)null);
        var acao = () => _service.RegistrarLanceAsync(partidaId, Guid.NewGuid());

        // Act + Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Partida não encontrada.");
        _jogadores.VerifyNoOtherCalls();
        _lances.VerifyNoOtherCalls();
    }

    // ---------- Consultas ----------

    [Fact]
    public async Task VerificarStatus_PartidaExistente_RetornaPartidaSemSalvar()
    {
        // Arrange
        var partida = CriarPartida();
        _partidas.Setup(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>())).ReturnsAsync(partida);

        // Act
        var resultado = await _service.VerificarStatusAsync(partida.Id);

        // Assert
        resultado.Should().BeSameAs(partida);
        _partidas.Verify(r => r.ObterAsync(partida.Id, It.IsAny<CancellationToken>()), Times.Once);
        _partidas.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VerificarStatus_PartidaInexistente_LancaKeyNotFoundException()
    {
        // Arrange
        _partidas.Setup(r => r.ObterAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Partida?)null);
        var acao = () => _service.VerificarStatusAsync(Guid.NewGuid());

        // Act + Assert
        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ConsultarRanking_RetornaRankingDoRepositorio()
    {
        // Arrange
        IReadOnlyList<Ranking> esperado = [new Ranking(), new Ranking()];
        _ranking.Setup(r => r.ObterAsync(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        // Act
        var resultado = await _service.ConsultarRankingAsync();

        // Assert
        resultado.Should().BeSameAs(esperado);
        _ranking.Verify(r => r.ObterAsync(It.IsAny<CancellationToken>()), Times.Once);
        _ranking.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConsultarHistorico_RetornaHistoricoDoRepositorioSemSalvar()
    {
        // Arrange
        IReadOnlyList<Partida> esperado = [CriarPartida(), CriarPartida("Finalizada")];
        _partidas.Setup(r => r.ObterHistoricoAsync(It.IsAny<CancellationToken>())).ReturnsAsync(esperado);

        // Act
        var resultado = await _service.ConsultarHistoricoAsync();

        // Assert
        resultado.Should().HaveCount(2).And.BeSameAs(esperado);
        _partidas.Verify(r => r.ObterHistoricoAsync(It.IsAny<CancellationToken>()), Times.Once);
        _partidas.VerifyNoOtherCalls();
    }
}
