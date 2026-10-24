using Microsoft.Maui.Graphics;
using PriceCheck.Services; // NavigationRetour

namespace PriceCheck.Controls;

public partial class BarreRetour : ContentView
{
    // Propriété personnalisable depuis le XAML : <controls:BarreRetour CouleurTexte="White" />
    // (utile sur l'écran Scanner, dont le fond est sombre). Sans valeur, le texte garde le bleu des liens.
    public static readonly BindableProperty CouleurTexteProperty = BindableProperty.Create(
        nameof(CouleurTexte), typeof(Color), typeof(BarreRetour), null,
        propertyChanged: (bindable, ancienne, nouvelle) =>
        {
            // Quand la valeur change, on l'applique au texte
            if (bindable is BarreRetour barre && nouvelle is Color couleur)
            {
                barre.LabelRetour.TextColor = couleur;
            }
        });

    public Color? CouleurTexte
    {
        get => (Color?)GetValue(CouleurTexteProperty);
        set => SetValue(CouleurTexteProperty, value);
    }

    public BarreRetour()
    {
        InitializeComponent();
    }

    // Appui sur "← Retour" : même logique que le bouton retour du téléphone
    private void OnRetourTapped(object? sender, TappedEventArgs e)
    {
        NavigationRetour.Gerer();
    }
}