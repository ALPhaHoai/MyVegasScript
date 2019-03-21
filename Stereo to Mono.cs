/**
 * This script converts stereo events to pairs of mono events.  It
 * only operates on the selected audio events or, if none are
 * selected, all audio events.
 *
 * Revision Date: March 26, 2010.
 **/

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using ScriptPortal.Vegas;

public class EntryPoint {

    Vegas myVegas;

    public void FromVegas(Vegas vegas) {
        myVegas = vegas;
        List<WorkItem> workItems = new List<WorkItem>();
        List<AudioEvent> events = new List<AudioEvent>();
        AddSelectedAudioEvents(events);
        if (0 == events.Count)
        {
            AddAllAudioEvents(events);
        }
        
        foreach (AudioEvent audioEvent in events)
        {
            if (audioEvent.Channels == ChannelRemapping.None)
            {
                Track destTrack = FindDestTrack(audioEvent.Track);
                if (null == destTrack)
                {
                    destTrack = CreateDestTrack(audioEvent.Track);
                }
                workItems.Add(new WorkItem(audioEvent, destTrack));
            }
        }

        foreach (WorkItem workItem in workItems)
        {
            AudioEvent destEvent = (AudioEvent) workItem.SrcEvent.Copy(workItem.DstTrack, workItem.SrcEvent.Start);
            workItem.SrcEvent.Channels = ChannelRemapping.DisableRight;
            destEvent.Channels = ChannelRemapping.DisableLeft;
            TrackEventGroup sourceGroup = workItem.SrcEvent.Group;
            if (null == sourceGroup)
            {
                sourceGroup = new TrackEventGroup();
                myVegas.Project.Groups.Add(sourceGroup);
                sourceGroup.Add(workItem.SrcEvent);
            }
            sourceGroup.Add(destEvent);
        }
    }
        
    class WorkItem
    {
        public readonly AudioEvent SrcEvent;
        public readonly Track DstTrack;
        public WorkItem(AudioEvent src, Track dst)
        {
            SrcEvent = src;
            DstTrack = dst;
        }
    }

    void AddSelectedAudioEvents(List<AudioEvent> events)
    {
        foreach (Track track in myVegas.Project.Tracks)
        {
            if (track.IsAudio())
            {
                foreach (AudioEvent audioEvent in track.Events)
                {
                    if (audioEvent.Selected)
                    {
                        events.Add(audioEvent);
                    }
                }
            }
        }
    }

    void AddAllAudioEvents(List<AudioEvent> events)
    {
        foreach (Track track in myVegas.Project.Tracks)
        {
            if (track.IsAudio())
            {
                foreach (AudioEvent audioEvent in track.Events)
                {
                    events.Add(audioEvent);
                }
            }
        }
    }
    
    // {07226981-7E13-45fc-825E-D54B6D63CBF1}
    Guid StereoToMonoPairCustomDataID = new Guid(0x7226981, 0x7e13, 0x45fc, 0x82, 0x5e, 0xd5, 0x4b, 0x6d, 0x63, 0xcb, 0xf1);

    AudioTrack FindDestTrack(Track srcTrack)
    {
        string dstTok = srcTrack.CustomData.GetObject(StereoToMonoPairCustomDataID) as string;
        if (String.IsNullOrEmpty(dstTok))
            return null;
        foreach (Track dstTrack in myVegas.Project.Tracks)
        {
            if (!dstTrack.IsAudio())
                continue;
            if (dstTrack == srcTrack)
                continue;
            string srcTok = dstTrack.CustomData.GetObject(StereoToMonoPairCustomDataID) as string;
            if (String.IsNullOrEmpty(srcTok))
                continue;
            if (dstTok == srcTok)
            {
                return dstTrack as AudioTrack;
            }
        }
        return null;
    }

    Track CreateDestTrack(Track srcTrack)
    {
        Track dstTrack = new AudioTrack(srcTrack.Index + 1);
        myVegas.Project.Tracks.Add(dstTrack);
        Guid pairTok = Guid.NewGuid();
        dstTrack.CustomData.SetObject(StereoToMonoPairCustomDataID, pairTok.ToString());
        srcTrack.CustomData.SetObject(StereoToMonoPairCustomDataID, pairTok.ToString());
        return dstTrack;
    }
    
}