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
    public string originalMd5 = "c4c1b52f9c4469c6c747942891de3cfd";

    // Get the application directory
    public string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    // Paths and path extensions
    public string chosenPatchPath = Path.Combine("_PatchData", "dp-stable.xdelta"); // We'll need to change this to not rely on a hardcoded name
    public string baseRomPath = Path.Combine("_PatchData", "rom_crack.z64"); // Same with this
    public string xdeltaPath = Path.Combine("_PatchData", "Xdelta3.exe");
    public string patchedRomPath = Path.Combine("_PatchData", "dinoplanet.z64");
    public string gitWorkDir;

    public string musicPath = Path.Combine("_PatchData", "music.mp3");
    public string assemblyPath;

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

        // Create a directory to handle all the things
        // Really just add a bunch of folders if they don't exist, write this better later, make it work for now
        if (!Directory.Exists(Path.Combine(baseDir, "_PatchData")))
        {
            CreateDirectory(Path.Combine(baseDir, "_PatchData")); // \\$"{baseDir}\\_PatchData");
        }
        if (!Directory.Exists(Path.Combine(baseDir, "_PatchData", "git")))
        {
            CreateDirectory(Path.Combine(baseDir, "_PatchData", "git"));
        }
        if (!Directory.Exists(Path.Combine(baseDir, "_Resources")))
        {
            CreateDirectory(Path.Combine(baseDir, "_Resources"));
        }
        if (!Directory.Exists(Path.Combine(baseDir, "_Game")))
        {
            CreateDirectory(Path.Combine(baseDir, "_Game"));
        }
    }

    /// <summary>
    /// Helper class to create directories with specific permissions. 
    /// This should help with I/O permissions issues.
    /// </summary>
    void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);

        DirectoryInfo dir = new DirectoryInfo(path);
        var dirSec = dir.GetAccessControl();

        // Directory security BS, futz with later
        //dirSec.AddAccessRule(new FileSystemAccessRule(path, FileSystemRights.FullControl, AccessControlType.Allow));
        //dir.SetAccessControl(dirSec);
    }

    /// <summary>
    /// Simple tool for moving files (string source, string destination)
    /// </summary>
    public void MoveFile(string source, string destination)
    {
        if (!File.Exists(source))
        {
            Debug.WriteLine($"FileIO.MoveFile(): {source} does not exist.");
        }
        else
        {
            // .net standard does not allow overwriting on move
            // Will ALWAYS overwrite, be careful!
            if (File.Exists(destination))
                File.Delete(destination);
            File.Move(source, destination);
        }
    }

    /// <summary>
    /// Simple tool for copying files (string source, string destination)
    /// </summary>
    public void CopyFile(string source, string destination)
    {
        if (!File.Exists(source))
        {
            Debug.WriteLine($"FileIO.CopyFile(): {source} does not exist.");
        }
        else
        {
            File.Copy(source, destination, true); // Will ALWAYS overwrite, be careful!
        }
    }

    /// <summary>
    /// Simple tool for deleting files (string path)
    /// </summary>
    public Task<string> DeleteFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.WriteLine($"FileIO.DeleteFile(): {path} does not exist.");
        }
        else
        {
            // Need to give permission to this somehow
            File.Delete(path);
            System.Diagnostics.Debug.WriteLine($"FileIO.DeleteFile: Deleted {path}");
        }

        // Some magic internet C# BS 
        return Task.FromResult<string>(null);
    }

	/// <summary>
	/// This is a custom method, that deletes a Directory. The reason this is used, instead of <see cref="Directory.Delete(string)"/>,
	/// is because this one sets the attributes of all files to be deletable, while <see cref="Directory.Delete(string)"/> does not do that on it's own.
	/// It's needed, because sometimes there are read-only files being generated, that would normally need more code in order to reset the attributes.<br/>
	/// Note, that this method acts recursively. Stolen from AM2R Community 👀.
	/// </summary>
	/// <param name="path">The directory to delete.</param>
	public static void DeleteDirectory(string path)
    {
		if (!Directory.Exists(path)) return;

		File.SetAttributes(path, FileAttributes.Normal);

		foreach (string file in Directory.GetFiles(path))
		{
			File.SetAttributes(file, FileAttributes.Normal);
			File.Delete(file);
		}

		foreach (string dir in Directory.GetDirectories(path))
		{
			DeleteDirectory(dir);
		}

		Directory.Delete(path, false);
	}

    /// <summary>
    /// Returns a bool value depending on whether or not a file exists (string path)
    /// </summary>
    public bool DoesFileExist(string path) // Returns a bool depending on if the file was found or not
    {
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
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
    /// Alternate Md5 generator, returns a bool value after comparing to the base rom Md5 (string path)
    /// </summary>
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
        // This is going to be important to making sure the patch always works regardless of name
        // The most recent patch should always be in the "active" folder
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
            FileInfo info = new FileInfo(item);
            Debug.WriteLine(item.ToString(), info.CreationTime, info.LastWriteTime);
        }
        // If we made it this far, we good
        return true;
        //chosenPatchPath 
    }
}