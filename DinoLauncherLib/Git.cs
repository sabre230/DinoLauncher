using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using static System.Net.WebRequestMethods;

namespace DinoLauncherLib;

public static class Git
{
    // Let's do a create a fresh outline
    //
    // [ ] Try to Fetch the selected online repo
    // [ ] Incorporate fallback repos, just in case
    // [ ] After Fetching, pull the latest commit
    // [ ] Show a progress bar for progress

    // In case of a branch change, we need to checkout
    // In case of a new commit, we need to fetch and then pull!
    // We will do NO pushing; this is a read-only operation

    // Primary remote repository
    private const string remoteRepo = "https://github.com/sabre230/DinoPatchRepo.git";
    private static string[] backupRemoteRepos = { "Put", "Backup", "Git", "Repos", "Here" };

    // The local repo should be using relative paths, use Path.Combine to account for multi-platform
    public static readonly Repository localRepo = new Repository(Path.Combine("_PatchData", "git"));



    public static async Task CheckRepoForPatch(UserPrefs prefs, FileIO fileIO)
    {
        // Use Path.Combine to account for other operating systems instead of simply writing out the whole path
        //                              "fileIO.baseDir/_PatchData/git"
        string workingDir = Path.Combine(fileIO.baseDir, "_PatchData", "git");

        #region ListRemoteReferences
        // Get available references from git
        //foreach (var item in Repository.ListRemoteReferences(remoteRepo).ToList())
        //{
        // List all remote references (branches, heads, etc.)
        //	Debug.WriteLine("ListRemoteReferences: " + item);
        //}
        #endregion

        // First check if the working directory exists
        try
        {
            // It should be made as soon as the application starts, but reduncancy doesn't hurt here
            if (Directory.Exists(workingDir))
            {
                Debug.WriteLine($"Git.CheckRepoForPatch: Found working path: {workingDir}");
            }
            else
            {
                // We shouldn't ever get this far, but just in case
                Debug.WriteLine($"Git.CheckRepoForPatch: Did not find working path at: {workingDir}!");
                // Kill the thread
                return;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Git.CheckForRepoPatch: " + e);
        }

        // Then see if we've already cloned previously
        try
        {
            if (Directory.Exists(Path.Combine(workingDir, ".git")))
            {
                Debug.WriteLine("Git.CheckRepoForPatch: .git folder exists, previous patch has been downloaded");
            }
            else
            {
                // We should have a good working directory
                Debug.WriteLine("Git.CheckRepoForPatch: .git folder is missing, this is a fresh install!");
                // And there's no .git folder yet, so we will need a fresh clone
                await CloneRemoteRepo(prefs, workingDir);
                // And wait until the clone is done to continue
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Git.CheckRepoForPatch: ERROR: {e}");
        }

        // Now, check if there's a patch already ready to go
        bool patchExists = fileIO.GetLocalPatchPath();
        if (!patchExists)
        {
            Debug.WriteLine("Git.CheckRepoForPatch: No patch exists, did cloning fail?");
        }
    }

	public static CheckoutProgressHandler CheckoutProgress()
	{
        // I will learn this eventually!
		Debug.WriteLine("Git.CheckoutProgress");
		return null;
	}

    //public static void CloneRemoteRepo(Func<TransferProgress, bool>, UserPrefs prefs)
    public static async Task CloneRemoteRepo(UserPrefs prefs, string localRepo)
    {
        // We've already confirmed the workingDir exists, so...
        // We clone the repo to a local path and will use that from now on
        if (prefs.desiredBranch.ToLower() == "nightly")
        {
            // We want the Nightly branch
            await CloneRemoteRepoAsync(localRepo, "nightly");
        }
        else
        {
            // We want the Stable branch (Default)
            await CloneRemoteRepoAsync(localRepo, "stable");
        }
    }

    public static async Task CloneRemoteRepoAsync(string localRepo, string branch)
    {
        await Task.Run(() => Repository.Clone(remoteRepo, localRepo, new CloneOptions { BranchName = branch, OnCheckoutProgress = CheckoutProgress() })); // Possibly need Checkout
    }

	// We are stealing this
	/// <summary>
	/// Git Pulls patching content from the repository.
	/// </summary>
	public static void PullPatchData(Func<TransferProgress, bool> transferProgressHandlerMethod)
	{
        // We will not be allowing pulling from main/master
        // Instead we are pulling from stable/nightly

        try
        {
            List<Branch> branches = localRepo.Branches.ToList();
            foreach (var item in branches)
            {
                Debug.WriteLine($"Git.PullPatchData: {item.FriendlyName}, {item.CanonicalName}, {item.Commits}");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Git.PullPatchData: {e}");
        }


        Branch originMaster = localRepo.Branches.FirstOrDefault(b => b.FriendlyName.Contains("stable") || b.FriendlyName.Contains("nightly"));
        if (originMaster == null)
            throw new UserCancelledException("Stable/Nightly branches not found! Corrupted or invalid git repo?");

        // Permanently undo commits not pushed to remote
        localRepo.Reset(ResetMode.Hard);

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
			Commands.Pull(localRepo, signature, options);
		}
		catch (Exception e)
		{
			Debug.WriteLine($"Git.PullPatchData: ERROR {e}");
			return;
		}
		Debug.WriteLine($"Git.PullPatchData: Repository pulled successfully.");
	}
};