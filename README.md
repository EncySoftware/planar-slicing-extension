# planar-slicing-extension
Extension that allows you to calculate toolpath using the CuraEngine which is the part of Ultimaker Cura (https://ultimaker.com/)

This plugin is designed to integrate an operation based on Ultimaker Cura.

Installation instructions:
1) Select the latest version and download the "planar-slicing-extension.zip" from the Assets section, extract the archive.
https://github.com/EncySoftware/planar-slicing-extension/releases

2) Download and install Ultimaker Cura (https://github.com/Ultimaker/Cura/releases/download/5.7.1/UltiMaker-Cura-5.7.1-win64-X64.exe)
Recommended version: 5.7.1

3) Launch the CAM system and add the xml operations:
☰ - Operations manager - Include unit - Select "CuraEngineToolpath_ExtOp.xml" from extracted archive - Close

4) Install the operation extension:
Settings - Extensions - Install - Select "CuraEngineOperation.Extensions.json" from extracted archive - OK

5) Restart the CAM system

6) The "Planar slicing" operation will appear in the "Additive" section
