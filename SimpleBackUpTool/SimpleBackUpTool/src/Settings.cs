using System;
using System.IO;
using SimpleJsonLibrary;
using Framework.Utils;

namespace SimpleBackUpTool
{
	public class Settings
	{
		public static string SettingsPath { get { return Path.Combine(BackUpProgram.AppDataRoot, "settings"); } }

		private static Settings singleton;
		public BackUpSettings LoadedSettings { get; private set; }

		public Settings()
		{
			if (singleton != null)
			{
				throw new Exception("Duplicate Settings Singleton");
			}

			singleton = this;


			if (!Directory.Exists(SettingsPath))
			{
				Directory.CreateDirectory(SettingsPath);
			}
		}

		
		public string[] UserSettings
		{
			get
			{
				return Directory.GetFiles(SettingsPath);
			}
		}


		public BackUpSettings Load(string settingsPath)
		{
			// loads selected settings.
			string json = File.ReadAllText(settingsPath);
			LoadedSettings = JsonUtility.FromJson<BackUpSettings>(json);
			LoggingUtilities.LogFormat("Loaded Settings: ({0})\n", json);
			return LoadedSettings;
		}

		public static string Save(BackUpSettings settings, string name)
		{
			string json = JsonUtility.ToJson(settings);

			name = Path.Combine(SettingsPath, name);
			name = Path.ChangeExtension(name, "json");

			File.WriteAllText(name, json);

			return name;
		}
	}
}
