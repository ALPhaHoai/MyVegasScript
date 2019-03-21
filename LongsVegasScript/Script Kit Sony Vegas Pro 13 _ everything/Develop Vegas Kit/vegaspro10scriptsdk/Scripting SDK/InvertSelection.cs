/**
 * This script inverts track and event selections.
 *
 * Revision Date: Oct. 23, 2006.
 **/

using Sony.Vegas;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
        foreach (Track track in vegas.Project.Tracks) {
            track.Selected = !track.Selected;
            foreach (TrackEvent trackEvent in track.Events) {
                trackEvent.Selected = !trackEvent.Selected;
            }
        }
    }
}
