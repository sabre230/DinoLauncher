using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Documents;
using DinoLauncherLib;
using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using LibGit2Sharp;
using Xceed.Wpf.AvalonDock.Properties;

namespace DinoLauncher;

public class MainForm : Form
{
    // I think it'll be easier to just pass these along in function parameters
    FileIO fileIO = new FileIO(); // Use to reference and perform directory/file functions
    Extras extras = new Extras();// Use for fun things like music or w/e
    UserPrefs prefs = new UserPrefs();
    JSON json = new JSON();

    // Quick and dirty copy/paste to get embedded background playing nice
    static Assembly ass = Assembly.GetExecutingAssembly();
    static Stream stream = ass.GetManifestResourceStream("DinoLauncher.res.Images.background.png");
    private readonly Bitmap formBG = new Bitmap(stream);

    // Looks like Eto doesn't auto-generate controls in the xeto form
    // According to internet wizards, I have to add them to my class as data members
    // or properties with the same name
    // Weird

    #region Form Controls
    // When adding/removing any controls from MainForm.xeto, these need to be adjusted to match
    public StackLayout      StackLayout;
    public ImageView        Image_DinosaurPlanetLogo;
    public CheckBox         CheckBox_UseHQModels;
    public DropDown         DropDown_BranchPicker;
    public string[]         DropDown_BranchPicker_Options = { "stable", "nightly" }; // Is there a better way?
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
    public Drawable         Drawable_Drawable;

    #endregion 


    public MainForm()
    {
        XamlReader.Load(this);
        Start();
    }

    void Start()
    {
        // We want these things to start up as soon as the window opens, be careful with the order!
        // We want to test things before implementing them
        //Testing();

        // Would rather do this before the window is rendered but it's fine for now
        foreach (var item in DropDown_BranchPicker_Options)
        {
            DropDown_BranchPicker.Items.Add(item);
        }

        // Update the status text with a random string from Extras
        UpdateInitialStatusMessage();

        // Setup our general file structure
        fileIO.SetupFileStructure();

        // Setup preferences after folder layout is finished
        prefs.Setup();

        // Update UI to match saved prefs
        UpdateUI();

        // Check for the game and activate the PLAY button if it exists (later)
        //CheckForGame();


    }

    // Use this to draw the background
    private void Drawable_Paint(object sender, PaintEventArgs e)
    {
        // Exit if sender is not a Drawable
        Drawable drawable = sender as Drawable;
        if (drawable == null) return;
        
        // Our form will not be changing dimensions, so we can keep this nice and simple
        e.Graphics.DrawImage(formBG, 0, 0, 800, 600);
    }

    void Testing()
    {
        //// Show all embedded resources in debug output
        //// Useful for getting exact object references
        //Assembly myAssembly = Assembly.GetExecutingAssembly();
        //string[] names = myAssembly.GetManifestResourceNames();
        //foreach (string name in names)
        //{
        //    Debug.WriteLine($"MainForm.Testing: {name}");
        //}
    }

    void UpdateUI()
    {
        // Load JSON stuff into UI
        CheckBox_UseHQModels.Checked = prefs.UseHQModels;
        DropDown_BranchPicker.SelectedValue = prefs.UpdateBranch;
        FilePicker_BrowseForRom.FilePath = prefs.OriginalRomPath;
    }

    public void CheckForGame()
    {
        if (File.Exists(fileIO.patchedRomPath))
        {
            // Wait on this until we can figure out a viable way to launch N64 games reliably
            //Button_PlayGame.Visible = true;
        }
    }

    // Control methods shoule be in order from top to bottom
    #region UseHQModels CheckBox
    private void UseHQModels_CheckedChanged(object sender, EventArgs e) //Not sure ItemCheckEventArgs is correct here
    {
        if (CheckBox_UseHQModels.Checked == true)
        {
            prefs.UseHQModels = true;
        }
        else
        {
            prefs.UseHQModels = false;
        }
    }

    private async void UseHQModels_MouseUp(object sender, EventArgs e) //Not sure ItemCheckEventArgs is correct here
    {
        // Save our JSON after making adjustments, typically on MouseUp
        await prefs.SaveJSON();
    }
    #endregion

    #region DropDown BranchPicker
    public void DropDown_BranchPicker_SelectionChanged(object sender, EventArgs e)
    {
        // Moved to DropDown_BranchPicker_MouseUp to prevent issues with saving prefs
        string selBranch = DropDown_BranchPicker.SelectedValue.ToString().ToLower();
        fileIO.currentBranch = selBranch;

        if (selBranch != null)
        {
            prefs.UpdateBranch = selBranch.ToLower();

            // Should be doing this on Picker_MouseUp
            //prefs.SaveJSON(prefs);
            Debug.WriteLine($"MainForm.DropDown: Selected branch: {selBranch}");

            Button_UpdatePatch.Enabled = true;
        }
        else
        {
            Debug.WriteLine($"MainForm.DropDown: DropDown is {selBranch}? That can't be right...");
        }
    }

    public async void DropDown_BranchPicker_MouseUp(object sender, EventArgs e)
    {
        // Get the selected item value as a lower-case string
        // Update our JSON file please
        await prefs.SaveJSON();
    }
    #endregion

    #region UpdatePatch Button
    public void UpdatePatch_ButtonPress(object sender, EventArgs e) { }

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
	#endregion

	#region FilePicker
	void FilePicker_DragDrop(object sender, EventArgs e)
    {
        // Worry about this later
    }

    async void FilePicker_PathChanged(object sender, EventArgs e)
    {
        var path = FilePicker_BrowseForRom.FilePath;
        Debug.WriteLine($"MainForm.FilePicker_PathChanged: Looking for {path}");

        try
        {
            // Copy the rom to our working directory
            if (FilePicker_BrowseForRom.FilePath.EndsWith(".z64"))
            {
                // Set the baserom path to be the local copy from now on
                File.Copy(path, fileIO.baseRomPath, true); // True for overwrite

                // Enable the Apply Patch button
                Button_PatchExecute.Enabled = true;

                // Only do this if the path is valid
                prefs.OriginalRomPath = path;
                await prefs.SaveJSON();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainForm.FilePicker_PathChanged: {ex}");
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
        // Copy the original rom file to the patchdata folder so we can have a controlled version
        if (FilePicker_BrowseForRom.FilePath.EndsWith(".z64") && File.Exists(fileIO.patchedRomPath))
        {
            File.Copy(FilePicker_BrowseForRom.FilePath, prefs.OriginalRomPath, true); // true for overwrite
        }

        Xdelta3.ApplyPatch(fileIO, (Path.Combine(fileIO.baseDir, fileIO.baseRomPath)),
                                   (fileIO.chosenPatchPath),
                                   (Path.Combine(fileIO.baseDir, fileIO.patchedRomPath)));

        // We will apply these changes AFTER patching, otherwise CRC will break
        if (prefs.UseHQModels)
        {
            Debug.WriteLine("Using HQ Models...");
            using var stream = File.Open((Path.Combine(fileIO.baseDir, fileIO.patchedRomPath)), FileMode.Open);
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
        // There are many different N64 emulators, it's tough to account for that
        // Find a way to initiate playing the game
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
        Control[] controls = {CheckBox_UseHQModels,
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
            Debug.WriteLine(ToString());
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

    /// <summary>
    /// Method that updates <see cref="progressBar"/>.
    /// </summary>
    /// <param name="value">The value that <see cref="progressBar"/> should be set to.</param>
    /// <param name="min">The min value that <see cref="progressBar"/> should be set to.</param>
    /// <param name="max">The max value that <see cref="progressBar"/> should be set to.</param>
    private void UpdateProgressBar(int value, int min, int max)
    {
        Application.Instance.Invoke(() =>
        {
            ProgressBar_Progress.MinValue = min;
            ProgressBar_Progress.MaxValue = max;
            ProgressBar_Progress.Value = value;
        });
    }
}