/*

  This script will "promote" the CC command markers embedded in the
  selected events' media to project CC command markers.

  Note that more than one command marker can not exist in the same
  location. When this type of failure occurs, the script records the
  error, contiues promoting the remainder of the command markers,
  and informs you of which ones failed upon completion.

  This script is only supported by Vegas version 9.0d and above.

  Last Modified: February 2010.

*/

using System;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using ScriptPortal.Vegas;

public class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
        Project proj = vegas.Project;
        foreach (Track track in proj.Tracks)
        {
            foreach (TrackEvent trackEvent in track.Events)
            {
                if (!trackEvent.Selected)
                    continue;
                Take activeTake = trackEvent.ActiveTake;
                if (null == activeTake)
                    continue;
                Media media = activeTake.Media;
                if (null == media)
                    continue;
                if (IsPairedAudioEvent(trackEvent, media))
                    continue;
                Timecode eventStart = trackEvent.Start;
                Timecode eventEnd = eventStart + trackEvent.Length;
                Timecode takeOffset = activeTake.Offset;
                Timecode position;
                foreach (MediaCommandMarker mcm in media.CommandMarkers)
                {
                    position = mcm.Position + eventStart - takeOffset;
                    if (position < eventStart || position > eventEnd)
                        continue;
                    CommandMarker commandmarker = new CommandMarker(position, mcm.CommandType, /*param*/mcm.CommandParameter, /*comment*/mcm.Label);
                    try
                    {
                        proj.CommandMarkers.Add(commandmarker);
                    }
                    catch (Exception e)
                    {
                        AddError(e, mcm, position);
                    }
                }
            }
        }
        if (0 < myErrors.Count)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("Some problems occured in promoting the selected media command markers to the project level:\r\n");
            foreach (String err in myErrors)
            {
                msg.Append("\r\n");
                msg.Append(err);
            }
            MessageBox.Show(msg.ToString());
        }
    }

    // skip audio events that are grouped with a selected video event
    // that uses the same media because they usually contain the same
    // captioning data
    private bool IsPairedAudioEvent(TrackEvent trackEvent, Media media)
    {
        if (trackEvent.IsAudio() && trackEvent.IsGrouped)
        {
            TrackEventGroup group = trackEvent.Group;
            if (null != group)
            {
                foreach (TrackEvent groupedEvent in group)
                {
                    if (groupedEvent != trackEvent)
                    {
                        if (groupedEvent.IsVideo() && groupedEvent.Selected)
                        {
                            Take take = groupedEvent.ActiveTake;
                            if (null != take)
                            {
                                Media groupedMedia = take.Media;
                                if (null != media)
                                {
                                    if (groupedMedia == media)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    
    private ArrayList myErrors = new ArrayList();

    private void AddError(Exception e, CommandMarker commandmarker, Timecode position)
    {
        myErrors.Add(String.Format("Failed to add command marker '{0}' at {1}: {2}",
                                   commandmarker.Label,
                                   position.ToString(),
                                   e.Message));
    }

    
}
