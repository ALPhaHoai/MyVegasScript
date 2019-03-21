using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using ScriptPortal.Vegas;

public class EntryPoint
{

    // set this to true if you want to allow files to be overwritten
    bool OverwriteExistingFiles = false;

    String defaultBasePath = "VegasPro_";

    Sony.Vegas.Vegas myVegas = null;

    enum RenderMode
    {
        Project = 0,
        Selection,
        Regions,
    }

    string IsAnAdministrator()
    {
        WindowsIdentity identity =
            WindowsIdentity.GetCurrent();
        if (identity != null)
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator) ? "True" : "False";
        }
        return "Error";
    }

    ArrayList SelectedTemplates = new ArrayList();

    public void FromVegas(Vegas vegas)
    {
        if (IsAnAdministrator() == "False")
        {
            MessageBox.Show("You dont have the rights. If your using Vista/7 then run Sony Vegas with Admin Rights.");
            return;
        }

        if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PreShutdownSave.veg"))
            MessageBox.Show(
                "Your not using this for the first time, please backup the PreShutdownSave.veg\nwhich is in My Documents before continuing. Or if you are sure you saved your earlier project in another file then continue.");

        myVegas = vegas;
        
        String projectPath = myVegas.Project.FilePath;
        if (String.IsNullOrEmpty(projectPath))
        {
            String dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            defaultBasePath = Path.Combine(dir, defaultBasePath);
        }
        else
        {
            String dir = Path.GetDirectoryName(projectPath);
            String fileName = Path.GetFileNameWithoutExtension(projectPath);
            defaultBasePath = Path.Combine(dir, fileName + "_");
        }

        DialogResult result = ShowBatchRenderDialog();
        myVegas.UpdateUI();
        if (DialogResult.OK == result)
        {
            // inform the user of some special failure cases
            String outputFilePath = FileNameBox.Text;
            RenderMode renderMode = RenderMode.Project;
            if (RenderRegionsButton.Checked)
            {
                renderMode = RenderMode.Regions;
            }
            else if (RenderSelectionButton.Checked)
            {
                renderMode = RenderMode.Selection;
            }
            DoBatchRender(SelectedTemplates, outputFilePath, renderMode);
        }
	else return;

        if (ShutDownState.Checked)
        {
            DialogResult sd = ShowShutDownDialog();
            if (sd == DialogResult.Cancel)
            {
                _tim.Stop();
                _tim.Enabled = false;
            }
        }

    }

    void DoBatchRender(ArrayList selectedTemplates, String basePath, RenderMode renderMode)
    {
        String outputDirectory = Path.GetDirectoryName(basePath);
        String baseFileName = Path.GetFileName(basePath);

        // make sure templates are selected
        if ((null == selectedTemplates) || (0 == selectedTemplates.Count))
            throw new ApplicationException("No render templates selected.");

        // make sure the output directory exists
        if (!Directory.Exists(outputDirectory))
            throw new ApplicationException("The output directory does not exist.");

        RenderStatus status = RenderStatus.Canceled;

        // enumerate through each selected render template
        foreach (RenderItem renderItem in selectedTemplates)
        {
            // construct the file name (most of it)
            String filename = Path.Combine(outputDirectory,
                                           FixFileName(baseFileName) +
                                           FixFileName(renderItem.Renderer.FileTypeName) +
                                           "_" +
                                           FixFileName(renderItem.Template.Name));

            if (RenderMode.Regions == renderMode)
            {
                int regionIndex = 0;
                foreach (Sony.Vegas.Region region in myVegas.Project.Regions)
                {
                    String regionFilename = String.Format("{0}[{1}]{2}",
                                                          filename,
                                                          regionIndex.ToString(),
                                                          renderItem.Extension);
                    // Render the region
                    status = DoRender(regionFilename, renderItem, region.Position, region.Length);
                    if (RenderStatus.Canceled == status) break;
                    regionIndex++;
                }
            }
            else
            {
                filename += renderItem.Extension;
                Timecode renderStart, renderLength;
                if (renderMode == RenderMode.Selection)
                {
                    renderStart = myVegas.SelectionStart;
                    renderLength = myVegas.SelectionLength;
                }
                else
                {
                    renderStart = new Timecode();
                    renderLength = myVegas.Project.Length;
                }
                status = DoRender(filename, renderItem, renderStart, renderLength);
            }
            if (RenderStatus.Canceled == status) break;
        }
    }

    // perform the render.  The Render method returns a member of the
    // RenderStatus enumeration.  If it is anything other than OK,
    // exit the loops.  This will throw an error message string if the
    // render does not complete successfully.
    RenderStatus DoRender(String filePath, RenderItem renderItem, Timecode start, Timecode length)
    {
        ValidateFilePath(filePath);

        // make sure the file does not already exist
        if (!OverwriteExistingFiles && File.Exists(filePath))
        {
            throw new ApplicationException("File already exists: " + filePath);
        }

        // perform the render.  The Render method returns
        // a member of the RenderStatus enumeration.  If
        // it is anything other than OK, exit the loops.
        RenderStatus status = myVegas.Render(filePath, renderItem.Template, start, length);

        switch (status)
        {
            case RenderStatus.Complete:
            case RenderStatus.Canceled:
                break;
            case RenderStatus.Failed:
            default:
                StringBuilder msg = new StringBuilder("Render failed:\n");
                msg.Append("\n    file name: ");
                msg.Append(filePath);
                msg.Append("\n    Renderer: ");
                msg.Append(renderItem.Renderer.FileTypeName);
                msg.Append("\n    Template: ");
                msg.Append(renderItem.Template.Name);
                msg.Append("\n    Start Time: ");
                msg.Append(start.ToString());
                msg.Append("\n    Length: ");
                msg.Append(length.ToString());
                throw new ApplicationException(msg.ToString());
        }
        return status;
    }

    String FixFileName(String name)
    {
        const Char replacementChar = '-';
        foreach (char badChar in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(badChar, replacementChar);
        }
        return name;
    }

    void ValidateFilePath(String filePath)
    {
        if (filePath.Length > 260)
            throw new ApplicationException("File name too long: " + filePath);
        foreach (char badChar in Path.GetInvalidPathChars())
        {
            if (0 <= filePath.IndexOf(badChar))
            {
                throw new ApplicationException("Invalid file name: " + filePath);
            }
        }
    }

    class RenderItem
    {
        public readonly Renderer Renderer = null;
        public readonly RenderTemplate Template = null;
        public readonly String Extension = null;

        public RenderItem(Renderer r, RenderTemplate t, String e)
        {
            Renderer = r;
            Template = t;
            // need to strip off the extension's leading "*"
            if (null != e) Extension = e.TrimStart('*');
        }
    }

    Button BrowseButton;
    TextBox FileNameBox;
    TreeView TemplateTree;
    RadioButton RenderProjectButton;
    RadioButton RenderRegionsButton;
    RadioButton RenderSelectionButton;
    CheckBox ShutDownState;

    DialogResult ShowBatchRenderDialog()
    {
        Form dlog = new Form();
        dlog.Text = "Batch Render";
        dlog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        dlog.MaximizeBox = false;
        dlog.StartPosition = FormStartPosition.CenterScreen;
        dlog.Width = 600;
        dlog.FormClosing += HandleFormClosing;

        int titleBarHeight = dlog.Height - dlog.ClientSize.Height;
        int buttonWidth = 80;

        FileNameBox = AddTextControl(dlog, "Base File Name", titleBarHeight + 6, 460, 10, defaultBasePath);

        BrowseButton = new Button();
        BrowseButton.Left = FileNameBox.Right + 4;
        BrowseButton.Top = FileNameBox.Top - 2;
        BrowseButton.Width = buttonWidth;
        BrowseButton.Height = BrowseButton.Font.Height + 12;
        BrowseButton.Text = "Browse...";
        BrowseButton.Click += new EventHandler(HandleBrowseClick);
        dlog.Controls.Add(BrowseButton);

        TemplateTree = new TreeView();
        TemplateTree.Left = 10;
        TemplateTree.Width = dlog.Width - 20;
        TemplateTree.Top = BrowseButton.Bottom + 10;
        TemplateTree.Height = 300;
        TemplateTree.CheckBoxes = true;
        TemplateTree.AfterCheck += new TreeViewEventHandler(HandleTreeViewCheck);
        dlog.Controls.Add(TemplateTree);

        int buttonTop = TemplateTree.Bottom + 16;
        int buttonsLeft = dlog.Width - (2 * (buttonWidth + 10));

        RenderProjectButton = AddRadioControl(dlog,
                                              "Render Project",
                                              6,
                                              buttonTop,
                                              true);
        RenderSelectionButton = AddRadioControl(dlog,
                                                "Render Selection",
                                                RenderProjectButton.Right,
                                                buttonTop,
                                                (0 != myVegas.SelectionLength.Nanos));
        RenderRegionsButton = AddRadioControl(dlog,
                                              "Render Regions",
                                              RenderSelectionButton.Right,
                                              buttonTop,
                                              (0 != myVegas.Project.Regions.Count));
        RenderProjectButton.Checked = true;

        Button okButton = new Button();
        okButton.Text = "OK";
        okButton.Left = dlog.Width - (2 * (buttonWidth + 10));
        okButton.Top = buttonTop;
        okButton.Width = buttonWidth;
        okButton.Height = okButton.Font.Height + 12;
        okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        dlog.AcceptButton = okButton;
        dlog.Controls.Add(okButton);

        Button cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Left = dlog.Width - (1 * (buttonWidth + 10));
        cancelButton.Top = buttonTop;
        cancelButton.Height = cancelButton.Font.Height + 12;
        cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        dlog.CancelButton = cancelButton;
        dlog.Controls.Add(cancelButton);

        ShutDownState = new CheckBox();
        ShutDownState.Text = "Shutdown when all rendering completes";
        ShutDownState.Left = 10;
        ShutDownState.Top = okButton.Bottom - 5;
        ShutDownState.Width = 300;
        dlog.Controls.Add(ShutDownState);

        dlog.Height = titleBarHeight + okButton.Bottom + 18;
        dlog.ShowInTaskbar = false;

        FillTemplateTree();

        return dlog.ShowDialog(myVegas.MainWindow);
    }

    TextBox AddTextControl(Form dlog, String labelName, int left, int width, int top, String defaultValue)
    {
        Label label = new Label();
        label.AutoSize = true;
        label.Text = labelName + ":";
        label.Left = left;
        label.Top = top + 4;
        dlog.Controls.Add(label);

        TextBox textbox = new TextBox();
        textbox.Multiline = false;
        textbox.Left = label.Right;
        textbox.Top = top;
        textbox.Width = width - (label.Width);
        textbox.Text = defaultValue;
        dlog.Controls.Add(textbox);

        return textbox;
    }

    RadioButton AddRadioControl(Form dlog, String labelName, int left, int top, bool enabled)
    {
        Label label = new Label();
        label.AutoSize = true;
        label.Text = labelName;
        label.Left = left;
        label.Top = top + 4;
        label.Enabled = enabled;
        dlog.Controls.Add(label);

        RadioButton radiobutton = new RadioButton();
        radiobutton.Left = label.Right;
        radiobutton.Width = 36;
        radiobutton.Top = top;
        radiobutton.Enabled = enabled;
        dlog.Controls.Add(radiobutton);

        return radiobutton;
    }

    void FillTemplateTree()
    {
        int projectAudioChannelCount = 0;
        if (AudioBusMode.Stereo == myVegas.Project.Audio.MasterBusMode)
        {
            projectAudioChannelCount = 2;
        }
        else if (AudioBusMode.Surround == myVegas.Project.Audio.MasterBusMode)
        {
            projectAudioChannelCount = 6;
        }
        bool projectHasVideo = ProjectHasVideo();
        bool projectHasAudio = ProjectHasAudio();
        foreach (Renderer renderer in myVegas.Renderers)
        {
            try
            {
                String rendererName = renderer.FileTypeName;
                TreeNode rendererNode = new TreeNode(rendererName);
                rendererNode.Tag = new RenderItem(renderer, null, null);
                foreach (RenderTemplate template in renderer.Templates)
                {
                    try
                    {
                        // filter out invalid templates
                        if (!template.IsValid())
                        {
                            continue;
                        }
                        // filter out video templates when project has
                        // no video.
                        if (!projectHasVideo && (0 < template.VideoStreamCount))
                        {
                            continue;
                        }
                        // filter out audio-only templates when project has no audio
                        if (!projectHasAudio && (0 == template.VideoStreamCount) && (0 < template.AudioStreamCount))
                        {
                            continue;
                        }
                        // filter out templates that have more channels than the project
                        if (projectAudioChannelCount < template.AudioChannelCount)
                        {
                            continue;
                        }
                        // filter out templates that don't have
                        // exactly one file extension
                        String[] extensions = template.FileExtensions;
                        if (1 != extensions.Length)
                        {
                            continue;
                        }
                        String templateName = template.Name;
                        TreeNode templateNode = new TreeNode(templateName);
                        templateNode.Tag = new RenderItem(renderer, template, extensions[0]);
                        rendererNode.Nodes.Add(templateNode);
                    }
                    catch (Exception e)
                    {
                        // skip it
                        MessageBox.Show(e.ToString());
                    }
                }
                if (0 == rendererNode.Nodes.Count)
                {
                    continue;
                }
                else if (1 == rendererNode.Nodes.Count)
                {
                    // skip it if the only template is the project
                    // settings template.
                    if (0 == ((RenderItem)rendererNode.Nodes[0].Tag).Template.Index)
                    {
                        continue;
                    }
                }
                else
                {
                    TemplateTree.Nodes.Add(rendererNode);
                }
            }
            catch
            {
                // skip it
            }
        }
    }

    bool ProjectHasVideo()
    {
        foreach (Track track in myVegas.Project.Tracks)
        {
            if (track.IsVideo())
            {
                return true;
            }
        }
        return false;
    }

    bool ProjectHasAudio()
    {
        foreach (Track track in myVegas.Project.Tracks)
        {
            if (track.IsAudio())
            {
                return true;
            }
        }
        return false;
    }

    void UpdateSelectedTemplates()
    {
        SelectedTemplates.Clear();
        foreach (TreeNode node in TemplateTree.Nodes)
        {
            foreach (TreeNode templateNode in node.Nodes)
            {
                if (templateNode.Checked)
                {
                    SelectedTemplates.Add(templateNode.Tag);
                }
            }
        }
    }

    void HandleBrowseClick(Object sender, EventArgs args)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "All Files (*.*)|*.*";
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.AddExtension = false;
        if (null != FileNameBox)
        {
            String filename = FileNameBox.Text;
            String initialDir = Path.GetDirectoryName(filename);
            if (Directory.Exists(initialDir))
            {
                saveFileDialog.InitialDirectory = initialDir;
            }
            saveFileDialog.DefaultExt = Path.GetExtension(filename);
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filename);
        }
        if (System.Windows.Forms.DialogResult.OK == saveFileDialog.ShowDialog())
        {
            if (null != FileNameBox)
            {
                FileNameBox.Text = Path.GetFullPath(saveFileDialog.FileName);
            }
        }
    }

    void HandleTreeViewCheck(object sender, TreeViewEventArgs args)
    {
        if (args.Node.Checked)
        {
            if (0 != args.Node.Nodes.Count)
            {
                if ((args.Action == TreeViewAction.ByKeyboard) || (args.Action == TreeViewAction.ByMouse))
                {
                    SetChildrenChecked(args.Node, true);
                }
            }
            else if (!args.Node.Parent.Checked)
            {
                args.Node.Parent.Checked = true;
            }
        }
        else
        {
            if (0 != args.Node.Nodes.Count)
            {
                if ((args.Action == TreeViewAction.ByKeyboard) || (args.Action == TreeViewAction.ByMouse))
                {
                    SetChildrenChecked(args.Node, false);
                }
            }
            else if (args.Node.Parent.Checked)
            {
                if (!AnyChildrenChecked(args.Node.Parent))
                {
                    args.Node.Parent.Checked = false;
                }
            }
        }
    }

    void HandleFormClosing(Object sender, FormClosingEventArgs args)
    {
        Form dlg = sender as Form;
        if (null == dlg) return;
        if (DialogResult.OK != dlg.DialogResult) return;
        String outputFilePath = FileNameBox.Text;
        try
        {
            String outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(outputDirectory)) throw new ApplicationException();
        }
        catch
        {
            String title = "Invalid Directory";
            StringBuilder msg = new StringBuilder();
            msg.Append("The output directory does not exist.\n");
            msg.Append("Please specify the directory and base file name using the Browse button.");
            MessageBox.Show(dlg, msg.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            args.Cancel = true;
            return;
        }
        try
        {
            String baseFileName = Path.GetFileName(outputFilePath);
            if (String.IsNullOrEmpty(baseFileName)) throw new ApplicationException();
            if (-1 != baseFileName.IndexOfAny(Path.GetInvalidFileNameChars())) throw new ApplicationException();
        }
        catch
        {
            String title = "Invalid Base File Name";
            StringBuilder msg = new StringBuilder();
            msg.Append("The base file name is not a valid file name.\n");
            msg.Append("Make sure it contains one or more valid file name characters.");
            MessageBox.Show(dlg, msg.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            args.Cancel = true;
            return;
        }
        UpdateSelectedTemplates();
        if (0 == SelectedTemplates.Count)
        {
            String title = "No Templates Selected";
            StringBuilder msg = new StringBuilder();
            msg.Append("No render templates selected.\n");
            msg.Append("Select one or more render templates from the available formats.");
            MessageBox.Show(dlg, msg.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            args.Cancel = true;
            return;
        }
    }

    void SetChildrenChecked(TreeNode node, bool checkIt)
    {
        foreach (TreeNode childNode in node.Nodes)
        {
            if (childNode.Checked != checkIt)
                childNode.Checked = checkIt;
        }
    }

    bool AnyChildrenChecked(TreeNode node)
    {
        foreach (TreeNode childNode in node.Nodes)
        {
            if (childNode.Checked) return true;
        }
        return false;
    }

    Button button1;
    readonly Timer _tim = new Timer();
    int iTime;
    Form dlog1;

    DialogResult ShowShutDownDialog()
    {
        dlog1 = new Form();
        dlog1.FormBorderStyle = FormBorderStyle.None;
        dlog1.MaximizeBox = false;
        dlog1.StartPosition = FormStartPosition.CenterScreen;
        dlog1.Width = 600;
        dlog1.TopMost = true;
        dlog1.Size = new Size(137, 60);

        Label textLabel = new Label();
        textLabel.AutoSize = true;
        textLabel.Location = new Point(12, 9);
        textLabel.Size = new Size(118, 13);
        textLabel.TabIndex = 0;
        textLabel.Text = "Shutdown in progress...";
        dlog1.Controls.Add(textLabel);

        button1 = new Button();
        button1.Location = new Point(30, 35);
        button1.Size = new Size(75, 23);
        button1.TabIndex = 1;
        button1.Text = "Cancel";
        button1.UseVisualStyleBackColor = true;
        button1.DialogResult = DialogResult.Cancel;
        dlog1.Controls.Add(button1);

        _tim.Interval = 1000;
        iTime = 0;
        _tim.Tick += TimTick;

        _tim.Enabled = true;
        _tim.Start();

        return dlog1.ShowDialog(myVegas.MainWindow);
    }

    void TimTick(object sender, EventArgs e)
    {
        if (iTime == 10)
        {
            myVegas.SaveProject(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PreShutdownSave.veg");
            myVegas.Exit();
            Process.Start("Shutdown", "-s -t 10");
            dlog1.Close();
            _tim.Enabled = false; // DO NOT REMOVE
            _tim.Stop(); // DO NOT REMOVE
        }
        else iTime++;
    }

}