/** 
 * Program:     AutoCutTenSecondFromVideos.js
 * Author: ALPhaHoai
 * 
 * Date: September 22, 2016
 *
 **/ 

import ScriptPortal.Vegas;
import System.Windows.Forms;

var TwentySeconds: Timecode = new Timecode("00:00:20.00");
var ThirtySeconds: Timecode = new Timecode("00:00:30.00");
var beginSecond : Timecode = new Timecode("00:00:00.00");

try
{
	// step through all selected video events:
	for (var track in Vegas.Project.Tracks) 
	{
		if( !track.Selected) continue;
		var tracktime = beginSecond;
		for (var evnt in track.Events) 
		{
			tracktime = tracktime + ThirtySeconds;
			evnt.Split(tracktime );
			tracktime = beginSecond;
			evnt.AdjustStartLength(evnt.Start + TwentySeconds, evnt.Length - TwentySeconds, false);
			
		}
	}
}
catch (errorMsg)
{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
