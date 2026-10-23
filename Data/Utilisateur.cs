using SQLite;

namespace PriceCheck.Data;

// Entité : un compte utilisateur (inscription / connexion).
[Table("utilisateurs")]
public class Utilisateur
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nom { get; set; } = "";

    // Email unique : deux comptes ne peuvent pas avoir le même
    [Unique]
    public string Email { get; set; } = "";

    // On ne stocke JAMAIS le mot de passe en clair, seulement son empreinte (hash)
    public string MotDePasseHash { get; set; } = "";

    // "Sel" : texte aléatoire mélangé au mot de passe avant le hachage, propre à chaque compte
    public string Sel { get; set; } = "";
}