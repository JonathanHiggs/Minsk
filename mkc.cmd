@echo off

REM Vars
set SLNDIR=%cd%

REM Restore + Build
dotnet build "%SLNDIR%\Minsk.Compiler" --nologo -v q || exit /b

REM Run
dotnet run -p "%SLNDIR%\Minsk.Compiler" --no-build -- %*
