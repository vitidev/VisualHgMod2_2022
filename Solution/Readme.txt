-------------------------------------------------------

Mercurial source code provider plugin for Visual Studio 2008

Author    : Bernd Schrader
Version   : 1.0.5
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

2005 Support (for release build only)
-------------------------------------
PkgCmd.vsct is not supported for VS2005, so we have to compile the project
one time with the following 2008 ItemGroup settings in VisualHG.csproj to
build the setup files VisualHG-2005.wxi and VisualHG-2008.wxi.

    <Reference Include="Microsoft.VisualStudio.OLE.Interop, Version=7.1.40304.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
    <Reference Include="Microsoft.VisualStudio.Shell.9.0" />
    <Reference Include="Microsoft.VisualStudio.Shell.Interop, Version=7.1.40304.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
    <Reference Include="Microsoft.VisualStudio.Shell.Interop.8.0, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
    <Reference Include="Microsoft.VisualStudio.Shell.Interop.9.0, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />

after that we have replace and rebuild the entries for 2005 compatibillity. 
Note: Now we get the bild error 'No registrationdata ...' which we ignore so far.

	<Reference Include="Microsoft.VisualStudio.OLE.Interop, Version=7.1.40304.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
	<Reference Include="Microsoft.VisualStudio.Shell" />
	<Reference Include="Microsoft.VisualStudio.Shell.Interop, Version=7.1.40304.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
	<Reference Include="Microsoft.VisualStudio.Shell.Interop.8.0, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
