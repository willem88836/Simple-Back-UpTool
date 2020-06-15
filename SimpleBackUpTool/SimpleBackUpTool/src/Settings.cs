﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
}