using PriceCheck.Models;

namespace PriceCheck.Services;

// Liste d'achats SIMULÉE : gardée en mémoire seulement.
// Classe "static" : une seule liste, partagée par tous les écrans.
public static class ListeCourses
{
    public static List<ArticleListe> Articles { get; } = new();

    // Ajoute un produit au meilleur prix. S'il est déjà dans la liste, on augmente la quantité.
    public static void Ajouter(ProduitSimule produit)
    {
        var meilleur = produit.Prix.OrderBy(p => p.Prix).First(); // le moins cher

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
        => DonneesSimulees.FormaterFcfa(Articles.Sum(a => a.PrixUnitaire * a.Quantite));
}
