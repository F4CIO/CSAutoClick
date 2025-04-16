using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IWshRuntimeLibrary;

namespace CSImageClick
{
    partial class Program
    {
        public static void CreateShortcutInUserStartupFolder(bool suppressAllErrors = false)
        {
            try
            {
                string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string currentUserStartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutFilePath = Path.Combine(currentUserStartupFolderPath, Path.GetFileNameWithoutExtension(exeFilePath) + ".lnk");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);
                shortcut.TargetPath = exeFilePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exeFilePath);
                shortcut.Description = "CSImageClick";
                shortcut.Save();
            }catch{
                if(!suppressAllErrors) {
                    throw;
                }
            }
        }

        public static bool ShortcutThatTargetOurExeInStartupFoldersExists(bool suppressAllErrors = false)
        {
            bool r = false;

            try
            {
                string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string currentUserStartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string allUsersStartupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows\Start Menu\Programs\Startup");
                List<string> shortcutsFilesPaths = new List<string>();
                if(Directory.Exists(currentUserStartupFolderPath))
                {
                    shortcutsFilesPaths.AddRange(Directory.GetFiles(currentUserStartupFolderPath, "*.lnk").ToList());
                }
                if(Directory.Exists(allUsersStartupFolderPath))
                {
                    shortcutsFilesPaths.AddRange(Directory.GetFiles(allUsersStartupFolderPath, "*.lnk").ToList());
                }

                var shell = new WshShell();
                foreach(string shortcutFilePath in shortcutsFilesPaths)
                {
                    var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);
                    if(string.Equals(shortcut.TargetPath, exeFilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        r = true;
                        break;
                    }
                }
            }
            catch
            {
                if(!suppressAllErrors)
                {
                    throw;
                }
            }

            return r;
        }

        public static void DeleteAllShortcutsInStartupFoldersThatTargetOurExe()
        {
            string exeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string currentUserStartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string allUsersStartupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows\Start Menu\Programs\Startup");
            List<string> shortcutsFilesPaths = new List<string>();
            if(Directory.Exists(currentUserStartupFolderPath))
            {
                shortcutsFilesPaths.AddRange(Directory.GetFiles(currentUserStartupFolderPath, "*.lnk").ToList());
            }
            if(Directory.Exists(allUsersStartupFolderPath))
            {
                shortcutsFilesPaths.AddRange(Directory.GetFiles(allUsersStartupFolderPath, "*.lnk").ToList());
            }

            var shell = new WshShell();
            foreach(string shortcutFilePath in shortcutsFilesPaths)
            {
                var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);
                if(string.Equals(shortcut.TargetPath, exeFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    System.IO.File.Delete(shortcutFilePath);
                }
            }
        }
    }
}
