using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;
using System;
using System.Collections.Generic;
// ------------------------------------------- From previous iteration
using System.Diagnostics;
using System.Linq;
using Debug = System.Diagnostics.Debug;
using System.Text;
using FileMode = System.IO.FileMode;
//using Microsoft.Maui.Animations;
using System.Reflection;
//using WMPLib;
//using Microsoft.Maui.Controls;
//using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.ComponentModel;
using LibGit2Sharp;
using System.Threading.Tasks;
//using System.Windows.Media;
using System.Windows;
using Color = Eto.Drawing.Color;
//using System.Windows.Forms;
// This could probably be done a lot better
// But the goal is to make it work for now
using Label = Eto.Forms.Label;
using CheckBox = Eto.Forms.CheckBox;
using DropDown = Eto.Forms.DropDown;
using Button = Eto.Forms.Button;
using ProgressBar = Eto.Forms.ProgressBar;
using Application = Eto.Forms.Application;
using System.Collections;
using System.Collections.ObjectModel;
using DinoLauncherLib;
using Microsoft.VisualBasic;

namespace DinoLauncher;

// note to self: remember to pass object through parameters when you need
// that specifically generated object for some reason like prefs
// Note to Self: Visual element should rely on prefs, not the other way around

public class MainForm : Eto.Forms.Form
{
    // I think it'll be easier to just pass these along in function parameters
    FileIO fileIO = new FileIO(); // Use to reference and perform directory/file functions
    Extras extras = new Extras();// Use for fun things like music or w/e
    Xdelta3 xdelta3 = new Xdelta3(); // xdelta stuff
    UserPrefs prefs = new UserPrefs();
    Git git = new Git();

    // Get this info from prefs please
    bool useHQModels;

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
 
    public Button           Button_PatchExecute;
    public Button           Button_PlayGame;
    public ProgressBar      ProgressBar_Progress;

    #endregion Form Controls


    public MainForm()
    {
        XamlReader.Load(this);
        Start();
    }

    void Start()
    {
        // We want these things to start up as soon as the window opens
        // We want to test things before moving them around
        Testing();

        // Setup our general file structure
        fileIO.SetupFileStructure();

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
            Debug.WriteLine(name);
        }
    }

    // Brain hurt fix later
    //public Collection<FileFilter> FileFilter_Filter(int sel)
    //{
    //    // 0 = .Z64 Rom
    //    // 1 = *.*

    //    string fName = "";
    //    string[] fExt = { "" };

    //    Collection c = new Collection();
    //    FileFilter f = new FileFilter();

    //    if (sel == 0)
    //    {
    //        // 0 = .Z64 Rom
    //        fName = new string("rom_crack.z64");
    //        fExt[0] = ".z64";
    //    }
    //    else if (sel == 1)
    //    {
    //        // 1 = *.*
    //        fName = new string("*.*");
    //        fExt[0] = "*.*";
    //    }

    //    return f(out fName, out fExt);
    //}

    // Control methods are in order from top to bottom
    #region UseHQModels CheckBox
    private void UseHQModels_CheckedChanged(object sender, EventArgs e) //Not sure ItemCheckEventArgs is correct here
    {
        if (CheckBox_UseHQModels.Checked == true)
        {
            // If toggled to true
            useHQModels = true;
            UpdateStatusText("High quality player models will be used", Color.FromArgb(255, 255, 255, 255));
        }
        else
        {
            // Else toggled to false
            useHQModels = false;
            UpdateStatusText("Standard quality player models will be used", Color.FromArgb(255, 255, 255, 255));
        }
    }
    #endregion

    #region DropDown BranchPicker
    private void DropDown_BranchPicker_SelectionChanged(object sender, EventArgs e)
    {
        // Get the selected item value as a lower-case string
        string selBranch = DropDown_BranchPicker.SelectedValue.ToString().ToLower();

        if (selBranch != null)
        {
            // Dropdown has an item selected
            if (selBranch == "stable")
            {
                // Choose the stable branch
                prefs.desiredBranch = "stable";
            }
            else if (selBranch == "nightly")
            {
                // Choose the nightly branch
                prefs.desiredBranch = "nightly";
            }
        }

        Debug.WriteLine("desired branch: " + prefs.desiredBranch);
        Debug.WriteLine("selected branch: " + selBranch);
    }
    #endregion

    #region UpdatePatch Button
    public void UpdatePatch_ButtonPress(object sender, EventArgs e)
    {
        // Actually, let's not do anything on press to prevent mis-clicks
    }

    public void UpdatePatch_ButtonRelease(object sender, EventArgs e)
    {
        //Task.Run(async () => { await GitTestTask(); }).Wait();
        git.CheckRepoForPatch(prefs, fileIO);
        //Task.Run(async () => { await git.CheckRepoForPatch(prefs, fileIO); }).Wait();

        //// Resets the button back to "Normal" visual state on release
        //// Only necessary on Windows because of course it is
        //Button b = (Button)sender;
        //VisualStateManager.GoToState(b, "Normal");
    }
    #endregion

    #region BrowseForFile Button
    void BrowseForFile_ButtonPress(object sender, EventArgs e)
    {
        // Do nothing on press to prevent misclicks, maybe play a partial sound idk 
        // But for now just do nothing
    }

    void BrowseForFile_ButtonRelease(object sender, EventArgs e)
    {
        //// Resets the button back to "Normal" visual state on release
        //// Only necessary on Windows because of course it is
        //Button b = (Button)sender;
        //VisualStateManager.GoToState(b, "Normal");

        // File pick on a separate thread
        FilePicker o = new()
        {
            Title = "Browse for \'rom_crack.z64\'"
            // We can specify file type later
        };

        //Task.Run(async () => { await BrowseForFileTask(o); }).Wait();
    }

    // Re-do the FilePicker method
    //// I don't think I can reuse a lot of this with Eto
    //public async Task<FilePicker> BrowseForFileTask()
    //{
    //    try
    //    {
    //        // Opens the file picker
    //        var result = await FilePicker.Callback();

    //        if (result != null)
    //        {
    //            if (result.FileName.EndsWith(".z64", StringComparison.OrdinalIgnoreCase))
    //            {
    //                using var stream = await result.OpenReadAsync();
    //                var romFile = ImageSource.FromStream(() => stream);

    //                if (fileIO.MD5Checksum((fileIO.currentDirectory + fileIO.romCrackPath)) == true)
    //                {
    //                    // Md5 check should be successful
    //                    Debug.WriteLine("Md5 Checksum successful!");

    //                    UpdateStatusText(fileIO.CalculateMD5(result.FullPath.ToString()), Color.FromArgb(255, 0, 255, 0));

    //                    // Load the result path
    //                    // load load load load load...

    //                    // No need to get rom_crack.z64 again
    //                    // Disable the button to prevent confusion
    //                    // Enable the patch button
    //                    //Application.Current.MainPage.Dispatcher.Dispatch(() => Button_BrowseForRom.IsVisible = false);

    //                    // Enable the patch button
    //                    //Application.Current.MainPage.Dispatcher.Dispatch(() => Button_PatchExecute.IsEnabled = true);
    //                }
    //                else
    //                {
    //                    Debug.WriteLine("Yours: " + fileIO.CalculateMD5(result.FullPath));
    //                    Debug.WriteLine("OG: " + fileIO.originalMd5);
    //                    // Md5 was not successful
    //                    UpdateStatusText("Bad Checksum! Check your file and try again.", Color.FromArgb(255 ,255 ,0 ,0));
    //                }
    //            }
    //            else
    //            {
    //                Debug.WriteLine("Incorrect file type chosen", Color.FromArgb(255 ,255 ,0 ,0));

    //                UpdateStatusText("That's not a .Z64 file.", Color.FromArgb(255, 255, 0, 0));
    //            }
    //        }
    //        return result;
    //    }
    //    catch (Exception e)
    //    {
    //        // The user canceled or something went wrong
    //        Debug.WriteLine(e);
    //    }
    //    return null;
    //}
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


        xdelta3.ApplyPatch(fileIO, (fileIO.currentDirectory + fileIO.romCrackPath),
                                   (fileIO.currentDirectory + fileIO.chosenPatchPath),
                                   (fileIO.currentDirectory + fileIO.patchedRomPath));

        // We will apply these changes AFTER patching, otherwise CRC will break
        if (useHQModels == true)
        {
            Debug.WriteLine("Using HQ Models...");
            using (var stream = System.IO.File.Open((fileIO.currentDirectory + fileIO.patchedRomPath), FileMode.Open))
            {
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

        //// Resets the button back to "Normal" visual state on release
        //// Only necessary on Windows because of course it is
        //// Apparently it's related to how Windows handles object focus
        //Button b = (Button)sender;
        //VisualStateManager.GoToState(b, "Normal");
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
        Eto.Forms.Control[] controls = {CheckBox_UseHQModels,
                                        DropDown_BranchPicker,
                                        Button_UpdatePatch,
                                        Button_BrowseForRom,
                                        Button_PatchExecute,
                                        Button_PlayGame};

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
}