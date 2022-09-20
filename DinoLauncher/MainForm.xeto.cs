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
using System.Windows.Media;
using System.Windows;
using Eto.
//using System.Collections.Generic;

namespace DinoLauncher;

// note to self: remember to pass object through parameters when you need
// that specifically generated object for some reason like prefs

public class MainForm : Form
{
    // I think it'll be easier to just pass these along in function parameters
    FileIO fileIO = new FileIO(); // Use to reference and perform directory/file functions
    Extras extras = new Extras();// Use for fun things like music or w/e
    Xdelta3 xdelta3 = new Xdelta3(); // xdelta stuff
    UserPrefs prefs = new UserPrefs();
    Git git = new Git();

    // Get this info from prefs please
    bool useHQModels;


    public MainForm()
    {
        XamlReader.Load(this);
        Start();
    }

    void Start()
    {
        // We want these things to start up as soon as the window opens

        // Setup our general file structure
        fileIO.SetupFileStructure();
        
        // Note to Self: Visual element should rely on prefs, not the other way around
        // Please clean this up soon
        prefs.useHQModels = 

        // Don't do music right now please, look at this after base functionality is confirmed to be working
        //extras.PlayMusic(@"\\Resources\\music.mp3");
    }

    // Control methods are in order from top to bottom
    #region UseHQModels CheckBox
    private void UseHQModels_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (CheckBox_UseHQModels.IsChecked == true)
        {
            // If toggled to true
            useHQModels = true;
            UpdateStatusText("High quality player models will be used", Color.FromArgb("FFFFFF"));
        }
        else
        {
            // Else toggled to false
            useHQModels = false;
            UpdateStatusText("Standard quality player models will be used", Color.FromArgb("FFFFFF"));
        }
    }
    #endregion

    #region BranchPicker Picker
    private void Picker_BranchPicker_SelectionChanged(object sender, EventArgs e)
    {
        if (Picker_BranchPicker.SelectedItem.ToString() == "Stable")
        {
            // Do this for now, make simple later
            fileIO.SetupFileStructure();
            // We want the STABLE branch
            //fileIO.chosenPatchPath = "\\PatchData\\dp-stable.xdelta";


            // Nah we're doing this different now
            prefs.desiredBranch = "stable";
        }
        else if (Picker_BranchPicker.SelectedItem.ToString() == "Nightly")
        {
            // Do this for now, make simple later
            fileIO.SetupFileStructure();

            prefs.desiredBranch = "nightly";
        }

        Debug.WriteLine("desired branch: " + prefs.desiredBranch);
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

        // Resets the button back to "Normal" visual state on release
        // Only necessary on Windows because of course it is
        Button b = (Button)sender;
        VisualStateManager.GoToState(b, "Normal");
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
        // Resets the button back to "Normal" visual state on release
        // Only necessary on Windows because of course it is
        Button b = (Button)sender;
        VisualStateManager.GoToState(b, "Normal");

        // File pick on a separate thread
        PickOptions o = new()
        {
            PickerTitle = "Browse for \'rom_crack.z64\'"
            // We can specify file type later
        };

        Task.Run(async () => { await BrowseForFileTask(o); }).Wait();
    }

    public async Task<FileResult> BrowseForFileTask(PickOptions options)
    {
        try
        {
            // Opens the file picker
            var result = await FilePicker.Default.PickAsync(options);

            if (result != null)
            {
                if (result.FileName.EndsWith(".z64", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = await result.OpenReadAsync();
                    var romFile = ImageSource.FromStream(() => stream);

                    if (fileIO.MD5Checksum((fileIO.currentDirectory + fileIO.romCrackPath)) == true)
                    {
                        // Md5 check should be successful
                        Debug.WriteLine("Md5 Checksum successful!");

                        UpdateStatusText(fileIO.CalculateMD5(result.FullPath.ToString()), Color.FromArgb("00FF00"));

                        // Load the result path
                        // load load load load load...

                        // No need to get rom_crack.z64 again
                        // Disable the button to prevent confusion
                        // Enable the patch button
                        Application.Current.MainPage.Dispatcher.Dispatch(() => Button_BrowseForRom.IsVisible = false);

                        // Enable the patch button
                        Application.Current.MainPage.Dispatcher.Dispatch(() => Button_PatchExecute.IsEnabled = true);
                    }
                    else
                    {
                        Debug.WriteLine("Yours: " + fileIO.CalculateMD5(result.FullPath));
                        Debug.WriteLine("OG: " + fileIO.originalMd5);
                        // Md5 was not successful
                        UpdateStatusText("Bad Checksum! Check your file and try again.", Color.FromArgb("FF0000"));
                    }
                }
                else
                {
                    Debug.WriteLine("Incorrect file type chosen", Color.FromArgb("FF0000"));

                    UpdateStatusText("That's not a .Z64 file.", Color.FromArgb("FF0000"));
                }
            }
            return result;
        }
        catch (Exception e)
        {
            // The user canceled or something went wrong
            Debug.WriteLine(e);
        }
        return null;
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


        xdelta3.ApplyPatch(fileIO, (fileIO.currentDirectory + fileIO.romCrackPath),
                                   (fileIO.currentDirectory + fileIO.chosenPatchPath),
                                   (fileIO.currentDirectory + fileIO.patchedRomPath));

        // We will apply these changes AFTER patching, otherwise CRC will break
        if (useHQModels == true)
        {
            Debug.WriteLine("Using HQ Models...");
            using (var stream = File.Open((fileIO.currentDirectory + fileIO.patchedRomPath), FileMode.Open))
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

        // Resets the button back to "Normal" visual state on release
        // Only necessary on Windows because of course it is
        // Apparently it's related to how Windows handles object focus
        Button b = (Button)sender;
        VisualStateManager.GoToState(b, "Normal");
    }
    #endregion

    #region PlayGame Button
    private void Button_PlayGame_Pressed(object sender, EventArgs e)
    {

    }

    private void Button_PlayGame_Released(object sender, EventArgs e)
    {
        // Reset button appearance on release
        Button b = (Button)sender;
        VisualStateManager.GoToState(b, "Normal");

        // There are many different N64 emulators, it's tough to account for that
        // So we may need to rely on the user's preferences for Z64 files
        // Find a way to initiate playing the game
        // Quick and dirty that isn't reliable
        Process.Start(@"cmd -c C:\Users\sabre\source\repos\DinoLauncherMAUI\DinoLauncherMAUI\bin\Debug\net6.0-windows10.0.19041.0\win10-x64\AppX\Game\dinosaurplanet.z64");
    }
    #endregion

    #region UpdateStatusText Methods
    public void UpdateStatusText(string statusText)
    {
        // Makes updating the text on the UI thread much easier
        Application.Current.MainPage.Dispatcher.Dispatch(() => Label_Status.Text = statusText);

    }

    public void UpdateStatusText(string statusText, Color color)
    {
        // Maybe there's a better way of creating an overload...
        // This works for now, look at this later
        Application.Current.MainPage.Dispatcher.Dispatch(() => Label_Status.Text = statusText);
        Application.Current.MainPage.Dispatcher.Dispatch(() => Label_Status.TextColor = color);

    }
    #endregion

    public void DisableAllControls()
    {
        // Seriously, just disable all the controls.
        // Use this for when the application needs to do things
        // on the backend and we don't want the user to break anything

        Application.Current.MainPage.Dispatcher.Dispatch(() => Button_UpdatePatch.IsEnabled = false);
        Application.Current.MainPage.Dispatcher.Dispatch(() => Button_BrowseForRom.IsEnabled = false);
        Application.Current.MainPage.Dispatcher.Dispatch(() => Button_PatchExecute.IsEnabled = false);
        Application.Current.MainPage.Dispatcher.Dispatch(() => Button_PlayGame.IsEnabled = false);
        Application.Current.MainPage.Dispatcher.Dispatch(() => CheckBox_UseHQModels.IsEnabled = false);
        Application.Current.MainPage.Dispatcher.Dispatch(() => Picker_BranchPicker.IsEnabled = false);
    }
}
