namespace PriceCheck.Services;

// Logique de "retour", partagée par le bouton du téléphone ET la flèche "← Retour" de chaque page.
public static class NavigationRetour
{
    // Renvoie true  = "retour géré par nous" (l'appli ne se ferme pas).
    // Renvoie false = rien à faire de spécial : Android peut fermer l'appli.
    public static bool Gerer()
    {
        var shell = Shell.Current;
        if (shell == null)
        {
            return false;
        }

        // Où est-on ? Exemple de valeur : "//Principal/AccueilPage" ou "//Principal/ScannerPage/ResultatPage"
        string route = shell.CurrentState?.Location?.OriginalString ?? "";
        if (route.Length == 0)
        {
            return false; // position inconnue : comportement normal
        }

        // 1) Page ouverte PAR-DESSUS une autre (Résultat, Relevé) : retour à la page précédente.
        //    Ce test passe en premier car la route de Résultat contient aussi celle de l'onglet d'origine.
        if (route.Contains("ResultatPage") || route.Contains("RelevePage"))
        {
            _ = shell.GoToAsync(".."); // ".." = revenir en arrière d'une page
            return true;
        }

        // 2) Inscription : retour vers Connexion
        if (route.Contains("InscriptionPage"))
        {
            _ = shell.GoToAsync("//ConnexionPage");
            return true;
        }

        // 3) Accueil ou Connexion : point de départ, on laisse l'appli se fermer
        if (route.EndsWith("AccueilPage") || route.Contains("ConnexionPage"))
        {
            return false;
        }

        // 4) Tous les autres onglets (Scanner, Ma liste, Historique) : retour à l'Accueil
        _ = shell.GoToAsync("//AccueilPage");
        return true;
    }
}