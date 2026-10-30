using Microsoft.Extensions.DependencyInjection;
using Xamarin.Forms;

namespace Waldmann;

public partial class App : Application
{
    private readonly ServiceProvider services;

    public App()
    {
        // Resources first: the shell's pages use the StaticResources defined in App.xaml.
        InitializeComponent();
        services = CompositionRoot.CreateServiceProvider();
        MainPage = services.GetRequiredService<AppShell>();
    }

    protected override void CleanUp()
    {
        services.Dispose();
        base.CleanUp();
    }
}
