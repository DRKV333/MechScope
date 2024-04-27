using Terraria;

namespace MechScope.Patches
{
    internal class UpdateMechPatch
    {
        public static void Load()
        {
            On_Wiring.UpdateMech += Prefix;
        }

        private static void Prefix(On_Wiring.orig_UpdateMech orig)
        {
            if (!SuspendableWireManager.Running)
                orig();
        }
    }
}