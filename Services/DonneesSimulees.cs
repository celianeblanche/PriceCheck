using PriceCheck.Models;

namespace PriceCheck.Services;

// Un produit avec ses prix par enseigne. SIMULATION : remplacé par le backend plus tard.
public class ProduitSimule
{
    public string Nom { get; set; } = "";
    public string Code { get; set; } = "";

    // Liste de prix : (enseigne, prix en FCFA, alerte éventuelle)
    public List<(string Enseigne, int Prix, string? Alerte)> Prix { get; set; } = new();

    // Code affiché comme dans la maquette : 6 001234 567890
    public string CodeAffiche => Code.Length == 13
        ? $"{Code[0]} {Code.Substring(1, 6)} {Code.Substring(7)}"
        : Code;

    // Trie les prix du moins cher au plus cher et prépare les lignes à afficher.
    public List<LignePrix> ConstruireLignes()
    {
        var tries = Prix.OrderBy(p => p.Prix).ToList();
        int reference = tries[0].Prix; // le moins cher sert de référence pour les écarts
        var lignes = new List<LignePrix>();

        for (int i = 0; i < tries.Count; i++)
        {
            var p = tries[i];
            lignes.Add(new LignePrix
            {
                Enseigne = p.Enseigne,
                PrixTexte = DonneesSimulees.FormaterFcfa(p.Prix),
                EcartTexte = i == 0 ? "Référence" : "+" + (p.Prix - reference),
                EstMeilleur = i == 0,
                Alerte = p.Alerte
            });
        }
        return lignes;
    }
}

// Classe "static" : on l'utilise directement, sans créer d'objet (DonneesSimulees.TrouverProduit(...)).
public static class DonneesSimulees
{
    private static readonly List<ProduitSimule> Produits = new()
    {
        new ProduitSimule
        {
            Nom = "Lait entier 1 L", Code = "6001234567890",
            Prix = { ("Super Plus", 1250, null), ("Marché Central", 1400, null),
                     ("Mini Prix", 1450, null), ("Express 24", 1600, "Relevé il y a 3 sem") }
        },
        new ProduitSimule
        {
            Nom = "Riz long grain 1 kg", Code = "6001234567891",
            Prix = { ("Marché Central", 900, null), ("Super Plus", 950, null),
                     ("Mini Prix", 1000, null), ("Express 24", 1100, null) }
        },
        new ProduitSimule
        {
            Nom = "Huile de tournesol 1 L", Code = "6001234567892",
            Prix = { ("Super Plus", 1800, null), ("Marché Central", 1850, null),
                     ("Mini Prix", 1900, null), ("Express 24", 2000, null) }
        },
    };

    // Cherche un produit par son code-barres. Renvoie null s'il n'existe pas.
    public static ProduitSimule? TrouverProduit(string code)
        => Produits.FirstOrDefault(p => p.Code == code);

    // 1250 -> "1 250 FCFA" (espace normal comme séparateur de milliers)
    public static string FormaterFcfa(int prix)
        => prix.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', ' ') + " FCFA";
}
