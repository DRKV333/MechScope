using Terraria.GameInput;
using Terraria.ModLoader;

namespace MechScope
{
    internal class ControlPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (MechScope.keyStep.JustPressed)
                SuspendableWireManager.Resume();

            if (MechScope.keyToggle.JustPressed)
                SuspendableWireManager.Active = !SuspendableWireManager.Active;

            if (MechScope.keyAutoStep.JustPressed)
                AutoStepWorld.Active = !AutoStepWorld.Active;

            if (MechScope.keySettings.JustPressed)
                MechScope.settingsUI.Visible = !MechScope.settingsUI.Visible;
        }
    }
}