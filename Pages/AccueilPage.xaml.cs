using Microsoft.Maui.Graphics; // pour le type Color
using PriceCheck.Data;         // IProduitRepository
using PriceCheck.Services;     // ServiceHelper, SessionUtilisateur

namespace PriceCheck.Pages;

public partial class AccueilPage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    // Accès aux données : on demande à MAUI le repository enregistré dans MauiProgram.cs
    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    public AccueilPage()
    {
        InitializeComponent();
    }

    // Appelé chaque fois que l'Accueil devient visible : on relit les derniers produits
    // (ainsi un relevé qu'on vient d'ajouter apparaît tout de suite).
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var produits = await _depot.ObtenirDerniersProduitsAsync(3);
        BindableLayout.SetItemsSource(ListeProduits, produits);
    }

    // "Scanner un produit" : ouvre l'onglet Scanner
    private async void OnScannerClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ScannerPage");
    }

    // "Comparer" : vérifie le code, cherche le produit dans la base, ouvre Résultat
    private async void OnComparerClicked(object? sender, EventArgs e)
    {
        string code = ChampCode.Text?.Trim() ?? "";

        // 1) Le code doit contenir exactement 13 chiffres
        if (code.Length != 13 || !code.All(char.IsDigit))
        {
            AfficherErreurCode("Saisissez les 13 chiffres du code-barres");
            return;
        }

        // 2) Le code doit exister dans la base de données
        if (await _depot.TrouverProduitAsync(code) == null)
        {
            AfficherErreurCode("Code inconnu. Essayez 6001234567890");
            return;
        }

        AfficherErreurCode(null); // tout va bien : on efface l'erreur

        // Ouvre Résultat en lui passant le code dans l'adresse ("?code=...")
        await Shell.Current.GoToAsync($"ResultatPage?code={code}");
    }

    // NOUVEAU : "Se déconnecter" : on efface la session, puis on retourne sur l'écran Connexion
    private async void OnSeDeconnecterTapped(object? sender, TappedEventArgs e)
    {
        SessionUtilisateur.Fermer(); // la personne n'est plus connectée, même après fermeture de l'appli
        await Shell.Current.GoToAsync("//ConnexionPage");
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