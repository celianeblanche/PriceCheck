using SQLite;

namespace PriceCheck.Data;

// Entité : un relevé de prix = "tel produit coûtait tel prix dans telle enseigne à telle date".
[Table("releves")]
public class Releve
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Lien vers la table produits (on stocke seulement l'Id du produit)
    [Indexed]
    public int ProduitId { get; set; }

    // Lien vers la table enseignes
    [Indexed]
    public int EnseigneId { get; set; }

    // Prix en FCFA, nombre entier brut (1250), PAS le texte "1 250 FCFA"
    public int Prix { get; set; }

    // Vraie date : on calculera "il y a 3 sem" à partir d'elle
    public DateTime DateReleve { get; set; }
}