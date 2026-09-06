using SteamPlaytimeTracker.Services.Lifetime;
using System.Runtime.CompilerServices;
using SteamPlaytimeTracker.IO;
using System.Text;
using System.IO;
using None = OneOf.Monads.None;
using OneOf.Monads;

namespace SteamPlaytimeTracker.Utility;

internal static class IOUtility
{
	private const FileOptions _DefaultOptions = FileOptions.SequentialScan | FileOptions.Asynchronous;
	private const int _DefaultBufferSize = 8192;
	/// <summary>
	/// Streams lines from a file asyncronously, exceptions are not catched.
	/// </summary>
	public static async IAsyncEnumerable<string> ReadLinesAsync(string filePath, int bufferSize = _DefaultBufferSize, Encoding? encoding = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		encoding ??= Encoding.UTF8;
		if(!cancellationToken.CanBeCanceled)
		{
			cancellationToken = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}

		await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, _DefaultOptions);
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
	public static IEnumerable<string> ReadLines(string filePath, int bufferSize = _DefaultBufferSize, Encoding? encoding = null)
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
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <exception cref="PathTooLongException"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	/// <exception cref="IOException"></exception>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="OperationCanceledException"></exception>
	public static async Task<(CopyResult CopyResult, Option<IOFailure> Failure)> CopyAsync(string fromPath, string toPath, int bufferSize = _DefaultBufferSize * 10, CancellationToken cancellationToken = default)
	{
		try
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);
			ArgumentException.ThrowIfNullOrWhiteSpace(fromPath);
			ArgumentException.ThrowIfNullOrWhiteSpace(toPath);
			if(!cancellationToken.CanBeCanceled)
			{
				cancellationToken = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
			}
			if(!File.Exists(fromPath))
			{
				return (CopyResult.FileNotFound, new None());
			}
			await using var sourceStream = new FileStream(fromPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, _DefaultOptions);
			await using var destinationStream = new FileStream(toPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, _DefaultOptions);
			await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
			return (CopyResult.Success, new None());
		}
		catch(Exception ex) when(ex is IOException or UnauthorizedAccessException)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from {0} to {1} due to insufficient file privilege.\n" +
				"This could be due to Steam being open or application needs admin privileges. The former can be resolved " +
				"by shutting down Steam.", fromPath, toPath);
			return (CopyResult.IOError, new IOFailure
			{
				FailureType = IOFailure.IOFailureType.Copy,
				FailureException = ex
			});
		}
		catch(ArgumentException ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from {0} to {1} because of bad arguments", fromPath, toPath);
			return (CopyResult.BadArg, new IOFailure
			{
				FailureType = IOFailure.IOFailureType.BadArg,
				FailureException = ex
			});
		}
		catch(OperationCanceledException ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from {0} to {1} because the operation was cancelled", fromPath, toPath);
			return (CopyResult.OperationCancelled, new IOFailure
			{
				FailureType = IOFailure.IOFailureType.OperationCancelled,
				FailureException = ex
			});
		}
		catch(Exception ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from {0} to {1}", fromPath, toPath);
			return (CopyResult.OtherFailure, new IOFailure
			{
				FailureType = IOFailure.IOFailureType.Other,
				FailureException = ex
			});
		}
	}
	public static async Task<Result<IOFailure, IAsyncEnumerable<T>>> HandleTmpFileLifetimeAsyncEnumerable<T>(string originalFilePath, Func<string, IAsyncEnumerable<T>> asyncFunc,
		int bufferSize = _DefaultBufferSize, CancellationToken cancellationToken = default)
	{
		var tmpFileName = $"{Guid.NewGuid()}_{Path.GetFileName(originalFilePath)}";
		var tmpFilePath = Path.Combine(ApplicationPath.GetPath(GlobalData.TmpFolderName), tmpFileName);
		try
		{
			var result = await CopyAsync(originalFilePath, tmpFilePath, bufferSize, cancellationToken).ConfigureAwait(false);
			if(result.CopyResult is CopyResult.IOError or CopyResult.BadArg or CopyResult.OperationCancelled or CopyResult.OtherFailure)
			{
				return result.Failure.IsSome() ? result.Failure.Value() : new IOFailure
				{
					FailureType = IOFailure.IOFailureType.Other,
					FailureException = null
				};
			}
			LoggingService.Logger.Information("Copied file to temporary location: {0}", tmpFilePath);
			return Result<IOFailure, IAsyncEnumerable<T>>.Success(asyncFunc(tmpFilePath));
		}
		catch(Exception ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from: {0}", originalFilePath);
			return new IOFailure
			{
				FailureType = IOFailure.IOFailureType.Other,
				FailureException = ex
			};
		}
	}
	public static async ValueTask<T?> HandleTmpFileLifetimeAsync<T>(string originalFilePath, Func<string, ValueTask<T>> asyncFunc, 
		int bufferSize = _DefaultBufferSize * 10, CancellationToken cancellationToken = default)
	{
		var tmpFileName = $"{Guid.NewGuid()}_{Path.GetFileName(originalFilePath)}";
		var tmpFilePath = Path.Combine(ApplicationPath.GetPath(GlobalData.TmpFolderName), tmpFileName);
		try
		{
			var result = await CopyAsync(originalFilePath, tmpFilePath, bufferSize, cancellationToken).ConfigureAwait(false);
			if(result.CopyResult is CopyResult.IOError or CopyResult.BadArg or CopyResult.OperationCancelled or CopyResult.OtherFailure)
			{
				return default;
			}
			LoggingService.Logger.Information("Copied file to temporary location: {0}", tmpFilePath);
			return await asyncFunc(tmpFilePath).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			LoggingService.Logger.Error(ex, "Failed to copy file from: {0}", originalFilePath);
			return default;
		}
		finally
		{
			TryDeleteFile(tmpFilePath);
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
			TryDeleteFile(tmpFilePath);
		}
	}
	public static void TryDeleteFile(string filePath)
	{
		try
		{
			if(File.Exists(filePath))
			{
				File.Delete(filePath);
				LoggingService.Logger.Information("Deleted tmp file from: {0}", filePath);
			}
		}
		catch(Exception e)
		{
			LoggingService.Logger.Error(e, "Failed to delete tmp file from: {0}", filePath);
		}
	}
	internal readonly record struct IOFailure
	{
		public IOFailureType FailureType { get; init; }
		public Exception? FailureException { get; init; }

		public enum IOFailureType
		{
			None,
			Copy,
			BadArg,
			OperationCancelled,
			Other
		}
	}
	public enum CopyResult
	{
		Success,
		FileNotFound,
		Unauthorized,
		IOError,
		BadArg,
		OperationCancelled,
		OtherFailure
	}
}
