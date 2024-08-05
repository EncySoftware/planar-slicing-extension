# planar-slicing-extension
Extension that allows you to calculate toolpath using the CuraEngine which is the part of Ultimaker Cura (https://ultimaker.com/)

This plugin is designed to integrate an operation based on Ultimaker Cura.

Instructions:

1) Download and install Ultimaker Cura (https://github.com/Ultimaker/Cura/releases/download/5.7.1/UltiMaker-Cura-5.7.1-win64-X64.exe)
Recommended version: 5.7.1

2) Launch the CAM system and add the xml operations:
☰ - Operations manager - Include unit - Select "CuraEngineToolpath_ExtOp.xml" - Close

3) Install the operation extension:
Settings - Extensions - Install - Select "CuraEngineOperation.Extensions.json" - OK

4) Restart the CAM system

5) The "Planar slicing" operation will appear in the "Additive" section
