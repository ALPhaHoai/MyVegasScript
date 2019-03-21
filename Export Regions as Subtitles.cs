/**
 * You can use this script to export Vegas regions in the .sub format
 * for use in DVDA as subtitles.  This script can aslo export regions
 * as tab separated text.
 *
 * To use this script:
 *
 * 1) Create named Vegas regions.
 * 2) Confirm no overlapped regions.
 * 3) Vegas>tools>scripting>run script>ExportRegionsAsSubtitles.js; save
 * 4) Import into DVDA using the Import Subtitles button in the DVDA timeline.
 *
 * Revision Date: Juse 22, 2006.
 **/
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Globalization;
using ScriptPortal.Vegas;

public class EntryPoint
{
    Vegas myVegas;

    public void FromVegas(Vegas vegas) {
        myVegas = vegas;
 
        String projName;

        String projFile = myVegas.Project.FilePath;
        if (String.IsNullOrEmpty(projFile)) {
            projName = "Untitled";
        } else  {
            projName = Path.GetFileNameWithoutExtension(projFile);
        }

        String exportFile = ShowSaveFileDialog("DVD Architect Subtitle Script (*.sub)|*.sub|" + 
                                                  "Vegas Region List (*.txt)|*.txt", 
                                                  "Save Regions as Subtitles", projName + "-Regions");
	
        if (null != exportFile) {
            String ext = Path.GetExtension(exportFile);
            // Works even if prev lastIndexOf fails or if the ext
            // contains but not equal to "sub"
            if ((null != ext) && (ext.ToUpper() == ".SUB"))
                ExportRegionsToSUB(exportFile);
            else
                ExportRegionsToTXT(exportFile);
        }
    }
        
        
    String TimeToString(Timecode time) {
        String[] decimalSeparators = new String[] {CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator};
        Int64 nanosPerCentisecond = 100000;

        // first round the time to the nearest centisecond
        Int64 nanos = time.Nanos;
        Double tmp = ((Double) nanos / (Double) nanosPerCentisecond) + 0.5;
        nanos = (Int64) tmp * nanosPerCentisecond;
        time = Timecode.FromNanos(nanos);

        // {"hh:mm:ss", "ddd"}
        String[] rgTime = time.ToString(RulerFormat.Time).Split(decimalSeparators, StringSplitOptions.None);
        StringBuilder sbRes = new StringBuilder();
	
        sbRes.Append(rgTime[0]);
        sbRes.Append(':');
	
        int iCentiseconds = (int) Math.Round(Double.Parse(rgTime[1]) / 10.0);
	
        sbRes.Append(((iCentiseconds / 10) >> 0 ) % 10);
        sbRes.Append(((iCentiseconds /  1) >> 0 ) % 10);
	
        return sbRes.ToString();
    }

    StreamWriter CreateStreamWriter(String fileName, Encoding encoding) {
        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        StreamWriter sw = new StreamWriter(fs, encoding);
        return sw;
    }

    void ExportRegionsToSUB(String exportFile) {
        StreamWriter streamWriter = null;
        try {
            streamWriter = CreateStreamWriter(exportFile, System.Text.Encoding.Unicode);
            //streamWriter.WriteLine("Start\tEnd\tLength\tName");
            int iSubtitle = 0;
            foreach (Region region in myVegas.Project.Regions) {
                StringBuilder tsv = new StringBuilder();
                tsv.Append((( iSubtitle / 1000 )>> 0) % 10 );
                tsv.Append((( iSubtitle /  100 )>> 0) % 10 );
                tsv.Append((( iSubtitle /   10 )>> 0) % 10 );
                tsv.Append((( iSubtitle /    1 )>> 0) % 10 );
                tsv.Append('\t');
                tsv.Append( TimeToString( region.Position ));
                tsv.Append('\t');
                tsv.Append( TimeToString( region.End ));
                tsv.Append('\t');
                tsv.Append(region.Label);
                streamWriter.WriteLine(tsv.ToString());
                streamWriter.WriteLine();
                iSubtitle++;
            }
        } finally {
            if (null != streamWriter)
                streamWriter.Close();
        }        
    }
 
    void ExportRegionsToTXT(String exportFile) {
        StreamWriter streamWriter = null;
        try {
            streamWriter = CreateStreamWriter(exportFile, System.Text.Encoding.Unicode);
            streamWriter.WriteLine("Start\tEnd\tLength\tName");
            foreach (Region region in myVegas.Project.Regions) {
                StringBuilder tsv = new StringBuilder();
                tsv.Append( region.Position.ToString( RulerFormat.Time ));
                tsv.Append('\t');
                tsv.Append( region.End.ToString( RulerFormat.Time  ));
                tsv.Append('\t');
                tsv.Append( region.Length.ToString( RulerFormat.Time  ));
                tsv.Append('\t');
                tsv.Append(region.Label);
                streamWriter.WriteLine(tsv.ToString());
            }
        } finally {
            if (null != streamWriter)
                streamWriter.Close();
        }        
    }
 
    // an example filter: "PNG File (*.png)|*.png|JPEG File (*.jpg)|*.jpg"
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
        if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
            return Path.GetFullPath(saveFileDialog.FileName);
        } else {
            return null;
        }
    }
}
