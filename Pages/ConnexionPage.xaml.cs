using Microsoft.Maui.Graphics; // pour le type Color

namespace PriceCheck.Pages;

public partial class ConnexionPage : ContentPage
{
    // Couleurs de la charte (mêmes codes HEX que dans PriceCheckTheme.xaml)
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

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
        await DisplayAlertAsync("Mot de passe oublié", "Fonction prévue avec le backend (non disponible pour le moment).", "OK");
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

        // SIMULATION : aucun compte n'est vérifié. La navigation vers l'Accueil sera ajoutée avec cet écran.
        await Shell.Current.GoToAsync("//AccueilPage"); // connexion SIMULÉE : tout identifiant valide est accepté
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
