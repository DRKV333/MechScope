using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MechScope.UI
{
    class UIToggle : UIText
    {
        Func<bool> get;
        Action set;

        public UIToggle(string text, Func<bool> get, Action set) : base(text, 1, false)
        {
            this.get = get;
            this.set = set;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (get())
            {
                TextColor = Color.Green;
            }
            else
            {
                TextColor = Color.Red;
            }
            base.DrawSelf(spriteBatch);
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);
            set();
        }
    }
}
