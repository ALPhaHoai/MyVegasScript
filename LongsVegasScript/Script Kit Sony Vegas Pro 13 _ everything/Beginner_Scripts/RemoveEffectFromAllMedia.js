// This script will remove an effect from each item in the current
// project's media pool.

import System.Windows.Forms;
import SonicFoundry.Vegas.Script;

// This is the full name of the effect plug-in you want to remove.
var plugInName = "Sonic Foundry Timecode";


// You should not need to modify the code below here... But, of
// course, you can if you want ;-)
try {
    var mediaEnum = new Enumerator(Vegas.Project.MediaPool);
    while (!mediaEnum.atEnd()) {
        var media = mediaEnum.item();
        var effectsEnum = new Enumerator(media.Effects);
        while (!effectsEnum.atEnd()) {
            var effect = effectsEnum.item();
            if (plugInName == effect.PlugIn.Name) {
                media.Effects.Remove(effect);
            }
            effectsEnum.moveNext();
        }
        mediaEnum.moveNext();
    }
} catch (e) {
    MessageBox.Show(e);
}
