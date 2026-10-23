using PriceCheck.Models;

namespace PriceCheck.Data;

// Contrat : ce que les pages peuvent demander à propos de la liste d'achats.
public interface IListeRepository
{
    // Les articles de la personne connectée, avec le meilleur prix du moment
    Task<List<ArticleListe>> ObtenirArticlesAsync();

    // Ajoute un produit à la liste (si déjà présent, la quantité augmente de 1)
    Task AjouterAsync(ProduitDetail produit);
}