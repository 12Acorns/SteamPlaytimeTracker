namespace SteamPlaytimeTracker.Core;

internal abstract class ViewModel : ObservableObject
{
	internal bool IsConstructed { get; private set; } = false;

	public virtual void OnConstructed()
	{
		IsConstructed = true;
	}
	public virtual void OnLoad(params ReadOnlySpan<object> args) { }
}
