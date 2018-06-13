using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace MechScope
{
    class ControlPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if(MechScope.keyStep.JustPressed)
                SuspendableWireManager.Resume();  

            if (MechScope.keyToggle.JustPressed)
                SuspendableWireManager.Active = !SuspendableWireManager.Active;

            if (MechScope.keyAutoStep.JustPressed)
                AutoStepWorld.Active = !AutoStepWorld.Active;
        }
    }
}
