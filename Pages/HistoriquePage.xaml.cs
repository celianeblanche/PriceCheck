using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class HistoriquePage : ContentPage
{
    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    public HistoriquePage()
    {
        InitializeComponent();
    }

    // Montre l'écran "Aucun historique" et cache tout le reste
    private void AfficherEtatVide()
    {
        NomProduit.Text = "";
        EtatVide.IsVisible = true;
        Chargement.IsVisible = false;
        Contenu.IsVisible = false;
    }

    // async : permet d'attendre ("await") sans bloquer l'écran.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // NOUVEAU : aucun produit consulté pour l'instant : on propose d'en chercher un
        if (!DernierProduit.Existe)
        {
            AfficherEtatVide();
            return;
        }

        // 1) État de chargement : blocs gris visibles, contenu caché
        EtatVide.IsVisible = false;
        Chargement.IsVisible = true;
        Contenu.IsVisible = false;

        // Courte pause pour que l'état de chargement reste visible (la lecture locale est très rapide)
        await Task.Delay(500);

        // 2) Lecture de l'historique du dernier produit consulté et remplissage de l'écran
        var historique = await _depot.ObtenirHistoriqueAsync(DernierProduit.Code);
        if (historique == null)
        {
            AfficherEtatVide(); // le produit n'existe plus ou n'a aucun prix
            return;
        }

        NomProduit.Text = historique.NomProduit;
        StatMin.Text = Formatage.FormaterNombre(historique.Min);
        StatMoyen.Text = Formatage.FormaterNombre(historique.Moyen);
        StatMax.Text = Formatage.FormaterNombre(historique.Max);
        BindableLayout.SetItemsSource(ListeReleves, historique.Lignes);

        // 3) On remplace le chargement par le contenu
        Chargement.IsVisible = false;
        Contenu.IsVisible = true;
    }

    // NOUVEAU : "Rechercher un produit" (écran vide) : retourne à l'Accueil, où se trouve la recherche
    private async void OnRechercherClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage");
    }

    // Toucher un prix : ouvre le Résultat du produit
    private async void OnReleveTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"ResultatPage?code={DernierProduit.Code}");
    }
}