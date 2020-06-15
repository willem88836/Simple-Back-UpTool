using Framework.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SimpleBackUpTool
{
    public class BackUpProgram : ApplicationContext
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
            settings = new Settings();

            ContextMenu contextMenu = new ContextMenu(new MenuItem[] {
                        new MenuItem("Exit", Exit),
                        new MenuItem("Create New Profile", CreateNewBackUpSettings)
                    });

            foreach(string setting in settings.UserSettings)
            {
                MenuItem item = new MenuItem(
                    "Profile: " + Path.GetFileNameWithoutExtension(setting),
                    (sender, e) => { StartBackUp((sender as MenuItem).Name); });
                item.Name = setting;
                contextMenu.MenuItems.Add(item);
            }


            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                // TODO: Animate icon when backing up? :D 
                Icon = Properties.Resources.AppIcon,
                ContextMenu = contextMenu,
                Visible = true
            };
        }

        private void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }

        private void StartBackUp(string userSettings)
        {
            LoggingUtilities.SetAppDataRoot(AppDataRoot);
            try
            {
                LoggingUtilities.LogFormat("\nInitiating Simple Back-Up ({0})\n", DateTime.Now.ToString());
                
                BackUpSettings loadedSettings = settings.Load(userSettings);

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

        private void CreateNewBackUpSettings(object sender, EventArgs e)
        {
            Console.Beep(200, 500);
            //TODO: Make a form or something for this.
            MessageBox.Show("whatever");
        }


        public static string RequestInput(string request)
        {
            DialogResult result = MessageBox.Show(request, "Input Request", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                result = MessageBox.Show("Always?", "Input Request", MessageBoxButtons.YesNo);
                return result == DialogResult.Yes ? "a" : "y";
            }
            else
            {
                result = MessageBox.Show("Always?", "Input Request", MessageBoxButtons.YesNo);
                return result == DialogResult.Yes ? "e" : "o";
            }
        }

        public static string RequestInput(string format, params object[] args)
        {
            return RequestInput(string.Format(format, args));
        }
    }
}
