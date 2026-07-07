@echo off
set TargetFramework=net10.0
set ProjectPrefix=StudioElf.Module.CRM.GitHubConnector
echo Building CRM GitHub Enterprise Connector extension...
dotnet build "..\%ProjectPrefix%.csproj" -c Debug
IF EXIST "..\bin\Debug\%TargetFramework%\%ProjectPrefix%.Oqtane.dll" (
    XCOPY "..\bin\Debug\%TargetFramework%\%ProjectPrefix%.Oqtane.dll" "..\..\..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\\" /Y
)
IF EXIST "..\bin\Debug\%TargetFramework%\%ProjectPrefix%.pdb" (
    XCOPY "..\bin\Debug\%TargetFramework%\%ProjectPrefix%.pdb" "..\..\..\..\oqtane.framework\Oqtane.Server\bin\Debug\%TargetFramework%\\" /Y
)
echo Done. DLL copied to Oqtane Server bin.
pause
