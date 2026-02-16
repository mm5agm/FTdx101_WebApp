!define APPNAME "FTdx101 WebApp"
!define COMPANY "MM5AGM"
!define VERSION "0.5.0"
!define INSTALLDIR "$PROGRAMFILES\${COMPANY}\${APPNAME}"

Name "${APPNAME} ${VERSION}"
OutFile "${APPNAME}_Setup.exe"
InstallDir "${INSTALLDIR}"

RequestExecutionLevel admin

Page directory
Page instfiles

Section "Install"
  SetOutPath "$INSTDIR"
  File /r "publish\*.*"
  CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\FTdx101_WebApp.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
 ; ExecShell "open" "http://localhost:8080"

  ; Add uninstall information to Windows Apps & Features
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME} ${VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$INSTDIR\FTdx101_WebApp.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${COMPANY}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSION}"
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\${APPNAME}.lnk"
  RMDir /r "$INSTDIR"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd