' This simple VBScript displays the version of the Vegas
' application in a message box.  Notice that any script that talks to
' Vegas will import the ScriptPortal.Vegas namespace. Please also
' notice that the portion of script code that Vegas will invoke must be
' placed in a subroutine named "Main" which is in a module named
' "MainModule".
'
' Revision Date: Feb. 26, 2004.

imports System.Windows.Forms
imports ScriptPortal.Vegas

Public Module MainModule
Sub Main
  MessageBox.Show("VegasApp.Version: " & VegasApp.Version)
End Sub
End Module
