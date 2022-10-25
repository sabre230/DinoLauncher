using System;
using System.IO;
using System.Security.AccessControl;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug; // We want to use the Windows debug out for, well, debugging
using System.Data.Common;
using System.Linq;

namespace DinoLauncherLib;

public class FileIO
{
    // Prepare file attribute variables
    public string lastWrite;
    public string creationDate;
    public string md5;
    // Keep ths around to check against the original rom_crack.z64 Md5 Checksum
    const string originalMd5 = "c4c1b52f9c4469c6c747942891de3cfd";

    // Get the application directory
    public string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    // Paths and path extensions
    public string chosenPatchPath = Path.Combine("_PatchData", "dp-stable.xdelta");
    public string baseRomPath = Path.Combine("_PatchData", "rom_crack.z64");
    public string xdeltaPath = Path.Combine("_Resources", "xdelta3.exe");
    public string patchedRomPath = Path.Combine("_Game", "dinosaurplanet.z64");
    public string gitWorkDir;

    //public string assemblyPath;
    //public string currentBranch;

    /// <summary>
    /// Function to quickly build the folder structure required by DinoLauncher
    /// </summary>
    public void SetupFileStructure()
    {
        // Store executable directory
        string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        gitWorkDir = Path.Combine(baseDir, "_PatchData");

        // Set the current working directory to the application root
        System.IO.Directory.SetCurrentDirectory(baseDir);


        // These are now included in the project by default
        // This check is now redunant and can probably be removed later
        #region Check for directories
        if (!Directory.Exists(Path.Combine(baseDir, "_PatchData")))
        {
            Directory.CreateDirectory(Path.Combine(baseDir, "_PatchData")); // \\$"{baseDir}\\_PatchData");
        }
        if (!Directory.Exists(Path.Combine(baseDir, "_PatchData", "git")))
        {
            Directory.CreateDirectory(Path.Combine(baseDir, "_PatchData", "git"));
        }
        if (!Directory.Exists(Path.Combine(baseDir, "_Resources")))
        {
            Directory.CreateDirectory(Path.Combine(baseDir, "_Resources"));
        }
        if (!Directory.Exists(Path.Combine(baseDir, "_Game")))
        {
            Directory.CreateDirectory(Path.Combine(baseDir, "_Game"));
        }
        #endregion

        if (!File.Exists(xdeltaPath))
        {
            // Make sure to copy over the xdelta3 utility so patching actually works
            //WriteResourceToFile("DinoLauncher.res.xdelta3.exe", xdeltaPath);
            // This is completely unnecessary and should probably change
            File.Copy(Path.Combine("res", "xdelta3.exe"), Path.Combine("_Resources","xdelta3.exe"));
        }
    }

    /// <summary>
    /// Use to get the file's creation date, last write date, and generate an Md5 (string path)
    /// </summary>
    public void GetFileInfo(string path) // Parameter will need to be baseDir + chosenPatchPath (FileIO.currentDir + FileIO.chosenPatch?)
    {
        // Get the creation date of the most recent patch and format it as YYYYMMDD
        creationDate = File.GetCreationTime(path).ToString("yyyyMMdd");
        // Get the last write date, sometimes more accurate than creationDate, I like having both available
        lastWrite = File.GetLastWriteTime(path).ToString("yyyMMdd");
        md5 = CalculateMD5(path);
    }

    /// <summary>
    /// Used by GetFileInfo to generate an Md5
    /// </summary>
    public string CalculateMD5(string filename)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filename);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Alternate Md5 generator, returns a bool value after comparing to the base ROM Md5
    /// </summary>
    /// <param name="path">Path to the base ROM</param>
    public bool MD5Checksum(string path)
    {
        md5 = CalculateMD5(path);

        if (md5 == originalMd5)
        {
            Debug.WriteLine("Md5 checksum passed!");
            return true;
        }
        else
        {
            Debug.WriteLine("Md5 checksum failed.");
            return false;
        }
    }

    public bool GetLocalPatchPath()
    {
        // This must be done AFTER cloning!!
        // This is going to be important to making sure the patch always works regardless of name
        // The most recent patch should always be in the "active" folder

        try
        {
            var files = Directory.GetFiles(Path.Combine(baseDir, "_PatchData", "git", "active"), "*.xdelta", SearchOption.AllDirectories).ToList();

            if (files.Count > 1)
            {
                Debug.WriteLine("FileIO.GetLocalPatchPath: Multiple files found in \"active\" which is not allowed!");
                return false;
            }
            else if (files.Count == 0)
            {
                Debug.WriteLine("FileIO.GetLocalPatchPath: No xdelta patch found in \"active\"!");
                return false;
            }

            foreach (var item in files)
            {
                // Need to find which is the most recent and assign that to our selected patch file
                // We'll figure this out later since we already are checking for a single xdelta file
                FileInfo info = new FileInfo(item);
                Debug.WriteLine($"FileIO.GetLocalPatchPath: {item.ToString()}, {info.CreationTime}, {info.LastWriteTime}");
            }

            // If we made it this far, everything should be good
            // Return True and set our patch path so we can use it!
            chosenPatchPath = files[0]; // I mean, it's simple and it should work, so...

            return true;
            //chosenPatchPath 
        }
        catch (Exception e)
        {
            Debug.WriteLine($"FileIO.GetLocalPatchPath: {e}");
            return false;
        }
    }
}