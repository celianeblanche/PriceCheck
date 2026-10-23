using Microsoft.Maui.Graphics;
using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class ScannerPage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    // Code-barres utilisé par le scan SIMULÉ (celui du lait, présent dans la base)
    private const string CodeDemonstration = "6001234567890";

    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    public ScannerPage()
    {
        InitializeComponent();
    }

    // Appui sur le cadre. SIMULATION : la caméra n'est pas utilisée ; on "lit" un code de démonstration.
    private async void OnCadreCameraTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Scan simulé",
            "La caméra n'est pas active dans cette version : un code-barres de démonstration est utilisé.", "OK");
        await Shell.Current.GoToAsync($"ResultatPage?code={CodeDemonstration}");
    }

    // "Comparer" : même contrôle que sur l'Accueil (13 chiffres, puis produit connu)
    private async void OnComparerClicked(object? sender, EventArgs e)
    {
        string code = ChampCode.Text?.Trim() ?? "";

        if (code.Length != 13 || !code.All(char.IsDigit))
        {
            AfficherErreurCode("Saisissez les 13 chiffres du code-barres");
            return;
        }

        // Le code doit exister dans la base de données
        if (await _depot.TrouverProduitAsync(code) == null)
        {
            AfficherErreurCode("Code inconnu. Essayez 6001234567890");
            return;
        }

        AfficherErreurCode(null);
        await Shell.Current.GoToAsync($"ResultatPage?code={code}");
    }

    // Montre ou cache l'erreur du champ (texte = null : pas d'erreur)
    private void AfficherErreurCode(string? texte)
    {
        bool enErreur = texte != null;
        ErreurCode.Text = texte;
        ErreurCode.IsVisible = enErreur;
        CadreCode.Stroke = new SolidColorBrush(enErreur ? CouleurErreur : CouleurBordureChamp);
        CadreCode.StrokeThickness = enErreur ? 2 : 1;
    }
}