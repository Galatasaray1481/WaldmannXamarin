using Waldmann.Presentation.Navigation;

namespace Waldmann.Testing;

/// <summary>Records navigation instead of performing it.</summary>
public sealed class RecordingNavigationService : INavigationService
{
    private readonly List<string> routes = [];

    public IReadOnlyList<string> Routes => routes;

    public string? LastRoute => routes.Count == 0 ? null : routes[^1];

    public int BackNavigations { get; private set; }

    public Task GoToAsync(string route)
    {
        routes.Add(route);
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        BackNavigations++;
        return Task.CompletedTask;
    }
}

/// <summary>Records dialogs instead of showing them.</summary>
public sealed class RecordingDialogService : IDialogService
{
    private readonly List<(string Title, string Message)> dialogs = [];

    public IReadOnlyList<(string Title, string Message)> Dialogs => dialogs;

    public (string Title, string Message)? LastDialog => dialogs.Count == 0 ? null : dialogs[^1];

    public Task ShowMessageAsync(string title, string message)
    {
        dialogs.Add((title, message));
        return Task.CompletedTask;
    }
}
