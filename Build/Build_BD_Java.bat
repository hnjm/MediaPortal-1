if not defined GIT_ROOT call .\BuildInit.bat

if not defined ant_home set ant_home=%NugetPackages%\ANT.1.10.7\tools

pushd %GIT_ROOT%\libbluray\src\libbluray\bdj
call %ant_home%\bin\ant -Dsrc_awt=:java-j2se
popd