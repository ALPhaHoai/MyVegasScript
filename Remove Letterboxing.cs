/**
 * Resize video events to match the aspect ratio of the project. This
 * removes any letterboxing or pillarboxing.  The script will operate
 * on the selected video event or, if none are selected, it will
 * operate on all video events.
 *
 * Revision Date: March 26, 2010.
 **/

using System;
using System.Collections.Generic;
using ScriptPortal.Vegas;

public class EntryPoint
{

    public void FromVegas(Vegas vegas)
    {
        double dWidthProject  = vegas.Project.Video.Width;
        double dHeightProject = vegas.Project.Video.Height;
        double dPixelAspect   = vegas.Project.Video.PixelAspectRatio;
        double dAspect        = dPixelAspect * dWidthProject / dHeightProject;

        List<VideoEvent> events = new List<VideoEvent>();
        AddSelectedVideoEvents(vegas, events);
        if (0 == events.Count)
        {
            AddAllVideoEvents(vegas, events);
        }

        foreach (VideoEvent videoEvent in events)
        {
            Take take = videoEvent.ActiveTake;
            if (null == take) continue;
            VideoStream videoStream = take.MediaStream as VideoStream;
            if (null == videoStream) continue;
            double dMediaPixelAspect = videoStream.PixelAspectRatio;
            foreach (VideoMotionKeyframe keyframe in videoEvent.VideoMotion.Keyframes)
            {
                MatchOutputAspect(keyframe, dMediaPixelAspect, dAspect);                                                                     
            }
        }
    }

    void AddSelectedVideoEvents(Vegas vegas, List<VideoEvent> events)
    {
        foreach (Track track in vegas.Project.Tracks)
        {
            if (track.IsVideo())
            {
                foreach (VideoEvent videoEvent in track.Events)
                {
                    if (videoEvent.Selected)
                    {
                        events.Add(videoEvent);
                    }
                }
            }
        }
    }

    void AddAllVideoEvents(Vegas vegas, List<VideoEvent> events)
    {
        foreach (Track track in vegas.Project.Tracks)
        {
            if (track.IsVideo())
            {
                foreach (VideoEvent videoEvent in track.Events)
                {
                    events.Add(videoEvent);
                }
            }
        }
    }

    void MatchOutputAspect(VideoMotionKeyframe keyframe, double dMediaPixelAspect, double dAspectOut)
    {
        VideoMotionKeyframe keyframeSave = keyframe;
        try
        {
            double rotation = keyframe.Rotation;    
            
            // undo rotation so that we can get at correct aspect ratio.
            //
            keyframe.RotateBy(-rotation);

            double dWidth         = Math.Abs(keyframe.TopRight.X   - keyframe.TopLeft.X);
            double dHeight        = Math.Abs(keyframe.BottomLeft.Y - keyframe.TopLeft.Y);
            double dCurrentAspect = dMediaPixelAspect * dWidth / dHeight;
            double centerY        = keyframe.Center.Y;
            double centerX        = keyframe.Center.X;        
        
            double dFactor;
        
            VideoMotionBounds bounds = new VideoMotionBounds(keyframe.TopLeft, keyframe.TopRight, keyframe.BottomRight, keyframe.BottomLeft);

            if (dCurrentAspect < dAspectOut)
            {
                // alter y coords            
                dFactor = dCurrentAspect / dAspectOut;            
                        
                bounds.TopLeft.Y     = (float) ((bounds.TopLeft.Y     - centerY) * dFactor + centerY);
                bounds.TopRight.Y    = (float) ((bounds.TopRight.Y    - centerY) * dFactor + centerY);
                bounds.BottomLeft.Y  = (float) ((bounds.BottomLeft.Y  - centerY) * dFactor + centerY);
                bounds.BottomRight.Y = (float) ((bounds.BottomRight.Y - centerY) * dFactor + centerY);
            }
            else
            {                          
                // alter x coords
                dFactor = dAspectOut / dCurrentAspect;            
                        
                bounds.TopLeft.X     = (float) ((bounds.TopLeft.X     - centerX) * dFactor + centerX);
                bounds.TopRight.X    = (float) ((bounds.TopRight.X    - centerX) * dFactor + centerX);
                bounds.BottomLeft.X  = (float) ((bounds.BottomLeft.X  - centerX) * dFactor + centerX);
                bounds.BottomRight.X = (float) ((bounds.BottomRight.X - centerX) * dFactor + centerX);
            }
        
            // set it to new bounds
            keyframe.Bounds = bounds;
        
            // restore rotation.        
            keyframe.RotateBy (rotation);
        
        }
        catch (Exception e)
        {
            // restore original settings on error
            keyframe = keyframeSave;
        }    
    }

}


