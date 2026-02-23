namespace SteamPlaytimeTracker.Core;

internal abstract class MenuModel : ObservableObject
{
	internal bool IsConstructed { get; private set; } = false;

	internal required string Title
	{
		get;
		init
		{
			field = value;
			OnPropertyChanged();
		}
	}

	public virtual void OnConstructed()
	{
		IsConstructed = true;
	}
	public virtual void OnLoad(params object[] args) { }
}