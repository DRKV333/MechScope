using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;

namespace MechScope.UI
{
    class UIIntBox : UITextPanel<string>
    {
        private string text;

        private bool hasFocus = false;
        private int maxDigits;

        Action<int> set;
        Func<int> get;

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
                    text = newText;
                    SetText(text + "|");
                    return;
                }

                int number;
                if (int.TryParse(newText, out number))
                {
                    text = newText;
                    SetText(text + "|");
                    set(number);
                }
            }
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            hasFocus = true;
            BackgroundColor = Color.Yellow;
            SetText(text + "|");
        }


    }
}
