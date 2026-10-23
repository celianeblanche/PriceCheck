using SQLite;

namespace PriceCheck.Data;

// Entité : une ligne de la table "articles_liste" = un produit dans la liste d'achats d'une personne.
// On ne stocke que l'essentiel (qui, quel produit, combien). Le nom et le meilleur prix
// sont relus dans la base des produits : la liste affiche toujours les prix à jour.
[Table("articles_liste")]
public class ArticleListeDb
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // À qui appartient cette liste (e-mail de la personne connectée)
    [Indexed]
    public string UtilisateurEmail { get; set; } = "";

    // Code-barres du produit ajouté
    public string CodeBarres { get; set; } = "";

    public int Quantite { get; set; } = 1;
}