/*

  This script will "promote" the markers and regions embedded in the
  selected events' media to project markers and regions.

  Note that more than one marker can not exist in the same
  location. When this type of failure occurs, the script records the
  error, contiues promoting the remainder of the markers and regions,
  and informs you of which ones failed upon completion.

  This script is only supported by Vegas version 6.0c and above.

  Last Modified: July 22, 2005.

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
                Timecode eventStart = trackEvent.Start;
                Timecode eventEnd = eventStart + trackEvent.Length;
                Timecode takeOffset = activeTake.Offset;
                Timecode position;
                foreach (MediaMarker mm in media.Markers)
                {
                    position = mm.Position + eventStart - takeOffset;
                    if (position < eventStart || position > eventEnd)
                        continue;
                    Marker marker = new Marker(position, mm.Label);
                    try
                    {
                        proj.Markers.Add(marker);
                    }
                    catch (Exception e)
                    {
                        AddError(e, mm, position);
                    }
                }
                foreach (MediaRegion mr in media.Regions)
                {
                    position = mr.Position + eventStart - takeOffset;
                    if (position < eventStart || position > eventEnd)
                        continue;
                    Region region = new Region(position, mr.Length, mr.Label);
                    try
                    {
                        proj.Regions.Add(region);
                    }
                    catch (Exception e)
                    {
                        AddError(e, mr, position);
                    }
                }
            }
        }
        if (0 < myErrors.Count)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("Some problems occured in promoting the selected media markers to the project level:\r\n");
            foreach (String err in myErrors)
            {
                msg.Append("\r\n");
                msg.Append(err);
            }
            MessageBox.Show(msg.ToString());
        }
    }

    private ArrayList myErrors = new ArrayList();

    private void AddError(Exception e, Marker marker, Timecode position)
    {
        myErrors.Add(String.Format("Failed to add marker '{0}' at {1}: {2}",
                                   marker.Label,
                                   position.ToString(),
                                   e.Message));
    }

    
}
