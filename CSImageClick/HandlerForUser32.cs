using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace CSImageClick
{
    internal class HandlerForUser32
    {
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public static List<Rectangle> GetAllVisibleWindowsAreas()
        {
            List<Rectangle> rectangles = new List<Rectangle>();

            // Enumerate all top-level windows
            EnumWindows(new EnumWindowsProc((hWnd, lParam) =>
            {
                // Check if the window is visible
                if(IsWindowVisible(hWnd))
                {
                    // Get the client rectangle
                    RECT clientRect;
                    if(GetClientRect(hWnd, out clientRect))
                    {
                        // Convert the top-left corner of the client area to screen coordinates
                        POINT topLeft = new POINT { X = clientRect.Left, Y = clientRect.Top };
                        ClientToScreen(hWnd, ref topLeft);

                        // Create a Rectangle object and add it to the list
                        rectangles.Add(new Rectangle(topLeft.X, topLeft.Y,
                                                      clientRect.Right - clientRect.Left,
                                                      clientRect.Bottom - clientRect.Top));
                    }
                }
                return true; // Continue enumeration
            }), IntPtr.Zero);

            return rectangles;
        }

        ////////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        public static List<Rectangle> GetAllControlAreas()
        {
            List<Rectangle> rectangles = new List<Rectangle>();

            // Enumerate all top-level windows
            EnumWindows(new EnumWindowsProc((hWnd, lParam) =>
            {
                // Check if the window is visible
                if(IsWindowVisible(hWnd))
                {
                    // Enumerate child windows (controls)
                    EnumChildWindows(hWnd, new EnumChildProc((childHWnd, childLParam) =>
                    {
                        // Check if the child window is visible
                        if(IsWindowVisible(childHWnd))
                        {
                            // Get the rectangle of the child window
                            RECT childRect;
                            if(GetWindowRect(childHWnd, out childRect))
                            {
                                // Create a Rectangle object and add it to the list
                                rectangles.Add(new Rectangle(childRect.Left, childRect.Top,
                                                              childRect.Right - childRect.Left,
                                                              childRect.Bottom - childRect.Top));
                            }
                        }
                        return true; // Continue enumeration
                    }), IntPtr.Zero);
                }
                return true; // Continue enumeration
            }), IntPtr.Zero);

            return rectangles;
        }

        //////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;

        public static void ClickAt(int x, int y, bool rightClick = false)
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
        }

        //////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        /////////////////////////////////////////////////////////////////////////////////
        
        //[DllImport("libc")]
        //private static extern uint geteuid();

        //public bool IsCurrentProcessElevated()
        //{
        //    if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    {
        //        // https://github.com/dotnet/sdk/blob/v6.0.100/src/Cli/dotnet/Installer/Windows/WindowsUtils.cs#L38
        //        using var identity = WindowsIdentity.GetCurrent();
        //        var principal = new WindowsPrincipal(identity);
        //        return principal.IsInRole(WindowsBuiltInRole.Administrator);
        //    }

        //    // https://github.com/dotnet/maintenance-packages/blob/62823150914410d43a3fd9de246d882f2a21d5ef/src/Common/tests/TestUtilities/System/PlatformDetection.Unix.cs#L58
        //    // 0 is the ID of the root user
        //    return geteuid() == 0;
        //}
    }
}
