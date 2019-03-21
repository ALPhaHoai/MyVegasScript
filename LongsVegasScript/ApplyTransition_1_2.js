/**
 * Apply Transition to Adjacent Video events and optionally move
 * events to overlap events.
 *
 * For use with Sonic Foundry Vegas Video 4.0
 *
 * Copyright 2003 murkWare (murk@murkvisuals.com)
 *
 * Minor update on Dec 18, 2003 to fix issues with latest API
 *
 * UPDATE 1/6/2004: Jeffrey Creem has added a "Zoom Percent" option
 *                  Which will zoom in on each event and pan across it
 **/
import System.Windows.Forms;
import ScriptPortal.Vegas;
import ScriptPortal.Vegas.Script;
var overlapTime = 30;

var ZoomSlidePercent = 80;


var dialog = new TransitionDialog(overlapTime);
var bFade  = false;  //Only true if the second list item is chosen
var bRandom  = false;  //Only true if the first list item is chosen
dialog.m_transList.Items.Add("Random For each event")
dialog.m_transList.Items.Add("Standard Cross Fade")
var count = 0;
var totalTrans = Vegas.Transitions.Count;
var num;
var transEnum = new Enumerator(Vegas.Transitions);

//Get the current ruler format so we can restore it later
var oPrevRulerFormat = Vegas.Project.Ruler.Format;

//Set the ruler format to absolute frames
Vegas.Project.Ruler.Format = RulerFormat.AbsoluteFrames;

while (!transEnum.atEnd())
{
	var trans = transEnum.item();
	if(count > 0)
		dialog.m_transList.Items.Add(trans.Name);
	count++;

	transEnum.moveNext();
}
try
{
	dialog.m_transList.SelectedIndex = 0
	var dialogResult = dialog.ShowDialog();
	var iTrans = int(dialog.m_transList.SelectedIndex);
	if(System.Windows.Forms.DialogResult.OK == dialogResult)
	{
		if (iTrans == 0)
		{
			bRandom = true;
		}
		else if(iTrans == 1)
		{
			bFade = true
		}

		var plugIn;
		if(iTrans > 1)
		{
			plugIn = Vegas.Transitions.GetChild(int(iTrans -1));
		}

		overlapTime = int(dialog.overlapTimeBox.Text);
		ZoomSlidePercent = int(dialog.ZoomSlidePercentBox.Text);

		var startoffset = overlapTime;
		var trackEnum = new Enumerator(Vegas.Project.Tracks);
		var fx;
 		while (!trackEnum.atEnd())
		{
			var tr = trackEnum.item();
   			var eventEnum = new Enumerator(tr.Events);

 			while (!eventEnum.atEnd())
			{
				var ev = eventEnum.item();

				if (ZoomSlidePercent  != 100 )
				{var startTimecode = new Timecode(ev.Start);
				 var stopTimecode = new Timecode(ev.Length);
 				 var startMotionKeyFrame = ev.VideoMotion.Keyframes[0];
				 var endMotionKeyFrame = new VideoMotionKeyframe(stopTimecode);


				 var Scale = new VideoMotionVertex(float(ZoomSlidePercent) / 100.0, float(ZoomSlidePercent) / 100.0);
				 Scale.X = Scale.X;
				 Scale.Y = Scale.Y;
				 startMotionKeyFrame.ScaleBy(Scale);
				 var Offset = new VideoMotionVertex(-startMotionKeyFrame.TopLeft.X, -startMotionKeyFrame.TopLeft.Y);
				 Offset.X = Offset.X * (0.5 + Math.random())/2.0;
				 Offset.Y = Offset.Y * (0.5 + Math.random())/2.0;

				 if (Math.random() > 0.5) {Offset.X = Offset.X * -1.0};
				 if (Math.random() > 0.5) {Offset.Y = Offset.Y * -1.0};
 				 startMotionKeyFrame.MoveBy(Offset);

				 ev.VideoMotion.Keyframes.Add(endMotionKeyFrame)

				 //endMotionKeyFrame.TopLeft(startMotionKeyFrame.TopLeft);
				 //endMotionKeyFrame.BottomLeft(startMotionKeyFrame.BottomLeft);
				 //endMotionKeyFrame.TopRight(startMotionKeyFrame.TopRight);
				 //endMotionKeyFrame.BottomRight(startMotionKeyFrame.BottomRight);

				 Offset.X = Offset.X*(-2.0);
				 Offset.Y = Offset.Y*(-2.0);
	 			 endMotionKeyFrame.MoveBy(Offset);

				}

				ev.FadeIn.Curve = CurveType.Slow
				if(bRandom)
				{
					num = int(Math.random() * totalTrans + 1);
					if (num > 23)
					{
						num = totalTrans - 1;
					}
					plugIn = Vegas.Transitions.GetChild(int(num));
				}


				var startTime = new Timecode(ev.Start);
				var length = new Timecode(ev.Length);
				var offset = new Timecode(startoffset);
				//MessageBox.Show("Start Time: " + startTime + " Offset: " + offset + " startoffset: " + startoffset);
				startTime  = startTime - offset;
				ev.AdjustStartLength(startTime,length,true);
				Vegas.UpdateUI();
				if(ev.MediaType == MediaType.Video && !bFade)
				{
					fx = new Effect(plugIn);
					ev.FadeIn.Transition = fx;
				}
				eventEnum.moveNext();
				startoffset = startoffset + overlapTime;
			}
			startoffset = overlapTime;
			trackEnum.moveNext();
		}
	}
} catch (e)
{
	MessageBox.Show(e + "\n\nReport this error to alphahoai@gmail.com, thank you\n\n" + num);
}
//Restore the original ruler format
Vegas.Project.Ruler.Format = oPrevRulerFormat;

// Form subclass that is the dialog box for this script
class TransitionDialog extends Form {
    var ZoomSlidePercentBox;
    var overlapTimeBox;
    var m_transList;

    function TransitionDialog(overlapTime) {
        this.Text = "Add Transitions to adjacent events";
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Width = 480;
        this.Height = 160;

        var buttonWidth = 80;
        var buttonHeight = 24;
         var buttonTop = 80;

        overlapTimeBox = addTextControl("Overlap Frames", 320, 140, 20, overlapTime.ToString());
        ZoomSlidePercentBox = addTextControl("Zoom Slide", 320, 140, 40 ,ZoomSlidePercent.ToString());

        m_transList = addComboBox(20,80,20);

        var okButton = new Button();
        okButton.Text = "OK";
        okButton.Left = this.Width - ((buttonWidth+10));
        okButton.Top = buttonTop;
        okButton.Width = buttonWidth;
        okButton.Height = buttonHeight;
        okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        AcceptButton = okButton;
        Controls.Add(okButton);

        var label = new Label();
        label.AutoSize = true;
        label.Text =  "Copyright 2003 murkWare (www.murkvisuals.com)"
        label.Left = 20;
        label.Top = 70;
        Controls.Add(label);
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

    function addComboBox(left,width,top)
    {

        var transList = new ComboBox();

      //  transList.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
      //              Or System.Windows.Forms.AnchorStyles.Right)
        transList.DropDownWidth = width;
       // transList.Items.AddRange(tem 5"});
        transList.Location = new System.Drawing.Point(left, top);
        transList.Size = new System.Drawing.Size(280, 21);
        transList.TabIndex = 7;
        Controls.Add(transList);

		return transList;
    }

}
