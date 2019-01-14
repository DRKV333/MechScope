using Harmony;
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
        public static ModHotKey keyToggle;
        public static ModHotKey keyStep;
        public static ModHotKey keyAutoStep;
        public static ModHotKey keySettings;
        public static SettingsUI settingsUI;

        private static HarmonyInstance harmonyInstance;
        private static UserInterface userInterface;
        private static LegacyGameInterfaceLayer UILayer;

        public MechScope()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadBackgrounds = true,
                AutoloadGores = true,
                AutoloadSounds = true,
            };
        }

        public override void Load()
        {
            if (harmonyInstance == null)
                harmonyInstance = HarmonyInstance.Create(Name);

            harmonyInstance.PatchAll();

            keyToggle = RegisterHotKey("Toggle", "NumPad1");
            keyStep = RegisterHotKey("Step", "NumPad2");
            keyAutoStep = RegisterHotKey("Auto step", "NumPad3");
            keySettings = RegisterHotKey("Settings", "NumPad5");

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

        public override void PreSaveAndQuit()
        {
            SuspendableWireManager.Active = false;
            settingsUI.Visible = false;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(x => x.Name == "Vanilla: Inventory");
            layers.Insert(index + 1, UILayer);
        }

        public override void Unload()
        {
            harmonyInstance.UnpatchAll();

            keyToggle = null;
            keyStep = null;
            keyAutoStep = null;
            keySettings = null;
            settingsUI = null;
            userInterface = null;
            UILayer = null;
        }
    }
}