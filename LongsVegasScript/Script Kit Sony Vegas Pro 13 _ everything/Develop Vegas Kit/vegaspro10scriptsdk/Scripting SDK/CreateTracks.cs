/**
 * Sample script that script creates 10 tracks each containing 10
 * events.  No media is assigned to the events.
 *
 * Revision Date: Oct. 23, 2006.
 **/

using Sony.Vegas;

public class EntryPoint {

    public void FromVegas(Vegas vegas) {

        int trackCount = 10;
        int eventCount = 10;
        Timecode eventLength = Timecode.FromSeconds(10);
        MediaType mediaType = MediaType.Audio;

        for (int i = 0; i < trackCount; i++) {

            // create a track
            Track track;
            if (mediaType == MediaType.Audio) 
                track = new AudioTrack(i, "Audio " + ((i/2)+1));
            else
                track = new VideoTrack(i, "Video " + ((i/2)+1));

            // add the track
            vegas.Project.Tracks.Add(track);

            Timecode startTime = Timecode.FromSeconds(0);

            for (int j = 0; j < eventCount; j++) {

                // create an event
                TrackEvent trackEvent;
                if (mediaType == MediaType.Audio) 
                    trackEvent = new AudioEvent(startTime, eventLength, "Audio Event " + (j+1));
                else
                    trackEvent = new VideoEvent(startTime, eventLength, "Video Event " + (j+1));

                // add the event to the track
                track.Events.Add(trackEvent);

                // increment the start time
                startTime += eventLength;
            }

            // toggle the media type
            if (mediaType == MediaType.Audio) 
                mediaType = MediaType.Video;
            else
                mediaType = MediaType.Audio;
        }
    }
}
