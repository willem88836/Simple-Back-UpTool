using Framework.Utils;
using SimpleBackUpTool.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SimpleBackUpTool
{
    public class BackUpProgram : ApplicationContext
    {
        public static BackUpProgram Instance;
        private Settings settings;
        private BackUp backUp;
        private NotifyIcon trayIcon;
        private Thread loadIconThread;
        private Thread backUpThread;

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
            if (Instance != null)
            {
                throw new Exception("Duplicate BackUpProgram Singleton");
            }

            Instance = this;
            settings = new Settings();

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.AppIcon,
                Visible = true
            };

            Reloadmenu();
        }
       
        private void Reloadmenu()
        {
            ContextMenu contextMenu = new ContextMenu(
                new MenuItem[] {
                    new MenuItem("Exit", Exit),
                    new MenuItem("Open AppData", ShowAppData),
                    new MenuItem("Create New Profile", CreateNewBackUpSettings)
                });

            foreach (string setting in settings.UserSettings)
            {
                MenuItem item = new MenuItem(
                    "Profile: " + Path.GetFileNameWithoutExtension(setting),
                    (sender, e) => { StartBackUp((sender as MenuItem).Name); });
                item.Name = setting;
                contextMenu.MenuItems.Add(item);
            }

            trayIcon.ContextMenu = contextMenu;
        }


        private void LoadBackingUpMenu()
        {
            ContextMenu contextMenu = new ContextMenu(
                new MenuItem[]
                {
                    new MenuItem("Exit", (sender, e) => {  StopBackUp(); Exit(sender, e); }),
                    new MenuItem("Open AppData", ShowAppData),
                    new MenuItem("Stop Back-Up", (sender, e) => { StopBackUp(); Reloadmenu(); })
                });

            trayIcon.ContextMenu = contextMenu;
        }


        private void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void StartBackUp(string userSettings)
        {
            LoadBackingUpMenu();

            backUpThread = new Thread(() =>
            {
                LoggingUtilities.SetAppDataRoot(AppDataRoot);
                try
                {
                    LoadingIcon();
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
                finally
                {
                    if(loadIconThread != null)
                    {
                        loadIconThread.Abort();
                        loadIconThread = null;
                        trayIcon.Icon = Resources.AppIcon;
                    }

                    Reloadmenu();

                    Console.Beep(400, 500);
                    LoggingUtilities.LogFormat("Finished Simple Back-Up ({0})\n", DateTime.Now.ToString());
                }
            });

            backUpThread.Start();
        }

        private void StopBackUp()
        {
            backUpThread.Abort();
            loadIconThread.Abort();
            trayIcon.Icon = Resources.AppIcon;
            Reloadmenu();
            LoggingUtilities.Log("INTERRUPTED BACK-UP\n");
        }

        private void CreateNewBackUpSettings(object sender, EventArgs e)
        {
            Form1 form = new Form1(null);
            form.Show();
        }

        private void ShowAppData(object sender, EventArgs e)
        {
            Process.Start(AppDataRoot);
        }


        private void LoadingIcon()
        {
            loadIconThread = new Thread(() =>
            {
                int i = 0; 
                while (true)
                {
                    i++;
                    i %= 3;
                    switch (i)
                    {
                        case 0:
                            trayIcon.Icon = Resources.AppIcon;
                            break;
                        case 1:
                            trayIcon.Icon = Resources.AppIcon1;
                            break;
                        case 2:
                            trayIcon.Icon = Resources.AppIcon2;
                            break;
                    }
                    Thread.Sleep(1000);
                }
            });

            loadIconThread.Start();
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
