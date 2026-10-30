using Microsoft.Extensions.DependencyInjection;
using Waldmann.Core.Accounts;
using Waldmann.Infrastructure;
using Waldmann.Presentation;
using Waldmann.Presentation.Features;
using Waldmann.Presentation.Features.Chats;
using Waldmann.Presentation.Features.Offers;
using Waldmann.Presentation.Features.Profile;
using Waldmann.Presentation.Features.Registration;
using Waldmann.Presentation.Features.SignIn;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;
using Waldmann.Testing;

namespace Waldmann.IntegrationTests;

/// <summary>
/// The container exactly as the app's CompositionRoot builds it — minus the two Xamarin-only services
/// (Shell navigation and alerts), which are replaced by recorders.
/// </summary>
public class UserJourneyTests
{
    private ServiceProvider provider = null!;
    private RecordingNavigationService navigation = null!;
    private RecordingDialogService dialogs = null!;

    [SetUp]
    public void SetUp()
    {
        navigation = new RecordingNavigationService();
        dialogs = new RecordingDialogService();
        provider = new ServiceCollection()
            .AddWaldmannInfrastructure()
            .AddWaldmannApplication(new DemoAccountInfo(InfrastructureServiceCollectionExtensions.DemoCredentials))
            .AddSingleton<INavigationService>(navigation)
            .AddSingleton<IDialogService>(dialogs)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [TearDown]
    public void TearDown() => provider.Dispose();

    [Test]
    public async Task DemoAccount_SignInBrowseChatSignOut()
    {
        var signIn = provider.GetRequiredService<SignInViewModel>();
        signIn.UseDemoAccountCommand.Execute(null);
        await signIn.SignInCommand.ExecuteAsync(null);
        Assert.That(navigation.LastRoute, Is.EqualTo(Routes.Offers));

        var offers = provider.GetRequiredService<OffersViewModel>();
        await offers.OnAppearingAsync();
        var profile = provider.GetRequiredService<ProfileViewModel>();
        await profile.OnAppearingAsync();
        var chats = provider.GetRequiredService<ChatsViewModel>();
        await chats.OnAppearingAsync();

        Assert.Multiple(() =>
        {
            Assert.That(offers.Offers, Has.Count.EqualTo(6));
            Assert.That(offers.Offers[0].Title, Is.EqualTo("Wildschwein Nackensteaks"), "cheapest wild boar first");
            Assert.That(profile.DisplayName, Is.EqualTo("Max Mustermann"));
            Assert.That(chats.Messages.Select(m => m.Sender), Is.EqualTo(new[] { "Jana (Jägerin)", "Flora (Wildbretkäuferin)" }));
        });

        await provider.GetRequiredService<AppShellViewModel>().SignOutCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IUserSession>().IsSignedIn, Is.False);
            Assert.That(navigation.LastRoute, Is.EqualTo(Routes.SignIn));
        });
    }

    [Test]
    public async Task Registration_ThenSignInWithTheNewAccount()
    {
        var registration = provider.GetRequiredService<RegistrationViewModel>();
        registration.FirstName = "Flora";
        registration.LastName = "Fleischer";
        registration.Email = "flora@markt.de";
        registration.Password = "geheim123";
        registration.PasswordConfirmation = "geheim123";
        registration.SelectedRole = registration.Roles.Single(r => r.Role == UserRole.GameMeatBuyer);
        await registration.RegisterCommand.ExecuteAsync(null);

        var signIn = provider.GetRequiredService<SignInViewModel>();
        signIn.Email = "FLORA@markt.de";
        signIn.Password = "geheim123";
        await signIn.SignInCommand.ExecuteAsync(null);

        var profile = provider.GetRequiredService<ProfileViewModel>();
        await profile.OnAppearingAsync();

        Assert.Multiple(() =>
        {
            Assert.That(navigation.BackNavigations, Is.EqualTo(1), "registration returns to the sign-in page");
            Assert.That(navigation.LastRoute, Is.EqualTo(Routes.Offers));
            Assert.That(profile.RoleText, Is.EqualTo("Wildbretkäufer/in"));
        });
    }

    [Test]
    public async Task SecondRegistrationWithSameEmail_IsRejected()
    {
        var registration = provider.GetRequiredService<RegistrationViewModel>();
        registration.FirstName = "Max";
        registration.LastName = "Zwilling";
        registration.Email = InfrastructureServiceCollectionExtensions.DemoCredentials.Email;
        registration.Password = "geheim123";
        registration.PasswordConfirmation = "geheim123";
        registration.SelectedRole = registration.Roles[0];

        await registration.RegisterCommand.ExecuteAsync(null);

        Assert.That(dialogs.LastDialog!.Value.Message, Is.EqualTo(Strings.Error_AccountAlreadyExists));
    }

    [Test]
    public async Task WrongPassword_KeepsUserOnSignIn()
    {
        var signIn = provider.GetRequiredService<SignInViewModel>();
        signIn.Email = InfrastructureServiceCollectionExtensions.DemoCredentials.Email;
        signIn.Password = "falsch";

        await signIn.SignInCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(dialogs.LastDialog!.Value.Message, Is.EqualTo(Strings.Error_InvalidCredentials));
            Assert.That(navigation.Routes, Is.Empty);
        });
    }

    [Test]
    public void Container_HasOneSessionAndOneShellViewModel()
    {
        var session = provider.GetRequiredService<IUserSession>();
        var shell = provider.GetRequiredService<AppShellViewModel>();
        var signIn = provider.GetRequiredService<SignInViewModel>();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IUserSession>(), Is.SameAs(session));
            Assert.That(provider.GetRequiredService<AppShellViewModel>(), Is.SameAs(shell));
            Assert.That(provider.GetRequiredService<SignInViewModel>(), Is.Not.SameAs(signIn));
        });
    }
}
