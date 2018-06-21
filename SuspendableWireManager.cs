using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace MechScope
{
    static class SuspendableWireManager
    {
        public enum SuspendMode
        {
            perSingle = 0,
            perWire = 1,
            perSource = 2,
            perStage = 3
        }

        struct QueuedWireTrip
        {
            public int left;
            public int top;
            public int width;
            public int height;
        }

        private const string wireThreadName = "wireThread";
        private const int maxQueuedTrips = 100;

        public static bool Running { get; private set; }
        public static bool Active { get { return active; } set { active = value; if(!value) Resume(); } }
        public static SuspendMode Mode = SuspendMode.perStage;

        private static bool active = false; 
        private static Queue<QueuedWireTrip> queuedWireTrips = new Queue<QueuedWireTrip>();
        private static Thread wireThread;
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
                VisualizerWorld.AddStart(new Point16(left, top));
                return true;
            }

            if (Running)
            {
                if(queuedWireTrips.Count < maxQueuedTrips)
                    queuedWireTrips.Enqueue(new QueuedWireTrip() { left = left, top = top, height = height, width = width });
                return false;
            }

            VisualizerWorld.ResetSegments();

            Running = true;

            wireThread = new Thread(delegate (object rand) {
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

            if(!Running && queuedWireTrips.Count > 0)
            {
                QueuedWireTrip trip = queuedWireTrips.Dequeue();
                BeginTripWire(trip.left, trip.top, trip.width, trip.height);
            }
        }

        public static void SuspendWire()
        {
            if (Running && active)
            {
                runnigBackup = Wiring.running;
                Wiring.running = false;
                VisualizerWorld.BuildMarkerCache();
                mainWait.Set();
                wiringWait.WaitOne();
            }
        }

        public static CodeInstruction[] MakeSuspendSnippet(ILGenerator generator, SuspendMode mode)
        {
            /*
             * [ldloc.2]
             * [ldarg.1]
             * [call void VisualizerWorld::AddWireSegment(Point16,int)]
             * 
             * [ldsfld SuspendMode  SuspendableWireManager::Mode]
             * [ldc.i4.0] //SuspenMode.perSingle
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
            else
                len += 1;

            if (mode == SuspendMode.perWire)
                len += 3;

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
            if(mode == SuspendMode.perWire)
            {
                //When in perSingle mode, we still want to clear segments after each wire, even though we don't pause there.
                inst[i++] = new CodeInstruction(OpCodes.Ldsfld, typeof(SuspendableWireManager).GetField("Mode"));
                inst[i++] = new CodeInstruction(OpCodes.Ldc_I4_0);
                labelWire = generator.DefineLabel();
                inst[i++] = new CodeInstruction(OpCodes.Beq, labelWire);
            }
            inst[i++] = new CodeInstruction(OpCodes.Ldsfld, typeof(SuspendableWireManager).GetField("Mode"));
            inst[i++] = new CodeInstruction(OpCodes.Ldc_I4, (Int32)mode);
            inst[i++] = new CodeInstruction(OpCodes.Bne_Un, labelSkip);
            inst[i++] = new CodeInstruction(OpCodes.Call, typeof(SuspendableWireManager).GetMethod("SuspendWire"));
            if(mode != SuspendMode.perSingle)
            {
                //We don't want to clear wire segments in HitWireSingle
                inst[i] = new CodeInstruction(OpCodes.Call, typeof(VisualizerWorld).GetMethod("ResetSegments"));
                if(mode == SuspendMode.perWire)
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
