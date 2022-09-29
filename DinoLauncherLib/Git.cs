using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace DinoLauncherLib;

public static class Git
{
    private const string dinoPatchURL = "https://github.com/sabre230/DinoPatchRepo.git";
	public static readonly Repository repo = new Repository(dinoPatchURL);

	public static async Task CheckRepoForPatch(UserPrefs prefs, FileIO fileIO)
    {
        string workingDir = $"{ fileIO.baseDir }\\_PatchData\\git";

		foreach (var item in Repository.ListRemoteReferences(dinoPatchURL).ToList())
		{
			Debug.WriteLine("ListRemoteReferences: " + item);
		}

		try
        {
            // First check if the working directory exists
            if (Directory.Exists(workingDir))
            {
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Found working path: {workingDir}");

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
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Did not find working path at: {workingDir}!");
                // Kill the method
                return;
            }

            // Then update git stuff
            // We've already confirmed the workingDir exists, so...
            if (prefs.desiredBranch.ToLower() == "nightly")
            {
                // We want the Nightly branch
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Nightly branch downloading...");
                await Task.Run(() => Repository.Clone(dinoPatchURL, workingDir, new CloneOptions { BranchName = "nightly", Checkout = true, OnCheckoutProgress = CheckoutProgress() }));
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Nightly branch downloaded!");
            }
            else
            {
                // We want the Stable branch
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Stable branch downloading...");
                await Task.Run(() => Repository.Clone(dinoPatchURL, workingDir, new CloneOptions { BranchName = "stable" }));
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Stable branch downloaded!");
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("Git.CheckForRepoPatch: " + e);
        }
    }

	public static CheckoutProgressHandler CheckoutProgress()
	{
		Debug.WriteLine("Git.CheckoutProgress");
		return null;
	}

	// Perhaps we should be putting together a progress bar for this?
	// Yes.

	// We are stealing
	/// <summary>
	/// Git Pulls patching content from the repository.
	/// </summary>
	public static void PullPatchData(Func<TransferProgress, bool> transferProgressHandlerMethod)
	{
        //// Throw if we neither have a master nor main branch
        // We will not be allowing pulling from main/master
        // Instead we are pulling from stable/nightly

        // Can't seem to find our repo?
        // Throws a RepositoryNotFoundException for some reason :/
        // Works fine in the other methods, but why not here?

        List<Branch> branches = repo.Branches.ToList();
        foreach (var item in branches)
        {
            Debug.WriteLine($"{item.FriendlyName}, {item.CanonicalName}, {item.Commits}");
        }

        Branch originMaster = repo.Branches.FirstOrDefault(b => b.FriendlyName.Contains("stable") || b.FriendlyName.Contains("nightly"));
        if (originMaster == null)
            throw new UserCancelledException("Neither branch 'stable' nor branch 'nightly' could be found! Corrupted or invalid git repo?");

        // Permanently undo commits not pushed to remote
        repo.Reset(ResetMode.Hard);

		// Credential information to fetch
		PullOptions options = new PullOptions
		{
			FetchOptions = new FetchOptions { OnTransferProgress = tp => transferProgressHandlerMethod(tp) }
		};

		// Create dummy user information to create a merge commit
		Signature signature = new Signature("null", "null", DateTimeOffset.Now);

		// Pull
		try
		{
			Commands.Pull(repo, signature, options);
		}
		catch
		{
			Debug.WriteLine("Repository pull attempt failed!");
			return;
		}
		Debug.WriteLine("Repository pulled successfully.");
	}
};