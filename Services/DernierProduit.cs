namespace PriceCheck.Services;

// Mémorise le dernier produit consulté par la personne connectée,
// pour que l'écran Historique affiche CE produit.
// "Preferences" = petit espace de stockage clé/valeur fourni par MAUI (il survit à la fermeture de l'appli).
public static class DernierProduit
{
    // La clé contient l'e-mail : chaque compte a son propre "dernier produit"
    private static string Cle => "dernier_produit_" + SessionUtilisateur.Email;

    // MODIFIÉ : code-barres du dernier produit consulté, ou "" si la personne n'a encore rien consulté
    // (avant, on renvoyait le lait par défaut, ce qui déroutait les nouveaux utilisateurs)
    public static string Code => Preferences.Default.Get(Cle, "");

    // NOUVEAU : vrai si la personne a déjà consulté au moins un produit
    public static bool Existe => Code.Length > 0;

    // Appelé par l'écran Résultat quand un produit est affiché
    public static void Enregistrer(string code) => Preferences.Default.Set(Cle, code);
}