# planar-slicing-extension
Extension that allows you to calculate toolpath using the CuraEngine which is the part of Ultimaker Cura (https://ultimaker.com/)

This plugin is designed to integrate an operation based on Ultimaker Cura.

## Installation instructions:
If this extension was previously installed, it is recommended to uninstall it first. See the uninstallation instructions below.
1) Download and install Ultimaker Cura (https://github.com/Ultimaker/Cura/releases/download/5.7.1/UltiMaker-Cura-5.7.1-win64-X64.exe)
Recommended version: 5.7.1

2) Select the latest version and download the "planar-slicing-extension.dext" from the Assets section.
https://github.com/EncySoftware/planar-slicing-extension/releases 

3) Download and install recommended ENCY version (as indicated in the downloaded extension release description) 
   
4) Double click on the downloaded "planar-slicing-extension.dext" file for automatic installation (ENCY must be closed during the installation)

5) Launch ENCY

6) The "Planar slicing" operation will appear in the "Additive" section

## Manual extension installation instructions:
If this extension was previously installed, it is recommended to uninstall it first. See the uninstallation instructions below.

You can also install the “planar-slicing-extension” through ENCY manually:
1) Launch ENCY
   
2) Open settings
   
3) Select the “Extensions” tab
   
4) Click “Install” and, in the window that appears, select the downloaded file “planar-slicing-extension.dext”

5) Restart ENCY

## Uninstallation instructions:
1) Launch ENCY

2) Open settings

3) Select the “Extensions” tab

4) Select “Extension for Planar slicing operation based on Cura” from the list

5) Click “Uninstall”
   
6) Restart ENCY

## Localization:
The operation language is based on ENCY language, provided that this language is available in Cura. You can also manually add translations by creating a file in the “UserLocalization” folder with the extension “*.po” and the language name in the BCP 47 Code format (example en-US.po). The “UserLocalization” folder is located at the path where Planar slicing extension is installed. You can find the path in ENCY settings, in the "Extensions" tab, by selecting the Planar slicing extension.
