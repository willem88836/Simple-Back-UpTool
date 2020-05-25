using System.IO;
using Framework.Features.Json;
using Framework.Utils;

namespace ConsoleApp2.src
{
	public class Settings
	{
		[JsonIgnore] public static Settings Instance;
		[JsonIgnore] public static readonly string SettingsName = "settings.json";
		[JsonIgnore] public static string SettingsPath { get { return Path.Combine(Program.AppDataRoot, SettingsName); } }

		public string TargetDirectory = "";
		public string[] OriginDirectories = new string[0];
		public bool UseShortNames = false;
		public BackUp.OverwriteState DefaultOverwriteState = BackUp.OverwriteState.Unset; 


		public static void Load()
		{
			// TODO: Make some kind of prompt to select settings with. 
			string settingsFilePath = SettingsPath;

			if (!File.Exists(settingsFilePath))
			{
				Instance = new Settings();
				string newJson = JsonUtility.ToJson(Instance);
				File.WriteAllText(settingsFilePath, newJson);
				return;
			}

			string json = File.ReadAllText(settingsFilePath);
			LoggingUtilities.LogFormat("Loaded Settings: ({0})\n", json);
			Instance = JsonUtility.FromJson<Settings>(json);
		}

		public static void Store()
		{
			string newJson = JsonUtility.ToJson(Instance);
			File.WriteAllText(SettingsPath, newJson);
		}
	}
}
