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
        struct wireSegment
        {
            public Point16 point;
            public int color;
        }

        private const int maxWireVisual = 5000;

        private static Point16 StartHighlight = Point16.Zero;
        private static List<wireSegment> WireHighlight = new List<wireSegment>();
        private static Point16 PointHighlight = Point16.Zero;

        private Texture2D pixel;

        public override void Initialize()
        {
            pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin();

            if(SuspendableWireManager.Active)
            {
                Color indicatorColor = Color.Yellow;

                if (SuspendableWireManager.Running)
                    indicatorColor = Color.Red;

                Main.spriteBatch.Draw(pixel, new Rectangle(Main.mouseX + 20, Main.mouseY - 20, 10, 10), indicatorColor);

                if (AutoStepWorld.Active)
                    Main.spriteBatch.Draw(pixel, new Rectangle(Main.mouseX + 30, Main.mouseY - 20, 10, 10), Color.Green);

                if(SuspendableWireManager.Running)
                {
                    

                    foreach (var item in WireHighlight)
                    {
                        Color wireColor;
                        switch (item.color)
                        {
                            case 1: wireColor = Color.Red; break;
                            case 2: wireColor = Color.Blue; break;
                            case 3: wireColor = Color.Green; break;
                            case 4: wireColor = Color.Yellow; break;
                            default: wireColor = Color.Purple; break;
                        }
                        wireColor.A = 32;

                        DrawFilledRectFast(item.point.X * 16, item.point.Y * 16, 16, 16, wireColor);

                        DrawRectFast(StartHighlight.X * 16, StartHighlight.Y * 16, 16, 16, Color.White);
                        if (SuspendableWireManager.Mode == SuspendableWireManager.SuspendMode.perSingle)
                            DrawRectFast(PointHighlight.X * 16, PointHighlight.Y * 16, 16, 16, Color.Red);
                    }
                }
            }

            Main.spriteBatch.End();
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
        }

        public static void AddWireSegment(Point16 point, int color)
        {
            if (WireHighlight.Count < maxWireVisual)
                WireHighlight.Add(new wireSegment() { point = point, color = color });
            PointHighlight = point;
        }

        public static void MarkStart(Point16 point)
        {
            StartHighlight = point;
        }
    }
}
