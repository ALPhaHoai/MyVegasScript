// This script renders a still image sequence from the current Vegas
// project. It first presents a dialog that allows you to specify the
// output directory, base file name, file format (PNG or JPEG), start
// time, stop time, and step time.  Then it enters a loop that exports
// a sequence of image files.  See below for more details.

import System;
import System.IO;
import System.Windows.Forms;
import SonicFoundry.Vegas.Script;


// The first thing this script does is save off the preview & project
// settings that will be set later.  This is so they can be restored
// after the script is complete.
var origPreviewRenderQuality = Vegas.Project.Preview.RenderQuality;
var origPreviewFillSize = Vegas.Project.Preview.FullSize;
var origFieldOrder = Vegas.Project.Video.FieldOrder;
var origProjectDeinterlaceMethod = Vegas.Project.Video.DeinterlaceMethod;
var origCursor = Vegas.Cursor;
var origSelectionStart = Vegas.SelectionStart;
var origSelectionLength = Vegas.SelectionLength;

// Set the overwriteExistingFiles variable to true if you want to
// allow the script to overwrite existing files.
var overwriteExistingFiles = false;

var renderStatus = 0;

try {

    // Default image file name. This includes the output directory,
    // base name, and extension.  The final output file names will
    // have 6 digit numbers inserted before the extension.  This
    // variable can be modified using the dialog.
    var imageFileName = "c:\\Image_.png";

    // Make a Time object of zero for comparisons.
    var zeroTime = new Timecode(0);

    // Set the default start, stop, and step times. If there is a
    // selection, use it for the start and stop times, otherwise do
    // the whole project.
    var startTime, stopTime, stepTime;
    if (Vegas.SelectionLength > zeroTime) {
        startTime = Vegas.SelectionStart;
        stopTime = startTime + Vegas.SelectionLength;
    } else {
        startTime = new Timecode(0);
        stopTime = Vegas.Project.Length;
    }

    // The stepTime is the number of frames to skip forward between
    // each output file.
    stepTime = new Timecode("00:00:00;01");

    // Show the script's dialog box.
    var dialog = new RenderImageSequenceDialog(imageFileName, startTime, stopTime, stepTime);
    var dialogResult = dialog.ShowDialog();

    // if the OK button was pressed...
    if (System.Windows.Forms.DialogResult.OK == dialogResult) {

        // Set the preview quality and size.
        Vegas.Project.Preview.RenderQuality = VideoRenderQuality.Best;
        Vegas.Project.Preview.FullSize = true;

        // Set the field order and deinterlace method
        Vegas.Project.Video.FieldOrder = VideoFieldOrder.ProgressiveScan;
        Vegas.Project.Video.DeinterlaceMethod = VideoDeinterlaceMethod.InterpolateFields;

        // Get the basis for output image file names
        imageFileName = Path.GetFullPath(dialog.fileNameBox.Text);
        var baseImageFileName = Path.GetDirectoryName(imageFileName);
        baseImageFileName += Path.DirectorySeparatorChar;
        baseImageFileName += Path.GetFileNameWithoutExtension(imageFileName);

        // Get the output image file name extension and corresponding
        // file format. ImageFileFormat.None indicates that the
        // current prefs setting will be used but that may not
        // correspond to the specified file extension.
        var imageFileNameExt = Path.GetExtension(imageFileName);
        var imageFormat = ImageFileFormat.None;
        if (0 == String.Compare(imageFileNameExt, ".jpg", true)) {
            imageFormat = ImageFileFormat.JPEG;
        } else if (0 == String.Compare(imageFileNameExt, ".png", true)) {
            imageFormat = ImageFileFormat.PNG;
        }

        // Get the start, stop, and step times specified in the
        // dialog.
        startTime = new Timecode(dialog.startTimeBox.Text);
        stopTime = new Timecode(dialog.stopTimeBox.Text);
        stepTime = new Timecode(dialog.stepTimeBox.Text);

        // Make sure the step time is legal.
        if (stepTime <= zeroTime) {
            throw "step time must be greater than zero";
        }

        // compute the length of time and total number of frames that
        // will be rendered.
        var deltaTime = stopTime - startTime;
        if (zeroTime > deltaTime) {
            throw "start time must be greater than or equal to stop time";
        }
        var deltaFrames = deltaTime.FrameCount;
        var stepFrames = stepTime.FrameCount;
        if (0 == stepFrames) {
            throw "insufficient step time";
        }
        var renderCount = Math.floor(double(deltaFrames) / double(stepFrames));
        
        // compute the number of digits required to uniquely name each
        // file and the corresponding format string.
        var renderCountDigits = renderCount.toString().length;
        var imageIndexFormatString = "{0:D" + renderCountDigits + "}";
        
        // Enter the render loop.
        var filename, renderStatus;
        var imageNdex = 0;
        var currentTime = startTime;
        while (currentTime <= stopTime) {

            // move the cursor to the current time and redraw the user
            // interface.
            Vegas.Cursor = currentTime;
            Vegas.UpdateUI();

            // compose the file name
            filename = baseImageFileName;
            filename += String.Format(imageIndexFormatString, imageNdex);
            filename += imageFileNameExt;

            // check if the file already exists and, if so, throw an
            // error if the script is not allowed to overwrite it.
            if (!overwriteExistingFiles && File.Exists(filename)) {
                throw "file already exists: " + filename;
            }

            // save a snapshot.  The SaveSnapshot method returns a
            // member of the RenderStatus enumeration. If the user
            // hits the escape key or quits the app, exit the loop.
            renderStatus = Vegas.SaveSnapshot(filename, imageFormat, currentTime);
            if (RenderStatus.Complete != renderStatus) {
                break;
            }

            // increment the current time and the image index.
            currentTime += stepTime;
            imageNdex += 1;
        }
    }
} catch (e) {
    MessageBox.Show(e);
}

// restore the project and preview settings
Vegas.Project.Preview.RenderQuality = origPreviewRenderQuality;
Vegas.Project.Preview.FullSize = origPreviewFillSize;
Vegas.Project.Video.FieldOrder = origFieldOrder;
Vegas.Project.Video.DeinterlaceMethod = origProjectDeinterlaceMethod;
Vegas.Cursor = origCursor;
Vegas.SelectionStart = origSelectionStart;
Vegas.SelectionLength = origSelectionLength;




// Button subclass that shows a save file dialog when clicked
class BrowseButton extends Button {
    var myResultBox = null;

    function BrowseButton(resultBox) {
        myResultBox = resultBox;
    }

    protected override function OnClick(e : EventArgs) {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "PNG File (*.png)|*.png|JPEG File (*.jpg)|*.jpg";
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.AddExtension = true;
        if (null != myResultBox) {
            var filename = myResultBox.Text;
            var initialDir = Path.GetDirectoryName(filename);
            if (Directory.Exists(initialDir)) {
                saveFileDialog.InitialDirectory = initialDir;
            }
            saveFileDialog.DefaultExt = Path.GetExtension(filename);
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filename);
        }
        if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
            if (null != myResultBox) {
                myResultBox.Text = Path.GetFullPath(saveFileDialog.FileName);
            }
        }
    }
}

// Form subclass that is the dialog box for this script
class RenderImageSequenceDialog extends Form {
    var browseButton;
    var fileNameBox;
    var startTimeBox;
    var stopTimeBox;
    var stepTimeBox;

    function RenderImageSequenceDialog(baseFileName, startTime, stopTime, stepTime) {
        this.Text = "Render Image Sequence";
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Width = 480;

        var buttonWidth = 80;
        var buttonHeight = 24;

        fileNameBox = addTextControl("Base File Name", 6, 380, 10, baseFileName);
        browseButton = new BrowseButton(fileNameBox);
        browseButton.Left = fileNameBox.Right + 4;
        browseButton.Top = fileNameBox.Top - 2;
        browseButton.Width = buttonWidth;
        browseButton.Height = buttonHeight;
        browseButton.Text = "Browse...";
        Controls.Add(browseButton);

        var timeBoxTop = fileNameBox.Bottom + 16;
        startTimeBox = addTextControl("Start Time", 10, 140, timeBoxTop, startTime.ToString());
        stopTimeBox = addTextControl("Stop Time", 160, 140, timeBoxTop, stopTime.ToString());
        stepTimeBox = addTextControl("Step Time", 320, 140, timeBoxTop, stepTime.ToString());

        var buttonTop = stepTimeBox.Bottom + 16;
        var okButton = new Button();
        okButton.Text = "OK";
        okButton.Left = this.Width - (2*(buttonWidth+10));
        okButton.Top = buttonTop;
        okButton.Width = buttonWidth;
        okButton.Height = buttonHeight;
        okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        AcceptButton = okButton;
        Controls.Add(okButton);

        var cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Left = this.Width - (1*(buttonWidth+10));
        cancelButton.Top = buttonTop;
        cancelButton.Width = buttonWidth;
        cancelButton.Height = buttonHeight;
        cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        CancelButton = cancelButton;
        Controls.Add(cancelButton);

        this.Height = okButton.Bottom + 30;

    }

    function addTextControl(labelName, left, width, top, defaultValue) {
        var label = new Label();
        label.AutoSize = true;
        label.Text = labelName + ":";
        label.Left = left;
        label.Top = top + 4;
        Controls.Add(label);

        var textbox = new TextBox();
        textbox.Multiline = false;
        textbox.Left = label.Right;
        textbox.Top = top;
        textbox.Width = width - (label.Width);
        textbox.Text = defaultValue;
        Controls.Add(textbox);

        return textbox;
    }

}
