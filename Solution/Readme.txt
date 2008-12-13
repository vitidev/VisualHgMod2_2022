-------------------------------------------------------

Mercurial source code provider plugin for Visual Studio 2008

Author    : Bernd Schrader
Version   : 1.0.3
State     : beta
Licence   : GNU General Public License (GPL) (v2.0)

-------------------------------------------------------

Get the source code

  clone the repository from http://hg.sharesource.org/visualhg

Usage of the VisualHG build environment requires at least:

  Visual Studio 2008 Standard Edition or Higher
  Visual Studio 2008 SDK
  Votive (Including Wix 3.0) beta for setup

Usage of the VisualHG itself requires at least:

  Mercurial and TortoiseHG installed
  
Experimental Hive Options

	You can start Visual Studio with the experimental hive if you copy
	the following settings to the debug command of the VisualHG project.

	Right-click the VisualHG project and select the "Properties" option.
	Select the Debug tab and put the path to devenv.exe for your appropriate
	version of VS.NET into the "Start External Program" field.
	
	Start External Program:
		C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe
	
	Command line arguments:
		/ranu /rootsuffix Exp /noVSIP /Log
