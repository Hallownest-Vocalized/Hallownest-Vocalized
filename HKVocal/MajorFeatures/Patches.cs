using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKVocals.MajorFeatures
{
    public class Patches
    {
        internal static void Hook()
        {
            ModHooks.LanguageGetHook += LanguageGetPatches;
        }

        private static string LanguageGetPatches(string key, string sheetTitle, string orig)
        {
            if (key == "SHOP_DESC_NOTCH_4")
                 return orig.Replace("more charms", "more of your charms");
            if (key == "HORNET_FOUNTAIN_1")
                return "Again we meet little ghost.<page>I'm normally quite perceptive. You I underestimated, though I've since guessed the truth.<page>You've seen beyond this kingdom's bounds. Yours is resilience born of two voids.<page>It's no surprise then you've managed to reach the heart of this world. In doing so, you shall know the sacrifice that keeps it standing.";
            if (key == "WITCH_REWARD_2A")
                return "Ahhh... Your Dream Nail holds over 200 Essence. You're proving your talent in its collection.<page>Have you seen that great door just outside? My tribe closed it long ago and forbade its opening.<page>Ah, but as far as I can tell I'm the only member of my tribe still breathing. That means I mustn't feel bad about breaking a taboo. As proof of my belief in you, I'll open the door.";
            if (key == "WATERWAYS_GREET")
                return "Ho ho! Aren't these waterways thrilling? A labyrinth of pipes and tunnels.<page>I couldn't have asked for a better place to employ my talents. It's all so orderly, so considered, nothing like the crude irregularity of those caverns.<page>Ahh but so sad, my trunk is telling me those Fungal Wastes are close and I sense my damp adventure may have reached its end. Guess I'll be calling this one done.";
            if (key == "WATERWAYS_BOUGHT")
                return "I'd wager these pipes and chambers were once used to carry the city's waste. Would've been a horrid stench down here. Thankfully that constant rain has flushed them clean. ";
            return orig;
        }
    }
}
