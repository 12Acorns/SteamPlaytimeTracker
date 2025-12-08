using System.Windows;

namespace SteamPlaytimeTracker.Extensions;
internal static class DependencyPropertyExtension
{
	extension(DependencyProperty)
	{
		public static DependencyProperty Register<TElement, TClass>(string name, TElement? elementDefaultValue = default) => 
			DependencyProperty.Register(name, typeof(TElement), typeof(TClass), new PropertyMetadata(elementDefaultValue));
	}
}
