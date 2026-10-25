namespace PriceCheck.Models;

// Modèle : décrit UN produit affiché dans "Derniers produits" et dans les résultats de recherche.
public class ProduitRecent
{
    public string Code { get; set; } = "";       // NOUVEAU : code-barres, sert à ouvrir le Résultat quand on touche la carte
    public string Nom { get; set; } = "";        // ex. "Lait entier 1 L"
    public string Enseigne { get; set; } = "";   // ex. "Super Plus"
    public string Prix { get; set; } = "";       // ex. "1 250 FCFA" (texte)
    public string Anciennete { get; set; } = ""; // ex. "il y a 2 j"
}