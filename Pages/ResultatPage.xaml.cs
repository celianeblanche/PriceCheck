using PriceCheck.Services;

namespace PriceCheck.Pages;

[QueryProperty(nameof(Code), "code")]
public partial class ResultatPage : ContentPage
{
    private string _code = "";

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            Charger();
        }
    }

    public ResultatPage()
    {
        InitializeComponent();
    }

    // Rechargé chaque fois que la page redevient visible (ex. retour depuis "Ajouter un relevé")
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Charger();
    }

    private void Charger()
    {
        var produit = DonneesSimulees.TrouverProduit(_code);

        if (produit == null)
        {
            NomProduit.Text = "Produit introuvable";
            CodeProduit.Text = $"Code-barres {_code}";
            BindableLayout.SetItemsSource(ListeLignes, null);
            return;
        }

        NomProduit.Text = produit.Nom;
        CodeProduit.Text = $"Code-barres {produit.CodeAffiche}";
        BindableLayout.SetItemsSource(ListeLignes, produit.ConstruireLignes());
    }

    private async void OnRetourTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // Ajoute le produit à la liste d'achats (SIMULATION : liste gardée en mémoire seulement)
    private async void OnAjouterListeClicked(object? sender, EventArgs e)
    {
        var produit = DonneesSimulees.TrouverProduit(_code);
        if (produit == null)
        {
            return; // produit inconnu : rien à ajouter
        }

        ListeCourses.Ajouter(produit);
        await DisplayAlertAsync("Ma liste", "Produit ajouté à votre liste.", "OK");
    }

    private async void OnAjouterReleveClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"RelevePage?code={_code}");
    }
}
