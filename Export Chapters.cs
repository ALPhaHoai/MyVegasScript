using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Globalization;
using ScriptPortal.Vegas;
using System.Xml;

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

        String exportFile = ShowSaveFileDialog("XML Chapters List (*.xml)|*.xml|" +
                                          "CSV Chapters List (*.csv)|*.csv|" +
                                          "TXT Chapter List (*.txt)|*.txt",
                                          "Export Chapter Information", projName + "_chapters");
	
        if (null != exportFile) {
            String ext = Path.GetExtension(exportFile);
            if ((null != ext) && (ext.ToUpper() == ".XML"))
            {
                ExportchaptersToXML(exportFile);
            }
            else if ((null != ext) && (ext.ToUpper() == ".TXT"))
            {
                ExportchaptersToTXT(exportFile);
            }
            else
            {   // should be CVS
                ExportchaptersToCSV(exportFile);
            }
        }
    }
        


    StreamWriter CreateStreamWriter(String fileName, Encoding encoding) {
        FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        StreamWriter sw = new StreamWriter(fs, encoding);
        return sw;
    }

    void ExportchaptersToCSV(String exportFile)
    {
        StreamWriter streamWriter = null;
        try {
            streamWriter = CreateStreamWriter(exportFile, System.Text.Encoding.Unicode);

            int tot_markers = myVegas.Project.Markers.Count;
            int counter = 1;
            foreach (Marker marker in myVegas.Project.Markers) {
                StringBuilder tsv = new StringBuilder();
                tsv.Append(marker.Position.ToString());
                if(counter !=tot_markers)
                tsv.Append(',');
                counter++;
                streamWriter.Write(tsv);
            }
        } finally {
            if (null != streamWriter)
            {
                streamWriter.Close();
                System.Windows.Forms.MessageBox.Show("Export successful. File name: " + exportFile, "Chapter File Export");
            }
        }        
    }

    void ExportchaptersToTXT(String exportFile) {
        StreamWriter streamWriter = null;
        try {
            streamWriter = CreateStreamWriter(exportFile, System.Text.Encoding.Unicode);
            //streamWriter.WriteLine("Timecode");
            foreach (Marker marker in myVegas.Project.Markers) {
                StringBuilder tsv = new StringBuilder();
                tsv.Append(marker.Position.ToString());
                tsv.Append('\t');
                streamWriter.WriteLine(tsv.ToString());
            }
        } finally {
            if (null != streamWriter)
            {
                streamWriter.Close();
                System.Windows.Forms.MessageBox.Show("Export successful. File name: " + exportFile, "Chapter File Export");
            }
        }        
    }


    void ExportchaptersToXML(String exportFile)
    {
        XmlDocument doc = null;

        try
        {
            doc = new XmlDocument();
            XmlProcessingInstruction xmlPI = doc.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\"");
            doc.AppendChild(xmlPI);
            XmlElement root = doc.CreateElement("Chapters");
            System.Text.Encoding myCharacterEncoding = System.Text.Encoding.UTF8;
            doc.AppendChild(root);
            XmlElement chapter;
            foreach (Marker marker in myVegas.Project.Markers)
            {
                chapter = doc.CreateElement("chapter");
                chapter.SetAttribute("ID", marker.Index.ToString());
                chapter.SetAttribute("Time-code", marker.Position.ToString());
                root.AppendChild(chapter);
            }
            XmlTextWriter writer = new XmlTextWriter(exportFile, myCharacterEncoding);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.IndentChar = ' ';
            doc.WriteTo(writer);
            writer.Close();
        }
        catch {
            doc = null;
        }
        finally
        {
            if (null != doc)
            {
                System.Windows.Forms.MessageBox.Show("Export successful. File name: " + exportFile, "Chapter File Export");
            }
        } 
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
        if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog()) {
            return Path.GetFullPath(saveFileDialog.FileName);
        } else {
            return null;
        }
    }
}
