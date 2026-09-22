using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Domain.Repositories;

/// <summary>Define as operações de persistência das contas de usuário.</summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Obtém a conta associada a um e-mail.</summary>
    /// <param name="email">E-mail da conta, comparado sem diferenciar maiúsculas.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A conta ou <see langword="null"/> se não existir.</returns>
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Indica se já existe uma conta com o e-mail informado.</summary>
    /// <param name="email">E-mail a verificar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns><see langword="true"/> quando o e-mail já está em uso.</returns>
    Task<bool> EmailExisteAsync(string email, CancellationToken cancellationToken = default);
}
