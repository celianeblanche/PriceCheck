namespace PriceCheck.Services;

// Mémorise le dernier produit consulté par la personne connectée,
// pour que l'écran Historique affiche CE produit.
// "Preferences" = petit espace de stockage clé/valeur fourni par MAUI (il survit à la fermeture de l'appli).
public static class DernierProduit
{
    // Produit affiché tant que la personne n'a encore rien consulté (le lait, comme sur la maquette)
    public const string CodeParDefaut = "6001234567890";

    // La clé contient l'e-mail : chaque compte a son propre "dernier produit"
    private static string Cle => "dernier_produit_" + SessionUtilisateur.Email;

    // Code-barres du dernier produit consulté (ou le produit par défaut)
    public static string Code => Preferences.Default.Get(Cle, CodeParDefaut);

    // Appelé par l'écran Résultat quand un produit est affiché
    public static void Enregistrer(string code) => Preferences.Default.Set(Cle, code);
}