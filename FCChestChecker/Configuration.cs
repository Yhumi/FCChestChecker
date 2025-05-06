using Dalamud.Configuration;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace FCChestChecker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string APIUrl { get; set; } = string.Empty;
    public string APIKey { get; set; } = string.Empty;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }

    public static Configuration Load()
    {
        try
        {
            var contents = File.ReadAllText(Svc.PluginInterface.ConfigFile.FullName);
            var json = JObject.Parse(contents);
            var version = (int?)json["Version"] ?? 0;
            return json.ToObject<Configuration>() ?? new();
        }
        catch (Exception e)
        {
            Svc.Log.Error($"Failed to load config from {Svc.PluginInterface.ConfigFile.FullName}: {e}");
            return new();
        }
    }
}
