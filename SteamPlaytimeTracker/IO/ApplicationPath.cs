using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;

namespace SteamPlaytimeTracker.IO;

internal static class ApplicationPath
{
	private static readonly Dictionary<string, (string LocalPath, ApplicationPathOption Option)> _pathMap = [];
	private static readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
	private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	private static readonly string _exePath = GetExePath();

	public static void AddOrUpdatePath(string lookupName, ApplicationPathOption option, params ReadOnlySpan<string> relativePath) =>
		AddOrUpdatePath(lookupName, Path.Combine(relativePath), option);
	public static void AddOrUpdatePath(string lookupName, params ReadOnlySpan<string> relativePath) =>
		AddOrUpdatePath(lookupName, Path.Combine(relativePath), (ApplicationPathOption?)null);
	public static void AddOrUpdatePath(string lookupName, string relativePath, ApplicationPathOption? option = null)
	{
		ref var path = ref CollectionsMarshal.GetValueRefOrAddDefault(_pathMap, lookupName, out var exists);
		path.LocalPath = relativePath;
		if(option != null)
		{
			path.Option = option.Value;
		}
		else if(!exists)
		{
			path.Option = ApplicationPathOption.LocalAppData;
		}
		LoggingService.Logger.Information("ApplicationPath AddOrUpdatePath succeeded. " +
			"LookupName: {LookupName}, Path: {RelativePath}, PathOption: {PathOption}", lookupName, relativePath, path.Option);
	}

	public static bool TryAddPath(string lookupName, ApplicationPathOption option, params ReadOnlySpan<string> relativePath) =>
		TryAddPath(lookupName, Path.Combine(relativePath), option);
	public static bool TryAddPath(string lookupName, params ReadOnlySpan<string> relativePath) =>
		TryAddPath(lookupName, ApplicationPathOption.LocalAppData, relativePath);
	public static bool TryAddPath(string lookupName, string relativePath, ApplicationPathOption option = ApplicationPathOption.LocalAppData)
	{
		if(string.IsNullOrWhiteSpace(lookupName) || string.IsNullOrWhiteSpace(relativePath))
		{
			LoggingService.Logger.Warning("ApplicationPath TryAddPath failed due to invalid parameters. " +
				"LookupName: {LookupName}, Path: {RelativePath}, PathOption: {PathOption}", lookupName, relativePath, option);
			return false;
		}
		LoggingService.Logger.Information("ApplicationPath TryAddPath succeeded. " +
			"LookupName: {LookupName}, Path: {RelativePath}, PathOption: {PathOption}", lookupName, relativePath, option);
		return _pathMap.TryAdd(lookupName, (relativePath, option));
	}
	public static bool TryGetPath(string lookupName, [NotNullWhen(true)] out string globalPath)
	{
		if(!_pathMap.TryGetValue(lookupName, out (string RelativePath, ApplicationPathOption Option) binding))
		{
			LoggingService.Logger.Warning("ApplicationPath lookup failed for name: {LookupName}", lookupName);
			globalPath = null!;
			return false;
		}
		globalPath = binding.Option switch
		{
			ApplicationPathOption.CustomGlobal => binding.RelativePath,
			_ => Path.Combine(GetBasePath(binding.Option), binding.RelativePath)
		};
		LoggingService.Logger.Information("ApplicationPath lookup succeeded for name: {LookupName}, Path: {GlobalPath}", lookupName, globalPath);
		return true;
	}
	public static string GetPath(string lookupName)
	{
		if(!TryGetPath(lookupName, out var globalPath))
		{
			return string.Empty;
		}
		return globalPath;
	}

	private static string GetBasePath(ApplicationPathOption option) => option switch
	{
		ApplicationPathOption.AppData => _appData,
		ApplicationPathOption.LocalAppData => _localAppData,
		ApplicationPathOption.LocalLowAppData => OperatingSystem.IsWindows() ? _localAppData : Directory.CreateDirectory(Path.Combine(_appData, "LocalLow")).FullName,
		ApplicationPathOption.ExeLocation => _exePath,
		_ => _localAppData
	};
	private static string GetExePath()
	{
		var exePath = Environment.ProcessPath;
		string exeDirectory;
		if(!string.IsNullOrEmpty(exePath))
		{
			exeDirectory = Path.GetDirectoryName(exePath) ?? string.Empty;
			if(!string.IsNullOrEmpty(exeDirectory))
			{
				return exeDirectory;
			}
		}
		exePath = AppDomain.CurrentDomain.BaseDirectory;
		if(File.Exists(Path.Combine(exePath, "SteamPlaytimeTracker.exe")))
		{
			return exePath;
		}
		exePath = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
		if(!string.IsNullOrEmpty(exePath))
		{
			exeDirectory = Path.GetDirectoryName(exePath) ?? string.Empty;
			if(!string.IsNullOrEmpty(exeDirectory))
			{
				return exeDirectory;
			}
		}
		throw new AuthenticationException("Failed to determine executable path.");
	}
}
