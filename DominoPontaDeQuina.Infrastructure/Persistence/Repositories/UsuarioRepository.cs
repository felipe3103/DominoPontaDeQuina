using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Infrastructure.Persistence.Repositories;

/// <summary>Implementa a persistência das contas de usuário com o <see cref="DominoDbContext"/>.</summary>
public sealed class UsuarioRepository : EfRepository<Usuario>, IUsuarioRepository
{
    readonly DominoDbContext db;

    /// <summary>Inicializa o repositório de contas de usuário.</summary>
    /// <param name="db">Contexto do banco de dados.</param>
    public UsuarioRepository(DominoDbContext db) : base(db) => this.db = db;

    /// <inheritdoc />
    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.Usuarios.AsNoTracking().FirstOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);

    /// <inheritdoc />
    public Task<bool> EmailExisteAsync(string email, CancellationToken cancellationToken = default) =>
        db.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken);
}
