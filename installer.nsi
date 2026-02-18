!define APPNAME "FTdx101 WebApp"
!define COMPANY "MM5AGM"
!define VERSION "0.5.0"
!define INSTALLDIR "$PROGRAMFILES32\${COMPANY}\${APPNAME}"
!define DOTNET_URL "https://dotnet.microsoft.com/en-us/download/dotnet/10.0"

Name "${APPNAME} ${VERSION}"
OutFile "FTdx101_WebApp_Setup_${VERSION}.exe"
InstallDir "${INSTALLDIR}"

RequestExecutionLevel admin

Page directory
Page instfiles

Function .onInit
    ; --- Method 1: run x86 dotnet.exe and read the version ---
    nsExec::ExecToStack '"$PROGRAMFILES32\dotnet\dotnet.exe" --version'
    Pop $0
    Pop $1
    StrCmp $0 "0" 0 try_filesystem
    StrCpy $2 $1 3
    StrCmp $2 "10." dotnet_ok try_filesystem

    ; --- Method 2: filesystem check for any 10.x.y runtime directory ---
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
    File /r /x "publish" "publish\*.*"

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
SectionEnd

Section "Uninstall"
    Delete "$DESKTOP\${APPNAME}.lnk"
    Delete "$SMPROGRAMS\${COMPANY}\${APPNAME}.lnk"
    RMDir "$SMPROGRAMS\${COMPANY}"
    RMDir /r "$INSTDIR"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd