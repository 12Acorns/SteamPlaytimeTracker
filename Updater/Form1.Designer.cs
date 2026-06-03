namespace Updater;

partial class Form1
{
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if(disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		installPathDialog = new FolderBrowserDialog();
		label1 = new Label();
		InstallPathTxtBox = new TextBox();
		InstallOpenBrowserBtn = new Button();
		AssetsLstView = new ListView();
		ReleaseAssetsLbl = new Label();
		richTextBox1 = new RichTextBox();
		checkBox1 = new CheckBox();
		StartUpdateBtn = new Button();
		progressBar1 = new ProgressBar();
		SuspendLayout();
		// 
		// installPathDialog
		// 
		installPathDialog.ShowNewFolderButton = false;
		installPathDialog.HelpRequest += folderBrowserDialog1_HelpRequest;
		// 
		// label1
		// 
		label1.AutoSize = true;
		label1.Location = new Point(11, 9);
		label1.Margin = new Padding(4, 0, 4, 0);
		label1.Name = "label1";
		label1.Size = new Size(252, 32);
		label1.TabIndex = 0;
		label1.Text = "Enter installation path:\r\n";
		label1.Click += label1_Click;
		// 
		// InstallPathTxtBox
		// 
		InstallPathTxtBox.Location = new Point(269, 6);
		InstallPathTxtBox.Margin = new Padding(4, 2, 4, 2);
		InstallPathTxtBox.Name = "InstallPathTxtBox";
		InstallPathTxtBox.PlaceholderText = "Enter Installation Path";
		InstallPathTxtBox.Size = new Size(903, 39);
		InstallPathTxtBox.TabIndex = 1;
		InstallPathTxtBox.WordWrap = false;
		// 
		// InstallOpenBrowserBtn
		// 
		InstallOpenBrowserBtn.Location = new Point(1187, 6);
		InstallOpenBrowserBtn.Margin = new Padding(4, 2, 4, 2);
		InstallOpenBrowserBtn.Name = "InstallOpenBrowserBtn";
		InstallOpenBrowserBtn.Size = new Size(199, 38);
		InstallOpenBrowserBtn.TabIndex = 3;
		InstallOpenBrowserBtn.Text = "Browse";
		InstallOpenBrowserBtn.UseVisualStyleBackColor = true;
		InstallOpenBrowserBtn.Click += button2_Click;
		// 
		// AssetsLstView
		// 
		AssetsLstView.FullRowSelect = true;
		AssetsLstView.Location = new Point(15, 85);
		AssetsLstView.Margin = new Padding(4, 2, 4, 2);
		AssetsLstView.MultiSelect = false;
		AssetsLstView.Name = "AssetsLstView";
		AssetsLstView.Size = new Size(1373, 200);
		AssetsLstView.TabIndex = 5;
		AssetsLstView.UseCompatibleStateImageBehavior = false;
		AssetsLstView.View = View.Details;
		AssetsLstView.SelectedIndexChanged += listView1_SelectedIndexChanged;
		// 
		// ReleaseAssetsLbl
		// 
		ReleaseAssetsLbl.AutoSize = true;
		ReleaseAssetsLbl.Location = new Point(11, 51);
		ReleaseAssetsLbl.Margin = new Padding(4, 0, 4, 0);
		ReleaseAssetsLbl.Name = "ReleaseAssetsLbl";
		ReleaseAssetsLbl.Size = new Size(151, 32);
		ReleaseAssetsLbl.TabIndex = 6;
		ReleaseAssetsLbl.Text = "Assets for {0}";
		ReleaseAssetsLbl.Click += ReleaseAssetsLbl_Click;
		// 
		// richTextBox1
		// 
		richTextBox1.AcceptsTab = true;
		richTextBox1.BackColor = SystemColors.Window;
		richTextBox1.Location = new Point(15, 292);
		richTextBox1.Margin = new Padding(4, 2, 4, 2);
		richTextBox1.Name = "richTextBox1";
		richTextBox1.ReadOnly = true;
		richTextBox1.Size = new Size(1371, 550);
		richTextBox1.TabIndex = 7;
		richTextBox1.Text = "LEGAL NOTICE";
		// 
		// checkBox1
		// 
		checkBox1.AutoSize = true;
		checkBox1.CheckAlign = ContentAlignment.MiddleRight;
		checkBox1.Location = new Point(596, 879);
		checkBox1.Margin = new Padding(4, 2, 4, 2);
		checkBox1.Name = "checkBox1";
		checkBox1.Size = new Size(569, 36);
		checkBox1.TabIndex = 8;
		checkBox1.Text = "By ticking this checkbox, I accept the legal notice";
		checkBox1.UseVisualStyleBackColor = true;
		checkBox1.CheckedChanged += checkBox1_CheckedChanged;
		// 
		// StartUpdateBtn
		// 
		StartUpdateBtn.Enabled = false;
		StartUpdateBtn.Location = new Point(1185, 875);
		StartUpdateBtn.Margin = new Padding(4, 2, 4, 2);
		StartUpdateBtn.Name = "StartUpdateBtn";
		StartUpdateBtn.Size = new Size(201, 43);
		StartUpdateBtn.TabIndex = 9;
		StartUpdateBtn.Text = "Update";
		StartUpdateBtn.UseVisualStyleBackColor = true;
		StartUpdateBtn.Click += StartUpdateBtn_Click;
		// 
		// progressBar1
		// 
		progressBar1.Location = new Point(15, 875);
		progressBar1.Margin = new Padding(4, 2, 4, 2);
		progressBar1.Name = "progressBar1";
		progressBar1.Size = new Size(419, 43);
		progressBar1.TabIndex = 10;
		// 
		// Form1
		// 
		AutoScaleDimensions = new SizeF(13F, 32F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(1395, 928);
		Controls.Add(progressBar1);
		Controls.Add(StartUpdateBtn);
		Controls.Add(checkBox1);
		Controls.Add(richTextBox1);
		Controls.Add(ReleaseAssetsLbl);
		Controls.Add(AssetsLstView);
		Controls.Add(InstallOpenBrowserBtn);
		Controls.Add(InstallPathTxtBox);
		Controls.Add(label1);
		FormBorderStyle = FormBorderStyle.FixedSingle;
		Margin = new Padding(4, 2, 4, 2);
		Name = "Form1";
		Text = "Steam Playtime Trakcer Updater";
		Load += Form1_Load;
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private FolderBrowserDialog installPathDialog;
	private Label label1;
	private TextBox InstallPathTxtBox;
	private Button InstallOpenBrowserBtn;
	private ListView AssetsLstView;
	private Label ReleaseAssetsLbl;
	private RichTextBox richTextBox1;
	private CheckBox checkBox1;
	private Button StartUpdateBtn;
	private ProgressBar progressBar1;
}
