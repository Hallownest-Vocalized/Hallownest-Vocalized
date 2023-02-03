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
            return orig;
        }
    }
}
