/**
 * This script contains some helper functions that can be cut and
 * pasted into other scripts.
 *
 * Revision Date: Apr 20, 2004.
**/

import System.IO;
import System.Windows.Forms;
import Sony.Vegas;

function CreateTrack(mediaType) {
    var track;
    if (mediaType == MediaType.Audio) {
        track = new AudioTrack();
    } else {
        track = new VideoTrack();
    }
    Vegas.Project.Tracks.Add(track);
    return track;
}

function CreateTrackAndEvent(mediaType)
{
    var track, evnt;
    if (mediaType == MediaType.Audio) {
        track = new AudioTrack();
        evnt = new AudioEvent();
    } else {
        track = new VideoTrack();
        evnt = new VideoEvent();
    }
    Vegas.Project.Tracks.Add(track);
    track.Events.Add(evnt);
    return evnt;
}

function CreateGeneratedMedia(generatorName, presetName) {
    var generator = Vegas.Generators.GetChildByName(generatorName);
    var media = new Media(generator, presetName);
    if (!media.IsValid())
        throw "failed to create media: " + generatorName + " (" + presetName + ")";
    return media;
}

// finds the first selected track... note that when multiple tracks
// are selected, this only returns the first.  returns null if no
// tracks are selected.
function FindSelectedTrack() : Track {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        if (track.Selected) {
            return track;
        }
        trackEnum.moveNext();
    }
    return null;
}

// finds the first selected event... note that when multiple events
// are selected, this only returns the first.  returns null if no
// events are selected.
function FindSelectedEvent() : TrackEvent {
    var trackEnum = new Enumerator(Vegas.Project.Tracks);
    while (!trackEnum.atEnd()) {
        var track : Track = Track(trackEnum.item());
        var eventEnum = new Enumerator(track.Events);
        while (!eventEnum.atEnd()) {
            var evnt : TrackEvent = TrackEvent(eventEnum.item());
            if (evnt.Selected) {
                return evnt;
            }
            eventEnum.moveNext();
        }
        trackEnum.moveNext();
    }
    return null;
}

function FindPlugInNode(rootNode : PlugInNode, nameRegExp : RegExp) : PlugInNode {
    if (null != rootNode.Name.match(nameRegExp)) {
        return rootNode;
    } else {
        var children : Enumerator = new Enumerator(rootNode);
        while (!children.atEnd()) {
            var childNode : PlugInNode = PlugInNode(children.item())
            var childMatch : PlugInNode = FindPlugInNode(childNode, nameRegExp);
            if (null != childMatch) {
                return childMatch;
            }
            children.moveNext();
        }
        return null;
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

function CreateTransitionEffect(nameRegExp : RegExp) : Effect {
    var plugIn = FindPlugInNode(Vegas.Transitions, /Dissolve/);
    if (null == plugIn)
        throw "failed to find plug-in";
    return new Effect(plugIn);
}

function FindEnvelopeByType(envelopes : Envelopes, type : EnvelopeType) : Envelope {
    var i : int;
    var count : int = envelopes.Count;
    for (i = 0; i < count; i++) {
        var envelope = envelopes[i];
        if (envelope.Type == type) {
            return envelope;
        }
    }
    return null;
}

function FindEnvelopeByName(envelopes : Envelopes, name : string) : Envelope {
    var i : int;
    var count : int = envelopes.Count;
    for (i = 0; i < count; i++) {
        var envelope = envelopes[i];
        if (envelope.Name == name) {
            return envelope;
        }
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

// an example filter: "PNG File (*.png)|*.png|JPEG File (*.jpg)|*.jpg"
function ShowOpenFileDialog(filter, title, defaultFilename) {
    var openFileDialog = new OpenFileDialog();
    if (null == filter) {
        filter = "All Files (*.*)|*.*";
    }
    openFileDialog.Filter = filter;
    if (null != title)
        openFileDialog.Title = title;
    openFileDialog.CheckPathExists = true;
    openFileDialog.AddExtension = true;
    if (null != defaultFilename) {
        var initialDir = Path.GetDirectoryName(defaultFilename);
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
