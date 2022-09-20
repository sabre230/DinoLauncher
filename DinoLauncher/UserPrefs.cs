using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace DinoLauncher;

public class UserPrefs //: FileIO
{
    // Ayy yo handle save data dawg don't @ me
    public string desiredBranch;
    public string baseRomPath;
    public string patchedRomPath;
    public bool useHQModels;

    public string configPath = @"config.json";

    public void Setup()
    {
        // Check if the config file already exists
        if (!File.Exists(configPath))
        {
            //Console.WriteLine($"No config.json found, creating a new one...");
            Task.Run(async () => { await CreateJSONTask(); }).Wait();
        }

        // Load up and parse the JSON file here
        using (StreamReader reader = new StreamReader(configPath))
        {
            // Hook up all the JSON BS
            string json = reader.ReadToEnd();
            JsonTextReader jreader = new JsonTextReader(new StringReader(json));
            JObject jObject = JObject.Parse(json);

            // Set our variables with information parsed from the JSON config file
            desiredBranch = (string)jObject.SelectToken("UpdateBranch"); // { stable, nightly, custom }
            baseRomPath = (string)jObject.SelectToken("OriginalRomPath");
            patchedRomPath = (string)jObject.SelectToken("PatchedRomPath");
            useHQModels = (bool)jObject.SelectToken("useHQModels");
        }

        #region
        //// Might not need some of these but I like having them there for now
        //void LoadAllUserPreferences()
        //{

        //}

        //void LoadUserPreference(DataMemberAttribute dma)
        //{

        //}

        //void SaveAllUserPreferences()
        //{

        //}

        //void SaveUserPreference(DataMemberAttribute dma)
        //{

        //}
        #endregion
    }

    public static async Task CreateJSONTask()
    {
        // Let's make our JSON file
        System.Threading.Thread createFileThread = new System.Threading.Thread(delegate () { JSON.CreateJSON(); });
        createFileThread.Start();

        await JSON.CreateJSON();
    }
}

// Do the JSON BS
public class JSON
{

    public static Task CreateJSON()
    {
        // Create a .json file with these tokens 
        JObject configFile = new JObject(
            new JProperty("UpdateBranch", "stable"),                        // Default to stable
            new JProperty("OriginalRomPath", "PatchData\\rom_crack.z64"),   // Default to PatchData/rom_crack.z64
            new JProperty("PatchedRomPath", "Game\\dinosaurplanet.z64"),
            new JProperty("useHQModels", "false")                           // Default to false
            );

        File.WriteAllText("%appdata%\\config.json", configFile.ToString());
        return null;
    }
}



