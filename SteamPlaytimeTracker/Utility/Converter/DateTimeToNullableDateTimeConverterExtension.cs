using System.Globalization;
using System.Windows.Data;

namespace SteamPlaytimeTracker.Utility.Converter;

internal sealed class DateTimeToNullableDateTimeConverterExtension : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if(value is DateTime dt)
		{
			return (DateTime?)dt;
		}
		return value;
	}
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if(value is DateTime dt)
		{
			return dt;
		}
		return value;
	}
}