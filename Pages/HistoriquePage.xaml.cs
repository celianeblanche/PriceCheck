using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class HistoriquePage : ContentPage
{
    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    // Produit dont on affiche l'historique : le dernier consulté (ou le lait par défaut).
    // C'est une propriété : sa valeur est relue à chaque fois qu'on l'utilise.
    private static string CodeProduit => DernierProduit.Code;

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