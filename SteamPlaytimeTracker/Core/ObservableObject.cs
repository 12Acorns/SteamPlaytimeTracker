using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace SteamPlaytimeTracker.Core;

internal abstract class ObservableObject : INotifyPropertyChanged
{
	protected static Dispatcher Dispatcher => App.Current.Dispatcher;

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}