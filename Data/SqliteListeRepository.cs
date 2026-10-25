using PriceCheck.Models;
using PriceCheck.Services;
using SQLite;

namespace PriceCheck.Data;

// Implémentation SQLite du contrat IListeRepository.
public class SqliteListeRepository : IListeRepository
{
    private readonly BaseDeDonnees _base;
    private readonly IProduitRepository _produits; // pour relire nom et prix des produits

    // Indique si la table "articles_liste" a déjà été créée pendant cette session
    private bool _tableCreee;

    // MAUI fournit lui-même les deux éléments demandés (ils sont enregistrés dans MauiProgram.cs)
    public SqliteListeRepository(BaseDeDonnees baseDeDonnees, IProduitRepository produits)
    {
        _base = baseDeDonnees;
        _produits = produits;
    }

    // Ouvre la base et crée la table de la liste si elle n'existe pas encore.
    // (CreateTableAsync ne fait rien si la table existe déjà : les données ne sont pas touchées.)
    private async Task<SQLiteAsyncConnection> ConnexionAsync()
    {
        var db = await _base.ObtenirConnexionAsync();
        if (!_tableCreee)
        {
            await db.CreateTableAsync<ArticleListeDb>();
            _tableCreee = true;
        }
        return db;
    }

    public async Task AjouterAsync(ProduitDetail produit)
    {
        var db = await ConnexionAsync();
        string email = SessionUtilisateur.Email; // la liste appartient à la personne connectée
        string code = produit.Code;

        // Le produit est-il déjà dans la liste de cette personne ?
        var existant = await db.Table<ArticleListeDb>()
            .Where(a => a.UtilisateurEmail == email && a.CodeBarres == code)
            .FirstOrDefaultAsync();

        if (existant != null)
        {
            existant.Quantite++;
            await db.UpdateAsync(existant); // met à jour la ligne existante
            return;
        }

        await db.InsertAsync(new ArticleListeDb
        {
            UtilisateurEmail = email,
            CodeBarres = code,
            Quantite = 1
        });
    }

    // NOUVEAU : supprime l'article de la liste de la personne connectée
    public async Task RetirerAsync(string code)
    {
        var db = await ConnexionAsync();
        string email = SessionUtilisateur.Email;

        // On retrouve la ligne (à la fois par personne ET par produit : on ne touche jamais à la liste d'un autre compte)
        var ligne = await db.Table<ArticleListeDb>()
            .Where(a => a.UtilisateurEmail == email && a.CodeBarres == code)
            .FirstOrDefaultAsync();

        if (ligne != null)
        {
            await db.DeleteAsync(ligne); // supprime cette ligne de la table
        }
    }

    public async Task<List<ArticleListe>> ObtenirArticlesAsync()
    {
        var db = await ConnexionAsync();
        string email = SessionUtilisateur.Email;

        // Les lignes de la liste de cette personne, dans l'ordre d'ajout
        var lignes = await db.Table<ArticleListeDb>()
            .Where(a => a.UtilisateurEmail == email)
            .OrderBy(a => a.Id)
            .ToListAsync();

        var articles = new List<ArticleListe>();
        foreach (var ligne in lignes)
        {
            // On relit le produit pour avoir son nom et son meilleur prix ACTUEL
            var produit = await _produits.TrouverProduitAsync(ligne.CodeBarres);
            var meilleur = produit?.MeilleurPrix;
            if (produit == null || meilleur == null)
            {
                continue; // produit disparu ou sans prix : on ne l'affiche pas
            }

            articles.Add(new ArticleListe
            {
                Code = produit.Code,
                Nom = produit.Nom,
                Enseigne = meilleur.Enseigne,
                PrixUnitaire = meilleur.Prix,
                Quantite = ligne.Quantite
            });
        }
        return articles;
    }
}