using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using SkiaSharp.Views.WPF;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Graphing.Data;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.Services.Navigation;
using System.Collections.ObjectModel;
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

	private bool _override = true;

	public SteamAppViewModel(INavigationService navigationService)
	{
		NavigationService = navigationService;
		SwitchBackToHomeViewCommand = new RelayCommand(o =>
		{
			CleanUp();
			NavigationService.NavigateTo<HomeViewModel>();
		});
		Plot = new();

		AvailableGraphingOptions = [
			new GrapthViewSelectionData(GraphViewSelectionId.YearPlaytime, "Year Playtime", false),
			new GrapthViewSelectionData(GraphViewSelectionId.MonthPlaytime, "Month Playtime", true),
			new GrapthViewSelectionData(GraphViewSelectionId.DayPlaytime, "Day Playtime", false)
		];
	}

	public INavigationService NavigationService { get; }
	public RelayCommand SwitchBackToHomeViewCommand { get; }

	public WpfPlot Plot { get; }
	public string TotalPlaytimeText
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public string StartDateText
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public string EndDateText
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public DateTime StartDate
	{
		get;
		set
		{
			if(value > EndDate && !_override && ShowEndDatePicker)
			{
				field = EndDate - TimeSpan.FromDays(1);
			}
			else
			{
				field = value;
			}
			if(!_override)
			{
				RefreshPlots(setStartEndToMinMax: false);
			}
			OnPropertyChanged();
		}
	}
	public DateTime EndDate
	{
		get;
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
			if(!_override)
			{
				RefreshPlots(setStartEndToMinMax: false);
			}
			OnPropertyChanged();
		}
	}
	public DateTime MinStartDate
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public DateTime MaxEndDate
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public bool ShowEndDatePicker
	{
		get;
		set
		{
			field = value;
			EndDatePickerVisiblity = !field ? Visibility.Hidden : Visibility.Visible;
			OnPropertyChanged();
		}
	}
	public Visibility EndDatePickerVisiblity
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public SteamAppEntry SelectedApp
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public ObservableCollection<GrapthViewSelectionData> AvailableGraphingOptions
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public GrapthViewSelectionData SelectedGraphingOption
	{
		get;
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

	public override void OnLoad(params object[] args)
	{
		if(args.Length < 1 || args[0] is not SteamAppEntry app)
		{
			throw new ArgumentException("Expected a SteamApp object as the first argument.");
		}

		SelectedApp = app;
		SelectedGraphingOption = AvailableGraphingOptions.FirstOrDefault(x => x.IsSelectedByDefault);

		InitDateRange();
		InitPlot();
		CreatePlots();

		var hours = SelectedApp.PlaytimeSlices.Sum(x => x.SessionLength.TotalHours);
		// n2 = 2 decimal places
		TotalPlaytimeText = $"Total Playtime: '{hours:n2}' hours";
	}

	private void CreatePlots()
	{
		ShowEndDatePicker = true;
		Plot.Plot.Title(show: true);
		var barPlot = SelectedGraphingOption.Id.Id switch
		{
			GraphViewSelectionId.YearPlaytimeId => CreateYearPlaytimeBars(SelectedApp),
			GraphViewSelectionId.MonthPlaytimeId => CreateMonthPlaytimeBars(SelectedApp),
			GraphViewSelectionId.DayPlaytimeId => CreateDayPlaytimeBars(SelectedApp),
			_ => throw new ArgumentOutOfRangeException(nameof(SelectedGraphingOption), "Invalid graphing option selected.")
		};

		AddEventHandleToPlot(barPlot);

		barPlot.ValueLabelStyle.ForeColor = ScottPlot.Colors.White;
		barPlot.ValueLabelStyle.FontSize = 24;
		Plot.Refresh();
	}
	private void AddEventHandleToPlot(BarPlot barPlot)
	{
		static (DateTime Start, DateTime End) GetStartAndEndOfYear(string yearString)
		{
			var startOfSpecifiedDate = DateTime.ParseExact(yearString, "yyyy", CultureInfo.InvariantCulture);
			var endOfSpecifiedDate = new DateTime(startOfSpecifiedDate.Year + 1, 1, 1);
			return (startOfSpecifiedDate, endOfSpecifiedDate);
		}
		static (DateTime Start, DateTime End) GetStartAndEndOfMonth(string monthName)
		{
			var startOfSpecifiedDate = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture);
			var endOfSpecifiedDate = startOfSpecifiedDate.LastDayOfMonth() + TimeSpan.FromDays(1);

			return (startOfSpecifiedDate, endOfSpecifiedDate);
		}
		static (DateTime Start, DateTime End) GetStartAndEndOfMonthFromMonthNum(int month)
		{
			var startOfSpecifiedDate = new DateTime(DateTime.Now.Year, month, 1);
			var endOfSpecifiedDate = startOfSpecifiedDate.LastDayOfMonth() + TimeSpan.FromDays(1);
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
				if(!bar.Rect.Contains(pos.X, pos.Y))
				{
					return;
				}

				var pipeIdx = bar.Label.IndexOf(' ');
				var dateSpecified = bar.Label[..pipeIdx];

				_override = true;
				(StartDate, EndDate) = SelectedGraphingOption.Id.Id switch
				{
					GraphViewSelectionId.YearPlaytimeId => GetStartAndEndOfYear(dateSpecified),
					GraphViewSelectionId.MonthPlaytimeId => GetStartAndEndOfMonth(dateSpecified),
					GraphViewSelectionId.DayPlaytimeId => GetStartAndEndOfMonthFromMonthNum(StartDate.Month),
					_ => throw new ArgumentOutOfRangeException(nameof(SelectedGraphingOption), "Invalid graphing option selected.")
				};
				SelectedGraphingOption = AvailableGraphingOptions.First(x => x.Id.Id == Math.Min(SelectedGraphingOption.Id.Id + 1, 2));
				_override = false;
				RefreshPlots(setStartEndToMinMax: false);

				e.Handled = true;
			}
			_trackedEvents.Add(OnClickBarEvent);
			HomeWindow.OnMouseDownA += OnClickBarEvent;
		}
	}
	private void RefreshPlots(bool setStartEndToMinMax = true)
	{
		RemoveEventHandlesFromPlot();

		Plot.Plot.Clear();
		RefreshDateRange(setStartEndToMinMax);
		CreatePlots();
		Plot.Plot.Axes.AutoScale();
	}
	private void RefreshDateRange(bool setStartEndToMinMax = true)
	{
		var selectedOption = SelectedGraphingOption.Id.Id;
		var minDate = SelectedApp.PlaytimeSlices.MinBy(x => x.SessionStart.Ticks)!.SessionStart;
		var maxDate = SelectedApp.PlaytimeSlices.MaxBy(x => x.SessionStart.Ticks)!.SessionStart;
		if(selectedOption is GraphViewSelectionId.YearPlaytimeId)
		{
			MinStartDate = new DateTime(minDate.Year, 1, 1);
			MaxEndDate = new DateTime(maxDate.Year + 1, 1, 1);
		}
		else if(selectedOption is GraphViewSelectionId.MonthPlaytimeId)
		{
			MinStartDate = new DateTime(minDate.Year, minDate.Month, 1);
			MaxEndDate = new DateTime(maxDate.Year, maxDate.Month, 1).FirstDayOfNextMonth();
		}
		else if(selectedOption is GraphViewSelectionId.DayPlaytimeId)
		{
			MinStartDate = new DateTime(minDate.Year, minDate.Month, 1);
			MaxEndDate = new DateTime(maxDate.Year, maxDate.Month, 1).FirstDayOfNextMonth();
		}
		if(setStartEndToMinMax)
		{
			_override = true;
			StartDate = MinStartDate;
			EndDate = MaxEndDate;
			_override = false;
		}
	}
	private void RemoveEventHandlesFromPlot()
	{
		foreach(var ev in _trackedEvents)
		{
			HomeWindow.OnMouseDownA -= ev;
		}
		_trackedEvents.Clear();
	}
	private BarPlot CreateYearPlaytimeBars(SteamAppEntry game)
	{
		if(game is null)
		{
			return new BarPlot([]);
		}
		var start = new DateTime(StartDate.Year, 1, 1).Ticks;
		var end = new DateTime(EndDate.Year + 1, 1, 1).Ticks;
		var playtimeByYear = game.PlaytimeSlices.Where(x => x.SessionStart.Ticks >= start && x.SessionStart.Ticks < end)
			.GroupBy(x => x.SessionStart.ToString("yyyy", CultureInfo.InvariantCulture))
			.Select((x, idx) =>
			{
				var playtimeHours = x.Sum(x => x.SessionLength.TotalHours);
				return new Bar()
				{
					Value = playtimeHours,
					ValueLabel = $"{x.Key} | {playtimeHours:n2}",
					Position = idx,
					LineWidth = 1.5f,
					ValueBase = 0,
					FillColor = GlobalData.GetYearPlotColour(DateTime.ParseExact(x.Key, "yyyy", CultureInfo.InvariantCulture).Year),
				};
			});

		Plot.Plot.Title(show: false);
		Plot.Plot.XLabel("Year", 20);

		return Plot.Plot.Add.Bars(playtimeByYear.ToList());
	}
	private BarPlot CreateMonthPlaytimeBars(SteamAppEntry game)
	{
		if(game is null)
		{
			return new BarPlot([]);
		}

		var start = StartDate.Ticks;
		var end = EndDate.Ticks;

		var playtimeByMonth = game.PlaytimeSlices.Where(x => x.SessionStart.Ticks >= start && x.SessionStart.Ticks < end)
			.GroupBy(x => new DateTime(x.SessionStart.Ticks).ToString("MMMM", CultureInfo.InvariantCulture))
			.Select((x, idx) =>
			{
				var playtimeHours = x.Sum(x => x.SessionLength.TotalHours);
				return new Bar()
				{
					Value = playtimeHours,
					ValueLabel = $"{x.Key} | {playtimeHours:n2}",
					Position = idx,
					LineWidth = 1.5f,
					ValueBase = 0,
					FillColor = GlobalData.GetMonthPlotColour(DateTime.ParseExact(x.Key, "MMMM", CultureInfo.InvariantCulture).Month),
				};
			});

		Plot.Plot.Title(StartDate.ToString("yyyy", CultureInfo.InvariantCulture), 24);
		Plot.Plot.XLabel("Month", 20);

		return Plot.Plot.Add.Bars(playtimeByMonth.ToList());
	}
	private BarPlot CreateDayPlaytimeBars(SteamAppEntry game)
	{
		if(game is null)
		{
			return new BarPlot([]);
		}

		var start = new DateTime(StartDate.Year, StartDate.Month, 1);
		var end = start.FirstDayOfNextMonth();

		var playtimeByDay = game.PlaytimeSlices.Where(x => x.SessionStart >= start && x.SessionStart < end)
			.GroupBy(x => x.SessionStart.ToString("dd", CultureInfo.InvariantCulture))
			.Select((x, idx) =>
			{
				var playtimeHours = x.Sum(x => x.SessionLength.TotalHours);
				var dayDateTime = x.FirstOrDefault()?.SessionStart ?? DateTimeOffset.UtcNow;
				return new Bar()
				{
					Value = playtimeHours,
					ValueLabel = $"{x.Key} | {playtimeHours:n2}",
					Position = idx,
					LineWidth = 1.5f,
					ValueBase = 0,
					FillColor = GlobalData.GetDayPlotColour(dayDateTime.DateTime),
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
			AxisColor = ScottPlot.Colors.White,
			GridMajorLineColor = ScottPlot.Colors.Transparent,
		});
		Plot.Plot.FigureBackground = new BackgroundStyle()
		{
			Color = _defaultGrayScottColour,
			AntiAlias = false
		};
	}
	private void InitDateRange()
	{
		_override = true;
		StartDate = new DateTime(SelectedApp.PlaytimeSlices.MinBy(x => x.SessionStart.Ticks)!.SessionStart.Ticks);
		EndDate = new DateTime(SelectedApp.PlaytimeSlices.MaxBy(x => x.SessionStart.Ticks)!.SessionStart.Ticks) + TimeSpan.FromDays(1);
		_override = false;

		MinStartDate = StartDate;
		MaxEndDate = EndDate;

	}
	private void CleanUp()
	{
		RemoveEventHandlesFromPlot();
		Plot.Plot.Clear();
	}
}
