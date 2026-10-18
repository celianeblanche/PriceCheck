using PriceCheck.Models;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class ListePage : ContentPage
{
    public ListePage()
    {
        InitializeComponent();
    }

    // OnAppearing : appelé chaque fois que l'onglet devient visible.
    // On relit la liste pour afficher les ajouts faits depuis l'écran Résultat.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        bool vide = ListeCourses.Articles.Count == 0;
        EtatVide.IsVisible = vide;        // montre l'état vide si la liste est vide...
        ContenuListe.IsVisible = !vide;   // ...sinon montre la liste

        // On donne une copie de la liste ("ToList") pour que l'affichage soit bien rafraîchi
        BindableLayout.SetItemsSource(ListeArticles, ListeCourses.Articles.ToList());
        TotalListe.Text = ListeCourses.TotalTexte();
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
