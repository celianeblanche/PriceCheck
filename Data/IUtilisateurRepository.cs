namespace PriceCheck.Data;

// Contrat : ce que les pages Connexion et Inscription peuvent demander.
public interface IUtilisateurRepository
{
    // Crée un compte. Renvoie false si cet e-mail est déjà utilisé.
    Task<bool> CreerCompteAsync(string email, string motDePasse);

    // Vérifie e-mail + mot de passe. Renvoie le compte si c'est bon, null sinon.
    Task<Utilisateur?> ConnecterAsync(string email, string motDePasse);
}