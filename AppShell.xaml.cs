namespace PriceCheck;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages qui ne sont pas des onglets : on enregistre leur route pour pouvoir y naviguer.
        Routing.RegisterRoute("ResultatPage", typeof(Pages.ResultatPage));
        Routing.RegisterRoute("RelevePage", typeof(Pages.RelevePage));
    }
}
