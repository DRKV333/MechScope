using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.Utilities;

namespace MechScope
{
    internal static class SuspendableWireManager
    {
        public enum SuspendMode
        {
            perSingle = 0,
            perWire = 1,
            perSource = 2,
            perStage = 3
        }

        private const string wireThreadName = "wireThread";
        private const int maxQueuedTrips = 100;

        public static bool Running { get; private set; }
        public static bool Active { get { return active; } set { active = value; if (!value) Resume(); } }
        public static SuspendMode Mode = SuspendMode.perStage;
        public static int QueuedNum { get { return queuedWireTrips.Count; } }

        private static bool active = false;
        private static Queue<Rectangle> queuedWireTrips = new Queue<Rectangle>();
        private static bool runnigBackup = false; //Wiring.running should be the same as Running in theory, but it isn't. (probably a bug)
        private static AutoResetEvent wiringWait = new AutoResetEvent(false);
        private static AutoResetEvent mainWait = new AutoResetEvent(false);

        static SuspendableWireManager()
        {
            Running = false;
        }

        public static bool IsWireThread => Thread.CurrentThread.Name == wireThreadName;

        public static bool BeginTripWire(int left, int top, int width, int height)
        {
            if (!Active || IsWireThread)
            {
                VisualizerWorld.AddStart(new Rectangle(left, top, width, height));
                return true;
            }

            if (Running)
            {
                if (queuedWireTrips.Count < maxQueuedTrips)
                    queuedWireTrips.Enqueue(new Rectangle(left, top, width, height));
                return false;
            }

            VisualizerWorld.ResetSegments();

            Running = true;

            Thread wireThread = new Thread(delegate (object rand)
            {
                Main.rand = (UnifiedRandom)rand; //Main.rand is ThreadStatic, but we need it for faulty gates
                Wiring.TripWire(left, top, width, height);
                Running = false;
                mainWait.Set();
            });

            wireThread.IsBackground = true;
            wireThread.Name = wireThreadName;
            wireThread.Start(Main.rand);

            mainWait.WaitOne();

            return false;
        }

        public static void Resume()
        {
            if (!Running)
                return;

            Wiring.running = runnigBackup;

            wiringWait.Set();
            mainWait.WaitOne();

            while (!Running && queuedWireTrips.Count > 0)
            {
                Rectangle trip = queuedWireTrips.Dequeue();
                BeginTripWire(trip.X, trip.Y, trip.Width, trip.Height);
            }
        }

        public static void SuspendWire()
        {
            if (Running && active)
            {
                runnigBackup = Wiring.running;
                Wiring.running = false;
                VisualizerWorld.BuildMarkerCache();
                AutoStepWorld.ResetTimer();
                mainWait.Set();
                wiringWait.WaitOne();
            }
        }

        public static void MakeSuspendSnippet(ILCursor cursor, SuspendMode mode)
        {
            /* (Stuff in [] is dependent on mode)
             *
             * [ldloc.2]
             * [ldarg.1]
             * [call void VisualizerWorld::AddWireSegment(Point16,int)]
             *
             * [ldsfld SuspendMode  SuspendableWireManager::Mode]
             * [ldc.i4.0] //SuspenMode.perSingle
             * [beq single]
             * [ldsfld SuspendMode  SuspendableWireManager::Mode]
             * [ldc.i4.1] //SuspenMode.perWire
             * [beq single]
             *
             * ldsfld SuspendMode  SuspendableWireManager::Mode
             * ldc.i4 <mode>
             * bne.un skip
             * call void SuspendableWireManager::SuspendWire()
             * [single:] [call void VisualizerWorld::ResetSegments()]
             *
             * skip: nop
             */

            ILLabel labelSkip = cursor.DefineLabel();
            ILLabel labelSingle = null;

            if (mode == SuspendMode.perSingle)
            {
                //In HitWireSingle we want to exfiltrate the call arguments, so we can visualize the wire we hit.
                cursor.EmitLdloc2();
                cursor.EmitLdarg1();
                cursor.EmitCall(typeof(VisualizerWorld).GetMethod("AddWireSegment"));
            }
            else if (mode == SuspendMode.perSource)
            {
                //When in perSingle or perWire mode, we still want to clear segments after each source, even though we don't pause there.
                labelSingle = cursor.DefineLabel();

                cursor.EmitLdsfld(typeof(SuspendableWireManager).GetField("Mode"));
                cursor.EmitLdcI4(0);
                cursor.EmitBeq(labelSingle);

                cursor.EmitLdsfld(typeof(SuspendableWireManager).GetField("Mode"));
                cursor.EmitLdcI4(1);
                cursor.EmitBeq(labelSingle);
            }

            cursor.EmitLdsfld(typeof(SuspendableWireManager).GetField("Mode"));
            cursor.EmitLdcI4((int)mode);
            cursor.EmitBneUn(labelSkip);
            cursor.EmitCall(typeof(SuspendableWireManager).GetMethod("SuspendWire"));

            if (mode == SuspendMode.perSource || mode == SuspendMode.perStage)
            {
                //We want to clear visuals after we are done with a source or stage
                if (labelSingle != null)
                    cursor.MarkLabel(labelSingle);
                cursor.EmitCall(typeof(VisualizerWorld).GetMethod("ResetSegments"));
            }

            cursor.MarkLabel(labelSkip);
            cursor.EmitNop();
        }
    }
}