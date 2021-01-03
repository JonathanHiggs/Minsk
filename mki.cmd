@echo off

REM Vars
set SLNDIR=%cd%

REM Restore + Build
dotnet build "%SLNDIR%\Minsk.Interactive" --nologo -v q || exit /b

REM Run
dotnet run -p "%SLNDIR%\Minsk.Interactive" --no-build -- %*
