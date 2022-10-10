using System;
using System.Diagnostics;
using System.IO;

namespace DinoLauncherLib;

public static class Xdelta3
{
    public static void ApplyPatch(FileIO fio, string fBaseRomPath, string fPatchPath, string fPatchedRomPath)
    {
        try
        {
            //fio.SetupFileStructure();

            // Point to 'xdelta3.exe' in the dumbest way possible
            string exeDir = Path.Combine(fio.baseDir, fio.xdeltaPath);

            // Specify arguments for procStartInfo()
            string args = $" -d -s {fBaseRomPath} {fPatchPath} {fPatchedRomPath}";

            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo
                {
                    FileName = exeDir,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    // Don't bother opening a window for this
                    CreateNoWindow = true,
                    WorkingDirectory = Path.Combine(fio.baseDir)
                };

                // Wait for the process to complete before exiting
                using Process process = new Process();
                process.StartInfo = procStartInfo;
                process.Start();

                process.WaitForExit();

                Debug.WriteLine(process.StandardOutput.ReadToEnd());
                Debug.WriteLine(exeDir + args);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*** Error occured executing the following commands.");
                Debug.WriteLine("exeDir: " + exeDir);
                Debug.WriteLine("args: " + args);
                Debug.WriteLine("exception: " + ex.Message);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

        }
    }
}