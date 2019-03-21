//****************************************************************************
//*      Program: BkupSave.cs
//*       Author: Jerry am Ende
//*  Description: This script saves your project and creates an increntalBackupFile
//*      Created: Dec 30, 2011
//*      Updated: 
//****************************************************************************
using System;
using System.Collections;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO; 
using ScriptPortal.Vegas;

class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
        try
        {
            if (String.IsNullOrEmpty(vegas.Project.FilePath))
            {
                MessageBox.Show("Unsaved Project\r\nManually Save your Project\r\nBefore Running this Script");
            }
            else
            {
                // Find Current File Name
                string fullProjectPath = vegas.Project.FilePath.ToString();

                // Split our the Name without the .veg Extension 
                string[] fileParts = Regex.Split(fullProjectPath, ".veg");

                // Add a DateTime Qualify to make the Backup Name unique
                string bkupSaveName = string.Format(fileParts[0] + "-{0:yyyy-MM-dd_hh-mm-ss-tt}.veg", DateTime.Now);

                // Save the Current Project
                vegas.SaveProject(fullProjectPath);

                // rename the Vegas Project File on disk to the temp file name
                // thanks to "Gary James" for this upgrade 
                File.Move(fullProjectPath, bkupSaveName);

                // Save the Project one more time
                vegas.SaveProject(fullProjectPath);
            }
        }

        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
    }
}
