/** 
 * Program:     AutoCutTenSecondFromVideos.js
 * Author: ALPhaHoai
 * 
 * Date: September 22, 2016
 *
 **/
import ScriptPortal.Vegas;
import System.Windows.Forms;
import Microsoft.Win32;

var TwentySeconds: Timecode = new Timecode("00:00:20.00");
var ThirtySeconds: Timecode = new Timecode("00:00:30.00");
var beginSecond: Timecode = new Timecode("00:00:00.00");

try {
    // step through all selected video events:
    for (var track in Vegas.Project.Tracks) {
        if (!track.Selected) continue;
        var tracktime = beginSecond;
        for (var evnt in track.Events) {
            tracktime = tracktime + ThirtySeconds;
            evnt.Split(tracktime);
            tracktime = beginSecond;
            evnt.AdjustStartLength(evnt.Start + TwentySeconds, evnt.Length - TwentySeconds, false);

        }
    }
    DeleteGap();
} catch (errorMsg) {
    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}



// Ham can dung
function DeleteGap() {
    try {
        // step through all selected video events:
        var FirstTrack: Track = Vegas.Project.Tracks.Item(0);

        // step through all selected video events:
        for (var track in Vegas.Project.Tracks) {
            if (!track.Selected) continue;
            var tracktime = new Timecode(0);
            for (var evnt in track.Events) {
                //evnt.AdjustStartLength(tracktime,evnt.Length,true);
                evnt.Start = tracktime;
                tracktime = tracktime + evnt.Length;
            }
        }
    } catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

}