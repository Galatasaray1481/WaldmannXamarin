using Microsoft.Extensions.DependencyInjection;
using Waldmann.Core.Accounts;
using Waldmann.Core.Chats;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Infrastructure.InMemory;

namespace Waldmann.Infrastructure.Tests;

public class PasswordHasherTests
{
    [Test]
    public void Derive_MatchesRfc7914TestVector()
    {
        // RFC 7914, section 11: PBKDF2-HMAC-SHA256, P = "passwd", S = "salt", c = 1 (first 32 bytes).
        var derived = PasswordHasher.Derive("passwd", System.Text.Encoding.ASCII.GetBytes("salt"), iterations: 1);

        Assert.That(Convert.ToHexStringLower(derived), Is.EqualTo("55ac046e56e3089fec1691c22544b605f94185216dde0465e68b9d57c20dacbc"));
    }

    [Test]
    public void Hash_IsSaltedAndVerifiable()
    {
        var first = PasswordHasher.Hash("geheim123");
        var second = PasswordHasher.Hash("geheim123");

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.EqualTo(second), "salted");
            Assert.That(first, Does.Not.Contain("geheim123"));
            Assert.That(PasswordHasher.Verify("geheim123", first), Is.True);
            Assert.That(PasswordHasher.Verify("Geheim123", first), Is.False);
        });
    }

    [TestCase("")]
    [TestCase("nodot")]
    [TestCase("a.b.c")]
    [TestCase("AAAA.AAAA")]
    public void Verify_MalformedHash_IsFalse(string stored) => Assert.That(PasswordHasher.Verify("x", stored), Is.False);
}

public class InMemoryAccountRepositoryTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    [Test]
    public async Task DemoAccount_CanSignIn()
    {
        var result = await new InMemoryAccountRepository().SignInAsync(InfrastructureServiceCollectionExtensions.DemoCredentials, None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.DisplayName, Is.EqualTo("Max Mustermann"));
            Assert.That(result.Value.Role, Is.EqualTo(UserRole.Hunter));
        });
    }

    [TestCase("demo@waldmann.example", "falsch")]
    [TestCase("unbekannt@waldmann.example", "waldmann")]
    public async Task WrongLogin_IsInvalidCredentials(string email, string password)
    {
        var result = await new InMemoryAccountRepository().SignInAsync(new Credentials(email, password), None);

        Assert.That(result.Error!.Kind, Is.EqualTo(ErrorKind.InvalidCredentials));
    }

    [Test]
    public async Task Register_ThenSignInWithDifferentEmailCasing()
    {
        var repository = new InMemoryAccountRepository();

        var registered = await repository.RegisterAsync(new Registration("Jana", "Jäger", "Jana@Revier.de", "geheim123", UserRole.Hunter), None);
        var signedIn = await repository.SignInAsync(new Credentials("jana@revier.de", "geheim123"), None);

        Assert.That(signedIn.Value.Id, Is.EqualTo(registered.Value.Id));
    }

    [Test]
    public async Task Register_TakenEmail_IsRejected()
    {
        var result = await new InMemoryAccountRepository()
            .RegisterAsync(new Registration("X", "Y", "DEMO@waldmann.example", "geheim123", UserRole.GameMeatBuyer), None);

        Assert.That(result.Error!.Kind, Is.EqualTo(ErrorKind.AccountAlreadyExists));
    }

    [Test]
    public void Cancelled_Throws()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.That(() => new InMemoryAccountRepository().SignInAsync(InfrastructureServiceCollectionExtensions.DemoCredentials, cancellation.Token), Throws.InstanceOf<OperationCanceledException>());
    }
}

public class InMemoryContentTests
{
    [Test]
    public async Task Offers_AreTheSixLegacyOffers()
    {
        var offers = await new InMemoryOfferRepository().GetOffersAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(offers.Select(o => o.Title), Is.EquivalentTo(new[]
            {
                "Wildschwein Nackensteaks", "Hirschsteak vom Rückenfilet", "Wildschweinrücken ohne Knochen",
                "Hirschsteak aus Keule", "Rehsteak aus Keule", "Wildschweinsteak aus Keule",
            }));
            Assert.That(offers.Single(o => o.Title == "Wildschwein Nackensteaks").PricePerKg, Is.EqualTo(34.90m));
            Assert.That(offers.Where(o => o.Title != "Wildschwein Nackensteaks").Select(o => o.PricePerKg), Has.All.EqualTo(69.90m));
            Assert.That(offers.Select(o => o.Id), Is.Unique);
            Assert.That(offers.Select(o => o.Rating), Has.All.InRange(0, GameMeatOffer.MaximumRating));
        });
    }

    [Test]
    public async Task EveryOfferImage_IsEmbeddedInTheApp()
    {
        // The images live in src/Waldmann/Images; a renamed file would otherwise only show up as an empty tile.
        var imageFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "src", "Waldmann", "Images");
        var offers = await new InMemoryOfferRepository().GetOffersAsync(CancellationToken.None);

        Assert.That(offers.Select(o => Path.Combine(imageFolder, o.ImageKey + ".png")), Has.All.Matches<string>(File.Exists));
    }

    [Test]
    public async Task Chat_HasTheLegacySampleMessages()
    {
        var messages = await new InMemoryChatRepository().GetMessagesAsync(UserId.New(), CancellationToken.None);

        Assert.That(messages, Is.EqualTo(new[] { new ChatMessage("Jana", "Jägerin", "Hallo"), new ChatMessage("Flora", "Wildbretkäuferin", "Guten Tag") }));
    }

    [Test]
    public void Container_ResolvesEveryPortAsSingleton()
    {
        using var provider = new ServiceCollection().AddWaldmannInfrastructure().BuildServiceProvider(validateScopes: true);
        var accounts = provider.GetRequiredService<IAccountRepository>();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IAccountRepository>(), Is.SameAs(accounts));
            Assert.That(provider.GetRequiredService<IOfferRepository>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IChatRepository>(), Is.Not.Null);
        });
    }
}
