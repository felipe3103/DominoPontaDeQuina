namespace DominoPontaDeQuina.Application.Security;

/// <summary>Representa o token JWT emitido para uma conta autenticada.</summary>
/// <param name="Token">O JWT assinado, usado no cabeçalho <c>Authorization: Bearer</c>.</param>
/// <param name="ExpiraEm">Instante de expiração do token em UTC.</param>
/// <param name="UsuarioId">Identificador da conta autenticada.</param>
/// <param name="Email">E-mail da conta autenticada.</param>
public sealed record TokenAcesso(string Token, DateTime ExpiraEm, Guid UsuarioId, string Email);
