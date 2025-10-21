using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Services.Lifetime;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SteamPlaytimeTracker.Utility;

internal static class IOUtility
{
	private const FileOptions DefaultOptions = FileOptions.SequentialScan | FileOptions.Asynchronous;
	private const int DefaultBufferSize = 8192;

	/// <summary>
	/// Streams lines from a file asyncronously, exceptions are not catched.
	/// </summary>
	public static async IAsyncEnumerable<string> ReadLinesAsync(string filePath, int bufferSize = DefaultBufferSize, Encoding? encoding = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		encoding ??= Encoding.UTF8;
		if(!cancellationToken.CanBeCanceled)
		{
			cancellationToken = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}

		await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, DefaultOptions);
		using var reader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: true);
		string? line;
		while((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
		{
			yield return line!;
			if(cancellationToken.IsCancellationRequested)
			{
				yield break;
			}
		}
	}
	/// <summary>
	/// Streams lines from a file, exceptions are not catched.
	/// </summary>
	public static IEnumerable<string> ReadLines(string filePath, int bufferSize = DefaultBufferSize, Encoding? encoding = null)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		encoding ??= Encoding.UTF8;

		using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan);
		using var reader = new StreamReader(fileStream, encoding, detectEncodingFromByteOrderMarks: true);
		string? line;
		while((line = reader.ReadLine()) != null)
		{
			yield return line!;
		}
	}
	public static async Task CopyAsync(string fromPath, string toPath, int bufferSize = DefaultBufferSize * 10, CancellationToken cancellationToken = default)
	{
		if(!cancellationToken.CanBeCanceled)
		{
			cancellationToken = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}

		try
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
			ArgumentException.ThrowIfNullOrWhiteSpace(fromPath);
			ArgumentException.ThrowIfNullOrWhiteSpace(toPath);

			if(!File.Exists(fromPath))
			{
				throw new FileNotFoundException("Source file does not exist", fromPath);
			}
			await using var sourceStream = new FileStream(fromPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, DefaultOptions);
			await using var destinationStream = new FileStream(toPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, DefaultOptions);
			await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from {0} to {1}", fromPath, toPath);
			throw;
		}
	}
	public static async ValueTask<T?> HandleTmpFileLifetimeAsync<T>(string originalFilePath, Func<string, ValueTask<T>> asyncFunc, 
		int bufferSize = DefaultBufferSize * 10, CancellationToken cancellationToken = default)
	{
		var tmpFileName = $"{Guid.NewGuid()}_{Path.GetFileName(originalFilePath)}";
		var tmpFilePath = Path.Combine(ApplicationPath.GetPath(GlobalData.TmpFolderName), tmpFileName);
		try
		{
			await CopyAsync(originalFilePath, tmpFilePath, bufferSize, cancellationToken).ConfigureAwait(false);
			LoggingService.Logger.Information("Copied file to temporary location: {0}", tmpFilePath);
			return await asyncFunc(tmpFilePath).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file");
			return default;
		}
		finally
		{
			try
			{
				if(File.Exists(tmpFilePath))
				{
					File.Delete(tmpFilePath);
				}
				LoggingService.Logger.Information("Deleted tmp file from: {0}", tmpFilePath);
			}
			catch(Exception e)
			{
				LoggingService.Logger.Error(e, "Failed to delete tmp file from: {0}", tmpFilePath);
			}
		}
	}
	public static T? HandleTmpFileLifetime<T>(string originalFilePath, Func<string, T> func)
	{
		var tmpFileName = $"{Guid.NewGuid()}_{Path.GetFileName(originalFilePath)}";
		var tmpFilePath = Path.Combine(ApplicationPath.GetPath(GlobalData.TmpFolderName), tmpFileName);
		try
		{
			File.Copy(originalFilePath, tmpFilePath);
			LoggingService.Logger.Information("Copied file to temporary location: {0}", tmpFilePath);
			return func(tmpFilePath);
		}
		catch(Exception ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file or execute function provided");
			return default;
		}
		finally
		{
			try
			{
				if(File.Exists(tmpFilePath))
				{
					File.Delete(tmpFilePath);
					LoggingService.Logger.Information("Deleted tmp file from: {0}", tmpFilePath);
				}
			}
			catch(Exception e)
			{
				LoggingService.Logger.Error(e, "Failed to delete tmp file from: {0}", tmpFilePath);
			}
		}
	}
}
