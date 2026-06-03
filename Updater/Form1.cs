using AppServiceUpdater = AppServices.Updater.Updater;
using AppServices.Common.Client;
using AppServices.Updater;

namespace Updater;

public partial class Form1 : Form
{
	private UpdateHelper _updateHelper;
	private ListViewItem? _lastSelectedItem;
	private bool _isUpdating;

	public Form1(string path)
	{
		InitializeComponent();
		var githubClient = new GitHubClient("12Acorns", "SteamPlaytimeTracker");
		_updateHelper = new UpdateHelper(githubClient);
		Name = "Steam Playtime Trakcer Updater";
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
			if(!File.Exists(Path.Combine(path, "SteamPlaytimeTracker.exe")))
			{
				MessageBox.Show("The specified path does not contain the Steam Playtime Tracker executable. Please enter the correct installation folder.");
				return;
			}
			StartUpdateBtn.Enabled = CanUpdate();
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
		var fullPath = Path.Combine(path, "SteamPlaytimeTracker.exe");
		if(Directory.Exists(path) && File.Exists(fullPath))
		{
			InstallPathTxtBox.Text = path;
		}
	}
	public Form1()
	{
		InitializeComponent();
		var githubClient = new GitHubClient("12Acorns", "SteamPlaytimeTracker");
		_updateHelper = new UpdateHelper(githubClient);
		Name = "Steam Playtime Trakcer Updater";
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
			if(!File.Exists(Path.Combine(path, "SteamPlaytimeTracker.exe")))
			{
				MessageBox.Show("The specified path does not contain the Steam Playtime Tracker executable. Please enter the correct installation folder.");
				return;
			}
			StartUpdateBtn.Enabled = CanUpdate();
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
	}

	private void Form1_Load(object sender, EventArgs e)
	{

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
		if(!File.Exists(Path.Combine(selectedPath, "SteamPlaytimeTracker.exe")))
		{
			MessageBox.Show("The selected path does not contain the Steam Playtime Tracker executable. Please select the correct installation folder.");
			InstallPathTxtBox.Text = string.Empty;
			return;
		}
		InstallPathTxtBox.Text = selectedPath;
	}

	private void listView1_SelectedIndexChanged(object sender, EventArgs e)
	{
		_lastSelectedItem = AssetsLstView.FocusedItem;
		StartUpdateBtn.Enabled = CanUpdate();
	}
	private void ReleaseAssetsLbl_Click(object sender, EventArgs e)
	{

	}
	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
		StartUpdateBtn.Enabled = CanUpdate();
	}
	private void StartUpdateBtn_Click(object sender, EventArgs e)
	{
		if(_isUpdating)
		{
			MessageBox.Show("An update is already in progress. Please wait for it to finish before starting a new one.");
			return;
		}
		_isUpdating = true;
		var updater = new AppServiceUpdater(_updateHelper, InstallPathTxtBox.Text, "SteamPlaytimeTracker.exe");
		_ = Task.Run(async () =>
		{
			var asset = _lastSelectedItem?.SubItems[0].Text!;
			var updateTask = updater.TryUpdate(asset, new Version(0, 0, 0), true);
			while(!updateTask.IsCompleted)
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
				progressBar1.Value = progressBar1.Maximum;
				if(!updateTask?.Result.Success ?? false)
				{
					if(updateTask?.Result.Exception is not null)
					{
						MessageBox.Show($"Failed to update the application. Error: {updateTask.Result.Exception}");
					}
					else
					{
						MessageBox.Show("Failed to update the application. Please try again later.");
					}
				}
				else if(!updateTask?.Result.Value ?? false)
				{
					MessageBox.Show("Failed to update the application. Please try again later.");
				}
				else
				{
					MessageBox.Show("Application updated successfully!");
				}
				progressBar1.Value = progressBar1.Minimum;
				_isUpdating = false;
				var result = MessageBox.Show("Do you wish to exit the updater? Click 'Yes' to exit or 'No' to stay.", "Alert!", MessageBoxButtons.YesNo);
				if(result == DialogResult.Yes)
				{
					Close();
				}
				if(updateTask?.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled)
				{
					updateTask?.Dispose();
				}
			});
		}).ContinueWith(x =>
		{
			MessageBox.Show($"Error: {x.Exception}");
			_isUpdating = false;
		}, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(x => x.Dispose());
	}
	private bool CanUpdate() => checkBox1.Checked && !_isUpdating && AssetsLstView.FocusedItem is not null &&
			InstallPathTxtBox.Text.Length > 0;
}
