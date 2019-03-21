// sample script that batch renders the current project.
import System.IO;
import System.Windows.Forms;
import SonicFoundry.Vegas.Script;

// The outputDirectory variable specifies where the rendered files
// will be created (do not include the trailing back-slash)
var outputDirectory = "D:\\renders";

// The baseFileName variable is the prefix of each output file name.
// For each output file, the name of the renderer template and the
// renderer's file format extension is added to form the complete file
// name.
var baseFileName = "BatchRender_";

// The overwriteExistingFiles variable determines whether or not it is
// OK to overwrite files that may already exist whose name is the same
// as ones created by running this script.  For safety, the default is
// false.  Set the variable to true to allow overwrites.
var overwriteExistingFiles = false;

// The rendererRegexp and templateRegexp variable are regular
// expressions used to match renderer file type names and template
// names.  For example, to match every "Video For Windows" (avi)
// renderer template whose name ends with "DV", use:

var rendererRegexp = /Video for Windows/;
var templateRegexp = /DV$/;

// The effect will (typically) be to render DV encoded AVI files in
// both NTSC and and PAL formats.  Another approach might be to define
// some custom templates whose names have something in common.  For
// example, say you've defined some templates for the MP3 file format
// named "Custom Low", "Custom Medium", and "Custom High". The
// following configuration would select every MP3 renderer template
// that begins with "Custom":

//var rendererRegexp = /MP3/;
//var templateRegexp = /^Custom/;

// Another example might be to use multiple renderers as well as
// multiple templates. You can use a regular expression that matches
// multiple names by separating match strings by a "|" character. The
// following configuration will render MP3 and Ogg Vorbis audio files
// at 96 and 128 Kbps:

//var rendererRegexp = /MP3|OggVorbis/;
//var templateRegexp = /96 Kbps|128 Kbps/;

// There are lots of other ways to configure the regular expressions
// to match almost any combination of renderers and templates.  You
// can find more details on using regular expressions by searching the
// Internet or the .NET Framework SDK documentation for "JScript
// Regular Expressions"

// If you set the renderEachRegion to true, each region will be
// rendered as a separate file. If you leave it set to false and there
// is a selection, just that will be rendered.  Otherwise the entire
// project will be rendered.
var renderEachRegion = false;



// Of course if that's not enough (and this is one of the best things
// about using scripts) you can always modify the code below to make
// it work any way you want.


try {

    // make sure the output directory exists
    if (!Directory.Exists(outputDirectory))
    {
        var msg;
        msg = "The output directory (" + outputDirectory + ") does not exist.\n";
        msg += "Please create it or edit the script to specify another directory";
        throw msg;
    }


    var zeroTime = new Timecode(0);
    var renderStart, renderLength;

    var renderers, renderer, templates, renderTemplate;
    var templateFound = false;
    var renderStatus = RenderStatus.Complete;
    
    // enumerate through each renderer
    renderers = new Enumerator(Vegas.Renderers);
    while (!renderers.atEnd() && (renderStatus == RenderStatus.Complete)) {

        renderer = renderers.item();
        // try to match the renderer
        if (null != renderer.FileTypeName.match(rendererRegexp)) {

            // enumerate through each template
            templates = new Enumerator(renderer.Templates);
            while (!templates.atEnd() && (renderStatus == RenderStatus.Complete)) {
                renderTemplate = templates.item();

                // try to match the template
                if (null != renderTemplate.Name.match(templateRegexp)) {
                    templateFound = true;

                    // construct the file name (most of it)
                    var filename = outputDirectory;
                    filename += Path.DirectorySeparatorChar;
                    filename += baseFileName;
                    filename += renderTemplate.Name;

                    if (renderEachRegion) {
                        var regions = new Enumerator(Vegas.Project.Regions);
                        var regionIndex = 0;
                        while (!regions.atEnd() && (renderStatus == RenderStatus.Complete)) {
                            var region = regions.item();
                            var regionFilename = filename + "[" + regionIndex + "]";
                            // need to strip off the extension's leading "*"
                            regionFilename += renderer.FileExtension.substring(1);

                            // Render the region
                            renderStart = region.Start;
                            renderLength = region.Length
                            renderStatus = DoRender(regionFilename, renderer, renderTemplate, renderStart, renderLength);
                            regionIndex++;
                            regions.moveNext();
                        }
                    } else {
                        // need to strip off the extension's leading "*"
                        filename += renderer.FileExtension.substring(1); 

                        // Render only the selected portion of the
                        // time line. But if there is no selection,
                        // render the entire project.
                        if (Vegas.SelectionLength > zeroTime) {
                            renderStart = Vegas.SelectionStart;
                            renderLength = Vegas.SelectionLength;
                        } else {
                            renderStart = new Timecode(0);
                            renderLength = Vegas.Project.Length;
                        }
                        renderStatus = DoRender(filename, renderer, renderTemplate, renderStart, renderLength);
                    }
                }
                templates.moveNext();
            }
        }
        renderers.moveNext();
    }

    // inform the user of some special failure cases
    if (!templateFound)
        throw "no renderer templates match";
    if (renderStatus == RenderStatus.Failed)
        throw "render failed";

} catch (e) {
    MessageBox.Show(e);
}


// perform the render.  The Render method returns a member of the
// RenderStatus enumeration.  If it is anything other than OK, exit
// the loops.
function DoRender(fileName, rndr, rndrTemplate, start, length) {
    // make sure the file does not already exist
    if (!overwriteExistingFiles && File.Exists(fileName)) {
        throw "File already exists: " + fileName;
    }

    // perform the render.  The Render method returns
    // a member of the RenderStatus enumeration.  If
    // it is anything other than OK, exit the loops.
    return Vegas.Render(fileName, rndrTemplate, start, length);
}
