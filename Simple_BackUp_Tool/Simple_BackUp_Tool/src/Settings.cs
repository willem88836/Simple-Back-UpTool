using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Framework.Features.Json;
using Framework.Utils;

public class Settings
{
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
			settingsPath = CreateSettings();
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
				settingsPath = CreateSettings();
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
		Console.WriteLine("Fill in Settings: ");

		BackUpSettings settings = new BackUpSettings();

		FieldInfo[] fields = typeof(BackUpSettings).GetFields().Where(info => info.GetCustomAttribute(typeof(JsonIgnore)) == null && !info.IsLiteral).ToArray();
		foreach (FieldInfo field in fields)
		{
			Console.WriteLine(field.Name + ": ");
			object input = Convert.ChangeType(Console.ReadLine(), field.DeclaringType);
			field.SetValue(settings, input);
		}

		string json = JsonUtility.ToJson(settings);

		Console.WriteLine("File Name: ");
		string fileName = Console.ReadLine();
		fileName = Path.Combine(SettingsPath, fileName);

		File.WriteAllText(fileName, json);

		return fileName;
	}
}