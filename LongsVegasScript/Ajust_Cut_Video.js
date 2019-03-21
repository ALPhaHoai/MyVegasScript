/*
* Author : ALPhaHoai
* Date : Octorber 5, 2016
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


Ajust_Cut_Video();
Delete_All_None_Selected_Events();
DeleteEmptySpaceBetweenEvents_ALPhaHoai_Ver01();
var Selected_Track: Track = FindSelectedTrack();
Vegas.Cursor = Selected_Track.Length; // Dua con tro chuot vao vi tri cuoi cung cua track


function Ajust_Cut_Video() { // Dieu chinh cat video ra 3 phut

    var Ajust_Time: Timecode = new Timecode("00:03:00.00"); // 3 phut, co the chinh do dai video tuy y

    try {
        var Selected_Event: TrackEvent = FindSelectedEvent();
        if (Selected_Event == null) throw "Error: Error: You must select an Event.";
        var Selected_Track: Track = FindSelectedTrack();
        if (Selected_Track == null) throw "Error: Error: You must select an Track.";
        var First_Event: TrackEvent = Selected_Track.Events.Item(0);
        var Ajust_Time_ToMilliseconds = Ajust_Time.ToMilliseconds();
        var Length_Event: Timecode = Selected_Event.Length;
        var Length_Event_ToMilliseconds = Length_Event.ToMilliseconds();
        var Ratio = Length_Event_ToMilliseconds / Ajust_Time_ToMilliseconds;
        var Int_Ratio: int = parseInt(Ratio);
        if (Int_Ratio + 0.5 < Ratio) Int_Ratio += 1; // Lam tron Int_Ratio
        if (Int_Ratio <= 2) MessageBox.Show("Error : Event not long enough"); // Event khong du do dai
        else {
            CutVideo10(); // Cut video ra cac phan 10s, khong cach
            Un_Selected_All_Event(); // Bo chon tat ca cac events
            First_Event.Selected = true; // Chon event dau tien cua track
            Select_Event_Step_n(Int_Ratio); // Chon cac event cach Int_Ratio-1 event ke tu event dau tien
        }
    }
    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

}

function FindSelectedTrack(): Track {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track: Track = Track(trackEnum.item());
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

function FindSelectedEvent(): TrackEvent {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track: Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt: TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Selected) {
                return evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
    return null;
}


function Select_Event_Step_n(n) { // Ham nay chon nhung event cach nhau n-1 event, n kieu int
    // Phai dam bao chi co 1 event dc chon !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //  , nhung event cach nhau n event se dc chon
    var Selected_Event: TrackEvent;
    var First_Event_Index;// So hieu (Index) cua event dau tien trong nhom events dc chon
    try {

        Selected_Event = FindSelectedEvent(); //Event dang dc chon
        if (Selected_Event == null) throw "Error: You must select an Event."; //Kiem tra xem co dang chon 1 event khong.
        First_Event_Index = Selected_Event.Index % n; // So hieu (Index) cua event dau tien trong nhom events dc chon

        for (var track in Vegas.Project.Tracks) { // Ham lap for trong cac TrackEvent

            if (!track.Selected) continue; // Bo qua nhung track khong dc chon
            for (var evnt in track.Events) {
                if (evnt.Index % n == First_Event_Index)
                    evnt.Selected = true;
            }
        }

    }
    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
}


function CutVideo10() {  // Ham nay cut event ra lam nhieu phan nho
    // Moi phan dai 10s
    // Cac phan la lien tuc, khong co khoang cach

    var tenSecond: Timecode = new Timecode("00:00:10.00");
    var beginSecond: Timecode = new Timecode("00:00:00.00");

    try {
        // step through all selected video events:
        for (var track in Vegas.Project.Tracks) {
            if (!track.Selected) continue; // bo qua nhung track khong dc chon
            var tracktime = beginSecond;
            for (var evnt in track.Events) {
                tracktime = tracktime + tenSecond;
                evnt.Split(tracktime);
                tracktime = beginSecond;

            }
        }
    }
    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}


function Un_Selected_All_Event() {
    {
        var trackEnum = new Enumerator(Vegas.Project.Tracks);
        while (!trackEnum.atEnd()) {
            var track: Track = Track(trackEnum.item());
            var eventEnum = new Enumerator(track.Events);
            while (!eventEnum.atEnd()) {
                var evnt: TrackEvent = TrackEvent(eventEnum.item());
                evnt.Selected = false;
                eventEnum.moveNext();
            }
            trackEnum.moveNext();
        }
    }
}


function FindNoneSelectedEvent(): TrackEvent {
    try {
        for (var track in Vegas.Project.Tracks) {
            for (var evnt in track.Events) {
                if (evnt.Selected == false)
                    return evnt;
            }
        }
        return null;
    }
    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
}


function Number_of_None_Selected_Event(): int {
    try {
        var count: int = 0;
        for (var track in Vegas.Project.Tracks) {
            for (var evnt in track.Events) {
                if (evnt.Selected == false)
                    count++;
            }
        }
        return count;
    }
    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
}

function Delete_All_None_Selected_Events() {
    var NumberofNoneSelectedEvent: int = Number_of_None_Selected_Event();
    var evnt: TrackEvent;
    var thisTrack: Track;
    for (var i: int = 0; i < NumberofNoneSelectedEvent; i++) {
        evnt = FindNoneSelectedEvent();
        thisTrack = evnt.Track;
        thisTrack.Events.Remove(evnt);

    }
}


function DeleteEmptySpaceBetweenEvents_ALPhaHoai_Ver01() {
    //time intervals for split events.

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
    }

    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

}