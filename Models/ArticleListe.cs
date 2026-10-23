using PriceCheck.Services;

namespace PriceCheck.Models;

// Modèle : un produit dans la liste d'achats.
public class ArticleListe
{
    public string Code { get; set; } = "";        // code-barres (sert à rouvrir le Résultat)
    public string Nom { get; set; } = "";
    public string Enseigne { get; set; } = "";    // enseigne du meilleur prix au moment de l'ajout
    public int PrixUnitaire { get; set; }         // prix en FCFA
    public int Quantite { get; set; } = 1;

    // Propriétés calculées, utilisées par l'écran
    public string QuantiteTexte => $"Qté {Quantite}";
    public string PrixTexte => Formatage.FormaterFcfa(PrixUnitaire * Quantite); // changé : Formatage
}