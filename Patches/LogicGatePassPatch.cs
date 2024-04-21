using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Terraria;

namespace MechScope.Patches
{
    [HarmonyPatch(typeof(Wiring), "LogicGatePass")]
    [HarmonyPriority(Priority.Normal)]
    internal class LogicGatePassPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> original)
        {
            bool inject = false;
            bool injectedPostBranch = false;
            bool injectedPreClearGatesDone = false;

            int GatesDoneCount = 0;

            Stack<Label> prevJumps = new Stack<Label>();

            foreach (var item in original)
            {
                if (inject)
                {
                    inject = false;

                    CodeInstruction[] inst = SuspendableWireManager.MakeSuspendSnippet(generator, SuspendableWireManager.SuspendMode.perStage);

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

                //We also need to put one on the end before IL_00D7
                if (!injectedPreClearGatesDone && item.opcode == OpCodes.Ldsfld)
                {
                    FieldInfo SetField = item.operand as FieldInfo;
                    if (SetField != null && SetField.Name == "_GatesDone")
                    {
                        if (++GatesDoneCount == 4)
                        {
                            injectedPreClearGatesDone = true;
                            inject = true;
                        }
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