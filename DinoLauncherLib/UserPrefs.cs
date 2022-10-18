using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
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
    // Am I doing this correctly?
    [JsonProperty("UpdateBranch")]
    public string UpdateBranch { get; set; }
    [JsonProperty("OriginalRomPath")]
    public string OriginalRomPath { get; set; }
    [JsonProperty("PatchedRomPath")]
    public string PatchedRomPath { get; set; }
    [JsonProperty("UseHQModels")]
    public bool UseHQModels { get; set; }

    public readonly string configFile = "config.json";

    readonly static JsonSerializer serializer = new JsonSerializer();

    #region Setup and Prepare
    public async void Setup()
    {
        // Check if the config file already exists
        if (!File.Exists(configFile))
        {
            Task.Run(async () => { await CreateJSONTask(); }).Wait();
        }

        await LoadJSON();

        // Just to confirm we are loading our JSON data correctly
        DebugJSON();
    }

    public void PrepareJSON()
    {
        // Maybe handle our reader stuff here?
        //StreamReader reader = new StreamReader(configFile);

        // Hook up all the JSON stuff
        //var json = reader.ReadToEnd();
        //JsonTextReader jreader = new JsonTextReader(new StringReader(json));
        //JObject jObject = JObject.Parse(configFile);
    }
    #endregion

    #region Create
    public async Task CreateJSON()
    {
        await CreateJSONTask();
    }

    public Task<string> CreateJSONTask()
    {
        ////Create a new JSON object with these tokens
        JObject configFile = new JObject(
            new JProperty("UpdateBranch", UpdateBranch),
            new JProperty("OriginalRomPath", OriginalRomPath),
            new JProperty("PatchedRomPath", PatchedRomPath),
            new JProperty("UseHQModels", UseHQModels)
            );

        File.WriteAllText(Path.Combine("res", "config.json"), configFile.ToString());

        // Lol just copy the resource for now
        // No need to overcomplicate; time is running out
        File.Copy(Path.Combine("res", "config.json"), "config.json"); // Just put it in the root folder for now, fix later

        return Task.FromResult<string>(null); // Magic
    }
    #endregion

    #region Save
    public async Task SaveJSON()
    {
        // We need to update our JSON config file with the most recent settings
        await SaveJSONTask(this);
    }

    public Task<string> SaveJSONTask(UserPrefs prefs)
    {
        Debug.WriteLine("JSON.SaveJSON: Saving JSON...");
        serializer.NullValueHandling = NullValueHandling.Ignore;

        JSON.Serialize(this, prefs);
        return Task.FromResult<string>(null); // Magic
    }
    #endregion

    #region Load
    public async Task LoadJSON()
    {
        await LoadJSONTask();
    }

    // This might not be necessary, we will see
    public Task<string> LoadJSONTask()
    {
        try
        {
            Debug.WriteLine($"JSON: Loading...");
            serializer.NullValueHandling = NullValueHandling.Ignore;
            JSON json = new JSON().FromJson(File.ReadAllText(configFile));

            UpdateBranch = json.UpdateBranch;
            OriginalRomPath = json.OriginalRomPath;
            PatchedRomPath = json.PatchedRomPath;
            UseHQModels = json.UseHQModels;

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
        Debug.WriteLine($"UserPrefs.UpdateBranch = {UpdateBranch}");
        Debug.WriteLine($"UserPrefs.OriginalRomPath = {OriginalRomPath}");
        Debug.WriteLine($"UserPrefs.PatchedRomPath = {PatchedRomPath}");
        Debug.WriteLine($"UserPrefs.UseHQModels = {UseHQModels}");
    }
    #endregion


}

public class JSON
{
    // Am I doing this correctly?
    [JsonProperty("UpdateBranch")]
    public string UpdateBranch { get; set; }
    [JsonProperty("OriginalRomPath")]
    public string OriginalRomPath { get; set; }
    [JsonProperty("PatchedRomPath")]
    public string PatchedRomPath { get; set; }
    [JsonProperty("UseHQModels")]
    public bool UseHQModels { get; set; }

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
