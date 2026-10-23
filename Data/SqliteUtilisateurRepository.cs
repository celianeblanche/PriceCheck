using PriceCheck.Services;
using SQLite;

namespace PriceCheck.Data;

// Implémentation SQLite du contrat IUtilisateurRepository.
public class SqliteUtilisateurRepository : IUtilisateurRepository
{
    private readonly BaseDeDonnees _base;

    // La base est "injectée" : MAUI nous donne celle enregistrée dans MauiProgram.cs
    public SqliteUtilisateurRepository(BaseDeDonnees baseDeDonnees)
    {
        _base = baseDeDonnees;
    }

    // On compare toujours les e-mails en minuscules, sans espaces autour :
    // "Awa@Mail.com " et "awa@mail.com" désignent le même compte.
    private static string Normaliser(string email) => email.Trim().ToLowerInvariant();

    public async Task<bool> CreerCompteAsync(string email, string motDePasse)
    {
        var db = await _base.ObtenirConnexionAsync();
        email = Normaliser(email);

        // Un compte existe déjà avec cet e-mail ?
        var existant = await db.Table<Utilisateur>().Where(u => u.Email == email).FirstOrDefaultAsync();
        if (existant != null)
        {
            return false;
        }

        // On stocke l'empreinte et le sel, jamais le mot de passe
        var (hash, sel) = MotDePasseHasher.Hacher(motDePasse);

        try
        {
            await db.InsertAsync(new Utilisateur
            {
                Nom = email.Split('@')[0], // pas de champ "nom" dans l'écran : on prend ce qui précède le @
                Email = email,
                MotDePasseHash = hash,
                Sel = sel
            });
            return true;
        }
        catch (SQLiteException)
        {
            // Filet de sécurité : la base a refusé (e-mail "Unique" déjà pris entre-temps)
            return false;
        }
    }

    public async Task<Utilisateur?> ConnecterAsync(string email, string motDePasse)
    {
        var db = await _base.ObtenirConnexionAsync();
        email = Normaliser(email);

        var utilisateur = await db.Table<Utilisateur>().Where(u => u.Email == email).FirstOrDefaultAsync();
        if (utilisateur == null)
        {
            return null; // aucun compte avec cet e-mail
        }

        // On refait le calcul de l'empreinte et on compare
        bool correct = MotDePasseHasher.Verifier(motDePasse, utilisateur.MotDePasseHash, utilisateur.Sel);
        return correct ? utilisateur : null;
    }
}