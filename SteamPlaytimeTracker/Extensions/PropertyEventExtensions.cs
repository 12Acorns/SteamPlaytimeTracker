using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.Extensions;

internal static class PropertyEventExtensions
{
	public static void OnPropertyChanged<T>(this PropertyChangedEventHandler? propertyChangedEventHandler, T self, [CallerMemberName] string? propName = null) =>
		propertyChangedEventHandler?.Invoke(self, new PropertyChangedEventArgs(propName));
}
