using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DinoLauncherLib;

public class UserPrefs
{
    public string desiredBranch;
    public string baseRomPath;
    public string patchedRomPath;
    public bool useHQModels;

    public string configFile = "config.json";

    public void Setup()
    {
        // Check if the config file already exists
        if (!File.Exists(configFile))
        {
            Task.Run(async () => { await CreateJSONTask(); }).Wait();
        }

        // Load up and parse the JSON file here
        using StreamReader reader = new StreamReader(configFile);
        // Hook up all the JSON BS
        string json = reader.ReadToEnd();
        JsonTextReader jreader = new JsonTextReader(new StringReader(json));
        JObject jObject = JObject.Parse(json);

        // Set our variables with information parsed from the JSON config file
        desiredBranch = (string)jObject.SelectToken("UpdateBranch"); // { stable, nightly, custom }
        baseRomPath = (string)jObject.SelectToken("OriginalRomPath");
        patchedRomPath = (string)jObject.SelectToken("PatchedRomPath");
        useHQModels = (bool)jObject.SelectToken("useHQModels");

        // Just to confirm we are loading our JSON data correctly
        System.Diagnostics.Debug.WriteLine($"UserPrefs.desiredBranch = {desiredBranch}");
        System.Diagnostics.Debug.WriteLine($"UserPrefs.baseRomPath = {baseRomPath}");
        System.Diagnostics.Debug.WriteLine($"UserPrefs.patchedRomPath = {patchedRomPath}");
        System.Diagnostics.Debug.WriteLine($"UserPrefs.useHQModels = {useHQModels}");

        // We are done with our readers, close them please
        jreader.Close();
        reader.Close();
    }

    public void CreateJSON()
    {
        CreateJSONTask();
    }
    
    // There's a better way surely but I can't keep staring a this
    public void SaveJSON(UserPrefs prefs)
    {
        SaveJSONTask(prefs);
    }

    public static async Task CreateJSONTask()
    {
        // Let's make our JSON file
        //System.Threading.Thread createFileThread = new System.Threading.Thread(delegate () { JSON.CreateJSON(); });
        //createFileThread.Start();

        await JSON.CreateJSON();
    }

    public static async Task SaveJSONTask(UserPrefs prefs)
    {
        // We need to update our JSON config file with the most recent settings
        await JSON.SaveJSON(prefs);
    }
}

// Do the JSON BS
public class JSON
{
    public static Task<string> CreateJSON()
    {
        System.Diagnostics.Debug.WriteLine("JSON.SaveJSON: Creating new JSON...");
        // Create a .json file with these tokens 
        JObject configFile = new JObject(
            new JProperty("UpdateBranch", "stable"),                        // Default to stable
            new JProperty("OriginalRomPath", "_PatchData\\rom_crack.z64"),   // Default to PatchData/rom_crack.z64
            new JProperty("PatchedRomPath", "_Game\\dinosaurplanet.z64"),
            new JProperty("useHQModels", "false")                           // Default to false
            );

        File.WriteAllText("config.json", configFile.ToString());
        return Task.FromResult<string>(null); // Magic
    }

        public static Task<string> SaveJSON(UserPrefs prefs)
    {
        System.Diagnostics.Debug.WriteLine("JSON.SaveJSON: Saving JSON...");
        // Create a new JSON object with these tokens 
        JObject configFile = new JObject(
            new JProperty("UpdateBranch".ToUpperInvariant(), prefs.desiredBranch),
            new JProperty("OriginalRomPath", prefs.baseRomPath),
            new JProperty("PatchedRomPath", prefs.patchedRomPath),
            new JProperty("useHQModels", prefs.useHQModels)
            );

        File.WriteAllText("config.json", configFile.ToString());
        return Task.FromResult<string>(null); // Magic
    }
}