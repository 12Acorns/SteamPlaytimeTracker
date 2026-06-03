using System.Diagnostics;
using CommandLine;

namespace Updater;

internal static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();
		var clArgs = Environment.GetCommandLineArgs();
		if(clArgs.Length > 0)
		{
			clArgs = clArgs[1..];
		}
		var parseRes = Parser.Default.ParseArguments<CLOptions>(clArgs);
		var options = parseRes.Value;
		if(!string.IsNullOrEmpty(options?.ExtractionPath))
		{
			Application.Run(new Form1(options.ExtractionPath));
		}
		else
		{
			Application.Run(new Form1());
		}
	}
	private sealed class CLOptions
	{
		[Option(shortName: 'p', longName: "app-path", Required = false, HelpText = "The path which to extract the update to. This path must be the same as where the application is installed on disk.")]
		public string? ExtractionPath { get; set; }
	}
}