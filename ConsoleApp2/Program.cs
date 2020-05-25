using System;
using ConsoleApp2.src;
using Framework.Utils;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace ConsoleApp2
{
	class Program
	{
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		const int SW_HIDE = 0;
		const int SW_SHOW = 5;

		private static IntPtr window;


		public static string AppDataRoot { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AFB"); } }


		private static BackUp backUp;

		static void Main(string[] args)
		{
			try
			{
				LoggingUtilities.LogFormat("\nInitiating AFB ({0})\n", DateTime.Now.ToString());

				Settings.Load();
				window = GetConsoleWindow();
				ShowWindow(window, SW_HIDE);
				backUp = new BackUp();
				Settings.Store();
			}
			catch(Exception e)
			{
				LoggingUtilities.Log("ERROR\n");
				LoggingUtilities.Log(e.StackTrace);
				LoggingUtilities.Log(e.Message + "\n");
				LoggingUtilities.Log(e.Data.ToString() + "\n");

				Console.Beep(800, 500);
			}

			Console.Beep(400, 500);
			LoggingUtilities.LogFormat("Finished AFB ({0})\n", DateTime.Now.ToString());
		}


		public static string RequestInput(string request)
		{
			Console.Clear();
			ShowWindow(window, SW_SHOW);
			Console.WriteLine(request);
			string i = Console.ReadLine();
			ShowWindow(window, SW_HIDE);
			return i.ToLower();
		}

		public static string RequestInput(string format, params object[] args)
		{
			return RequestInput(string.Format(format, args));
		}
	}
}
