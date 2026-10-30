using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Waldmann;

/// <summary>
/// Images embedded in this assembly (Images/*.png). Replaces the 24 hand-written
/// <c>ImageSource.FromResource("…")</c> calls of the legacy pages — four of which pointed at images
/// that do not exist in this app (three BurgerTasters resources and "filtericon.png").
/// </summary>
public static class EmbeddedImages
{
    public static ImageSource Get(string key) =>
        ImageSource.FromResource($"Waldmann.Images.{key}.png", typeof(EmbeddedImages).Assembly);
}

/// <summary>Converts an image key from a ViewModel (e.g. <c>offer_reh_keule</c>) into an embedded image.</summary>
public sealed class EmbeddedImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string key && key.Length > 0 ? EmbeddedImages.Get(key) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>XAML: <c>Source="{local:EmbeddedImage waldmann_logo}"</c>.</summary>
[ContentProperty(nameof(Key))]
public sealed class EmbeddedImageExtension : IMarkupExtension<ImageSource>
{
    public string Key { get; set; } = string.Empty;

    public ImageSource ProvideValue(IServiceProvider serviceProvider) => EmbeddedImages.Get(Key);

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}
