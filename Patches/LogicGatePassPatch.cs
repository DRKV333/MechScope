using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace MechScope.Patches
{
    internal class LogicGatePassPatch
    {
        public static void Load()
        {
            IL_Wiring.LogicGatePass += Transpiler;
        }

        private static void Transpiler(ILContext context)
        {
            InjectedPostBranch(context);
            InjectedPreClearGatesDone(context);
        }

        //We want to inject after the end of the first loop containing CheckLogicGate.
        private static void InjectedPostBranch(ILContext context)
        {
            ILCursor cursor = new ILCursor(context).Goto(0);

            cursor.GotoNext(MoveType.After, x => x.MatchBgt(out ILLabel label) && label.Target.OpCode == OpCodes.Ldsfld);
            SuspendableWireManager.MakeSuspendSnippet(cursor, SuspendableWireManager.SuspendMode.perStage);
        }

        //We also need to put one on the end before _GatesDone.Clear()
        private static void InjectedPreClearGatesDone(ILContext context)
        {
            ILCursor cursor = new ILCursor(context).Goto(0);

            cursor.Goto(cursor.Instrs.Count - 1);
            cursor.GotoPrev(MoveType.Before, x => x.MatchLdsfld("Terraria.Wiring", "_GatesDone"));
            SuspendableWireManager.MakeSuspendSnippet(cursor, SuspendableWireManager.SuspendMode.perStage);
        }
    }
}