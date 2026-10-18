namespace PriceCheck.Services;

// Service SIMULÉ : enregistre un relevé de prix en mémoire seulement.
// Plus tard, cette méthode enverra les données au backend.
public static class ReleveService
{
    public static void Enregistrer(string code, string enseigne, int prix)
    {
        var produit = DonneesSimulees.TrouverProduit(code);
        if (produit == null)
        {
            return; // produit inconnu : rien à faire
        }

        // L'enseigne existe déjà ? (sans tenir compte des majuscules/minuscules)
        int index = produit.Prix.FindIndex(p =>
            string.Equals(p.Enseigne, enseigne, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            // On met à jour son prix et on retire l'alerte "relevé ancien" (le relevé est neuf)
            produit.Prix[index] = (produit.Prix[index].Enseigne, prix, null);
        }
        else
        {
            produit.Prix.Add((enseigne, prix, null)); // nouvelle enseigne
        }
    }
}
