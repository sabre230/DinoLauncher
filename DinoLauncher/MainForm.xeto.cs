using Eto.Forms;
using Eto.Serialization.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DinoLauncherLib;
using Eto.Drawing;
using LibGit2Sharp;
using System.Windows;

namespace DinoLauncher;
// note to self: remember to pass object through parameters when you need
// that specifically generated object for some reason like prefs
// Note to Self: Visual element should rely on prefs, not the other way around

public class MainForm : Form
{
    // I think it'll be easier to just pass these along in function parameters
    FileIO fileIO = new DinoLauncherLib.FileIO(); // Use to reference and perform directory/file functions
    Extras extras = new DinoLauncherLib.Extras();// Use for fun things like music or w/e
    UserPrefs prefs = new DinoLauncherLib.UserPrefs();

    // Looks like Eto doesn't auto-generate controls in the xeto form
    // According to internet wizards, I have to add them to my class as data members
    // or properties with the same name
    // Weird

    #region Form Controls

    public StackLayout      StackLayout;
    public ImageView        Image_DinosaurPlanetLogo;
    public CheckBox         CheckBox_UseHQModels;
    public DropDown         DropDown_BranchPicker;
    public string[]         DropDown_BranchPicker_Options = { "Stable", "Nightly" }; // Is there a better way?
    public Label            Label_VerNum;
    public Label            Label_MainSubtitle;
    public Label            Label_MainTextBody;
    public Label            Label_UpdateBranchInfo;
    public Label            Label_Status;
    public Button           Button_UpdatePatch;
    public Button           Button_BrowseForRom;
    public Button           Button_TestButton; 
    public Button           Button_PatchExecute;
    public Button           Button_PlayGame;
    public ProgressBar      ProgressBar_Progress;
    public FilePicker       FilePicker_BrowseForRom;

	private static int currentGitObject = 0;

    #endregion Form Controls


    public MainForm()
    {
        XamlReader.Load(this);
        Start();
    }

    void Start()
    {
        // We want these things to start up as soon as the window opens
        // We want to test things before implementing them
        Testing();

        // Update the status text with a random string from Extras
        UpdateInitialStatusMessage();

        // Update options to match JSON
        DropDown_BranchPicker.SelectedValue = prefs.desiredBranch;
        CheckBox_UseHQModels.Checked = prefs.useHQModels;

        // Setup our general file structure
        fileIO.SetupFileStructure();
        
        // Setup preferences after folder layout is finished
        prefs.Setup();

        // Do a check for the patch file and inform the user

        // Check for the game and activate the PLAY button if it exists
        CheckForGame();

        // Would rather do this before building but it's fine for now
        // Please focus on functionality first
        foreach (var item in DropDown_BranchPicker_Options)
        {
            DropDown_BranchPicker.Items.Add(item);
        }
    }

    void Testing()
    {
        // Show all embedded resources in debug output
        // Useful for getting exact object references
        Assembly myAssembly = Assembly.GetExecutingAssembly();
        string[] names = myAssembly.GetManifestResourceNames();
        foreach (string name in names)
        {
            Debug.WriteLine($"MainForm.Testing: {name}");
        }
    }

    public void BrowseForFile()
    {

    }

    public void CheckForGame()
    {
        if (File.Exists(fileIO.patchedRomPath))
        {
            Button_PlayGame.Visible = true;
        }
    }

    // Control methods are in order from top to bottom
    #region UseHQModels CheckBox
    private void UseHQModels_CheckedChanged(object sender, EventArgs e) //Not sure ItemCheckEventArgs is correct here
    {
        if (CheckBox_UseHQModels.Checked == true)
        {
            // If toggled to true
            prefs.useHQModels = true;
            UpdateStatusText("High quality player models will be used", Color.FromArgb(255, 255, 255));
        }
        else
        {
            // Else toggled to false
            prefs.useHQModels = false;
            UpdateStatusText("Standard quality player models will be used", Color.FromArgb(255, 255, 255));
        }

        // Update our JSON after making adjustments
        prefs.SaveJSON(prefs);
    }
    #endregion

    #region DropDown BranchPicker
    public void DropDown_BranchPicker_SelectionChanged(object sender, EventArgs e)
    {
        // Get the selected item value as a lower-case string
        string selBranch = DropDown_BranchPicker.SelectedValue.ToString().ToLower();
        fileIO.currentBranch = selBranch;

        if (selBranch != null)
        {
            // Dropdown has an item selected
            if (selBranch == "stable")
            {
                // Choose the stable branch
                prefs.desiredBranch = "Stable";
            }
            else if (selBranch == "nightly")
            {
                // Choose the nightly branch
                prefs.desiredBranch = "Nightly";
            }

            prefs.SaveJSON(prefs);
            Debug.WriteLine($"MainForm.cs: Selected branch: {selBranch}");

        }
        else
        {
            Debug.WriteLine("MainForm.cs: DropDown is null? That can't be right...");
        }
    }
    #endregion

    #region UpdatePatch Button
    public void UpdatePatch_ButtonPress(object sender, EventArgs e)
    {
        
    }

    public async void UpdatePatch_ButtonRelease(object sender, EventArgs e)
    {
        Debug.WriteLine("MainForm.UpdatePatch: Checking for updates...");
        ToggleAllControls(false, true);

		await Git.CheckRepoForPatch(prefs, fileIO);

        // AM2R Launcher method
        //Git.PullPatchData(TransferProgressHandlerMethod);
        Debug.WriteLine("MainForm.UpdatePatch: Done checking!");
        ToggleAllControls(true, true);
    }

	/// <summary>
	/// This is just a helper method for the git commands in order to have a progress bar display for them.
	/// </summary>
	private bool TransferProgressHandlerMethod(TransferProgress transferProgress)
	{
		//if (isGitProcessGettingCancelled) return false;

		// This needs to be in an Invoke, in order to access the variables from the main thread
		// Otherwise this will throw a runtime exception
		Eto.Forms.Application.Instance.Invoke(() =>
		{
			ProgressBar_Progress.MinValue = 0;
			ProgressBar_Progress.MaxValue = transferProgress.TotalObjects;
			if (currentGitObject >= transferProgress.ReceivedObjects)
				return;
			Label_Status.Text = transferProgress.ReceivedObjects + " (" + ((int)transferProgress.ReceivedBytes / 1000000) + "MB) / " + transferProgress.TotalObjects + " objects";
			currentGitObject = transferProgress.ReceivedObjects;
			ProgressBar_Progress.Value = transferProgress.ReceivedObjects;
		});

		return true;
	}

	
    /// <summary>
    /// Method that updates <see cref="progressBar"/>.
    /// </summary>
    /// <param name="value">The value that <see cref="progressBar"/> should be set to.</param>
    /// <param name="min">The min value that <see cref="progressBar"/> should be set to.</param>
    /// <param name="max">The max value that <see cref="progressBar"/> should be set to.</param>
    private void UpdateProgressBar(int value, int min, int max)
	{
		Eto.Forms.Application.Instance.Invoke(() =>
		{
			ProgressBar_Progress.MinValue = min;
			ProgressBar_Progress.MaxValue = max;
			ProgressBar_Progress.Value = value;
		});
	}
	#endregion

    // This button doesn't exist anymore...
	#region FilePicker
	void FilePicker_DragDrop(object sender, EventArgs e)
    {
        // Do nothing on press to prevent misclicks
    }

    void FilePicker_PathChanged(object sender, EventArgs e)
    {
        Debug.WriteLine(FilePicker_BrowseForRom.FilePath);

        if (FilePicker_BrowseForRom.FilePath.EndsWith(".z64"))
        {
            fileIO.CopyFile(FilePicker_BrowseForRom.FilePath, fileIO.baseRomPath);
        }
    }
    #endregion

    #region PatchExecute Button
    void PatchExecute_ButtonPress(object sender, EventArgs e)
    {
        //Debug.WriteLine("Button pressed");
    }

    void PatchExecute_ButtonRelease(object sender, EventArgs e)
    {
        // MAUI doesn't handle button releases at this time, so we need to account for that
        Debug.WriteLine("Button released");

        // Copy the original rom file to the patchdata folder so we can have a controlled version
        if (FilePicker_BrowseForRom.FilePath.EndsWith(".z64"))
        {
            fileIO.CopyFile(FilePicker_BrowseForRom.FilePath, prefs.baseRomPath);
        }

        Xdelta3.ApplyPatch(fileIO, (Path.Combine(fileIO.baseDir, fileIO.baseRomPath)),
                                   (fileIO.chosenPatchPath),
                                   (Path.Combine(fileIO.baseDir, fileIO.patchedRomPath)));

        // We will apply these changes AFTER patching, otherwise CRC will break
        if (prefs.useHQModels)
        {
            Debug.WriteLine("Using HQ Models...");
            using var stream = System.IO.File.Open((Path.Combine(fileIO.baseDir, fileIO.patchedRomPath)), FileMode.Open);
            // It seems the position changes after any time it's read, so we have to keep setting the stream position
            // Fix later, get working now
            // Swap Sabre's model to HQ (0x7 to 0x8) at position 0x037EECA1
            stream.Position = 0x037EECA1;
            Debug.WriteLine($"Old Sabre Model Value: 0x037EECA1 0{stream.ReadByte()} ");

            stream.Position = 0x037EECA1;
            stream.WriteByte(8);

            stream.Position = 0x037EECA1;
            Debug.WriteLine($"New Sabre Model Value: 0x037EECA1 0{stream.ReadByte()} ");


            // Swap Krystal's model to HQ (0x0 to 0x2) at position 0x37EF18D
            stream.Position = 0x37EF18D;
            Debug.WriteLine($"Old Krystal Model Value: 0x37EF18D 0{stream.ReadByte()} ");

            stream.Position = 0x37EF18D;
            stream.WriteByte(2);

            stream.Position = 0x37EF18D;
            Debug.WriteLine($"New Krystal Model Value: 0x37EF18D 0{stream.ReadByte()} ");
        }
    }
    #endregion

    #region PlayGame Button
    private void Button_PlayGame_Pressed(object sender, EventArgs e)
    {

    }

    private void Button_PlayGame_Released(object sender, EventArgs e)
    {
        //// Reset button appearance on release
        //Button b = (Button)sender;
        //VisualStateManager.GoToState(b, "Normal");

        // There are many different N64 emulators, it's tough to account for that
        // So we may need to rely on the user's preferences for Z64 files
        // Find a way to initiate playing the game
        // Quick and dirty that isn't reliable
        Process.Start(@"cmd -c C:\Users\sabre\source\repos\DinoLauncherMAUI\DinoLauncherMAUI\bin\Debug\net6.0-windows10.0.19041.0\win10-x64\AppX\Game\dinosaurplanet.z64");
    }
    #endregion

    void TestButton_ButtonPress(object sender, EventArgs e)
    {
        //InfoPopup("ALERT", "This is a test!");
    }

    void TestButton_ButtonRelease(object sender, EventArgs e)
    {
        Testing();
    }

    #region UpdateStatusText Methods
    /// <summary>
    /// Method to quickly update the status text at the bottom of the screen.
    /// </summary>
    /// <param name="statusText"></param>
    public void UpdateStatusText(string statusText)
    {
        Label_Status.Text = statusText;
    }

    /// <summary>
    /// Method to quickly update the status text at the bottom of the screen, including color.
    /// </summary>
    /// <param name="statusText"></param>
    /// <param name="color"></param>
    public void UpdateStatusText(string statusText, Color color)
    {
        // Overload to optionally change the color of the status text for quick readability
        Label_Status.Text = statusText;
        Label_Status.TextColor = color;

    }
    #endregion

    #region ToggleAllControls
    /// <summary>
    /// Feed in two bool values to enable/disable/hide/unhide all controls. Useful in preventing
    /// the user from getting confused or breaking something during the process.
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="visible"></param>
    public void ToggleAllControls(bool enabled, bool visible)
    {
        // These are the controls that will be toggleable, add/remove as necessary
        Eto.Forms.Control[] controls = {CheckBox_UseHQModels,
                                        DropDown_BranchPicker,
                                        Button_UpdatePatch,
                                        FilePicker_BrowseForRom,
                                        Button_PatchExecute };

        foreach (var item in controls)
        {
            try
            {
                item.Enabled = enabled;
                item.Visible = visible;
            }
            catch (Exception ex)
            {
                // Just in case we're trying to enable/disable a UI control that doesn't work that way
                Debug.WriteLine($"DisableAllControls: {ex}");
            }
        }
    }
    #endregion

    public void UpdateInitialStatusMessage()
    {
        // Feeding Random a seed based on current milliconds and not CPU frequency because why would I?
        Random r = new Random(DateTime.Now.Millisecond);
        int i = r.Next(extras.randomStatusMessages.Length);
        Label_Status.Text = extras.randomStatusMessages[i];
    }

    // Pop ups! Ain't working right now. Not a priority, fix later.
    public void InfoPopup(string titleText, string bodyText)
    {
        Dialog dialog = new Dialog();

        // Controls and layout
        var layout = new DynamicLayout(dialog);
        var label = new Label { Text = bodyText };
        var okayButton = new Button { Text = "Okay!" };
        dialog.DefaultButton = okayButton;
        dialog.DefaultButton.Click += (sender, e) =>
        {
            Debug.WriteLine(dialog, "Default button clicked");
            dialog.Close();
        };

        // Generate layout for the dialog popup
        layout.BeginVertical();
        layout.Width = 315;
        layout.Height = 235;
        layout.AddRow(new Label { Text = bodyText });
        layout.EndBeginVertical();
        layout.AddRow(null, okayButton);
        layout.EndVertical();

        // dialog properties
        dialog.DisplayMode = DialogDisplayMode.Default;
        dialog.Topmost = true;
        dialog.Resizable = true;
        dialog.Width = 320;
        dialog.Height = 240;

        dialog.Title = titleText;


        // Show the dialog box
        // Need to clarify the parent is derived from Window
        try
        {
            Debug.WriteLine(this.ToString());
            dialog.Content = layout;
            // MainForm.InfoPopup: System.InvalidOperationException: Window must be the root of the tree. Cannot add Window as a child of Visual.
            // Figure it out later I guess
            dialog.ShowModal(this);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"MainForm.InfoPopup: {e}");
        }
        
    }
}