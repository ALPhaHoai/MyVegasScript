/**
 * Copies the Location data (animation keyframes) from the first mask
 * of the selected video event's Bezier Masking FX to a newly created 'Titles & Text' event
 *
 * Revision Date: Aug 17 2018
 **/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using ScriptPortal.Vegas;

public class EntryPoint
{
    Vegas myVegas;

    public void FromVegas(Vegas vegas)
    {
        myVegas = vegas;

        System.Collections.Generic.List<OFXDouble2DKeyframe> locations = new System.Collections.Generic.List<OFXDouble2DKeyframe>();
        Tuple<Timecode, Timecode> durationTrackEvent;

        VideoEvent trackEvent = (VideoEvent)FindFirstSelectedEventUnderCursor();
        if (trackEvent == null)
        {
            MessageBox.Show("Please select the video event on which VEGAS Bezier Masking has been applied.", "No selected event", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        else
        {
            durationTrackEvent = new Tuple<Timecode, Timecode>(trackEvent.Start, trackEvent.End);
            Effects fxs = trackEvent.Effects;
            bool bezierWasFound = false;
            foreach (Effect fx in fxs)
            {
                bezierWasFound = fx.PlugIn.UniqueID == "{Svfx:com.sonycreativesoftware:bzmasking}";
                if (bezierWasFound)
                {
                    OFXParameters parameter = fx.OFXEffect.Parameters;
                    foreach (OFXParameter param in parameter)
                    {
                        if (param.Name == "Location_0")
                        {
                           OFXDouble2DParameter locationTracking = (OFXDouble2DParameter)param;
                           locations = new List<OFXDouble2DKeyframe>(locationTracking.Keyframes);
                           break;
                        }

                    }

                    break;
                }
            }
            if (!bezierWasFound)
            {
                MessageBox.Show("Please apply VEGAS Bezier Masking to the video event", "VEGAS Bezier Masking not applied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        if (locations.Count == 0)
        {
            MessageBox.Show("Please add Motion Tracking to the VEGAS Bezier Masking FX", "No tracking data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        else
        {
           using (OffsetDialog offsetPrompt = new OffsetDialog())
           {
               DialogResult result = offsetPrompt.ShowDialog();
               if (result == DialogResult.OK)
               {
                  VideoTrack titleTrack = myVegas.Project.AddVideoTrack();
                  PlugInNode textAndTitles = myVegas.Generators.GetChildByUniqueID("{Svfx:com.sonycreativesoftware:titlesandtext}"); // GetChildByName("VEGAS Titles & Text");// 
                  Timecode lengthEvent = durationTrackEvent.Item2 - durationTrackEvent.Item1;
                  Media media = new Media(textAndTitles, "Placeholder");
                  media.Length = lengthEvent;
                  VideoEvent vEvent = titleTrack.AddVideoEvent(durationTrackEvent.Item1, lengthEvent);
                  Take take = new Take(media.Streams[0]);
                  vEvent.Takes.Add(take);

                  Effect fxText = media.Generator;
                  OFXParameters parameter = fxText.OFXEffect.Parameters;
                  foreach (OFXParameter param in parameter)
                  {
                      if (param.Name == "Location")
                      {
                          OFXDouble2DParameter locationText = (OFXDouble2DParameter)param;
                          locationText.Keyframes.Clear();
                          foreach (OFXDouble2DKeyframe location in locations)
                          {
                              OFXDouble2D tmpValue = location.Value;
                              tmpValue.X += offsetPrompt.X;
                              tmpValue.Y += offsetPrompt.Y;
                              locationText.SetValueAtTime(location.Time, tmpValue);
                          }

                          break;
                      }
                  }
               }
            }
        }
    }

    /// <summary>
    /// Returns the first selected event that's under the cursor
    /// </summary>
    /// <returns>The first selected event or null if no event selected</returns>
    TrackEvent FindFirstSelectedEventUnderCursor()
    {
        foreach (Track track in myVegas.Project.Tracks)
        {
            foreach (TrackEvent trackEvent in track.Events)
            {
                if (trackEvent.Selected && trackEvent.Start <= myVegas.Transport.CursorPosition && trackEvent.End >= myVegas.Transport.CursorPosition)
                {
                    return trackEvent;
                }
            }
        }

        return null;
    }
}

// ......................................
public class OffsetDialog : Form
{
    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.label_xOffset = new System.Windows.Forms.Label();
        this.label_yOffset = new System.Windows.Forms.Label();
        this.textBox_xOffset = new System.Windows.Forms.TextBox();
        this.textBox_yOffset = new System.Windows.Forms.TextBox();
        this.trackBar_xOffset = new System.Windows.Forms.TrackBar();
        this.trackBar_yOffset = new System.Windows.Forms.TrackBar();
        this.button_Ok = new System.Windows.Forms.Button();
        this.button_Cancel = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.trackBar_xOffset)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.trackBar_yOffset)).BeginInit();
        this.SuspendLayout();
        // 
        // label_xOffset
        // 
        this.label_xOffset.AutoSize = true;
        this.label_xOffset.Location = new System.Drawing.Point(13, 48);
        this.label_xOffset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.label_xOffset.Name = "label_xOffset";
        this.label_xOffset.Size = new System.Drawing.Size(95, 25);
        this.label_xOffset.TabIndex = 0;
        this.label_xOffset.Text = "X Offset:";
        // 
        // label_yOffset
        // 
        this.label_yOffset.AutoSize = true;
        this.label_yOffset.Location = new System.Drawing.Point(12, 150);
        this.label_yOffset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.label_yOffset.Name = "label_yOffset";
        this.label_yOffset.Size = new System.Drawing.Size(96, 25);
        this.label_yOffset.TabIndex = 3;
        this.label_yOffset.Text = "Y Offset:";
        // 
        // textBox_xOffset
        // 
        this.textBox_xOffset.Location = new System.Drawing.Point(465, 48);
        this.textBox_xOffset.Margin = new System.Windows.Forms.Padding(4);
        this.textBox_xOffset.Name = "textBox_xOffset";
        this.textBox_xOffset.Size = new System.Drawing.Size(132, 31);
        this.textBox_xOffset.TabIndex = 2;
        this.textBox_xOffset.TextChanged += new System.EventHandler(this.textBox_xOffset_TextChanged);
        // 
        // textBox_yOffset
        // 
        this.textBox_yOffset.Location = new System.Drawing.Point(462, 144);
        this.textBox_yOffset.Margin = new System.Windows.Forms.Padding(4);
        this.textBox_yOffset.Name = "textBox_yOffset";
        this.textBox_yOffset.Size = new System.Drawing.Size(132, 31);
        this.textBox_yOffset.TabIndex = 5;
        this.textBox_yOffset.TextChanged += new System.EventHandler(this.textBox_yOffset_TextChanged);
        // 
        // trackBar_xOffset
        // 
        this.trackBar_xOffset.Location = new System.Drawing.Point(115, 45);
        this.trackBar_xOffset.Maximum = 500;
        this.trackBar_xOffset.Minimum = -500;
        this.trackBar_xOffset.Name = "trackBar_xOffset";
        this.trackBar_xOffset.Size = new System.Drawing.Size(333, 90);
        this.trackBar_xOffset.TabIndex = 1;
        this.trackBar_xOffset.ValueChanged += new System.EventHandler(this.trackBar_xOffset_ValueChanged);
        // 
        // trackBar_yOffset
        // 
        this.trackBar_yOffset.Location = new System.Drawing.Point(115, 141);
        this.trackBar_yOffset.Maximum = 500;
        this.trackBar_yOffset.Minimum = -500;
        this.trackBar_yOffset.Name = "trackBar_yOffset";
        this.trackBar_yOffset.Size = new System.Drawing.Size(333, 90);
        this.trackBar_yOffset.TabIndex = 4;
        this.trackBar_yOffset.ValueChanged += new System.EventHandler(this.trackBar_yOffset_ValueChanged);
        // 
        // button_Ok
        // 
        this.button_Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.button_Ok.Location = new System.Drawing.Point(199, 240);
        this.button_Ok.Margin = new System.Windows.Forms.Padding(4);
        this.button_Ok.Name = "button_Ok";
        this.button_Ok.Size = new System.Drawing.Size(100, 49);
        this.button_Ok.TabIndex = 6;
        this.button_Ok.Text = "OK";
        this.button_Ok.UseVisualStyleBackColor = true;
        this.button_Ok.Click += new System.EventHandler(this.buttonOK_Click);
        // 
        // button_Cancel
        // 
        this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.button_Cancel.Location = new System.Drawing.Point(333, 240);
        this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
        this.button_Cancel.Name = "button_Cancel";
        this.button_Cancel.Size = new System.Drawing.Size(100, 49);
        this.button_Cancel.TabIndex = 7;
        this.button_Cancel.Text = "Cancel";
        this.button_Cancel.UseVisualStyleBackColor = true;
        this.button_Cancel.Click += new System.EventHandler(this.buttonCancel_Click);
        // 
        // OffsetDialog
        // 
        this.AcceptButton = this.button_Ok;
        this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.CancelButton = this.button_Cancel;
        this.ClientSize = new System.Drawing.Size(617, 327);
        this.ControlBox = false;
        this.Controls.Add(this.button_Cancel);
        this.Controls.Add(this.button_Ok);
        this.Controls.Add(this.textBox_yOffset);
        this.Controls.Add(this.textBox_xOffset);
        this.Controls.Add(this.label_yOffset);
        this.Controls.Add(this.label_xOffset);
        this.Controls.Add(this.trackBar_xOffset);
        this.Controls.Add(this.trackBar_yOffset);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Margin = new System.Windows.Forms.Padding(4);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "OffsetDialog";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
        this.Text = "Text Position";
        ((System.ComponentModel.ISupportInitialize)(this.trackBar_xOffset)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.trackBar_yOffset)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    // ...................

    private System.Windows.Forms.Label label_xOffset;
    private System.Windows.Forms.Label label_yOffset;
    private System.Windows.Forms.TextBox textBox_xOffset;
    private System.Windows.Forms.TextBox textBox_yOffset;
    private System.Windows.Forms.TrackBar trackBar_xOffset;
    private System.Windows.Forms.TrackBar trackBar_yOffset;
    private System.Windows.Forms.Button button_Ok;
    private System.Windows.Forms.Button button_Cancel;

    // ...................

    private double offsetX = 0.0;
    private double offsetY = 0.0;

    public OffsetDialog()
    {
        InitializeComponent();
        textBox_xOffset.Text = String.Format("{0:F2}", offsetX);
        textBox_yOffset.Text = String.Format("{0:F2}", offsetX);
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
        try
        {
            double.TryParse(textBox_xOffset.Text, out offsetX);
            double.TryParse(textBox_yOffset.Text, out offsetY);

            if (IsWithinAllowedRange(offsetX) && IsWithinAllowedRange(offsetY))
            {
                Close();
            }
            else
            {
                MessageBox.Show("Value is outside the expected -0.5 to 0.5 range.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        Close();
    }
    private void trackBar_xOffset_ValueChanged(object sender, EventArgs e)
    {
        textBox_xOffset.Text = (0.001 * trackBar_xOffset.Value).ToString();
    }
    private void trackBar_yOffset_ValueChanged(object sender, EventArgs e)
    {
        textBox_yOffset.Text = (0.001 * trackBar_yOffset.Value).ToString();
    }

    private bool IsWithinAllowedRange(double v)
    {
        return ((v >= -0.5) && (v <= 0.5));
    }

    public double X
    {
        get { return offsetX; }
    }
    public double Y
    {
        get { return offsetY; }
    }

    private void textBox_xOffset_TextChanged(object sender, EventArgs e)
    {
        double.TryParse(textBox_xOffset.Text, out offsetX);
        if (!IsWithinAllowedRange(offsetX))
        {
            textBox_xOffset.Text = "0";
            offsetX = 0;
        }
        trackBar_xOffset.Value = (int)(1000 * offsetX);
    }

    private void textBox_yOffset_TextChanged(object sender, EventArgs e)
    {
        double.TryParse(textBox_yOffset.Text, out offsetY);
        if (!IsWithinAllowedRange(offsetY))
        {
            textBox_yOffset.Text = "0";
            offsetY = 0;
        }
        trackBar_yOffset.Value = (int)(1000 * offsetY);
    }
}

