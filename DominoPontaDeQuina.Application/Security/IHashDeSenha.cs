namespace DominoPontaDeQuina.Application.Security;

/// <summary>Protege e confere senhas sem nunca armazená-las em texto puro.</summary>
public interface IHashDeSenha
{
    /// <summary>Gera o hash de uma senha em texto puro.</summary>
    /// <param name="senha">Senha informada pelo usuário.</param>
    /// <returns>O hash a ser persistido.</returns>
    string Gerar(string senha);

    /// <summary>Confere uma senha contra o hash persistido.</summary>
    /// <param name="senha">Senha informada pelo usuário.</param>
    /// <param name="hashPersistido">Hash gravado no banco.</param>
    /// <returns><see langword="true"/> quando a senha confere.</returns>
    bool Conferir(string senha, string hashPersistido);
}
