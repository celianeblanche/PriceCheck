using SQLite; // bibliothèque sqlite-net-pcl : fournit [Table], [PrimaryKey], etc.

namespace PriceCheck.Data;

// Entité : une ligne de la table "produits" dans la base de données.
[Table("produits")]
public class Produit
{
    // Identifiant unique, numéroté automatiquement par la base (1, 2, 3...)
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Code-barres : unique, la base refuse deux produits avec le même code
    [Unique]
    public string CodeBarres { get; set; } = "";

    public string Nom { get; set; } = "";
}