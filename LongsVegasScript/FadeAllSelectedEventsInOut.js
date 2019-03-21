

import ScriptPortal.Vegas;
import System.Windows.Forms;
import Microsoft.Win32;



try
{
	
	for (var track in Vegas.Project.Tracks) {
		if( track.Selected){
			for (var evnt in track.Events) {
				if(evnt.Selected == true)
				{
				evnt.FadeIn.Length = new Timecode(500);
				evnt.FadeOut.Length = new Timecode(500);
				}
			}
		}
	}
}

catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
