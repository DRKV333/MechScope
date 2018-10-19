using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;

namespace MechScope.UI
{
    internal class UIAutoText : UIText
    {
        private Func<string> text;

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