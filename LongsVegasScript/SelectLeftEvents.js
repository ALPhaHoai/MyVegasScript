/*
* Program : SelectLeftEvents
* Author : ALPhaHoai
* Date : September 26, 2016
* Location : Ha Noi, Viet Nam
* Script : Sony Vegas Pro 13
* Decription : Script select left events in selected tracks
*/
import ScriptPortal.Vegas;
import System.Windows.Forms;

SelectLeftEvents();

function SelectLeftEvents (){	 //ham nay chon nhung events nam ben trai cua con tro chuot
								 // chi chon nhung event thuoc track dc chon
try{
	for (var track in Vegas.Project.Tracks) 
	{
		if( !track.Selected) continue;	// bo qua nhung track khong dc chon
		for (var evnt in track.Events) 
		{
			if (evnt.Start < Vegas.Cursor ) evnt.Selected = true ;
			// Vegas.Cursor ( Timecode) : vi tri dang dc chon
		}
	}
	}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

