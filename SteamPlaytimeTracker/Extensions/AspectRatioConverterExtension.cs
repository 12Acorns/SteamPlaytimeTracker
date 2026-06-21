using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Markup;

namespace SteamPlaytimeTracker.Extensions;

[MarkupExtensionReturnType(typeof(double))]
public sealed class AspectRatioConverterExtension : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if(value is double width && double.TryParse(parameter?.ToString(), out double ratio))
		{
			return width * ratio;
		}
		return DependencyProperty.UnsetValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}