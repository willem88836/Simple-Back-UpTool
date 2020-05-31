using System;
using System.IO;
using Framework.Features.Json;
using Framework.Utils;

public class Settings
{
	private static readonly string defaultSettingsName = "settings.json";
	public static string SettingsPath { get { return Path.Combine(Program.AppDataRoot, "settings"); } }

	private static Settings singleton;
	private static BackUpSettings loaded;


	public Settings()
	{
		if (singleton != null)
		{
			throw new Exception("Duplicate Singleton");
		}

		singleton = this;
	}


	public BackUpSettings Load()
	{
		string[] files = Directory.GetFiles(SettingsPath);

		string settingsPath = "";

		// Creates new settings or initializes selection sreen
		// based on whether settings exist. 
		if (files == null || files.Length == 0)
		{
			LoggingUtilities.Log("No prior settings found, creating new settings");
			settingsPath = CreateNewSettings();
		}
		else
		{
			Console.WriteLine("Select Settings: ");
			for(int i = 0; i < files.Length; i++)
			{
				Console.WriteLine(i + ") " + Path.GetFileNameWithoutExtension(files[i]));
			}
			Console.WriteLine("n) create new settings");

			ConsoleKeyInfo input = Console.ReadKey();

			// Gives user the option to create new settings.
			if (input.Key == ConsoleKey.R)
			{
				LoggingUtilities.Log("User wants to create new settings.");
				settingsPath = CreateNewSettings();
			}
			else
			{
				string c = input.KeyChar.ToString();
				int i = 0;
				if (int.TryParse(c, out i))
				{
					settingsPath = files[i];
				}
				else
				{
					throw new Exception("Unexpected input");
				}
			}
		}

		// loads selected settings.
		string json = File.ReadAllText(settingsPath);
		LoggingUtilities.LogFormat("Loaded Settings: ({0})\n", json);
		loaded = JsonUtility.FromJson<BackUpSettings>(json);
		return loaded;
	}

	/// <summary>
	///		Creates new settings, returns settings path.
	/// </summary>
	private string CreateSettings()
	{
		//TODO create JSONBuilder from Console.
		throw new NotImplementedException();
	}


	public static void Store()
	{
		string newJson = JsonUtility.ToJson(Instance);
		File.WriteAllText(SettingsPath, newJson);
	}
}