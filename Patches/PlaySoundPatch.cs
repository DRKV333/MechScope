using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Audio;

namespace MechScope.Patches
{
    internal class PlaySoundPatch
    {
        public static void Load()
        {
            On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback += Prefix;
        }

        private static SlotId Prefix(On_SoundEngine.orig_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback orig, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback)
        {
            if (SuspendableWireManager.IsWireThread)
                return SlotId.Invalid;

            return orig(ref style, position, updateCallback);
        }
    }
}
