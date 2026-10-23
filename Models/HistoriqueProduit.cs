using Microsoft.Maui.Graphics;
using PriceCheck.Services;

namespace PriceCheck.Models;

// Tout ce dont l'écran Historique a besoin : nom, statistiques et lignes de relevés.
public class HistoriqueProduit
{
    public string Code { get; set; } = "";
    public string NomProduit { get; set; } = "";
    public int Min { get; set; }
    public int Moyen { get; set; }
    public int Max { get; set; }
    public List<LigneReleve> Lignes { get; set; } = new();

    // "releves" doit être trié du plus récent au plus ancien.
    public static HistoriqueProduit Construire(string code, string nom, List<(DateTime Date, int Prix)> releves)
    {
        var historique = new HistoriqueProduit { Code = code, NomProduit = nom };
        if (releves.Count == 0)
        {
            return historique; // aucun relevé : statistiques à 0, liste vide
        }

        historique.Min = releves.Min(r => r.Prix);
        historique.Max = releves.Max(r => r.Prix);
        historique.Moyen = (int)Math.Round(releves.Average(r => r.Prix), MidpointRounding.AwayFromZero);

        var gris = Color.FromArgb("#475569");
        var vert = Color.FromArgb("#15803D");    // baisse de prix = bonne nouvelle
        var orange = Color.FromArgb("#B45309");  // hausse de prix

        for (int i = 0; i < releves.Count; i++)
        {
            var r = releves[i];
            string variation;
            Color couleur = gris;

            if (i == releves.Count - 1)
            {
                variation = "Premier relevé"; // le plus ancien : rien à comparer
            }
            else
            {
                int ecart = r.Prix - releves[i + 1].Prix; // différence avec le relevé plus ancien
                if (ecart == 0) { variation = "Stable"; }
                else if (ecart > 0) { variation = "+" + ecart; couleur = orange; }
                else { variation = "−" + Math.Abs(ecart); couleur = vert; }
            }

            historique.Lignes.Add(new LigneReleve
            {
                DateTexte = Formatage.FormaterDate(r.Date),
                PrixTexte = Formatage.FormaterFcfa(r.Prix),
                VariationTexte = variation,
                CouleurVariation = couleur
            });
        }
        return historique;
    }
}