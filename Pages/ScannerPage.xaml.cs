using BarcodeScanning;          // CameraView, BarcodeFormats, Methods, OnDetectionFinishedEventArg
using Microsoft.Maui.Graphics;
using PriceCheck.Data;
using PriceCheck.Services;

namespace PriceCheck.Pages;

public partial class ScannerPage : ContentPage
{
    private static readonly Color CouleurErreur = Color.FromArgb("#DC2626");
    private static readonly Color CouleurBordureChamp = Color.FromArgb("#94A3B8");

    private readonly IProduitRepository _depot = ServiceHelper.Obtenir<IProduitRepository>();

    // La vue caméra EN COURS (null quand la caméra est éteinte).
    // Elle est créée à chaque activation et détruite à chaque arrêt.
    private CameraView? _camera;

    // Vrai pendant qu'on traite un code lu : évite de réagir plusieurs fois au même code-barres
    // (la caméra "voit" le code plusieurs fois par seconde).
    private bool _lectureEnCours;

    // Vrai pendant la demande d'autorisation : évite de lancer deux démarrages avec deux appuis rapides.
    private bool _demarrageEnCours;

    public ScannerPage()
    {
        InitializeComponent();
    }

    // Appelé quand l'onglet Scanner devient visible : la caméra est éteinte, on peut relire un code.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _lectureEnCours = false;
    }

    // Appelé quand on quitte l'onglet : on détruit la caméra (elle consomme de la batterie
    // et doit être libérée pour les autres applis).
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ArreterCamera();
    }

    // Appui sur le cadre : demande l'autorisation de la caméra puis crée et allume la caméra.
    private async void OnCadreCameraTapped(object? sender, TappedEventArgs e)
    {
        if (_camera != null || _demarrageEnCours)
        {
            return; // la caméra tourne déjà (ou démarre) : rien à faire
        }

        _demarrageEnCours = true;
        try
        {
            // Affiche la fenêtre Android "Autoriser PriceCheck à prendre des photos ?" (si pas déjà accordée).
            // Renvoie true si l'autorisation est accordée.
            bool autorise = await Methods.AskForRequiredPermissionAsync();
            if (!autorise)
            {
                await DisplayAlertAsync("Caméra refusée",
                    "Sans accès à la caméra, le scan est impossible. Vous pouvez saisir le code-barres à la main, " +
                    "ou autoriser la caméra dans les réglages Android (Applications > PriceCheck > Autorisations).",
                    "OK");
                return;
            }

            // On crée une vue caméra NEUVE à chaque activation (c'est ce qui règle le problème du 2e scan)
            var camera = new CameraView
            {
                CameraEnabled = true,                       // allume la caméra
                BarcodeSymbologies = BarcodeFormats.Ean13,  // on ne lit que les codes-barres à 13 chiffres
                VibrationOnDetected = true                  // le téléphone vibre quand un code est lu
            };
            camera.OnDetectionFinished += OnDetectionTerminee; // méthode appelée quand un code est lu

            _lectureEnCours = false;
            _camera = camera;

            // Insert(0, ...) : la caméra va au FOND de la pile, le texte reste par-dessus
            ZoneCamera.Children.Insert(0, camera);
            MessageCamera.IsVisible = false; // cache le texte "Appuyez pour activer"
        }
        finally
        {
            _demarrageEnCours = false;
        }
    }

    // Éteint et détruit la caméra, puis remet le texte d'invitation
    private void ArreterCamera()
    {
        var camera = _camera;
        _camera = null; // on met à null d'abord : plus aucun traitement ne la touchera

        if (camera != null)
        {
            camera.OnDetectionFinished -= OnDetectionTerminee; // on se désabonne
            camera.CameraEnabled = false;                      // éteint la caméra
            ZoneCamera.Children.Remove(camera);                // retire la vue de l'écran
            camera.Handler?.DisconnectHandler();               // libère la caméra côté Android
        }

        MessageCamera.IsVisible = true;
    }

    // Appelée par la bibliothèque quand un code-barres vient d'être lu.
    private async void OnDetectionTerminee(object? sender, OnDetectionFinishedEventArg e)
    {
        if (_lectureEnCours)
        {
            return; // un code est déjà en cours de traitement
        }

        // On prend le premier code-barres lu (il peut y en avoir plusieurs dans l'image)
        var resultat = e.BarcodeResults.FirstOrDefault();
        if (resultat == null)
        {
            return;
        }

        _lectureEnCours = true;
        string code = resultat.DisplayValue ?? "";

        // L'événement peut arriver depuis un autre "fil" que celui de l'écran :
        // on revient sur le fil principal avant de toucher à l'interface ou de naviguer.
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (_camera != null)
            {
                _camera.PauseScanning = true; // on arrête de chercher pendant qu'on traite ce code
            }

            var produit = await _depot.TrouverProduitAsync(code);
            if (produit != null)
            {
                // Produit connu : on ouvre sa comparaison (la caméra est détruite en quittant l'écran)
                await Shell.Current.GoToAsync($"ResultatPage?code={code}");
                return;
            }

            // Produit inconnu : on le dit, puis on relance la recherche de codes
            await DisplayAlertAsync("Produit inconnu",
                $"Le code-barres {code} n'est pas encore dans la base de données.", "OK");
            _lectureEnCours = false;
            if (_camera != null)
            {
                _camera.PauseScanning = false;
            }
        });
    }

    // "Comparer" : même contrôle que sur l'Accueil (13 chiffres, puis produit connu)
    private async void OnComparerClicked(object? sender, EventArgs e)
    {
        string code = ChampCode.Text?.Trim() ?? "";

        if (code.Length != 13 || !code.All(char.IsDigit))
        {
            AfficherErreurCode("Saisissez les 13 chiffres du code-barres");
            return;
        }

        // Le code doit exister dans la base de données
        if (await _depot.TrouverProduitAsync(code) == null)
        {
            AfficherErreurCode("Code inconnu. Essayez 6001234567890");
            return;
        }

        AfficherErreurCode(null);
        await Shell.Current.GoToAsync($"ResultatPage?code={code}");
    }

    // Montre ou cache l'erreur du champ (texte = null : pas d'erreur)
    private void AfficherErreurCode(string? texte)
    {
        bool enErreur = texte != null;
        ErreurCode.Text = texte;
        ErreurCode.IsVisible = enErreur;
        CadreCode.Stroke = new SolidColorBrush(enErreur ? CouleurErreur : CouleurBordureChamp);
        CadreCode.StrokeThickness = enErreur ? 2 : 1;
    }
}