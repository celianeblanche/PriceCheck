namespace PriceCheck.Services;

// Mémorise QUI est connecté, même après la fermeture de l'appli.
// "Preferences" = petit espace de stockage clé/valeur fourni par MAUI.
public static class SessionUtilisateur
{
    private const string CleEmail = "utilisateur_email";

    // E-mail de la personne connectée ("" si personne)
    public static string Email => Preferences.Default.Get(CleEmail, "");

    public static bool EstConnecte => Email.Length > 0;

    // Appelé après une connexion ou une inscription réussie
    public static void Ouvrir(string email) => Preferences.Default.Set(CleEmail, email);

    // Pour une future déconnexion
    public static void Fermer() => Preferences.Default.Remove(CleEmail);
}