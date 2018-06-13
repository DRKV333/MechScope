using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace MechScope.Patches
{
    [HarmonyPatch(typeof(Wiring), "LogicGatePass")]
    [HarmonyPriority(Priority.Normal)]
    class LogicGatePassPatch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> original)
        {

            bool inject = false;
            bool injectedPostBranch = false;
            bool injectedPostBlockTP = false;

            Stack<Label> prevJumps = new Stack<Label>();

            foreach (var item in original)
            {
                
                if (inject)
                {
                    inject = false;

                    CodeInstruction[] inst = SuspendableWireManager.MakeSuspendSnippet(generator, SuspendableWireManager.SuspendMode.perStage);

                    //There is a branch on the end we need to skip
                    
                    if(injectedPostBlockTP)
                    {
                        Label label = prevJumps.Pop();
                        inst[0].labels.Add(label);
                        item.labels.Remove(label);
                    }

                    foreach (var item2 in inst)
                    {
                        //ErrorLogger.Log(item2);
                        yield return item2;
                    }

                }

                //We want to inject after the end of the first loop at IL_0045
                if (!injectedPostBranch && (item.opcode == OpCodes.Bgt || item.opcode == OpCodes.Bgt_S))
                {
                    injectedPostBranch = true;
                    inject = true;
                }

                //We also need to put one on the end after IL_00E9
                if (!injectedPostBlockTP && item.opcode == OpCodes.Stsfld)
                {
                    FieldInfo SetField = item.operand as FieldInfo;
                    if (SetField != null && SetField.Name == "blockPlayerTeleportationForOneIteration")
                    {
                        injectedPostBlockTP = true;
                        inject = true;
                    }
                }

                if (item.opcode == OpCodes.Brfalse || item.opcode == OpCodes.Brfalse_S)
                {
                    prevJumps.Push((Label)item.operand);
                }

                //ErrorLogger.Log(item);
                yield return item;

                
            }
        }
    }
}
