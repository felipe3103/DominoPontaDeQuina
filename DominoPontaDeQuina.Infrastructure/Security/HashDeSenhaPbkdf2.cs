using System.Security.Cryptography;
using DominoPontaDeQuina.Application.Security;

namespace DominoPontaDeQuina.Infrastructure.Security;

/// <summary>
/// Protege senhas com PBKDF2/HMAC-SHA256 e sal aleatório por senha.
/// O hash é persistido como <c>pbkdf2-sha256$iteracoes$sal$hash</c>, formato que
/// mantém os parâmetros junto do valor e permite aumentá-los no futuro.
/// </summary>
public sealed class HashDeSenhaPbkdf2 : IHashDeSenha
{
    const string Prefixo = "pbkdf2-sha256";
    const int Iteracoes = 210_000;
    const int TamanhoSalBytes = 16;
    const int TamanhoHashBytes = 32;

    /// <inheritdoc />
    public string Gerar(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);

        var sal = RandomNumberGenerator.GetBytes(TamanhoSalBytes);
        var hash = Derivar(senha, sal, Iteracoes);

        return string.Join('$', Prefixo, Iteracoes, Convert.ToBase64String(sal), Convert.ToBase64String(hash));
    }

    /// <inheritdoc />
    public bool Conferir(string senha, string hashPersistido)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(hashPersistido))
            return false;

        var partes = hashPersistido.Split('$');
        if (partes.Length != 4 || partes[0] != Prefixo || !int.TryParse(partes[1], out var iteracoes))
            return false;

        byte[] sal, esperado;
        try
        {
            sal = Convert.FromBase64String(partes[2]);
            esperado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

        // Comparação em tempo fixo: evita deduzir o hash pelo tempo de resposta.
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }

    static byte[] Derivar(string senha, byte[] sal, int iteracoes) =>
        Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, TamanhoHashBytes);
}
