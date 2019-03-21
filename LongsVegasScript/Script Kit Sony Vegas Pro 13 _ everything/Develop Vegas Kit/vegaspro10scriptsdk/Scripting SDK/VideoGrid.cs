/**
 * This script creates video tracks whose track motion is preset to
 * create a "Brady Bunch"-style grid.
 *
 * Revision Date: Jul 09, 2007.
 **/

using System;
using Sony.Vegas;

public class EntryPoint
{
    // modifiy these variable to change to dimentions of the grid
    int TracksX = 3;
    int TracksY = 3;
    
    public void FromVegas(Vegas vegas)
    {
        int width = vegas.Project.Video.Width;
        int height = vegas.Project.Video.Height;
        int trackWidth = width / TracksX;
        int trackHeight = height / TracksY;
        int startX = -width/2 + trackWidth/2;
        int startY = height/2 - trackHeight/2;
        int trackIndex = 0;
        for (int y = 0; y < TracksY; y++) {
            for (int x = 0; x < TracksX; x++) {
                VideoTrack track = new VideoTrack(trackIndex++);
                vegas.Project.Tracks.Add(track);
                TrackMotionKeyframe mkf = track.TrackMotion.MotionKeyframes[0];
                mkf.Width = trackWidth;
                mkf.Height = trackHeight;
                mkf.PositionX = startX + (x*trackWidth);
                mkf.PositionY = startY - (y*trackHeight);
            }
        }
    }
}
