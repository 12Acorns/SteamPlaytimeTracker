using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using SkiaSharp.Views.WPF;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Graphing.Data;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal class SteamAppViewModel : Core.ViewModel
{
	private static readonly SolidColorBrush _defaultGray = (Application.Current.FindResource("defaultGray") as SolidColorBrush)!;
	private static readonly ScottPlot.Color _defaultGrayScottColour = ScottPlot.Color.FromSKColor(_defaultGray.Color.ToSKColor());

	private readonly List<EventHandler<MouseButtonEventArgs>> _trackedEvents = [];
	private readonly INavigationService _navigationService;
	private readonly DbAccess _db;

	private bool _override = true;

	public SteamAppViewModel(INavigationService navigationService, DbAccess db)
	{
		_navigationService = navigationService;
		_db = db;
		SwitchBackToHomeViewCommand = new RelayCommand(o => NavigationService.NavigateTo<HomeViewModel>());
		Plot = new();

		AvailableGraphingOptions = [
			new GrapthViewSelectionData(GraphViewSelectionId.YearPlaytime, "Year Playtime", false),
			new GrapthViewSelectionData(GraphViewSelectionId.MonthPlaytime, "Month Playtime", true),
			new GrapthViewSelectionData(GraphViewSelectionId.DayPlaytime, "Day Playtime", false)
		];
		SelectedGraphingOption = AvailableGraphingOptions.FirstOrDefault(x => x.IsSelectedByDefault);
	}

	public INavigationService NavigationService => _navigationService;
	public RelayCommand SwitchBackToHomeViewCommand { get; set; }

	public WpfPlot Plot { get; }
	public string TotalPlaytimeText
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public string StartDateText
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public string EndDateText
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public DateTime StartDate
	{
		get => field;
		set
		{
			if(value > EndDate && !_override)
			{
				field = EndDate - TimeSpan.FromDays(1);
			}
			else
			{
				field = value;
			}
			RefreshPlots();
			OnPropertyChanged();
		}
	}
	public DateTime EndDate
	{
		get => field;
		set
		{
			if(value < StartDate && !_override)
			{
				field = StartDate + TimeSpan.FromDays(1);
			}
			else
			{
				field = value;
			}
			RefreshPlots();
			OnPropertyChanged();
		}
	}
	public DateTime MinStartDate
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public DateTime MaxEndDate
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public bool ShowEndDatePicker
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public SteamApp SelectedApp
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public ObservableCollection<GrapthViewSelectionData> AvailableGraphingOptions
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public GrapthViewSelectionData SelectedGraphingOption
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
			if(!_override)
			{
				RefreshPlots();
			}
		}
	}

	public override void OnLoad(params scoped ReadOnlySpan<object> args)
	{
		if(args.Length < 1 || args[0] is not SteamApp app)
		{
			throw new ArgumentException("Expected a SteamApp object as the first argument.");
		}

		SelectedApp = app;
		InitDateRange();
		InitPlot();

		CreatePlots();

		TotalPlaytimeText =$"Total Playtime: '{_db.SteamAppEntries
			.Where(x => x.SteamApp.AppId == SelectedApp.Id)
			.First()
				.PlaytimeSegments
				.Sum(x => TimeSpan.FromTicks(x.SessionTimeTicks).TotalHours):n2}' hours";
	}

	private void CreatePlots()
	{
		ShowEndDatePicker = true;
		Plot.Plot.Title(show: true);
		BarPlot barPlot = SelectedGraphingOption.Id.Id switch
		{
			GraphViewSelectionId.YearPlaytimeId => CreateYearPlaytimeBars(),
			GraphViewSelectionId.MonthPlaytimeId => CreateMonthPlaytimeBars(),
			GraphViewSelectionId.DayPlaytimeId => CreateDayPlaytimeBars(),
			_ => throw new ArgumentOutOfRangeException(nameof(SelectedGraphingOption), "Invalid graphing option selected.")
		};

		AddEventHandleToPlot(barPlot);

		barPlot.ValueLabelStyle.ForeColor = ScottPlot.Colors.White;
		barPlot.ValueLabelStyle.FontSize = 24;
		Plot.Refresh();
	}
	private void AddEventHandleToPlot(BarPlot barPlot)
	{
		(DateTime Start, DateTime End) InitAndGetStartAndEnd()
		{
			InitDateRange();
			return (StartDate, EndDate);
		}
		static (DateTime Start, DateTime End) GetStartAndEndOfYear(string yearString)
		{
			var startOfSpecifiedDate = DateTime.ParseExact(yearString, "yyyy", CultureInfo.InvariantCulture);
			var endOfSpecifiedDate = new DateTime(startOfSpecifiedDate.Year, 12, 31, 23, 59, 59, 999);
			return (startOfSpecifiedDate, endOfSpecifiedDate);
		}
		static (DateTime Start, DateTime End) GetStartAndEndOfMonth(string monthName)
		{
			var startOfSpecifiedDate = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture);
			var endOfSpecifiedDate = startOfSpecifiedDate.LastDayOfMonth();

			return (startOfSpecifiedDate, endOfSpecifiedDate);
		}

		foreach(var bar in barPlot.Bars)
		{
			void OnClickBarEvent(object? sender, MouseButtonEventArgs e)
			{
				if((e.LeftButton & MouseButtonState.Pressed) == 0 || e.Handled)
				{
					return;
				}
				var posM = (Vector)Mouse.GetPosition(Plot) * Plot.DisplayScale;
				var pos = Plot.Plot.GetCoordinates(new Pixel(posM.X, posM.Y));
				if(bar.Rect.Contains(pos.X, pos.Y))
				{
					var pipeIdx = bar.Label.IndexOf(' ');
					var dateSpecified = bar.Label[..pipeIdx];

					var (startOfSpecifiedDate, endOfSpecifiedDate) = SelectedGraphingOption.Id.Id switch
					{
						GraphViewSelectionId.YearPlaytimeId => GetStartAndEndOfYear(dateSpecified),
						GraphViewSelectionId.MonthPlaytimeId => GetStartAndEndOfMonth(dateSpecified),
						GraphViewSelectionId.DayPlaytimeId => InitAndGetStartAndEnd(),
						_ => throw new ArgumentOutOfRangeException(nameof(SelectedGraphingOption), "Invalid graphing option selected.")
					};
					_override = true;
					StartDate = startOfSpecifiedDate;
					EndDate = endOfSpecifiedDate;
					_override = false;

					SelectedGraphingOption = AvailableGraphingOptions.First(x => x.Id.Id == SelectedGraphingOption.Id.Id switch
					{
						GraphViewSelectionId.YearPlaytimeId => GraphViewSelectionId.MonthPlaytimeId,
						GraphViewSelectionId.MonthPlaytimeId => GraphViewSelectionId.DayPlaytimeId,
						GraphViewSelectionId.DayPlaytimeId => GraphViewSelectionId.DayPlaytimeId,
						_ => throw new ArgumentOutOfRangeException(nameof(SelectedGraphingOption), "Invalid graphing option selected.")
					});

					e.Handled = true;
				}
			}
			_trackedEvents.Add(OnClickBarEvent);
			HomeWindow.OnMouseDownA += OnClickBarEvent;
		}
	}
	private void RefreshPlots()
	{
		RemoveEventHandlesFromPlot();

		Plot.Plot.Clear();
		CreatePlots();
		Plot.Plot.Axes.AutoScale();
	}
	private void RemoveEventHandlesFromPlot()
	{
		foreach(var ev in _trackedEvents)
		{
			HomeWindow.OnMouseDownA -= ev;
		}
		_trackedEvents.Clear();
	}
	private BarPlot CreateYearPlaytimeBars()
	{
		var game = _db.SteamAppEntries.FirstOrDefault(x => x.SteamApp.AppId == SelectedApp.Id);
		if(game is null)
		{
			return new BarPlot([]);
		}
		var start = new DateTime(StartDate.Year, 1, 1, 0, 0, 0, 0).Ticks;
		var end = new DateTime(EndDate.Year, 12, 31, 23, 59, 59, 999).Ticks;
		int currBarPlotXPos = 0;
		var playtimeByYear = game.PlaytimeSegments.Where(x => x.StartTimeTicks >= start && x.StartTimeTicks <= end)
			.GroupBy(x => new DateTime(x.StartTimeTicks).ToString("yyyy", CultureInfo.InvariantCulture))
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

		Plot.Plot.Title(show: false);
		Plot.Plot.XLabel("Year", 20);

		return Plot.Plot.Add.Bars(playtimeByYear.ToList());
	}
	private BarPlot CreateMonthPlaytimeBars()
	{
		var game = _db.SteamAppEntries.FirstOrDefault(x => x.SteamApp.AppId == SelectedApp.Id);
		if(game is null)
		{
			return new BarPlot([]);
		}

		var start = StartDate.Ticks;
		var end = EndDate.Ticks;

		int currBarPlotXPos = 0;
		var playtimeByMonth = game.PlaytimeSegments.Where(x => x.StartTimeTicks >= start && x.StartTimeTicks <= end)
			.GroupBy(x => new DateTime(x.StartTimeTicks).ToString("MMMM", CultureInfo.InvariantCulture))
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

		Plot.Plot.Title(StartDate.ToString("yyyy", CultureInfo.InvariantCulture), 24);
		Plot.Plot.XLabel("Month", 20);

		return Plot.Plot.Add.Bars(playtimeByMonth.ToList());
	}
	private BarPlot CreateDayPlaytimeBars()
	{
		var game = _db.SteamAppEntries.FirstOrDefault(x => x.SteamApp.AppId == SelectedApp.Id);
		if(game is null)
		{
			return new BarPlot([]);
		}

		var start = new DateTime(StartDate.Year, StartDate.Month, 1);
		var end = start.LastDayOfMonth();

		int currBarPlotXPos = 0;
		var playtimeByDay = game.PlaytimeSegments.Where(x => x.StartTimeTicks >= start.Ticks && x.StartTimeTicks <= end.Ticks)
			.GroupBy(x => new DateTime(x.StartTimeTicks).ToString("dd", CultureInfo.InvariantCulture))
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
		Plot.Plot.Title(start.ToString("MMMM", CultureInfo.InvariantCulture), 24);
		Plot.Plot.XLabel("Day", 20);

		ShowEndDatePicker = false;

		return Plot.Plot.Add.Bars(playtimeByDay.ToList());
	}
	private void InitPlot()
	{
		Plot.UserInputProcessor.IsEnabled = false;

		Plot.Plot.Axes.Margins(bottom: 0);
		Plot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
		Plot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
		Plot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
		Plot.Plot.Axes.Bottom.MinorTickStyle.Length = 0;
		Plot.Plot.YLabel("Playtime (h)", 20);

		Plot.Plot.Axes.Title.Label.ForeColor = ScottPlot.Colors.White;
		Plot.Plot.Axes.Left.TickLabelStyle.FontSize = 20;
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
	private void InitDateRange()
	{
		var app = _db.SteamAppEntries.Where(x => x.SteamApp.AppId == SelectedApp.Id).First();
		StartDate = new DateTime(app.PlaytimeSegments.MinBy(x => x.StartTimeTicks)!.StartTimeTicks);
		EndDate = new DateTime(app.PlaytimeSegments.MaxBy(x => x.StartTimeTicks)!.StartTimeTicks);

		MinStartDate = StartDate;
		MaxEndDate = EndDate;

		_override = false;
	}
}
