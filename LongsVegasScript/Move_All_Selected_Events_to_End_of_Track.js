/*
* Chuong trinh nay di chuyen tat ca cac event dang dc chon trong track dc chon dau tien ve cuoi cung cua track
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
var Selected_Track: Track = FindSelectedTrack();
//var First_Event : TrackEvent = Selected_Track.Events.Item(0);

Move_All_Selected_Events_to_End_of_this_Track(Selected_Track);


function Move_All_Selected_Events_to_End_of_this_Track(this_Track) {

    var Num_Selected_Event: int = Number_Selected_Event_in_this_Track(this_Track)
    var Selected_Event: TrackEvent;
    for (var i: int = 0; i < Num_Selected_Event; i++) {
        Selected_Event = FindSelectedEvent_in_this_Track(this_Track);
        if (Selected_Event.Track == this_Track) {
            Move_this_Event_to_End_of_Track(Selected_Event, this_Track);
        }

    }


}


function Move_this_Event_to_End_of_Track(this_Event, this_Track) {
    try {
        if (this_Event.Track != this_Track) throw "Error: Event nay khong thuoc Track nay";

        this_Event.Copy(this_Track, this_Track.Length);//Copy "this_Event" ra 1 ban sao dat phia cuoi cung cua track "this_Track"
        var End_Event = Find_End_Event_of_that_Track(this_Track);
        End_Event.Selected = false;
        this_Track.Events.Remove(this_Event); // Delete event "this_Event" la event dang dc chon
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


function Find_End_Event_of_that_Track(that_Track): TrackEvent { //Dua ra event cuoi cung cua that_Track
    var eventEnum = new Enumerator(that_Track.Events);
    var End_Event: TrackEvent = TrackEvent(eventEnum.item());
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track: Track = Track(trackEnum.item());

        while (!eventEnum.atEnd()) {
            var evnt: TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Index > End_Event.Index) {
                End_Event = evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
    return End_Event;
}

function Number_Selected_Event_in_this_Track(this_Track): int { //Dua ra so event dang dc chon, tren tat ca cac track
    try {
        var count: int = 0;
        var trackEnum = new Enumerator(Vegas.Project.Tracks);
        while (!trackEnum.atEnd()) {
            var track: Track = Track(trackEnum.item());
            var eventEnum = new Enumerator(track.Events);
            while (!eventEnum.atEnd()) {
                var evnt: TrackEvent = TrackEvent(eventEnum.item());
                if (evnt.Selected && evnt.Track == this_Track) {
                    count++;
                }
                eventEnum.moveNext();
            }
            trackEnum.moveNext();
        }
        return count;
    }
    catch (errorMsg) {
        MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
}

function FindSelectedEvent_in_this_Track(this_Track): TrackEvent {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track: Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt: TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Selected && evnt.Track == this_Track) {
                return evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
    return null;
}