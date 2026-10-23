using Microsoft.Extensions.Logging;
using PriceCheck.Data; // BaseDeDonnees, IProduitRepository, SqliteProduitRepository

namespace PriceCheck;

public static class MauiProgram
{
    // Point de départ de l'application : configure les polices et démarre l'App.
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // AddFont("fichier", "alias") : l'alias est le nom utilisé dans FontFamily.
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");   // texte courant
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold"); // boutons du modèle de départ
                fonts.AddFont("Inter-Bold.ttf", "InterBold");         // titres, prix, liens
            });

        // La base de données : une seule instance partagée par toute l'appli (singleton).
        builder.Services.AddSingleton<BaseDeDonnees>();

        // NOUVEAU : quand une page demande IProduitRepository, on lui donne la version SQLite.
        builder.Services.AddSingleton<IProduitRepository, SqliteProduitRepository>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}