/**
 * This script will create a CD burning project from a music play list
 * file in ASX format.  You can save play lists in Windows Media
 * Player in ASX format.  This script can be easily modified to
 * support any play list format that based on XML.
 *
 *
 * Note: The file named PlaylistToProject.xml must be in the same
 * directory as this script.  It is this script's configuration file
 * and informs the Vegas script engine to load the XML .NET
 * assembly... typically only the core and GUI assemblies are loaded.
 **/
import System.Windows.Forms;
import System.IO;
import System.Xml;
import SonicFoundry.Vegas.Script;


// These variables specify the amount of time to put before the first
// CD track and between each track.  These Timecode constructors
// specify the number of milliseconds.
var firstTrackPadding = new Timecode(2000);
var interTrackPadding = new Timecode(2000);

// Change the following variable to true if you want each audio file
// to be normalized.
var normalizeEvents = false;

// Prompt the user to select a play list file.  If the user cancels,
// the return value of ShowOpenFileDialog will be null.
var asxPath = ShowOpenFileDialog("ASX Playlist (*.asx)|*.asx", null);
if (null != asxPath) {
    PlaylistToProject();
}

function PlaylistToProject() {
    try {
        if ((null == asxPath) || (0 >= asxPath.length))
            throw "playlist not specified";

        var xml = new System.Xml.XmlDocument();
        if (null == xml)
            throw "failed to create XML document.";
        xml.Load(asxPath);
        var doc = xml.DocumentElement;
        if (null == doc)
            throw "failed to load play list file: (" + asxPath + ")";

        // create a new project
        var wasCreated = Vegas.NewProject(true, false);
        if (!wasCreated) {
            return;
        }
        var proj = Vegas.Project;

        // first switch the ruler format to audio cd time (this is
        // broken at the moment)
        proj.Ruler.Format = RulerFormat.AudioCDTime;

        // create a new audio track
        var track = new Track(MediaType.Audio);
        proj.Tracks.Add(track);

        // enumerate the entries in the playlist
        var trackCount = DoEntries(track, new Enumerator(doc.SelectNodes("Entry")), firstTrackPadding);
        if (0 == trackCount)
            throw "no entries found";

    } catch (e) {
        MessageBox.Show("Error: " + e);
    }
}


// iterate over each entry in the play list
function DoEntries(track, entryEnum, currentTime) {
    if (entryEnum.atEnd())
        return 0;
    currentTime += DoEntry(track, entryEnum.item(), currentTime);
    currentTime += interTrackPadding;
    entryEnum.moveNext();
    return 1 + DoEntries(track, entryEnum, currentTime);
}

// this function is called for each entry in the play list
function DoEntry(track, entry, currentTime) {

    // find the path of the media file
    var pathElt = entry.SelectSingleNode("Ref");
    if (null == pathElt)
        return new Timecode(0);
    var path = pathElt.GetAttribute("href");
    if (null == path)
        return new Timecode(0);

    // create a new media object
    var media = new Media(path);
    if (!media.IsValid())
        return new Timecode(0);

    // get the length of the stream
    var stream = media.Streams[0];
    var length = stream.Length;

    // create the new event & add it to the track
    var evnt = new Event(currentTime, length);
    track.Events.Add(evnt);

    // create a new take using the media stream
    var takeName = null;
    var nameElt = entry.SelectSingleNode("Param[@Name = \"Name\"]");
    if (null != nameElt)
        takeName = nameElt.GetAttribute("Value");
    evnt.Takes.Add(new Take(media, 0, true, takeName));

    // normalize the event if needed... broken when the peaks are not
    // built first
    evnt.Normalize = normalizeEvents;

    // create a CD region
    Vegas.Project.Regions.Add(new Region(currentTime, length, takeName));

    return length;
}


// an example filter: "PNG File (*.png)|*.png|JPEG File (*.jpg)|*.jpg"
function ShowOpenFileDialog(filter, defaultFilename) {
    var openFileDialog = new OpenFileDialog();
    if (null == filter) {
        filter = "All Files (*.*)|*.*";
    }
    openFileDialog.Filter = filter;
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
