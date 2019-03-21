' This most simple VBScript displays the version of the Vegas
' application in a message box.  Notice that any script that talks to
' Vegas will import the SonicFoundry.Vegas.Script namespace. Please also
' notice that the portion of script code that Vegas will invoke must be
' placed in a subroutine named "Main" which is in a module named
' "MainModule".

imports System.Windows.Forms
imports SonicFoundry.Vegas.Script

Public Module MainModule
Sub Main
  MessageBox.Show("Vegas.Version: " & VegasApp.Version)
End Sub
End Module
