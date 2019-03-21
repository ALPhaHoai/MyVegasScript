/*

  This script will export project 608CC1 and 608CC3 command markers to 
 * two .TXT files which can be used Closed Caption with QuickTime Player.

  Last Modified: April 2010.

*/

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Globalization;
using ScriptPortal.Vegas;

public class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
        Project proj = vegas.Project;

        String sProjName;

        String sProjFile = vegas.Project.FilePath;
        if (String.IsNullOrEmpty(sProjFile))
        {
            sProjName = "Untitled";
        }
        else
        {
            sProjName = Path.GetFileNameWithoutExtension(sProjFile);
        }

        String sExportFile = ShowSaveFileDialog("QuickTime (*.txt)|*.txt", "Save Closed Caption as QuickTime Caption", sProjName);

        if (null != sExportFile)
        {
            StreamWriter streamWriter = null;
            FileStream filestream = null;
            String directory = Path.GetDirectoryName(sExportFile);
            String filename = Path.GetFileNameWithoutExtension(sExportFile);
            String sExt = Path.GetExtension(sExportFile);
            if ( ((null != sExt) && (sExt.ToUpper() != ".txt")) || null == sExt )
            {
                sExportFile = Path.Combine(directory, filename + ".txt");
            }

            try
            {               
                List<CommandMarker> cc1List = new List<CommandMarker>();
                List<CommandMarker> cc3List = new List<CommandMarker>();
                bool bCC1HasText = false;
                bool bCC3HasText = false;
                foreach (CommandMarker commandmarker in proj.CommandMarkers)
                {
                    if (MarkerCommandType.CEA608CC1.ToString() == commandmarker.CommandType.ToString())
                    {
                        cc1List.Add(commandmarker);
                        if (!bCC1HasText && 0 != (commandmarker.CommandParameter.ToString()).Length)
                        {
                            bCC1HasText = true;
                        }
                    }

                    if (MarkerCommandType.CEA608CC3.ToString() == commandmarker.CommandType.ToString())
                    {
                        cc3List.Add(commandmarker);
                        if (!bCC3HasText && 0 != (commandmarker.CommandParameter.ToString()).Length)
                        {
                            bCC3HasText = true;
                        }
                    }
                }

                if( bCC1HasText )
                {
                    filestream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(filestream, System.Text.Encoding.UTF8);

                    WriteQCaptionHeader(ref streamWriter);

                    for (int n = 0; n < cc1List.Count; n++)
                    {
                        if (0 != (cc1List[n].CommandParameter.ToString()).Length)
                        {
                            StringBuilder sOut = new StringBuilder();
                            sOut.Append(TimecodeToTimeString(cc1List[n].Position, false));
                            sOut.Append(' ');
                            string sText = cc1List[n].CommandParameter;
                            sText = sText.Replace("-[br]", "");
                            sText = sText.Replace("[br]", " ");
                            sOut.Append(sText);
                            sOut.Append(' ');
                            if (cc1List.Count - 1 != n && 0 == (cc1List[n+1].CommandParameter.ToString()).Length)
                            {
                                sOut.Append(TimecodeToTimeString(cc1List[n+1].Position, false));
                            }
                            else if(cc1List.Count - 1 == n)
                            {
                                sOut.Append(TimecodeToTimeString(cc1List[n].Position, true));
                            }

                            streamWriter.WriteLine(sOut.ToString());
                        }
                    }
                    streamWriter.Close();
                    streamWriter = null;
                    CreateSMILFile(directory, filename + "EditAndRunMe", filename, filename);
                }

                if (bCC3HasText)
                {
                    if (bCC1HasText)
                    {
                        sExportFile = Path.Combine(directory, filename + "3.txt");
                    }

                    filestream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(filestream, System.Text.Encoding.UTF8);

                    WriteQCaptionHeader(ref streamWriter);

                    for (int n = 0; n < cc3List.Count; n++)
                    {
                        if (0 != (cc3List[n].CommandParameter.ToString()).Length)
                        {
                            StringBuilder sOut = new StringBuilder();
                            sOut.Append(TimecodeToTimeString(cc3List[n].Position, false));
                            sOut.Append(' ');
                            string sText = cc3List[n].CommandParameter;
                            sText = sText.Replace("-[br]", "");
                            sText = sText.Replace("[br]", " ");
                            sOut.Append(sText);
                            sOut.Append(' ');
                            if( cc3List.Count - 1 != n && 0 == cc3List[n+1].CommandParameter.ToString().Length )
                            {
                                sOut.Append(TimecodeToTimeString(cc3List[n+1].Position, false));
                            }
                            else if( cc3List.Count - 1 == n )
                            {
                                sOut.Append(TimecodeToTimeString(cc3List[n].Position, true));
                            }

                            streamWriter.WriteLine(sOut.ToString());
                        }
                    }
                    streamWriter.Close();
                    streamWriter = null;
                    CreateSMILFile(directory, filename + "EditAndRunMe3", filename, filename+"3");
                }
            }
            finally
            {
                if (null != streamWriter)
                {
                    streamWriter.Close();
                }
            }                  
        }
    }

    void WriteQCaptionHeader(ref StreamWriter streamWriter)
    {
        StringBuilder sBuffer = new StringBuilder();
        sBuffer.Append("{QTtext}");
        sBuffer.Append("{font: Arial}");
        sBuffer.Append("{textColor: 65535, 65535, 0}");
        sBuffer.Append("{justify: center}");
        sBuffer.Append("{size: 20}");
        sBuffer.Append("{backcolor:0, 0, 0}");
        sBuffer.Append("{timescale: 100}");
        sBuffer.Append("{width: 640}");
        sBuffer.Append("{height: 80}\r\n");
        sBuffer.AppendLine();
        sBuffer.Append("[00:00:00.00]");
        sBuffer.AppendLine();
        streamWriter.WriteLine(sBuffer.ToString());
    }

    void CreateSMILFile(String sDirectory, String sExportFileName, String sMediaName, String sCaptionName)
    {
        String sExportFile = Path.Combine(sDirectory, sExportFileName + ".smil");
        FileStream filestream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        StreamWriter streamWriter = new StreamWriter(filestream, System.Text.Encoding.ASCII);
        streamWriter.WriteLine("<smil xmlns:qt=\"http://www.apple.com/quicktime/resources/smilextensions\" qt:autoplay=\"true\" qt:time-slider=\"true\">");
        streamWriter.WriteLine("<head>");
        streamWriter.WriteLine("<meta name=\"title\" content=\"QuickTime Closed Caption Test\"/>");
        streamWriter.WriteLine("<layout>");
        streamWriter.WriteLine("<root-layout background-color=\"white\" width=\"640\" height=\"480\"/>");
        streamWriter.WriteLine("<region id=\"videoregion\" top=\"1%\" left=\"1%\" width=\"98%\" height=\"90%\"/>");
        streamWriter.WriteLine("<region id=\"textregion\" top=\"91%\" left=\"1%\" width=\"98%\" height=\"10%\"/>");
        streamWriter.WriteLine("</layout>");
        streamWriter.WriteLine("</head>");
        streamWriter.WriteLine("<body>");
        streamWriter.WriteLine("<par>");
        streamWriter.WriteLine("<!-- VIDEO -->");
        StringBuilder sVideo = new StringBuilder();
        sVideo.AppendFormat("<video src=\"{0}.mov\" region=\"videoregion\"/>", sMediaName);
        streamWriter.WriteLine(sVideo.ToString());
        streamWriter.WriteLine("<!-- CAPTIONS -->");
        StringBuilder sCaption = new StringBuilder();
        sCaption.AppendFormat("<textstream src=\"{0}.txt\" region=\"textregion\" system-language=\"en\" system-captions=\"on\" title=\"english captions\" alt=\"english captions\"/>", sCaptionName);
        streamWriter.WriteLine(sCaption.ToString());
        streamWriter.WriteLine("</par>");
        streamWriter.WriteLine("</body>");
        streamWriter.WriteLine("</smil>");
        streamWriter.Close();
    }

    String TimecodeToTimeString(Timecode timecode, bool bLast)
    {
        Int64 time = Convert.ToInt64(timecode.ToMilliseconds());
        Int64 hours = time / 3600000;
        Int64 mins = (time - hours * 3600000)/60000;
        Int64 secs = (time - hours * 3600000 - mins * 60000) / 1000;
        Int64 ssecs = time - hours * 3600000 - mins * 60000 - secs * 1000 + 5;
        if (999 < ssecs)
        {
            ssecs -= 1000;
            secs += 1;
            if (59 < secs)
            {
                secs -= 60;
                mins += 1;
                if (59 < mins)
                {
                    mins -= 60;
                    hours += 1;
                }
            }
        }

        if (bLast)
        {
            secs += 2;
            if (secs > 59)
            {
                mins += 1;
                secs -= 60;
            }

            if (mins > 59)
            {
                hours += 1;
                mins -= 60;
            }
        }

        return String.Format("[{0:00}:{1:00}:{2:00}.{3:00}]", hours, mins, secs, ssecs/10);
    }

    String ShowSaveFileDialog(String sFilter, String sTitle, String sDefaultFileName) 
    {
        SaveFileDialog fileDialog = new SaveFileDialog();

        if(null == sFilter) 
        {
            sFilter = "All Files (*.*)|*.*";
        }

        fileDialog.Filter = sFilter;

        if(null != sTitle)
        {
            fileDialog.Title = sTitle;
        }
        fileDialog.CheckPathExists = true;
        fileDialog.AddExtension = true;
        
        if(null != sDefaultFileName) 
        {
            String sDir = Path.GetDirectoryName(sDefaultFileName);
            if( Directory.Exists(sDir) ) 
            {
                fileDialog.InitialDirectory = sDir;
            }
            fileDialog.DefaultExt = Path.GetExtension(sDefaultFileName);
            fileDialog.FileName = Path.GetFileName(sDefaultFileName);
        }
        
        if ( System.Windows.Forms.DialogResult.OK == fileDialog.ShowDialog() ) 
        {
            return Path.GetFullPath(fileDialog.FileName);
        } 
        else 
        {
            return null;
        }
    }  
}
