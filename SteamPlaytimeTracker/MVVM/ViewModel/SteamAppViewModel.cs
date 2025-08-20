using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Core;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using ScottPlot.WPF;
using ScottPlot;
using SkiaSharp.Views.WPF;
using System.Drawing;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal class SteamAppViewModel : Core.ViewModel
{
	private static readonly SolidColorBrush _defaultGray = (Application.Current.FindResource("defaultGray") as SolidColorBrush)!;
	private static readonly ScottPlot.Color _defaultGrayScottColour = ScottPlot.Color.FromSKColor(_defaultGray.Color.ToSKColor());

	private readonly INavigationService _navigationService;
	private readonly DbAccess _db;

	public SteamAppViewModel(INavigationService navigationService, DbAccess db)
	{
		_navigationService = navigationService;
		_db = db;
		SwitchBackToHomeViewCommand = new RelayCommand(o => NavigationService.NavigateTo<HomeViewModel>());
		Plot = new();
		Plot.UserInputProcessor.IsEnabled = false;

		Plot.Plot.Axes.Margins(bottom: 0);
		Plot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
		Plot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
		Plot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
		Plot.Plot.Axes.Bottom.MinorTickStyle.Length = 0;
		Plot.Plot.YLabel("Playtime (h)", 20);
		Plot.Plot.XLabel("Month", 20);

		Plot.Plot.Axes.Title.Label.ForeColor = ScottPlot.Colors.White;
		Plot.Plot.SetStyle(new PlotStyle()
		{
			AxisColor = ScottPlot.Colors.White
		});
		Plot.Plot.FigureBackground = new BackgroundStyle()
		{
			Color = _defaultGrayScottColour,
			AntiAlias = false
		};
	}

	public INavigationService NavigationService => _navigationService;
	public RelayCommand SwitchBackToHomeViewCommand { get; set; }

	public WpfPlot Plot { get; }
	public SteamApp SelectedApp
	{
		get => field;
		set
		{
			field = value;
			var game = _db.SteamAppEntries.ToList().First(x => x.SteamApp.FromDTO() == field);
			int currBarPlotXPos = 0;
			var playtimeByMonth = game.PlaytimeSegments.GroupBy(x => new DateTime(x.StartTimeTicks).ToString("MMMM", CultureInfo.InvariantCulture))
				.Select(x =>
				{
					var playtimeHours = x.Sum(x => TimeSpan.FromTicks(x.SessionTimeTicks).TotalHours);
					return new Bar()
					{
						Value = playtimeHours,
						ValueLabel = $"{x.Key} | {playtimeHours:n2}",
						Position = currBarPlotXPos++,
						LineWidth = 1.5f,
						ValueBase = 0,
						FillColor = ScottPlot.Color.RandomHue(),
					};
				});
			var bars = Plot.Plot.Add.Bars(playtimeByMonth.ToList());
			bars.ValueLabelStyle.ForeColor = ScottPlot.Colors.White;
			bars.ValueLabelStyle.FontSize = 24;
			Plot.Refresh();
			OnPropertyChanged();
		}
	}
}
