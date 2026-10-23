using SQLite;

namespace PriceCheck.Data;

// Entité : une ligne de la table "enseignes" (Super Plus, Mini Prix...).
[Table("enseignes")]
public class Enseigne
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Nom unique de l'enseigne
    [Unique]
    public string Nom { get; set; } = "";
}