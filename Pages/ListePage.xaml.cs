using PriceCheck.Data;
using PriceCheck.Models;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class ListePage : ContentPage
{
    // Accès à la liste d'achats enregistrée dans la base
    private readonly IListeRepository _liste = ServiceHelper.Obtenir<IListeRepository>();

    public ListePage()
    {
        InitializeComponent();
    }

    // OnAppearing : appelé chaque fois que l'onglet devient visible.
    // On relit la liste dans la base pour afficher les ajouts faits depuis l'écran Résultat.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var articles = await _liste.ObtenirArticlesAsync();

        bool vide = articles.Count == 0;
        EtatVide.IsVisible = vide;        // montre l'état vide si la liste est vide...
        ContenuListe.IsVisible = !vide;   // ...sinon montre la liste

        BindableLayout.SetItemsSource(ListeArticles, articles);

        // Total = somme de (prix x quantité) de chaque article
        TotalListe.Text = Formatage.FormaterFcfa(articles.Sum(a => a.PrixUnitaire * a.Quantite));
    }

    // "Scanner un produit" (liste vide) : ouvre l'onglet Scanner
    private async void OnScannerClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ScannerPage");
    }

    // "Ajouter un produit" : retourne à l'Accueil (scan ou saisie du code-barres)
    private async void OnAjouterProduitClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AccueilPage");
    }

    // Toucher une carte : ouvre le Résultat du produit.
    // "sender" = l'élément touché ; son BindingContext est l'ArticleListe qu'il affiche.
    private async void OnArticleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject cible && cible.BindingContext is ArticleListe article)
        {
            await Shell.Current.GoToAsync($"ResultatPage?code={article.Code}");
        }
    }
}