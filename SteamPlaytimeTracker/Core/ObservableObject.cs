using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace SteamPlaytimeTracker.Core;

internal abstract class ObservableObject : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}