using Microsoft.Win32;
using ScottPlot.WPF;
using Serilog;
using SkiaSharp.Views.WPF;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Graphing.Data;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.Services.DataTransfer;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Utility;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal class SteamAppViewModel : Core.ViewModel
{
	private static readonly SolidColorBrush _defaultGray = (Application.Current.FindResource("defaultGray") as SolidColorBrush)!;
	private static readonly ScottPlot.Color _defaultGrayScottColour = ScottPlot.Color.FromSKColor(_defaultGray.Color.ToSKColor());

	private readonly List<EventHandler<MouseButtonEventArgs>> _trackedEvents = [];
	private readonly ILocalizationService _localizationService;
	private readonly ILogger _logger;

	public SteamAppViewModel(INavigationService navigationService, ILocalizationService localizationService, 
		ILogger logger, ExportService exportService)
	{
		NavigationService = navigationService;
		_localizationService = localizationService;
		_logger = logger;
		SwitchBackToHomeViewCommand = new RelayCommand(o =>
		{
			NavigationService.NavigateTo<HomeViewModel>();
		});
		ImportPlaytimeCommand = new RelayCommand(o =>
		{
			//PlaytimeUtility.CorrectIntervalOverlapV2();
		});
		ExportPlaytimeCommand = new RelayCommand(o =>
		{
			// TODO: Move export logic to a service and inject here. Also add service to sanatise file name and handle edge cases like no playtime data, or failed exports
			var exportDir = Path.Combine(ApplicationPath.GetPath(GlobalData.AppDataStoreLookupName), "Exports");
			var saveFileDialog = new SaveFileDialog
			{
				Filter = "JSON Files (*.json)|*.json",
				DefaultExt = "json",
				AddExtension = true,
				FileName = $"{SelectedApp.Name.Replace(':', ' ')}_{SelectedApp.AppId}_PlaytimeExport_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json",
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
				exportService.ExportPlaytimeData(stream, SelectedApp);
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
		PlaytimePlot = new();

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
		EndDatePickerVisiblity = Visibility.Visible;
	}

	public INavigationService NavigationService { get; }
	public RelayCommand SwitchBackToHomeViewCommand { get; }
	public RelayCommand ImportPlaytimeCommand { get; }
	public RelayCommand ExportPlaytimeCommand { get; }

	public WpfPlot PlaytimePlot { get; private set; }
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
	public Visibility EndDatePickerVisiblity
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public ImageSource CapsuleImage
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public AppDTO SelectedApp
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
	public int ImageWidth => 150;
	public int ImageHeight => (int)(150 * SteamCapsule.HeightScaleFactor);

	public override void OnLoad(params object[] args)
	{

		GraphingDates = new(DateTime.Now, () => OnPropertyChanged(nameof(GraphingDates)));
		GraphingDates.SetMinimumAllowedDate(DateTime.Now.AddMonths(-1));
		GraphingDates.SetMaximumAllowedDate(DateTime.Now.AddMonths(1));
		if(args is not [AppDTO app, ..])
		{
#if DEBUG_TEST_STEAM_APP_VIEW
			_logger.Warning("DEBUG_TEST_STEAM_APP_VIEW is currently enabled. No exception thrown.");
			return;
#else
			throw new ArgumentException("Expected a SteamApp object as the first argument.");
#endif
		}

		SelectedApp = app;
		SelectedGraphingOption = AvailableGraphingOptions.FirstOrDefault(x => x.IsSelectedByDefault);
		CapsuleImage = LoadBitmap(SelectedApp.CapsuleImageUrl);

		PlaytimePlot = new();
		PlaytimePlot.ConfigureDarkTheme();

		var totalPlaytimeHours = SelectedApp.GetTotalPlaytime().TotalHours;
		TotalPlaytimeText = _localizationService.GetFormattedTranslation(
			key: GlobalData.LocPlaytimeHoursText,
			paramaters: (Key: "Playtime Hours", Value: $"{totalPlaytimeHours:n2}"));
	}

	private BitmapImage LoadBitmap(string url)
	{
		var bmp = new BitmapImage();
		bmp.BeginInit();
		bmp.UriSource = new Uri(url, UriKind.Absolute);
		bmp.CacheOption = BitmapCacheOption.OnLoad;
		bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
		bmp.DecodePixelWidth = ImageWidth;
		bmp.DecodePixelHeight = ImageHeight;
		bmp.EndInit();
		return bmp;
	}
}
