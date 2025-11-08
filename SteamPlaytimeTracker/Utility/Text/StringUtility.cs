namespace SteamPlaytimeTracker.Utility.Text;

internal static class StringUtility
{
	// Probably bug: If parameter in template which is not present in parameters then the length of the
	// create string will be too short as it appears the replacement will include the format param name,
	// whilst CalculateBufferLength assumes removal of template paramater
	public static string FormatNamed(string template, params (string Name, object Value)[] paramaters)
	{
		var bufferLength = CalculateBufferLength(template, paramaters);
		return string.Create(bufferLength, (template, paramaters), (span, state) =>
		{
			var (templateInner, paramatersInner) = state;
			var spanIndex = 0;
			for(int i = 0; i < templateInner.Length; i++)
			{
				if(templateInner[i] is not '{')
				{
					span[spanIndex++] = templateInner[i];
					continue;
				}
				var matched = false;
				foreach(var (name, value) in paramatersInner)
				{
					if(templateInner.AsSpan(i + 1, name.Length).Equals(name, StringComparison.Ordinal) &&
					   i + 1 + name.Length < templateInner.Length &&
					   templateInner[i + 1 + name.Length] is '}')
					{
						var valueString = value?.ToString() ?? string.Empty;
						valueString.AsSpan().CopyTo(span[spanIndex..]);
						spanIndex += valueString.Length;
						i += name.Length + 1;
						matched = true;
						break;
					}
				}
				if(!matched)
				{
					span[spanIndex++] = templateInner[i];
				}
			}
		});
	}
	private static int CalculateBufferLength(string template, ReadOnlySpan<(string Name, object Value)> paramaters)
	{
		var initialLength = template.Length;
		foreach(var (name, value) in paramaters)
		{
			var placeholderCount = CountOccurrences(template, name);
			if(placeholderCount > 0)
			{
				// Remove named paramater, and add value
				initialLength += placeholderCount * (-(name.Length + 2) + (value?.ToString()?.Length ?? 0));
			}
		}
		return initialLength;
	}
	private static int CountOccurrences(string template, string name)
	{
		var count = 0;
		for(int i = 0; i < template.Length - name.Length - 1; i++)
		{
			if(template[i] is not '{')
			{
				continue;
			}
			if(template.AsSpan(i + 1, name.Length).Equals(name, StringComparison.Ordinal) &&
			   i + 1 + name.Length < template.Length &&
			   template[i + 1 + name.Length] is '}')
			{
				count++;
				i += name.Length + 1;
			}
		}
		return count;
	}
}
