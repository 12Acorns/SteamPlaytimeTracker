using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;

namespace SteamPlaytimeTracker.Extensions;

internal static class SelectorExtensions
{
	extension(Selector element)
	{
		public void RemoveFocus()
		{
			FrameworkElement parent = (FrameworkElement)element.Parent;
			while(parent != null && parent is IInputElement && !((IInputElement)parent).Focusable)
			{
				parent = (FrameworkElement)parent.Parent;
			}

			DependencyObject scope = FocusManager.GetFocusScope(element);
			FocusManager.SetFocusedElement(scope, parent as IInputElement);
		}
	}
}
