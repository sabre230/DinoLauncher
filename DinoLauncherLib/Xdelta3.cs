using System;
using System.Diagnostics;

namespace DinoLauncherLib;

public static class Xdelta3 //: IDinoLauncher
{
    public static void ApplyPatch(FileIO f, string fBaseRomPath, string fPatchPath, string fPatchedRomPath)
    {
        try
        {
            // This can't be right...
            f.SetupFileStructure();

            // Point to 'xdelta3.exe' in the dumbest way possible
            string exeDir = f.currentDirectory + f.xdeltaPath;
            // Specify arguments for procStartInfo()
            string args = $" -d -s {fBaseRomPath} {fPatchPath} {fPatchedRomPath}";

            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo();

                procStartInfo.FileName = exeDir;
                procStartInfo.Arguments = args;

                // Supposed to output executable output to something readable but it doesn't seem to work that way
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Don't bother opening a window for this
                procStartInfo.CreateNoWindow = true;

                // Wait for the process to complete before exiting
                using (Process process = new Process())
                {
                    process.StartInfo = procStartInfo;
                    process.Start();

                    process.WaitForExit();

                    Debug.WriteLine(process.StandardOutput.ReadToEnd());
                    Debug.WriteLine(exeDir + args);
                }
                //return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** Error occured executing the following commands.");
                Debug.WriteLine(exeDir);
                Debug.WriteLine(args);
                Debug.WriteLine(ex.Message);
                //return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

        }
        return;
    }
}