!define APPNAME "FTdx101 WebApp"
!define COMPANY "MM5AGM"
!define VERSION "0.5.0"
!define INSTALLDIR "$PROGRAMFILES64\${COMPANY}\${APPNAME}"
!define DOTNET_URL "https://dotnet.microsoft.com/en-us/download/dotnet/10.0"

Name "${APPNAME} ${VERSION}"
OutFile "FTdx101_WebApp_Setup_${VERSION}.exe"
InstallDir "${INSTALLDIR}"

RequestExecutionLevel admin

Page directory
Page instfiles

; -------------------------------------------------------------------------
; .NET 10 detection - two methods for reliability:
;   1. Run dotnet.exe directly and check the version string starts with "10."
;   2. Filesystem fallback - look for any 10.x.y directory in the shared folder
; -------------------------------------------------------------------------
Function .onInit

    ; --- Method 1: run dotnet.exe and read the version ---
    nsExec::ExecToStack '"$PROGRAMFILES64\dotnet\dotnet.exe" --version'
    Pop $0   ; exit code (string "0" = success)
    Pop $1   ; stdout, e.g. "10.0.103\r\n"

    StrCmp $0 "0" 0 try_filesystem   ; non-zero exit = dotnet not found/broken
    StrCpy $2 $1 3                   ; grab first 3 chars, e.g. "10."
    StrCmp $2 "10." dotnet_ok try_filesystem

    ; --- Method 2: filesystem check for any 10.x.y runtime directory ---
    try_filesystem:
        FindFirst $0 $1 "$PROGRAMFILES64\dotnet\shared\Microsoft.AspNetCore.App\10*"
        FindClose $0
        StrCmp $1 "" no_dotnet dotnet_ok

    no_dotnet:
        MessageBox MB_YESNO|MB_ICONEXCLAMATION \
            ".NET 10 Runtime is required but was not found on this machine.$\n$\n\
Click Yes to open the Microsoft download page.$\n\
Install .NET 10 Runtime, then re-run this installer.$\n$\n\
Click No to cancel." \
            IDNO cancel_install
        ExecShell "open" "${DOTNET_URL}"

    cancel_install:
        Abort

    dotnet_ok:
FunctionEnd

Section "Install"
    SetOutPath "$INSTDIR"
    File /r "publish\*.*"

    ; Desktop and Start Menu shortcuts
    CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\FTdx101_WebApp.exe"
    CreateDirectory "$SMPROGRAMS\${COMPANY}"
    CreateShortCut "$SMPROGRAMS\${COMPANY}\${APPNAME}.lnk" "$INSTDIR\FTdx101_WebApp.exe"

    ; Uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    ; Add/Remove Programs entry
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