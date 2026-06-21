using File = System.IO.File;
using IWshRuntimeLibrary;

namespace Common.Utility;

public static class ShortcutUtility
{
	/// <param name="fullFilePath">FullPath to the target file. Includes the extension</param>
	/// <param name="shortcutName">The name of the shortcut. Does not contain the file extension</param>
	/// <returns></returns>
	public static IWshShortcut CreateShortcut(string fullFilePath, string shortcutName)
	{
		if(!File.Exists(fullFilePath))
		{
			throw new FileNotFoundException("No file found to create a shortcut to", nameof(fullFilePath));
		}
		object shellDesktop = "Desktop";
		var shell = new WshShell();
		var shortcutAddress = (string)shell.SpecialFolders.Item(ref shellDesktop) + $"\\{shortcutName}.lnk";
		var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
		shortcut.Description = $"New shortcut for {shortcutName}";
		shortcut.TargetPath = fullFilePath;
		shortcut.Save();
		return shortcut;
	}
}
