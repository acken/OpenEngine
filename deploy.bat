@echo off

SET DIR=%~d0%~p0%
SET SOURCEDIR=%DIR%
SET BINARYDIR="%DIR%build_output"
SET DEPLOYDIR="%DIR%ReleaseBinaries"

IF NOT EXIST %BINARYDIR% (
  mkdir %BINARYDIR%
) ELSE (
  del %BINARYDIR%\* /Q
)

%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %SOURCEDIR%\OpenEngine.sln /property:OutDir=%BINARYDIR%\;Configuration=Release /target:rebuild

IF NOT EXIST %DEPLOYDIR% (
  mkdir %DEPLOYDIR%
) ELSE (
  del %DEPLOYDIR%\* /Q
)

IF NOT EXIST %DEPLOYDIR%\Installer (
  mkdir %DEPLOYDIR%\Installer
) ELSE (
  del %DEPLOYDIR%\Installer\* /Q
)

copy %BINARYDIR%\OpenEngine.Core.dll %DEPLOYDIR%\OpenEngine.Core.dll
copy %BINARYDIR%\OpenEngine.Core.pdb %DEPLOYDIR%\OpenEngine.Core.pdb
copy %BINARYDIR%\OpenEngine.Console.exe %DEPLOYDIR%\OpenEngine.Console.exe
copy %BINARYDIR%\OpenEngine.Console.pdb %DEPLOYDIR%\OpenEngine.Console.pdb
copy %BINARYDIR%\OpenEngine.Service.exe %DEPLOYDIR%\OpenEngine.Service.exe
copy %BINARYDIR%\OpenEngine.Service.pdb %DEPLOYDIR%\OpenEngine.Service.pdb
copy %BINARYDIR%\style.css %DEPLOYDIR%\style.css

copy %BINARYDIR%\Configuration\* %DEPLOYDIR%\
