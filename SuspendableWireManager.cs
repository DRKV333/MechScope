using HarmonyLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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

        public static bool BeginTripWire(int left, int top, int width, int height)
        {
            if (!Active || Thread.CurrentThread.Name == wireThreadName)
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

        public static CodeInstruction[] MakeSuspendSnippet(ILGenerator generator, SuspendMode mode)
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

            int len = 5;
            if (mode == SuspendMode.perSingle)
                len += 3;

            if (mode == SuspendMode.perSource || mode == SuspendMode.perStage)
                len += 1;

            if (mode == SuspendMode.perSource)
                len += 6;

            int i = 0;

            CodeInstruction[] inst = new CodeInstruction[len];

            Label labelSkip = generator.DefineLabel();
            Label labelWire = default(Label);

            if (mode == SuspendMode.perSingle)
            {
                //In HitWireSingle we want to exfiltrate the call arguments, so we can visualize the wire we hit.
                inst[i++] = new CodeInstruction(OpCodes.Ldloc_2);
                inst[i++] = new CodeInstruction(OpCodes.Ldarg_1);
                inst[i++] = new CodeInstruction(OpCodes.Call, typeof(VisualizerWorld).GetMethod("AddWireSegment"));
            }
            if (mode == SuspendMode.perSource)
            {
                //When in perSingle and perWire mode, we still want to clear segments after each source, even though we don't pause there.
                inst[i++] = new CodeInstruction(OpCodes.Ldsfld, typeof(SuspendableWireManager).GetField("Mode"));
                inst[i++] = new CodeInstruction(OpCodes.Ldc_I4_0);
                labelWire = generator.DefineLabel();
                inst[i++] = new CodeInstruction(OpCodes.Beq, labelWire);
                inst[i++] = new CodeInstruction(OpCodes.Ldsfld, typeof(SuspendableWireManager).GetField("Mode"));
                inst[i++] = new CodeInstruction(OpCodes.Ldc_I4_1);
                inst[i++] = new CodeInstruction(OpCodes.Beq, labelWire);
            }
            inst[i++] = new CodeInstruction(OpCodes.Ldsfld, typeof(SuspendableWireManager).GetField("Mode"));
            inst[i++] = new CodeInstruction(OpCodes.Ldc_I4, (Int32)mode);
            inst[i++] = new CodeInstruction(OpCodes.Bne_Un, labelSkip);
            inst[i++] = new CodeInstruction(OpCodes.Call, typeof(SuspendableWireManager).GetMethod("SuspendWire"));
            if (mode == SuspendMode.perSource || mode == SuspendMode.perStage)
            {
                //We want to clear visuals after we are done with a source or stage
                inst[i] = new CodeInstruction(OpCodes.Call, typeof(VisualizerWorld).GetMethod("ResetSegments"));
                if (mode == SuspendMode.perSource)
                {
                    inst[i].labels.Add(labelWire);
                }
                i++;
            }
            inst[i] = new CodeInstruction(OpCodes.Nop);
            inst[i].labels.Add(labelSkip);

            return inst;
        }
    }
}