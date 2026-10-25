using Microsoft.Maui.Graphics; // pour le type Color
using PriceCheck.Data;         // IProduitRepository
using PriceCheck.Models;       // ProduitRecent
using PriceCheck.Services;     // ServiceHelper, SessionUtilisateur, AjoutProduitService

namespace PriceCheck.Pages;

public partial class AccueilPage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    // Accès aux données : on demande à MAUI le repository enregistré dans MauiProgram.cs
    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    // Numéro de la dernière recherche lancée. Si on tape vite, plusieurs recherches se chevauchent :
    // seule la plus récente a le droit d'afficher son résultat (sinon un vieux résultat écraserait le bon).
    private int _numeroRecherche;

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

        // Si une recherche était en cours (retour depuis un Résultat), on la rafraîchit
        await MettreAJourRechercheAsync(ChampRecherche.Text);
    }

    // NOUVEAU : appelée à chaque lettre tapée dans le champ de recherche
    private async void OnRechercheModifiee(object? sender, TextChangedEventArgs e)
    {
        await MettreAJourRechercheAsync(e.NewTextValue);
    }

    // NOUVEAU : lance la recherche et choisit ce qu'on affiche (résultats ou Accueil normal)
    private async Task MettreAJourRechercheAsync(string? texte)
    {
        texte = texte?.Trim() ?? "";
        int numero = ++_numeroRecherche; // cette recherche est maintenant la plus récente

        // Champ vide : on revient à l'Accueil normal
        if (texte.Length == 0)
        {
            BlocResultats.IsVisible = false;
            BlocAccueil.IsVisible = true;
            return;
        }

        var resultats = await _depot.RechercherAsync(texte);

        // Pendant l'attente, une recherche plus récente a démarré : on abandonne celle-ci
        if (numero != _numeroRecherche)
        {
            return;
        }

        BindableLayout.SetItemsSource(ListeResultats, resultats);
        AucunResultat.IsVisible = resultats.Count == 0;
        BlocAccueil.IsVisible = false;
        BlocResultats.IsVisible = true;
    }

    // NOUVEAU : toucher une carte (résultat ou dernier produit) ouvre la comparaison.
    // "sender" = l'élément touché ; son BindingContext est le ProduitRecent qu'il affiche.
    private async void OnProduitTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject cible && cible.BindingContext is ProduitRecent produit)
        {
            await Shell.Current.GoToAsync($"ResultatPage?code={produit.Code}");
        }
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

        AfficherErreurCode(null); // le format est bon : on efface l'erreur

        // 2) Code inconnu : on propose de l'ajouter (comme sur l'écran Scanner)
        bool disponible = await _depot.TrouverProduitAsync(code) != null;
        if (!disponible)
        {
            disponible = await AjoutProduitService.ProposerAjoutAsync(this, code);
        }

        // 3) Produit connu (ou tout juste ajouté) : on ouvre Résultat avec le code dans l'adresse ("?code=...")
        if (disponible)
        {
            await Shell.Current.GoToAsync($"ResultatPage?code={code}");
        }
    }

    // "Se déconnecter" : on efface la session, puis on retourne sur l'écran Connexion
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