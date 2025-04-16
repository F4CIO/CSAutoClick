# ReadMe.txt for CSImageClick 1.0

## Overview
CSImageClick is a Windows application designed to automate mouse clicks based on image recognition. It continuously scans the screen for images found in same folder and performs clicks when matches are found. This tool is particularly useful for repetitive tasks that require clicking on specific UI elements.

## Features
- Image Recognition: Automatically detects images on the screen and clicks on them.
- Configurable Settings: Customize the behavior of the application through a configuration file.
- System Tray Integration: Run the application in the background with a system tray icon for easy access.
- Debug Logging: Optionally enable debug logs to track the application's actions and errors.
- Run on Startup: Option to configure the application to start automatically with Windows.
- Scale other than 100% in Display settings is NOT supported.

## Requirements
- Windows 7 Operating System or newer
- .NET Framework 3.5 (should already be included in Windows 7 and newer )
- Scale of 100% in Display settings for the screen where you intend to use it

## Installation
1. Download the latest release of CSImageClick.
2. Extract the contents of the zip file to a desired location on your computer.
3. Run CSImageClick.exe 
4. Insure you see 'A' icon in your tray near clock. Windows may hide some tray icons. Google for 'how to unhide tray icon in windows' for advice.
5. Now you are ready to configure your auto-click work -see Usage section below for that.

## Usage
1. If the app is running you right-click its 'A' tray icon to access the context menu and exit app so its not running while you configure it.
2. Grab from screen popup button, image or other UI elements that you want to auto-click BUT you must write down its exact top-left corner location relative to its main parent window or screen. Here how you can do it:
- One good tool for taking screenshoots or part of screen is Greenshoot (https://getgreenshot.org/downloads). Install it.
- Right click its tray icon and select capture full screen -> your screen. 
- In popup select MS Paint to open resulting image there.
- Use rectangular selection tool. Position mouse on top-left corner of image/control you want this app to auto-detect and click on. Write down coordinates but don't move mouse. Now press mouse button and expand selection. Once you selected target image/control click on Crop tool.
3. Save that little image in same folder where .exe file of this app is but you must name file like in this example myAnnoyingButton.X1230.Y780.rightClick.png where numbers are ones you previousely written down. rightClick is optional keyword to tell which mouse button to use.
4. Run CSImageClick.exe and by right-clicking its tray icon insure its Enabled.
5. Observe tray icon colors -it should flash green and yellow when image is detected. 
6. If nothing is recognized and auto-clicked see Troubleshooting and Configuration sections.
7. OPTIONAL: by default click is done at the center of image (small one after cropping). You can click at arbitary location by specifying offset (relative to X and Y in file name). To specify horizontal offset of 50 and vertical of -10 syntax would be: myAnnoyingButton.X1230.Y780.OX50.OY-10.rightClick.png

## Configuration
The application uses a configuration file named `CSImageClick.ini`. This file is created automatically on the first run if it does not exist. You can manually edit this file to customize the following settings:

- `Enabled`: Set to `true` to enable the auto-click feature, or `false` to disable it.
- `CheckEveryXSeconds`: The interval (in seconds) at which the application checks for images on the screen.
- `PrecisionPercent`: We compare screen area with your template file pixel by pixel. Most of the time its not needed to compare all to recognize match. 25% can work well for many use cases.
- `AllowedDisplacementInPixels`: If image at expected location on screen doesn't match image from file we can tilt expexted area few pixels and check again. Use 1, 0 or extreemly small number here not to slow down screen scan.
- `ShowDebugMarkers`: Set to `true` to display debug markers on the screen. Not working yet.
- `DebugLogsEnabled`: Set to `true` to enable logging of debug information.
- `FlashTrayIconDuringScan`: Set to `true` to enable tray icon flashing from dark to bright green during screen scan.
- `FlashTrayIconDuringAction`: Set to `true` to enable tray icon flashing to yellow when image is detected on screen.
- `RestartEveryXMinutes`: Allow application to replenish itself by auto-restarting ocasionally. Not mandatory and can be switched off by setting to 0.

### Example `CSImageClick.ini` Configuration
Enabled=true
CheckEveryXSeconds=5
PrecisionPercent=50
AllowedDisplacementInPixels=1
ShowDebugMarkers=true
DebugLogsEnabled=false
FlashTrayIconDuringScan=true
FlashTrayIconDuringAction=true
RestartEveryXMinutes=10080

## Troubleshooting
- If the application does not detect images on one screen but works on the other insure you are using 100% scale for that screen in Windows Display settings. Other scale is NOT supported.
- If the application does not detect images, ensure that the images are in the correct format and named appropriately.
- Check the log file CSImageClick.log for any error messages that may indicate issues with image processing or configuration.
- In .ini file set DebugLogsEnabled=true to see more detailed logs in .log file but remember to switch it of later to releave your system resources.

## Logging
The application logs its activities to a file named `CSImageClick.log`. This log file will be created in the same directory as the application. If debug logging is enabled, additional information will be recorded.

## For Developers
Code was mostly generated by AI in a time race so don't blame me for poor quality:/ 

## License
- This application is free, open-source and licensed under MIT license.

## Contact
For support or feedback, please visit www.f4cio.com

---

Thank you for using CSImageClick! Happy clicking!