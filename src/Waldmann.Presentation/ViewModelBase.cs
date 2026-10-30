using CommunityToolkit.Mvvm.ComponentModel;
using Waldmann.Core.Common;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;

namespace Waldmann.Presentation;

/// <summary>
/// Common ViewModel behaviour: a busy flag against double submission and one way to report failures —
/// validation issues inline under the form, everything else as a dialog.
/// </summary>
public abstract partial class ViewModelBase(IDialogService dialogs) : ObservableObject
{
    protected IDialogService Dialogs { get; } = dialogs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasValidationMessage))]
    private string? validationMessage;

    public bool HasValidationMessage => !string.IsNullOrEmpty(ValidationMessage);

    protected async Task RunBusyAsync(Func<Task> operation)
    {
        Guard.NotNull(operation);
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await operation();
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task ReportFailureAsync(Error error)
    {
        Guard.NotNull(error);
        if (error.Kind == ErrorKind.Validation)
        {
            ValidationMessage = ErrorMessages.For(error);
            return;
        }

        ValidationMessage = null;
        await Dialogs.ShowMessageAsync(Strings.Common_NoticeTitle, ErrorMessages.For(error));
    }
}
