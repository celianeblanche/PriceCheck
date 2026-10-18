using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PriceCheck.Models;

// Modèle : une ligne de la comparaison (une enseigne et son prix).
// L'écran Résultat affiche une carte par LignePrix.
public class LignePrix
{
    public string Enseigne { get; set; } = "";
    public string PrixTexte { get; set; } = "";   // ex. "1 250 FCFA"
    public string EcartTexte { get; set; } = "";  // "Référence" ou "+150"
    public bool EstMeilleur { get; set; }         // true = prix le plus bas
    public string? Alerte { get; set; }           // ex. "Relevé il y a 3 sem" (null = pas d'alerte)

    // Propriétés calculées (lecture seule) : l'écran les utilise pour choisir l'affichage.
    // "=>" signifie "la valeur est calculée à partir des autres propriétés".
    public bool AAlerte => Alerte != null;

    // Meilleur prix : fond vert clair, bordure verte. Autres : fond blanc, bordure grise.
    public Color CouleurFond => EstMeilleur ? Color.FromArgb("#E8F6EE") : Colors.White;
    public Brush CouleurBordure => new SolidColorBrush(EstMeilleur ? Color.FromArgb("#15803D") : Color.FromArgb("#E2E8F0"));
    public Color CouleurPrix => EstMeilleur ? Color.FromArgb("#15803D") : Color.FromArgb("#0F172A");
}
