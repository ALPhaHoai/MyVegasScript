// This script will add an effect to each item in the current
// project's media pool.

import System.Windows.Forms;
import SonicFoundry.Vegas.Script;


// This is the media type of the effect you want to add (choose
// MediaType.Audio or MediaType.Video).
var mediaType = MediaType.Video; 

// This is the full name of the effect plug-in you want to add.
var plugInName = "Sonic Foundry Timecode";

// This is the name of the preset you want. Set this to null if you
// want the default preset.
var presetName = "SMPTE Drop (29.97 fps)";



// You should not need to modify the code below here... But, of
// course, you can if you want ;-)
try {
    var fx, isVideoEffect;
    if (MediaType.Video == mediaType) {
        fx = Vegas.VideoFX;
        isVideoEffect = true;
    } else if (MediaType.Audio == mediaType) {
        fx = Vegas.AudioFX;
        isVideoEffect = false;
    } else {
        throw "unknown media type: MediaType." + mediaType;
    }
    
    var plugIn = fx.GetChildByName(plugInName);
    if (null == plugIn) {
        throw "could not find a plug-in named: '" + plugInName + "'";
    }

    var mediaEnum = new Enumerator(Vegas.Project.MediaPool);
    while (!mediaEnum.atEnd()) {
        var media = mediaEnum.item();
        // only add the effect if the media object has the proper type
        // of stream(s)
        if ((isVideoEffect && media.HasVideo()) || (!isVideoEffect && media.HasAudio())) {
            var effect = new Effect(plugIn);
            media.Effects.Add(effect);
            if (null != presetName) {
                effect.Preset = presetName;
            }
        }
        mediaEnum.moveNext();
    }
} catch (e) {
    MessageBox.Show(e);
}
