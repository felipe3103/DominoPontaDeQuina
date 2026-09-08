using DominoPontaDeQuina.Application.Services;
using DominoPontaDeQuina.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DominoPontaDeQuina.WebApi.Controllers;

/// <summary>Expõe as operações de partidas oferecidas por <see cref="IPartidaService"/>.</summary>
[ApiController]
[Route("api/partidas")]
[Produces("application/json")]
public sealed class PartidasController(IPartidaService partidas) : ControllerBase
{
    /// <summary>Inicia uma nova partida.</summary>
    /// <param name="request">Dados da partida a iniciar.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A partida criada.</returns>
    /// <response code="201">Partida iniciada.</response>
    /// <response code="400">Pontuação alvo inválida.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PartidaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PartidaResponse>> IniciarPartida(
        [FromBody] IniciarPartidaRequest request,
        CancellationToken cancellationToken)
    {
        var partida = await partidas.IniciarPartidaAsync(request.PontuacaoAlvo, cancellationToken);
        var resposta = PartidaResponse.De(partida);
        return CreatedAtAction(nameof(VerificarStatus), new { partidaId = resposta.Id }, resposta);
    }

    /// <summary>Consulta o histórico de partidas.</summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Partidas ordenadas da mais recente para a mais antiga.</returns>
    /// <response code="200">Histórico consultado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PartidaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PartidaResponse>>> ConsultarHistorico(
        CancellationToken cancellationToken)
    {
        var historico = await partidas.ConsultarHistoricoAsync(cancellationToken);
        return Ok(historico.Select(PartidaResponse.De).ToList());
    }

    /// <summary>Consulta o status atual de uma partida.</summary>
    /// <param name="partidaId">Identificador da partida.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A partida consultada.</returns>
    /// <response code="200">Partida encontrada.</response>
    /// <response code="404">Partida não encontrada.</response>
    [HttpGet("{partidaId:guid}")]
    [ProducesResponseType(typeof(PartidaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartidaResponse>> VerificarStatus(
        Guid partidaId,
        CancellationToken cancellationToken)
    {
        var partida = await partidas.VerificarStatusAsync(partidaId, cancellationToken);
        return Ok(PartidaResponse.De(partida));
    }

    /// <summary>Registra um jogador e sua participação inicial na partida.</summary>
    /// <param name="partidaId">Identificador da partida.</param>
    /// <param name="request">Dados do jogador.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O jogador registrado.</returns>
    /// <response code="201">Jogador registrado na partida.</response>
    /// <response code="400">Nome do jogador inválido.</response>
    /// <response code="404">Partida não encontrada.</response>
    [HttpPost("{partidaId:guid}/jogadores")]
    [ProducesResponseType(typeof(JogadorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JogadorResponse>> RegistrarJogador(
        Guid partidaId,
        [FromBody] RegistrarJogadorRequest request,
        CancellationToken cancellationToken)
    {
        var jogador = await partidas.RegistrarJogadorAsync(partidaId, request.Nome, request.UsuarioId, cancellationToken);
        return CreatedAtAction(nameof(VerificarStatus), new { partidaId }, JogadorResponse.De(jogador));
    }

    /// <summary>Registra um lance realizado por um jogador na partida.</summary>
    /// <param name="partidaId">Identificador da partida.</param>
    /// <param name="request">Dados do lance.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O lance registrado.</returns>
    /// <response code="201">Lance registrado.</response>
    /// <response code="404">Partida ou jogador não encontrado.</response>
    /// <response code="409">A partida não está em andamento.</response>
    [HttpPost("{partidaId:guid}/lances")]
    [ProducesResponseType(typeof(LanceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LanceResponse>> RegistrarLance(
        Guid partidaId,
        [FromBody] RegistrarLanceRequest request,
        CancellationToken cancellationToken)
    {
        var lance = await partidas.RegistrarLanceAsync(partidaId, request.JogadorId, cancellationToken);
        return CreatedAtAction(nameof(VerificarStatus), new { partidaId }, LanceResponse.De(lance));
    }
}
