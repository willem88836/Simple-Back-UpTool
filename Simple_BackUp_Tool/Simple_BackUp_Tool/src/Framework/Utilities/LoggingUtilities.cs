using System;
using System.IO;

namespace Framework.Utils
{
	public static class LoggingUtilities
	{
		private static string appdataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		public static void SetAppDataRoot(string root)
		{
			LoggingUtilities.appdataRoot = root;
		}

		/// <summary>
		///		Logs a message into the 
		///		UnityEngine.Debug.Log(),
		///		System.Diagnostics.Debug.Writeline(), 
		///		System.Console.Writeline()
		/// </summary>
		public static void Log(string message)
		{
#if UNITY_EDITOR
			UnityEngine.Debug.Log(message);
#endif

			string logPath = Path.Combine(appdataRoot, "log.txt");
			if (!File.Exists(logPath))
			{
				if (!Directory.Exists(appdataRoot))
				{
					Directory.CreateDirectory(appdataRoot);
				}
				FileStream stream = File.Create(logPath);
				stream.Close();
			}

			File.AppendAllText(logPath, message);

#if false
			System.Diagnostics.Debug.WriteLine(message);
			System.Console.WriteLine(message);
#endif
		}

		/// <summary>
		///		Logs a message into the 
		///		UnityEngine.Debug.Log(),
		///		System.Diagnostics.Debug.Writeline(), 
		///		System.Console.Writeline()
		/// </summary>
		public static void Log(object obj)
		{
			Log(obj.ToString());
		}

		/// <summary>
		///		Logs a message into the 
		///		UnityEngine.Debug.Log(),
		///		System.Diagnostics.Debug.Writeline(), 
		///		System.Console.Writeline()
		/// </summary>
		public static void LogFormat(string message, params object[] obj)
		{
			string msg = string.Format(message, obj);
			Log(msg);
		}
	}
}
