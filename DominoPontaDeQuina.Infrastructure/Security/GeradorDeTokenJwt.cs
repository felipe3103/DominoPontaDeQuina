using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DominoPontaDeQuina.Application.Security;
using DominoPontaDeQuina.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DominoPontaDeQuina.Infrastructure.Security;

/// <summary>Emite tokens JWT assinados com HMAC-SHA256 a partir de <see cref="OpcoesJwt"/>.</summary>
/// <param name="opcoes">Opções de emissão configuradas na seção <c>Jwt</c>.</param>
public sealed class GeradorDeTokenJwt(IOptions<OpcoesJwt> opcoes) : IGeradorDeTokenJwt
{
    readonly OpcoesJwt opcoes = opcoes.Value;

    /// <inheritdoc />
    public TokenAcesso Gerar(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        opcoes.Validar();

        var emitidoEm = DateTime.UtcNow;
        var expiraEm = emitidoEm.AddMinutes(opcoes.MinutosDeExpiracao);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            // jti identifica cada token individualmente, o que permite revogá-los no futuro.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: opcoes.Issuer,
            audience: opcoes.Audience,
            claims: claims,
            notBefore: emitidoEm,
            expires: expiraEm,
            signingCredentials: new SigningCredentials(opcoes.ChaveDeAssinatura(), SecurityAlgorithms.HmacSha256));

        return new TokenAcesso(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiraEm,
            usuario.Id,
            usuario.Email);
    }
}
