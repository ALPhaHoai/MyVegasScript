/**
 * Sample script that creates a project from an XML file.  You can use
 * the Export XML.js sample script to create a sample XML file from
 * any project.  Then you can make manual modifications to the XML
 * file (in much the same way one might edit an EDL) and use this
 * script to bring in the changes.
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

    bool UseProjectRulerFormatForTimecodes = true;
    RulerFormat myTimecodeFormat = RulerFormat.Nanoseconds;

    // use a hashtable to keep track of new generated media keys which
    // can be different from the keys that were exported.
    Hashtable myGeneratedMediaKeys = new Hashtable();

    Vegas myVegas = null;

    public void FromVegas(Vegas vegas) {
        myVegas = vegas;
        String inputFile = ShowOpenFileDialog("Xml Files *.xml|*.xml", "Import XML", null);
        if (null != inputFile) {
            ImportXml(inputFile);
        }
    }

    void ImportXml(String inputFile) {
        Boolean wasCreated = myVegas.NewProject(true, false);
        if (!wasCreated)
            return;
        XmlDocument doc = new XmlDocument();
        doc.Load(inputFile);
        XmlElement root = doc.DocumentElement;
        ImportProject(root, myVegas.Project);
    }

    void ImportProject(XmlElement parent, Project proj) {
        XmlElement elt = parent["Project"];
        // import ruler properties first so that all timecodes will be
        // parsed using the proper project ruler format.
        ImportRulerProperties(elt, proj.Ruler);
        if (UseProjectRulerFormatForTimecodes)
            myTimecodeFormat = myVegas.Project.Ruler.Format;
        ImportProjectVideoProperties(elt, proj.Video);
        ImportPreviewVideoProperties(elt, proj.Preview);
        ImportProjectAudioProperties(elt, proj.Audio);
        ImportSummaryProperties(elt, proj.Summary);
        ImportAudioCDProperties(elt, proj.AudioCD);
        ImportMediaPool(elt, proj.MediaPool);
        ImportBusTracks(elt, proj.BusTracks);
        ImportTracks(elt, proj.Tracks);
        ImportMarkers(elt, proj.Markers, "Markers");
        ImportMarkers(elt, proj.Regions, "Regions");
        ImportMarkers(elt, proj.CDTracks, "CDTracks");
        ImportMarkers(elt, proj.CDIndices, "CDIndices");
        ImportMarkers(elt, proj.CommandMarkers, "CommandMarkers");
    }

    void ImportRulerProperties(XmlElement parent, RulerProperties props) {
        XmlElement elt = parent["Ruler"];
        if (null == elt) return;
        try { props.Format = ChildRulerFormat(elt, "Format"); } catch {}
        try { props.StartTime = ChildTimecode(elt, "StartTime"); } catch {}
        try { props.BeatsPerMinute = ChildDouble(elt, "BeatsPerMinute"); } catch {}
        try { props.BeatsPerMeasure = ChildUInt32(elt, "BeatsPerMeasure"); } catch {}
        try { props.BeatValue = ChildBeatValue(elt, "BeatValue"); } catch {}
    }

    void ImportProjectVideoProperties(XmlElement parent, ProjectVideoProperties props) {
        XmlElement elt = parent["Video"];
        if (null == elt) return;
        try { props.Width = ChildInt32(elt, "Width"); } catch {}
        try { props.Height = ChildInt32(elt, "Height"); } catch {}
        try { props.FrameRate = ChildDouble(elt, "FrameRate"); } catch {}
        try { props.FieldOrder = ChildFieldOrder(elt, "FieldOrder"); } catch {}
        try { props.PixelAspectRatio = ChildDouble(elt, "PixelAspectRatio"); } catch {}
        try { props.RenderQuality = ChildVideoRenderQuality(elt, "RenderQuality"); } catch {}
        try { props.MotionBlurType = ChildMotionBlurType(elt, "MotionBlurType"); } catch {}
        try { props.DeinterlaceMethod = ChildVideoDeinterlaceMethod(elt, "DeinterlaceMethod"); } catch {}
    }

    void ImportPreviewVideoProperties(XmlElement parent, PreviewVideoProperties props) {
        XmlElement elt = parent["Preview"];
        if (null == elt) return;
        try { props.RenderQuality = ChildVideoRenderQuality(elt, "RenderQuality"); } catch {}
        try { props.FullSize = ChildBoolean(elt, "FullSize"); } catch {}
    }

    void ImportProjectAudioProperties(XmlElement parent, ProjectAudioProperties props) {
        XmlElement elt = parent["Audio"];
        if (null == elt) return;
        try { props.SampleRate = ChildUInt32(elt, "SampleRate"); } catch {}
        try { props.BitDepth = ChildUInt32(elt, "BitDepth"); } catch {}
        try { props.MasterBusMode = ChildAudioBusMode(elt, "MasterBusMode"); } catch {};
        try { props.ResampleQuality = ChildAudioResampleQuality(elt, "ResampleQuality"); } catch {}
        try { props.LFELowpassFilterEnabled = ChildBoolean(elt, "LFELowpassFilterEnabled"); } catch {}
        try { props.LFELowpassFilterCutoffFrequency = ChildUInt32(elt, "LFELowpassFilterCutoffFrequency"); } catch {}
        try { props.LFELowpassFilterQuality = ChildLowPassFilterQuality(elt, "LFELowpassFilterQuality"); } catch {}
    }

    void ImportAudioCDProperties(XmlElement parent, AudioCDProperties props) {
        XmlElement elt = parent["AudioCD"];
        if (null == elt) return;
        try { props.UPC = ChildString(elt, "UPC"); } catch {}
        try { props.FirstTrack = ChildUInt32(elt, "FirstTrack"); } catch {}
    }
    
    void ImportSummaryProperties(XmlElement parent, SummaryProperties props) {
        XmlElement elt = parent["Summary"];
        if (null == elt) return;
        try { props.Title = ChildString(elt, "Title"); } catch {}
        try { props.Artist = ChildString(elt, "Artist"); } catch {}
        try { props.Engineer = ChildString(elt, "Engineer"); } catch {}
        try { props.Copyright = ChildString(elt, "Copyright"); } catch {}
        try { props.Comments = ChildString(elt, "Comments"); } catch {}
    }

    void ImportMediaPool(XmlElement parent, MediaPool pool) {
        XmlElement elt = parent["MediaPool"];
        if (null == elt) return;
        foreach (XmlElement mediaElt in elt.SelectNodes("Media")) {
            ImportMedia(mediaElt, pool);
        }
        foreach (XmlElement subclipElt in elt.SelectNodes("Subclip")) {
            ImportSubclip(subclipElt, pool);
        }
        ImportMediaBin(elt["MediaBin"], pool.RootMediaBin, pool);
    }

    void ImportMedia(XmlElement elt, MediaPool pool) {
        String key = ChildString(elt, "KeyString");
        Media media = null;
        if (IsGeneratedMediaKey(key)) {
            XmlElement effectElt = elt["Effect"];
            if (null == effectElt) return;
            String plugInName = ChildString(effectElt, "PlugIn");
            PlugInNode plugIn = myVegas.Generators.GetChildByName(plugInName);
            if (null == plugIn) return;
            media = new Media(plugIn);
            myGeneratedMediaKeys[key] = media.KeyString;
        } else {
            media = new Media(key);
        }
        try { media.UseCustomTimecode = ChildBoolean(elt, "UseCustomTimecode"); } catch { }
        if (media.IsOffline() || media.UseCustomTimecode) {
            media.TimecodeIn = ChildTimecode(elt, "TimecodeIn");
        }
        if (media.IsOffline()) {
            Timecode tcOut = ChildTimecode(elt, "TimecodeOut");
            if (0 >= tcOut.Nanos) {
                tcOut = ChildTimecode(elt, "Length");
                if (0 >= tcOut.Nanos) {
                    throw new ApplicationException("offline media duration not specified.");
                }
                tcOut = media.TimecodeIn + tcOut;
            }
            media.TimecodeOut = tcOut;
        }
        ImportMediaInternal(elt, media);
    }

    void ImportSubclip(XmlElement elt, MediaPool pool) {
        String parentMediaKey = ChildString(elt, "ParentMedia");
        Media parentMedia = FindMedia(pool, parentMediaKey);
        if (null == parentMedia)
            throw new ApplicationException("could not find subclip's parent media: " + parentMediaKey);
        Timecode start = ChildTimecode(elt, "Start");
        Timecode length = ChildTimecode(elt, "Length");
        Boolean reverse = false;
        try { reverse = ChildBoolean(elt, "IsReversed"); } catch {}
        String displayName = null;
        try { displayName = ChildString(elt, "FilePath"); } catch {}
        Subclip subclip = new Subclip(parentMediaKey, start, length, reverse, displayName);
        ImportMediaInternal(elt, subclip);
    }

    void ImportMediaInternal(XmlElement elt, Media media) {
        try { media.TapeName = ChildString(elt, "TapeName"); } catch {}
        try { media.Comment = ChildString(elt, "Comment"); } catch {}
        ImportStreams(elt, media);
        ImportEffects(elt, media.Effects, myVegas.VideoFX);
    }

    void ImportStreams(XmlElement parent, Media media) {
        XmlElement elt = parent["Streams"];
        if (null == elt) return;
        Int32 streamIndex = 0;
        foreach (XmlElement child in elt) {
            ImportStream(child, media, streamIndex++);
        }
    }

    void ImportStream(XmlElement parent, Media media, Int32 streamIndex) {
        MediaStream stream = null;
        if (media.IsOffline()) {
            if (parent.Name == "AudioStream") {
                stream = media.CreateOfflineStream(MediaType.Audio);
            } else if (parent.Name == "VideoStream") {
                stream = media.CreateOfflineStream(MediaType.Video);
            }
        } else {
            stream = (MediaStream) media.Streams[streamIndex];
            AudioStream audioStream = null;
            VideoStream videoStream = null;
            if (parent.Name == "AudioStream") {
                audioStream = (AudioStream) stream;
            } else if (parent.Name == "VideoStream") {
                videoStream = (VideoStream) stream;
                try { videoStream.FieldOrder = ChildFieldOrder(parent, "FieldOrder"); } catch {}
                try { videoStream.PixelAspectRatio = ChildDouble(parent, "PixelAspectRatio"); } catch {}
                try { videoStream.AlphaChannel = ChildVideoAlphaType(parent, "AlphaChannel"); } catch {}
                try { videoStream.BackgroundColor = ChildVideoColor(parent, "BackgroundColor"); } catch {}
            }
        }
    }

    void ImportMediaBin(XmlElement elt, MediaBin bin, MediaPool pool) {
        if (null == elt) return;
        foreach (XmlElement mediaElt in elt.SelectNodes("./MediaRef")) {
            String mediaKey = mediaElt.InnerText;
            Media media = FindMedia(pool, mediaKey);
            if (null != media) {
                bin.Add(media);
            }
        }
        foreach (XmlElement subBinElt in elt.SelectNodes("./MediaBin")) {
            String subBinName = ChildString(subBinElt, "Name");
            MediaBin subBin = new MediaBin(subBinName);
            bin.Add(subBin);
            ImportMediaBin(subBinElt, subBin, pool);
        }
    }

    void ImportBusTracks(XmlElement parent, BusTracks busTracks) {
        XmlElement elt = parent["BusTracks"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportBusTrack(child, busTracks);
        }
    }

    void ImportBusTrack(XmlElement parent, BusTracks busTracks) {
        BusTrack busTrack = null;
        AudioBusTrack audioBusTrack = null;
        VideoBusTrack videoBusTrack = null;
        String busName = ChildString(parent, "Name");
        if (parent.Name == "AudioBusTrack") {
            if ((busName == "Master") || (busName == "Surround Master")) {
                audioBusTrack = (AudioBusTrack) busTracks[1];
            } else {
                audioBusTrack = new AudioBusTrack();
                busTracks.Add(audioBusTrack);
                if (audioBusTrack.Name != busName)
                    throw new ApplicationException("bad audio bus track order");
            }
            busTrack = (BusTrack) audioBusTrack;
            try { busTrack.Description = ChildString(parent, "Description"); } catch {}
            try { audioBusTrack.Mute = ChildBoolean(parent, "Mute"); } catch {}
            try { audioBusTrack.Solo = ChildBoolean(parent, "Solo"); } catch {}
            try { audioBusTrack.PanType = ChildPanType(parent, "PanType"); } catch {}
            ImportEffects(parent, busTrack.Effects, myVegas.AudioFX);
        } else if (parent.Name == "VideoBusTrack") {
            videoBusTrack = (VideoBusTrack) busTracks[0];
            busTrack = (BusTrack) videoBusTrack;
            ImportEffects(parent, busTrack.Effects, myVegas.VideoFX);
        }
        ImportEnvelopes(parent, busTrack.Envelopes);
    }

    void ImportTracks(XmlElement parent, Tracks tracks) {
        XmlElement elt = parent["Tracks"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportTrack(child, tracks);
        }
    }

    void ImportTrack(XmlElement parent, Tracks tracks) {
        Track track = null;
        AudioTrack audioTrack = null;
        VideoTrack videoTrack = null;
        if (parent.Name == "AudioTrack") {
            audioTrack = new AudioTrack();
            track = (Track) audioTrack;
        } else if (parent.Name == "VideoTrack") {
            videoTrack = new VideoTrack();
            track = (Track) videoTrack;
        }        
        tracks.Add(track);
        try { track.Name = ChildString(parent, "Name"); } catch {}
        try { track.Solo = ChildBoolean(parent, "Solo"); } catch {}
        try { track.Mute = ChildBoolean(parent, "Mute"); } catch {}
        try { track.Selected = ChildBoolean(parent, "Selected"); } catch {}
        if (null != audioTrack) {
            try { audioTrack.InvertPhase = ChildBoolean(parent, "InvertPhase"); } catch {}
            // needs bus track assignment
        } else if (null != videoTrack) {
            try { videoTrack.TopFadeColor = ChildVideoColor(parent, "TopFadeColor"); } catch {}
            try { videoTrack.BottomFadeColor = ChildVideoColor(parent, "BottomFadeColor"); } catch {}
        }
        ImportTrackEvents(parent, track.Events);
        ImportEnvelopes(parent, track.Envelopes);
        if (null != audioTrack)
            ImportEffects(parent, track.Effects, myVegas.AudioFX);
        else if (null != videoTrack)
            ImportEffects(parent, track.Effects, myVegas.VideoFX);
    }

    void ImportTrackEvents(XmlElement parent, TrackEvents events) {
        XmlElement elt = parent["Events"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportTrackEvent(child, events);
        }
    }

    void ImportTrackEvent(XmlElement parent, TrackEvents events) {
        TrackEvent trackEvent = null;
        AudioEvent audioEvent = null;
        VideoEvent videoEvent = null;
        if (parent.Name == "AudioEvent") {
            audioEvent = new AudioEvent();
            trackEvent = (TrackEvent) audioEvent;
        } else if (parent.Name == "VideoEvent") {
            videoEvent = new VideoEvent();
            trackEvent = (TrackEvent) videoEvent;
        }        
        events.Add(trackEvent);
        try { trackEvent.Name = ChildString(parent, "Name"); } catch {}
        trackEvent.Start = ChildTimecode(parent, "Start");
        Timecode length = ChildTimecode(parent, "Length");
        if (0 >= length.Nanos) {
            throw new ApplicationException("invalid event length");
        }
        trackEvent.Length = length;
        try { trackEvent.PlaybackRate = ChildDouble(parent, "PlaybackRate"); } catch {}
        try { trackEvent.Mute = ChildBoolean(parent, "Mute"); } catch {}
        try { trackEvent.Locked = ChildBoolean(parent, "Locked"); } catch {}
        try { trackEvent.Loop = ChildBoolean(parent, "Loop"); } catch {}
        try { trackEvent.Selected = ChildBoolean(parent, "Selected"); } catch {}
        if (null != audioEvent) {
            try { audioEvent.Normalize = ChildBoolean(parent, "Normalize"); } catch {}
            try { audioEvent.InvertPhase = ChildBoolean(parent, "InvertPhase"); } catch {}
            try { audioEvent.Channels = ChildChannelRemapping(parent, "Channels"); } catch {}
        } else if (null != videoEvent) {
            try { videoEvent.UnderSampleRate = ChildDouble(parent, "UnderSampleRate"); } catch {}
            try { videoEvent.MaintainAspectRatio = ChildBoolean(parent, "MaintainAspectRatio"); } catch {}
            try { videoEvent.ReduceInterlace = ChildBoolean(parent, "ReduceInterlace"); } catch {}
            try { videoEvent.ResampleMode = ChildVideoResampleMode(parent, "ResampleMode"); } catch {}
        }
        Take activeTake = ImportTakes(parent, trackEvent.Takes, trackEvent.MediaType);
        if (null != activeTake)
            trackEvent.ActiveTake = activeTake;
        ImportFade(parent, "FadeIn", trackEvent.FadeIn);
        ImportFade(parent, "FadeOut", trackEvent.FadeOut);
        if (null != videoEvent) {
            ImportEnvelopes(parent, videoEvent.Envelopes);
            ImportEffects(parent, videoEvent.Effects, myVegas.VideoFX);
            ImportVideoMotion(parent, videoEvent.VideoMotion);
        }
    }

    Take ImportTakes(XmlElement parent, Takes takes, MediaType mediaType) {
        XmlElement elt = parent["Takes"];
        if (null == elt) return null;
        Take activeTake = null;
        foreach (XmlElement child in elt) {
            Take take = ImportTake(child, takes, mediaType);
            if (null != take)
                activeTake = take;
        }
        return activeTake;
    }

    Take ImportTake(XmlElement parent, Takes takes, MediaType mediaType) {
        String mediaKey = ChildString(parent, "MediaRef");
        Media media = FindMedia(myVegas.Project.MediaPool, mediaKey);
        if (null == media) return null;
        Int32 streamIndex = ChildInt32(parent, "StreamIndex");
        MediaStream stream = media.Streams.GetItemByMediaType(mediaType, streamIndex);
        if (null == stream)
            throw new ApplicationException("bad index for take media stream: " + mediaKey);
        Take take = new Take(stream);
        takes.Add(take);
        try { take.Name = ChildString(parent, "Name"); } catch {}
        try { take.Offset = ChildTimecode(parent, "Offset"); } catch {}
        Boolean isActive = false;
        try { isActive = ChildBoolean(parent, "IsActive"); } catch {}
        if (isActive)
            return take;
        else
            return null;
    }

    void ImportFade(XmlElement parent, String fadeName, Fade fade) {
        XmlElement elt = parent[fadeName];
        if (null == elt) return;
        try { fade.Length = ChildTimecode(elt, "Length"); } catch {}
        try { fade.Curve = ChildCurveType(elt, "Curve"); } catch {}
        try { fade.ReciprocalCurve = ChildCurveType(elt, "ReciprocalCurve"); } catch {}
        try { fade.Gain = ChildSingle(elt, "Gain"); } catch {}
        XmlElement transitionElt = parent["Transition"];
        if (null == transitionElt) return;
        Effect transition = CreateEffect(transitionElt, myVegas.Transitions);
        if (null == transition) return;
        fade.Transition = transition;
        ImportKeyframes(transitionElt, transition.Keyframes);
    }

    void ImportEnvelopes(XmlElement parent, Envelopes envelopes) {
        XmlElement elt = parent["Envelopes"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportEnvelope(child, envelopes);
        }
    }

    void ImportEnvelope(XmlElement elt, Envelopes envelopes) {
        EnvelopeType type = ChildEnvelopeType(elt, "Type");
        Boolean isSurroundMode = (myVegas.Project.Audio.MasterBusMode == AudioBusMode.Surround);
        Envelope envelope = null;
        switch (type) {
            case EnvelopeType.PanY:
            case EnvelopeType.PanSmoothness:
            case EnvelopeType.PanCenter:
                // skip surround-related envelopes if not in surround mode
                if (!isSurroundMode)
                    return;
                // make sure the surround pan position x envelope exists
                Envelope panEnvelope = envelopes.FindByType(EnvelopeType.Pan);
                if (null == panEnvelope) {
                    panEnvelope = new Envelope(EnvelopeType.Pan);
                    envelopes.Add(panEnvelope);
                }
                envelope = envelopes.FindByType(type);
                if (null == envelope)
                    throw new ApplicationException("Failed to create envelope of type: " + type.ToString());
                break;
            default:
                envelope = new Envelope(type);
                envelopes.Add(envelope);
                break;
        }
        ImportPoints(elt, envelope.Points);
    }

    void ImportPoints(XmlElement parent, EnvelopePoints points) {
        XmlElement elt = parent["Points"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportPoint(child, points);
        }
    }

    void ImportPoint(XmlElement elt, EnvelopePoints points) {
        Timecode x = AttributeTimecode(elt, "X");
        Double y = AttributeDouble(elt, "Y");
        CurveType curve = AttributeCurveType(elt, "Curve");
        EnvelopePoint point = points.GetPointAtX(x);
        if (null == point) {
            point = new EnvelopePoint(x, y, curve);
            points.Add(point);
        } else {
            point.Y = y;
            point.Curve = curve;
        }
    }

    void ImportEffects(XmlElement parent, Effects effects, PlugInNode rootNode) {
        XmlElement elt = parent["Effects"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportEffect(child, effects, rootNode);
        }
    }

    void ImportEffect(XmlElement parent, Effects effects, PlugInNode rootNode) {
        Effect effect = CreateEffect(parent, rootNode);
        if (null == effect) return;
        effects.Add(effect);
        ImportKeyframes(parent, effect.Keyframes);
    }

    Effect CreateEffect(XmlElement elt, PlugInNode rootNode) {
        if (!elt.HasChildNodes) return null;
        String plugInName = ChildString(elt, "PlugIn");
        switch (plugInName) {
            case "Track Noise Gate":
            case "Track EQ":
            case "Track Compressor":
                // ignore default audio track effects
                return null;
        }
        PlugInNode plugIn = rootNode.GetChildByName(plugInName);
        if (null == plugIn) return null;
        Effect effect = new Effect(plugIn);
        return effect;
    }

    void ImportKeyframes(XmlElement parent, Keyframes keyframes) {
        XmlElement elt = parent["Keyframes"];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportKeyframe(child, keyframes);
        }
    }

    void ImportKeyframe(XmlElement parent, Keyframes keyframes) {
        Timecode position = ChildTimecode(parent, "Position");
        Keyframe keyframe = null;
        if (0 == position.Nanos)
        {
            keyframe = (Keyframe) keyframes[0];
        } else {
            keyframe = new Keyframe(position);
            keyframes.Add(keyframe);
        }
        try { keyframe.Type = ChildVideoKeyframeType(parent, "Type"); } catch {}
    }

    void ImportMarkers(XmlElement parent, IList markers, String type) {
        XmlElement elt = parent[type];
        if (null == elt) return;
        foreach (XmlElement child in elt) {
            ImportMarker(child, markers, type);
        }
    }

    void ImportMarker(XmlElement parent, IList markers, String type) {
        Timecode position = ChildTimecode(parent, "Position");
        Marker marker = null;
        String label = String.Empty;
        Timecode length = null;
        switch (type) {
            case "Markers":
                try { label = ChildString(parent, "Label"); } catch {}
                marker = new Marker(position, label);
                break;
            case "CDIndices":
                try { label = ChildString(parent, "Label"); } catch {}
                marker = new CDMarker(position, label);
                break;
            case "Regions":
                length = ChildTimecode(parent, "Length");
                try { label = ChildString(parent, "Label"); } catch {}
                marker = new Region(position, length, label);
                break;
            case "CDTracks":
                length = ChildTimecode(parent, "Length");
                try { label = ChildString(parent, "Label"); } catch {}
                marker = new CDRegion(position, length);
                break;
            case "CommandMarkers":
                MarkerCommandType command = new MarkerCommandType(ChildString(parent, "CommandType"));
                String param = String.Empty;
                try { ChildString(parent, "CommandParameter"); } catch {}
                String comment = String.Empty;
                try { ChildString(parent, "Comment"); } catch {}
                marker = new CommandMarker(position, command, param, comment);
                break;
            default:
                throw new ApplicationException("Unknown marker type: "  + type);
        }
        markers.Add(marker);
    }

    void ImportVideoMotion(XmlElement parent, VideoMotion videoMotion) {
        XmlElement elt = parent["VideoMotion"];
        if (null == elt) return;
        try { videoMotion.ScaleToFill = ChildBoolean(elt, "ScaleToFill"); } catch {}
        ImportVideoMotionKeyframes(elt, videoMotion.Keyframes);
    }

    void ImportVideoMotionKeyframes(XmlElement parent, VideoMotionKeyframes keyframes) {
        XmlElement elt = parent["Keyframes"];
        if (null == elt) return;
        foreach (XmlElement child in elt.SelectNodes("VideoMotionKeyframe")) {
            ImportVideoMotionKeyframe(child, keyframes);
        }
    }

    void ImportVideoMotionKeyframe(XmlElement parent, VideoMotionKeyframes keyframes) {
        Timecode position = ChildTimecode(parent, "Position");
        VideoMotionKeyframe keyframe;
        if (position.Nanos == 0)
            keyframe = (VideoMotionKeyframe) keyframes[0];
        else
        {
            keyframe = new VideoMotionKeyframe(position);
            keyframes.Add(keyframe);
        }
        try { keyframe.Type = ChildVideoKeyframeType(parent, "Type"); } catch {}
        try { keyframe.Smoothness = ChildSingle(parent, "Smoothness"); } catch {}
        try { keyframe.Center = ChildVideoMotionVertex(parent, "Center"); } catch {}
        try { keyframe.Rotation = ChildDouble(parent, "Rotation"); } catch {}
        keyframe.Bounds = ChildVideoMotionBounds(parent, "Bounds");
    }

    VideoMotionBounds ChildVideoMotionBounds(XmlElement parent, String boundsName) {
        XmlElement elt = parent[boundsName];
        VideoMotionVertex topLeft = ChildVideoMotionVertex(elt, "TopLeft");
        VideoMotionVertex topRight = ChildVideoMotionVertex(elt, "TopRight");
        VideoMotionVertex bottomRight = ChildVideoMotionVertex(elt, "BottomRight");
        VideoMotionVertex bottomLeft = ChildVideoMotionVertex(elt, "BottomLeft");
        return new VideoMotionBounds(topLeft, topRight, bottomRight, bottomLeft);
    }

    VideoMotionVertex ChildVideoMotionVertex(XmlElement parent, String vertexName) {
        XmlElement elt = parent[vertexName];
        if (null == elt)
            throw new ApplicationException("video motion vertex missing: " + vertexName);
        Single x = AttributeSingle(elt, "X");
        Single y = AttributeSingle(elt, "Y");
        return new VideoMotionVertex(x, y);
    }

    VideoColor ChildVideoColor(XmlElement parent, String childName) {
        XmlElement elt = parent[childName];
        if (null == elt)
            throw new ApplicationException("Element not found: " + childName);
        Byte red = AttributeByte(elt, "red");
        Byte green = AttributeByte(elt, "green");
        Byte blue = AttributeByte(elt, "blue");
        Byte alpha = 255;
        try { alpha = AttributeByte(elt, "alpha"); } catch {}
        return new VideoColor(red, green, blue, alpha); 
    }

    Timecode ChildTimecode(XmlElement parent, String childName) {
        XmlElement childElt = parent[childName];
        if (null == childElt) new Timecode();
        // first try to get the Nanos attribute
        String nanoVal = childElt.GetAttribute("Nanos");
        if (!String.IsNullOrEmpty(nanoVal)) {
            return Timecode.FromNanos(Int64.Parse(nanoVal, myNumberFormat));
        }
        // fall back to using the string representation
        String val = childElt.InnerText;
        return Timecode.FromString(val, myTimecodeFormat);
    }

    RulerFormat ChildRulerFormat(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) {
            case "Samples":
                return RulerFormat.Samples;
            case "Time":
                return RulerFormat.Time;
            case "Seconds":
                return RulerFormat.Seconds;
            case "TimeAndFrames":
                return RulerFormat.TimeAndFrames;
            case "AbsoluteFrames":
                return RulerFormat.AbsoluteFrames;
            case "MeasuresAndBeats":
                return RulerFormat.MeasuresAndBeats;
            case "SmpteNonDrop":
                return RulerFormat.SmpteNonDrop;
            case "Smpte30":
                return RulerFormat.Smpte30;
            case "SmpteDrop":
                return RulerFormat.SmpteDrop;
            case "SmpteEBU":
                return RulerFormat.SmpteEBU;
            case "SmpteFilmSync":
                return RulerFormat.SmpteFilmSync;
            case "FeetAndFrames16mm":
                return RulerFormat.FeetAndFrames16mm;
            case "FeetAndFrames35mm":
                return RulerFormat.FeetAndFrames35mm;
            case "AudioCDTime":
                return RulerFormat.AudioCDTime;
            case "SmpteFilmSyncIVTC":
                return RulerFormat.SmpteFilmSyncIVTC;
            default:
                throw new ApplicationException("Unknown RulerFormat: " + val);
        }
    }

    EnvelopeType ChildEnvelopeType(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) {
            case "Volume":
                return EnvelopeType.Volume;
            case "Pan":
                return EnvelopeType.Pan;
            case "BusA":
                return EnvelopeType.BusA;
            case "BusB":
                return EnvelopeType.BusB;
            case "BusC":
                return EnvelopeType.BusC;
            case "BusD":
                return EnvelopeType.BusD;
            case "BusE":
                return EnvelopeType.BusE;
            case "BusF":
                return EnvelopeType.BusF;
            case "BusG":
                return EnvelopeType.BusG;
            case "BusH":
                return EnvelopeType.BusH;
            case "BusI":
                return EnvelopeType.BusI;
            case "BusJ":
                return EnvelopeType.BusJ;
            case "BusK":
                return EnvelopeType.BusK;
            case "BusL":
                return EnvelopeType.BusL;
            case "BusM":
                return EnvelopeType.BusM;
            case "BusN":
                return EnvelopeType.BusN;
            case "BusO":
                return EnvelopeType.BusO;
            case "BusP":
                return EnvelopeType.BusP;
            case "BusQ":
                return EnvelopeType.BusQ;
            case "BusR":
                return EnvelopeType.BusR;
            case "BusS":
                return EnvelopeType.BusS;
            case "BusT":
                return EnvelopeType.BusT;
            case "BusU":
                return EnvelopeType.BusU;
            case "BusV":
                return EnvelopeType.BusV;
            case "BusW":
                return EnvelopeType.BusW;
            case "BusX":
                return EnvelopeType.BusX;
            case "BusY":
                return EnvelopeType.BusY;
            case "BusZ":
                return EnvelopeType.BusZ;
            case "Composite":
                return EnvelopeType.Composite;
            case "FadeToColor":
                return EnvelopeType.FadeToColor;
            case "Mute":
                return EnvelopeType.Mute;
            case "MotionBlurLength":
                return EnvelopeType.MotionBlurLength;
            case "VideoSupersampling":
                return EnvelopeType.VideoSupersampling;
            case "PanY":
                return EnvelopeType.PanY;
            case "PanSmoothness":
                return EnvelopeType.PanSmoothness;
            case "PanCenter":
                return EnvelopeType.PanCenter;
            case "TransitionProgress":
                return EnvelopeType.TransitionProgress;
            case "Fade":
                return EnvelopeType.Fade;
            case "Velocity":
                return EnvelopeType.Velocity;
            default:
                throw new ApplicationException("Unknown EnvelopeType: " + val);
        }
    }

    CurveType ChildCurveType(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        return ParseCurveType(val);
    }

    CurveType ParseCurveType(String val) {
        switch (val) { 
            case "Sharp": 
                return CurveType.Sharp; 
            case "Slow": 
                return CurveType.Slow; 
            case "None": 
                return CurveType.None; 
            case "Linear": 
                return CurveType.Linear; 
            case "Fast": 
                return CurveType.Fast; 
            case "Smooth":
                return CurveType.Smooth; 
            default:
                throw new ApplicationException("Unknown CurveType: " + val);
        }
    }

    BeatValue ChildBeatValue(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Whole":
                return BeatValue.Whole;
            case "Half":
                return BeatValue.Half;
            case "Quarter":
                return BeatValue.Quarter;
            case "Eighth":
                return BeatValue.Eighth;
            case "Sixteenth":
                return BeatValue.Sixteenth;
            case "ThirtySecond":
                return BeatValue.ThirtySecond;
            default:
                throw new ApplicationException("Unknown BeatValue: " + val);
        }
    }

    VideoFieldOrder ChildFieldOrder(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "ProgressiveScan":
                return VideoFieldOrder.ProgressiveScan;
            case "UpperFieldFirst":
                return VideoFieldOrder.UpperFieldFirst;
            case "LowerFieldFirst":
                return VideoFieldOrder.LowerFieldFirst;
            default:
                throw new ApplicationException("Unknown VideoFieldOrder: " + val);
        }
    }

    VideoRenderQuality ChildVideoRenderQuality(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Draft":
                return VideoRenderQuality.Draft;
            case "Preview":
                return VideoRenderQuality.Preview;
            case "Good":
                return VideoRenderQuality.Good;
            case "Best":
                return VideoRenderQuality.Best;
            default:
                throw new ApplicationException("Unknown VideoRenderQuality: " + val);
        }
    }

    MotionBlurType ChildMotionBlurType(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Gaussian":
                return MotionBlurType.Gaussian;
            case "Pyramid":
                return MotionBlurType.Pyramid;
            case "Box":
                return MotionBlurType.Box;
            case "AsymmetricGaussian":
                return MotionBlurType.AsymmetricGaussian;
            case "AsymmetricPyramid":
                return MotionBlurType.AsymmetricPyramid;
            case "AsymmetricBox":
                return MotionBlurType.AsymmetricBox;
            default:
                throw new ApplicationException("Unknown MotionBlurType: " + val);
        }
    }

    VideoDeinterlaceMethod ChildVideoDeinterlaceMethod(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "None":
                return VideoDeinterlaceMethod.None;
            case "BlendFields":
                return VideoDeinterlaceMethod.BlendFields;
            case "InterpolateFields":
                return VideoDeinterlaceMethod.InterpolateFields;
            default:
                throw new ApplicationException("Unknown VideoDeinterlaceMethod: " + val);
        }
    }

    AudioBusMode ChildAudioBusMode(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Stereo":
                return AudioBusMode.Stereo;
            case "Surround":
                return AudioBusMode.Surround;
            default:
                throw new ApplicationException("Unknown AudioBusMode: " + val);
        }
    }

    AudioResampleQuality ChildAudioResampleQuality(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Preview":
                return AudioResampleQuality.Preview;
            case "Good":
                return AudioResampleQuality.Good;
            case "Best":
                return AudioResampleQuality.Best;
            default:
                throw new ApplicationException("Unknown AudioResampleQuality: " + val);
        }
    }

    VideoResampleMode ChildVideoResampleMode(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Smart":
                return VideoResampleMode.Smart;
            case "Force":
                return VideoResampleMode.Force;
            case "Disable":
                return VideoResampleMode.Disable;
            default:
                throw new ApplicationException("Unknown VideoResampleMode: " + val);
        }
    }

    LowPassFilterQuality ChildLowPassFilterQuality(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Preview":
                return LowPassFilterQuality.Preview;
            case "Good":
                return LowPassFilterQuality.Good;
            case "Best":
                return LowPassFilterQuality.Best;
            default:
                throw new ApplicationException("Unknown LowPassFilterQuality: " + val);
        }
    }

    ChannelRemapping ChildChannelRemapping(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "None":
                return ChannelRemapping.None;
            case "DisableLeft":
                return ChannelRemapping.DisableLeft;
            case "DisableRight":
                return ChannelRemapping.DisableRight;
            case "MuteLeft":
                return ChannelRemapping.MuteLeft;
            case "MuteRight":
                return ChannelRemapping.MuteRight;
            case "Mono":
                return ChannelRemapping.Mono;
            case "Swap":
                return ChannelRemapping.Swap;
            default:
                throw new ApplicationException("Unknown ChannelRemapping: " + val);
        }
    }

    VideoAlphaType ChildVideoAlphaType(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "None":
                return VideoAlphaType.None;
            case "Straight":
                return VideoAlphaType.Straight;
            case "Premultiplied":
                return VideoAlphaType.Premultiplied;
            case "PremultipliedDirty":
                return VideoAlphaType.PremultipliedDirty;
            default:
                throw new ApplicationException("Unknown VideoAlphaType: " + val);
        }
    }

    PanType ChildPanType(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Add":
                return PanType.Add;
            case "ConstantPower":
                return PanType.ConstantPower;
            case "Balance":
                return PanType.Balance;
            case "Notch3Db":
                return PanType.Notch3Db;
            case "Notch6Db":
                return PanType.Notch6Db;
            case "Film":
                return PanType.Film;
            default:
                throw new ApplicationException("Unknown PanType: " + val);
        }
    }

    VideoKeyframeType ChildVideoKeyframeType(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        switch (val) { 
            case "Linear":
                return VideoKeyframeType.Linear;
            case "Hold":
                return VideoKeyframeType.Hold;
            case "Slow":
                return VideoKeyframeType.Slow;
            case "Fast":
                return VideoKeyframeType.Fast;
            case "Smooth":
                return VideoKeyframeType.Smooth;
            case "Sharp":
                return VideoKeyframeType.Sharp;
            default:
                throw new ApplicationException("Unknown VideoKeyframeType: " + val);
        }
    }

    Boolean ChildBoolean(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        return Boolean.Parse(val);
    }

    Double ChildDouble(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        return Double.Parse(val, myNumberFormat);
    }

    Single ChildSingle(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        return Single.Parse(val, myNumberFormat);
    }

    Int32 ChildInt32(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        return Int32.Parse(val, myNumberFormat);
    }

    UInt32 ChildUInt32(XmlElement parent, String childName) {
        String val = ChildString(parent, childName);
        return UInt32.Parse(val, myNumberFormat);
    }

    String ChildString(XmlElement parent, String childName) {
        XmlElement elt = parent[childName];
        if (null == elt)
            throw new ApplicationException("Child element not found: " + childName);
        return elt.InnerText;
    }

    Timecode AttributeTimecode(XmlElement elt, String name) {
        // First try to use the Nanos value.
        String nanosVal = elt.GetAttribute("Nanos");
        if (!String.IsNullOrEmpty(nanosVal)) {
            return Timecode.FromNanos(Int64.Parse(nanosVal, myNumberFormat));
        }
        // Fall back to using the string reprsentation.
        String val = AttributeString(elt, name);
        return Timecode.FromString(val, myTimecodeFormat);
    }

    CurveType AttributeCurveType(XmlElement elt, String name) {
        String val = AttributeString(elt, name);
        return ParseCurveType(val);
    }

    Double AttributeDouble(XmlElement elt, String name) {
        String val = AttributeString(elt, name);
        return Double.Parse(val, myNumberFormat);
    }

    Single AttributeSingle(XmlElement elt, String name) {
        String val = AttributeString(elt, name);
        return Single.Parse(val, myNumberFormat);
    }

    Byte AttributeByte(XmlElement elt, String name) {
        String val = AttributeString(elt, name);
        return Byte.Parse(val, myNumberFormat);
    }

    String AttributeString(XmlElement elt, String name) {
        String val = elt.GetAttribute(name);
        if (String.IsNullOrEmpty(val))
            throw new ApplicationException("Attribute not found: " + name);
        return val;
    }

    Boolean IsGeneratedMediaKey(String key) {
        return key.StartsWith("META:\\Video Generator\\");
    }

    Media FindMedia(MediaPool pool, String mediaKey) {
        Media media = (Media) pool[mediaKey];
        if (null == media) {
            String newKey = (String) myGeneratedMediaKeys[mediaKey];
            if (null != newKey) {
                media = (Media) pool[newKey];
            }
        }
        return media;
    }

    String ShowOpenFileDialog(String filter, String title, String defaultFilename) {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (null == filter) {
            filter = "All Files (*.*)|*.*";
        }
        openFileDialog.Filter = filter;
        if (null != title)
            openFileDialog.Title = title;
        openFileDialog.CheckPathExists = true;
        openFileDialog.AddExtension = true;
        if (null != defaultFilename) {
            String initialDir = Path.GetDirectoryName(defaultFilename);
            if (Directory.Exists(initialDir)) {
                openFileDialog.InitialDirectory = initialDir;
            }
            openFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
            openFileDialog.FileName = Path.GetFileName(defaultFilename);
        }
        if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog()) {
            return Path.GetFullPath(openFileDialog.FileName);
        } else {
            return null;
        }
    }
}
