using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SteamPlaytimeTracker.Utility.Converter;

internal sealed class EnumToStringConverterExtension : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if(value is Enum enumValue)
		{
			return enumValue.ToString();
		}
		return string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if(value is string strValue && targetType.IsEnum)
		{
			if(Enum.TryParse(targetType, strValue, out var result))
			{
				return result!;
			}
		}
		return Activator.CreateInstance(targetType)!;
	}
}
