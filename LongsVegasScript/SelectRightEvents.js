/*
* Author : ALPhaHoai
* Date : September 23, 2016
* Location : Ha Noi City, Viet Nam.
* From : HUST
*/
import System.IO; 		// Khai bao cac thu vien su dung, import cac class
import System;
import ScriptPortal.Vegas;
import System.Windows.Forms;
import Microsoft.Win32;
import Sony.Vegas.Script;
import System.Drawing.SystemColors;

SelectRightEvents();

function SelectRightEvents (){	 //ham nay chon nhung events nam ben phai cua con tro chuot
								 // chi chon nhung event thuoc track dc chon
try{
	for (var track in Vegas.Project.Tracks)
	{
		if( !track.Selected) continue;	// bo qua nhung track khong dc chon
		for (var evnt in track.Events)
		{
			if (evnt.End > Vegas.Cursor ) evnt.Selected = true ;
			// Vegas.Cursor ( Timecode) : vi tri dang dc chon
		}
	}
	}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

