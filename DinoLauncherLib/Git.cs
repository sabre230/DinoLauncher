using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    private const string remoteRepo = "https://github.com/sabre230/DinoPatchRepo.git";
    public static readonly Repository localRepo = new Repository(@"\PatchData\Git");

    public static async Task CheckRepoForPatch(UserPrefs prefs, FileIO fileIO)
    {
        string workingDir = $"{fileIO.baseDir}\\_PatchData\\git";

        
        //foreach (var item in Repository.ListRemoteReferences(remoteRepo).ToList())
		//{
            // List all remote references (branches, heads, etc.)
		//	Debug.WriteLine("ListRemoteReferences: " + item);
		//}

		try
        {
            // First check if the working directory exists
            // It should be made as soon as the application starts, but redundancy isn't a bad thing here
            if (Directory.Exists(workingDir))
            {
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Found working path: {workingDir}");
            }
            else
            {
                // We shouldn't ever get this far, but just in case
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Did not find working path at: {workingDir}!");
                return;
            }


            // We've already confirmed the workingDir exists, so...
            if (prefs.desiredBranch.ToLower() == "nightly")
            {
                // We want the Nightly branch
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Nightly branch downloading...");
                await Task.Run(() => Repository.Clone(remoteRepo, workingDir, new CloneOptions { BranchName = "nightly", OnCheckoutProgress = CheckoutProgress()})); // Possibly need Checkout
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Nightly branch downloaded!");
            }
            else
            {
                // We want the Stable branch by default
                System.Diagnostics.Debug.WriteLine($"Git.CheckRepoForPatch: Stable branch downloading...");
                await Task.Run(() => Repository.Clone(remoteRepo, workingDir, new CloneOptions { BranchName = "stable" }));
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

	// We are stealing this
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