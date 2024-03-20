using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using BepInEx;
using System.IO;

namespace LiveSplitHelper
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class LiveSplitHelper : BaseUnityPlugin
    {
        public const string pluginGuid = "2fd308b761844bd6bcc61cfa3ae1a70c";
        public const string pluginName = "LiveSplitHelper";
        public const string pluginVersion = "1.0";

        public static BepInEx.Logging.ManualLogSource pLogger;
        Harmony harmony = new Harmony(pluginGuid);

        List<MethodInfo> original = new List<MethodInfo>();
        List<MethodInfo> patch = new List<MethodInfo>();

        private static readonly byte[] signature = { 0x85, 0x1E, 0xA7, 0x85, 0xC5, 0x33, 0xA3, 0xAF, 0x50, 0xBC };
        public static string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // ---------------------------------------

        public static bool leftHandGrabbed = false;
        public static bool rightHandGrabbed = false;
        public static float[] position = { 0, 0 };

        // ---------------------------------------

        private void Awake()
        {
            // static logger
            pLogger = Logger;

            // ArmScript_v2 -> Update()
            original.Add(AccessTools.Method(typeof(ArmScript_v2), "Update"));
            patch.Add(AccessTools.Method(typeof(ArmScript_v2_Update), "Prefix"));

            // ClimberMain -> Update()
            original.Add(AccessTools.Method(typeof(ClimberMain), "Update"));
            patch.Add(AccessTools.Method(typeof(ClimberMain_Update), "Prefix"));

            // patch all
            for (int i = 0; i < original.Count; i++)
            {
                if (patch[i].Name == "Prefix") harmony.Patch(original[i], new HarmonyMethod(patch[i]));
                else if (patch[i].Name == "Postfix") harmony.Patch(original[i], null, new HarmonyMethod(patch[i]));
            }
        }

        // ---------------------------------------

        // HANDS
        public static class ArmScript_v2_Update
        {
            public static bool Prefix(ArmScript_v2 __instance)
            {
                if (__instance.isLeft)
                {
                    if (__instance.grabbedSurface != null) leftHandGrabbed = true;
                    else leftHandGrabbed = false;
                }
                else
                {
                    if (__instance.grabbedSurface != null) rightHandGrabbed = true;
                    else rightHandGrabbed = false;
                }

                return true;
            }
        }

        // POSITION
        public static class ClimberMain_Update
        {
            public static bool Prefix(ClimberMain __instance)
            {
                position[0] = __instance.body.transform.position.x;
                position[1] = __instance.body.transform.position.y;
                return true;
            }
        }
    }
}