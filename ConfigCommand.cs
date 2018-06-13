using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace MechScope
{
    class ConfigCommand : ModCommand
    {
        public override string Command { get { return "msconfig"; } }

        public override CommandType Type { get { return CommandType.World; } }

        public override string Usage { get { return "/msconfig [setting]\n/msconfig [setting] [value]\nPossible settings: arate, smode\nValid values for smode: single, wire, source, stage"; } }

        public override string Description { get { return "Sets various settings for MechScope"; } }

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1)
            {
                Main.NewText("see '/help msconfig'");
                return;
            }

            if (args.Length == 1)
            {
                string val;
                switch (args[0])
                {
                    case "arate": val = AutoStepWorld.Rate.ToString(); break;
                    case "smode": val = SuspendableWireManager.Mode.ToString(); break;
                    default: val = "not a valid setting (see '/help msconfig')"; break;
                }
                Main.NewText(args[0] + " is " + val);
            }
            else
            {
                switch (args[0])
                {
                    case "arate":
                        int newRate;
                        if (int.TryParse(args[1], out newRate) && newRate > 0)
                        {
                            AutoStepWorld.Rate = newRate;
                            Main.NewText(string.Format("Auto step now happens every {0} frames.", newRate));
                        }
                        else
                        {
                            Main.NewText(args[1] + " is not a valid number");
                        }
                        break;

                    case "smode":
                        switch (args[1])
                        {
                            case "single":
                                SuspendableWireManager.Mode = SuspendableWireManager.SuspendMode.perSingle;
                                Main.NewText("Now suspending after every wire search step");
                                break;
                            case "wire":
                                SuspendableWireManager.Mode = SuspendableWireManager.SuspendMode.perWire;
                                Main.NewText("Now suspending after each wire");
                                break;
                            case "source":
                                SuspendableWireManager.Mode = SuspendableWireManager.SuspendMode.perSource;
                                Main.NewText("Now suspending after every activation source");
                                break;
                            case "stage":
                                SuspendableWireManager.Mode = SuspendableWireManager.SuspendMode.perStage;
                                Main.NewText("Now suspending after every logic gate execution step");
                                break;
                            default:
                                Main.NewText("Valid modes: single, wire, gate, step");
                                break;
                        }
                        break;
                        
                    default:
                        Main.NewText(args[1] + " is not a valid setting (see '/help msconfig')");
                        break;
                }
            }

        }
    }
}
