using System.Reflection;
using Waldmann.Core.Accounts;
using Waldmann.Core.Chats;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Core.Validation;
using Waldmann.Presentation.Features;
using Waldmann.Presentation.Features.Chats;
using Waldmann.Presentation.Features.Offers;
using Waldmann.Presentation.Features.Profile;
using Waldmann.Presentation.Features.Registration;
using Waldmann.Presentation.Features.SignIn;
using Waldmann.Presentation.Features.Upcoming;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;
using Waldmann.Testing;

namespace Waldmann.Presentation.Tests;

public class SignInViewModelTests
{
    private static readonly Credentials Demo = new("demo@waldmann.example", "waldmann");
    private readonly UserAccount jana = new(UserId.New(), "Jana", "Jäger", "jana@revier.de", UserRole.Hunter);
    private Mock<IAccountRepository> accounts = null!;
    private UserSession session = null!;
    private RecordingNavigationService navigation = null!;
    private RecordingDialogService dialogs = null!;

    [SetUp]
    public void SetUp()
    {
        accounts = new Mock<IAccountRepository>();
        session = new UserSession();
        navigation = new RecordingNavigationService();
        dialogs = new RecordingDialogService();
    }

    [Test]
    public async Task SignIn_Success_OpensOffersAndForgetsPassword()
    {
        accounts.Setup(a => a.SignInAsync(new Credentials("jana@revier.de", "geheim123"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAccount>.Success(jana));
        var viewModel = Create();
        viewModel.Email = "jana@revier.de";
        viewModel.Password = "geheim123";

        await viewModel.SignInCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(navigation.LastRoute, Is.EqualTo(Routes.Offers));
            Assert.That(session.CurrentAccount, Is.EqualTo(jana));
            Assert.That(viewModel.Password, Is.Null);
            Assert.That(viewModel.IsNotBusy, Is.True);
        });
    }

    [Test]
    public async Task SignIn_Rejected_ShowsDialog()
    {
        accounts.Setup(a => a.SignInAsync(It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAccount>.Failure(Error.Of(ErrorKind.InvalidCredentials)));
        var viewModel = Create();
        viewModel.Email = "jana@revier.de";
        viewModel.Password = "falsch";

        await viewModel.SignInCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(dialogs.LastDialog!.Value.Message, Is.EqualTo(Strings.Error_InvalidCredentials));
            Assert.That(navigation.Routes, Is.Empty);
        });
    }

    [Test]
    public async Task SignIn_EmptyForm_ShowsInlineValidation()
    {
        var viewModel = Create();

        await viewModel.SignInCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.HasValidationMessage, Is.True);
            Assert.That(viewModel.ValidationMessage, Is.EqualTo(Strings.Validation_EmailInvalid + Environment.NewLine + Strings.Validation_PasswordRequired));
            Assert.That(dialogs.Dialogs, Is.Empty);
        });
    }

    [Test]
    public void DemoAccount_IsOfferedAndFillsTheForm()
    {
        var viewModel = Create();

        viewModel.UseDemoAccountCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.HasDemoAccount, Is.True);
            Assert.That(viewModel.DemoHint, Does.Contain("demo@waldmann.example / waldmann"));
            Assert.That(viewModel.Email, Is.EqualTo(Demo.Email));
            Assert.That(viewModel.Password, Is.EqualTo(Demo.Password));
        });
    }

    [Test]
    public void WithoutDemoAccount_NoHintAndNothingFilled()
    {
        var viewModel = Create(new DemoAccountInfo(null));

        viewModel.UseDemoAccountCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.HasDemoAccount, Is.False);
            Assert.That(viewModel.DemoHint, Is.Empty);
            Assert.That(viewModel.Email, Is.Null);
        });
    }

    [Test]
    public async Task Register_NavigatesToRegistration()
    {
        await Create().OpenRegistrationCommand.ExecuteAsync(null);

        Assert.That(navigation.LastRoute, Is.EqualTo(Routes.Registration));
    }

    private SignInViewModel Create(DemoAccountInfo? demo = null) =>
        new(new SignInUseCase(accounts.Object, session), demo ?? new DemoAccountInfo(Demo), navigation, dialogs);
}

public class RegistrationViewModelTests
{
    [Test]
    public void Roles_OfferEveryRoleWithGermanLabel()
    {
        var viewModel = Create(new Mock<IAccountRepository>().Object, out _, out _);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Roles.Select(r => r.ToString()), Is.EqualTo(new[] { "Jäger/in", "Wildbretkäufer/in" }));
            Assert.That(viewModel.PasswordHint, Is.EqualTo($"Mindestens {AccountRules.MinimumPasswordLength} Zeichen."));
        });
    }

    [Test]
    public async Task Register_Success_ConfirmsAndGoesBack()
    {
        var accounts = new Mock<IAccountRepository>();
        accounts.Setup(a => a.RegisterAsync(It.Is<Registration>(r => r.Role == UserRole.GameMeatBuyer), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration r, CancellationToken _) => Result<UserAccount>.Success(new UserAccount(UserId.New(), r.FirstName, r.LastName, r.Email, r.Role)));
        var viewModel = Create(accounts.Object, out var navigation, out var dialogs);
        viewModel.FirstName = "Flora";
        viewModel.LastName = "Fleischer";
        viewModel.Email = "flora@markt.de";
        viewModel.Password = "geheim123";
        viewModel.PasswordConfirmation = "geheim123";
        viewModel.SelectedRole = viewModel.Roles[1];

        await viewModel.RegisterCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(dialogs.LastDialog!.Value.Title, Is.EqualTo(Strings.Registration_SuccessTitle));
            Assert.That(navigation.BackNavigations, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Register_WithoutRole_ShowsInlineValidation()
    {
        var viewModel = Create(new Mock<IAccountRepository>(MockBehavior.Strict).Object, out var navigation, out _);
        viewModel.FirstName = "Flora";
        viewModel.LastName = "Fleischer";
        viewModel.Email = "flora@markt.de";
        viewModel.Password = "geheim123";
        viewModel.PasswordConfirmation = "geheim123";

        await viewModel.RegisterCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ValidationMessage, Is.EqualTo(Strings.Validation_RoleRequired));
            Assert.That(navigation.BackNavigations, Is.Zero);
        });
    }

    [Test]
    public async Task Register_TakenEmail_ShowsDialog()
    {
        var accounts = new Mock<IAccountRepository>();
        accounts.Setup(a => a.RegisterAsync(It.IsAny<Registration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAccount>.Failure(Error.Of(ErrorKind.AccountAlreadyExists)));
        var viewModel = Create(accounts.Object, out _, out var dialogs);
        viewModel.FirstName = "Flora";
        viewModel.LastName = "Fleischer";
        viewModel.Email = "flora@markt.de";
        viewModel.Password = "geheim123";
        viewModel.PasswordConfirmation = "geheim123";
        viewModel.SelectedRole = viewModel.Roles[0];

        await viewModel.RegisterCommand.ExecuteAsync(null);

        Assert.That(dialogs.LastDialog!.Value.Message, Is.EqualTo(Strings.Error_AccountAlreadyExists));
    }

    private static RegistrationViewModel Create(IAccountRepository accounts, out RecordingNavigationService navigation, out RecordingDialogService dialogs)
    {
        navigation = new RecordingNavigationService();
        dialogs = new RecordingDialogService();
        return new RegistrationViewModel(new RegisterUseCase(accounts), navigation, dialogs);
    }
}

public class ContentViewModelTests
{
    [Test]
    public async Task Offers_LoadOnceAsGermanTiles()
    {
        var repository = new Mock<IOfferRepository>();
        repository.Setup(r => r.GetOffersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new GameMeatOffer(new OfferId("reh-keule"), "Rehsteak aus Keule", GameAnimal.RoeDeer, 69.90m, 4, "offer_reh_keule")]);
        var viewModel = new OffersViewModel(new GetOffersUseCase(repository.Object), new RecordingDialogService());

        await viewModel.OnAppearingAsync();
        await viewModel.OnAppearingAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Offers.Single(), Is.EqualTo(new OfferTile("Rehsteak aus Keule", "Reh", "69,90\u00A0€ / kg", "★★★★☆", "4 von 5 Sternen", "offer_reh_keule")));
            repository.Verify(r => r.GetOffersAsync(It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task Profile_ShowsSignedInAccount()
    {
        var session = new UserSession();
        session.Start(new UserAccount(UserId.New(), "Flora", "Fleischer", "flora@markt.de", UserRole.GameMeatBuyer));
        var viewModel = new ProfileViewModel(session);

        await viewModel.OnAppearingAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.DisplayName, Is.EqualTo("Flora Fleischer"));
            Assert.That(viewModel.Email, Is.EqualTo("flora@markt.de"));
            Assert.That(viewModel.RoleText, Is.EqualTo("Wildbretkäufer/in"));
        });
    }

    [Test]
    public async Task Profile_WithoutSession_StaysEmpty()
    {
        var viewModel = new ProfileViewModel(new UserSession());

        await viewModel.OnAppearingAsync();

        Assert.That(viewModel.DisplayName, Is.Empty);
    }

    [Test]
    public async Task Chats_ShowSenderWithDescription()
    {
        var session = new UserSession();
        session.Start(new UserAccount(UserId.New(), "Max", "Mustermann", "max@revier.de", UserRole.Hunter));
        var chats = new Mock<IChatRepository>();
        chats.Setup(c => c.GetMessagesAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync([new ChatMessage("Jana", "Jägerin", "Hallo")]);
        var viewModel = new ChatsViewModel(new GetChatMessagesUseCase(chats.Object, session), new RecordingNavigationService(), new RecordingDialogService());

        await viewModel.OnAppearingAsync();
        await viewModel.OnAppearingAsync();

        Assert.That(viewModel.Messages, Is.EqualTo(new[] { new ChatLine("Jana (Jägerin)", "Hallo") }));
    }

    [Test]
    public async Task Chats_WithoutSession_ReturnToSignIn()
    {
        var navigation = new RecordingNavigationService();
        var dialogs = new RecordingDialogService();
        var viewModel = new ChatsViewModel(new GetChatMessagesUseCase(Mock.Of<IChatRepository>(), new UserSession()), navigation, dialogs);

        await viewModel.OnAppearingAsync();

        Assert.Multiple(() =>
        {
            Assert.That(dialogs.LastDialog!.Value.Message, Is.EqualTo(Strings.Error_NotSignedIn));
            Assert.That(navigation.LastRoute, Is.EqualTo(Routes.SignIn));
        });
    }

    [Test]
    public void UpcomingFeatures_HaveTitleDescriptionAndStatus()
    {
        Assert.Multiple(() =>
        {
            foreach (var feature in Enum.GetValues<UpcomingFeature>())
            {
                var viewModel = new UpcomingFeatureViewModel(feature);
                Assert.That(viewModel.Title, Is.Not.Empty, feature.ToString());
                Assert.That(viewModel.Description, Is.Not.Empty, feature.ToString());
                Assert.That(viewModel.Status, Is.EqualTo(Strings.Common_InPreparation));
            }
        });
    }

    [Test]
    public void UnknownUpcomingFeature_Throws()
    {
        var viewModel = new UpcomingFeatureViewModel((UpcomingFeature)99);

        Assert.Multiple(() =>
        {
            Assert.That(() => viewModel.Title, Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => viewModel.Description, Throws.InstanceOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public async Task SignOut_EndsSessionAndShowsSignIn()
    {
        var session = new UserSession();
        session.Start(new UserAccount(UserId.New(), "Max", "Mustermann", "max@revier.de", UserRole.Hunter));
        var navigation = new RecordingNavigationService();

        await new AppShellViewModel(session, navigation).SignOutCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(session.IsSignedIn, Is.False);
            Assert.That(navigation.LastRoute, Is.EqualTo(Routes.SignIn));
        });
    }
}

public class FormattingTests
{
    [TestCase(-1, "☆☆☆☆☆")]
    [TestCase(0, "☆☆☆☆☆")]
    [TestCase(4, "★★★★☆")]
    [TestCase(9, "★★★★★")]
    public void RatingStars_AreClamped(int rating, string expected) => Assert.That(DisplayFormat.RatingStars(rating), Is.EqualTo(expected));

    [Test]
    [SetCulture("en-US")]
    public void PricePerKg_IsAlwaysGerman() => Assert.That(DisplayFormat.PricePerKg(1234.5m), Is.EqualTo("1.234,50\u00A0€ / kg"));

    [Test]
    public void EveryEnumValue_HasALabelOrMessage()
    {
        Assert.Multiple(() =>
        {
            foreach (var role in Enum.GetValues<UserRole>())
            {
                Assert.That(DisplayFormat.Role(role), Is.Not.Empty);
            }

            foreach (var animal in Enum.GetValues<GameAnimal>())
            {
                Assert.That(DisplayFormat.Animal(animal), Is.Not.Empty);
            }

            foreach (var issue in Enum.GetValues<ValidationIssue>())
            {
                Assert.That(ErrorMessages.For(issue), Is.Not.Empty);
            }

            foreach (var kind in Enum.GetValues<ErrorKind>().Where(k => k != ErrorKind.Validation))
            {
                Assert.That(ErrorMessages.For(Error.Of(kind)), Is.Not.Empty);
            }
        });
    }

    [Test]
    public void UnknownEnumValues_Throw()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => DisplayFormat.Role((UserRole)99), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => DisplayFormat.Animal((GameAnimal)99), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => ErrorMessages.For((ValidationIssue)99), Throws.InstanceOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void EveryResourceString_IsDefined()
    {
        var properties = typeof(Strings).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(string));

        Assert.Multiple(() =>
        {
            foreach (var property in properties)
            {
                Assert.That(property.GetValue(null) as string, Is.Not.Null.And.Not.Empty, property.Name);
            }
        });
    }
}
