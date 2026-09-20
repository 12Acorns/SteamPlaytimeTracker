using Sigil;
using System.Reflection;
using System.Windows;

namespace SteamPlaytimeTracker.Extensions;

internal static class WindowExtensions
{
	private static readonly FieldInfo _showAsDialogFieldInfo = typeof(Window)
		.GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NullReferenceException();
	private static readonly Func<Window, bool> _modelWindowGetter =
		Emit<Func<Window, bool>>
			.NewDynamicMethod("__get__showingAsDialog__")
			.LoadArgument(0)
			.CastClass<Window>()
			.LoadField(_showAsDialogFieldInfo)
			.Return()
			.CreateDelegate();

	extension(Window w)
	{
		public bool IsModal => _modelWindowGetter(w);
	}
}
