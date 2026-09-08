using DominoPontaDeQuina.Application.Services;
using DominoPontaDeQuina.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DominoPontaDeQuina.WebApi.Controllers;

/// <summary>Expõe a consulta de ranking oferecida por <see cref="IPartidaService"/>.</summary>
[ApiController]
[Route("api/ranking")]
[Produces("application/json")]
public sealed class RankingController(IPartidaService partidas) : ControllerBase
{
    /// <summary>Consulta o ranking de jogadores por vitórias.</summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Ranking ordenado por vitórias.</returns>
    /// <response code="200">Ranking consultado.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RankingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RankingResponse>>> ConsultarRanking(
        CancellationToken cancellationToken)
    {
        var ranking = await partidas.ConsultarRankingAsync(cancellationToken);
        return Ok(ranking.Select(RankingResponse.De).ToList());
    }
}
