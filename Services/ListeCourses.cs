using PriceCheck.Models;

namespace PriceCheck.Services;

// Liste d'achats : gardée en mémoire seulement (pas encore dans la base).
// Classe "static" : une seule liste, partagée par tous les écrans.
public static class ListeCourses
{
    public static List<ArticleListe> Articles { get; } = new();

    // Ajoute un produit au meilleur prix. S'il est déjà dans la liste, on augmente la quantité.
    public static void Ajouter(ProduitDetail produit)
    {
        var meilleur = produit.MeilleurPrix; // le moins cher
        if (meilleur == null)
        {
            return; // produit sans prix : rien à ajouter
        }

        var existant = Articles.FirstOrDefault(a => a.Code == produit.Code);
        if (existant != null)
        {
            existant.Quantite++;
            existant.PrixUnitaire = meilleur.Prix;
            existant.Enseigne = meilleur.Enseigne;
            return;
        }

        Articles.Add(new ArticleListe
        {
            Code = produit.Code,
            Nom = produit.Nom,
            Enseigne = meilleur.Enseigne,
            PrixUnitaire = meilleur.Prix,
            Quantite = 1
        });
    }

    // Total de la liste (prix x quantité), au format "3 950 FCFA"
    public static string TotalTexte()
        => Formatage.FormaterFcfa(Articles.Sum(a => a.PrixUnitaire * a.Quantite));
}