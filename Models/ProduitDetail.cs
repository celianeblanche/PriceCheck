using PriceCheck.Services;

namespace PriceCheck.Models;

// Le prix le plus récent d'un produit dans UNE enseigne.
public class PrixEnseigne
{
    public string Enseigne { get; set; } = "";
    public int Prix { get; set; }              // en FCFA (nombre brut)
    public DateTime DateReleve { get; set; }   // sert à calculer l'alerte "relevé ancien"
}

// Un produit avec ses prix par enseigne, lu depuis la base de données.
public class ProduitDetail
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public string Code { get; set; } = "";
    public List<PrixEnseigne> Prix { get; set; } = new();

    // Code affiché comme dans la maquette : 6 001234 567890
    public string CodeAffiche => Code.Length == 13
        ? $"{Code[0]} {Code.Substring(1, 6)} {Code.Substring(7)}"
        : Code;

    // Le prix le plus bas (null si le produit n'a aucun relevé)
    public PrixEnseigne? MeilleurPrix => Prix.OrderBy(p => p.Prix).FirstOrDefault();

    // Relevé de 14 jours ou plus : on affiche "Relevé il y a N sem". Sinon : pas d'alerte (null).
    private static string? AlerteAnciennete(DateTime date)
    {
        int jours = (DateTime.Today - date.Date).Days;
        return jours >= 14 ? $"Relevé il y a {jours / 7} sem" : null;
    }

    // Trie les prix du moins cher au plus cher et prépare les lignes de l'écran Résultat.
    public List<LignePrix> ConstruireLignes()
    {
        var lignes = new List<LignePrix>();
        if (Prix.Count == 0)
        {
            return lignes; // aucun prix : rien à afficher
        }

        var tries = Prix.OrderBy(p => p.Prix).ToList();
        int reference = tries[0].Prix; // le moins cher sert de référence pour les écarts

        for (int i = 0; i < tries.Count; i++)
        {
            var p = tries[i];
            lignes.Add(new LignePrix
            {
                Enseigne = p.Enseigne,
                PrixTexte = Formatage.FormaterFcfa(p.Prix),
                EcartTexte = i == 0 ? "Référence" : "+" + (p.Prix - reference),
                EstMeilleur = i == 0,
                Alerte = AlerteAnciennete(p.DateReleve)
            });
        }
        return lignes;
    }
}