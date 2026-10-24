using PriceCheck.Services; // SessionUtilisateur, NavigationRetour

namespace PriceCheck;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages qui ne sont pas des onglets : on enregistre leur route pour pouvoir y naviguer.
        Routing.RegisterRoute("ResultatPage", typeof(Pages.ResultatPage));
        Routing.RegisterRoute("RelevePage", typeof(Pages.RelevePage));

        // Si une personne est déjà connectée (session gardée par SessionUtilisateur),
        // on ouvre directement la barre d'onglets (Accueil) au lieu de l'écran Connexion.
        if (SessionUtilisateur.EstConnecte)
        {
            CurrentItem = Items.OfType<TabBar>().First();
        }
    }

    // Appelée quand on appuie sur le bouton retour du téléphone (ou qu'on fait le geste retour).
    // true = "je m'en suis occupé" (l'appli ne se ferme pas) ; false = Android ferme l'appli.
    protected override bool OnBackButtonPressed()
    {
        if (NavigationRetour.Gerer())
        {
            return true;
        }
        return base.OnBackButtonPressed();
    }
}