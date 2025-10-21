using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamPlaytimeTracker.IO;

internal static class ApplicationPath
{
	private static readonly Dictionary<string, (string LocalPath, ApplicationPathOption Option)> _pathMap = [];
	private static readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
	private static readonly string _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

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
		if(binding.Option == ApplicationPathOption.CustomGlobal)
		{
			globalPath = binding.RelativePath;
		}
		else
		{
			globalPath = Path.Combine(GetBasePath(binding.Option), binding.RelativePath);
		}
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
		_ => _localAppData
	};
}
