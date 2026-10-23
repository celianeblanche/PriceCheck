using PriceCheck.Models;
using PriceCheck.Services;

namespace PriceCheck.Data;

// Implémentation SQLite du contrat IProduitRepository.
public class SqliteProduitRepository : IProduitRepository
{
    private readonly BaseDeDonnees _base;

    // La base est "injectée" : MAUI nous donne celle enregistrée dans MauiProgram.cs
    public SqliteProduitRepository(BaseDeDonnees baseDeDonnees)
    {
        _base = baseDeDonnees;
    }

    public async Task<ProduitDetail?> TrouverProduitAsync(string code)
    {
        var db = await _base.ObtenirConnexionAsync();

        // 1) Le produit lui-même
        var produit = await db.Table<Produit>().Where(p => p.CodeBarres == code).FirstOrDefaultAsync();
        if (produit == null)
        {
            return null;
        }

        // 2) Ses relevés et toutes les enseignes (pour retrouver les noms)
        int produitId = produit.Id;
        var releves = await db.Table<Releve>().Where(r => r.ProduitId == produitId).ToListAsync();
        var enseignes = await db.Table<Enseigne>().ToListAsync();

        // 3) Pour chaque enseigne, on garde le relevé le plus récent
        var prix = releves
            .GroupBy(r => r.EnseigneId)
            .Select(groupe => groupe.OrderByDescending(r => r.DateReleve).First())
            .Select(r => new PrixEnseigne
            {
                Enseigne = enseignes.First(e => e.Id == r.EnseigneId).Nom,
                Prix = r.Prix,
                DateReleve = r.DateReleve
            })
            .ToList();

        return new ProduitDetail
        {
            Id = produit.Id,
            Nom = produit.Nom,
            Code = produit.CodeBarres,
            Prix = prix
        };
    }

    public async Task EnregistrerReleveAsync(string code, string nomEnseigne, int prix)
    {
        var db = await _base.ObtenirConnexionAsync();

        var produit = await db.Table<Produit>().Where(p => p.CodeBarres == code).FirstOrDefaultAsync();
        if (produit == null)
        {
            return; // produit inconnu : rien à enregistrer
        }

        // L'enseigne existe déjà ? (sans tenir compte des majuscules/minuscules)
        nomEnseigne = nomEnseigne.Trim();
        var enseignes = await db.Table<Enseigne>().ToListAsync();
        var enseigne = enseignes.FirstOrDefault(e =>
            string.Equals(e.Nom, nomEnseigne, StringComparison.OrdinalIgnoreCase));

        if (enseigne == null)
        {
            enseigne = new Enseigne { Nom = nomEnseigne };
            await db.InsertAsync(enseigne); // après l'insertion, enseigne.Id est rempli automatiquement
        }

        // On AJOUTE un relevé (on ne modifie jamais l'ancien) : c'est ce qui construit l'historique
        await db.InsertAsync(new Releve
        {
            ProduitId = produit.Id,
            EnseigneId = enseigne.Id,
            Prix = prix,
            DateReleve = DateTime.Now
        });
    }

    public async Task<HistoriqueProduit?> ObtenirHistoriqueAsync(string code)
    {
        var db = await _base.ObtenirConnexionAsync();

        // On affiche l'historique de l'enseigne la moins chère actuellement
        var detail = await TrouverProduitAsync(code);
        var meilleur = detail?.MeilleurPrix;
        if (detail == null || meilleur == null)
        {
            return null;
        }

        var enseignes = await db.Table<Enseigne>().ToListAsync();
        int enseigneId = enseignes.First(e => e.Nom == meilleur.Enseigne).Id;
        int produitId = detail.Id;

        // Relevés de ce produit dans cette enseigne, du plus récent au plus ancien
        var releves = await db.Table<Releve>()
            .Where(r => r.ProduitId == produitId && r.EnseigneId == enseigneId)
            .OrderByDescending(r => r.DateReleve)
            .ToListAsync();

        return HistoriqueProduit.Construire(
            code, detail.Nom,
            releves.Select(r => (r.DateReleve, r.Prix)).ToList());
    }

    public async Task<List<ProduitRecent>> ObtenirDerniersProduitsAsync(int nombre)
    {
        var db = await _base.ObtenirConnexionAsync();

        var releves = await db.Table<Releve>().OrderByDescending(r => r.DateReleve).ToListAsync();
        var produits = await db.Table<Produit>().ToListAsync();
        var enseignes = await db.Table<Enseigne>().ToListAsync();

        var resultat = new List<ProduitRecent>();
        foreach (var r in releves) // du plus récent au plus ancien
        {
            if (resultat.Count >= nombre)
            {
                break;
            }

            var produit = produits.First(p => p.Id == r.ProduitId);
            if (resultat.Any(x => x.Nom == produit.Nom))
            {
                continue; // un seul relevé affiché par produit
            }

            resultat.Add(new ProduitRecent
            {
                Nom = produit.Nom,
                Enseigne = enseignes.First(e => e.Id == r.EnseigneId).Nom,
                Prix = Formatage.FormaterFcfa(r.Prix),
                Anciennete = Formatage.FormaterAnciennete(r.DateReleve)
            });
        }
        return resultat;
    }
}