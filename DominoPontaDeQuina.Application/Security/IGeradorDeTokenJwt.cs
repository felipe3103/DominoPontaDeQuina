using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Application.Security;

/// <summary>Emite os tokens JWT usados na autenticação da API.</summary>
public interface IGeradorDeTokenJwt
{
    /// <summary>Emite um token de acesso para a conta informada.</summary>
    /// <param name="usuario">Conta já autenticada.</param>
    /// <returns>O token assinado e seus metadados.</returns>
    TokenAcesso Gerar(Usuario usuario);
}
