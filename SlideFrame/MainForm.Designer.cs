namespace SlideFrame
{
    partial class MainForm
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
            if (disposing && (components != null))
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			contextMenuStrip = new ContextMenuStrip(components);
			playToolStripMenuItem = new ToolStripMenuItem();
			pauseToolStripMenuItem = new ToolStripMenuItem();
			fullscreenToolStripMenuItem = new ToolStripMenuItem();
			sortToolStripMenuItem = new ToolStripMenuItem();
			timeToolStripMenuItem = new ToolStripMenuItem();
			nameToolStripMenuItem = new ToolStripMenuItem();
			shuffleToolStripMenuItem = new ToolStripMenuItem();
			intervalToolStripMenuItem = new ToolStripMenuItem();
			folderToolStripMenuItem = new ToolStripMenuItem();
			exitToolStripMenuItem = new ToolStripMenuItem();
			MediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
			contextMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)MediaPlayer).BeginInit();
			SuspendLayout();
			// 
			// contextMenuStrip
			// 
			contextMenuStrip.ImageScalingSize = new Size(28, 28);
			contextMenuStrip.Items.AddRange(new ToolStripItem[] { playToolStripMenuItem, pauseToolStripMenuItem, fullscreenToolStripMenuItem, sortToolStripMenuItem, intervalToolStripMenuItem, folderToolStripMenuItem, exitToolStripMenuItem });
			contextMenuStrip.Name = "contextMenuStrip1";
			contextMenuStrip.Size = new Size(283, 294);
			// 
			// playToolStripMenuItem
			// 
			playToolStripMenuItem.Image = Properties.Resources.StatusAnnotations_Play_32xLG_color;
			playToolStripMenuItem.Name = "playToolStripMenuItem";
			playToolStripMenuItem.Size = new Size(282, 36);
			playToolStripMenuItem.Text = "Play";
			playToolStripMenuItem.Click += playToolStripMenuItem_Click;
			// 
			// pauseToolStripMenuItem
			// 
			pauseToolStripMenuItem.Image = Properties.Resources.StatusAnnotations_Pause_32xLG_color;
			pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
			pauseToolStripMenuItem.Size = new Size(282, 36);
			pauseToolStripMenuItem.Text = "Pause";
			pauseToolStripMenuItem.Click += pauseToolStripMenuItem_Click;
			// 
			// fullscreenToolStripMenuItem
			// 
			fullscreenToolStripMenuItem.Name = "fullscreenToolStripMenuItem";
			fullscreenToolStripMenuItem.Size = new Size(282, 36);
			fullscreenToolStripMenuItem.Text = "Full Screen";
			fullscreenToolStripMenuItem.Click += fullscreenToolStripMenuItem_Click;
			// 
			// sortToolStripMenuItem
			// 
			sortToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { timeToolStripMenuItem, nameToolStripMenuItem, shuffleToolStripMenuItem });
			sortToolStripMenuItem.Name = "sortToolStripMenuItem";
			sortToolStripMenuItem.Size = new Size(282, 36);
			sortToolStripMenuItem.Text = "Sort";
			// 
			// timeToolStripMenuItem
			// 
			timeToolStripMenuItem.Checked = true;
			timeToolStripMenuItem.CheckState = CheckState.Checked;
			timeToolStripMenuItem.Name = "timeToolStripMenuItem";
			timeToolStripMenuItem.Size = new Size(196, 40);
			timeToolStripMenuItem.Text = "Time";
			timeToolStripMenuItem.Click += sortToolStripMenuItem_Click;
			// 
			// nameToolStripMenuItem
			// 
			nameToolStripMenuItem.Name = "nameToolStripMenuItem";
			nameToolStripMenuItem.Size = new Size(196, 40);
			nameToolStripMenuItem.Text = "Name";
			nameToolStripMenuItem.Click += sortToolStripMenuItem_Click;
			// 
			// shuffleToolStripMenuItem
			// 
			shuffleToolStripMenuItem.Name = "shuffleToolStripMenuItem";
			shuffleToolStripMenuItem.Size = new Size(196, 40);
			shuffleToolStripMenuItem.Text = "Shuffle";
			shuffleToolStripMenuItem.Click += sortToolStripMenuItem_Click;
			// 
			// intervalToolStripMenuItem
			// 
			intervalToolStripMenuItem.Name = "intervalToolStripMenuItem";
			intervalToolStripMenuItem.Size = new Size(282, 36);
			intervalToolStripMenuItem.Text = "Set Interval...";
			intervalToolStripMenuItem.Click += intervalToolStripMenuItem_Click;
			// 
			// folderToolStripMenuItem
			// 
			folderToolStripMenuItem.Name = "folderToolStripMenuItem";
			folderToolStripMenuItem.Size = new Size(282, 36);
			folderToolStripMenuItem.Text = "Set Folder...";
			folderToolStripMenuItem.Click += folderToolStripMenuItem_Click;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new Size(282, 36);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
			// 
			// MediaPlayer
			// 
			MediaPlayer.ContextMenuStrip = contextMenuStrip;
			MediaPlayer.Enabled = true;
			MediaPlayer.Location = new Point(444, 221);
			MediaPlayer.Name = "MediaPlayer";
			MediaPlayer.OcxState = (AxHost.State)resources.GetObject("MediaPlayer.OcxState");
			MediaPlayer.Size = new Size(472, 306);
			MediaPlayer.TabIndex = 1;
			MediaPlayer.MediaChange += MediaPlayer_MediaChange;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(12F, 30F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.Black;
			BackgroundImageLayout = ImageLayout.Zoom;
			ClientSize = new Size(1377, 849);
			ContextMenuStrip = contextMenuStrip;
			Controls.Add(MediaPlayer);
			Name = "MainForm";
			Text = "Slide Frame";
			FormClosing += MainForm_FormClosing;
			Shown += MainForm_Shown;
			SizeChanged += MainForm_SizeChanged;
			KeyUp += MainForm_KeyUp;
			MouseClick += MainForm_MouseClick;
			contextMenuStrip.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)MediaPlayer).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private ContextMenuStrip contextMenuStrip;
		private ToolStripMenuItem playToolStripMenuItem;
		private ToolStripMenuItem sortToolStripMenuItem;
		private ToolStripMenuItem timeToolStripMenuItem;
		private ToolStripMenuItem nameToolStripMenuItem;
		private ToolStripMenuItem shuffleToolStripMenuItem;
		private AxWMPLib.AxWindowsMediaPlayer MediaPlayer;
		private ToolStripMenuItem pauseToolStripMenuItem;
		private ToolStripMenuItem fullscreenToolStripMenuItem;
		private ToolStripMenuItem exitToolStripMenuItem;
		private ToolStripMenuItem intervalToolStripMenuItem;
		private ToolStripMenuItem folderToolStripMenuItem;
	}
}
