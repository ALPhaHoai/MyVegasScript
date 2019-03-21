//Script nay lam hai dau event loi ra 1s

import System; 
import System.IO; 
import System.Windows.Forms; 
import Sony.Vegas; 

var trimAmt = new Timecode("00:00:01:00"); 
try { //Go through the list of Tracks 
  var trackEnum = new Enumerator(Vegas.Project.Tracks); 
  while (!trackEnum.atEnd()) { 
  var track : Track = Track(trackEnum.item()); 

    //Go through the list of Events 
    var eventEnum = new Enumerator(track.Events); 
    while (!eventEnum.atEnd()) { 
    var evnt : TrackEvent = TrackEvent(eventEnum.item()); 
	var TwoSec : Timecode = new Timecode("00:00:02:00");
	var OneSec : Timecode = new Timecode("00:00:01:00");
      if (evnt.Selected) 
	  { 
		//Get current take offset 
		var tke = evnt.ActiveTake; 
		var tkeoffset = tke.Offset; 
		tkeoffset = tkeoffset - trimAmt; 
        evnt.End += TwoSec; 
		tke.Offset = tkeoffset; 
		evnt.Start -= OneSec;
      } 
      
	  eventEnum.moveNext(); 
    } 
    trackEnum.moveNext(); 
  } 


} catch (e) { 
    MessageBox.Show(e); 
} 