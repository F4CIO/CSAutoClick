using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSImageClick
{
    partial class Program
    {
        private static void CreateIniFileIfNotExists()
        {
            string filePath = "CSImageClick.ini";
            if(!System.IO.File.Exists(filePath))
            {
                using(var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Enabled=true");
                    writer.WriteLine("CheckEveryXSeconds=5");
                    writer.WriteLine("PrecisionPercent=50");
                    writer.WriteLine("AllowedDisplacementInPixels=50");
                    writer.WriteLine("ShowDebugMarkers=false");
                    writer.WriteLine("DebugLogsEnabled=false");
                    writer.WriteLine("FlashTrayIconDuringScan=true");
                    writer.WriteLine("FlashTrayIconDuringAction=true");
                    writer.WriteLine("RestartEveryXMinutes=10080");
                }
            }
        }

        private static void LoadIniFile()
        {
            try
            {
                var config = ParseIniFile("CSImageClick.ini");

                if(config.ContainsKey("Enabled")) isEnabled = bool.Parse(config["Enabled"]);
                if(config.ContainsKey("CheckEveryXSeconds")) checkEveryXSeconds = int.Parse(config["CheckEveryXSeconds"]);
                if(config.ContainsKey("PrecisionPercent")) precisionPercent = int.Parse(config["PrecisionPercent"]);
                if(config.ContainsKey("AllowedDisplacementInPixels")) allowedDisplacementInPixels = int.Parse(config["AllowedDisplacementInPixels"]);
                if(config.ContainsKey("ShowDebugMarkers")) showDebugMarkers = bool.Parse(config["ShowDebugMarkers"]);
                if(config.ContainsKey("DebugLogsEnabled")) debugLogsEnabled = bool.Parse(config["DebugLogsEnabled"]);
                if(config.ContainsKey("FlashTrayIconDuringScan")) flashTrayIconDuringScan = bool.Parse(config["FlashTrayIconDuringScan"]);
                if(config.ContainsKey("FlashTrayIconDuringAction")) flashTrayIconDuringAction = bool.Parse(config["FlashTrayIconDuringAction"]);
                if(config.ContainsKey("RestartEveryXMinutes")) restartEveryXMinutes = int.Parse(config["RestartEveryXMinutes"]);
                Log("Configuration loaded successfully.");
            }
            catch(Exception ex)
            {
                LogError($"Failed to load configuration: {ex.Message} {ex.StackTrace}");
            }
        }

        private static Dictionary<string, string> ParseIniFile(string filePath)
        {
            var config = new Dictionary<string, string>();

            if(!System.IO.File.Exists(filePath))
                return config;

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach(var line in lines)
            {
                if(line == null || line.Trim().Length == 0 || line.StartsWith(";")) continue;

                var parts = line.Split(new[] { '=' }, 2);
                if(parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    config[key] = value;
                }
            }

            return config;
        }

        private static void UpdateIniFile(string key, string value)
        {
            var config = ParseIniFile("CSImageClick.ini");
            config[key] = value;

            using(var writer = new StreamWriter("CSImageClick.ini"))
            {
                foreach(var entry in config)
                {
                    writer.WriteLine($"{entry.Key}={entry.Value}");
                }
            }
            Log($"Updated config: {key} = {value}");
        }
    }
}
