using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;

/*
UI:
	context menu:
		Play
		Pause
		First
		toggle Full Screen
		change Sort
			Time Ascending
			Time Descending
			Name
			Shuffle
		change Interval
		change image Folder
		Exit
	mouse:
		click left half of window for previous slide
		click right half of window for next slide
	keyboard:
		Media Previous Track for previous slide
		Media Next Track for next slide
		Media Play Pause to toggle Playing
		Home to go to first slide
		F11 to toggle Fullscreen
 */

/*
CONSIDER:
	log for error messages and file ops?
	caption for date? description?
 */

namespace SlideFrame
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			MediaPlayer.Visible = false;
			DoubleBuffered = true;
			FolderTimer.Tick += FolderTimer_Tick;
			SlidesTimer.Tick += SlidesTimer_Tick;
		}

		// milliseconds - interval between slide changes
		private static int SlidesInterval = 5000;

		// timer for images folder updates
		private readonly System.Windows.Forms.Timer FolderTimer = new() { Interval = 60000, Enabled = true };

		// timer for slide changes
		private readonly System.Windows.Forms.Timer SlidesTimer = new() { Interval = SlidesInterval, Enabled = false };

		// path to images folder
		private string SlidesFolder = ""; // @"C:\Users\Scott\Slides"

		// slides loaded from folder and sorted
		private readonly List<Slide> Slides = [];

		// the current slide being displayed
		private Slide? CurrSlide = null;

		// random number generate for slide shuffle
		private readonly Random Random = new();

		/// <summary>
		/// Control slide advantment via timer
		/// </summary>
		private bool Playing
		{
			get => SlidesTimer.Enabled;
			set
			{
				if (SlidesTimer.Enabled == value)
					return;
				SlidesTimer.Enabled = value;
				if (value)
				{
					if (CurrSlide is not null && !CurrSlide.IsVideo)
						SlidesTimer.Interval = SlidesInterval;
				}
			}
		}

		/// <summary>
		/// Fit the MediaPlayer to fill the form
		/// </summary>
		private void SizeMediaPlayer()
		{
			// using Dock property on the media player didn't seem to work!?
			MediaPlayer.Bounds = ClientRectangle;
			//Debug.WriteLine($"Ctl: {MediaPlayer.Bounds}");
		}

		/// <summary>
		/// Show the MediaPlayer for video playing
		/// </summary>
		private void ShowMediaPlayer()
		{
			if (MediaPlayer.Visible)
				return;
			MediaPlayer.uiMode = "none";            // no play control toolbar
			SizeMediaPlayer();
			MediaPlayer.settings.volume = 0;        // no sound
			MediaPlayer.Visible = true;             // make it visible
			MediaPlayer.enableContextMenu = false;  // disable the player's own video control menu
			MediaPlayer.Enabled = false;            // disable so all interactions pass through to the form
		}

		/// <summary>
		/// Hide the MediaPlayer
		/// </summary>
		private void HideMediaPlayer()
		{
			if (!MediaPlayer.Visible)
				return;
			MediaPlayer.Ctlcontrols.stop();         // stop any playing video
			MediaPlayer.Visible = false;            // hide the control so images show on the background
			MediaPlayer.enableContextMenu = false;  // disable the player's own video control menu
			MediaPlayer.Enabled = false;            // disable so all interactions pass through to the form
		}

		private bool _Fullscreen = false;
		/// <summary>
		/// Control Full Screen display, versus windowed display
		/// </summary>
		public bool Fullscreen
		{
			get => _Fullscreen;
			set
			{
				// the "Minimized" state has been disallowed by removing the button from the header,
				// just for simplicity!
				_Fullscreen = value;
				fullscreenToolStripMenuItem.Checked = _Fullscreen;
				WindowState = _Fullscreen ? FormWindowState.Maximized : FormWindowState.Normal;
				if (_Fullscreen)
					FormBorderStyle = FormBorderStyle.None;
				else
					FormBorderStyle = FormBorderStyle.Sizable;
			}
		}

		private enum SortKind { TimeAscending, TimeDescending, Name, Shuffle };

		private SortKind _Sort = SortKind.TimeAscending;
		/// <summary>
		/// Ways to sort the images for display
		/// </summary>
		private SortKind Sort
		{
			get => _Sort;
			set
			{
				if (value == _Sort)
					return;
				_Sort = value;
				var item = timeToolStripMenuItem.Owner?.Items.OfType<ToolStripMenuItem>().First(i => i.Text?.Replace(" ", "") == value.ToString());
				// Clear the checked state for all items
				if (item is not null)
				{
					if (item.Owner is not null)
					{
						foreach (var sib in item.Owner.Items.OfType<ToolStripMenuItem>())
							sib.Checked = false;
					}
					item.Checked = true;
				}
			}
		}

		/// <summary>
		/// Represent a slide image or video
		/// </summary>
		private class Slide
		{
			public string FullPath;         // path to the media
			public DateTime Time;       // timestamp for sorting
			public int Rand;            // random number for shuffle sort
			public bool Exists;         // flag for folder update tracking of deleted items
			public bool IsVideo;        // the slide is a video
			public bool Invalid;        // not an image or video (removed from the list)
			public int Orientation;     // EXIF data from image for proper rotation during display

			public string Name => Path.GetFileName(FullPath);

			public Slide(string path)
			{
				FullPath = path;
				// easy default for video and images without EXIF DateTimeOriginal
				Time = File.GetLastWriteTime(FullPath);
				Exists = true;

				// check file type
				switch (System.IO.Path.GetExtension(FullPath).ToLower())
				{
					case ".jpg":
					case ".jpeg":
					case ".png":
					case ".bmp":
					case ".tif":
					case ".tiff":
						break;
					case ".mp4":
						IsVideo = true;
						break;
					default:
						Invalid = true;
						return;
				}

				if (!IsVideo)
				{
					// for images, check EXIF properties for Orientation and DateTimeOriginal
					try
					{
						using Image image = Bitmap.FromFile(FullPath);
						// https://exiv2.org/tags.html
						PropertyItem[] propItems = image.PropertyItems;
						// Exif.Image.Orientation
						var prop = propItems.FirstOrDefault(i => i.Id == 0x0112);
						if (prop is not null && prop.Value is not null)
						{
							Orientation = prop.Value[0];
						}
						// Exif.Image.DateTimeOriginal
						prop = propItems.FirstOrDefault(i => i.Id == 0x9003);
						if (prop is not null && prop.Value is not null)
						{
							// "2024:12:24 18:41:55"
							var bytes = prop.Value;
							bytes[4] = (byte)'-';
							bytes[7] = (byte)'-';
							var s = Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1);
							if (!DateTime.TryParse(s, out Time))
								Time = File.GetLastWriteTime(FullPath);
						}
					}
					catch (Exception exc)
					{
						Debug.WriteLine($"Error fetching EXIF properties: {exc.Message}");
					}
				}
			}
		}

		/// <summary>
		/// Set a new images folder path and update slides
		/// </summary>
		/// <param name="path">New path to images folder</param>
		private void SetFolder(string path)
		{
			SlidesFolder = path;
			UpdateSlides();
		}

		/// <summary>
		/// Update the slides list from the folder via timer
		/// </summary>
		private void FolderTimer_Tick(object? sender, EventArgs e)
		{
			UpdateSlides();
		}

		/// <summary>
		/// Advance to the next slide via timer
		/// </summary>
		private void SlidesTimer_Tick(object? sender, EventArgs e)
		{
			NextSlide();
		}

		/// <summary>
		/// Advance display to the next slide
		/// </summary>
		public void NextSlide()
		{
			if (CurrSlide is null)
			{
				// start at the first, if there is one
				CurrSlide = Slides.FirstOrDefault();
			}
			else if (CurrSlide == Slides.LastOrDefault())
			{
				// wrap from last to first
				CurrSlide = Slides.FirstOrDefault();
			}
			else
			{
				// advance to the next
				int i = Slides.IndexOf(CurrSlide);
				CurrSlide = Slides[i + 1];
			}
			ShowSlide();
		}

		/// <summary>
		/// Change display to the previous slide
		/// </summary>
		public void PrevSlide()
		{
			if (CurrSlide is null)
			{
				// start at the first, if there is one
				CurrSlide = Slides.LastOrDefault();
			}
			else if (CurrSlide == Slides.FirstOrDefault())
			{
				// wrap from first to last
				CurrSlide = Slides.LastOrDefault();
			}
			else
			{
				// move to the previous slide
				int i = Slides.IndexOf(CurrSlide);
				CurrSlide = Slides[i - 1];
			}
			ShowSlide();
		}

		/// <summary>
		/// Display the image or video for the current slide
		/// </summary>
		private void ShowSlide()
		{
			// remove any current image or vodeo
			HideMediaPlayer();
			if (BackgroundImage is not null)
			{
				BackgroundImage.Dispose();
				BackgroundImage = null;
			}
			if (CurrSlide is null)
				return;
			if (!File.Exists(CurrSlide.FullPath))
			{
				Debug.WriteLine($"Media no longer exists: {CurrSlide.FullPath}");
				Slides.Remove(CurrSlide);
				CurrSlide = null;
				return;
			}
			try
			{
				if (!CurrSlide.IsVideo)
				{
					Image image = Bitmap.FromFile(CurrSlide.FullPath);
					switch (CurrSlide.Orientation)
					{
						case 1:     // 888888
									// 88    
									// 8888  
									// 88    
									// 88    
							break;
						case 2:     // 888888
									//     88
									//   8888
									//     88
									//     88
							image.RotateFlip(RotateFlipType.RotateNoneFlipX);
							break;
						case 3:     //     88
									//     88
									//   8888
									//     88
									// 888888
							image.RotateFlip(RotateFlipType.Rotate180FlipNone);
							break;
						case 4:     // 88     
									// 88     
									// 8888   
									// 88
									// 888888
							image.RotateFlip(RotateFlipType.RotateNoneFlipY);
							break;
						case 5:     // 8888888888
									// 88  88    
									// 88        
							image.RotateFlip(RotateFlipType.Rotate270FlipY);
							break;
						case 6:     // 88
									// 88  88
									// 8888888888
							image.RotateFlip(RotateFlipType.Rotate90FlipNone);
							break;
						case 7:     //         88
									//     88  88
									// 8888888888
							image.RotateFlip(RotateFlipType.Rotate90FlipY);
							break;
						case 8:     // 8888888888
									//     88  88
									//         88
							image.RotateFlip(RotateFlipType.Rotate270FlipNone);
							break;
					}
					BackgroundImage = image;
				}
				else // video
				{
					// set the video file in the MediaPlayer
					MediaPlayer.URL = CurrSlide.FullPath;
					// show the player
					ShowMediaPlayer();
					// start it playing
					MediaPlayer.Ctlcontrols.play();
				}
				// we need to stop the timer if it was running to reset the interval properly
				// but want to restart it only if we were actively playing
				var wasPlaying = Playing;
				Playing = false;
				// set timer for photos; update for video when we can access duration later
				SlidesTimer.Interval = SlidesInterval;
				Playing = wasPlaying;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error displaying media: {ex.Message}");
			}
		}

		/// <summary>
		/// Update the Slides list to match the current state of the folder
		/// </summary>
		private void UpdateSlides()
		{
			try
			{
				if (string.IsNullOrEmpty(SlidesFolder) || !Path.Exists(SlidesFolder))
				{
					// cease and desist
					Playing = false;
					Slides.Clear();
					CurrSlide = null;
					return;
				}
				var files = Directory.EnumerateFiles(SlidesFolder);
				bool changed = false;
				// clear Exists flag for all slides
				// we'll set it for entries we find again
				// the ones that remain clear are then identified as no longer in the folder
				foreach (var slide in Slides)
				{
					slide.Exists = false;
				}
				foreach (var file in files)
				{
					// look for media already encountered
					var slide = Slides.FirstOrDefault(s => s.FullPath == file);
					if (slide is not null)
					{
						// flag to keep it and move on
						slide.Exists = true;
						continue;
					}
					slide = new(file);
					if (!slide.Invalid)
					{
						// list has changed with added file(s)
						changed = true;
						Slides.Add(slide);
						Debug.WriteLine($"++++ {slide.FullPath} {slide.Time}");
					}
				}
				// remove deleted slides from the list
				var deleted = Slides.Where(s => !s.Exists).ToList();
				foreach (var slide in deleted)
				{
					Slides.Remove(slide);
					if (slide == CurrSlide)
						CurrSlide = null;
					Debug.WriteLine($"---- {slide.FullPath} {slide.Time}");
					// list has changed with deleted file(s)
					changed = true;
				}
				if (changed)
				{
					// need to re-sort after changes to the list
					SortSlides();
					CurrSlide ??= Slides.First();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error updating slides list: {ex.Message}");
			}
		}

		/// <summary>
		/// Sort the slides according to the current user preference
		/// </summary>
		private void SortSlides()
		{
			switch (Sort)
			{
				case SortKind.TimeAscending:
					Slides.Sort((a, b) => DateTime.Compare(a.Time, b.Time));
					break;
				case SortKind.TimeDescending:
					Slides.Sort((a, b) => -DateTime.Compare(a.Time, b.Time));
					break;
				case SortKind.Name:
					Slides.Sort((a, b) => string.Compare(a.Name, b.Name));
					break;
				case SortKind.Shuffle:
					// NOTE: I tried just using a Random.Next() in the Sort comparitor.
					//		That seemed very clever, but the results didn't look good.
					//		I suspect that Next() was being factored out of the loop (called just once)
					//		so that all comparisons were the same!? So...
					// assigning random values to slides for sorting
					foreach (var slide in Slides)
						slide.Rand = Random.Next();
					Slides.Sort((a, b) => Math.Sign(a.Rand - b.Rand));
					break;
				default:
					break;
			}
			Debug.WriteLine("");
			foreach (var slide in Slides)
			{
				Debug.WriteLine($"{slide.FullPath} {slide.Time}");
			}
			Debug.WriteLine("");
		}

		/// <summary>
		/// Context menu Play command
		/// </summary>
		private void playToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Playing = true;
		}

		/// <summary>
		/// Context menu Pause command
		/// </summary>
		private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Playing = false;
		}

		/// <summary>
		/// Set current slide to the first slide
		/// </summary>
		private void firstToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CurrSlide = Slides.FirstOrDefault();
			ShowSlide();
		}

		/// <summary>
		/// Context menu Full Screen toggle command
		/// </summary>
		private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Fullscreen = !Fullscreen;
		}

		/// <summary>
		/// Context menu Sort kind seletion
		/// </summary>
		private void sortToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (sender is not ToolStripMenuItem item || item.Owner is null)
				return;
			// get the SortKind from the menu text
			Sort = (SortKind)Enum.Parse(typeof(SortKind), item.Text?.Replace(" ", "") ?? "TimeAscending");
			SortSlides();
		}

		/// <summary>
		/// Context menu Interval set command
		/// </summary>
		private void intervalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// using VB InputBox dialog to easily input a number for the interval
			string input = Interaction.InputBox("Enter interval in seconds (decimal point allowed)", "Slide Frame", $"{SlidesInterval / 1000.0}",
					contextMenuStrip.Bounds.X, contextMenuStrip.Bounds.Y);
			if (double.TryParse(input, out double interval))
			{
				// convert to milliseconds for timer
				SlidesInterval = Math.Max(1, (int)Math.Ceiling(interval * 1000));
				SlidesTimer.Interval = SlidesInterval;
			}
		}

		/// <summary>
		/// Context menu Folder set command
		/// </summary>
		private void folderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// simple use of standard folder browser
			FolderBrowserDialog dialog = new()
			{
				Description = "Select images folder",
				ShowNewFolderButton = false,
				RootFolder = Environment.SpecialFolder.Personal,
				SelectedPath = SlidesFolder ?? ""
			};
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				SetFolder(dialog.SelectedPath);
				ShowSlide();
			}
		}

		/// <summary>
		/// Context menu Exit program command
		/// </summary>
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Track the maximized state and keep MediaPlayer full on the form
		/// </summary>
		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			Fullscreen = WindowState == FormWindowState.Maximized;
			SizeMediaPlayer();
		}

		/// <summary>
		/// Allow slide advancement with mouse clicks
		/// </summary>
		private void MainForm_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.X >= ClientSize.Width / 2)
					NextSlide();
				else
					PrevSlide();
			}
		}

		/// <summary>
		/// Process media keys and F11 for Full Screen
		/// </summary>
		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.MediaNextTrack:
					NextSlide();
					break;
				case Keys.MediaPreviousTrack:
					PrevSlide();
					break;
				case Keys.MediaPlayPause:
					Playing = !Playing;
					break;
				case Keys.F11:
					Fullscreen = !Fullscreen;
					break;
				case Keys.Home:
					CurrSlide = Slides.FirstOrDefault();
					ShowSlide();
					break;
			}
		}

		/// <summary>
		/// Process MediaChange for the player
		/// </summary>
		private void MediaPlayer_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
		{
			if (MediaPlayer.currentMedia is null)
				return;
			SizeMediaPlayer();      // JIC
									// Set the SlidesTimer based on the video duration
									// we have to wait until the player has loaded the video after setting it in ShowSlide for valid values
									// then we can calculate the video's remaining time and set the timer
									// NOTE: for some reason we seem to get called here twice for each video started
									//		so we update based on the current position
			var dur = MediaPlayer.currentMedia.duration;
			var pos = MediaPlayer.Ctlcontrols.currentPosition;
			var rem = (short)Math.Ceiling((dur - pos) * 1000);
			//Debug.WriteLine($"duration: {dur} position: {pos}");
			SlidesTimer.Interval = (int)Math.Max(rem, SlidesInterval);
		}

		/// <summary>
		/// Process the startup of the window
		/// </summary>
		private void MainForm_Shown(object sender, EventArgs e)
		{
			// load the options saved in the Settings file
			// Interval
			SlidesInterval = Math.Max(1, Settings1.Default.Interval);
			// form Bounds
			if (!string.IsNullOrEmpty(Settings1.Default.Bounds))
			{
				RectangleConverter conv = new();
				var o = conv.ConvertFromString(Settings1.Default.Bounds);
				if (o is not null)
					Bounds = (Rectangle)o;
			}
			// WindowState (Full Screen)
			if (Enum.TryParse(Settings1.Default.State, out FormWindowState state))
			{
				WindowState = state;    // this will be seen in SizeChanged and set FullScreen
			}
			if (Enum.TryParse(Settings1.Default.Sort, out SortKind sort))
			{
				Sort = sort;
			}
			// SlidesFolder
			SetFolder(Settings1.Default.Folder);
			// set it going with saved playing state
			ShowSlide();
			Playing = Settings1.Default.Playing;
		}

		/// <summary>
		/// Process the shutdown of the app
		/// </summary>
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// save user options in the Settings file
			Settings1.Default.Folder = SlidesFolder;
			Settings1.Default.Playing = Playing;
			Settings1.Default.Interval = SlidesInterval;
			Settings1.Default.State = WindowState.ToString();
			Settings1.Default.Sort = Sort.ToString();
			// set the Normal state so we can save the Normal Bounds
			// NOTE: RestoreBounds property should work here but did not!
			WindowState = FormWindowState.Normal;
			RectangleConverter conv = new();
			Settings1.Default.Bounds = conv.ConvertToString(Bounds) ?? "";
			Settings1.Default.Save();
		}
	}
}
