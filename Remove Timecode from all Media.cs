/**
 * This script will remove all effects of a particular type from items
 * in the project's media pool.
 *
 * Revision Date: Jan. 02, 2007
 **/

using System;
using ScriptPortal.Vegas;

class EntryPoint
{
    // This is the class id of the effect plug-in you want to add. The
    // class id is used rather than the name so the script will still
    // work in localized versions of Vegas.
    Guid plugInClassID = new Guid("2869bb94-4971-4ccc-94ca-743666a85938"); // "Sony Timecode"

    public void FromVegas(Vegas vegas)
    {

        PlugInNode plugIn = vegas.VideoFX.GetChildByClassID(plugInClassID);
        if (null == plugIn)
        {
            throw new ApplicationException("Could not find video plug-in.");
        }

        foreach (Media media in vegas.Project.MediaPool)
        {
            foreach (Effect effect in media.Effects)
            {
                if (effect.PlugIn == plugIn)
                {
                    media.Effects.Remove(effect);
                    break;
                }
            }
        }
    }
}
