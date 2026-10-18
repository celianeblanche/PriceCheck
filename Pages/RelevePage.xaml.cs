using System.Globalization;
using Microsoft.Maui.Graphics;
using PriceCheck.Services;

namespace PriceCheck.Pages;

// Reçoit le code-barres du produit dans l'adresse de navigation ("RelevePage?code=...")
[QueryProperty(nameof(Code), "code")]
public partial class RelevePage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    private string _code = "";

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            // "?." : si le produit est null, on n'essaie pas de lire son nom ; "??" : texte de remplacement
            NomProduit.Text = DonneesSimulees.TrouverProduit(value)?.Nom ?? "";
        }
    }

    public RelevePage()
    {
        InitializeComponent();
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

        // SIMULATION : enregistré en mémoire seulement
        ReleveService.Enregistrer(_code, ChampEnseigne.Text!.Trim(), prix);

        await DisplayAlertAsync("Relevé enregistré", "Simulation : le relevé est gardé en mémoire jusqu'à la fermeture de l'application.", "OK");
        await Shell.Current.GoToAsync(".."); // retour à Résultat
    }

    private async void OnAnnulerClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
