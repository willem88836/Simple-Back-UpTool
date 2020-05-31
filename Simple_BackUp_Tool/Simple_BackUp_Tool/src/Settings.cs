using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
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
			throw new Exception("Duplicate Settings Singleton");
		}

		singleton = this;
	}

	public BackUpSettings Load()
	{
		if (!Directory.Exists(SettingsPath))
		{
			Directory.CreateDirectory(SettingsPath);
		}

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
		loaded = JsonUtility.FromJson<BackUpSettings>(json);
		LoggingUtilities.LogFormat("Loaded Settings: ({0})\n", json);
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
			if (field.FieldType.IsArray)
			{
				// TODO: Make this more generic.
				List<string> elements = new List<string>();
				while(true)
				{
					Console.WriteLine(field.Name + " element: ");
					string input = Console.ReadLine();
					if (input == "x")
					{
						break;
					}
					object element = Convert.ChangeType(input, field.FieldType.GetElementType());
					elements.Add(input);

				}
				field.SetValue(settings, elements.ToArray());
			}
			else
			{
				Console.WriteLine(field.Name + ": ");
				object input = null;
				if (field.FieldType.IsEnum)
				{
					input = Enum.Parse(field.FieldType, Console.ReadLine());
				}
				else
				{
					input = Convert.ChangeType(Console.ReadLine(), field.FieldType);
				}

				field.SetValue(settings, input);
			}
		}

		string json = JsonUtility.ToJson(settings);

		Console.WriteLine("File Name: ");
		string fileName = Console.ReadLine();
		fileName = Path.Combine(SettingsPath, fileName);
		fileName = Path.ChangeExtension(fileName, "json");

		File.WriteAllText(fileName, json);

		return fileName;
	}
}