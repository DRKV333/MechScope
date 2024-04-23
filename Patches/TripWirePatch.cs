using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace MechScope.Patches
{
    internal static class TripWirePatch
    {
        public static void Load()
        {
            On_Wiring.TripWire += Prefix;
            IL_Wiring.TripWire += Transpiler;
        }

        private static void Prefix(On_Wiring.orig_TripWire orig, int left, int top, int width, int height)
        {
            if (SuspendableWireManager.BeginTripWire(left, top, width, height))
                orig(left, top, width, height);
        }

        private static void Transpiler(ILContext context)
        {
            InjectGrabTeleporterArray(context);
            InjectPostWire(context);
        }

        //Before assigning the local that stores the teleporters queued for triggering, we want to exfiltrate it to the visualizer.
        private static void InjectGrabTeleporterArray(ILContext context)
        {
            ILCursor cursor = new ILCursor(context).Goto(0);

            cursor.GotoNext(MoveType.Before, x => x.MatchStloc0());

            cursor.EmitDup();
            cursor.EmitCall(typeof(VisualizerWorld).GetMethod("ReportTeleporterArray"));
        }

        //Inject code after call to XferWater.
        private static void InjectPostWire(ILContext context)
        {
            ILCursor cursor = new ILCursor(context).Goto(0);

            while (cursor.TryGotoNext(MoveType.After, x => x.MatchCall("Terraria.Wiring", "XferWater")))
            {
                ILLabel beforeSuspend = cursor.MarkLabel();

                SuspendableWireManager.MakeSuspendSnippet(cursor, SuspendableWireManager.SuspendMode.perWire);

                //We want to be inside of the Wiring._wireList.Count > 0 branch, but outside of the XferWater branch
                Instruction currentTarget = cursor.Next;

                for (int i = 0; i < 2; i++)
                {
                    cursor.GotoPrev(MoveType.Before, x => x.MatchBle(currentTarget));
                    cursor.Next.Operand = beforeSuspend;
                }

                cursor.Goto(currentTarget, MoveType.Before);
            }

            cursor.MoveAfterLabels();
            SuspendableWireManager.MakeSuspendSnippet(cursor, SuspendableWireManager.SuspendMode.perSource);
        }
    }
}