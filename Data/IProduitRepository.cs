using PriceCheck.Models;

namespace PriceCheck.Data;

public interface IProduitRepository
{
    // Cherche un produit et ses prix par code-barres (null si inconnu)
    Task<ProduitDetail?> TrouverProduitAsync(string code);

    // NOUVEAU : cherche des produits par leur nom (sans tenir compte des accents ni des majuscules)
    Task<List<ProduitRecent>> RechercherAsync(string texte);

    // Enregistre un nouveau relevé de prix (crée l'enseigne si elle n'existe pas)
    Task EnregistrerReleveAsync(string code, string enseigne, int prix);

    // Crée un produit inconnu avec son premier relevé de prix.
    // Renvoie false si le code-barres existe déjà.
    Task<bool> CreerProduitAsync(string code, string nom, string enseigne, int prix);

    // Historique des prix d'un produit (null si produit inconnu)
    Task<HistoriqueProduit?> ObtenirHistoriqueAsync(string code);

    // Les produits qui ont reçu un relevé le plus récemment (pour l'Accueil)
    Task<List<ProduitRecent>> ObtenirDerniersProduitsAsync(int nombre);
}