using DominoPontaDeQuina.Application.Services;
using DominoPontaDeQuina.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DominoPontaDeQuina.WebApi.Controllers;

/// <summary>Expõe o cadastro de contas e a emissão de tokens JWT.</summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AutenticacaoController(IAutenticacaoService autenticacao) : ControllerBase
{
    /// <summary>Cadastra uma nova conta de usuário.</summary>
    /// <param name="request">E-mail e senha da conta.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A conta criada, sem o hash da senha.</returns>
    /// <response code="201">Conta cadastrada.</response>
    /// <response code="400">E-mail ou senha fora das regras aceitas.</response>
    /// <response code="409">Já existe uma conta com o e-mail informado.</response>
    [AllowAnonymous]
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Registrar(
        [FromBody] RegistrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await autenticacao.RegistrarAsync(request.Email, request.Senha, cancellationToken);
        return CreatedAtAction(nameof(Eu), UsuarioResponse.De(usuario));
    }

    /// <summary>Autentica uma conta e devolve o token de acesso.</summary>
    /// <param name="request">Credenciais da conta.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O token JWT e seus metadados.</returns>
    /// <response code="200">Autenticado.</response>
    /// <response code="401">Credenciais inválidas.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var token = await autenticacao.AutenticarAsync(request.Email, request.Senha, cancellationToken);
        return Ok(TokenResponse.De(token));
    }

    /// <summary>Devolve a conta associada ao token informado na requisição.</summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A conta autenticada.</returns>
    /// <response code="200">Conta encontrada.</response>
    /// <response code="401">Token ausente, expirado ou inválido.</response>
    /// <response code="404">A conta do token não existe mais.</response>
    [Authorize]
    [HttpGet("eu")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> Eu(CancellationToken cancellationToken)
    {
        var usuarioId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(usuarioId, out var id))
            throw new UnauthorizedAccessException("O token não identifica uma conta válida.");

        var usuario = await autenticacao.ObterPorIdAsync(id, cancellationToken);
        return Ok(UsuarioResponse.De(usuario));
    }
}
