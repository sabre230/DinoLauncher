using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DinoLauncherLib;

// Merging the JSON class with UserPrefs
// Having a hard time passing objects back and forth in a cleanly fashion
public class UserPrefs
{
    public string UpdateBranch;
    public string OriginalRomPath;
    public string PatchedRomPath;
    public bool useHQModels;

    public string configFile = "config.json";

    //readonly static JObject jObj = new JObject(
    //new JProperty("UpdateBranch", ""),
    //new JProperty("OriginalRomPath", ""),
    //new JProperty("PatchedRomPath", ""),
    //new JProperty("useHQModels", ""));

    readonly static JsonSerializer serializer = new JsonSerializer();

    #region Setup and Prepare
    public void Setup()
    {
        // Check if the config file already exists
        if (!File.Exists(configFile))
        {
            Task.Run(async () => { await CreateJSONTask(); }).Wait();
        }

        PrepareJSON();
        // Just to confirm we are loading our JSON data correctly
        DebugJSON();
    }

    public void PrepareJSON()
    {
        // Maybe handle our reader stuff here?
        //StreamReader reader = new StreamReader(configFile);

        //JSON.Deserialize(configFile);
        DebugJSON();
        //JSON.Serialize(jObj, this);
        DebugJSON();

        // Hook up all the JSON stuff
        //var json = reader.ReadToEnd();
        //JsonTextReader jreader = new JsonTextReader(new StringReader(json));
        //JObject jObject = JObject.Parse(configFile);
    }
    #endregion

    #region Create
    public void CreateJSON(UserPrefs prefs)
    {
        // Not concerned about actually running async yet
        CreateJSONTask();
    }

    public static async Task CreateJSONTask()
    {
        await CreateJSON();
    }

    public static Task<string> CreateJSON()
    {
        // Create a new JSON object with these tokens 
        //JObject configFile = new JObject(
        //    new JProperty("UpdateBranch", desiredBranch.ToLower()),
        //    new JProperty("OriginalRomPath", baseRomPath),
        //    new JProperty("PatchedRomPath", patchedRomPath),
        //    new JProperty("UseHQModels", useHQModels)
        //    );

        // Lol just copy the resource for now
        // No need to overcomplicate; time is running out
        File.Copy(Path.Combine("res", "config.json"), "config.json"); // Just put it in the root folder for now, fix later

        return Task.FromResult<string>(null); // Magic
    }
    #endregion

    #region Save
    // There's a better way surely but I can't keep staring a this
    public void Save()
    {
        // Not concerned about actually running async
        SaveJSONTask();
    }

    public static async Task SaveJSONTask()
    {
        // We need to update our JSON config file with the most recent settings
        await SaveJSON();
    }

    public static Task<string> SaveJSON()
    {
        Debug.WriteLine("JSON.SaveJSON: Saving JSON...");
        serializer.NullValueHandling = NullValueHandling.Ignore;

        // Create a new JSON object with these tokens 
        //JObject configFile = new JObject(
        //    new JProperty("UpdateBranch", prefs.desiredBranch.ToLower()),
        //    new JProperty("OriginalRomPath", prefs.baseRomPath),
        //    new JProperty("PatchedRomPath", prefs.patchedRomPath),
        //    new JProperty("useHQModels", prefs.useHQModels)
        //    );

        //File.WriteAllText("config.json", jObj.ToString());



        return Task.FromResult<string>(null); // Magic

    }
    #endregion

    #region Load
    public async Task LoadJSON(string path)
    {
        await LoadJSONTask(path);
    }

    // This might not be necessary, we will see
    public Task<string> LoadJSONTask(string path)
    {
        try
        {
            Debug.WriteLine($"JSON: Loading...");

            JSON.Deserialize("config.json");
            JSON config = new JSON().FromJson(File.ReadAllText(path));

            // Set our variables with information parsed from the JSON config file
            // I think I'm doing this wrong
            UpdateBranch = config.UpdateBranch;
            OriginalRomPath = config.OriginalRomPath;
            PatchedRomPath = config.PatchedRomPath;
            useHQModels = config.useHQModels;


            Debug.WriteLine($"JSON: Done loading.");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"JSON.Load: ERROR: {e}");
        }

        return Task.FromResult<string>(null); // Magic
    }
    #endregion

    #region Debug
    public void DebugJSON()
    {
        // Putting this all in its own method to keep things organized
        Debug.WriteLine($"UserPrefs.desiredBranch = {UpdateBranch}");
        Debug.WriteLine($"UserPrefs.baseRomPath = {OriginalRomPath}");
        Debug.WriteLine($"UserPrefs.patchedRomPath = {PatchedRomPath}");
        Debug.WriteLine($"UserPrefs.useHQModels = {useHQModels}");
    }
    #endregion


}

public class JSON
{
    [JsonProperty("UpdateBranch")]
    public string UpdateBranch { get; set; }
    [JsonProperty("OriginalRomPath")]
    public string OriginalRomPath { get; set; }
    [JsonProperty("PatchedRomPath")]
    public string PatchedRomPath { get; set; }
    [JsonProperty("UseHQModels")]
    public bool useHQModels { get; set; }

    public JSON FromJson(string json) => JsonConvert.DeserializeObject<JSON>(json, s);

    private static readonly JsonSerializerSettings s = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    public static void Serialize(object obj, UserPrefs prefs)
    {
        var serializer = new JsonSerializer();

        using (var sw = new StreamWriter(prefs.configFile))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            serializer.Serialize(writer, obj);
        }
    }

    public static object Deserialize(string path)
    {
        var serializer = new JsonSerializer();

        using (var sw = new StreamReader(path))
        using (var reader = new JsonTextReader(sw))
        {
            return serializer.Deserialize(reader);
        }
    }
}
