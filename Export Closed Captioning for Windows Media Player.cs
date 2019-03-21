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

        String sExportFile = ShowSaveFileDialog("Synchronized Accessible Media Interchange (SAMI) (*.smi)|*.smi", "Save Closed Caption as SAMI", sProjName);

        if (null != sExportFile)
        {
            String filename = Path.GetFileNameWithoutExtension(sExportFile);
            String directory = Path.GetDirectoryName(sExportFile);
            String sExt = Path.GetExtension(sExportFile);
            if ( ((null != sExt) && (sExt.ToUpper() != ".SMI")) || null == sExt )
            {
                sExportFile = Path.Combine(directory, filename + ".smi");
            }

            FileStream filestream = null;
            StreamWriter streamWriter = null;

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
                    streamWriter = new StreamWriter(filestream, System.Text.Encoding.ASCII);

                    WriterSAMIHeader(ref streamWriter);

                    for (int n = 0; n < cc1List.Count; n++)
                    {
                        Int64 nCurPos = Convert.ToInt64(cc1List[n].Position.ToMilliseconds());
                        StringBuilder sOut = new StringBuilder();
                        if (0 != (cc1List[n].CommandParameter.ToString()).Length)
                        {
                            sOut.AppendFormat("<SYNC Start=\"{0}\">\r\n", nCurPos);
                            sOut.Append("<P Class=ENCC ID=MyFont>");
                            string sText = cc1List[n].CommandParameter;
                            sText = sText.Replace("-[br]", "");
                            sText = sText.Replace("[br]", " ");
                            sOut.AppendFormat("{0}</P>\r\n", sText);
                            streamWriter.Write(sOut.ToString());
                            streamWriter.WriteLine("</SYNC>\r\n");
                            if ((cc1List.Count - 1) == n)
                            {
                                sOut = new StringBuilder();
                                sOut.AppendFormat("<SYNC Start=\"{0}\">\r\n", (nCurPos + 2000));
                                sOut.Append("<P Class=ENCC>&nbsp</P>\r\n");
                                sOut.Append("</SYNC>\r\n");
                                streamWriter.Write(sOut.ToString());
                            }
                        }
                        else
                        {
                            sOut.AppendFormat("<SYNC Start=\"{0}\">\r\n", nCurPos);
                            sOut.Append("<P Class=ENCC>&nbsp</P>\r\n");
                            sOut.Append("</SYNC>\r\n");
                            streamWriter.WriteLine(sOut.ToString());
                        }
                    }

                    WriterSAMITailer(ref streamWriter);

                    //create .asx
                    {
                        sExportFile = Path.Combine(directory, filename + "EditAndRunMe.asx");
                        FileStream asxstream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                        StreamWriter asxStreamWriter = new StreamWriter(asxstream, System.Text.Encoding.ASCII);
                        asxStreamWriter.WriteLine("<ASX Version=\"3.0\">");
                        asxStreamWriter.WriteLine("<AUTHOR>Author:</AUTHOR>");
                        asxStreamWriter.WriteLine("<ABSTRACT>Abstract:</ABSTRACT>");
                        asxStreamWriter.WriteLine("<TITLE>Title:</TITLE>");
                        asxStreamWriter.WriteLine("<COPYRIGHT>(c) 2018 MAGIX Software GmbH. All rights reserved.</COPYRIGHT>");
                        asxStreamWriter.WriteLine("<ENTRY>");
                        StringBuilder sBuf = new StringBuilder();
                        sBuf.AppendFormat("<Ref HREF=\"{0}.wmv?SAMI={1}.smi\"/>", filename, filename);
                        asxStreamWriter.WriteLine(sBuf.ToString());
                        asxStreamWriter.WriteLine("</ENTRY>");
                        asxStreamWriter.WriteLine("</ASX>");
                        asxStreamWriter.Close();
                    }
                }

                if (bCC3HasText)
                {
                    if (bCC1HasText)
                    {
                         sExportFile = Path.Combine(directory, filename + "3.smi");
                    }

                    filestream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(filestream, System.Text.Encoding.ASCII);

                    WriterSAMIHeader(ref streamWriter);

                    for (int n = 0; n < cc3List.Count; n++)
                    {
                        Int64 nCurPos = Convert.ToInt64(cc3List[n].Position.ToMilliseconds());
                        StringBuilder sOut = new StringBuilder();
                        if (0 != (cc3List[n].CommandParameter.ToString()).Length)
                        {
                            sOut.AppendFormat("<SYNC Start=\"{0}\">\r\n", nCurPos);
                            sOut.Append("<P Class=ENCC ID=MyFont>");
                            string sText = cc3List[n].CommandParameter;
                            sText = sText.Replace("-[br]", "");
                            sText = sText.Replace("[br]", " ");
                            sOut.AppendFormat("{0}</P>\r\n", sText);
                            streamWriter.Write(sOut.ToString());
                            streamWriter.WriteLine("</SYNC>\r\n");

                            if ((cc3List.Count - 1) == n)
                            {
                                sOut = new StringBuilder();
                                sOut.AppendFormat("<SYNC Start=\"{0}\">\r\n", (nCurPos + 2000));
                                sOut.Append("<P Class=ENCC>&nbsp</P>\r\n");
                                sOut.Append("</SYNC>\r\n");
                                streamWriter.Write(sOut.ToString());
                            }
                        }
                        else
                        {
                            sOut.AppendFormat("<SYNC Start=\"{0}\">\r\n", nCurPos);
                            sOut.Append("<P Class=ENCC>&nbsp</P>\r\n");
                            sOut.Append("</SYNC>\r\n");
                            streamWriter.WriteLine(sOut.ToString());
                        }
                    }

                    WriterSAMITailer(ref streamWriter);
                    //create .asx
                    {
                        sExportFile = Path.Combine(directory, filename + "EditAndRunMe3.asx");
                        FileStream asxstream = new FileStream(sExportFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                        StreamWriter asxStreamWriter = new StreamWriter(asxstream, System.Text.Encoding.ASCII);
                        asxStreamWriter.WriteLine("<ASX Version=\"3.0\">");
                        asxStreamWriter.WriteLine("<AUTHOR>Author:</AUTHOR>");
                        asxStreamWriter.WriteLine("<ABSTRACT>Abstract:</ABSTRACT>");
                        asxStreamWriter.WriteLine("<TITLE>Title:</TITLE>");
                        asxStreamWriter.WriteLine("<COPYRIGHT>(c) 2018 MAGIX Software GmbH. All Rights Reserved.</COPYRIGHT>");
                        asxStreamWriter.WriteLine("<ENTRY>");
                        StringBuilder sBuf = new StringBuilder();
                        sBuf.AppendFormat("<Ref HREF=\"{0}.wmv?SAMI={1}.smi\"/>", filename, filename+"3");
                        asxStreamWriter.WriteLine(sBuf.ToString());
                        asxStreamWriter.WriteLine("</ENTRY>");
                        asxStreamWriter.WriteLine("</ASX>");
                        asxStreamWriter.Close();
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

    void WriterSAMIHeader(ref StreamWriter streamWriter)
    {
        streamWriter.WriteLine("<SAMI>");
        streamWriter.WriteLine("<HEAD>");
        streamWriter.WriteLine("<SAMIParam>");
        streamWriter.WriteLine("  Metrics {time:ms;}");
        streamWriter.WriteLine("  Spec {MSFT:1.0;}");
        streamWriter.WriteLine("</SAMIParam>");
        streamWriter.WriteLine("<STYLE TYPE=\"text/css\">");
        streamWriter.WriteLine("<!--");
        streamWriter.WriteLine(@"P {");
        streamWriter.WriteLine(@"   font-size: 12 pt;");
        streamWriter.WriteLine(@"   font-family: Arial;");
        streamWriter.WriteLine(@"   font-weight: Regular;");
        streamWriter.WriteLine(@"   color: #fffff0;");
        streamWriter.WriteLine(@"   background: #000000;");
        streamWriter.WriteLine(@"   text-align: Center;");
        streamWriter.WriteLine(@"   padding-left: 5px;");
        streamWriter.WriteLine(@"   padding-right: 5px;");
        streamWriter.WriteLine(@"   padding-bottom: 2px;");
        streamWriter.WriteLine(@"  }");
        streamWriter.WriteLine("#MyFont {Name: MyFont; color: yellow; margin-left: 30pt; margin-right: 30pt;}");
        streamWriter.WriteLine(".ENCC {Name: English; lang: en-US-CC; color: white; margin-left: 30pt; margin-right: 30pt;}");
        streamWriter.WriteLine(" -->");
        streamWriter.WriteLine("</STYLE>");
        streamWriter.WriteLine("</HEAD>");
        streamWriter.WriteLine("<BODY>");
        streamWriter.WriteLine();
    }

    void WriterSAMITailer(ref StreamWriter streamWriter)
    {
        streamWriter.WriteLine();
        streamWriter.WriteLine("</BODY>");
        streamWriter.WriteLine("</SAMI>");
        streamWriter.Close();
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
