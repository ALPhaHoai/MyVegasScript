/**
 * Script to grouping video events with their associated audio events
 * (as requested in VEG-8930)
 * 
 * Revision Date: Feb. 3, 2016
 **/

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using ScriptPortal.Vegas;

public class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
		// Collect lists of the audio of media with video
		// and video events of media with audio
		
		List<TrackEvent> evtListAudio = new List<TrackEvent>();
		List<TrackEvent> evtListVideo = new List<TrackEvent>();

		foreach (Track trkItem in vegas.Project.Tracks)
		{
			foreach (TrackEvent evtItem in trkItem.Events)
			{
				if ((evtItem.MediaType == MediaType.Audio) && (evtItem.ActiveTake.Media.HasVideo()))
					evtListAudio.Add(evtItem);
				else if ((evtItem.MediaType == MediaType.Video) && (evtItem.ActiveTake.Media.HasAudio()))
					evtListVideo.Add(evtItem);
			}
		}
		
		// For all of the listed video events, group together any
		// audio events sharing the same start/end, and sharing the same media

        int numVidEvents = evtListVideo.Count;
        for(int ixVidEvt = 0; ixVidEvt < numVidEvents; ++ixVidEvt)
        {
            TrackEvent evtVid = evtListVideo[ixVidEvt];

            TrackEventGroup grpTmp = null;
			bool matchFound = false; // defer creating a group, or adding the events to the group until a match is found

            // Walk the audio-event list in reverse order so matched items can be safetly removed from the list
            int numAudEvents = evtListAudio.Count;
            for(int ixAudEvt = numAudEvents-1; ixAudEvt >= 0; --ixAudEvt)
            {
                TrackEvent evtAud = evtListAudio[ixAudEvt];

                // For a match the event must;
                // have the same start and end points and reference the same media
				bool test1 = evtVid.Start == evtAud.Start;
				bool test2 = evtVid.End == evtAud.End;
				bool test3 = evtVid.ActiveTake.Media == evtAud.ActiveTake.Media;

				if (test1 && test2 && test3)
				{
					if(!matchFound)
					{
                        // On the first match, create the group and add the video event
						matchFound = true;
            			grpTmp = new TrackEventGroup();
			            vegas.Project.TrackEventGroups.Add(grpTmp);
						grpTmp.Add(evtVid);
					}

                    // Add the matching audio event and remove it from the list leaving only unmatched events
					grpTmp.Add(evtAud);
                    evtListAudio.RemoveAt(ixAudEvt);
				}
            }
        }
	}
}
