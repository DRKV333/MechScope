using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MechScope
{
    class AutoStepWorld : ModWorld
    {
        public static bool Active = false;
        public static int Rate = 30;

        private int count = 0;

        public override void PostUpdate()
        {
            if(Active)
            {
                count++;
                if(count > Rate)
                {
                    count = 0;
                    SuspendableWireManager.Resume();
                }
            }
        }
    }
}
