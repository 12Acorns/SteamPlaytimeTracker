using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Services.Localization;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;

namespace SteamPlaytimeTracker.Extensions;

[MarkupExtensionReturnType(typeof(string))]
internal class LocalizationExtensions : MarkupExtension
{
	public string Key { get; set; } = "[No Key]";

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if(DesignerProperties.GetIsInDesignMode(new DependencyObject()))
		{
			return Key;
		}
		if(App.ServiceProvider.GetService<ILocalizationService>() is var locService && locService is null)
		{
			return Key;
		}
		var binding = new Binding()
		{
			Path = new PropertyPath($"[{Key}]"),
			Source = locService,
			Mode = BindingMode.OneWay,
			UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
		};
		return binding.ProvideValue(serviceProvider);
	}
}