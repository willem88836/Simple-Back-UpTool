using System;
using Framework.Utils;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.CompilerServices;

public class Program
{
	private static Program program;
	private Settings settings;
	private BackUp backUp;



	public static string AppDataRoot { get { 
			return Path.Combine(
				Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData), 
					"Simple_Back_Up"); 
		} 
	}

	static void Main(string[] args)
	{
		program = new Program();
	}


	public Program()
	{
		LoggingUtilities.SetAppDataRoot(AppDataRoot);
		try
		{
			LoggingUtilities.LogFormat("\nInitiating AFB ({0})\n", DateTime.Now.ToString());

			settings = new Settings();
			BackUpSettings loadedSettings = settings.Load();

			backUp = new BackUp();
			backUp.Start(loadedSettings);
		}
		catch (Exception e)
		{
			LoggingUtilities.Log("ERROR\n");
			LoggingUtilities.Log(e.StackTrace + "\n");
			LoggingUtilities.Log(e.Message + "\n");
			LoggingUtilities.Log(e.Data.ToString() + "\n");

			Console.Beep(200, 500);
			Console.Beep(200, 500);
			Console.Beep(200, 500);
		}

		Console.Beep(400, 500);
		LoggingUtilities.LogFormat("Finished AFB ({0})\n", DateTime.Now.ToString());
	}



	public static string RequestInput(string request)
	{
		Console.Clear();
		Console.WriteLine(request);
		string input = Console.ReadLine();
		return input.ToLower();
	}

	public static string RequestInput(string format, params object[] args)
	{
		return RequestInput(string.Format(format, args));
	}
}
