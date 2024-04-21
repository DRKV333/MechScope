using HarmonyLib;
using MechScope.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MechScope
{
    public class MechScope : Mod
    {
        public static ModKeybind keyToggle;
        public static ModKeybind keyStep;
        public static ModKeybind keyAutoStep;
        public static ModKeybind keySettings;
        public static SettingsUI settingsUI;
        public static LegacyGameInterfaceLayer UILayer;
        public static Harmony harmony;

        private static UserInterface userInterface;

        public MechScope()
        {
            //Properties = new ModProperties()
            //{
            //    Autoload = true,
            //    AutoloadBackgrounds = true,
            //    AutoloadGores = true,
            //    AutoloadSounds = true,
            //};
        }

        public override void Load()
        {
            if (harmony == null)
                harmony = new Harmony(Name);

            harmony.PatchAll();

            keyToggle = KeybindLoader.RegisterKeybind(this, "Toggle", "NumPad1");
            keyStep = KeybindLoader.RegisterKeybind(this, "Step", "NumPad2");
            keyAutoStep = KeybindLoader.RegisterKeybind(this, "Auto step", "NumPad3");
            keySettings = KeybindLoader.RegisterKeybind(this, "Settings", "NumPad5");

            if (!Main.dedServ)
            {
                settingsUI = new SettingsUI();
                userInterface = new UserInterface();
                userInterface.SetState(settingsUI);
                settingsUI.Activate();
                UILayer = new LegacyGameInterfaceLayer("MechScope: Settings menu",
                    delegate
                    {
                        if (settingsUI.Visible)
                        {
                            settingsUI.Draw(Main.spriteBatch);
                            userInterface.Update(Main._drawInterfaceGameTime);
                        }
                        return true;
                    }
                );
            }
        }

        public override void Unload()
        {
            harmony.UnpatchAll();

            keyToggle = null;
            keyStep = null;
            keyAutoStep = null;
            keySettings = null;
            settingsUI = null;
            userInterface = null;
            UILayer = null;
        }
    }
    public class MechScopeModSystem : ModSystem
    {
        public override void PreSaveAndQuit()
        {
            SuspendableWireManager.Active = false;
            MechScope.settingsUI.Visible = false;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(x => x.Name == "Vanilla: Inventory");
            layers.Insert(index + 1, MechScope.UILayer);
        }
    }
}