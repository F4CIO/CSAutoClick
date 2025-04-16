using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace CSAutoClick
{
    class Program
    {
        private static NotifyIcon trayIcon;
        private static bool isEnabled = false;
        private static bool showDebugMarkers = false;
        private static bool debugLogsEnabled = false; // New variable for debug logging
        private static int checkEveryXSeconds = 5;
        private static int precisionPercent = 70;
        private static List<string> imageFilePaths = new List<string>();
        private static bool shouldExit = false; // Flag to signal the thread to exit
        private static Mutex mutex; // Mutex to prevent multiple instances

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private const int LOGPIXELSX = 88; // DPI for width
        private const int LOGPIXELSY = 90; // DPI for height

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [STAThread]
        static void Main()
        {
            // Create a unique mutex name based on the executable's path
            string mutexName = "Global\\" + Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            mutex = new Mutex(true, mutexName, out bool isNewInstance);

            if(!isNewInstance)
            {
                LogError($"CSAutoClick from path '{Application.ExecutablePath}' is already running. Aborting this instance.");
                return; // Exit if another instance is already running
            }

            try
            {
                CreateConfigIfNotExists();
                LoadConfig();
                ScanForImages();

                // Initialize the context menu first
                trayIcon = new NotifyIcon()
                {
                    ContextMenu = new ContextMenu(new[]
                    {
                        new MenuItem("Enabled", ToggleEnabled) { Checked = isEnabled },
                        new MenuItem("Run on startup", ToggleRunOnStartup) { Checked = false }, // Set initial state
                        new MenuItem("Exit", Exit)
                    }),
                    Visible = true,
                    Text = "CSAutoClick" // Set the tooltip text
                };

                // Set the initial icon
                trayIcon.Icon = CreateIcon(isEnabled ? Color.Green : Color.Gray);

                // Check if the application is set to run on startup
                string appName = "CSAutoClick";
                using(var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if(key.GetValue(appName) != null)
                    {
                        trayIcon.ContextMenu.MenuItems[1].Checked = true; // Check the "Run on startup" checkbox
                    }
                }

                // Add MouseClick event handler to toggle Enabled state
                trayIcon.MouseClick += (sender, e) =>
                {
                    if(e.Button == MouseButtons.Left) // Check if the left mouse button was clicked
                    {
                        ToggleEnabled(sender, e); // Toggle the Enabled state
                    }
                };

                // Start the AutoClick process in a new thread
                Thread autoClickThread = new Thread(AutoClick);
                autoClickThread.IsBackground = true; // Set the thread as a background thread
                autoClickThread.Start();

                Application.Run();
            }
            catch(Exception ex)
            {
                LogError("An error occurred in Main: " + ex.Message);
            }
            finally
            {
                mutex.ReleaseMutex(); // Release the mutex when the application exits
            }
        }

        private static void CreateConfigIfNotExists()
        {
            string filePath = "CSAutoClick.ini";
            if(!File.Exists(filePath))
            {
                using(var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Enabled=false");
                    writer.WriteLine("CheckEveryXSeconds=5");
                    writer.WriteLine("PrecisionPercent=70");
                    writer.WriteLine("ShowDebugMarkers=false");
                    writer.WriteLine("DebugLogsEnabled=false"); // Add this line
                }
            }
        }

        private static void LoadConfig()
        {
            try
            {
                var config = ParseIniFile("CSAutoClick.ini");

                if(config.ContainsKey("Enabled"))
                    isEnabled = bool.Parse(config["Enabled"]);
                if(config.ContainsKey("CheckEveryXSeconds"))
                    checkEveryXSeconds = int.Parse(config["CheckEveryXSeconds"]);
                if(config.ContainsKey("PrecisionPercent"))
                    precisionPercent = int.Parse(config["PrecisionPercent"]);
                if(config.ContainsKey("ShowDebugMarkers"))
                    showDebugMarkers = bool.Parse(config["ShowDebugMarkers"]);
                if(config.ContainsKey("DebugLogsEnabled")) // Load the debug logs setting
                    debugLogsEnabled = bool.Parse(config["DebugLogsEnabled"]);

                Log("Configuration loaded successfully.");
            }
            catch(Exception ex)
            {
                LogError("Failed to load configuration: " + ex.Message);
            }
        }

        private static Dictionary<string, string> ParseIniFile(string filePath)
        {
            var config = new Dictionary<string, string>();

            if(!File.Exists(filePath))
                return config;

            string[] lines = File.ReadAllLines(filePath);
            foreach(var line in lines)
            {
                if(line == null || line.Trim().Length == 0 || line.StartsWith(";")) continue;

                var parts = line.Split(new[] { '=' }, 2);
                if(parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    config[key] = value;
                }
            }

            return config;
        }

        private static void ScanForImages()
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
                LogError("Error scanning for images: " + ex.Message);
            }
        }

        private static void ToggleEnabled(object sender, EventArgs e)
        {
            isEnabled = !isEnabled;
            trayIcon.ContextMenu.MenuItems[0].Checked = isEnabled;
            UpdateConfig("Enabled", isEnabled.ToString());

            // Update the tray icon based on the new state
            trayIcon.Icon = CreateIcon(isEnabled ? Color.Green : Color.Gray);
            Log($"AutoClick is now {(isEnabled ? "enabled" : "disabled")}.");
        }

        private static void ToggleRunOnStartup(object sender, EventArgs e)
        {
            string appName = "CSAutoClick";
            string appPath = Application.ExecutablePath;

            using(var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if(key.GetValue(appName) == null)
                {
                    key.SetValue(appName, appPath);
                    trayIcon.ContextMenu.MenuItems[1].Checked = true; // Check the "Run on startup" checkbox
                    Log("Set to run on startup.");
                }
                else
                {
                    key.DeleteValue(appName);
                    trayIcon.ContextMenu.MenuItems[1].Checked = false; // Uncheck the "Run on startup" checkbox
                    Log("Removed from startup.");
                }
            }
        }

        private static void Exit(object sender, EventArgs e)
        {
            shouldExit = true; // Signal the thread to exit
            trayIcon.Visible = false; // Hide the tray icon
            Log("Exiting application.");

            Application.Exit(); // Exit the application
        }

        private static void AutoClick()
        {
            while(!shouldExit)
            {
                if(isEnabled)
                {
                    foreach(var imagePath in imageFilePaths)
                    {
                        try
                        {
                            using(var template = new Image<Bgr, byte>(imagePath))
                            {
                                DetectAndClick(template, imagePath); // Call the synchronous version
                            }
                        }
                        catch(Exception ex)
                        {
                            LogError($"Error processing image {imagePath}: {ex.Message}");
                        }
                    }
                }
                Thread.Sleep(checkEveryXSeconds * 1000); // Synchronously wait
            }
        }

        private static void DetectAndClick(Image<Bgr, byte> template, string imagePath)
        {
            foreach(var screen in Screen.AllScreens)
            {
                try
                {
                    using(var screenCapture = CaptureScreen(screen.Bounds)) // Synchronous capture
                    using(var screenImage = new Image<Bgr, byte>(screenCapture))
                    using(var result = new Mat())
                    {
                        CvInvoke.MatchTemplate(screenImage, template, result, TemplateMatchingType.CcoeffNormed);

                        double minValue = 0, maxValue = 0;
                        Point minLocation = new Point(), maxLocation = new Point();
                        CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLocation, ref maxLocation);

                        if(maxValue * 100 >= precisionPercent)
                        {
                            Point matchLocation = maxLocation;
                            int offsetX = GetSubstringAsInt(imagePath, ".OX", ".") ?? template.Width / 2;
                            int offsetY = GetSubstringAsInt(imagePath, ".OY", ".") ?? template.Height / 2;
                            int clickX = screen.Bounds.X + matchLocation.X + offsetX;
                            int clickY = screen.Bounds.Y + matchLocation.Y + offsetY;

                            bool isRightClick = imagePath.IndexOf("RightClick", StringComparison.OrdinalIgnoreCase) >= 0;

                            ClickAt(clickX, clickY, isRightClick);

                            if(debugLogsEnabled)
                            {
                                Log($"DEBUG: Clicked at ({clickX}, {clickY}) for image {Path.GetFileName(imagePath)}.");
                                Log($"DEBUG: Detected image '{Path.GetFileName(imagePath)}' at ({clickX}, {clickY}) with confidence {maxValue * 100:F2}%.");
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    LogError($"Error detecting and clicking on screen {screen.DeviceName}: {ex.Message}");
                }
            }
        }

        private static Bitmap CaptureScreen(Rectangle bounds)
        {
            Bitmap screenCapture = new Bitmap(bounds.Width, bounds.Height);
            using(Graphics g = Graphics.FromImage(screenCapture))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return screenCapture; // Ensure to dispose of this Bitmap in the calling method
        }

        private static void ClickAt(int x, int y, bool rightClick = false)
        {
            Cursor.Position = new Point(x, y);
            if(rightClick)
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
            else
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }

            if(debugLogsEnabled)
            {
                Log($"DEBUG: Mouse {(rightClick ? "right" : "left")} clicked at ({x}, {y}).");
            }
        }

        private static Icon CreateIcon(Color color)
        {
            int width = 16; // Icon width
            int height = 16; // Icon height
            Bitmap bitmap = new Bitmap(width, height);

            using(Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent); // Set background to transparent
                using(Brush brush = new SolidBrush(color))
                {
                    // Draw the letter "A"
                    Font font = new Font("Arial", 12, FontStyle.Bold);
                    g.DrawString("A", font, brush, new PointF(0, 0));
                }

                using(Pen pen = new Pen(color))
                {
                    g.DrawEllipse(pen, new Rectangle(1, 1, width - 2, height - 2));
                }
            }

            // Create the icon from the bitmap
            Icon icon = Icon.FromHandle(bitmap.GetHicon());
            // Dispose of the bitmap to prevent memory leak
            bitmap.Dispose();

            // Return the icon, but ensure to release the handle when done
            return icon;
        }

        private static void UpdateConfig(string key, string value)
        {
            var config = ParseIniFile("CSAutoClick.ini");
            config[key] = value;

            using(var writer = new StreamWriter("CSAutoClick.ini"))
            {
                foreach(var entry in config)
                {
                    writer.WriteLine($"{entry.Key}={entry.Value}");
                }
            }
            Log($"Updated config: {key} = {value}");
        }

        public static int? GetSubstringAsInt(string fileName, string prefix, string suffix)
        {
            int prefixIndex = fileName.IndexOf(prefix);
            int suffixIndex = fileName.IndexOf(suffix, prefixIndex + prefix.Length);

            if(prefixIndex != -1 && suffixIndex != -1)
            {
                int start = prefixIndex + prefix.Length; // Start after the prefix
                string value = fileName.Substring(start, suffixIndex - start);
                if(int.TryParse(value, out int result))
                {
                    return result; // Return the parsed integer
                }
            }

            return null; // Return null if extraction fails
        }

        private static void Log(string message)
        {
            try
            {
                using(var writer = new StreamWriter("CSAutoClick.log", true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch(Exception ex)
            {
                // If logging fails, we can choose to ignore it or handle it differently
                Console.WriteLine("Logging failed: " + ex.Message);
            }
        }

        private static void LogError(string message)
        {
            Log("ERROR: " + message);
        }
    }
}


