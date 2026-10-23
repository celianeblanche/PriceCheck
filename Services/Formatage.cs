using System.Globalization;

namespace PriceCheck.Services;

// Fonctions d'affichage partagées (nombres, prix, dates).
// Elles remplacent celles qui étaient dans DonneesSimulees et HistoriqueSimule.
public static class Formatage
{
    private static readonly string[] Mois =
        { "janv.", "févr.", "mars", "avr.", "mai", "juin", "juil.", "août", "sept.", "oct.", "nov.", "déc." };

    // 1250 -> "1 250" (espace comme séparateur de milliers, sans l'unité)
    public static string FormaterNombre(int n)
        => n.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');

    // 1250 -> "1 250 FCFA"
    public static string FormaterFcfa(int prix) => FormaterNombre(prix) + " FCFA";

    // 5 octobre 2026 -> "5 oct. 2026"
    public static string FormaterDate(DateTime date)
        => $"{date.Day} {Mois[date.Month - 1]} {date.Year}";

    // Ancienneté d'un relevé : "aujourd'hui", "il y a 2 j", "il y a 3 sem"
    public static string FormaterAnciennete(DateTime date)
    {
        int jours = (DateTime.Today - date.Date).Days;
        if (jours <= 0) return "aujourd'hui";
        if (jours < 7) return $"il y a {jours} j";
        return $"il y a {jours / 7} sem";
    }
}