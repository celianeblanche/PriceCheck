using System.Globalization;
using PriceCheck.Data;

namespace PriceCheck.Services;

// Parcours "produit inconnu" : propose de l'ajouter (nom, enseigne, prix), puis le crée dans la base.
// Utilisé par l'écran Scanner ET par l'écran Accueil.
public static class AjoutProduitService
{
    // "page" = l'écran qui affiche les fenêtres de saisie.
    // Renvoie true si le produit a été créé, false si la personne annule ou se trompe.
    public static async Task<bool> ProposerAjoutAsync(Page page, string code)
    {
        bool veutAjouter = await page.DisplayAlertAsync("Produit inconnu",
            $"Le code-barres {code} n'est pas encore dans la base. Voulez-vous l'ajouter ?",
            "Ajouter", "Annuler");
        if (!veutAjouter)
        {
            return false;
        }

        // DisplayPromptAsync : petite fenêtre avec un champ de saisie. Renvoie null si on annule.
        string? nom = await page.DisplayPromptAsync("Nouveau produit", "Nom du produit (ex. Lait entier 1 L)",
            "Suivant", "Annuler", maxLength: 60);
        if (string.IsNullOrWhiteSpace(nom))
        {
            return false;
        }

        string? enseigne = await page.DisplayPromptAsync("Nouveau produit", "Enseigne où vous l'avez vu (ex. Super Plus)",
            "Suivant", "Annuler", maxLength: 40);
        if (string.IsNullOrWhiteSpace(enseigne))
        {
            return false;
        }

        string? prixTexte = await page.DisplayPromptAsync("Nouveau produit", "Prix en FCFA (chiffres seulement)",
            "Ajouter", "Annuler", keyboard: Keyboard.Numeric, maxLength: 9);
        if (prixTexte == null)
        {
            return false; // annulé
        }

        if (!TryLirePrix(prixTexte, out int prix))
        {
            await page.DisplayAlertAsync("Prix invalide", "Saisissez un prix en chiffres, par exemple 1250.", "OK");
            return false;
        }

        // Création RÉELLE dans la base : le produit et son premier relevé de prix
        var depot = ServiceHelper.Obtenir<IProduitRepository>();
        bool cree = await depot.CreerProduitAsync(code, nom.Trim(), enseigne.Trim(), prix);
        if (!cree)
        {
            await page.DisplayAlertAsync("Ajout impossible", "Ce produit existe déjà.", "OK");
        }
        return cree;
    }

    // Convertit le texte saisi en nombre. Accepte "1250" et "1 250".
    // Renvoie true si c'est un entier strictement positif ; le résultat est placé dans "prix".
    private static bool TryLirePrix(string? texte, out int prix)
    {
        string nettoye = (texte ?? "").Replace(" ", "").Replace("\u00A0", "").Replace("\u202F", "");
        return int.TryParse(nettoye, NumberStyles.None, CultureInfo.InvariantCulture, out prix) && prix > 0;
    }
}