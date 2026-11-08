@echo off
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe

set GAC=%WINDIR%\Microsoft.NET\assembly\GAC_MSIL
set UIA=/r:"%GAC%\UIAutomationClient\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationClient.dll" /r:"%GAC%\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll" /r:"%GAC%\WindowsBase\v4.0_4.0.0.0__31bf3856ad364e35\WindowsBase.dll"

"%CSC%" /nologo /target:winexe /optimize+ /codepage:65001 ^
 /win32icon:"%~dp0app.ico" ^
 /out:"%~dp0ShuangpinHint.exe" ^
 /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll %UIA% ^
 "%~dp0Program.cs"
if errorlevel 1 (
  echo Build FAILED
  pause
  exit /b 1
)
echo Build OK -^> %~dp0ShuangpinHint.exe
