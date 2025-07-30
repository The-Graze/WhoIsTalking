using HarmonyLib;
using System;
using System.Reflection;

namespace WhoIsTalking.Patches
{
    public class HarmonyP
    {
        private static Harmony instance;    

        public static bool IsPatched { get; private set; }
        public const string InstanceId = PluginInfo.GUID;

        internal static void ApplyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                {
                    instance = new Harmony(InstanceId);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}