/**
 * This application extension demonstrates how to create a custom view
 * command with a dockable control that contains a web browser.  It
 * also demonstrates how to use track asynchronous tasks using a
 * ProgressWorker object.
 *
 * Revision Date: Jul 09, 2007.
 **/
using System;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using Sony.Vegas;

namespace SampleWebBrowserExtenstion
{
    
public class SampleWebBrowser : DockableControl
{
    WebBrowser myBrowser;
    
    public SampleWebBrowser() : base("SampleWebBrowser")
    {
        this.DisplayName = "Sample Web Browser";
    }

    public override DockWindowStyle DefaultDockWindowStyle
    {
        get { return DockWindowStyle.Docked; }
    }

    public override Size DefaultFloatingSize
    {
        get { return new Size(640, 480); }
    }
    
    protected override void OnLoad(EventArgs args)
    {
        myBrowser = new WebBrowser();
        myBrowser.Dock = DockStyle.Fill;
        this.Controls.Add(myBrowser);
        myBrowser.Navigating += this.HandleNavigating;
        myBrowser.ProgressChanged += HandleProgressChanged;
        myBrowser.DocumentCompleted += HandleDocumentCompleted;
        myBrowser.StatusTextChanged += HandleStatusTextChanged;
        myBrowser.Url = new Uri("http://www.sonycreativesoftware.com/");
    }

    ProgressWorker myNavWorker;
    ManualResetEvent myNavEvent = new ManualResetEvent(false);
    
    void HandleNavigating(Object sender, WebBrowserNavigatingEventArgs args)
    {
        if (null == myNavWorker)
        {
            myNavWorker = new ProgressWorker();
            myNavWorker.BlockUserInput = false;
            myNavWorker.DoWork += this.HandleNavWorkerDoWork;
            myNavWorker.Dequeued += this.HandleNavWorkerDequeued;
            myNavEvent.Reset();
            myVegas.QueueProgressWorker(myNavWorker);
        }
    }

    void HandleProgressChanged(Object sender, WebBrowserProgressChangedEventArgs args)
    {
        if (null != myNavWorker)
        {
            myNavWorker.ProgressMin = 0.0;
            myNavWorker.ProgressMax = (double) args.MaximumProgress;
            myNavWorker.Progress = (double) args.CurrentProgress;
        }
    }

    void HandleStatusTextChanged(Object sender, EventArgs args)
    {
        if (null != myNavWorker)
        {
            myNavWorker.ProgressText = myBrowser.StatusText;
        }
    }
    
    void HandleDocumentCompleted(Object sender, WebBrowserDocumentCompletedEventArgs args)
    {
        if (null != myNavWorker)
        {
            myNavWorker.Progress = myNavWorker.ProgressMax;
            myNavEvent.Set();
        }
    }
    
    void HandleNavWorkerDoWork(ProgressWorker worker, ProgressEventArgs args)
    {
        while (!worker.Canceled)
        {
            if (myNavEvent.WaitOne(100, false))
            {
                break;
            }
        }
        if (worker.Canceled)
        {
            myBrowser.Stop();
        }
    }

    void HandleNavWorkerDequeued(Object sender, EventArgs args)
    {
        myNavWorker = null;
    }
}

public class SampleWebBrowserModule : ICustomCommandModule
{
    protected Vegas myVegas = null;

    public void InitializeModule(Vegas vegas)
    {
        myVegas = vegas;
    }

    CustomCommand mySampleWebBrowserCommand = new CustomCommand(CommandCategory.View, "Sample Web Browser");

    public ICollection GetCustomCommands()
    {
        mySampleWebBrowserCommand.MenuPopup += this.HandleSampleWebBrowserCmdMenuPopup;
        mySampleWebBrowserCommand.Invoked += this.HandleSampleWebBrowserCmdInvoked;
        return new CustomCommand[] { mySampleWebBrowserCommand };
    }

    void HandleSampleWebBrowserCmdMenuPopup(Object sender, EventArgs args)
    {
        CustomCommand cmd = (CustomCommand) sender;
        cmd.Checked = myVegas.FindDockView("SampleWebBrowser");
    }

    void HandleSampleWebBrowserCmdInvoked(Object sender, EventArgs args)
    {
        if (!myVegas.ActivateDockView("SampleWebBrowser"))
        {
            SampleWebBrowser dockView = new SampleWebBrowser();
            dockView.AutoLoadCommand = mySampleWebBrowserCommand;
            myVegas.LoadDockView(dockView);
        }
    }

}

}
