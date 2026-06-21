using System.Diagnostics;

namespace Installer;

internal static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		if(!Environment.IsPrivilegedProcess)
		{
			MessageBox.Show("Need administrative privileged to create files under Program Files.");
			var proc = new ProcessStartInfo
			{
				UseShellExecute = true,
				WorkingDirectory = Environment.CurrentDirectory,
				FileName = Application.ExecutablePath,
				Verb = "runas"
			};
			try
			{
				Process.Start(proc);
			}
			catch
			{
				// The user refused the elevation.
				// Do nothing and return directly ...
				return;
			}
			Application.Exit();
			return;
		}

		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();
		var defaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam Playtime Tracker");
		Directory.CreateDirectory(defaultInstallPath);
		Application.Run(new Form1(defaultInstallPath));
	}
}