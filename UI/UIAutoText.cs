using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework.Graphics;

namespace MechScope.UI
{
    class UIAutoText : UIText
    {
        Func<string> text;

        public UIAutoText(Func<string> text) : base(text(), 1, false)
        {
            this.text = text;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            SetText(text());
            base.DrawSelf(spriteBatch);
        }
    }
}
