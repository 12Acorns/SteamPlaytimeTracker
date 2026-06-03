using SteamPlaytimeTracker.Services.Process;
using SteamPlaytimeTracker.Core;
using System.Windows.Data;
using SteamPlaytimeTracker.Utility.ProcessTracking;
using SteamPlaytimeTracker.MVVM.View.UserControls.Process;

namespace SteamPlaytimeTracker.MVVM.ViewModel.Window;

internal sealed class ProcessTrackingSelectionWindowModel : MenuModel
{
	private readonly IProcessListingService _processListingService;
	private List<ProcessInfoTrackableUserControl> _viewableTrackables => _processesTracked.Select(x =>
		new ProcessInfoTrackableUserControl(x)).ToList();
	private List<ProcessTrackingInfo> _processesTracked = [];

	// TODO: Load tracked info from db, present those, then fetch currently active processes, add ones which are not already present
	// (done by comparing file paths)
	public ProcessTrackingSelectionWindowModel(IProcessListingService processListingService)
	{
		Title = "Tracked Process Manager";
		_processListingService = processListingService;
	}

	public ListCollectionView ProcessView { get; private set; } = null!;

	public override void OnLoad(params object[] args)
	{
		_processesTracked = _processListingService.GetProcessList().Select(x => new ProcessTrackingInfo(x, true)).ToList();
		ProcessView = (ListCollectionView)CollectionViewSource.GetDefaultView(_viewableTrackables);
	}
}