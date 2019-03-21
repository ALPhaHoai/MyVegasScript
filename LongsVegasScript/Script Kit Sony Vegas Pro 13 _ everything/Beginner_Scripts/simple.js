/**
 * This most simple JScript displays the version of the Vegas
 * application in a message box.  Notice that any script that talks to
 * Vegas will import the SonicFoundry.Vegas.Script namespace.
 **/
import System.Windows.Forms;
import SonicFoundry.Vegas.Script;

MessageBox.Show("Vegas.Version: " + Vegas.Version);
