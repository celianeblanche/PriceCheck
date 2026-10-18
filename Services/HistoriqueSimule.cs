using PriceCheck.Models;
using Microsoft.Maui.Graphics;

namespace PriceCheck.Services;

// Historique SIMULÉ d'un produit (le lait). Remplacé par le backend plus tard.
public static class HistoriqueSimule
{
    // Produit dont on affiche l'historique
    public const string CodeProduit = "6001234567890";

    // Relevés du plus récent au plus ancien : (date, prix en FCFA)
    private static readonly List<(DateTime Date, int Prix)> Releves = new()
    {
        (new DateTime(2026, 10, 5), 1250),
        (new DateTime(2026, 9, 21), 1300),
        (new DateTime(2026, 9, 7),  1200),
        (new DateTime(2026, 8, 24), 1200),
    };

    private static readonly string[] Mois =
        { "janv.", "févr.", "mars", "avr.", "mai", "juin", "juil.", "août", "sept.", "oct.", "nov.", "déc." };

    public static string NomProduit => DonneesSimulees.TrouverProduit(CodeProduit)?.Nom ?? "";

    // Statistiques calculées à partir des relevés
    public static int Min => Releves.Min(r => r.Prix);
    public static int Max => Releves.Max(r => r.Prix);
    public static int Moyen => (int)Math.Round(Releves.Average(r => r.Prix), MidpointRounding.AwayFromZero);

    // 1250 -> "1 250" (sans l'unité)
    public static string FormaterNombre(int n)
        => n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', ' ');

    // Prépare les lignes à afficher, avec la variation par rapport au relevé précédent
    public static List<LigneReleve> ConstruireLignes()
    {
        var gris = Color.FromArgb("#475569");
        var vert = Color.FromArgb("#15803D");    // baisse de prix = bonne nouvelle
        var orange = Color.FromArgb("#B45309");  // hausse de prix

        var lignes = new List<LigneReleve>();
        for (int i = 0; i < Releves.Count; i++)
        {
            var r = Releves[i];
            string variation;
            Color couleur = gris;

            if (i == Releves.Count - 1)
            {
                variation = "Premier relevé"; // le plus ancien : rien à comparer
            }
            else
            {
                int ecart = r.Prix - Releves[i + 1].Prix; // différence avec le relevé plus ancien
                if (ecart == 0) { variation = "Stable"; }
                else if (ecart > 0) { variation = "+" + ecart; couleur = orange; }
                else { variation = "−" + Math.Abs(ecart); couleur = vert; }
            }

            lignes.Add(new LigneReleve
            {
                DateTexte = $"{r.Date.Day} {Mois[r.Date.Month - 1]} {r.Date.Year}",
                PrixTexte = DonneesSimulees.FormaterFcfa(r.Prix),
                VariationTexte = variation,
                CouleurVariation = couleur
            });
        }
        return lignes;
    }
}
