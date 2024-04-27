using MonoMod.Cil;
using Terraria;

namespace MechScope.Patches
{
    internal class HitWirePatch
    {
        public static void Load()
        {
            IL_Wiring.HitWire += Transpiler;
        }

        private static void Transpiler(ILContext context)
        {
            ILCursor cursor = new ILCursor(context).Goto(0);

            //Inject code after call to HitWireSingle.
            cursor.GotoNext(MoveType.After, x => x.MatchCall("Terraria.Wiring", "HitWireSingle"));

            //The end of a branch is in front, but we want to be outside if it.
            cursor.MoveAfterLabels();

            SuspendableWireManager.MakeSuspendSnippet(cursor, SuspendableWireManager.SuspendMode.perSingle);
        }
    }
}