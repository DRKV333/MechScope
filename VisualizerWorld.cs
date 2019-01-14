using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MechScope
{
    internal class VisualizerWorld : ModWorld
    {
        private class WireSegment
        {
            public bool red;
            public bool blue;
            public bool green;
            public bool yellow;

            public int numWires()
            {
                int num = 0;
                if (red) num++;
                if (blue) num++;
                if (green) num++;
                if (yellow) num++;
                return num;
            }
        }

        private struct ColoredMark
        {
            public string mark;
            public Color color;

            public ColoredMark(string mark, Color color)
            {
                this.mark = mark; this.color = color;
            }
        }

        private const int maxWireVisual = 5000;

        public static bool ShowWireSkip = false;
        public static bool ShowGatesDone = true;
        public static bool ShowUpcomingGates = true;
        public static bool ShowTriggeredLamps = false;
        public static bool ShowTeleporters = true;
        public static bool ShowPumps = true;

        private static readonly Color ColorWRed = new Color(255, 0, 0, 128);
        private static readonly Color ColorWBlue = new Color(0, 0, 255, 128);
        private static readonly Color ColorWGreen = new Color(0, 255, 0, 128);
        private static readonly Color ColorWYellow = new Color(255, 255, 0, 128);

        private static List<Rectangle> StartHighlight;
        private static Dictionary<Point16, WireSegment> WireHighlight;
        private static Point16 PointHighlight = Point16.Zero;
        private static Dictionary<Point16, ColoredMark> MarkCache;

        private static Texture2D pixel;

        private static Dictionary<Point16, bool> WiringGatesDone;
        private static Queue<Point16> WiringGatesCurrent;
        private static Queue<Point16> WiringGatesNext; //We need static references for both of these, because they get swapped around.
        private static Dictionary<Point16, bool> WiringWireSkip;
        private static Vector2[] WiringTeleporters;

        public override void Initialize()
        {
            if (!Main.dedServ)
            {
                pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }

            StartHighlight = new List<Rectangle>();
            WireHighlight = new Dictionary<Point16, WireSegment>();
            MarkCache = new Dictionary<Point16, ColoredMark>();

            WiringGatesDone = (Dictionary<Point16, bool>)typeof(Wiring).GetField("_GatesDone", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            WiringGatesCurrent = (Queue<Point16>)typeof(Wiring).GetField("_GatesCurrent", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            WiringGatesNext = Wiring._GatesNext;
            WiringWireSkip = (Dictionary<Point16, bool>)typeof(Wiring).GetField("_wireSkip", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            WiringTeleporters = new Vector2[8];
        }

        public override void PostDrawTiles()
        {
            if (SuspendableWireManager.Active)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                DrawIndicators();

                if (SuspendableWireManager.Running)
                {
                    DrawWireSegments();
                    DrawSimpleHeighlights();
                    DrawReflectionMarkers();
                }

                Main.spriteBatch.End();
            }
        }

        private void DrawReflectionMarkers()
        {
            foreach (var item in MarkCache)
            {
                DrawTileMarker(item.Key, item.Value);
            }
        }

        private void DrawSimpleHeighlights()
        {
            foreach (var item in StartHighlight)
            {
                DrawTileBorder(new Point16(item.Location), Color.Red, item.Width, item.Height);
            }
            if (SuspendableWireManager.Mode == SuspendableWireManager.SuspendMode.perSingle)
                DrawTileBorder(PointHighlight, Color.Red);
        }

        private void DrawIndicators()
        {
            Color indicatorColor = Color.Yellow;

            if (SuspendableWireManager.Running)
                indicatorColor = Color.Red;

            Main.spriteBatch.Draw(pixel, new Rectangle(Main.mouseX + 20, Main.mouseY - 20, 10, 10), indicatorColor);

            if (AutoStepWorld.Active)
                Main.spriteBatch.Draw(pixel, new Rectangle(Main.mouseX + 30, Main.mouseY - 20, 10, 10), Color.Green);
        }

        private void DrawWireSegments()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            foreach (var item in WireHighlight)
            {
                DrawTileBorder(item.Key, Color.White);

                int startY = 2;
                int height = (int)Math.Ceiling(14f / item.Value.numWires());

                if (item.Value.red)
                {
                    Main.spriteBatch.Draw(pixel, WorldRectToScreen(new Rectangle(item.Key.X * 16, item.Key.Y * 16 + startY, 16, height)), ColorWRed);
                    startY += height;
                }
                if (item.Value.blue)
                {
                    Main.spriteBatch.Draw(pixel, WorldRectToScreen(new Rectangle(item.Key.X * 16, item.Key.Y * 16 + startY, 16, height)), ColorWBlue);
                    startY += height;
                }
                if (item.Value.green)
                {
                    Main.spriteBatch.Draw(pixel, WorldRectToScreen(new Rectangle(item.Key.X * 16, item.Key.Y * 16 + startY, 16, height)), ColorWGreen);
                    startY += height;
                }
                if (item.Value.yellow)
                {
                    Main.spriteBatch.Draw(pixel, WorldRectToScreen(new Rectangle(item.Key.X * 16, item.Key.Y * 16 + startY, 16, height)), ColorWYellow);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }

        private void DrawTileMarker(Point16 tile, ColoredMark mark)
        {
            Vector2 text = Main.fontMouseText.MeasureString(mark.mark);
            Vector2 loc = new Vector2(tile.X * 16 - (int)Main.screenPosition.X + 8, tile.Y * 16 - (int)Main.screenPosition.Y + 12) - text / 2;

            if (Main.LocalPlayer.gravDir == -1)
                loc.Y = Main.screenHeight - loc.Y - 16;

            Main.spriteBatch.DrawString(Main.fontMouseText, mark.mark, loc, mark.color);
        }

        private void DrawTileBorder(Point16 tile, Color color, int width = 1, int height = 1)
        {
            Rectangle rect = WorldRectToScreen(new Rectangle(tile.X * 16, tile.Y * 16, width * 16, height * 16));

            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 2), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width, rect.Y, 2, rect.Height + 2), null, color);
        }

        private Rectangle WorldRectToScreen(Rectangle rect)
        {
            Rectangle newRect = new Rectangle(rect.X - (int)Main.screenPosition.X, rect.Y - (int)Main.screenPosition.Y, rect.Width, rect.Height);

            if (Main.LocalPlayer.gravDir == -1)
                newRect.Y = Main.screenHeight - newRect.Y - newRect.Height;

            return newRect;
        }

        public static void ResetSegments()
        {
            WireHighlight.Clear();
            StartHighlight.Clear();
        }

        public static void AddWireSegment(Point16 point, int color)
        {
            PointHighlight = point;

            WireSegment segment;
            if (!WireHighlight.TryGetValue(point, out segment))
            {
                if (WireHighlight.Count > maxWireVisual)
                    return;

                segment = new WireSegment();
                WireHighlight.Add(point, segment);
            }

            switch (color)
            {
                case 1: segment.red = true; break;
                case 2: segment.blue = true; break;
                case 3: segment.green = true; break;
                case 4: segment.yellow = true; break;
            }
        }

        public static void AddStart(Rectangle trip)
        {
            StartHighlight.Add(trip);
        }

        public static void ReportTeleporterArray(Vector2[] arr)
        {
            WiringTeleporters = arr;
        }

        public static void BuildMarkerCache()
        {
            MarkCache.Clear();

            if (ShowWireSkip)
            {
                foreach (var item in WiringWireSkip)
                {
                    if (item.Value)
                    {
                        MarkCache[item.Key] = new ColoredMark("X", Color.Red);
                    }
                }
            }

            if (ShowGatesDone)
            {
                foreach (var item in WiringGatesDone)
                {
                    if (item.Value)
                    {
                        MarkCache[item.Key] = new ColoredMark("X", Color.White);
                    }
                }
            }

            if (ShowUpcomingGates)
            {
                foreach (var item in WiringGatesCurrent)
                {
                    MarkCache[item] = new ColoredMark("O", Color.Red);
                }
                foreach (var item in WiringGatesNext)
                {
                    MarkCache[item] = new ColoredMark("O", Color.Red);
                }
            }

            if (ShowTriggeredLamps)
            {
                foreach (var item in Wiring._LampsToCheck)
                {
                    MarkCache[item] = new ColoredMark("?", Color.Orange);
                }
            }

            if (ShowTeleporters)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 v = WiringTeleporters[i];
                    if (v != null && v.X >= 0 && v.Y >= 0)
                    {
                        while (MarkCache.ContainsKey(v.ToPoint16()))
                            v.X++;

                        MarkCache[v.ToPoint16()] = new ColoredMark((i / 2 + 1).ToString(), Color.White);
                    }
                }
            }

            if (ShowPumps)
            {
                for (int i = 0; i < Wiring._numInPump; i++)
                {
                    Point16 point = new Point16(Wiring._inPumpX[i], Wiring._inPumpY[i]);
                    MarkCache[point] = new ColoredMark(i.ToString(), Color.Red);
                }
                for (int i = 0; i < Wiring._numOutPump; i++)
                {
                    Point16 point = new Point16(Wiring._outPumpX[i], Wiring._outPumpY[i]);
                    MarkCache[point] = new ColoredMark(i.ToString(), Color.Green);
                }
            }
        }

        public static void Unload()
        {
            pixel = null;

            StartHighlight = null;
            WireHighlight = null;
            MarkCache = null;

            WiringGatesDone = null;
            WiringGatesCurrent = null;
            WiringGatesNext = null;
            WiringWireSkip = null;
            WiringTeleporters = null;
        }
    }
}