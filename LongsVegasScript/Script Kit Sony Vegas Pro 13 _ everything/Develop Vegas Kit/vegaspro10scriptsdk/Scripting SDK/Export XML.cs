/**
 * Sample script that exports most of the current project's internal
 * data to an XML file.
 *
 * Note: Not every aspect of project data is accessible from the
 * scripting API so it is likely that information will be lost in a
 * round trip to and from Xml.  Most noticeably, you'll loose effect
 * properties.
 *
 * Revision Date: June 20, 2006.
 **/

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Globalization;
using Sony.Vegas;

public class EntryPoint
{

    NumberFormatInfo myNumberFormat = NumberFormatInfo.InvariantInfo;

    System.Text.Encoding myCharacterEncoding = System.Text.Encoding.UTF8;

    bool UseProjectRulerFormatForTimecodes = true;
    RulerFormat myTimecodeFormat = RulerFormat.Nanoseconds;

    Vegas myVegas = null;
    
    public void FromVegas(Vegas vegas) {
        myVegas = vegas;
        if (UseProjectRulerFormatForTimecodes)
            myTimecodeFormat = myVegas.Project.Ruler.Format;

        String outputFile = myVegas.Project.FilePath;
        if (!String.IsNullOrEmpty(outputFile)) {
            String fileNameWOExt = Path.GetFileNameWithoutExtension(outputFile);
            String directoryName = Path.GetDirectoryName(outputFile);
            outputFile = Path.Combine(directoryName, fileNameWOExt + ".xml");
        }

        outputFile = ShowSaveFileDialog("XML Files (*.xml)|*.xml", "XML Output File", outputFile);
        myVegas.UpdateUI();

        if (null != outputFile) {
            ExportXml(outputFile);
        }
    }

    void ExportXml(String outputFile) {
        XmlDocument doc = new XmlDocument();
        XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version=\"1.0\"");
        doc.AppendChild(xmlPI);
        XmlElement root = doc.CreateElement("Vegas");
        doc.AppendChild(root);

        ExportProject(root, myVegas.Project);
    
        XmlTextWriter writer = new XmlTextWriter(outputFile, myCharacterEncoding);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        writer.IndentChar = ' ';
        doc.WriteTo(writer);
        writer.Close();
    }

    void ExportProject(XmlElement parent, Project proj) {
        XmlElement elt = AddChild(parent, "Project");
        ChildString(elt, "FilePath", proj.FilePath);
        ChildTimecode(elt, "Length", proj.Length);
        ExportProjectVideoProperties(elt, proj.Video);
        ExportPreviewVideoProperties(elt, proj.Preview);
        ExportProjectAudioProperties(elt, proj.Audio);
        ExportRulerProperties(elt, proj.Ruler);
        ExportSummaryProperties(elt, proj.Summary);
        ExportAudioCDProperties(elt, proj.AudioCD);
        ExportMediaPool(elt, proj.MediaPool);
        ExportTracks(elt, proj.Tracks);
        ExportBusTracks(elt, proj.BusTracks);
        ExportMarkers(elt, proj.Markers, "Markers");
        ExportMarkers(elt, proj.Regions, "Regions");
        ExportMarkers(elt, proj.CDTracks, "CDTracks");
        ExportMarkers(elt, proj.CDIndices, "CDIndices");
        ExportMarkers(elt, proj.CommandMarkers, "CommandMarkers");
    }

    void ExportProjectVideoProperties(XmlElement parent, ProjectVideoProperties props) {
        XmlElement elt = AddChild(parent, "Video");
        ChildInt32(elt, "Width", props.Width);
        ChildInt32(elt, "Height", props.Height);
        ChildDouble(elt, "FrameRate", props.FrameRate);
        ChildObject(elt, "FieldOrder", props.FieldOrder);
        ChildDouble(elt, "PixelAspectRatio", props.PixelAspectRatio);
        ChildObject(elt, "RenderQuality", props.RenderQuality);
        ChildObject(elt, "MotionBlurType", props.MotionBlurType);
        ChildObject(elt, "DeinterlaceMethod", props.DeinterlaceMethod);
    }

    void ExportPreviewVideoProperties(XmlElement parent, PreviewVideoProperties props) {
        XmlElement elt = AddChild(parent, "Preview");
        ChildInt32(elt, "Width", props.Width);
        ChildInt32(elt, "Height", props.Height);
        ChildDouble(elt, "FrameRate", props.FrameRate);
        ChildObject(elt, "FieldOrder", props.FieldOrder);
        ChildDouble(elt, "PixelAspectRatio", props.PixelAspectRatio);
        ChildObject(elt, "RenderQuality", props.RenderQuality);
        ChildBoolean(elt, "FullSize", props.FullSize);
    }

    void ExportProjectAudioProperties(XmlElement parent, ProjectAudioProperties props) {
        XmlElement elt = AddChild(parent, "Audio");
        ChildUInt32(elt, "SampleRate", props.SampleRate);
        ChildUInt32(elt, "BitDepth", props.BitDepth);
        ChildObject(elt, "MasterBusMode", props.MasterBusMode);
        ChildObject(elt, "ResampleQuality", props.ResampleQuality);
        ChildBoolean(elt, "LFELowpassFilterEnabled", props.LFELowpassFilterEnabled);
        ChildUInt32(elt, "LFELowpassFilterCutoffFrequency", props.LFELowpassFilterCutoffFrequency);
        ChildObject(elt, "LFELowpassFilterQuality", props.LFELowpassFilterQuality);
    }

    void ExportRulerProperties(XmlElement parent, RulerProperties props) {
        XmlElement elt = AddChild(parent, "Ruler");
        ChildObject(elt, "Format", props.Format);
        ChildTimecode(elt, "StartTime", props.StartTime);
        ChildDouble(elt, "BeatsPerMinute", props.BeatsPerMinute);
        ChildUInt32(elt, "BeatsPerMeasure", props.BeatsPerMeasure);
        ChildObject(elt, "BeatValue", props.BeatValue);
    }

    void ExportSummaryProperties(XmlElement parent, SummaryProperties props) {
        XmlElement elt = AddChild(parent, "Summary");
        ChildString(elt, "Title", props.Title);
        ChildString(elt, "Artist", props.Artist);
        ChildString(elt, "Engineer", props.Engineer);
        ChildString(elt, "Copyright", props.Copyright);
        ChildString(elt, "Comments", props.Comments);
    }

    void ExportAudioCDProperties(XmlElement parent, AudioCDProperties props) {
        XmlElement elt = AddChild(parent, "AudioCD");
        ChildString(elt, "UPC", props.UPC);
        ChildUInt32(elt, "FirstTrack", props.FirstTrack);
    }

    void ExportMediaPool(XmlElement parent, MediaPool pool) {
        XmlElement elt = AddChild(parent, "MediaPool");
        elt.SetAttribute("Count", pool.Count.ToString(myNumberFormat));
        IEnumerator enumMedia = pool.GetEnumerator();
        while (enumMedia.MoveNext()) {
            ExportMedia(elt, (Media) enumMedia.Current);
        }
        ExportMediaBin(elt, pool.RootMediaBin);
    }

    void ExportMedia(XmlElement parent, Media media) {
        Type type = media.GetType();
        XmlElement elt = AddChild(parent, type.Name);
        ChildString(elt, "FilePath", media.FilePath);
        ChildString(elt, "KeyString", media.KeyString);
        ChildInt32(elt, "UseCount", media.UseCount);
        ChildBoolean(elt, "UseCustomTimecode", media.UseCustomTimecode);
        ChildTimecode(elt, "TimecodeIn", media.TimecodeIn);
        ChildTimecode(elt, "TimecodeOut", media.TimecodeOut);
        ChildTimecode(elt, "Length", media.Length);
        ChildObject(elt, "RulerFormat", media.RulerFormat);
        ChildString(elt, "TapeName", media.TapeName);
        ChildInt64(elt, "AverageDataRate", media.AverageDataRate);
        ChildString(elt, "Comment", media.Comment);
        if (media.IsSubclip()) {
            Subclip subclip = (Subclip) media;
            ChildString(elt, "ParentMedia", subclip.ParentMedia.KeyString);
            ChildTimecode(elt, "Start", subclip.Start);
            ChildBoolean(elt, "IsReversed", subclip.IsReversed);
        }
        if (media.IsGenerated())
            ExportEffect(elt, media.Generator, true);
        ExportMediaStreams(elt, media.Streams, media.IsOffline());
        ExportEffects(elt, media.Effects, true);
    }

    void ExportMediaStreams(XmlElement parent, MediaStreams streams, Boolean isOffline) {
        XmlElement elt = AddChild(parent, "Streams");
        elt.SetAttribute("Count", streams.Count.ToString(myNumberFormat));
        //IEnumerator enumStream = streams.GetEnumerator();
        foreach (MediaStream stream in streams) {
            ExportMediaStream(elt, stream, isOffline);
        }
    }

    void ExportMediaStream(XmlElement parent, MediaStream stream, Boolean isOffline) {
        Type type = stream.GetType();
        AudioStream audioStream = null;
        VideoStream videoStream = null;
        if (type.Name == "AudioStream")
            audioStream = (AudioStream) stream;
        else if (type.Name == "VideoStream")
            videoStream = (VideoStream) stream;
        XmlElement elt = AddChild(parent, type.Name);
        ChildObject(elt, "MediaType", stream.MediaType);
        if (isOffline) return;
        ChildTimecode(elt, "Offset", stream.Offset);
        ChildTimecode(elt, "Length", stream.Length);
        if (null != audioStream) {
            ChildObject(elt, "Format", audioStream.Format);
            ChildInt64(elt, "AverageDataRate", audioStream.AverageDataRate);
            ChildUInt32(elt, "SampleRate", audioStream.SampleRate);
            ChildUInt32(elt, "BitDepth", audioStream.BitDepth);
            ChildObject(elt, "Channels", audioStream.Channels);
        } else if (null != videoStream) {
            ChildString(elt, "Format", videoStream.Format);
            ChildInt64(elt, "AverageDataRate", videoStream.AverageDataRate);
            ChildInt32(elt, "Width", videoStream.Width);
            ChildInt32(elt, "Height", videoStream.Height);
            ChildDouble(elt, "FrameRate", videoStream.FrameRate);
            ChildObject(elt, "FieldOrder", videoStream.FieldOrder);
            ChildDouble(elt, "PixelAspectRatio", videoStream.PixelAspectRatio);
            ChildInt32(elt, "ColorDepth", videoStream.ColorDepth);
            ChildObject(elt, "AlphaChannel", videoStream.AlphaChannel);
            AddVideoColor(elt, "BackgroundColor", videoStream.BackgroundColor);
        }
    }

    void ExportMediaBin(XmlElement parent, MediaBin bin) {
        XmlElement elt = AddChild(parent, "MediaBin");
        ChildString(elt, "Name", bin.Name);
        IEnumerator enumBins = bin.GetEnumerator();
        while (enumBins.MoveNext()) {
            Type type = enumBins.Current.GetType();
            if (type.Name == "MediaBin") {
                ExportMediaBin(elt, (MediaBin) enumBins.Current);
            } else {
                Media media = (Media) enumBins.Current;
                ChildString(elt, "MediaRef", media.KeyString);
            }
        }
    }

    void ExportTracks(XmlElement parent, Tracks tracks) {
        XmlElement elt = AddChild(parent, "Tracks");
        elt.SetAttribute("Count", tracks.Count.ToString(myNumberFormat));
        foreach (Track track in tracks) {
            ExportTrack(elt, track);
        }
    }

    void ExportTrack(XmlElement parent, Track track) {
        Type type = track.GetType();
        AudioTrack audioTrack = null;
        VideoTrack videoTrack = null;
        if (type.Name == "AudioTrack")
            audioTrack = (AudioTrack) track;
        else if (type.Name == "VideoTrack")
            videoTrack = (VideoTrack) track;
        XmlElement elt = AddChild(parent, type.Name);
        ChildString(elt, "Name", track.Name);
        ChildInt32(elt, "Index", track.Index);
        ChildInt32(elt, "DisplayIndex", track.DisplayIndex);
        ChildTimecode(elt, "Length", track.Length);
        ChildObject(elt, "MediaType", track.MediaType);
        ChildBoolean(elt, "Solo", track.Solo);
        ChildBoolean(elt, "Mute", track.Mute);
        ChildBoolean(elt, "Selected", track.Selected);
        if (null != audioTrack) {
            ChildBoolean(elt, "InvertPhase", audioTrack.InvertPhase);
        } else if (null != videoTrack) {
            AddVideoColor(elt, "TopFadeColor", videoTrack.TopFadeColor);
            AddVideoColor(elt, "BottomFadeColor", videoTrack.BottomFadeColor);
        }
        if (null == track.BusTrack)
            AddChild(elt, "BusTrack");
        else
            ChildString(elt, "BusTrack", track.BusTrack.Name);
        ExportEvents(elt, track.Events);
        ExportEnvelopes(elt, track.Envelopes);
        ExportEffects(elt, track.Effects, (null != videoTrack));
    }

    void ExportEvents(XmlElement parent, TrackEvents events)
    {
        XmlElement elt = AddChild(parent, "Events");
        elt.SetAttribute("Count", events.Count.ToString(myNumberFormat));
        foreach (TrackEvent trackEvent in events) {
            ExportTrackEvent(elt, trackEvent);
        }
    }

    void ExportTrackEvent(XmlElement parent, TrackEvent trackEvent) {
        Type type = trackEvent.GetType();
        VideoEvent videoEvent = null;
        AudioEvent audioEvent = null;
        if (type.Name == "AudioEvent")
            audioEvent = (AudioEvent) trackEvent;
        else if (type.Name == "VideoEvent") 
            videoEvent = (VideoEvent) trackEvent;
        XmlElement elt = AddChild(parent, type.Name);
        ChildString(elt, "Name", trackEvent.Name);
        ChildInt32(elt, "Index", trackEvent.Index);
        ChildObject(elt, "MediaType", trackEvent.MediaType);
        ChildTimecode(elt, "Start", trackEvent.Start);
        ChildTimecode(elt, "Length", trackEvent.Length);
        ChildTimecode(elt, "End", trackEvent.End);
        ChildDouble(elt, "PlaybackRate", trackEvent.PlaybackRate);
        ChildBoolean(elt, "Mute", trackEvent.Mute);
        ChildBoolean(elt, "Locked", trackEvent.Locked);
        ChildBoolean(elt, "Loop", trackEvent.Loop);
        ChildBoolean(elt, "Selected", trackEvent.Selected);
        if (null != audioEvent) {
            ChildBoolean(elt, "Normalize", audioEvent.Normalize);
            ChildBoolean(elt, "InvertPhase", audioEvent.InvertPhase);
            ChildObject(elt, "Channels", audioEvent.Channels);
        } else if (null != videoEvent) {
            ChildDouble(elt, "UnderSampleRate", videoEvent.UnderSampleRate);
            ChildBoolean(elt, "MaintainAspectRatio", videoEvent.MaintainAspectRatio);
            ChildBoolean(elt, "ReduceInterlace", videoEvent.ReduceInterlace);
            ChildObject(elt, "ResampleMode", videoEvent.ResampleMode);
        }
        ExportTakes(elt, trackEvent.Takes);
        if (null == trackEvent.ActiveTake)
            AddChild(elt, "ActiveTake");
        else
            ChildString(elt, "ActiveTake", trackEvent.ActiveTake.Name);
        ExportFade(elt, "FadeIn", trackEvent.FadeIn);
        ExportFade(elt, "FadeOut", trackEvent.FadeOut);
        if (null != videoEvent)
        {
            ExportEnvelopes(elt, videoEvent.Envelopes);
            ExportEffects(elt, videoEvent.Effects, true);
            ExportVideoMotion(elt, videoEvent.VideoMotion);
        }
    }

    void ExportTakes(XmlElement parent, Takes takes) {
        XmlElement elt = AddChild(parent, "Takes");
        elt.SetAttribute("Count", takes.Count.ToString(myNumberFormat));
        foreach (Take take in takes) {
            ExportTake(elt, take);
        }
    }

    void ExportTake(XmlElement parent, Take take) {
        XmlElement elt = AddChild(parent, "Take");
        ChildString(elt, "Name", take.Name);
        ChildInt32(elt, "Index", take.Index);
        ChildString(elt, "MediaRef", take.Media.KeyString);
        ChildString(elt, "MediaPath", take.MediaPath);
        ChildInt32(elt, "StreamIndex", take.StreamIndex);
        ChildTimecode(elt, "Offset", take.Offset);
        ChildTimecode(elt, "Length", take.Length);
        ChildTimecode(elt, "AvailableLength", take.AvailableLength);
        ChildBoolean(elt, "IsActive", take.IsActive);
    }

    void ExportFade(XmlElement parent, String fadeName, Fade fade) {
        XmlElement elt = AddChild(parent, fadeName);
        ChildTimecode(elt, "Length", fade.Length);
        ChildObject(elt, "Curve", fade.Curve);
        ChildObject(elt, "ReciprocalCurve", fade.ReciprocalCurve);
        ChildSingle(elt, "Gain", fade.Gain);
        if (null == fade.Transition)
            AddChild(elt, "Transition");
        else
            ExportNamedEffect(elt, "Transition", fade.Transition as Effect, true);
    }

    void ExportBusTracks(XmlElement parent, BusTracks busTracks)
    {
        XmlElement elt = AddChild(parent, "BusTracks");
        elt.SetAttribute("Count", busTracks.Count.ToString(myNumberFormat));
        foreach (BusTrack busTrack in busTracks) {
            ExportBusTrack(elt, busTrack);
        }
    }

    void ExportBusTrack(XmlElement parent, BusTrack busTrack) {
        Type type = busTrack.GetType();
        AudioBusTrack audioBus = null;
        VideoBusTrack videoBus = null;
        if (type.Name == "AudioBusTrack")
            audioBus = (AudioBusTrack) busTrack;
        else if (type.Name == "VideoBusTrack")
            videoBus = (VideoBusTrack) busTrack;
        XmlElement elt = AddChild(parent, type.Name);
        ChildString(elt, "Name", busTrack.Name);
        ChildObject(elt, "MediaType", busTrack.MediaType);
        ChildString(elt, "Description", busTrack.Description);
        if (null != audioBus) {
            ChildBoolean(elt, "Mute", audioBus.Mute);
            ChildBoolean(elt, "Solo", audioBus.Solo);
            ChildObject(elt, "PanType", audioBus.PanType);
        } else if (null != videoBus) {
            AddVideoColor(elt, "TopFadeColor", videoBus.TopFadeColor);
            AddVideoColor(elt, "BottomFadeColor", videoBus.BottomFadeColor);
            ChildBoolean(elt, "Bypass", videoBus.Bypass);
        }
        ExportEnvelopes(elt, busTrack.Envelopes);
        ExportEffects(elt, busTrack.Effects, (null != videoBus));
    }

    void ExportEnvelopes(XmlElement parent, Envelopes envelopes) {
        XmlElement elt = AddChild(parent, "Envelopes");
        elt.SetAttribute("Count", envelopes.Count.ToString(myNumberFormat));
        foreach (Envelope envelope in envelopes) {
            ExportEnvelope(elt, envelope);
        }
    }

    void ExportEnvelope(XmlElement parent, Envelope envelope) {
        XmlElement elt = AddChild(parent, "Envelope");
        ChildString(elt, "Name", envelope.Name);
        ChildObject(elt, "Type", envelope.Type);
        ChildInt32(elt, "Index", envelope.Index);
        ChildDouble(elt, "Min", envelope.Min);
        ChildDouble(elt, "Max", envelope.Max);
        ChildDouble(elt, "Neutral", envelope.Neutral);
        ExportEnvelopePoints(elt, envelope.Points);
    }

    void ExportEnvelopePoints(XmlElement parent, EnvelopePoints points) {
        XmlElement elt = AddChild(parent, "Points");
        elt.SetAttribute("Count", points.Count.ToString(myNumberFormat));
        foreach (EnvelopePoint point in points) {
            ExportEnvelopePoint(elt, point);
        }
    }

    void ExportEnvelopePoint(XmlElement parent, EnvelopePoint point) {
        XmlElement elt = AddChild(parent, "Point");
        elt.SetAttribute("X", TimecodeToString(point.X));
        elt.SetAttribute("Nanos", point.X.Nanos.ToString(myNumberFormat));
        elt.SetAttribute("Y", point.Y.ToString(myNumberFormat));
        elt.SetAttribute("Curve", point.Curve.ToString());
    }

    void ExportEffects(XmlElement parent, Effects effects, Boolean hasKeyframes) {
        XmlElement elt = AddChild(parent, "Effects");
        elt.SetAttribute("Count", effects.Count.ToString(myNumberFormat));
        IEnumerator enumEffects = effects.GetEnumerator();
        foreach (Effect effect in effects) {
            ExportEffect(elt, effect, hasKeyframes);
        }
    }

    void ExportEffect(XmlElement parent, Effect effect, Boolean hasKeyframes) {
        ExportNamedEffect(parent, "Effect", effect, hasKeyframes);
    }

    void ExportNamedEffect(XmlElement parent, String effectName, Effect effect, Boolean hasKeyframes) {
        XmlElement elt = AddChild(parent, effectName);
        ChildString(elt, "PlugIn", effect.PlugIn.Name);
        ChildString(elt, "Description", effect.Description);
        ChildBoolean(elt, "ApplyBeforePanCrop", effect.ApplyBeforePanCrop);
        if (hasKeyframes)
            ExportKeyFrames(elt, effect.Keyframes);
    }

    void ExportKeyFrames(XmlElement parent, Keyframes keyframes) {
        XmlElement elt = AddChild(parent, "Keyframes");
        elt.SetAttribute("Count", keyframes.Count.ToString(myNumberFormat));
        foreach (Keyframe keyframe in keyframes) {
            ExportKeyframe(elt, keyframe);
        }
    }

    void ExportKeyframe(XmlElement parent, Keyframe keyframe) {
        XmlElement elt = AddChild(parent, "Keyframe");
        ChildTimecode(elt, "Position", keyframe.Position);
        ChildObject(elt, "Type", keyframe.Type);
    }

    void ExportMarkers(XmlElement parent, IList markers, String type) {
        XmlElement elt = AddChild(parent, type);
        elt.SetAttribute("Count", markers.Count.ToString(myNumberFormat));
        foreach (Marker marker in markers) {
            ExportMarker(elt, marker);
        }
    }

    void ExportMarker(XmlElement parent, Marker marker) {
        Type type = marker.GetType();
        XmlElement elt = AddChild(parent, type.Name);
        ChildTimecode(elt, "Position", marker.Position);
        switch (type.Name) {
            case "CDMarker":
            case "Marker":
                ChildString(elt, "Label", marker.Label);
                break;
            case "Region":
            case "CDRegion":
                Region region = (Region) marker;
                ChildTimecode(elt, "Length", region.Length);
                ChildTimecode(elt, "End", region.End);
                ChildString(elt, "Label", region.Label);
                break;
            case "CommandMarker":
                CommandMarker commandMarker = (CommandMarker) marker;
                ChildObject(elt, "CommandType", commandMarker.CommandType);
                ChildString(elt, "CommandParameter", commandMarker.CommandParameter);
                ChildString(elt, "Comment", commandMarker.Comment);
                break;
            default:
                break;
        }
    }

    void ExportVideoMotion(XmlElement parent, VideoMotion videoMotion) {
        XmlElement elt = AddChild(parent, "VideoMotion");
        ChildBoolean(elt, "ScaleToFill", videoMotion.ScaleToFill);
        ExportViideoMotionKeyframes(elt, videoMotion.Keyframes);
    }

    void ExportViideoMotionKeyframes(XmlElement parent, VideoMotionKeyframes keyframes) {
        XmlElement elt = AddChild(parent, "Keyframes");
        elt.SetAttribute("Count", keyframes.Count.ToString(myNumberFormat));
        foreach (VideoMotionKeyframe keyframe in keyframes) {
            ExportVideoMotionKeyframe(elt, keyframe);
        }
    }

    void ExportVideoMotionKeyframe(XmlElement parent, VideoMotionKeyframe keyframe) {
        XmlElement elt = AddChild(parent, "VideoMotionKeyframe");
        ChildObject(elt, "Type", keyframe.Type);
        ChildTimecode(elt, "Position", keyframe.Position);
        ChildSingle(elt, "Smoothness", keyframe.Smoothness);
        ChildDouble(elt, "Rotation", keyframe.Rotation);
        ExportVideoMotionBounds(elt, keyframe.Bounds, "Bounds");
        ExportVideoMotionVertex(elt, keyframe.Center, "Center");
    }

    void ExportVideoMotionBounds(XmlElement parent, VideoMotionBounds bounds, String boundsName) {
        XmlElement elt = AddChild(parent, boundsName);
        ExportVideoMotionVertex(elt, bounds.TopLeft, "TopLeft");
        ExportVideoMotionVertex(elt, bounds.TopRight, "TopRight");
        ExportVideoMotionVertex(elt, bounds.BottomRight, "BottomRight");
        ExportVideoMotionVertex(elt, bounds.BottomLeft, "BottomLeft");
    }

    void ExportVideoMotionVertex(XmlElement parent, VideoMotionVertex vertex, String location) {
        XmlElement elt = AddChild(parent, location);
        elt.SetAttribute("X", vertex.X.ToString(myNumberFormat));
        elt.SetAttribute("Y", vertex.Y.ToString(myNumberFormat));
    }

    XmlElement AddVideoColor(XmlElement parent, String childName, VideoColor childValue) {
        XmlElement child = AddChild(parent, childName);
        child.SetAttribute("red", childValue.R.ToString(myNumberFormat));
        child.SetAttribute("green", childValue.G.ToString(myNumberFormat));
        child.SetAttribute("blue", childValue.B.ToString(myNumberFormat));
        child.SetAttribute("alpha", childValue.A.ToString(myNumberFormat));
        return child;
    }

    XmlElement AddChild(XmlElement parent, String childName) {
        return ChildString(parent, childName, null);
    }

    // For Timecodes, both the string and nanos are exported so that
    // importing them can be precise and flexible.
    XmlElement ChildTimecode(XmlElement parent, String childName, Timecode childValue) {
        XmlElement elt = ChildString(parent, childName, TimecodeToString(childValue));
        elt.SetAttribute("Nanos", childValue.Nanos.ToString(myNumberFormat));
        return elt;
    }

    XmlElement ChildBoolean(XmlElement parent, String childName, Boolean childValue) {
        return ChildString(parent, childName, childValue.ToString());
    }

    XmlElement ChildDouble(XmlElement parent, String childName, Double childValue) {
        return ChildString(parent, childName, childValue.ToString(myNumberFormat));
    }

    XmlElement ChildSingle(XmlElement parent, String childName, Single childValue) {
        return ChildString(parent, childName, childValue.ToString(myNumberFormat));
    }

    XmlElement ChildInt64(XmlElement parent, String childName, Int64 childValue) {
        return ChildString(parent, childName, childValue.ToString(myNumberFormat));
    }

    XmlElement ChildInt32(XmlElement parent, String childName, Int32 childValue) {
        return ChildString(parent, childName, childValue.ToString(myNumberFormat));
    }

    XmlElement ChildUInt32(XmlElement parent, String childName, UInt32 childValue) {
        return ChildString(parent, childName, childValue.ToString(myNumberFormat));
    }

    XmlElement ChildObject(XmlElement parent, String childName, Object childValue) {
        return ChildString(parent, childName, childValue.ToString());
    }

    XmlElement ChildString(XmlElement parent, String childName, String childValue) {
        XmlElement child = parent.OwnerDocument.CreateElement(childName);
        parent.AppendChild(child);
        if (null != childValue)
            child.InnerText = childValue.ToString();
        return child;
    }

    String TimecodeToString(Timecode timecode) {
        return timecode.ToString(myTimecodeFormat);
    }


    String ShowSaveFileDialog(String filter, String title, String defaultFilename) {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        if (null == filter) {
            filter = "All Files (*.*)|*.*";
        }
        saveFileDialog.Filter = filter;
        if (null != title)
            saveFileDialog.Title = title;
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.AddExtension = true;
        if (null != defaultFilename) {
            String initialDir = Path.GetDirectoryName(defaultFilename);
            if (Directory.Exists(initialDir)) {
                saveFileDialog.InitialDirectory = initialDir;
            }
            saveFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
            saveFileDialog.FileName = Path.GetFileName(defaultFilename);
        }
        if (DialogResult.OK == saveFileDialog.ShowDialog()) {
            return Path.GetFullPath(saveFileDialog.FileName);
        } else {
            return null;
        }
    }

}

