/**
 * This script renders each audio track individually.
 *
 * Revision Date: March 26, 2010.
 **/

using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using ScriptPortal.Vegas;

public class EntryPoint
{
    Vegas myVegas;
    
    String OutputDir;
    Renderer SelectedRenderer;
    RenderTemplate SelectedTemplate;
    bool CanUseSelection = false;
    bool UseSelection = true;

    string[] SupportedExtensions = {"*.wav", "*.aif", "*.flac", "*.mp3", "*.ogg", "*.pca", "*.w64"};

    TextBox OutputDirBox = new TextBox();
    Button BrowseButton = new Button();
    ComboBox RendererCombo = new ComboBox();
    ComboBox TemplateCombo = new ComboBox();
    CheckBox UseSelectionBox = new CheckBox();
    Button okButton = new Button();
    
    int ProjectChannelCount = 0;
    
    public void FromVegas(Vegas vegas)
    {
        myVegas = vegas;
        if (AudioBusMode.Surround == myVegas.Project.Audio.MasterBusMode)
        {
            ProjectChannelCount = 6;
        }
        else if (AudioBusMode.Stereo == myVegas.Project.Audio.MasterBusMode)
        {
            ProjectChannelCount = 2;
        }

        CanUseSelection = (1 < myVegas.Transport.LoopRegionLength.Nanos);
        
        if (DialogResult.OK != DoDialog())
        {
            return;
        }
        
        RenderArgs args = new RenderArgs();
        if (null == SelectedTemplate)
        {
            throw new ApplicationException("render template not selected.");
        }

        args.RenderTemplate = SelectedTemplate;
        args.UseSelection = CanUseSelection && UseSelection;
        foreach (Track track in myVegas.Project.Tracks)
        {
            if (track.IsAudio())
            {
                AudioTrack audioTrack = (AudioTrack) track;
                String trackName = String.Format("track {0:D2}", audioTrack.DisplayIndex);
                if (!String.IsNullOrEmpty(audioTrack.Name))
                {
                    trackName = String.Format("{0} ({1})", trackName, track.Name);
                }
                trackName += args.RenderTemplate.FileExtensions[0].Substring(1);
                args.OutputFile = Path.Combine(OutputDir, trackName);
                audioTrack.Solo = true;
                myVegas.Render(args);
                audioTrack.Solo = false;
            }
        }
    }


    DialogResult DoDialog()
    {
        Form form = new Form();
        form.SuspendLayout();
        form.AutoScaleMode = AutoScaleMode.Font;
        form.AutoScaleDimensions = new SizeF(6F, 13F);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterParent;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.HelpButton = false;
        form.ShowInTaskbar = false;
        form.Text = "Render Audio Tracks";
        form.AutoSize = true;
        form.AutoSizeMode = AutoSizeMode.GrowAndShrink;

        TableLayoutPanel layout = new TableLayoutPanel();
        layout.AutoSize = true;
        layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        layout.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
        layout.ColumnCount  = 3;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        form.Controls.Add(layout);

        Label label;

        label = new Label();
        label.Text = "Output Folder:";
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = new Padding(8, 8, 8, 4);
        label.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        layout.Controls.Add(label);
        layout.SetColumnSpan(label, 3);
        
        if (!String.IsNullOrEmpty(myVegas.Project.FilePath))
        {
            OutputDirBox.Text = Path.GetDirectoryName(myVegas.Project.FilePath);
        }
        else
        {
            OutputDirBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        OutputDirBox.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        OutputDirBox.Margin = new Padding(16, 8, 8, 4);
        layout.Controls.Add(OutputDirBox);
        layout.SetColumnSpan(OutputDirBox, 2);

        BrowseButton.FlatStyle = FlatStyle.System;
        BrowseButton.Text = "Browse";
        BrowseButton.AutoSize = true;
        layout.Controls.Add(BrowseButton);


        label = new Label();
        label.Text = "Save as type:";
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = new Padding(8, 8, 8, 4);
        label.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        layout.Controls.Add(label);

        RendererCombo.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        layout.Controls.Add(RendererCombo);
        layout.SetColumnSpan(RendererCombo, 2);

        label = new Label();
        label.Text = "Template:";
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = new Padding(8, 8, 8, 4);
        label.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        layout.Controls.Add(label);

        TemplateCombo.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        layout.Controls.Add(TemplateCombo);
        layout.SetColumnSpan(TemplateCombo, 2);

        UseSelectionBox.Text = "Render loop region only";
        UseSelectionBox.Checked = CanUseSelection && UseSelection;
        UseSelectionBox.Enabled = CanUseSelection;
        UseSelectionBox.AutoSize = false;
        UseSelectionBox.FlatStyle = FlatStyle.System;
        UseSelectionBox.Margin = new Padding(16, 8, 8, 4);
        UseSelectionBox.Anchor = AnchorStyles.Left|AnchorStyles.Right;
        layout.Controls.Add(UseSelectionBox);
        layout.SetColumnSpan(UseSelectionBox, 3);
            
        FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
        buttonPanel.FlowDirection = FlowDirection.RightToLeft;
        buttonPanel.Size = Size.Empty;
        buttonPanel.AutoSize = true;
        buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        buttonPanel.Margin = new Padding(8, 8, 8, 8);
        buttonPanel.Anchor = AnchorStyles.Top|AnchorStyles.Right;
        layout.Controls.Add(buttonPanel);
        layout.SetColumnSpan(buttonPanel, 3);
            
        Button cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.FlatStyle = FlatStyle.System;
        cancelButton.DialogResult = DialogResult.Cancel;
        buttonPanel.Controls.Add(cancelButton);
        form.CancelButton = cancelButton;

        okButton.Text = "OK";
        okButton.FlatStyle = FlatStyle.System;
        okButton.DialogResult = DialogResult.OK;
        buttonPanel.Controls.Add(okButton);
        form.AcceptButton = okButton;

        BrowseButton.Click += HandleBrowseOutputDir;
        FillRenderers(RendererCombo);
        RendererCombo.SelectedValueChanged += HandleRendererChanged;
        if (0 < RendererCombo.Items.Count)
        {
            RendererCombo.SelectedIndex = 0;
        }
        form.ResumeLayout();
        
        
        DialogResult result = form.ShowDialog(myVegas.MainWindow);
        if (DialogResult.OK == result)
        {
            OutputDir = OutputDirBox.Text;
            SelectedTemplate = TemplateCombo.SelectedItem as RenderTemplate;
            if (CanUseSelection) UseSelection = UseSelectionBox.Checked;
        }
        return result;
    }

    void HandleBrowseOutputDir(Object sender, EventArgs args)
    {
        string newDir = null;
        myVegas.FileUtilities.SelectDirectoryDlg(myVegas.MainWindow.Handle, "Output Folder", OutputDirBox.Text, true, out newDir);
        if (!String.IsNullOrEmpty(newDir))
        {
            OutputDirBox.Text = newDir;
        }
    }
    
    void FillRenderers(ComboBox box)
    {
        foreach (Renderer renderer in myVegas.Renderers)
        {
            if (ShouldAddRenderer(renderer))
            {
                box.Items.Add(renderer);
            }
        }
    }

    bool ShouldAddRenderer(Renderer renderer)
    {
        if (renderer.Name.Contains("Scott"))
        {
            return false;
        }
        foreach (string ext in renderer.FileExtensions)
        {
            if (ShouldAddExtension(ext))
            {
                return true;
            }
        }
        return false;
    }

    bool ShouldAddExtension(string ext)
    {
        foreach (string supportedExt in SupportedExtensions)
        {
            if (ext == supportedExt)
            {
                return true;
            }
        }
        return false;
    }

    void HandleRendererChanged(Object sender, EventArgs args)
    {
        Renderer renderer = RendererCombo.SelectedItem as Renderer;
        if (null == renderer) return;
        TemplateCombo.Items.Clear();
        foreach (RenderTemplate plate in renderer.Templates)
        {
            if (ProjectChannelCount < plate.AudioChannelCount)
            {
                continue;
            }
            TemplateCombo.Items.Add(plate);
        }
        if (0 < TemplateCombo.Items.Count)
        {
            TemplateCombo.SelectedIndex = 0;
            okButton.Enabled = true;
        }
        else
        {
            okButton.Enabled = false;
        }
    }

}