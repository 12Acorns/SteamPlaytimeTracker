using SteamPlaytimeTracker.Core;
using System.Diagnostics.CodeAnalysis;

namespace SteamPlaytimeTracker.Services.Navigation;

internal sealed class NavigagtionEventArgs : EventArgs
{
	[SetsRequiredMembers]
	public NavigagtionEventArgs(ViewModel view, object[]? @params = default)
	{
		ViewModel = view;
		Params = @params ?? [];
	}

	public required ViewModel ViewModel { get; init; }
	public object[] Params { get; init; } = [];
}