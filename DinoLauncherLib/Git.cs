using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace DinoLauncherLib;

public static class Git
{
    private const string dinoPatchURL = "https://github.com/sabre230/DinoPatchRepo.git";

    public static async Task CheckRepoForPatch(UserPrefs prefs, FileIO fileIO)
    {
        string workingDir = $"{ fileIO.baseDir }\\_PatchData\\git";

        try
        {
            // First check if the working directory exists
            if (Directory.Exists(workingDir))
            {
                System.Diagnostics.Debug.WriteLine($"CheckRepoForPatch: Found working path: {workingDir}");


                fileIO.DeleteFile($"{workingDir}\\.git\\objects\\pack\\*.idx");

                // Go through the directory and remove all items
                foreach (var item in Directory.GetFiles($"{workingDir}", "*", SearchOption.AllDirectories ))
                {
                    await fileIO.DeleteFile(item);
                }

                // Please remove this damn thing already
                //await fileIO.DeleteDirectory($"{workingDir}\\git");
                //await fileIO.DeleteDirectory(workingDir);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CheckRepoForPatch: Did not find working path at: {workingDir}!");
                // Kill the method
                return;
            }

            // Then update git stuff
            // We've already confirmed the workingDir exists, so...

            if (prefs.desiredBranch.ToLower() == "nightly")
            {
                // We want the Nightly branch
                System.Diagnostics.Debug.WriteLine($"CheckRepoForPatch: Nightly branch downloading...");
                await Task.Run(() => Repository.Clone(dinoPatchURL, workingDir, new CloneOptions { BranchName = "nightly" }));
                System.Diagnostics.Debug.WriteLine($"CheckRepoForPatch: Nightly branch downloaded!");
            }
            else
            {
                // We want the Stable branch
                System.Diagnostics.Debug.WriteLine($"CheckRepoForPatch: Stable branch downloading...");
                await Task.Run(() => Repository.Clone(dinoPatchURL, workingDir, new CloneOptions { BranchName = "stable" }));
                System.Diagnostics.Debug.WriteLine($"CheckRepoForPatch: Stable branch downloaded!");
            }

        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("CheckForRepoPatch: " + e);
        }
    }

    // Perhaps we should be putting together a progress bar for this?
    // Yes.

}