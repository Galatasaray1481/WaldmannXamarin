using Waldmann.Core.Accounts;
using Waldmann.Core.Chats;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Core.Validation;

namespace Waldmann.Core.Tests;

public class ResultAndErrorTests
{
    [Test]
    public void Success_HasNoError()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Result.Success().IsSuccess, Is.True);
            Assert.That(Result.Success().Error, Is.Null);
        });
    }

    [Test]
    public void GenericFailure_ValueThrows()
    {
        Result<int> result = Error.Of(ErrorKind.NotSignedIn);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(() => result.Value, Throws.InvalidOperationException);
        });
    }

    [Test]
    public void GenericSuccess_ExposesValue()
    {
        Result<string> result = "ok";

        Assert.That(result.Value, Is.EqualTo("ok"));
    }

    [Test]
    public void ErrorFactories_GuardTheirInvariants()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => Error.Of(ErrorKind.Validation), Throws.ArgumentException);
            Assert.That(() => Error.Validation([]), Throws.ArgumentException);
            Assert.That(() => Result.Failure(null!), Throws.ArgumentNullException);
            Assert.That(Error.Validation([ValidationIssue.EmailInvalid]).Issues, Is.EqualTo(new[] { ValidationIssue.EmailInvalid }));
        });
    }
}

public class AccountRulesTests
{
    [TestCase("jana@revier.de")]
    [TestCase("jana.jaeger@forst-verwaltung.museum")]
    [TestCase(" jana@revier.de ")]
    public void IsValidEmail_AcceptsRealAddresses(string email) => Assert.That(AccountRules.IsValidEmail(email), Is.True);

    [TestCase(null)]
    [TestCase("")]
    [TestCase("jana")]
    [TestCase("jana@revier")]
    [TestCase("ja na@revier.de")]
    public void IsValidEmail_RejectsMalformedAddresses(string? email) => Assert.That(AccountRules.IsValidEmail(email), Is.False);

    [Test]
    public void ValidateCredentials_ReportsBothFields()
    {
        Assert.That(AccountRules.ValidateCredentials("kaputt", ""), Is.EqualTo(new[] { ValidationIssue.EmailInvalid, ValidationIssue.PasswordRequired }));
    }

    [Test]
    public void ValidateRegistration_EmptyForm_ReportsEveryIssue()
    {
        var issues = AccountRules.ValidateRegistration(null, " ", "x", "kurz", "anders", hasRole: false);

        Assert.That(issues, Is.EqualTo(new[]
        {
            ValidationIssue.FirstNameRequired,
            ValidationIssue.LastNameRequired,
            ValidationIssue.EmailInvalid,
            ValidationIssue.PasswordTooShort,
            ValidationIssue.PasswordsDoNotMatch,
            ValidationIssue.RoleRequired,
        }));
    }

    [Test]
    public void ValidateRegistration_ValidForm_HasNoIssues()
    {
        var password = new string('x', AccountRules.MinimumPasswordLength);

        Assert.That(AccountRules.ValidateRegistration("Jana", "Jäger", "jana@revier.de", password, password, hasRole: true), Is.Empty);
    }
}

public class AccountUseCaseTests
{
    private static readonly UserAccount Jana = new(UserId.New(), "Jana", "Jäger", "jana@revier.de", UserRole.Hunter);

    [Test]
    public async Task SignIn_Success_StartsSessionWithTrimmedEmail()
    {
        var accounts = new Mock<IAccountRepository>(MockBehavior.Strict);
        accounts.Setup(a => a.SignInAsync(new Credentials("jana@revier.de", "geheim123"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAccount>.Success(Jana));
        var session = new UserSession();

        var result = await new SignInUseCase(accounts.Object, session).ExecuteAsync(" jana@revier.de ", "geheim123");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(session.CurrentAccount, Is.EqualTo(Jana));
        });
    }

    [Test]
    public async Task SignIn_Rejected_LeavesSessionEmpty()
    {
        var accounts = new Mock<IAccountRepository>();
        accounts.Setup(a => a.SignInAsync(It.IsAny<Credentials>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAccount>.Failure(Error.Of(ErrorKind.InvalidCredentials)));
        var session = new UserSession();

        var result = await new SignInUseCase(accounts.Object, session).ExecuteAsync("jana@revier.de", "falsch");

        Assert.Multiple(() =>
        {
            Assert.That(result.Error!.Kind, Is.EqualTo(ErrorKind.InvalidCredentials));
            Assert.That(session.IsSignedIn, Is.False);
        });
    }

    [Test]
    public async Task SignIn_InvalidInput_DoesNotCallRepository()
    {
        var accounts = new Mock<IAccountRepository>(MockBehavior.Strict);

        var result = await new SignInUseCase(accounts.Object, new UserSession()).ExecuteAsync(null, null);

        Assert.That(result.Error!.Kind, Is.EqualTo(ErrorKind.Validation));
    }

    [Test]
    public async Task Register_ValidInput_PassesTrimmedRegistration()
    {
        var accounts = new Mock<IAccountRepository>(MockBehavior.Strict);
        accounts.Setup(a => a.RegisterAsync(new Registration("Jana", "Jäger", "jana@revier.de", "geheim123", UserRole.Hunter), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserAccount>.Success(Jana));

        var result = await new RegisterUseCase(accounts.Object)
            .ExecuteAsync(new RegistrationInput(" Jana ", " Jäger ", " jana@revier.de ", "geheim123", "geheim123", UserRole.Hunter));

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task Register_InvalidInput_DoesNotCallRepository()
    {
        var accounts = new Mock<IAccountRepository>(MockBehavior.Strict);

        var result = await new RegisterUseCase(accounts.Object)
            .ExecuteAsync(new RegistrationInput("Jana", "Jäger", "jana@revier.de", "geheim123", "geheim123", Role: null));

        Assert.That(result.Error!.Issues, Is.EqualTo(new[] { ValidationIssue.RoleRequired }));
    }

    [Test]
    public void Session_StartAndEnd()
    {
        var session = new UserSession();
        session.Start(Jana);
        var signedIn = session.IsSignedIn;
        session.End();

        Assert.Multiple(() =>
        {
            Assert.That(signedIn, Is.True);
            Assert.That(session.IsSignedIn, Is.False);
        });
    }

    [Test]
    public void SensitiveRecords_DoNotPrintPasswords()
    {
        var texts = new[]
        {
            new Credentials("jana@revier.de", "geheim123").ToString(),
            new Registration("Jana", "Jäger", "jana@revier.de", "geheim123", UserRole.Hunter).ToString(),
            new RegistrationInput("Jana", "Jäger", "jana@revier.de", "geheim123", "geheim123", UserRole.Hunter).ToString(),
        };

        Assert.Multiple(() =>
        {
            Assert.That(texts, Has.None.Contains("geheim123"));
            Assert.That(Jana.DisplayName, Is.EqualTo("Jana Jäger"));
            Assert.That(new UserId(Guid.Empty).ToString(), Is.EqualTo(new string('0', 32)));
        });
    }
}

public class ContentUseCaseTests
{
    [Test]
    public async Task GetOffers_SortsByAnimalThenPrice()
    {
        var offers = new Mock<IOfferRepository>();
        offers.Setup(o => o.GetOffersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Offer("reh", GameAnimal.RoeDeer, 69.90m),
            Offer("hirsch", GameAnimal.RedDeer, 69.90m),
            Offer("wildschwein-teuer", GameAnimal.WildBoar, 69.90m),
            Offer("wildschwein-guenstig", GameAnimal.WildBoar, 34.90m),
        ]);

        var sorted = await new GetOffersUseCase(offers.Object).ExecuteAsync();

        Assert.That(sorted.Select(o => o.Id.Value), Is.EqualTo(new[] { "wildschwein-guenstig", "wildschwein-teuer", "hirsch", "reh" }));
    }

    [Test]
    public async Task GetChatMessages_RequiresSession()
    {
        var chats = new Mock<IChatRepository>(MockBehavior.Strict);

        var result = await new GetChatMessagesUseCase(chats.Object, new UserSession()).ExecuteAsync();

        Assert.That(result.Error!.Kind, Is.EqualTo(ErrorKind.NotSignedIn));
    }

    [Test]
    public async Task GetChatMessages_ReturnsMessagesOfSignedInUser()
    {
        var session = new UserSession();
        var account = new UserAccount(UserId.New(), "Jana", "Jäger", "jana@revier.de", UserRole.Hunter);
        session.Start(account);
        var chats = new Mock<IChatRepository>();
        chats.Setup(c => c.GetMessagesAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync([new ChatMessage("Flora", "Wildbretkäuferin", "Guten Tag")]);

        var result = await new GetChatMessagesUseCase(chats.Object, session).ExecuteAsync();

        Assert.That(result.Value.Single().Content, Is.EqualTo("Guten Tag"));
    }

    private static GameMeatOffer Offer(string id, GameAnimal animal, decimal price) =>
        new(new OfferId(id), id, animal, price, 4, id);
}
