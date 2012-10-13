@echo off
SET version=%1
IF "%version%"=="" (
ECHO Please enter the version:
SET /p version=
)
ECHO -------------Building solution -----------------------
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild" "%~dp0\..\Squiggle.sln" /p:Configuration=Release /v:M /fl /flp:LogFile="%~dp0\msbuild.log";Verbosity=Normal /nr:false
ECHO -------------Zipping flies ---------------------------
CALL :ZipFolder Squiggle.UI\bin\x86\Release Client
CALL :ZipFolder Squiggle.Bridge\bin\x86\Release Bridge
CALL :ZipFolder Squiggle.Multicast\bin\x86\Release Multicast
CALL :ZipFolder Scripts Scripts
ECHO -------------Done ------------------------------------
GOTO :EOF
:ZipFolder
"%ProgramFiles%\7-Zip\7z" a -r -tzip "%~dp0\Squiggle-%version% %2.zip" "%~dp0\..\%1\*.*"