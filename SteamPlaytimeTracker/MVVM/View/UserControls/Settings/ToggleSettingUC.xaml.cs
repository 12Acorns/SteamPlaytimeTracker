using SteamPlaytimeTracker.Core;
using System.Windows.Controls;
using System.Windows;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using SteamPlaytimeTracker.Extensions;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Settings;

public partial class ToggleSettingUC : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty ToggleCommandProperty =
		DependencyProperty.Register(nameof(ToggleCommand), typeof(RelayCommand), typeof(ToggleSettingUC));

	public ToggleSettingUC()
	{
		InitializeComponent();
		DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Text
	{
		get => field;
		set
		{
			var text = value;
			if(IsMultiColour(value))
			{
				text = Colourize(value);
			}
			field = text;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
	public RelayCommand ToggleCommand
	{
		get => (RelayCommand)GetValue(ToggleCommandProperty);
		set => SetValue(ToggleCommandProperty, value);
	}


	private static bool IsMultiColour(ReadOnlySpan<char> text) => text.Contains("[Foreground=", StringComparison.Ordinal);
	private string Colourize(string text)
	{
		var textSpan = text.AsSpan();
		tbl_Text.Inlines.Clear();
		var finalText = new StringBuilder();
		var offset = text.Length - textSpan.Length;
		bool isFirst = true;
		while(IsMultiColour(textSpan))
		{
			var idx = textSpan.IndexOf("[Foreground=", StringComparison.Ordinal);
			var endIdx = textSpan.IndexOf(']');
			var equalIdx = textSpan.IndexOf('=');
			var colourSegment = text[(equalIdx + 1 + offset)..(endIdx + offset)];

			if(isFirst)
			{
				tbl_Text.Inlines.Add(new Run(text[offset..(idx + offset)])
				{
					Foreground = Foreground,
				});
				isFirst = false;
			}

			finalText.Append(textSpan[..idx]);
			textSpan = textSpan[(endIdx + 1)..];
			offset = text.Length - textSpan.Length;

			var nextIdx = textSpan.IndexOf('[');
			if(nextIdx == -1)
			{
				nextIdx = textSpan.Length;
			}

			var addText = text[offset..(nextIdx + offset)];
			tbl_Text.Inlines.Add(new Run(addText)
			{
				Foreground = new BrushConverter().ConvertFromString(colourSegment) as Brush,
			});
		}
		finalText.Append(textSpan);
		return finalText.ToString();
	}
}
