using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class HistoriquePage : ContentPage
{
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

        // SIMULATION : on attend 0,8 seconde comme si on téléchargeait les données.
        await Task.Delay(800);

        // 2) Remplissage avec les données (simulées)
        NomProduit.Text = HistoriqueSimule.NomProduit;
        StatMin.Text = HistoriqueSimule.FormaterNombre(HistoriqueSimule.Min);
        StatMoyen.Text = HistoriqueSimule.FormaterNombre(HistoriqueSimule.Moyen);
        StatMax.Text = HistoriqueSimule.FormaterNombre(HistoriqueSimule.Max);
        BindableLayout.SetItemsSource(ListeReleves, HistoriqueSimule.ConstruireLignes());

        // 3) On remplace le chargement par le contenu
        Chargement.IsVisible = false;
        Contenu.IsVisible = true;
    }

    // Toucher un relevé : ouvre le Résultat du produit
    private async void OnReleveTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"ResultatPage?code={HistoriqueSimule.CodeProduit}");
    }
}
