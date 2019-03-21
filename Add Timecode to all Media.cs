/**
 * This script will add an effect to each item in the current
 * project's media pool.
 *
 * Revision Date: Jan. 2, 2007
**/

using System;
using ScriptPortal.Vegas;

class EntryPoint
{
    // plugInClassID is the class id of the effect plug-in you want to
    // add. The class id is used rather than the name so the script
    // will still work in localized versions of Vegas.
    Guid plugInClassID = new Guid("2869bb94-4971-4ccc-94ca-743666a85938"); // "Sony Timecode"

    // presetName is the name of the preset you want to use. Set it to
    // null if you want the default preset.
    String presetName = null;
    //String presetName = "SMPTE Drop (29.97 fps)";
    
    public void FromVegas(Vegas vegas)
    {

        PlugInNode plugIn = vegas.VideoFX.GetChildByClassID(plugInClassID);
        if (null == plugIn)
        {
            throw new ApplicationException("Could not find video plug-in.");
        }

        foreach (Media media in vegas.Project.MediaPool)
        {
            // only add the effect if the media object has video
            if (!media.HasVideo())
                continue;
            // and if it does not already have the effect
            if (MediaHasEffect(media, plugIn))
                continue;
            Effect effect = new Effect(plugIn);
            media.Effects.Add(effect);
            if (null != presetName)
                effect.Preset = presetName;
        }
    }

    public bool MediaHasEffect(Media media, PlugInNode plugIn)
    {
        foreach (Effect effect in media.Effects)
        {
            if (effect.PlugIn == plugIn)
                return true;
        }
        return false;
    }

}
