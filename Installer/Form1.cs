using AppServiceUpdater = AppServices.Updater.Updater;
using AppServices.Common.Client;
using AppServices.Updater;
using Common.Utility;
using AppServices.Common.App;
using System.Reflection;

namespace Installer;

public partial class Form1 : Form
{
	private readonly UpdateHelper _updateHelper;
	private ListViewItem? _lastSelectedItem;
	private bool _isInstalling;

	public Form1(string path)
	{
		InitializeComponent();
		var githubClient = new GitHubClient("12Acorns", "SteamPlaytimeTracker");
		_updateHelper = new UpdateHelper(githubClient);
		InstallPathTxtBox.KeyDown += (s, e) =>
		{
			if(e.KeyCode is not (Keys.Enter or Keys.Return))
			{
				return;
			}
			var path = InstallPathTxtBox.Text;
			if(string.IsNullOrEmpty(path))
			{
				MessageBox.Show("Please enter a valid path.");
				return;
			}
			if(!Directory.Exists(path))
			{
				MessageBox.Show("The specified path does not exist. Please enter a valid path.");
				return;
			}
			StartInstallBtn.Enabled = CanInstall();
		};
		ReleaseAssetsLbl.Text = "Fetching latest release information...";
		var latestReleaseTask = _updateHelper.GetLatestReleaseAsync().ContinueWith(x =>
		{
			Invoke(() =>
			{
				if(!x.Result.Success)
				{
					MessageBox.Show($"Failed to fetch the latest release information. " +
						$"Error: {x.Result.Exception ?? new Exception("Unknown error")}");
					return;
				}
				var release = x.Result.Value;
				ReleaseAssetsLbl.Text = $"Assets for Latest Release: {release.Name}";
				var assets = release.Assets;
				if(assets is null or [])
				{
					MessageBox.Show("No assets found for the latest release.");
					return;
				}
				AssetsLstView.Columns.Add("Asset Name");
				AssetsLstView.Columns.Add("Date Updated");
				AssetsLstView.Columns.Add("Downloads");
				AssetsLstView.Columns.Add("Size (MB)");
				AssetsLstView.Items.AddRange(assets.Select(asset => new ListViewItem([
						asset.Name!,
						asset.UpdatedAt!.Value.ToString("dd/MM/yyyy"),
						asset.DownloadCount.ToString(),
						$"{asset.Size/1_000_000f:n2}"
					]
				)).ToArray());
				foreach(ColumnHeader column in AssetsLstView.Columns)
				{
					column.Width = -2;
				}
			});
		});
		path = path.TrimEnd([' ', '\\', '/', '\"']);
		path = path.TrimStart([' ', '\"']);
		if(Directory.Exists(path))
		{
			InstallPathTxtBox.Text = path;
		}
	}
	public Form1() : this("") { }

	private void Form1_Load(object sender, EventArgs e)
	{
		try
		{
			richTextBox1.Rtf = File.ReadAllText("resources/EULA.rtf");
		}
		catch(Exception ex)
		{
			MessageBox.Show($"Failed to retrieve legal notice. Closing installer. Reason: {ex}", "Error!", MessageBoxButtons.OK);
			Close();
		}
	}

	private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
	{

	}

	private void label1_Click(object sender, EventArgs e)
	{

	}

	private void button2_Click(object sender, EventArgs e)
	{
		_ = installPathDialog.ShowDialog();
		var selectedPath = installPathDialog.SelectedPath;
		if(string.IsNullOrEmpty(selectedPath))
		{
			MessageBox.Show("Please select a valid path.");
			InstallPathTxtBox.Text = string.Empty;
			return;
		}
		if(!Directory.Exists(selectedPath))
		{
			MessageBox.Show("The selected path does not exist. Please select a valid path.");
			InstallPathTxtBox.Text = string.Empty;
			return;
		}
		InstallPathTxtBox.Text = selectedPath;
	}

	private void listView1_SelectedIndexChanged(object sender, EventArgs e)
	{
		_lastSelectedItem = AssetsLstView.FocusedItem;
		StartInstallBtn.Enabled = CanInstall();
	}
	private void ReleaseAssetsLbl_Click(object sender, EventArgs e)
	{

	}
	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
		StartInstallBtn.Enabled = CanInstall();
	}
	private void StartUpdateBtn_Click(object sender, EventArgs e)
	{
		if(_isInstalling)
		{
			MessageBox.Show("An install is already in progress. Please wait for it to finish before starting a new one.");
			return;
		}
		_isInstalling = true;
		InstallPathTxtBox.Enabled = false;
		InstallPathTxtBox.ReadOnly = true;
		var version = new SemanticVersion() { Version = new(0, 0, 0), Type = new FullRelease() };
		var updater = new AppServiceUpdater(_updateHelper, InstallPathTxtBox.Text, "SteamPlaytimeTracker.exe");
		_ = Task.Run(async () =>
		{
			var asset = _lastSelectedItem?.SubItems[0].Text!;
			var installTask = updater.TryUpdate(asset, currentVersion: version, true);
			while(!installTask.IsCompleted)
			{
				Invoke(() =>
				{
					if(progressBar1.Value == progressBar1.Maximum)
					{
						progressBar1.Value = progressBar1.Minimum;
					}
					progressBar1.PerformStep();
				});
				await Task.Delay(500).ConfigureAwait(false);
			}
			Invoke(() =>
			{
				InstallPathTxtBox.Enabled = true;
				InstallPathTxtBox.ReadOnly = false;
				progressBar1.Value = progressBar1.Maximum;
				if(!installTask?.Result.Success ?? false)
				{
					if(installTask?.Result.Exception is not null)
					{
						MessageBox.Show($"Failed to install the application. Error: {installTask.Result.Exception}");
					}
					else
					{
						MessageBox.Show("Failed to install the application. Please try again later.");
					}
				}
				else if(!installTask?.Result.Value ?? false)
				{
					MessageBox.Show("Failed to install the application. Please try again later.");
				}
				else
				{
					MessageBox.Show("Application install successfully!");
				}
				progressBar1.Value = progressBar1.Minimum;
				_isInstalling = false;
				var result = MessageBox.Show("Closing installer, do you wish to autolaunch the application and create a desktop shortcut?", "Alert!", MessageBoxButtons.YesNo);
				if(result == DialogResult.Yes)
				{
					var shortCutTarget = Directory.EnumerateFiles(InstallPathTxtBox.Text, "*.exe", new EnumerationOptions() { RecurseSubdirectories = true })
						.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals("SteamPlaytimeTracker", StringComparison.InvariantCultureIgnoreCase));
					if(!string.IsNullOrWhiteSpace(shortCutTarget))
					{
						ShortcutUtility.CreateShortcut(shortCutTarget, "Steam Playtime Tracker");
					}
					else
					{
						MessageBox.Show("Failed to create shortcut.");
					}
				}
				if(installTask?.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled)
				{
					installTask?.Dispose();
				}
				Close();
			});
		}).ContinueWith(x =>
		{
			MessageBox.Show($"Error: {x.Exception}");
			_isInstalling = false;
		}, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(x => x.Dispose());
	}
	private bool CanInstall() => checkBox1.Checked && !_isInstalling && AssetsLstView.FocusedItem is not null && !string.IsNullOrWhiteSpace(InstallPathTxtBox.Text) && 
		Directory.Exists(InstallPathTxtBox.Text);

	private void richTextBox1_TextChanged(object sender, EventArgs e)
	{

	}
}
