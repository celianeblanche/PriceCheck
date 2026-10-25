using PriceCheck.Models;
using PriceCheck.Services;
using SQLite;

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

    // NOUVEAU : recherche par nom. Tous les mots tapés doivent se retrouver dans le nom du produit.
    // Exemple : "lait ent" trouve "Lait entier 1 L".
    public async Task<List<ProduitRecent>> RechercherAsync(string texte)
    {
        var resultat = new List<ProduitRecent>();

        // Mots tapés, sans accents ni majuscules : "Lait ENT" -> ["lait", "ent"]
        var mots = Formatage.Normaliser(texte).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (mots.Length == 0)
        {
            return resultat;
        }

        var db = await _base.ObtenirConnexionAsync();
        var produits = await db.Table<Produit>().ToListAsync();
        var releves = await db.Table<Releve>().ToListAsync();
        var enseignes = await db.Table<Enseigne>().ToListAsync();

        // On garde les produits dont le nom contient tous les mots (20 au maximum)
        var trouves = produits
            .Where(p =>
            {
                string nom = Formatage.Normaliser(p.Nom);
                return mots.All(mot => nom.Contains(mot));
            })
            .OrderBy(p => p.Nom)
            .Take(20)
            .ToList();

        foreach (var produit in trouves)
        {
            // Le relevé le plus récent de chaque enseigne, puis le moins cher d'entre eux
            var meilleur = releves
                .Where(r => r.ProduitId == produit.Id)
                .GroupBy(r => r.EnseigneId)
                .Select(groupe => groupe.OrderByDescending(r => r.DateReleve).First())
                .OrderBy(r => r.Prix)
                .FirstOrDefault();

            resultat.Add(new ProduitRecent
            {
                Code = produit.CodeBarres,
                Nom = produit.Nom,
                Enseigne = meilleur == null ? "" : enseignes.First(e => e.Id == meilleur.EnseigneId).Nom,
                Prix = meilleur == null ? "Pas de prix" : Formatage.FormaterFcfa(meilleur.Prix),
                Anciennete = meilleur == null ? "" : Formatage.FormaterAnciennete(meilleur.DateReleve)
            });
        }
        return resultat;
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

    // Crée un produit encore inconnu, puis enregistre son premier relevé de prix.
    public async Task<bool> CreerProduitAsync(string code, string nom, string enseigne, int prix)
    {
        var db = await _base.ObtenirConnexionAsync();

        // Le produit existe déjà ? Alors on ne le recrée pas.
        var existant = await db.Table<Produit>().Where(p => p.CodeBarres == code).FirstOrDefaultAsync();
        if (existant != null)
        {
            return false;
        }

        try
        {
            await db.InsertAsync(new Produit { CodeBarres = code, Nom = nom.Trim() });
        }
        catch (SQLiteException)
        {
            // Filet de sécurité : la base a refusé (code-barres "Unique" déjà pris entre-temps)
            return false;
        }

        // Le produit existe maintenant : on réutilise la méthode des relevés
        // (elle crée aussi l'enseigne si elle est nouvelle).
        await EnregistrerReleveAsync(code, enseigne, prix);
        return true;
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
                Code = produit.CodeBarres, // NOUVEAU : nécessaire pour ouvrir la comparaison au toucher
                Nom = produit.Nom,
                Enseigne = enseignes.First(e => e.Id == r.EnseigneId).Nom,
                Prix = Formatage.FormaterFcfa(r.Prix),
                Anciennete = Formatage.FormaterAnciennete(r.DateReleve)
            });
        }
        return resultat;
    }
}