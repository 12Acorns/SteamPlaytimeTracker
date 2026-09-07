using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.Stylers;
using ScottPlot.WPF;
using SkiaSharp.Views.WPF;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DataTransfer;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Graphing.Data;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.Services.DataTransfer;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Utility;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal class SteamAppViewModel : Core.ViewModel
{
	private static readonly SolidColorBrush _defaultGray = (Application.Current.FindResource("defaultGray") as SolidColorBrush)!;
	private static readonly ScottPlot.Color _defaultGrayScottColour = ScottPlot.Color.FromSKColor(_defaultGray.Color.ToSKColor());

	private readonly List<EventHandler<MouseButtonEventArgs>> _trackedEvents = [];
	private readonly ILocalizationService _localizationService;
	private bool _override = true;

	public SteamAppViewModel(INavigationService navigationService, ILocalizationService localizationService)
	{
		NavigationService = navigationService;
		_localizationService = localizationService;
		SwitchBackToHomeViewCommand = new RelayCommand(o =>
		{
			NavigationService.NavigateTo<HomeViewModel>();
		});
		ImportPlaytimeCommand = new RelayCommand(o =>
		{

		});
		ExportPlaytimeCommand = new RelayCommand(o =>
		{
			// TODO: Move export logic to a service and inject here. Also add service to sanatise file name and handle edge cases like no playtime data, or failed exports
			var app = SelectedApp.SteamApp!;
			var exportDir = Path.Combine(ApplicationPath.GetPath(GlobalData.AppDataStoreLookupName), "Exports");
			var saveFileDialog = new SaveFileDialog
			{
				Filter = "JSON Files (*.json)|*.json",
				DefaultExt = "json",
				AddExtension = true,
				FileName = $"{app.Name.Replace(':', ' ')}_{app.AppId}_PlaytimeExport_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json",
				DefaultDirectory = exportDir,
				InitialDirectory = exportDir
			};
			if(saveFileDialog.ShowDialog() ?? false)
			{
				using var stream = (FileStream)saveFileDialog.OpenFile();
				if(stream is null)
				{
					MessageBox.Show("Failed to open file for export.", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
				ExportService.ExportPlaytimeData(stream, SelectedApp);
				var res = MessageBox.Show(
					messageBoxText: $"Playtime data exported successfully. Path: '{stream.Name}'.\nPress Yes to copy path to clipboard.",
					caption: "Export Successful", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Information);
				if(res is MessageBoxResult.Yes)
				{
					Clipboard.SetText(stream.Name);
				}
				return;
			}
			MessageBox.Show("Failed to open file for export.", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
		});
		Plot = new();

		AvailableGraphingOptions = [
			new GrapthViewSelectionData(GraphViewSelectionId.YearPlaytime, _localizationService[GlobalData.LocAppViewYearPlaytimeViewKey].Text, false),
			new GrapthViewSelectionData(GraphViewSelectionId.MonthPlaytime, _localizationService[GlobalData.LocAppViewMonthPlaytimeViewKey].Text, true),
			new GrapthViewSelectionData(GraphViewSelectionId.DayPlaytime, _localizationService[GlobalData.LocAppViewDayPlaytimeViewKey].Text, false)
		];

		localizationService.PropertyChanged += (sender, args) =>
		{
			AvailableGraphingOptions = [
				new GrapthViewSelectionData(GraphViewSelectionId.YearPlaytime, _localizationService[GlobalData.LocAppViewYearPlaytimeViewKey].Text, false),
				new GrapthViewSelectionData(GraphViewSelectionId.MonthPlaytime, _localizationService[GlobalData.LocAppViewMonthPlaytimeViewKey].Text, true),
				new GrapthViewSelectionData(GraphViewSelectionId.DayPlaytime, _localizationService[GlobalData.LocAppViewDayPlaytimeViewKey].Text, false)
			];
		};
	}

	public INavigationService NavigationService { get; }
	public RelayCommand SwitchBackToHomeViewCommand { get; }
	public RelayCommand ImportPlaytimeCommand { get; }
	public RelayCommand ExportPlaytimeCommand { get; }

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
	public GraphDateTime GraphingDates
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
		}
	}

	public override void OnLoad(params object[] args)
	{
		if(args is not [SteamAppEntry app, ..])
		{
			throw new ArgumentException("Expected a SteamApp object as the first argument.");
		}

		SelectedApp = app;
		SelectedGraphingOption = AvailableGraphingOptions.FirstOrDefault(x => x.IsSelectedByDefault);

		var totalPlaytimeHours = SelectedApp.PlaytimeSlices.Sum(x => x.SessionLength.TotalHours);
		TotalPlaytimeText = _localizationService[GlobalData.LocPlaytimeHoursText, (Key: "Playtime Hours", Value: $"{totalPlaytimeHours:n2}")];
	}
}
