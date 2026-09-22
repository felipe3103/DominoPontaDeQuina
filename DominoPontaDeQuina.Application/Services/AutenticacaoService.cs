using System.Net.Mail;
using DominoPontaDeQuina.Application.Security;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Repositories;

namespace DominoPontaDeQuina.Application.Services;

/// <summary>Implementa o cadastro e a autenticação de contas com emissão de JWT.</summary>
/// <param name="usuarios">Repositório das contas de usuário.</param>
/// <param name="hashDeSenha">Serviço de proteção de senhas.</param>
/// <param name="geradorDeToken">Emissor dos tokens de acesso.</param>
public sealed class AutenticacaoService(
    IUsuarioRepository usuarios,
    IHashDeSenha hashDeSenha,
    IGeradorDeTokenJwt geradorDeToken) : IAutenticacaoService
{
    /// <summary>Tamanho mínimo aceito para a senha.</summary>
    public const int TamanhoMinimoSenha = 8;

    /// <inheritdoc />
    public async Task<Usuario> RegistrarAsync(string email, string senha, CancellationToken cancellationToken = default)
    {
        var emailNormalizado = NormalizarEmail(email);
        ValidarSenha(senha);

        if (await usuarios.EmailExisteAsync(emailNormalizado, cancellationToken))
            throw new InvalidOperationException("Já existe uma conta cadastrada com este e-mail.");

        var usuario = new Usuario
        {
            Email = emailNormalizado,
            SenhaHash = hashDeSenha.Gerar(senha)
        };

        return await usuarios.AdicionarAsync(usuario, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TokenAcesso> AutenticarAsync(string email, string senha, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarios.ObterPorEmailAsync((email ?? string.Empty).Trim().ToLowerInvariant(), cancellationToken);

        // A mesma mensagem é devolvida para conta inexistente e senha errada, para não
        // revelar quais e-mails estão cadastrados.
        if (usuario is null || !hashDeSenha.Conferir(senha ?? string.Empty, usuario.SenhaHash))
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

        return geradorDeToken.Gerar(usuario);
    }

    /// <inheritdoc />
    public async Task<Usuario> ObterPorIdAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        await usuarios.ObterPorIdAsync([usuarioId], cancellationToken)
        ?? throw new KeyNotFoundException($"Conta {usuarioId} não encontrada.");

    static string NormalizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail é obrigatório.", nameof(email));

        var normalizado = email.Trim().ToLowerInvariant();

        if (!MailAddress.TryCreate(normalizado, out _))
            throw new ArgumentException("O e-mail informado não é válido.", nameof(email));

        return normalizado;
    }

    static void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimoSenha)
            throw new ArgumentException($"A senha deve ter ao menos {TamanhoMinimoSenha} caracteres.", nameof(senha));
    }
}
