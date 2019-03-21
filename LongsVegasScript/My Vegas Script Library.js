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

/* Vong for hoan hao
var Selected_Track : Track = FindSelectedTrack();
var This_Event : TrackEvent = Selected_Track.Events.Item(4);

This_Event.Selected = true;
*/

// Than chuong trinh, ham main

var linh : Timecode = new Timecode("00:00:01:00");
Move_All_Selected_Events_to_Right_X_sec(linh);
FadeInOutAllEvents();


function Move_All_Selected_Events_to_Right_X_sec(timemove){ // Dich chuyen tat ca cac event ben phai vi tri dc chon timemove (Timecode)

	for (var track in Vegas.Project.Tracks)
	{
		for (var evnt in track.Events)
		{
			if (evnt.Selected == true ) {

				evnt.Start += timemove;
			}
		}
	}
}

/*function Swap_Two_Events(){
//try {

	var Selected_Track : Track = FindSelectedTrack();
	var First_Selected_Event : TrackEvent = FindSelectedEvent();
	First_Selected_Event.Selected = false;
	var Second_Selected_Event : TrackEvent = FindSelectedEvent();
	Second_Selected_Event.Selected = false;

	var Start_First_Event : Timecode = First_Selected_Event.Start;
	var Start_Second_Event : Timecode = Second_Selected_Event.Start;
	var End_First_Event : Timecode = First_Selected_Event.End;
	var End_Second_Event : Timecode = Second_Selected_Event.End;
	var First_Selected_Event_Length : Timecode = End_First_Event - Start_First_Event;
	var Second_Selected_Event_Length : Timecode = End_Second_Event - Start_Second_Event;


    Move_this_Event_to_End_of_Track_in_this_Time(Second_Selected_Event, Selected_Track.Length);
	if(First_Selected_Event_Length <= Second_Selected_Event_Length){
		Move_this_Event_to_End_of_Track_in_this_Time(First_Selected_Event, Start_Second_Event);
	}
	//First_Selected_Event.Copy(Selected_Track,Start_Second_Event);
	//Selected_Track.Events.Remove(First_Selected_Event);
	//var End_Event : TrackEvent = Find_End_Event_of_that_Track(Selected_Track);
	//End_Event.Copy(Selected_Track,Start_First_Event);
	//End_Event = Find_End_Event_of_that_Track(Selected_Track);

	//Selected_Track.Events.Remove(End_Event);

    }

/*catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}
*/






















// Ket thuc ham main chuong trinh.

// Ham dung trong main
// Nhung ham dau tien la do minh viet, nhung ham cuoi cung la do Vegas ho tro

function DeleteGap (){ // Ham nay xoa nhung khoang trong giua cac events

var FirstTrack : Track = Vegas.Project.Tracks.Item(0);
try
{
	for (var track in Vegas.Project.Tracks) {
		if( !track.Selected) continue; // bo qua nhung track khong dc chon
		var tracktime = new Timecode(0);
		for (var evnt in track.Events) {
			//evnt.AdjustStartLength(tracktime,evnt.Length,true);
			evnt.Start = tracktime;
			tracktime = tracktime + evnt.Length;
		}
	}
}

catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}


function CutVideo1020(){
							/* Ham nay cut video ra lam nhieu phan nho
						    Cu 30s lai lay 10s.
						    Moi doan 10s, cach nhau 20s */

var TwentySeconds: Timecode = new Timecode("00:00:20.00"); // Khai bao cac bien timecode
var ThirtySeconds: Timecode = new Timecode("00:00:30.00");
var beginSecond : Timecode = new Timecode("00:00:00.00");

try{
	// step through all selected video events:
	for (var track in Vegas.Project.Tracks)
	{
		if( !track.Selected) continue; // bo qua nhung track khong dc chon
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
catch (errorMsg){
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Bao loi
	}
}


function CutVideo10(){  // Ham nay cut event ra lam nhieu phan nho
						// Moi phan dai 10s
						// Cac phan la lien tuc, khong co khoang cach

var tenSecond : Timecode = new Timecode("00:00:10.00");
var beginSecond : Timecode = new Timecode("00:00:00.00");

try
{
	// step through all selected video events:
	for (var track in Vegas.Project.Tracks)
	{
		if( !track.Selected) continue; // bo qua nhung track khong dc chon
		var tracktime = beginSecond;
		for (var evnt in track.Events)
		{
			tracktime = tracktime + tenSecond;
			evnt.Split(tracktime );
			tracktime = beginSecond;

		}
	}
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}


function FadeInOutAllEvents (){ // Ham nay lam mem 2 dau su kien
								// Keo dai 2 dau su kien ra 1s
	var OneSec : Timecode = new Timecode("00:00:01.00");
    var TwoSec : Timecode = new Timecode("00:00:02.00");
	try
{
	var evnt = FindSelectedEvent();
	if (evnt == null)
	{
		throw "ALPhaHoai Error: You must select an Event.";
	}

	var FirstTrack : Track = Vegas.Project.Tracks.Item(0);

// step through all selected video events:
// Vong lap bat dau
	for (var track in Vegas.Project.Tracks) {
		if( !track.Selected) continue; // bo qua nhung track khong dc chon
				for (var evnt in track.Events) {
			// 3 dong nay la cac dong tinh toan thuc thi tren tung event
			evnt.AdjustStartLength(evnt.Start - OneSec, evnt.Length + TwoSec, false);
			evnt.FadeIn.Length = new Timecode(1000);//1000~1 sec
			evnt.FadeOut.Length = new Timecode(1000);}
	}
}
catch (errorMsg)
{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
}


}

function FadeInOutOneEvent (){  // Ham nay lam mem 1 event duoc chon, tuong tu ham FadeInOutAllEvents
								// Chi lam mem 1 event  dc chon, keo dai event ra 2 dau, moi dau 1s

	var OneSec : Timecode = new Timecode("00:00:01.00");
var TwoSec : Timecode = new Timecode("00:00:02.00");
try
{
	var evnt = FindSelectedEvent();
	if (evnt == null)
	{
		throw "Error: You must select an Event.";
	}
	evnt.AdjustStartLength(evnt.Start - OneSec, evnt.Length + TwoSec, false);
	evnt.FadeIn.Length = new Timecode(1000);//1000~1 sec
	evnt.FadeOut.Length = new Timecode(1000);
}
catch (errorMsg)
{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
}

}

function AddRegionsToEvents (){ // Them vung cho cac events dc chon, danh so tu 1,2,3,4...

var evnt : TrackEvent;
var myRegion : Region;
var RegionNumber;

try {

  //Find the selected event
  var track = FindSelectedTrack();
  if (null == track)
      throw "no selected track";

  var eventEnum = new Enumerator(track.Events);
  RegionNumber = 1;
  while (!eventEnum.atEnd()) {
    evnt = TrackEvent(eventEnum.item());
    myRegion = new Region(evnt.Start,evnt.Length,RegionNumber.ToString()); //Insert a region over this event
    Vegas.Project.Regions.Add(myRegion);
    eventEnum.moveNext();
    RegionNumber++;
  }

} catch (e) {
    MessageBox.Show(e);
}

}

function Aspectratio (){ // Toan man hinh , match output aspect
	var zero : int = 0;

function GetSelectionCount (mediaType) // Mot ham nho trong ham Aspectratio
{
    var cTracks   = Vegas.Project.Tracks.Count;
    var cSelected = zero;
    var ii;

    for (ii = zero; ii < cTracks; ii ++)
    {
        var track = Vegas.Project.Tracks[ii];

        if (track.MediaType == mediaType)
        {
            var eventEnum : Enumerator = new Enumerator(track.Events);

            while ( ! eventEnum.atEnd() )
            {
                if (eventEnum.item().Selected)
                {
                    cSelected ++;
                }

                eventEnum.moveNext();
            }
        }
    }

    return cSelected;
}

function GetActiveMediaStream (trackEvent : TrackEvent) // Mot ham nho trong ham Aspectratio
{
    try
    {
        if ( ! trackEvent.ActiveTake.IsValid())
        {
            throw "empty or invalid take";
        }

        var media = Vegas.Project.MediaPool.Find (trackEvent.ActiveTake.MediaPath);

        if (null == media)
        {
            throw "missing media";
        }

        var mediaStream = media.Streams.GetItemByMediaType (MediaType.Video, trackEvent.ActiveTake.StreamIndex);

        return mediaStream;
    }
    catch (e)
    {
        //MessageBox.Show(e);
        return null;
    }
}

function MatchOutputAspect (keyframe : VideoMotionKeyframe, dMediaPixelAspect : double, dAspectOut : double) // Mot ham nho trong ham Aspectratio
{
    var keyframeSave = keyframe;

    try
    {
        var rotation = keyframe.Rotation;

        // undo rotation so that we can get at correct aspect ratio.
        //
        keyframe.RotateBy (-rotation);

        var dWidth         = Math.abs(keyframe.TopRight.X   - keyframe.TopLeft.X);
        var dHeight        = Math.abs(keyframe.BottomLeft.Y - keyframe.TopLeft.Y);
        var dCurrentAspect = dMediaPixelAspect * dWidth / dHeight;
        var centerY        = keyframe.Center.Y;
        var centerX        = keyframe.Center.X;

        var dFactor;

        var bounds = new VideoMotionBounds(keyframe.TopLeft, keyframe.TopRight, keyframe.BottomRight, keyframe.BottomLeft);

        if (dCurrentAspect < dAspectOut)
        {
            // alter y coords
            dFactor = dCurrentAspect / dAspectOut;

            bounds.TopLeft.Y     = (bounds.TopLeft.Y     - centerY) * dFactor + centerY;
            bounds.TopRight.Y    = (bounds.TopRight.Y    - centerY) * dFactor + centerY;
            bounds.BottomLeft.Y  = (bounds.BottomLeft.Y  - centerY) * dFactor + centerY;
            bounds.BottomRight.Y = (bounds.BottomRight.Y - centerY) * dFactor + centerY;
        }
        else
        {
            // alter x coords
            dFactor = dAspectOut / dCurrentAspect;

            bounds.TopLeft.X     = (bounds.TopLeft.X     - centerX) * dFactor + centerX;
            bounds.TopRight.X    = (bounds.TopRight.X    - centerX) * dFactor + centerX;
            bounds.BottomLeft.X  = (bounds.BottomLeft.X  - centerX) * dFactor + centerX;
            bounds.BottomRight.X = (bounds.BottomRight.X - centerX) * dFactor + centerX;
        }

        // set it to new bounds
        keyframe.Bounds = bounds;

        // restore rotation.
        keyframe.RotateBy (rotation);

    }
    catch (e)
    {
        // restore original settings on error
        keyframe = keyframeSave;
        MessageBox.Show("MatchOuput: " + e);
    }
}


var dWidthProject  = Vegas.Project.Video.Width;
var dHeightProject = Vegas.Project.Video.Height;
var dPixelAspect   = Vegas.Project.Video.PixelAspectRatio;
var dAspect        = dPixelAspect * dWidthProject / dHeightProject;
var cSelected      = GetSelectionCount (MediaType.Video);


var cTracks = Vegas.Project.Tracks.Count;
var ii;
 // Ham chinh cua ham  Aspectratio
for (ii = zero; ii < cTracks; ii ++)
{
    var track   = Vegas.Project.Tracks[ii];

    if (! track.IsVideo())
    {
        continue;
    }

    var eventEnum : Enumerator = new Enumerator(track.Events);

    while ( ! eventEnum.atEnd() )
    {
        var trackEvent : TrackEvent = eventEnum.item();

        if ( !cSelected || trackEvent.Selected )
        {
            var mediaStream = GetActiveMediaStream (trackEvent);

            if (mediaStream)
            {
                var videoStream = VideoStream (mediaStream);

                var dMediaPixelAspect = videoStream.PixelAspectRatio;
                var videoEvent        = VideoEvent(eventEnum.item());
                var keyframes         = videoEvent.VideoMotion.Keyframes;

                var cKeyframes = keyframes.Count;
                var jj;

                for (jj = zero; jj < cKeyframes; jj ++)
                {
                    MatchOutputAspect (keyframes[jj], dMediaPixelAspect, dAspect);
                }
            }
        }

        eventEnum.moveNext();
    }
}

Vegas.UpdateUI(); // Xong ham Aspectratio
}


function SelectLeftEvents (){	 //ham nay chon nhung events nam ben trai cua con tro chuot
								 // chi chon nhung event thuoc track dc chon
try{
	for (var track in Vegas.Project.Tracks)
	{
		if( !track.Selected) continue;	// bo qua nhung track khong dc chon
		for (var evnt in track.Events)
		{
			if (evnt.Start <= Vegas.Cursor ) evnt.Selected = true ;
			// Vegas.Cursor ( Timecode) : vi tri dang dc chon
		}
	}
	}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function SelectRightEvents (){	 //ham nay chon nhung events nam ben phai cua con tro chuot
								 // chi chon nhung event thuoc track dc chon
try{
	for (var track in Vegas.Project.Tracks)
	{
		if( !track.Selected) continue;	// bo qua nhung track khong dc chon
		for (var evnt in track.Events)
		{
			if (evnt.End >= Vegas.Cursor ) evnt.Selected = true ;
			// Vegas.Cursor ( Timecode) : vi tri dang dc chon
		}
	}
	}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}


function Find_End_Event_of_First_Track() : TrackEvent { //Dua ra event cuoi cung cua track dau tien
	var End_Event : TrackEvent= FindSelectedEvent();
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Index > End_Event.Index) {
                End_Event = evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
	return End_Event;
}

function Find_End_Event_of_that_Track(that_Track) : TrackEvent { //Dua ra event cuoi cung cua that_Track
	var eventEnum = new Enumerator(that_Track.Events);
	var End_Event : TrackEvent = TrackEvent(eventEnum.item());
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());

        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Index > End_Event.Index) {
                End_Event = evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
	return End_Event;
}


function Select_Event_Step_n(n) { // Ham nay chon nhung event cach nhau n-1 event, n kieu int
								// Phai dam bao chi co 1 event dc chon !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
									   //  , nhung event cach nhau n event se dc chon
		var Selected_Event : TrackEvent ;
		var First_Event_Index ;// So hieu (Index) cua event dau tien trong nhom events dc chon
try
	{

		Selected_Event = FindSelectedEvent(); //Event dang dc chon
		if (Selected_Event == null) throw "Error: You must select an Event."; //Kiem tra xem co dang chon 1 event khong.
		First_Event_Index = Selected_Event.Index % n ; // So hieu (Index) cua event dau tien trong nhom events dc chon

		for (var track in Vegas.Project.Tracks) { // Ham lap for trong cac TrackEvent

				if( !track.Selected) continue; // Bo qua nhung track khong dc chon
				for (var evnt in track.Events)
					{
						if (evnt.Index % n == First_Event_Index)
						evnt.Selected = true ;
					}
		}

	}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Info_Event(){ // The hien 1 so thong tin co ban cua event
try{
	var Selected_Event : TrackEvent = FindSelectedEvent(); //Event dc chon

	if (Selected_Event == null) throw "Error: You must select an Event."; //Kiem tra xem co dang chon 1 event khong.

	var ViTriEvent : int =  Selected_Event.Index + 1; // Day la event thu may

	MessageBox.Show(Selected_Event.MediaType + "   "+ Selected_Event.Start + " - " + Selected_Event.End + " : " + Selected_Event.Length + "    Day la Event thu " + ViTriEvent);

}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}


function Move_Event_to_End_of_Track(){ //Di chuyen event dc chon xuong duoi cung cua track
try{
var Selected_Track : Track = FindSelectedTrack();
if (Selected_Track == null) throw "Error: Error: You must select an Track.";
var Selected_Event : TrackEvent = FindSelectedEvent();
if (Selected_Event == null) throw "Error: Error: You must select an Event.";

Selected_Event.Copy(Selected_Track,Selected_Track.Length);//Copy "Selected_Event" ra 1 ban sao dat phia cuoi cung cua track "Selected_Track"

Selected_Track.Events.Remove(Selected_Event); // Delete event "Selected_Event" la event dang dc chon
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Move_this_Event_to_End_of_Track(thisEvent){ //Di chuyen event dc chon xuong duoi cung cua track
try{
var thisTrack : Track = thisEvent.Track();

thisEvent.Copy(thisTrack,thisTrack.Length);//Copy "Selected_Event" ra 1 ban sao dat phia cuoi cung cua track "Selected_Track"

thisTrack.Events.Remove(thisEvent); // Delete event "Selected_Event" la event dang dc chon
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Move_this_Event_to_End_of_Track_in_this_Time(thisEvent, thisTime){ //Di chuyen event dc chon xuong duoi cung cua track
try{
var thisTrack : Track = thisEvent.Track();
thisEvent.Copy(thisTrack,thisTime);//Copy "Selected_Event" ra 1 ban sao dat phia cuoi cung cua track "Selected_Track"

thisTrack.Events.Remove(thisEvent); // Delete event "Selected_Event" la event dang dc chon
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Move_Event_to_First_of_Track(){ //Di chuyen event dc chon len dau cua track
try{




	var Selected_Track : Track = FindSelectedTrack();
	var Selected_Event : TrackEvent = FindSelectedEvent();

	if (Selected_Track == null) throw "Error: Error: You must select an Track.";
	if (Selected_Event == null) throw "Error: Error: You must select an Event.";

	Move_this_Event_to_End_of_Track(Selected_Event);

	var End_Event : TrackEvent = Find_End_Event_of_that_Track(Selected_Track);
	var End_Event_Index : int = End_Event.Index;
	var evnt : TrackEvent ;


    for ( var count = 0; count < End_Event_Index; count ++)
	{
		 evnt = Selected_Track.Events(0); // Evnt gan bang event dau tien cua Selected_Track
		 if(evnt != End_Event) 	 Move_this_Event_to_End_of_Track(evnt);
	}

    DeleteGap();
}

catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Move_this_Event_to_First_of_Track(this_Event){ //Di chuyen this_Event  len dau cua track
try{
	var Selected_Track : Track = this_Event.Track;
	var Selected_Event : TrackEvent = this_Event;

	Move_this_Event_to_End_of_Track(Selected_Event);

	var End_Event : TrackEvent = Find_End_Event_of_that_Track(Selected_Track);
	var End_Event_Index : int = End_Event.Index;
	var evnt : TrackEvent ;


    for ( var count = 0; count < End_Event_Index; count ++)
	{
		 evnt = Selected_Track.Events(0);// Evnt gan bang event dau tien cua Selected_Track
		 if(evnt != End_Event) 	 Move_this_Event_to_End_of_Track(evnt);
	}

    DeleteGap();
}

catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function FindSelectedTrack() : Track {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        if (track.Selected) {
            return track;
        }
        trackEnum.moveNext();
    }
    return null;


// finds the first selected event... note that when multiple events
// are selected, this only returns the first.  returns null if no
// events are selected.
}
function FindSelectedEvent() : TrackEvent {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Selected) {
                return evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
    return null;
}

function Un_Selected_All_Event(){
	{
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
			evnt.Selected = false ;
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
}
}

function Number_Selected_Event () : int { //Dua ra so event dang dc chon, tren tat ca cac track
try{
	var count : int = 0;
	var trackEnum = new Enumerator(Vegas.Project.Tracks);
	while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Selected)
						{
						count ++;
						}
            eventEnum.moveNext();
			}
        trackEnum.moveNext();
    }
	return count;
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Number_Event_in_All_Tracks () : int { //Dua ra so event trong tat ca track
try{
	var count : int = 0;
	var trackEnum = new Enumerator(Vegas.Project.Tracks);
	while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
						{
						count ++;
						}
            eventEnum.moveNext();
			}
        trackEnum.moveNext();
    }
	return count;
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Number_Event_in_this_Tracks (this_Track) : int { //Dua ra so event trong tat ca track
try{
	var count : int = 0;
	var trackEnum = new Enumerator(Vegas.Project.Tracks);
	while (!trackEnum.atEnd()) {
        var track : Track = this_Track(trackEnum.item());
		
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
						{
						count ++;
						}
            eventEnum.moveNext();
			}
        trackEnum.moveNext();
    }
	return count;
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}

function Ajust_Cut_Video (){ // Dieu chinh cat video ra 3 phut

	var Ajust_Time : Timecode = new Timecode("00:03:00.00"); // 3 phut, co the chinh do dai video tuy y

try{
	var Selected_Event : TrackEvent = FindSelectedEvent();
	if (Selected_Event == null) throw "Error: Error: You must select an Event.";
	var Selected_Track : Track = FindSelectedTrack();
	if (Selected_Track == null) throw "Error: Error: You must select an Track.";
	var First_Event : TrackEvent = Selected_Track.Events.Item(0);
	var Ajust_Time_ToMilliseconds = Ajust_Time.ToMilliseconds();
	var Length_Event : Timecode = Selected_Event.Length;
	var Length_Event_ToMilliseconds = Length_Event.ToMilliseconds();
	var Ratio = Length_Event_ToMilliseconds/Ajust_Time_ToMilliseconds;
	var Int_Ratio : int = parseInt(Ratio);
	if(Int_Ratio+ 0.5 < Ratio )  Int_Ratio += 1; // Lam tron Int_Ratio
	if(Int_Ratio <= 2 ) MessageBox.Show("Error : Event not long enough"); // Event khong du do dai
	else {
	CutVideo10(); // Cut video ra cac phan 10s, khong cach
	Un_Selected_All_Event(); // Bo chon tat ca cac events
	First_Event.Selected = true ; // Chon event dau tien cua track
	Select_Event_Step_n(Int_Ratio); // Chon cac event cach Int_Ratio-1 event ke tu event dau tien
	}
}
catch (errorMsg)
	{
	MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

}

































































// Nhung ham nay do Vegas ho tro VegasFunction:

/**
 * This script contains some helper functions that can be cut and
 * pasted into other scripts.
 *
 * Revision Date: Apr 20, 2004.
**/


function CreateTrack(mediaType) {
    var track;
    if (mediaType == MediaType.Audio) {
        track = new AudioTrack();
    } else {
        track = new VideoTrack();
    }
    Vegas.Project.Tracks.Add(track);
    return track;
}

function CreateTrackAndEvent(mediaType)
{
    var track, evnt;
    if (mediaType == MediaType.Audio) {
        track = new AudioTrack();
        evnt = new AudioEvent();
    } else {
        track = new VideoTrack();
        evnt = new VideoEvent();
    }
    Vegas.Project.Tracks.Add(track);
    track.Events.Add(evnt);
    return evnt;
}

function CreateGeneratedMedia(generatorName, presetName) {
    var generator = Vegas.Generators.GetChildByName(generatorName);
    var media = new Media(generator, presetName);
    if (!media.IsValid())
        throw "failed to create media: " + generatorName + " (" + presetName + ")";
    return media;
}

// finds the first selected track... note that when multiple tracks
// are selected, this only returns the first.  returns null if no
// tracks are selected.

function FindPlugInNode(rootNode : PlugInNode, nameRegExp : RegExp) : PlugInNode {
    if (null != rootNode.Name.match(nameRegExp)) {
        return rootNode;
    } else {
        var children : Enumerator = new Enumerator(rootNode);
        while (!children.atEnd()) {
            var childNode : PlugInNode = PlugInNode(children.item())
            var childMatch : PlugInNode = FindPlugInNode(childNode, nameRegExp);
            if (null != childMatch) {
                return childMatch;
            }
            children.moveNext();
        }
        return null;
    }
}

function FindRenderer(rendererRegExp : RegExp) : Renderer {
    var rendererEnum : Enumerator = new Enumerator(Vegas.Renderers);
    while (!rendererEnum.atEnd()) {
        var renderer : Renderer = Renderer(rendererEnum.item());
        if (null != renderer.FileTypeName.match(rendererRegExp)) {
            return renderer;
        }
        rendererEnum.moveNext();
    }
    return null;
}

function FindRenderTemplate(renderer : Renderer, templateRegExp : RegExp) : RenderTemplate {
    var templateEnum : Enumerator = new Enumerator(renderer.Templates);
    while (!templateEnum.atEnd()) {
        var renderTemplate : RenderTemplate = RenderTemplate(templateEnum.item());
        if (null != renderTemplate.Name.match(templateRegExp)) {
            return renderTemplate;
        }
        templateEnum.moveNext();
    }
    return null;
}

function CreateTransitionEffect(nameRegExp : RegExp) : Effect {
    var plugIn = FindPlugInNode(Vegas.Transitions, /Dissolve/);
    if (null == plugIn)
        throw "failed to find plug-in";
    return new Effect(plugIn);
}

function FindEnvelopeByType(envelopes : Envelopes, type : EnvelopeType) : Envelope {
    var i : int;
    var count : int = envelopes.Count;
    for (i = 0; i < count; i++) {
        var envelope = envelopes[i];
        if (envelope.Type == type) {
            return envelope;
        }
    }
    return null;
}


function ShowSaveFileDialog(filter, title, defaultFilename) {
    var saveFileDialog = new SaveFileDialog();
    if (null == filter) {
        filter = "All Files (*.*)|*.*";
    }
    saveFileDialog.Filter = filter;
    if (null != title)
        saveFileDialog.Title = title;
    saveFileDialog.CheckPathExists = true;
    saveFileDialog.AddExtension = true;
    if (null != defaultFilename) {
        var initialDir = Path.GetDirectoryName(defaultFilename);
        if (Directory.Exists(initialDir)) {
            saveFileDialog.InitialDirectory = initialDir;
        }
        saveFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
        saveFileDialog.FileName = Path.GetFileName(defaultFilename);
    }
    if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
        return Path.GetFullPath(saveFileDialog.FileName);
    } else {
        return null;
    }
}

// an example filter: "PNG File (*.png)|*.png|JPEG File (*.jpg)|*.jpg"
function ShowOpenFileDialog(filter, title, defaultFilename) {
    var openFileDialog = new OpenFileDialog();
    if (null == filter) {
        filter = "All Files (*.*)|*.*";
    }
    openFileDialog.Filter = filter;
    if (null != title)
        openFileDialog.Title = title;
    openFileDialog.CheckPathExists = true;
    openFileDialog.AddExtension = true;
    if (null != defaultFilename) {
        var initialDir = Path.GetDirectoryName(defaultFilename);
        if (Directory.Exists(initialDir)) {
            openFileDialog.InitialDirectory = initialDir;
        }
        openFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
        openFileDialog.FileName = Path.GetFileName(defaultFilename);
    }
    if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog()) {
        return Path.GetFullPath(openFileDialog.FileName);
    } else {
        return null;
    }
}

function TrimEventAtCursor() { // Function that trims event and moves all remaining events on track left.


  //Global declarations
    var dStart : Double;
    var dLength : Double;
    var dCursor : Double;
    var trackEnum : Enumerator;
    var evnt : TrackEvent;

    var EventBegin : Timecode = Vegas.Cursor;    // Use this to move cursor position.
    var track = FindSelectedTrack();             // Use this function to find the first selected track.
    var eventEnum = new Enumerator(track.Events);
    var BeyondCursor : boolean = false;          // This flag used to notify if event is to right of cursor.


  var EventFound : boolean = false;  // Function returns false if no video media under cursor.
  var DeleteFrames : Timecode = new Timecode("00:00:00:00");
  var DeleteTime : Double = DeleteFrames.ToMilliseconds();

  dCursor = Vegas.Cursor.ToMilliseconds(); // Remember the cursor position.

  //Go through each event on the track.

  while (!eventEnum.atEnd()) {
    evnt = TrackEvent(eventEnum.item());
    evnt.Selected = false;  // De-select the event


    // Get the event's start and length timecode, in milliseconds.
    dStart = evnt.Start.ToMilliseconds();
    dLength = evnt.Length.ToMilliseconds();


    if (BeyondCursor) {                     // Move, but don't truncate events to the
      evnt.Start=evnt.Start-DeleteFrames;   // right of the event under the cursor.
    }

/**
*     If the cursor timecode is between the beginning and end of the
*     event timecodes, then select the event, and trim start by n frames.
*     Move selected event and all events to right of cursor to left by n frames.
**/

    if ( (dCursor >= dStart) && ( dCursor < (dLength + dStart) ) ) {
      evnt.Selected = true; // Select this event.
      EventFound = true;
      BeyondCursor = true;  // Remaining events are to right of cursor.
      DeleteFrames = Vegas.Cursor - Timecode(evnt.Start);
      DeleteTime = DeleteFrames.ToMilliseconds();
      EventBegin = evnt.Start;
      dLength = dLength - DeleteTime;

      // Next two lines truncate start of event, and then move it left
      // by the same amount that it was truncated.
      evnt.AdjustStartLength(new Timecode(dStart), new Timecode(dLength), false);
      evnt.ActiveTake.Offset = DeleteFrames+evnt.ActiveTake.Offset;
    }

    eventEnum.moveNext(); // Go to next event on this timeline.
    Vegas.UpdateUI();

    }
return EventFound;
}


// Do la nhung ham do Vegas giup do