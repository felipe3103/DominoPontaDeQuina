using DominoPontaDeQuina.Application.Security;
using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Application.Services;

/// <summary>Expõe os casos de uso de cadastro e autenticação de contas.</summary>
public interface IAutenticacaoService
{
    /// <summary>Cadastra uma nova conta de usuário.</summary>
    /// <param name="email">E-mail que identifica a conta.</param>
    /// <param name="senha">Senha em texto puro, armazenada apenas como hash.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A conta criada.</returns>
    /// <exception cref="ArgumentException">E-mail ou senha fora das regras aceitas.</exception>
    /// <exception cref="InvalidOperationException">Já existe uma conta com o e-mail informado.</exception>
    Task<Usuario> RegistrarAsync(string email, string senha, CancellationToken cancellationToken = default);

    /// <summary>Autentica uma conta e emite o token de acesso.</summary>
    /// <param name="email">E-mail da conta.</param>
    /// <param name="senha">Senha em texto puro.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O token JWT emitido.</returns>
    /// <exception cref="UnauthorizedAccessException">Credenciais inválidas.</exception>
    Task<TokenAcesso> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default);

    /// <summary>Obtém a conta de um identificador extraído do token.</summary>
    /// <param name="usuarioId">Identificador da conta.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A conta correspondente.</returns>
    /// <exception cref="KeyNotFoundException">A conta não existe mais.</exception>
    Task<Usuario> ObterPorIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
