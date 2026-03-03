using System.Diagnostics;

namespace SteamPlaytimeTracker.Utility;

public static class ResourceUtility
{
	public static T? GetResource<T>(string resourceKey) where T : class
	{
		ThrowIfCalledStatically(new StackTrace());
		if (App.Current.Resources.Contains(resourceKey))
		{
			return App.Current.Resources[resourceKey] as T;
		}
		return null;
	}
	public static void AddOrUpdateResource<T>(string resourceKey, T resource) where T : class
	{
		ThrowIfCalledStatically(new StackTrace());
		if(App.Current.Resources.Contains(resourceKey))
		{
			App.Current.Resources[resourceKey] = resource;
		}
		else
		{
			App.Current.Resources.Add(resourceKey, resource);
		}
	}

	private static void ThrowIfCalledStatically(StackTrace stackTrace)
	{
		var frame = stackTrace.GetFrame(1);
		var method = frame?.GetMethod();
		if (method is { IsStatic: true})
		{
			throw new InvalidOperationException("This method cannot be called from a static context. " +
				"This is because application resources are instanced, meaning the property value is not yet set. " +
				"Hence, calling from a static context means you would be trying to access a null value (in this case, a null resource lookup)");
		}
	}
}