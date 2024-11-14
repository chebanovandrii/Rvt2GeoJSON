# GeoJSON
Revit Plugin to export Revit model to GeoJSON file which can be used in various map apps

# Tech Stacks
  C#

# Keyword
	Revit Plugin GeoJSON

# Environment
- This plugin is tested on Revit 2019~2025.

# Build
- Using Visual Studio <br/>
Open the appropriate Project solution using Visual Studio 2022 and Build.
Visual Studio will automatically copy Add-In file to %ProgramData%\Autodesk\Revit\Addins\ directory.
- Manual<br/>
Copy GeoJSON.dll to your own directory<br/>
Copy AddinFiles\GeoJSON.addin file to %ProgramData%\Autodesk\Revit\Addins\$(RevitVersion) directory and change "DLL File URL" to the path of GeoJSON.dll file.

# Usage
- Run Revit & Open Model to export GeoJSON
- Click "Architexor" ribbon / Export to GeoJSON button
- Adjust Longitude/Latitude/Angle if needs. <br>
This Longitude/Latitude indicates the Base Point of the project.
- Check/Uncheck categories as your need. <br>
The checked list will be saved for user convenience.

# Note
This Revit addin automatically exports the desired elements from a revit project into seperated GeoJSON files per level.
The project base point will acts the center of the coordinate system and be positioned by the user input or default setting of the project location on the Map.
User should make sure the project base point matches with the site location that represents the coordination input on the map.

# Contact
If you find any issues, contact me via chebanovandrii@gmail.com