# ReadMe.txt for CSAutoClick 1.0

## Overview
CSAutoClick is a Windows application designed to automate mouse clicks based on AI/computer vision image recognition. It continuously scans the screen for images found in same folder and performs clicks when matches are found. This tool is particularly useful for repetitive tasks that require clicking on specific UI elements like annoying popups.

## Features
- Image Recognition: Automatically detects images on the screen and clicks on them.
- Configurable Settings: Customize the behavior of the application through a configuration file.
- System Tray Integration: Run the application in the background with a system tray icon for easy access.
- Debug Logging: Optionally enable debug logs to track the application's actions and errors.
- Run on Startup: Option to configure the application to start automatically with Windows.
- Scale other than 100% in Display settings is NOT supported.

## Requirements
- Newer operating system like Windows 10 or Windows 11. For older operating system try using this software but if detection is not working use my other software [CSImageClick](https://github.com/F4CIO/CSImageClick).
- .NET Framework 3.5 (should already be included in Windows 7 and newer )
- Scale of 100% in Display settings for the screen where you intend to use it. Using other scales is not tested.

## Installation & Usage
1. Download the latest release of CSAutoClick.
2. Extract the contents of the zip file to a desired location on your computer.
3. Grab popup buttons, images or other UI elements that you want to auto-click and save them as .png image files beside CSAutoClick.exe file. One good tool for taking screenshoots or part of screen is Greenshoot (https://getgreenshot.org/downloads).   
4. Run CSAutoClick.exe 
5. Optionally: insure you see 'A' icon in your tray near clock. Windows may hide some tray icons. Google for 'how to unhide tray icon in windows' for advice.
6. If you don't see your desired UI element/image clicked when present on screen: Right click on 'A' tray icon and insure Enabled is checked. See Troubleshooting section below.

## Configuration
The application uses a configuration file named `CSAutoClick.ini`. This file is created automatically on the first run if it does not exist. You can manually edit this file to customize the following settings:

- `Enabled`: Set to `true` to enable the auto-click feature, or `false` to disable it.
- `CheckEveryXSeconds`: The interval (in seconds) at which the application checks for images on the screen.
- `PrecisionPercent`: The confidence level (0-100) required to consider an image match valid.
- `ShowDebugMarkers`: Set to `true` to display debug markers on the screen.
- `DebugLogsEnabled`: Set to `true` to enable logging of debug information.

### Example `CSAutoClick.ini` Configuration
<code>Enabled=false
CheckEveryXSeconds=5
PrecisionPercent=70
ShowDebugMarkers=false
DebugLogsEnabled=false'</code>

## Image Files
Place the images you want to detect in the same directory as the application. The application currently supports .png .jpg .bmp and .gif files. You can put 'RightClick' in file name of images to indicate whether a right-click should be performed (e.g., `MyButton1.RightClick.png`). Click location is by default in center of image. To set click to different location you can put put some pixel number beween .OX and . like for example myAnnoyingButton.OX-10.OY550.rightClick.png

## Logging
The application logs its activities to a file named `CSAutoClick.log`. This log file will be created in the same directory as the application. If debug logging is enabled, additional information will be recorded.

## Troubleshooting
- If the application does not detect images on one screen but works on the other insure you are using 100% scale for that screen in Windows Display settings. Other scale is NOT supported.
- If the application does not detect images, ensure that the images are in the correct format and named appropriately.
- Check the log file CSImageClick.log for any error messages that may indicate issues with image processing or configuration.
- In .ini file set DebugLogsEnabled=true to see more detailed logs in .log file but remember to switch it of later to releave your system resources.


## For Developers
If you are working on a source code of this app usefull to know is about Emgu.CV requirements, dependencies and limitations. Emgu CV is a .NET wrapper for the OpenCV library. 
To run this appliation on end user machine native Emgu dll-s should be present beside .exe file. One way to get these native dll-s is to import Emgu.CV.Runtime.Windows nuget in visual studio.
If you want to target lowest possible .net framework in order to support widest audience these versions you should target:
- .Net framework 4.6.1 (supported on Windows 7 SP1 or newer; Windows Server 2003 R2 released in 2006 support only up to .net 4.0)
- Emgu.CV 4.2.0.3636 https://www.nuget.org/packages/Emgu.CV/4.2.0.3636#supportedframeworks-body-tab
- Emgu.CV.runtime.windows 4.2.0.3636 https://www.nuget.org/packages/Emgu.CV.runtime.windows/4.2.0.3636#dependencies-body-tab.
  
Native dll-s can be found if you download zip of Emgu.CV.runtime.windows nuget and search for Native or x86/x64 subfolder. 
Many deployment tips can be found at https://www.emgu.com/wiki/index.php/Download_And_Installation#Targeting_.Net_Framework
Windows 7 SP1 download link: https://legacyupdate.net/download-center/download/5842/windows-7-and-windows-server-2008-r2-sp1-kb976932
Support list for .Net Framework versions https://en.wikipedia.org/wiki/.NET_Framework_version_history
In future CSAutoClick and CSImageClick should be merged into single application. 
Code was mostly generated by AI in a time race so don't blame me for poor quality:/ 

## License
- Free and open-source. This application is licensed under GPL V3. Check https://www.gnu.org/licenses/gpl-3.0.txt for more details.
- This application is provided as-is. Use it at your own risk. The author is not responsible for any damages or issues that may arise from using this software. 
- Application relies on Emgu CV. Emgu CV is a .NET wrapper for the OpenCV library. Its license can be found at https://www.gnu.org/licenses/gpl-3.0.txt

## Contact
For support or feedback, please visit www.f4cio.com/CSAutoClick 

---

Thank you for using CSAutoClick! Happy clicking!
