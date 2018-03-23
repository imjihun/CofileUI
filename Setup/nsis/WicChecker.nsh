!macro CheckWic
  ${If} ${RunningX64}
;    messageBox MB_OK "64"
    File "..\Prerequisites\wic_x64_enu.exe"
    ExecWait '"$INSTDIR\Prerequisites\wic_x64_enu.exe"'
  ${Else}
;    messageBox MB_OK "32"
    File "..\Prerequisites\wic_x86_enu.exe"
    ExecWait '"$INSTDIR\Prerequisites\wic_x86_enu.exe"'
  ${EndIf}
  
;  ClearErrors
;  ReadRegStr $0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\WIC" 0
;  ${If} ${Errors}
;    MessageBox MB_OK "Value not found"
;  ${Else}
;    ${IF} $0 == ""
;      MESSAGEBOX MB_OK "NUL exists and it's empty"
;    ${ELSE}
;;     WriteRegStr HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion\Ports" "NUL:" ""
;      MESSAGEBOX MB_OK "value exists"
;    ${ENDIF}
;  ${EndIf}
!macroend