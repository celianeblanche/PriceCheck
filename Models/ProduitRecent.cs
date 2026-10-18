namespace PriceCheck.Models;

// Modèle : décrit UN produit affiché dans "Derniers produits".
// SIMULATION : les valeurs viendront du backend plus tard.
public class ProduitRecent
{
    public string Nom { get; set; } = "";        // ex. "Lait entier 1 L"
    public string Enseigne { get; set; } = "";   // ex. "Super Plus"
    public string Prix { get; set; } = "";       // ex. "1 250 FCFA" (texte pour l'instant)
    public string Anciennete { get; set; } = ""; // ex. "il y a 2 j"
}
