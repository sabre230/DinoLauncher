using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DinoLauncherLib;

public static class Xdelta3
{
    public static Task<string> ApplyPatch(FileIO fio, string fBaseRomPath, string fPatchPath, string fPatchedRomPath)
    {
        try
        {            
            // Point to 'xdelta3.exe'
            string exeDir = Path.Combine(fio.baseDir, fio.xdeltaPath);

            // Specify arguments for procStartInfo()
            // Added some escaped quotes to get around the spaces in paths issue
            string args = $"-f -d -s \"{fBaseRomPath}\" \"{fPatchPath}\" \"{fPatchedRomPath}\"";

            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo
                {
                    // Trying something weird
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? exeDir : "xdelta3",
                    //FileName = exeDir,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
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

        return Task.FromResult<string>(null);
    }
}