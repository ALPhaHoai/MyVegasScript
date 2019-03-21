/*

  This script will export project 608CC1 and 608CC3 command markers to 
 * two .SRT files which can be used Closed Caption with YouTube video.

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

        String sExportFile = ShowSaveFileDialog("SubRip (*.srt)|*.srt", "Save Closed Caption as Subtitles", sProjName);

        if (null != sExportFile)
        {
            StreamWriter streamWriter = null;
            String sExt = Path.GetExtension(sExportFile);
            if ( ((null != sExt) && (sExt.ToUpper() != ".SRT")) || null == sExt )
            {
                sExportFile = Path.Combine(Path.GetDirectoryName(sExportFile),Path.GetFileNameWithoutExtension(sExportFile) + ".srt");
            }

            try
            {
                FileStream filestream = null;
               
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

                    int n = 0;
                    int iSubtitle = 1;
                    
                    for (n = 0; n < cc1List.Count; n++)
                    {
                        if (0 != (cc1List[n].CommandParameter.ToString()).Length)
                        {
                            StringBuilder sOut = new StringBuilder();
                            sOut.Append(iSubtitle);
                            sOut.Append("\r\n");
                            sOut.Append(TimecodeToSRTString(cc1List[n].Position, false));
                            sOut.Append(" --> ");

                            if(cc1List.Count - 1 != n)
                            {
                                sOut.Append(TimecodeToSRTString(cc1List[n + 1].Position, false));
                            }
                            else
                            {
                                sOut.Append(TimecodeToSRTString(cc1List[n].Position, true));
                            }

                            sOut.Append("\r\n");
                            string sText = cc1List[n].CommandParameter;
                            sText = sText.Replace("[br]", "\r\n");
                            sOut.Append(sText);
                            streamWriter.WriteLine(sOut.ToString());
                            streamWriter.WriteLine();
                            iSubtitle++;
                        }
                    }       
                }

                if (bCC3HasText)
                {
                    if (bCC1HasText)
                    {
                        if (null != streamWriter)
                        {
                            streamWriter.Close();
                        }
                        sExportFile = Path.Combine(Path.GetDirectoryName(sExportFile), Path.GetFileNameWithoutExtension(sExportFile) + "3.srt");
                    }

                    filestream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(filestream, System.Text.Encoding.UTF8);

                    int n = 0;
                    int iSubtitle = 1;
                    for (n = 0; n < cc3List.Count; n++)
                    {
                        if (0 != (cc3List[n].CommandParameter.ToString()).Length)
                        {
                            StringBuilder sOut = new StringBuilder();
                            sOut.Append(iSubtitle);
                            sOut.Append("\r\n");
                            sOut.Append(TimecodeToSRTString(cc3List[n].Position, false));
                            sOut.Append(" --> ");
                            if (cc3List.Count - 1 != n)
                            {
                                sOut.Append(TimecodeToSRTString(cc3List[n + 1].Position, false));
                            }
                            else
                            {
                                sOut.Append(TimecodeToSRTString(cc3List[n].Position, true));
                            }

                            sOut.Append("\r\n");
                            string sText = cc3List[n].CommandParameter;
                            sText = sText.Replace("[br]", "\r\n");
                            sOut.Append(sText);
                            streamWriter.WriteLine(sOut.ToString());
                            streamWriter.WriteLine();
                            iSubtitle++;
                        }
                    }
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

    String TimecodeToSRTString(Timecode timecode, bool bLast)
    {
        Int64 time = Convert.ToInt64(timecode.ToMilliseconds());
        Int64 hours = time / 3600000;
        Int64 mins = (time - hours * 3600000)/60000;
        Int64 secs = (time - hours * 3600000 - mins * 60000) / 1000;
        Int64 ssecs = time - hours * 3600000 - mins * 60000 - secs * 1000;
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

        return String.Format("{0:00}:{1:00}:{2:00},{3:000}", hours, mins, secs, ssecs);
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
