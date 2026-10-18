using Microsoft.Maui.Graphics;

namespace PriceCheck.Models;

// Modèle : une ligne de l'historique (un relevé de prix daté).
public class LigneReleve
{
    public string DateTexte { get; set; } = "";       // ex. "5 oct. 2026"
    public string PrixTexte { get; set; } = "";       // ex. "1 250 FCFA"
    public string VariationTexte { get; set; } = "";  // "−50", "+100", "Stable" ou "Premier relevé"
    public Color CouleurVariation { get; set; } = Color.FromArgb("#475569");
}
