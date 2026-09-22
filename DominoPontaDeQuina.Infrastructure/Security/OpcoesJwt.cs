using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DominoPontaDeQuina.Infrastructure.Security;

/// <summary>Reúne os parâmetros de emissão e validação dos tokens JWT.</summary>
public sealed class OpcoesJwt
{
    /// <summary>Nome da seção de configuração que alimenta estas opções.</summary>
    public const string Secao = "Jwt";

    /// <summary>Tamanho mínimo da chave para uso seguro com HMAC-SHA256.</summary>
    public const int TamanhoMinimoChaveBytes = 32;

    /// <summary>Obtém ou define quem emite o token.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Obtém ou define para quem o token é destinado.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Obtém ou define a chave simétrica usada na assinatura HMAC-SHA256.</summary>
    public string ChaveSecreta { get; set; } = string.Empty;

    /// <summary>Obtém ou define a validade do token em minutos.</summary>
    public int MinutosDeExpiracao { get; set; } = 60;

    /// <summary>Constrói a chave simétrica de assinatura.</summary>
    /// <returns>A chave usada para assinar e validar os tokens.</returns>
    public SymmetricSecurityKey ChaveDeAssinatura() => new(Encoding.UTF8.GetBytes(ChaveSecreta));

    /// <summary>Garante que a configuração está completa antes de emitir tokens.</summary>
    /// <exception cref="InvalidOperationException">Alguma opção obrigatória está ausente ou inválida.</exception>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException($"'{Secao}:{nameof(Issuer)}' não foi configurado.");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException($"'{Secao}:{nameof(Audience)}' não foi configurado.");

        if (Encoding.UTF8.GetByteCount(ChaveSecreta ?? string.Empty) < TamanhoMinimoChaveBytes)
            throw new InvalidOperationException(
                $"'{Secao}:{nameof(ChaveSecreta)}' precisa de ao menos {TamanhoMinimoChaveBytes} bytes para o HMAC-SHA256.");

        if (MinutosDeExpiracao <= 0)
            throw new InvalidOperationException($"'{Secao}:{nameof(MinutosDeExpiracao)}' deve ser maior que zero.");
    }
}
