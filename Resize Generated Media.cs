/**
 * This script resizes all generated media to match the size and pixel
 * aspect ratio of the project. The script will operate on the
 * selected video event or, if none are selected, it will operate on
 * all video events.
 *
 * Revision Date: March 26, 2010.
 **/

using System;
using System.Drawing;
using System.Collections.Generic;
using ScriptPortal.Vegas;

public class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
        Size projectSize = new Size(vegas.Project.Video.Width, vegas.Project.Video.Height);
        double projectAspect = vegas.Project.Video.PixelAspectRatio;

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
            if (!videoStream.Parent.IsGenerated()) continue;
            videoStream.Size = projectSize;
            videoStream.PixelAspectRatio = projectAspect;
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
        
}
