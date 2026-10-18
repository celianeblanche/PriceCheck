using Microsoft.Maui.Graphics; // pour le type Color
using PriceCheck.Models;       // ProduitRecent
using PriceCheck.Services;     // DonneesSimulees

namespace PriceCheck.Pages;

public partial class AccueilPage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    public AccueilPage()
    {
        InitializeComponent();

        // SIMULATION : produits fictifs (aucune base de données pour le moment).
        var produits = new List<ProduitRecent>
        {
            new() { Nom = "Lait entier 1 L",        Enseigne = "Super Plus",     Prix = "1 250 FCFA", Anciennete = "il y a 2 j" },
            new() { Nom = "Riz long grain 1 kg",    Enseigne = "Marché Central", Prix = "900 FCFA",   Anciennete = "il y a 5 j" },
            new() { Nom = "Huile de tournesol 1 L", Enseigne = "Super Plus",     Prix = "1 800 FCFA", Anciennete = "il y a 1 sem" },
        };

        BindableLayout.SetItemsSource(ListeProduits, produits);
    }

    // "Scanner un produit" : ouvre l'onglet Scanner
    private async void OnScannerClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ScannerPage");
    }

    // "Comparer" : vérifie le code, cherche le produit, ouvre Résultat
    private async void OnComparerClicked(object? sender, EventArgs e)
    {
        string code = ChampCode.Text?.Trim() ?? "";

        // 1) Le code doit contenir exactement 13 chiffres
        if (code.Length != 13 || !code.All(char.IsDigit))
        {
            AfficherErreurCode("Saisissez les 13 chiffres du code-barres");
            return;
        }

        // 2) SIMULATION : le code doit exister dans nos données fictives
        if (DonneesSimulees.TrouverProduit(code) == null)
        {
            AfficherErreurCode("Code inconnu (données simulées). Essayez 6001234567890");
            return;
        }

        AfficherErreurCode(null); // tout va bien : on efface l'erreur

        // Ouvre Résultat en lui passant le code dans l'adresse ("?code=...")
        await Shell.Current.GoToAsync($"ResultatPage?code={code}");
    }

    // Montre ou cache l'erreur du champ code (texte = null : pas d'erreur)
    private void AfficherErreurCode(string? texte)
    {
        bool enErreur = texte != null;
        ErreurCode.Text = texte;
        ErreurCode.IsVisible = enErreur;
        CadreCode.Stroke = new SolidColorBrush(enErreur ? CouleurErreur : CouleurBordureChamp);
        CadreCode.StrokeThickness = enErreur ? 2 : 1;
    }
}
