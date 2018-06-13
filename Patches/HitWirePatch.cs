using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using Harmony;
using Terraria;
using Terraria.ModLoader;
using System.Reflection;

namespace MechScope.Patches
{
    [HarmonyPatch(typeof(Wiring), "HitWire")]
    [HarmonyPriority(Priority.Normal)]
    class HitWirePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> original)
        {
            bool injectPostHitWireSingle = false;

            foreach (var item in original)
            {
                if(injectPostHitWireSingle)
                {
                    injectPostHitWireSingle = false;

                    CodeInstruction[] inst = SuspendableWireManager.MakeSuspendSnippet(generator, SuspendableWireManager.SuspendMode.perSingle);

                    //The end of a branch is in front, but we want to be outside if it.
                    inst[0].labels = new List<Label>(item.labels);
                    item.labels.Clear();

                    foreach (var item2 in inst)
                    {
                        //ErrorLogger.Log(item2);
                        yield return item2;
                    }
                }

                //Inject code after call to HitWireSingle at IL_0085
                if (item.opcode == OpCodes.Call)
                {
                    MethodInfo calledMethod = item.operand as MethodInfo;
                    if (calledMethod != null && calledMethod.Name == "HitWireSingle")
                    {
                        injectPostHitWireSingle = true;
                    }
                }

                //ErrorLogger.Log(item);
                yield return item;
            }
        }
    }
}
