using System.Security.Cryptography;

namespace PriceCheck.Services;

// Outil pour transformer un mot de passe en empreinte (hash) et pour la vérifier.
// On n'enregistre JAMAIS le mot de passe lui-même dans la base.
public static class MotDePasseHasher
{
    private const int TailleSel = 16;      // octets de "sel" aléatoire
    private const int TailleHash = 32;     // octets de l'empreinte
    private const int Iterations = 100_000; // nombre de répétitions du calcul : ralentit les attaques

    // Crée l'empreinte d'un mot de passe avec un sel tout neuf.
    // Renvoie (hash, sel), tous deux en texte (Base64) pour pouvoir les stocker dans la base.
    public static (string Hash, string Sel) Hacher(string motDePasse)
    {
        byte[] sel = RandomNumberGenerator.GetBytes(TailleSel); // sel aléatoire sûr
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            motDePasse, sel, Iterations, HashAlgorithmName.SHA256, TailleHash);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(sel));
    }

    // Vérifie un mot de passe saisi : on refait le calcul avec le sel stocké
    // et on compare avec l'empreinte stockée.
    public static bool Verifier(string motDePasse, string hashStocke, string selStocke)
    {
        byte[] sel = Convert.FromBase64String(selStocke);
        byte[] attendu = Convert.FromBase64String(hashStocke);

        byte[] calcule = Rfc2898DeriveBytes.Pbkdf2(
            motDePasse, sel, Iterations, HashAlgorithmName.SHA256, TailleHash);

        // Comparaison "à temps constant" : sa durée ne révèle pas à partir de quel octet ça diffère
        return CryptographicOperations.FixedTimeEquals(calcule, attendu);
    }
}