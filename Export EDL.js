/**
 * Sample script that exports the top-most video and audio track as a
 * CMX 3600 EDL.
 *
 *
 * Limitations:
 * - supports only 1 video and 1 audio track.
 * - supports cuts and cross fades (dissolves) only.
 * - assumes all audio is stereo.
 * - etc.
 *
 * Revision Date: Mar. 22, 2004
 **/
import System;
import System.Text;
import System.IO;
import System.Windows.Forms;
import ScriptPortal.Vegas;

var writer : StreamWriter = null;

try {
    var noTime = new Timecode();

    // find selected tracks
    var videoTrack = FindTopTrackOfType(MediaType.Video);
    var audioTrack = FindTopTrackOfType(MediaType.Audio);
    if ((null == videoTrack) && (null == audioTrack)) {
        throw "no tracks to export";
    }

    var projPath = Vegas.Project.FilePath;
    var title = Path.GetFileNameWithoutExtension(projPath);
    var outputFilename = Path.ChangeExtension(projPath, ".EDL");
    var outputFilename = ShowSaveFileDialog("EDL Files (*.EDL)|*.EDL", "Save EDL", title);
    if (null != outputFilename) {
        writer = new StreamWriter(outputFilename, false, System.Text.Encoding.UTF8, 512);

        // write the tifle and mode
        writer.WriteLine("TITLE:   " + title);
        writer.WriteLine("FCM: " + GetFCM());

        var editCount : int = 1;

        // do video track
        if (null != videoTrack) {
            editCount = ExportTrack(new Enumerator(videoTrack.Events), editCount, null);
        }

        // do audio track
        if (null != audioTrack) {
            editCount = ExportTrack(new Enumerator(audioTrack.Events), editCount, null);
        }

        writer.Close();
    }
} catch (e) {
    if (null != writer)
        writer.Close();
    MessageBox.Show(e);
}

function ExportTrack(events, editIndex, prevMediaPath) {
    if (events.atEnd()) return editIndex;
    var currentEvent = events.item();
    // look ahead to see if we need to do a dissolve.
    var nextEvent = null;
    events.moveNext();
    if (!events.atEnd()) {
        nextEvent = events.item();
    }

    // compute parameters for current event
    var activeTake = currentEvent.ActiveTake;
    var mediaPath = activeTake.MediaPath
    var media = Vegas.Project.MediaPool[mediaPath];
    var tapeName = GetTapeName(media);
    var trackType = GetTrackType(currentEvent);
    var sourceIn = media.TimecodeIn + activeTake.Offset;
    var timelineIn = currentEvent.Start;
    var clipLength = currentEvent.Length;
    if (null != nextEvent) {
        clipLength -= nextEvent.FadeIn.Length;
    }
    if (null != prevMediaPath) {
        var frameCount = currentEvent.FadeIn.Length.FrameCount;
        WriteEdit(editIndex, tapeName, trackType, "D", frameCount, sourceIn, timelineIn, clipLength);
        WriteClipComment("FROM", prevMediaPath);
        WriteClipComment("TO", mediaPath);
    } else {
        WriteEdit(editIndex, tapeName, trackType, "C", 0, sourceIn, timelineIn, clipLength);
        WriteClipComment("FROM", mediaPath);
    }
    // add comments for altered playback rates, etc. (may want to add
    // comments for inverted phase, channel remapping, looping, etc.
    if (currentEvent.PlaybackRate != 1) {
        writer.WriteLine("* Event PlaybackRate = " + currentEvent.PlaybackRate);
    }
    if (currentEvent.IsVideo()) {
        if (currentEvent.Envelopes.HasEnvelope(EnvelopeType.Velocity)) {
            writer.WriteLine("* Event has velocity envelope.");
        }
    }
    if (null != nextEvent) {
        var frameCount = nextEvent.FadeIn.Length.FrameCount;
        if (frameCount > 0) {
            sourceIn += clipLength;
            timelineIn += clipLength;
            WriteEdit(editIndex+1, tapeName, trackType, "C", 0, sourceIn, timelineIn, noTime);
            return ExportTrack(events, editIndex+1, mediaPath);
        }
    }
    return ExportTrack(events, editIndex+1, null);
}

// assumes all audio is stereo
function GetTrackType(evnt) {
    // "V/AAAA"
    if (evnt.IsVideo()) return "V     ";
    return "AA    ";
}

function GetTapeName(media) {
    var tapeName = media.TapeName;
    if ((null == tapeName) || (0 == tapeName.length)) {
        tapeName = "UNKNOWN";
    }
    // strip spaces (really should be any non-alphanumeric characters)
    if (null != tapeName.match(/ /)) {
        tapeName = tapeName.replace(/ /g, "");
    }
    if (tapeName.length > 8) {
        // tape name is too long... chop off the end
        tapeName = tapeName.substring(0, 8);
    }
    return tapeName;
}

function WriteEdit(editIndex, tapeName, trackType, editType, frameCount, sourceIn, timelineIn, clipLength) {
    var edit : StringBuilder = new StringBuilder();
    edit.Append(String.Format("{0:D3}", editIndex));
    edit.Append("  ");
    edit.Append(tapeName);
    edit.Append(" ");
    edit.Append(trackType);
    edit.Append(" ");
    edit.Append(editType);
    edit.Append(" ");
    if (frameCount > 0) {
        edit.Append(String.Format("{0:D3}", frameCount));
        //edit.Append(frameCount);
        edit.Append(" ");
    } else {
        edit.Append("    ");
    }
    edit.Append(sourceIn.ToString());
    edit.Append(" ");
    var sourceOut = sourceIn + clipLength;
    edit.Append(sourceOut.ToString());
    edit.Append(" ");
    edit.Append(timelineIn.ToString());
    edit.Append(" ");
    var timelineOut = timelineIn + clipLength;
    edit.Append(timelineOut.ToString());
    writer.WriteLine(edit.ToString());
}

function WriteClipComment(prefix, mediaPath) {
    var c = new StringBuilder("* ");
    c.Append(prefix);
    c.Append(" CLIP NAME: ");
    c.Append(Path.GetFileNameWithoutExtension(mediaPath));
    writer.WriteLine(c.ToString());
}

function GetFCM() : String {
    var rulerFormat = Vegas.Project.Ruler.Format;
    switch (rulerFormat) {
        case RulerFormat.SmpteDrop:
            return "DROP FRAME";
        case RulerFormat.SmpteFilmSync:
        case RulerFormat.SmpteFilmSyncIVTC:
            return "NON-DROP 24 FRAME";
        default:
            return "NON-DROP FRAME";
    }
}

function FindTopTrackOfType(mediaType) : Track {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        if (track.MediaType == mediaType) {
            return track;
        }
        trackEnum.moveNext();
    }
    return null;
}

// an example filter: "PNG File (*.png)|*.png|JPEG File (*.jpg)|*.jpg"
function ShowSaveFileDialog(filter, title, defaultFilename) {
    var saveFileDialog = new SaveFileDialog();
    if (null == filter) {
        filter = "All Files (*.*)|*.*";
    }
    saveFileDialog.Filter = filter;
    if (null != title)
        saveFileDialog.Title = title;
    saveFileDialog.CheckPathExists = true;
    saveFileDialog.AddExtension = true;
    if (null != defaultFilename) {
        var initialDir = Path.GetDirectoryName(defaultFilename);
        if (Directory.Exists(initialDir)) {
            saveFileDialog.InitialDirectory = initialDir;
        }
        saveFileDialog.DefaultExt = Path.GetExtension(defaultFilename);
        saveFileDialog.FileName = Path.GetFileName(defaultFilename);
    }
    if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
        return Path.GetFullPath(saveFileDialog.FileName);
    } else {
        return null;
    }
}
