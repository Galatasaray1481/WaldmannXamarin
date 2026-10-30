using System.Reflection;
using System.Runtime.CompilerServices;
using NetArchTest.Rules;
using Waldmann.Core.Common;
using Waldmann.Infrastructure;
using Waldmann.Presentation;

namespace Waldmann.ArchitectureTests;

/// <summary>The layer rules of docs/ARCHITECTURE.md as executable checks.</summary>
public class LayerRulesTests
{
    private static readonly Assembly Core = typeof(Result).Assembly;
    private static readonly Assembly Infrastructure = typeof(InfrastructureServiceCollectionExtensions).Assembly;
    private static readonly Assembly Presentation = typeof(ViewModelBase).Assembly;
    private static readonly Assembly Ui = typeof(Waldmann.App).Assembly;

    [Test]
    public void Core_IsFreeOfFrameworksAndOuterLayers()
    {
        AssertSuccessful(Types.InAssembly(Core).ShouldNot().HaveDependencyOnAny(
            "Waldmann.Infrastructure", "Waldmann.Presentation", "Xamarin", "Microsoft.Extensions", "CommunityToolkit").GetResult());
    }

    [Test]
    public void Infrastructure_DoesNotKnowPresentationOrXamarin()
    {
        AssertSuccessful(Types.InAssembly(Infrastructure).ShouldNot().HaveDependencyOnAny(
            "Waldmann.Presentation", "Xamarin", "CommunityToolkit").GetResult());
    }

    [Test]
    public void Presentation_IsFreeOfXamarinAndAdapters()
    {
        // Keeps every ViewModel testable on plain .NET — and portable to .NET MAUI later.
        AssertSuccessful(Types.InAssembly(Presentation).ShouldNot().HaveDependencyOnAny("Xamarin", "Waldmann.Infrastructure").GetResult());
    }

    [Test]
    public void Pages_UseViewModelsNotAdapters()
    {
        // Only the composition root may see Infrastructure.
        AssertSuccessful(Types.InAssembly(Ui)
            .That().ResideInNamespace("Waldmann.Views")
            .ShouldNot().HaveDependencyOn("Waldmann.Infrastructure")
            .GetResult());
    }

    [Test]
    public void ViewModels_AreSealed()
    {
        AssertSuccessful(Types.InAssembly(Presentation)
            .That().HaveNameEndingWith("ViewModel")
            .Should().BeSealed()
            .GetResult());
    }

    [Test]
    public void UseCases_AreSealed()
    {
        AssertSuccessful(Types.InAssembly(Core).That().HaveNameEndingWith("UseCase").Should().BeSealed().GetResult());
    }

    [Test]
    public void Adapters_AreHiddenBehindTheirPorts()
    {
        AssertSuccessful(Types.InAssembly(Infrastructure)
            .That().HaveNameEndingWith("Repository")
            .Should().NotBePublic()
            .GetResult());
    }

    [TestCaseSource(nameof(ProductionAssemblies))]
    public void NoGlobalMutableState(Assembly assembly)
    {
        // The legacy pages shared the signed-in user and a balance through public static fields
        // (Login.userID, Cart.Cash, …). Static state must be readonly or const.
        var offenders = assembly.GetTypes()
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute)))
            .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(field => !field.IsInitOnly && !field.IsLiteral && !field.IsDefined(typeof(CompilerGeneratedAttribute)))
            .Where(field => !(field.DeclaringType!.Name == "Strings" && field.Name is "resourceMan" or "resourceCulture"))
            .Select(field => $"{field.DeclaringType!.FullName}.{field.Name}")
            .ToList();

        Assert.That(offenders, Is.Empty);
    }

    [Test]
    public void Controls_RulesAreNotVacuous()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Types.InAssembly(Presentation).That().HaveNameEndingWith("ViewModel").GetTypes().Count(), Is.GreaterThanOrEqualTo(7));
            Assert.That(Types.InAssembly(Core).That().HaveNameEndingWith("UseCase").GetTypes().Count(), Is.GreaterThanOrEqualTo(4));
            Assert.That(Types.InAssembly(Infrastructure).That().HaveNameEndingWith("Repository").GetTypes().Count(), Is.EqualTo(3));
            Assert.That(Types.InAssembly(Ui).That().ResideInNamespace("Waldmann.Views").GetTypes().Count(), Is.GreaterThanOrEqualTo(6));
            AssertSuccessful(Types.InAssembly(Ui).That().ResideInNamespace("Waldmann.Views").Should().HaveDependencyOn("Xamarin.Forms").GetResult());
        });
    }

    private static IEnumerable<Assembly> ProductionAssemblies() => [Core, Infrastructure, Presentation, Ui];

    private static void AssertSuccessful(TestResult result) =>
        Assert.That(result.IsSuccessful, Is.True, "Violations: " + string.Join(", ", result.FailingTypeNames ?? []));
}
