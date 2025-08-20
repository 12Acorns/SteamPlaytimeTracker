using System.IO;
using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.Utility;

internal static class IOUtility
{
	private const FileOptions DefaultOptions = FileOptions.SequentialScan | FileOptions.Asynchronous;
	private const int DefaultBufferSize = 4096;

	public static async IAsyncEnumerable<string> ReadLinesAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions);
		using var reader = new StreamReader(fileStream);
		string? line;
		while((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
		{
			yield return line!;
		}
	}
	public static IEnumerable<string> ReadLines(string filePath)
	{
		using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan);
		using var reader = new StreamReader(fileStream);
		string? line;
		while((line = reader.ReadLine()) != null)
		{
			yield return line!;
		}
	}
}
