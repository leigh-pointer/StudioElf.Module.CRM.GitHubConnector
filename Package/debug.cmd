@echo off
setlocal
set TargetFramework=net10.0
set ProjectPrefix=StudioElf.Module.CRM.GitHubConnector
set BinDir=%~dp0..\bin\Debug\%TargetFramework%
set OqtaneDir=%~dp0..\..\..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%
set ContentDir=%~dp0..\..\..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\%ProjectPrefix%

:: Copy wwwroot assets (IF EXIST unreliable with wildcards in cmd — xcopy handles no-match gracefully)
xcopy "%~dp0..\wwwroot\GitHubConnector.css" "%ContentDir%\" /Y >nul 2>&1
xcopy "%~dp0..\wwwroot\interop.js" "%ContentDir%\" /Y >nul 2>&1

:: Skip build when called from MSBuild PostBuildPackage
if "%1"=="PostBuild" goto :CopyDll

echo Building %ProjectPrefix%...
dotnet build "%~dp0..\%ProjectPrefix%.csproj" -c Debug

:CopyDll
if exist "%BinDir%\%ProjectPrefix%.Oqtane.dll" copy /Y "%BinDir%\%ProjectPrefix%.Oqtane.dll" "%OqtaneDir%\" >nul
if exist "%BinDir%\%ProjectPrefix%.Oqtane.pdb" copy /Y "%BinDir%\%ProjectPrefix%.Oqtane.pdb" "%OqtaneDir%\" >nul

echo Done. DLL and wwwroot files copied.
if not "%1"=="PostBuild" pause
endlocal