/**
 * This simple JScript displays the version of the Vegas application
 * in a message box.  Notice that any script that talks to Vegas will
 * import the Sony.Vegas namespace.
 *
 * Revision Date: Feb. 26, 2004.
 **/
import System.Windows.Forms;
import Sony.Vegas;

MessageBox.Show("Vegas.Version: " + Vegas.Version);
