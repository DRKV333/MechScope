using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechScope.Patches
{
    [HarmonyPatch(typeof(Terraria.Wiring), "UpdateMech")]
    [HarmonyPriority(Priority.First)]
    class UpdateMechPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !SuspendableWireManager.Running;
        }
    }
}
