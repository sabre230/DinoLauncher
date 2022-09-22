using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.IO;
//using Android.OS;
using LibGit2Sharp;

namespace DinoLauncher;

public class Git
{
    const string dinoPatchURL = "https://github.com/sabre230/DinoPatchRepo.git";
    string workingDir;

    // -- Functions Outline --
    // Check which branch we're using
    // LOW PRIORTY: Check for custom source (possibility of 3rd party patches, user will be warned before this is enabled)
    // Check git repos, including backups
    // Check latest patch against local patch
    // Download latest patch and pass to Xdelta3

    public async Task CheckRepoForPatch(UserPrefs prefs, FileIO fileIO)
    {
        // workingDir should be the subfolder "git" in the "PatchData" folder
        // ".git" may be causing issues
        workingDir = fileIO.currentDirectory + "\\PatchData\\git";

        // Remove the git folder for testing purposes
        if (Directory.Exists(workingDir))
        {
            System.Diagnostics.Debug.WriteLine("Dir exists " + workingDir);
            // Remove that effing .git folder holy fart
            foreach (var item in Directory.GetFiles(workingDir + "\\.git", "*", new EnumerationOptions { RecurseSubdirectories = true} ))
            {
                await fileIO.DeleteFile(item);
            }

            // Please remove this damn thing already
            await fileIO.DeleteDirectory(workingDir + "\\git");
            await fileIO.DeleteDirectory(workingDir);
            System.Diagnostics.Debug.WriteLine(workingDir);
        }

        try
        {
            if (!Directory.Exists(workingDir))
            {
                System.Diagnostics.Debug.WriteLine("workingDir with .git exists");
                if (prefs.desiredBranch == "nightly")
                {
                    // We want the Nightly branch
                    await Task.Run(() => Repository.Clone(dinoPatchURL, workingDir, new CloneOptions { BranchName = "nightly" }));
                }
                else
                {
                    // We want the Stable branch
                    await Task.Run(() => Repository.Clone(dinoPatchURL, workingDir, new CloneOptions { BranchName = "stable" }));
                }
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("Stupid Directory Delete: " + e);
        }
    }

    // Perhaps we should be putting together a progress bar for this?

}