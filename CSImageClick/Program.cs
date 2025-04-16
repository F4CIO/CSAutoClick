using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace CSImageClick
{
    partial class Program
    {
        #region Private Members
        private static NotifyIcon trayIcon;
        private static bool isEnabled = false;
        private static bool showDebugMarkers = false;
        private static bool debugLogsEnabled = false;
        private static int checkEveryXSeconds = 5;
        private static int precisionPercent = 50;
        private static int allowedDisplacementInPixels = 1;
        private static bool flashTrayIconDuringScan = true;
        private static bool flashTrayIconDuringAction = true;
        private static int restartEveryXMinutes = 10080;//7 days
        private static DateTime startMoment = DateTime.Now;

        private static List<string> imageFilePaths = new List<string>();
        private static bool shouldExit = false; // Flag to signal the thread to exit
        private static Mutex mutex; // Mutex to prevent multiple instances -we can't do this with periodic restart
        private static Icon yellowIcon = null;
        private static Icon grayIcon = null;
        private static Icon greenIcon = null;
        private static Icon darkGgreenIcon = null;

        //internal static Form mainForm;
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Constructors And Initialization
        [STAThread]
        static void Main()
        {
            //// Create a unique mutex name based on the executable's path
            //string mutexName = "Global\\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            //mutex = new Mutex(true, mutexName, out bool isNewInstance);

            //if(!isNewInstance)
            //{
            //    LogError($"CSImageClick from path '{Application.ExecutablePath}' is already running. Aborting this instance.");
            //    return; // Exit if another instance is already running
            //}

            try
            {
                // Initialize the context menu first
                trayIcon = new NotifyIcon()
                {
                    Visible = true,
                    Text = "CSImageClick" // Set the tooltip text
                };

                // Set the initial icon
                grayIcon = HandlerForGraphics.CreateIcon(Color.Gray);
                greenIcon = HandlerForGraphics.CreateIcon(Color.Green);
                darkGgreenIcon = HandlerForGraphics.CreateIcon(Color.DarkGreen);
                yellowIcon = HandlerForGraphics.CreateIcon(Color.Yellow);
                trayIcon.Icon = yellowIcon;

                // Start the AutoClick process in a new thread
                Thread autoClickThread = new Thread(ProcessAllPeriodically);
                autoClickThread.IsBackground = true; // Set the thread as a background thread
                autoClickThread.Start();

                //mainForm = new Form1();
                //mainForm.Visible = true;
                Application.Run();
            }
            catch(Exception ex)
            {
                LogError($"An error occurred in Main: {ex.Message} {ex.StackTrace}");
            }
            //finally
            //{
            //    mutex.ReleaseMutex(); // Release the mutex when the application exits
            //}
        }
        #endregion

        #region Deinitialization And Destructors
        private static void Exit(object sender, EventArgs e)
        {
            shouldExit = true; // Signal the thread to exit
            trayIcon.Visible = false; // Hide the tray icon
            HandlerForUser32.DestroyIcon(grayIcon.Handle);
            HandlerForUser32.DestroyIcon(greenIcon.Handle);
            HandlerForUser32.DestroyIcon(darkGgreenIcon.Handle);
            HandlerForUser32.DestroyIcon(yellowIcon.Handle);
            Log("Exiting application.");

            Application.Exit(); // Exit the application
        }

        private static void RestartApplication()
        {
            Log("Auto-restarting...");

            // Start a new instance of the application
            Process.Start(Application.ExecutablePath);

            // Close the current instance
            Exit(null, null);
        }
        #endregion

        #region Event Handlers
        private static void ToggleEnabled(object sender, EventArgs e)
        {
            isEnabled = !isEnabled;

            trayIcon.ContextMenu.MenuItems[0].Checked = isEnabled;
            UpdateIniFile("Enabled", isEnabled.ToString());

            trayIcon.Icon = isEnabled ? darkGgreenIcon : grayIcon;
            Log($"AutoClick is now {(isEnabled ? "enabled" : "disabled")}.");
        }

        private static void ToggleRunOnStartup(object sender, EventArgs e)
        {
            try
            {
                bool runOnStartup = ShortcutThatTargetOurExeInStartupFoldersExists();
                if(runOnStartup)
                {
                    DeleteAllShortcutsInStartupFoldersThatTargetOurExe();
                    trayIcon.ContextMenu.MenuItems[1].Checked = false; 
                    Log("Removed from startup.");
                }
                else
                {
                    CreateShortcutInUserStartupFolder(true);
                    trayIcon.ContextMenu.MenuItems[1].Checked = ShortcutThatTargetOurExeInStartupFoldersExists(); 
                    Log("Set to run on startup.");
                }
            }
            catch(Exception ex)
            {
                Log($"Error toggling Run on startup: {ex.Message} {ex.StackTrace}");
            }
        }
        #endregion

        #region Private Methods
        private static void ScanForImagesInFolder()
        {
            try
            {
                imageFilePaths.Clear();
                imageFilePaths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.png").ToList());
                imageFilePaths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.jpg").ToList());
                imageFilePaths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bmp").ToList());
                imageFilePaths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), "*.gif").ToList());
                imageFilePaths = imageFilePaths.Where(f => !f.Contains(".tmp.")).ToList();
                imageFilePaths.Sort();
                Log($"Found {imageFilePaths.Count} image(s) for clicking.");
            }
            catch(Exception ex)
            {
                LogError($"Error scanning for images: {ex.Message} {ex.StackTrace}");
            }
        }

        private static void ProcessAllPeriodically()
        {
            try
            {
                CreateIniFileIfNotExists();
                LoadIniFile();
                ScanForImagesInFolder();

                trayIcon.ContextMenu = new ContextMenu(new[]
                  {
                        new MenuItem("Enabled", ToggleEnabled) { Checked = isEnabled },
                        new MenuItem("Run on startup", ToggleRunOnStartup) { Checked = false }, // Set initial state
                        new MenuItem("Exit", Exit)
                    });

                // Check if the application is set to run on startup
                trayIcon.ContextMenu.MenuItems[1].Checked = ShortcutThatTargetOurExeInStartupFoldersExists();

                // Add MouseClick event handler to toggle Enabled state
                trayIcon.MouseClick += (sender, e) =>
                {
                    if(e.Button == MouseButtons.Left) // Check if the left mouse button was clicked
                    {
                        ToggleEnabled(sender, e); // Toggle the Enabled state
                    }
                };

                trayIcon.Icon = isEnabled ? darkGgreenIcon : grayIcon;

                if(!isEnabled)
                {
                    Log("Initialized successfully. Service is disabled.");
                }
                else
                {
                    Log("Initialized successfully. Starting work..");
                }

                while(!shouldExit)
                {
                    if(isEnabled)
                    {
                        foreach(var imagePath in imageFilePaths)
                        {
                            try
                            {
                                using(var template = new Bitmap(imagePath))
                                {
                                    ProcessAll(template, imagePath); // Call the synchronous version
                                }
                            }
                            catch(Exception ex)
                            {
                                LogError($"Error processing image {imagePath}:{ex.Message} {ex.StackTrace}");
                            }
                        }
                    }

                    Thread.Sleep(checkEveryXSeconds * 1000); // Synchronously wait

                    if(restartEveryXMinutes > 0)
                    {
                        var restartEveryXMinutesFinalAsTicks = Math.Max(TimeSpan.FromMinutes(restartEveryXMinutes).Ticks, TimeSpan.FromSeconds(checkEveryXSeconds).Ticks);//we must not restart more often than checkEveryXSeconds
                        if(startMoment.Ticks + restartEveryXMinutesFinalAsTicks < DateTime.Now.Ticks)
                        {
                            RestartApplication();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                LogError($"An error occurred in AutoClick background thread: {ex.Message} {ex.StackTrace}");
            }
        }

        private static bool ProcessAll(Image template, string imagePath)
        {
            var matched = false;
            if(flashTrayIconDuringScan)
            {
                trayIcon.Icon = greenIcon;
            }

            foreach(var screen in Screen.AllScreens)
            {
                matched = ProcessWindowArea(screen.Bounds, template, imagePath);
            }

            if(!matched)
            {
                List<Rectangle> visibleWindows = HandlerForUser32.GetAllVisibleWindowsAreas();
                foreach(var rect in visibleWindows)
                {
                    ProcessWindowArea(rect, template, imagePath);
                }
            }

            if(!matched)
            {
                List<Rectangle> visibleControls = HandlerForUser32.GetAllControlAreas();
                foreach(var rect in visibleControls)
                {
                    ProcessWindowArea(rect, template, imagePath);
                }
            }

            trayIcon.Icon = isEnabled ? darkGgreenIcon : grayIcon;
            return matched;
        }

        //private static void UpdateUI(Bitmap bmp)
        //{
        //    // Check if we need to invoke on the UI thread
        //    if(mainForm.InvokeRequired)
        //    {
        //        // Use Invoke to call UpdateUI on the UI thread
        //        mainForm.Invoke(new Action<Bitmap>(UpdateUI), bmp);
        //    }
        //    else
        //    {
        //        // Update the UI control here
        //        mainForm.BackgroundImage = bmp;

        //        mainForm.Invalidate();
        //        mainForm.Visible = true;
        //        mainForm.Refresh();
        //    }
        //}

        private static bool ProcessWindowArea(Rectangle windowAreaRelativeToDesktopCorner, Image template, string imagePath)
        {
            //if(debugLogsEnabled)
            //{
            //    var b = CaptureAreaFromDesktop(windowAreaRelativeToDesktopCorner);
            //    UpdateUI(b);
            //    b.Save(Path.GetDirectoryName(Application.ExecutablePath)+"\\Debug\\" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".png", System.Drawing.Imaging.ImageFormat.Png);
            //}

            var matched = false;
            try
            {
                Rectangle hotAreaRelativeToWindowCorner = new Rectangle();

                // Extract coordinates from the imagePath filename
                string fileName = Path.GetFileNameWithoutExtension(imagePath)+".";

                // Extract X and Y coordinates using the new method
                int? x = HandlerForMisc.GetSubstringAsInt(fileName, ".X", ".");
                int? y = HandlerForMisc.GetSubstringAsInt(fileName, ".Y", ".");
                if(x.HasValue)
                {
                    hotAreaRelativeToWindowCorner.X = x.Value;
                }
                else
                {
                    return false;//wrong syntax of image file name -skip it
                }
                if(y.HasValue)
                {
                    hotAreaRelativeToWindowCorner.Y = y.Value;
                }
                else
                {
                    return false;//wrong syntax of image file name -skip it
                }
                hotAreaRelativeToWindowCorner.Width = template.Width;
                hotAreaRelativeToWindowCorner.Height = template.Height;

                // Ensure hotArea is within screen bounds
                if(hotAreaRelativeToWindowCorner.X < 0 || hotAreaRelativeToWindowCorner.Y < 0 || hotAreaRelativeToWindowCorner.X + template.Width > windowAreaRelativeToDesktopCorner.Width || hotAreaRelativeToWindowCorner.Y + template.Height > windowAreaRelativeToDesktopCorner.Height)
                {
                    return false; // Skip this screen if the hot area is out of bounds
                }

                // Point-by-point comparison
                using(var hotAreaRelativeToWindowCorner_Image = HandlerForGraphics.CaptureAreaFromDesktop(new Rectangle(windowAreaRelativeToDesktopCorner.X + hotAreaRelativeToWindowCorner.X, windowAreaRelativeToDesktopCorner.Y + hotAreaRelativeToWindowCorner.Y, hotAreaRelativeToWindowCorner.Width, hotAreaRelativeToWindowCorner.Height)))
                {
                    matched = HandlerForGraphics.PixelByPixelCompare(template, hotAreaRelativeToWindowCorner_Image, precisionPercent, 0, 0);
                    //if previous compare failed -tilt one rectangle and compare again. do that in every direction
                    for(int tiltX = -allowedDisplacementInPixels; tiltX <= allowedDisplacementInPixels; tiltX++)
                    {
                        for(int tiltY = -allowedDisplacementInPixels; tiltY <= allowedDisplacementInPixels; tiltY++)
                        {
                            if(!(tiltX == 0 && tiltY == 0))
                            {
                                matched = matched || HandlerForGraphics.PixelByPixelCompare(template, hotAreaRelativeToWindowCorner_Image, precisionPercent, tiltX, tiltY);
                            }
                        }
                    }
                    if(debugLogsEnabled)
                    {
                        //  hotAreaRelativeToWindowCorner_Image.Save(Path.Combine(Path.GetDirectoryName(imagePath), Path.GetFileNameWithoutExtension(imagePath) + ".lastFromArea"+ windowAreaRelativeToDesktopCorner.Left+ windowAreaRelativeToDesktopCorner.Top+ windowAreaRelativeToDesktopCorner.Right+ windowAreaRelativeToDesktopCorner.Bottom+"." + (matched ? "matched" : "notMatched") + ".tmp." + Path.GetExtension(imagePath)));
                    }
                }

                if(matched)
                {
                    if(flashTrayIconDuringAction)
                    {
                        trayIcon.Icon = yellowIcon;
                    }
                    int offsetX = HandlerForMisc.GetSubstringAsInt(fileName, ".OX", ".") ?? hotAreaRelativeToWindowCorner.Width / 2;
                    int offsetY = HandlerForMisc.GetSubstringAsInt(fileName, ".OY", ".") ?? hotAreaRelativeToWindowCorner.Height / 2;
                    int clickX = windowAreaRelativeToDesktopCorner.X + hotAreaRelativeToWindowCorner.X + offsetX;
                    int clickY = windowAreaRelativeToDesktopCorner.Y + hotAreaRelativeToWindowCorner.Y + offsetY;

                    bool isRightClick = imagePath.IndexOf("RightClick", StringComparison.OrdinalIgnoreCase) >= 0;

                    HandlerForUser32.ClickAt(clickX, clickY, isRightClick);

                    if(debugLogsEnabled)
                    {
                        Log($"DEBUG: Clicked at ({clickX}, {clickY}) for image {Path.GetFileName(imagePath)}.");
                        HandlerForMisc.BeepInNewThread();
                        //hotAreaRelativeToWindowCorner_Image.Save(Path.Combine(Path.GetDirectoryName(imagePath), Path.GetFileNameWithoutExtension(imagePath) + ".lastFromArea"+ windowAreaRelativeToDesktopCorner.Left+ windowAreaRelativeToDesktopCorner.Top+ windowAreaRelativeToDesktopCorner.Right+ windowAreaRelativeToDesktopCorner.Bottom+"." + (matched ? "matched" : "notMatched") + ".tmp." + Path.GetExtension(imagePath)));
                    }
                }
            }
            catch(Exception ex)
            {
                LogError($"Error detecting and clicking on window {windowAreaRelativeToDesktopCorner.Left + windowAreaRelativeToDesktopCorner.Top + windowAreaRelativeToDesktopCorner.Right + windowAreaRelativeToDesktopCorner.Bottom}: {ex.Message} {ex.StackTrace}");
            }
            return matched;
        }
        #endregion

        #region Helpers
        private static void Log(string message)
        {
            try
            {
                using(var writer = new StreamWriter("CSImageClick.log", true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch(Exception ex)
            {
                // If logging fails, we can choose to ignore it or handle it differently
                Console.WriteLine($"Logging failed: {ex.Message} {ex.StackTrace}");
            }
        }

        private static void LogError(string message)
        {
            Log("ERROR: " + message);
        }

        #endregion
    }
}


