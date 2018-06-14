using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using System.Reflection;

namespace MechScope
{
    class VisualizerWorld : ModWorld
    {
        class wireSegment
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

        private const int maxWireVisual = 5000;

        private static readonly Color ColorWRed = new Color(255, 0, 0, 128);
        private static readonly Color ColorWBlue = new Color(0, 0, 255, 128);
        private static readonly Color ColorWGreen = new Color(0, 255, 0, 128);
        private static readonly Color ColorWYellow = new Color(255, 255, 0, 128);



        private static List<Point16> StartHighlight = new List<Point16>();
        private static Dictionary<Point16, wireSegment> WireHighlight = new Dictionary<Point16, wireSegment>();
        private static Point16 PointHighlight = Point16.Zero;

        private Texture2D pixel;
        private Dictionary<Point16, bool> WiringGatesDone;

        public override void Initialize()
        {
            pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });

            WiringGatesDone = (Dictionary<Point16,bool>)typeof(Wiring).GetField("_GatesDone", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public override void PostDrawTiles()
        {
            

            if(SuspendableWireManager.Active)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                DrawIndicators();

                if (SuspendableWireManager.Running)
                {
                    DrawWireSegments();
                    DrawSimpleHeighlights();
                    DrawReflecionMarkers();
                }

                Main.spriteBatch.End();
            }
        }

        private void DrawReflecionMarkers()
        {
            foreach (var item in WiringGatesDone)
            {
                if(item.Value)
                {
                    Main.spriteBatch.Draw(mod.GetTexture("GateDone"), WorldRectToScreen(new Rectangle(item.Key.X * 16, item.Key.Y * 16, 16, 16)), Color.White);
                }
            }
        }

        private void DrawSimpleHeighlights()
        {
            foreach (var item in StartHighlight)
            {
                DrawRectFast(new Rectangle(item.X * 16, item.Y * 16, 16, 16), Color.Red);
            }
            if (SuspendableWireManager.Mode == SuspendableWireManager.SuspendMode.perSingle)
                DrawRectFast(new Rectangle(PointHighlight.X * 16, PointHighlight.Y * 16, 16, 16), Color.Red);
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
                DrawRectFast(new Rectangle(item.Key.X * 16, item.Key.Y * 16, 16, 16), Color.White);

                int startY = 2;
                int height = 14 / item.Value.numWires();

                if (item.Value.red)
                {
                    DrawFilledRectFast(new Rectangle(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, 16, height + 1), ColorWRed);
                    startY += height;
                }
                if (item.Value.blue)
                {
                    DrawFilledRectFast(new Rectangle(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, 16, height + 1), ColorWBlue);
                    startY += height;
                }
                if (item.Value.green)
                {
                    DrawFilledRectFast(new Rectangle(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, 16, height + 1), ColorWGreen);
                    startY += height;
                }
                if (item.Value.yellow)
                {
                    DrawFilledRectFast(new Rectangle(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, 16, height + 1), ColorWYellow);
                    startY += height;
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }

        private void DrawRectFast(Rectangle rect, Color color)
        {
            rect = WorldRectToScreen(rect);

            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, 2), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width, rect.Y, 2, rect.Height + 2), null, color);
        }

        private void DrawFilledRectFast(Rectangle rect, Color color)
        {
            Main.spriteBatch.Draw(pixel, WorldRectToScreen(rect), null, color);
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

            wireSegment segment;
            if (!WireHighlight.TryGetValue(point, out segment))
            {
                if (WireHighlight.Count > maxWireVisual)
                    return;

                segment = new wireSegment();
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

        public static void AddStart(Point16 point)
        {
            StartHighlight.Add(point);
        }
    }
}
