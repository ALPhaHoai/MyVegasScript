/**
 * This script will add regions for all events on the selected track
 *
 * By John Meyer 11/4/2003 (with ideas from Edward Troxel's "Markers to Events" script)
 *
 **/

import System;
import System.IO;
import System.Windows.Forms;
import ScriptPortal.Vegas;
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
}

