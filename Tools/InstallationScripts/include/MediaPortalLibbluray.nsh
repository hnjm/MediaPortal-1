#region Copyright (C) 2005-2020 Team MediaPortal
/*
// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#**********************************************************************************************************#
#
#                         MediaPortal's Current Version
#
#    This file is included in:
#       - MediaPortal setup
#       
#       
#
#**********************************************************************************************************#

# Correspond to nuget Package

#**********************************************************************************************************#
!include ${git_InstallScripts}\include\CompileTimeIfFileExist.nsh

!macro macro_build_libbluray_jar
!system 'ant -f ${LibblurayJAR} -Dsrc_awt=:java-j2se' = 0
!macroend 

!macro macro_check_libbluray
Section libbluray
!ifndef FORCE_BUILD_Libbluray_jar

	; Order of detection 
	; 1: Forced nuget package to use with libbluray_version.txt in nuget packages folder
	; 2: Use Nuget version set in BDReader project
	; 3: Build libbluray from git submodule directly

	;${!IfExist} "${git_NugetPackages}\libbluray.version.txt"
	!insertmacro CompileTimeIfFileExist "${git_NugetPackages}\libbluray_version.txt" libbluray_version.txt_is_present
	!ifdef libbluray_version.txt_is_present
		# file must contain text as example: #define BLURAY_VERSION_STRING "1.1.2"
		!searchparse /file "${git_NugetPackages}\libbluray.version.txt" `#define BLURAY_VERSION_STRING "` GIT_LIBBLURAY_VERSION `"`
		!echo "BUILD MESSAGE: libbluray.version.txt point to ${GIT_LIBBLURAY_VERSION} and found in ${git_NugetPackages}"
		Goto Libbluray_NugetCheck_label
	!endif

	# Give libbluray Nuget package from package.config BDReader project
	# check if file exist 
	!insertmacro CompileTimeIfFileExist "${git_DirectShowFilters}\BDReader\packages.config" BDReader_packages.config_is_present
	!ifdef BDReader_packages.config_is_present
		!searchparse /file "${git_DirectShowFilters}\BDReader\packages.config" `<package id="MP_Libbluray" version="` GIT_LIBBLURAY_VERSION `"`
		!echo "BUILD MESSAGE: Libbluray version read from BDReader project : ${GIT_LIBBLURAY_VERSION}"
		Goto Libbluray_NugetCheck_label
	!endif

	# Give libbluray version from bluray-version file available in libbluray submodule
	# check if file bluray-version.H exist 
	;${!IfExist} "${git_Libbluray}\src\libbluray\bluray-version.h"
	!insertmacro CompileTimeIfFileExist "${git_Libbluray}\src\libbluray\bluray-version.h" bluray-version.h_is_present
	!ifdef bluray-version.h_is_present
		!searchparse /file "${git_Libbluray}\src\libbluray\bluray-version.h" `#define BLURAY_VERSION_STRING "` GIT_LIBBLURAY_VERSION `"`
		!echo "BUILD MESSAGE: Local Git Libbluray Version : ${GIT_LIBBLURAY_VERSION}"
		Goto Libbluray_NugetCheck_label
	
	!else 
		!echo "BUILD MESSAGE: Bluray-version.h is missing"
		!define libbluray_is_missing
	!endif
	

	# Check if Nuget package is the same version than Git Libbluray submodule otherwise build lib with MP

	Libbluray_NugetCheck_label:

    ; ${!IfExist} "${git_NugetPackages}\Libbluray.${GIT_LIBBLURAY_VERSION}"
    !insertmacro CompileTimeIfFileExist "$${git_NugetPackages}\Libbluray.${GIT_LIBBLURAY_VERSION}" libbluray_nuget_is_present

	!ifdef libbluray_nuget_is_present
			!echo "BUILD MESSAGE: Libbluray Nuget Package is present and match to libbluray git version"
			!define Libbluray_use_Nuget_JAR "${git_NugetPackages}\Libbluray.${GIT_LIBBLURAY_VERSION}"
	!else 
			!echo "BUILD MESSAGE: Libbluray Nuget Package missing OR don't match to local git libbluray version OR libbluray is missing"
			!echo "BUILD MESSAGE: Libbluray will be build using MSbuild"
			!insertmacro macro_Build_libbluray_jar
	!endif
	
!else
	!echo "BUILD MESSAGE: Libbluray build jar forced flag by user, only using local git submodule"
	!searchparse /file "${git_Libbluray}\src\libbluray\bluray-version.h" `#define BLURAY_VERSION_STRING "` GIT_LIBBLURAY_VERSION `"`
	!echo "BUILD MESSAGE: Local Git Libbluray Version : ${GIT_LIBBLURAY_VERSION}"
	!insertmacro macro_Build_libbluray_jar
!endif
SectionEnd
!macroend