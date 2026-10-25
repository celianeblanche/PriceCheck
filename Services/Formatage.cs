using System.Globalization;
using System.Text;

namespace PriceCheck.Services;

// Fonctions d'affichage partagées (nombres, prix, dates, recherche).
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

    // NOUVEAU : met un texte en minuscules et retire les accents, pour la recherche.
    // "Blé ÉPEAUTRE" -> "ble epeautre". Ainsi "ble" trouve "Blé".
    public static string Normaliser(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return "";
        }

        var resultat = new StringBuilder();
        foreach (char c in texte.Trim().ToLowerInvariant())
        {
            // On remplace chaque lettre accentuée par la lettre simple correspondante
            resultat.Append(c switch
            {
                'à' or 'â' or 'ä' or 'á' or 'ã' or 'å' => 'a',
                'ç' => 'c',
                'é' or 'è' or 'ê' or 'ë' => 'e',
                'î' or 'ï' or 'í' or 'ì' => 'i',
                'ô' or 'ö' or 'ò' or 'ó' or 'õ' => 'o',
                'ù' or 'û' or 'ü' or 'ú' => 'u',
                'ÿ' or 'ý' => 'y',
                _ => c // les autres caractères restent tels quels
            });
        }
        return resultat.ToString();
    }
}