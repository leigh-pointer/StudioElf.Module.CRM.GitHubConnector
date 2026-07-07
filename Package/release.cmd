@echo off
if "%1"=="" (set TargetFramework=net10.0) else (set TargetFramework=%1)
if "%2"=="" (for %%f in (*.nuspec) do set NuspecFile=%%f) else (set NuspecFile=%2.nuspec)
del "*.nupkg"
"..\..\..\..\oqtane.framework\oqtane.package\FixProps.exe"
set ProjectName=%NuspecFile:.nuspec=%
"..\..\..\..\oqtane.framework\oqtane.package\nuget.exe" pack %NuspecFile% -Properties targetframework=%TargetFramework%;projectname=%ProjectName%
XCOPY "*.nupkg" "..\..\..\..\oqtane.framework\Oqtane.Server\Packages\" /Y
