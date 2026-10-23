using Microsoft.Maui.Graphics; // pour le type Color
using PriceCheck.Data;         // IUtilisateurRepository
using PriceCheck.Services;     // ServiceHelper, SessionUtilisateur

namespace PriceCheck.Pages;

public partial class ConnexionPage : ContentPage
{
    // Couleurs de la charte (mêmes codes HEX que dans PriceCheckTheme.xaml)
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    // Accès aux comptes : on demande à MAUI le repository enregistré dans MauiProgram.cs
    private readonly IUtilisateurRepository _comptes = ServiceHelper.Obtenir<IUtilisateurRepository>();

    public ConnexionPage()
    {
        InitializeComponent();
    }

    // Afficher / masquer le mot de passe
    private void OnAfficherMotDePasseTapped(object? sender, TappedEventArgs e)
    {
        ChampMotDePasse.IsPassword = !ChampMotDePasse.IsPassword;
        BoutonAfficher.Text = ChampMotDePasse.IsPassword ? "Afficher" : "Masquer";
    }

    private async void OnMotDePasseOublieTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Mot de passe oublié", "Fonction non disponible dans cette version.", "OK");
    }

    // Appelé quand on appuie sur "Se connecter"
    private async void OnSeConnecterClicked(object? sender, EventArgs e)
    {
        string email = ChampEmail.Text?.Trim() ?? "";
        string motDePasse = ChampMotDePasse.Text ?? "";

        bool emailValide = email.Contains('@') && email.Contains('.');
        bool motDePasseValide = motDePasse.Length >= 8;

        AfficherErreur(CadreEmail, ErreurEmail,
            emailValide ? null : "Saisissez une adresse e-mail valide");
        AfficherErreur(CadreMotDePasse, ErreurMotDePasse,
            motDePasseValide ? null : "Le mot de passe doit contenir au moins 8 caractères");

        if (!emailValide || !motDePasseValide)
        {
            return;
        }

        // On désactive le bouton pendant la vérification (le calcul prend un instant)
        var bouton = sender as Button;
        if (bouton != null) bouton.IsEnabled = false;

        try
        {
            // Vérification RÉELLE dans la base : null = e-mail inconnu ou mot de passe faux
            var utilisateur = await _comptes.ConnecterAsync(email, motDePasse);

            if (utilisateur == null)
            {
                // Même message dans les deux cas : on ne révèle pas si l'e-mail existe
                AfficherErreur(CadreMotDePasse, ErreurMotDePasse, "E-mail ou mot de passe incorrect");
                return;
            }

            SessionUtilisateur.Ouvrir(utilisateur.Email);
            await Shell.Current.GoToAsync("//AccueilPage");
        }
        finally
        {
            if (bouton != null) bouton.IsEnabled = true;
        }
    }

    // Va vers l'écran Inscription (route déclarée dans AppShell.xaml)
    private async void OnCreerUnCompteClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//InscriptionPage");
    }

    // Montre ou cache l'erreur d'un champ (texte = null : pas d'erreur)
    private static void AfficherErreur(Border cadre, Label message, string? texte)
    {
        bool enErreur = texte != null;
        message.Text = texte;
        message.IsVisible = enErreur;
        cadre.Stroke = new SolidColorBrush(enErreur ? CouleurErreur : CouleurBordureChamp);
        cadre.StrokeThickness = enErreur ? 2 : 1;
    }
}