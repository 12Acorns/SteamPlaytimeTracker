using System.Runtime.InteropServices;
using System.Text;

namespace SteamPlaytimeTracker.Localization.Data;

internal sealed record LocalizedText
{
	public static LocalizedText Empty => new()
	{
		Text = string.Empty,
		Key = string.Empty,
		CodeMapIndex = ushort.MaxValue,
		DistinctTextReplacementCount = 0,
		FormatType = FormatType.None
	};

	private LocalizedText() { }

	public string Text { get; private init; }
	public string Key { get; private init; }
	public ushort CodeMapIndex { get; private init; }
	public int DistinctTextReplacementCount { get; init; }
	public FormatType FormatType { get; private init; }
	public bool IsEmpty => CodeMapIndex == ushort.MaxValue;
	public bool IsPlaceholder => Text.StartsWith('[') && Text.EndsWith(']') && Text.Length > 2 && Text.AsSpan()[1..^1].SequenceEqual(Key);

	/// <summary>
	/// Converts the current text to a format string where each unique placeholder is replaced with a numbered format item.
	/// </summary>
	/// <remarks>This method replaces each unique placeholder in the text with a numbered format item (e.g., {0},
	/// {1}, etc.), allowing for consistent formatting and localization. The mapping between original placeholders and
	/// their assigned numbers is preserved in the returned LocalizedText object.</remarks>
	/// <returns>A new LocalizedText instance containing the numberized format string and updated mapping information.</returns>
	public LocalizedText ToNumberizedTextFormat()
	{
		var formatMap = new Dictionary<string, int>().GetAlternateLookup<ReadOnlySpan<char>>();
		var workingText = Text.AsSpan();
		var builder = new StringBuilder(Text.Length);
		int nextFormatStartIndex;
		var endOfFormattedSectionIndex = 0;
		var biggestFormatIndex = -1;
		while((nextFormatStartIndex = workingText.IndexOf('{')) > -1)
		{
			var segmentBeforeFormat = workingText[endOfFormattedSectionIndex..nextFormatStartIndex];
			builder.Append(segmentBeforeFormat);
			var endOfFormatIndex = workingText.IndexOf('}');
			var textInsideFormat = workingText.Slice(nextFormatStartIndex + 1, endOfFormatIndex - nextFormatStartIndex - 1);
			ref var formatIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(formatMap, textInsideFormat, out var exists);
			if(!exists)
			{
				formatIndex = biggestFormatIndex + 1;
				biggestFormatIndex = formatIndex;
			}
			builder.AppendFormat("{{{0}}}", formatIndex);
			workingText = workingText[(endOfFormatIndex + 1)..];
		}
		if(workingText.Length > 0)
		{
			builder.Append(workingText);
		}
		return new LocalizedText
		{
			Text = builder.ToString(),
			Key = Key,
			CodeMapIndex = CodeMapIndex,
			DistinctTextReplacementCount = biggestFormatIndex + 1,
			FormatType = FormatType.Numberized
		};
	}
	/// <summary>
	/// Creates a new instance of the LocalizedText class by analyzing the provided text for format placeholders and
	/// determining the appropriate format type.
	/// </summary>
	/// <remarks>If the text contains format placeholders, they must be consistently either all numbered or all
	/// named. Mixing both styles will result in an empty LocalizedText. The method determines the format type and counts
	/// the distinct placeholders for use in formatting operations.</remarks>
	/// <param name="text">The text to be localized. May contain format placeholders using either numbered (e.g., {0}) or named (e.g., {name})
	/// syntax.</param>
	/// <param name="key">The unique key that identifies the localized text entry.</param>
	/// <param name="codeMapIndex">The code map index associated with this localized text. Must be a valid ushort value.</param>
	/// <returns>A LocalizedText instance representing the analyzed text, with format information and metadata set according to the
	/// detected placeholders. Returns LocalizedText.Empty if the text contains a mix of numbered and named placeholders.</returns>
	public static LocalizedText Create(string text, string key, ushort codeMapIndex)
	{
		var workingText = text.AsSpan();
		var formatMap = new Dictionary<string, int>().GetAlternateLookup<ReadOnlySpan<char>>();
		bool? keepAsNumberized = null;
		int indexOfFormatBegin;
		var largestFormatIndex = -1;
		while((indexOfFormatBegin = workingText.IndexOf('{')) > -1)
		{
			var indexOfFormatEnd = workingText.IndexOf('}');
			var textInsideFormat = workingText.Slice(indexOfFormatBegin + 1, indexOfFormatEnd - indexOfFormatBegin - 1);
			if(keepAsNumberized == null)
			{
				keepAsNumberized = IsNumberized(textInsideFormat);
				if(keepAsNumberized.Value)
				{
					var numberizedValue = int.Parse(textInsideFormat);
					largestFormatIndex = Math.Max(largestFormatIndex, numberizedValue);
				}
				else
				{
					ref var formatIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(formatMap, textInsideFormat, out var exists);
					if(!exists)
					{
						formatIndex = largestFormatIndex + 1;
						largestFormatIndex = formatIndex;
					}
				}
				workingText = workingText[(indexOfFormatEnd + 1)..];
				continue;
			}
			if(keepAsNumberized.Value && !IsNumberized(textInsideFormat))
			{
				return Empty;
			}
			else if(!keepAsNumberized.Value)
			{
				ref var formatIndex = ref CollectionsMarshal.GetValueRefOrAddDefault(formatMap, textInsideFormat, out var exists);
				if(!exists)
				{
					formatIndex = largestFormatIndex + 1;
					largestFormatIndex = formatIndex;
				}
			}
			else
			{
				var numberizedValue = int.Parse(textInsideFormat);
				largestFormatIndex = Math.Max(largestFormatIndex, numberizedValue);
			}
			workingText = workingText[(indexOfFormatEnd + 1)..];
		}
		return new LocalizedText
		{
			Text = text,
			Key = key,
			CodeMapIndex = codeMapIndex,
			DistinctTextReplacementCount = largestFormatIndex + 1,
			FormatType = keepAsNumberized switch
			{
				null => FormatType.None,
				true => FormatType.Numberized,
				false => FormatType.Named
			}
		};
	}
	private static bool IsNumberized(ReadOnlySpan<char> formatText)
	{
		if(formatText.Length == 0)
		{
			return false;
		}
		formatText = formatText.Trim("{} ");
		for(int i = 0; i < formatText.Length; i++)
		{
			if(!char.IsDigit(formatText[i]))
			{
				return false;
			}
		}
		return true;
	}
	public override string ToString() => Text;
}
internal enum FormatType
{
	None,
	Numberized,
	Named
}