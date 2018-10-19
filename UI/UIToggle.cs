using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace MechScope.UI
{
    internal class UIToggle : UIText
    {
        private readonly Func<bool> get;
        private readonly Action set;

        public UIToggle(string text, Func<bool> get, Action set) : base(text, 1, false)
        {
            this.get = get;
            this.set = set;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (get())
                TextColor = Color.Green;
            else
                TextColor = Color.DarkRed;

            if (IsMouseHovering)
                TextColor *= 2;

            base.DrawSelf(spriteBatch);
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);
            set();

            base.MouseDown(evt);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);

            base.MouseOver(evt);
        }
    }
}