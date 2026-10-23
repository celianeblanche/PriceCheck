using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class HistoriquePage : ContentPage
{
    // Produit dont on affiche l'historique : le lait, comme sur la maquette.
    private const string CodeProduit = "6001234567890";

    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    public HistoriquePage()
    {
        InitializeComponent();
    }

    // async : permet d'attendre ("await") sans bloquer l'écran.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1) État de chargement : blocs gris visibles, contenu caché
        Chargement.IsVisible = true;
        Contenu.IsVisible = false;

        // Courte pause pour que l'état de chargement reste visible (la lecture locale est très rapide)
        await Task.Delay(500);

        // 2) Lecture de l'historique dans la base et remplissage de l'écran
        var historique = await _depot.ObtenirHistoriqueAsync(CodeProduit);
        if (historique != null)
        {
            NomProduit.Text = historique.NomProduit;
            StatMin.Text = Formatage.FormaterNombre(historique.Min);
            StatMoyen.Text = Formatage.FormaterNombre(historique.Moyen);
            StatMax.Text = Formatage.FormaterNombre(historique.Max);
            BindableLayout.SetItemsSource(ListeReleves, historique.Lignes);
        }

        // 3) On remplace le chargement par le contenu
        Chargement.IsVisible = false;
        Contenu.IsVisible = true;
    }

    // Toucher un relevé : ouvre le Résultat du produit
    private async void OnReleveTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"ResultatPage?code={CodeProduit}");
    }
}