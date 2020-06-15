using Framework.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace SimpleBackUpTool
{
    static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new BackUpProgram());
		}
	}


    internal class BackUpProgram : ApplicationContext
    {
        private Settings settings;
        private BackUp backUp;
        private NotifyIcon trayIcon;

        
        public static string AppDataRoot
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                        "Simple_Back_Up");
            }
        }

        public BackUpProgram()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Make Back-Up", StartBackUp),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }

        void StartBackUp(object sender, EventArgs e)
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
            catch (Exception ex)
            {
                LoggingUtilities.Log("ERROR\n");
                LoggingUtilities.Log(ex.StackTrace + "\n");
                LoggingUtilities.Log(ex.Message + "\n");
                LoggingUtilities.Log(ex.Data.ToString() + "\n");

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
}
