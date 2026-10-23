using Microsoft.Extensions.DependencyInjection;

namespace PriceCheck.Services;

// Petit outil pour récupérer un service enregistré dans MauiProgram.cs.
// Exemple : ServiceHelper.Obtenir<IProduitRepository>()
public static class ServiceHelper
{
    public static T Obtenir<T>() where T : notnull
        => IPlatformApplication.Current!.Services.GetRequiredService<T>();
}