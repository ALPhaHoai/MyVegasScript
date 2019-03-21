/**
 * This simple C# script displays the version of the Vegas application
 * in a message box.  Notice that any script that talks to Vegas will
 * use the ScriptPortal.Vegas namespace.
 *
 * Revision Date: Oct. 23, 2006.
 **/
using System;
using System.Windows.Forms;
using Sony.Vegas;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
        MessageBox.Show(vegas.Version);
    }
}
