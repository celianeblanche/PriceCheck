using System.Globalization;
using Microsoft.Maui.Graphics;
using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

// Reçoit le code-barres du produit dans l'adresse de navigation ("RelevePage?code=...")
[QueryProperty(nameof(Code), "code")]
public partial class RelevePage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    private string _code = "";

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            _ = AfficherNomProduitAsync(value); // lance la lecture du nom sans bloquer
        }
    }

    public RelevePage()
    {
        InitializeComponent();
    }

    // Lit le nom du produit dans la base et l'affiche en haut de l'écran
    private async Task AfficherNomProduitAsync(string code)
    {
        var produit = await _depot.TrouverProduitAsync(code);
        // "?." : si le produit est null, on n'essaie pas de lire son nom ; "??" : texte de remplacement
        NomProduit.Text = produit?.Nom ?? "";
    }

    // Convertit le texte saisi en nombre. Accepte "1250" et "1 250".
    // Renvoie true si c'est un entier strictement positif ; le résultat est placé dans "prix".
    private static bool TryLirePrix(string? texte, out int prix)
    {
        string nettoye = (texte ?? "").Replace(" ", "").Replace("\u00A0", "").Replace("\u202F", "");
        return int.TryParse(nettoye, NumberStyles.None, CultureInfo.InvariantCulture, out prix) && prix > 0;
    }

    // Appelé à chaque modification d'un des deux champs : met à jour l'erreur et le bouton.
    private void OnChampModifie(object? sender, TextChangedEventArgs e)
    {
        bool enseigneOk = !string.IsNullOrWhiteSpace(ChampEnseigne.Text);

        string prixTexte = ChampPrix.Text ?? "";
        bool prixOk = TryLirePrix(prixTexte, out _);          // "_" = on n'a pas besoin de la valeur ici
        bool prixEnErreur = prixTexte.Length > 0 && !prixOk;   // pas d'erreur tant que le champ est vide

        ErreurPrix.Text = prixEnErreur ? "Saisissez un prix en chiffres (ex. 1 250)" : null;
        ErreurPrix.IsVisible = prixEnErreur;
        CadrePrix.Stroke = new SolidColorBrush(prixEnErreur ? CouleurErreur : CouleurBordureChamp);
        CadrePrix.StrokeThickness = prixEnErreur ? 2 : 1;

        // Le bouton n'est actif que si tout est valide
        BoutonEnregistrer.IsEnabled = enseigneOk && prixOk;
    }

    private async void OnEnregistrerClicked(object? sender, EventArgs e)
    {
        if (!TryLirePrix(ChampPrix.Text, out int prix))
        {
            return; // sécurité : ne devrait pas arriver, le bouton est désactivé
        }

        // On désactive le bouton pendant l'enregistrement : un double appui ne crée pas deux relevés
        BoutonEnregistrer.IsEnabled = false;

        // Enregistrement RÉEL dans la base SQLite (il reste après la fermeture de l'appli)
        await _depot.EnregistrerReleveAsync(_code, ChampEnseigne.Text!.Trim(), prix);

        await DisplayAlertAsync("Relevé enregistré", "Merci ! Votre relevé de prix a été ajouté.", "OK");
        await Shell.Current.GoToAsync(".."); // retour à Résultat
    }

    private async void OnAnnulerClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}