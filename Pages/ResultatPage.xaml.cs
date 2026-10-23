using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

[QueryProperty(nameof(Code), "code")]
public partial class ResultatPage : ContentPage
{
    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    // NOUVEAU : accès à la liste d'achats enregistrée dans la base
    private readonly IListeRepository _liste = ServiceHelper.Obtenir<IListeRepository>();

    private string _code = "";

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            _ = ChargerAsync(); // "_ =" : on lance le chargement sans attendre (un "set" ne peut pas utiliser await)
        }
    }

    public ResultatPage()
    {
        InitializeComponent();
    }

    // Rechargé chaque fois que la page redevient visible (ex. retour depuis "Ajouter un relevé")
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ChargerAsync();
    }

    // Lit le produit dans la base et remplit l'écran
    private async Task ChargerAsync()
    {
        if (string.IsNullOrEmpty(_code))
        {
            return; // le code n'est pas encore arrivé : on attend
        }

        var produit = await _depot.TrouverProduitAsync(_code);

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

    // Ajoute le produit à la liste d'achats (enregistrée dans la base, propre à la personne connectée)
    private async void OnAjouterListeClicked(object? sender, EventArgs e)
    {
        var produit = await _depot.TrouverProduitAsync(_code);
        if (produit == null)
        {
            return; // produit inconnu : rien à ajouter
        }

        await _liste.AjouterAsync(produit);
        await DisplayAlertAsync("Ma liste", "Produit ajouté à votre liste.", "OK");
    }

    private async void OnAjouterReleveClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"RelevePage?code={_code}");
    }
}