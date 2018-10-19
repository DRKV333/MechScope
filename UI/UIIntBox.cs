using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace MechScope.UI
{
    internal class UIIntBox : UITextPanel<string>
    {
        private string text;

        private bool hasFocus = false;
        private readonly int maxDigits;

        private readonly Action<int> set;
        private readonly Func<int> get;

        public UIIntBox(Func<int> get, Action<int> set, int maxDigits) : base("", 1, false)
        {
            this.get = get;
            this.set = set;
            this.maxDigits = maxDigits;
        }

        public override void OnActivate()
        {
            BackgroundColor = Color.White;
            text = get().ToString();
            SetText(text);

            base.OnActivate();
        }

        public override void Update(GameTime gameTime)
        {
            if (hasFocus)
            {
                Main.hasFocus = true;
                Main.chatRelease = false;
                PlayerInput.WritingText = true;
                Main.instance.HandleIME();
                string newText = Main.GetInputText(text).Trim();

                if ((Main.mouseLeft && !IsMouseHovering) || Main.inputTextEnter || Main.inputTextEscape)
                {
                    hasFocus = false;
                    BackgroundColor = Color.White;
                    SetText(text);
                    return;
                }

                if (newText == text || newText.Length > maxDigits)
                    return;

                if (newText.Length == 0)
                {
                    Main.PlaySound(SoundID.MenuTick);
                    text = newText;
                    SetText(text + "|");
                    return;
                }

                int number;
                if (int.TryParse(newText, out number))
                {
                    Main.PlaySound(SoundID.MenuTick);
                    text = newText;
                    SetText(text + "|");
                    set(number);
                }
            }

            base.Update(gameTime);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (!hasFocus && !Main.drawingPlayerChat && IsMouseHovering)
                BorderColor = Color.CornflowerBlue;
            else
                BorderColor = Color.Black;

            base.DrawSelf(spriteBatch);
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            if (!Main.drawingPlayerChat)
            {
                Main.PlaySound(SoundID.MenuTick);
                hasFocus = true;
                BackgroundColor = Color.Yellow;
                SetText(text + "|");
            }
            base.MouseDown(evt);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            if (!Main.drawingPlayerChat && !hasFocus)
                Main.PlaySound(SoundID.MenuTick);

            base.MouseOver(evt);
        }
    }
}