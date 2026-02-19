!define APPNAME "FTdx101 WebApp"
!define COMPANY "MM5AGM"
!define VERSION "0.5.1"
!define INSTALLDIR "$PROGRAMFILES32\${COMPANY}\${APPNAME}"
!define DOTNET_URL "https://dotnet.microsoft.com/en-us/download/dotnet/10.0"

Name "${APPNAME} ${VERSION}"
OutFile "FTdx101_WebApp_Setup.exe"
InstallDir "${INSTALLDIR}"

RequestExecutionLevel admin

Page directory
Page instfiles

Function .onInit
    ; --- Method 1: registry check (most reliable) ---
    ; On 64-bit Windows the x86 registry hive lives under WOW6432Node.
    ; The key is written by the x86 ASP.NET Core runtime installer and holds
    ; the highest installed 10.x.y version as the default value.
    SetRegView 32
    ReadRegStr $0 HKLM \
        "SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.AspNetCore.App" \
        "Version"
    SetRegView lastused
    StrCmp $0 "" try_exe 0        ; empty = not found, fall through
    StrCpy $2 $0 3
    StrCmp $2 "10." dotnet_ok try_exe

    ; --- Method 2: run x86 dotnet.exe and read the version ---
    try_exe:
        nsExec::ExecToStack '"$PROGRAMFILES32\dotnet\dotnet.exe" --version'
        Pop $0
        Pop $1
        StrCmp $0 "0" 0 try_filesystem
        StrCpy $2 $1 3
        StrCmp $2 "10." dotnet_ok try_filesystem

    ; --- Method 3: filesystem check for any 10.x.y runtime directory ---
    try_filesystem:
        FindFirst $0 $1 "$PROGRAMFILES32\dotnet\shared\Microsoft.AspNetCore.App\10*"
        FindClose $0
        StrCmp $1 "" no_dotnet dotnet_ok

    no_dotnet:
        MessageBox MB_YESNO|MB_ICONEXCLAMATION \
            ".NET 10 x86 Runtime is required but was not found on this machine.$\n$\n\
IMPORTANT: You need the x86 (32-bit) version of .NET 10.$\n$\n\
Click Yes to open the download page - choose x86 under Windows.$\n\
Install .NET 10 x86 Runtime, then re-run this installer.$\n$\n\
Click No to cancel." \
            IDNO cancel_install
        ExecShell "open" "${DOTNET_URL}"

    cancel_install:
        Abort

    dotnet_ok:
FunctionEnd

Section "Install"
    SetOutPath "$INSTDIR"

    ; Exclude files that must not be shipped or must not overwrite user data.
    ; The build-installer.ps1 script removes these before NSIS runs;
    ; the /x flags here are a belt-and-braces safety net.
    File /r \
        /x "*.pdb" \
        /x "libman.json" \
        /x "web.config" \
        /x "radio_state.json" \
        /x "appsettings.user.json" \
        "publish\*"

    CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\FTdx101_WebApp.exe"
    CreateDirectory "$SMPROGRAMS\${COMPANY}"
    CreateShortCut "$SMPROGRAMS\${COMPANY}\${APPNAME}.lnk" "$INSTDIR\FTdx101_WebApp.exe"

    WriteUninstaller "$INSTDIR\Uninstall.exe"

    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME} ${VERSION}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$INSTDIR\FTdx101_WebApp.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${COMPANY}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSION}"
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" 65000
SectionEnd

Section "Uninstall"
    Delete "$DESKTOP\${APPNAME}.lnk"
    Delete "$SMPROGRAMS\${COMPANY}\${APPNAME}.lnk"
    RMDir "$SMPROGRAMS\${COMPANY}"
    RMDir /r "$INSTDIR"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd