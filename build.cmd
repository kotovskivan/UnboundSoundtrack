@echo off
setlocal
rem Build Unbound Soundtrack.exe with the C# compiler of .NET Framework 4.8 (built into Windows 10/11 - nothing to install).
rem Usage:  build.cmd "<Frosty Mod Manager folder>" ["<folder with the EA encoder files>"]
rem Result: out\Unbound Soundtrack\  (same layout as the release zip)

set "FMM=%~1"
set "ENC=%~2"
if "%FMM%"=="" (
  echo Usage: build.cmd "path\to\Frosty Mod Manager" ["path\to\encoder files"]
  exit /b 1
)
if not exist "%FMM%\FrostySdk.dll" (
  echo FrostySdk.dll not found in "%FMM%" - point to HarGabt's Frosty Mod Manager folder.
  exit /b 1
)
set "NET=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319"
set "WPF=%NET%\WPF"
set "HERE=%~dp0"
set "OUT=%HERE%out\Unbound Soundtrack"
mkdir "%OUT%\app\frosty\Profiles" "%OUT%\app\frosty\ThirdParty" "%OUT%\app\web" "%OUT%\app\encoder" 2>nul

"%NET%\csc.exe" -nologo -optimize+ -platform:x64 -target:winexe -win32icon:"%HERE%res\app.ico" ^
  -out:"%OUT%\Unbound Soundtrack.exe" ^
  -r:"%FMM%\FrostySdk.dll" -r:"%FMM%\FrostyCore.dll" ^
  -r:Microsoft.CSharp.dll -r:System.Web.Extensions.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -r:System.Management.dll ^
  -r:"%WPF%\PresentationCore.dll" -r:"%WPF%\PresentationFramework.dll" -r:"%WPF%\WindowsBase.dll" -r:System.Xaml.dll ^
  "%HERE%src\*.cs"
if errorlevel 1 exit /b 1

rem Frosty Toolsuite libraries (GPL v3) - to read the game and write the mod
for %%f in (FrostySdk.dll FrostyCore.dll FrostyHash.dll FrostyModSupport.dll FrostyControls.dll) do copy /y "%FMM%\%%f" "%OUT%\app\frosty\" >nul
copy /y "%FMM%\Profiles\NFSUnboundSDK.dll" "%OUT%\app\frosty\Profiles\" >nul
xcopy /y /e /q "%FMM%\ThirdParty" "%OUT%\app\frosty\ThirdParty\" >nul
rem the window (HTML/JS/CSS) and docs
xcopy /y /e /q "%HERE%web" "%OUT%\app\web\" >nul
copy /y "%HERE%docs\*" "%OUT%\" >nul
rem EA audio encoder (optional here): Tool.exe + eaxmaenc.dll + libatrac9.dll + libmp3lame.dll + msvcr120.dll
if not "%ENC%"=="" for %%f in (Tool.exe eaxmaenc.dll libatrac9.dll libmp3lame.dll msvcr120.dll) do copy /y "%ENC%\%%f" "%OUT%\app\encoder\" >nul

echo Built: %OUT%
