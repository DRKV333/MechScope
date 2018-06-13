using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MechScope
{
    public class MechScope : Mod
    {

        public static ModHotKey keyToggle;
        public static ModHotKey keyStep;
        public static ModHotKey keyAutoStep;

        HarmonyInstance harmonyInstance;

        public MechScope()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadBackgrounds = true,
                AutoloadGores = true,
                AutoloadSounds = true,
            };
        }

        public override void Load()
        {
            if (harmonyInstance == null)
                harmonyInstance = HarmonyInstance.Create(Name);

            harmonyInstance.PatchAll();

            keyToggle = RegisterHotKey("Toggle", "");
            keyStep = RegisterHotKey("Step", "");
            keyAutoStep = RegisterHotKey("Auto step", "");

        }

        public override void Unload()
        {
            MethodBase[] methods = harmonyInstance.GetPatchedMethods().ToArray();

            foreach (var item in methods)
            {
                harmonyInstance.RemovePatch(item, HarmonyPatchType.All, harmonyInstance.Id);
            }
        }
    }
}
