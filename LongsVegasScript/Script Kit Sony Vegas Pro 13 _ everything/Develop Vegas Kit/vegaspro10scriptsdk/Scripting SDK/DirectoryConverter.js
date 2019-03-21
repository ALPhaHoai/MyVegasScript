/**
 * This script renders media files found in an input directory using a
 * single render template and saves the results in an output
 * directory.  To change the input directory, output directory, and
 * render template, edit the variables below.
 *
 * Revision Date: Apr 20, 2004.
 **/

import System;
import System.Text;
import System.IO;
import System.Windows.Forms;
import Sony.Vegas;

// The inputDirectory variable specifies where the rendered files will
// be created (do not include the trailing back-slash)
var inputDirectory = "C:\\InputFiles";

// The outputDirectory variable specifies where the rendered files
// will be created (do not include the trailing back-slash)
var outputDirectory = "C:\\OutputFiles";

// The rendererRegexp and templateRegexp variable are regular
// expressions used to match renderer file type names and template
// names.
var rendererRegexp = /Windows Media Video V9/;
var templateRegexp = /256 Kbps Video/;

// The inputFileRegexp is used to filter the input files. Only those
// whose file name matches using this regular expression will be
// converted. The following will match all files the end with avi
// (ignoring case):
var inputFileRegexp = /.avi$/i;

// This version will match all input files.
//var inputFileRegexp = /.*/;

// The overwriteExistingFiles variable determines whether or not it is
// OK to overwrite files that may already exist whose name is the same
// as ones created by running this script.  For safety, the default is
// false.  Set the variable to true to allow overwrites.
var overwriteExistingFiles = false;


try {

    // make sure the output directory exists
    if (!Directory.Exists(inputDirectory))
    {
        var msg = new StringBuilder("The input directory (");
        msg.Append(outputDirectory);
        msg.Append(") does not exist.\n");
        msg.Append("Please edit the script to specify an existing directory.");
        throw msg;
    }

    // make sure the output directory exists
    if (!Directory.Exists(outputDirectory))
    {
        var msg = new StringBuilder("The output directory (");
        msg.Append(outputDirectory);
        msg.Append(") does not exist.\n");
        msg.Append("Please edit the script to specify an existing directory.");
        throw msg;
    }

    var renderer = FindRenderer(rendererRegexp);
    if (null == renderer) {
        throw "Failed to find renderer";
    }

    var renderTemplate = FindRenderTemplate(renderer, templateRegexp);
    if (null == renderTemplate) {
        throw "Failed to find render template";
    }

    var newExtension = renderer.FileExtension.substring(1);

    // create a new project with one video track and one audio track.
    var proj = new Project();

    var videoTrack = new VideoTrack();
    proj.Tracks.Add(videoTrack);

    var audioTrack = new AudioTrack();
    proj.Tracks.Add(audioTrack);

    // save the new project to the output directory 
    //Vegas.SaveProject(Path.Combine(outputDirectory, "temp.veg"));

    // enumerate the files in the input directory
    var fileEnum = new Enumerator(Directory.GetFiles(inputDirectory));
    while (!fileEnum.atEnd()) {
        var inputFile = fileEnum.item();

        // skip files that don't end with the right extension
        if (null == inputFile.match(inputFileRegexp)) {
            fileEnum.moveNext();
            continue;
        }

        // skip files that are not valid media files.
        var media = new Media(inputFile);
        if (!media.IsValid()) {
            fileEnum.moveNext();
            continue;
        }

        var videoStream = media.Streams.GetItemByMediaType(MediaType.Video, 0);
        var audioStream = media.Streams.GetItemByMediaType(MediaType.Audio, 0);

        // if needed, add a video event and associate video stream
        if (null != videoStream) {
            var videoLength = videoStream.Length;
            var videoEvent = new VideoEvent(new Timecode(), videoLength);
            videoTrack.Events.Add(videoEvent);
            var videoTake = new Take(videoStream);
            videoEvent.Takes.Add(videoTake);
        }

        // if needed, add a audio event and associate audio stream
        if (null != audioStream) {
            var audioLength = audioStream.Length;
            var audioEvent = new AudioEvent(new Timecode(), audioLength);
            audioTrack.Events.Add(audioEvent);
            var audioTake = new Take(audioStream);
            audioEvent.Takes.Add(audioTake);
        }

        var outputFileName = Path.GetFileNameWithoutExtension(inputFile) + newExtension;
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        var status = DoRender(outputPath, renderer, renderTemplate);
        if (status == RenderStatus.Canceled) {
            // may want have a dialog here allowing user to
            // continue with remaining files.
            break;
        } else if (status != RenderStatus.Complete) {
            throw "Failed on input file: " + inputFile;
        }

        // clean up the project.
        videoTrack.Events.Clear();
        audioTrack.Events.Clear();
        proj.MediaPool.Remove(inputFile);

        fileEnum.moveNext();
    }

} catch (e) {
    if (!e.skipMessageBox)
        MessageBox.Show(e);
}

// Perform the render.  The Render method returns a member of the
// RenderStatus enumeration which is, in turn, returned by this
// function.
function DoRender(fileName, rndr, rndrTemplate) {
    ValidateFileName(fileName);

    // make sure the file does not already exist
    if (!overwriteExistingFiles && File.Exists(fileName)) {
        throw "File already exists: " + fileName;
    }

    // perform the render.  The Render method returns
    // a member of the RenderStatus enumeration.  If
    // it is anything other than OK, exit the loops.
    //var status = Vegas.Render(fileName, rndrTemplate);
    var status = Vegas.Render(fileName, rndrTemplate);
    return status;
}

function ValidateFileName(fileName : System.String) {
    if (fileName.length > 260)
        throw "file name too long: " + fileName;
    var illegalCharCount = Path.InvalidPathChars.Length;
    var i = 0;
    while (i < illegalCharCount) {
        if (0 <= fileName.IndexOf(Path.InvalidPathChars[i])) {
            throw "invalid file name: " + fileName;
        }
        i++;
    }
}

function FindRenderer(rendererRegExp : RegExp) : Renderer {
    var rendererEnum : Enumerator = new Enumerator(Vegas.Renderers);
    while (!rendererEnum.atEnd()) {
        var renderer : Renderer = Renderer(rendererEnum.item());
        if (null != renderer.FileTypeName.match(rendererRegExp)) {
            return renderer;
        }
        rendererEnum.moveNext();
    }
    return null;
}

function FindRenderTemplate(renderer : Renderer, templateRegExp : RegExp) : RenderTemplate {
    var templateEnum : Enumerator = new Enumerator(renderer.Templates);
    while (!templateEnum.atEnd()) {
        var renderTemplate : RenderTemplate = RenderTemplate(templateEnum.item());
        if (null != renderTemplate.Name.match(templateRegExp)) {
            return renderTemplate;
        }
        templateEnum.moveNext();
    }
    return null;
}
