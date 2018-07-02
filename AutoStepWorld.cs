using Terraria.ModLoader;

namespace MechScope
{
    internal class AutoStepWorld : ModWorld
    {
        public static bool Active = false;
        public static int Rate = 30;

        private static int count = 0;

        public override void PostUpdate()
        {
            if (Active && SuspendableWireManager.Running)
            {
                count++;
                if (count > Rate)
                {
                    count = 0;
                    SuspendableWireManager.Resume();
                }
            }
        }

        public static void ResetTimer()
        {
            count = 0;
        }
    }
}