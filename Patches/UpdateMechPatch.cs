using Harmony;

namespace MechScope.Patches
{
    [HarmonyPatch(typeof(Terraria.Wiring), "UpdateMech")]
    [HarmonyPriority(Priority.First)]
    internal class UpdateMechPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return !SuspendableWireManager.Running;
        }
    }
}