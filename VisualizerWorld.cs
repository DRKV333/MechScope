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

        private static List<Point16> StartHighlight = new List<Point16>();
        private static Dictionary<Point16, wireSegment> WireHighlight = new Dictionary<Point16, wireSegment>();
        private static Point16 PointHighlight = Point16.Zero;

        private Texture2D pixel;

        public override void Initialize()
        {
            pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });
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
                }

                Main.spriteBatch.End();
            }
        }

        private void DrawSimpleHeighlights()
        {
            foreach (var item in StartHighlight)
            {
                DrawRectFast(item.X * 16, item.Y * 16, 16, 16, Color.Red);
            }
            if (SuspendableWireManager.Mode == SuspendableWireManager.SuspendMode.perSingle)
                DrawRectFast(PointHighlight.X * 16, PointHighlight.Y * 16, 16, 16, Color.Red);
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
                DrawRectFast(item.Key.X * 16, item.Key.Y * 16, 16, 16, Color.White);

                int startY = 2;
                int height = 14 / item.Value.numWires();

                if (item.Value.red)
                {
                    DrawFilledRectFast(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, height + 1, 16, new Color(255, 0, 0, 128));
                    startY += height;
                }
                if (item.Value.blue)
                {
                    DrawFilledRectFast(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, height + 1, 16, new Color(0, 0, 255, 128));
                    startY += height;
                }
                if (item.Value.green)
                {
                    DrawFilledRectFast(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, height + 1, 16, new Color(0, 255, 0, 128));
                    startY += height;
                }
                if (item.Value.yellow)
                {
                    DrawFilledRectFast(item.Key.X * 16 + 2, item.Key.Y * 16 + startY, height + 1, 16, new Color(128, 128, 0, 128));
                    startY += height;
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }

        private void DrawRectFast(int left, int top, int height, int width, Color color)
        {
            left -= (int)Main.screenPosition.X;
            top -= (int)Main.screenPosition.Y;

            if (Main.LocalPlayer.gravDir == -1)
                top = Main.screenHeight - top - height;

            Main.spriteBatch.Draw(pixel, new Rectangle(left, top, width, 2), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(left, top + height, width, 2), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(left, top, 2, height), null, color);
            Main.spriteBatch.Draw(pixel, new Rectangle(left + width, top, 2, height), null, color);
        }

        private void DrawFilledRectFast(int left, int top, int height, int width, Color color)
        {
            left -= (int)Main.screenPosition.X;
            top -= (int)Main.screenPosition.Y;

            if (Main.LocalPlayer.gravDir == -1)
                top = Main.screenHeight - top - height;

            Main.spriteBatch.Draw(pixel, new Rectangle(left, top, width, height), null, color);
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
