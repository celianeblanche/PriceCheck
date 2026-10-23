using Microsoft.Maui.Graphics; // pour le type Color
using PriceCheck.Data;         // IUtilisateurRepository
using PriceCheck.Services;     // ServiceHelper, SessionUtilisateur

namespace PriceCheck.Pages;

public partial class InscriptionPage : ContentPage
{
    // Couleurs de la charte (mêmes codes que PriceCheckTheme.xaml)
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    // Accès aux comptes : on demande à MAUI le repository enregistré dans MauiProgram.cs
    private readonly IUtilisateurRepository _comptes = ServiceHelper.Obtenir<IUtilisateurRepository>();

    public InscriptionPage()
    {
        InitializeComponent();
    }

    // Afficher / masquer le mot de passe
    private void OnAfficherMotDePasseTapped(object? sender, TappedEventArgs e)
    {
        ChampMotDePasse.IsPassword = !ChampMotDePasse.IsPassword;
        BoutonAfficher.Text = ChampMotDePasse.IsPassword ? "Afficher" : "Masquer";
    }

    // Appelé quand on appuie sur "Créer mon compte"
    private async void OnCreerMonCompteClicked(object? sender, EventArgs e)
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
            return; // erreur : on s'arrête
        }

        // On désactive le bouton pendant le calcul : un double appui ne crée pas deux comptes
        var bouton = sender as Button;
        if (bouton != null) bouton.IsEnabled = false;

        try
        {
            // Création RÉELLE du compte dans la base (false = e-mail déjà utilisé)
            bool cree = await _comptes.CreerCompteAsync(email, motDePasse);

            if (!cree)
            {
                AfficherErreur(CadreEmail, ErreurEmail, "Un compte existe déjà avec cet e-mail");
                return;
            }

            // Le compte est créé : la personne est directement connectée
            SessionUtilisateur.Ouvrir(email.ToLowerInvariant());
            await Shell.Current.GoToAsync("//AccueilPage");
        }
        finally
        {
            // Quoi qu'il arrive (succès ou erreur), on réactive le bouton
            if (bouton != null) bouton.IsEnabled = true;
        }
    }

    // Retour vers Connexion.
    // "//ConnexionPage" = la route déclarée dans AppShell.xaml ("//" = depuis la racine du Shell).
    private async void OnDejaUnCompteClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ConnexionPage");
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