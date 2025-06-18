using System.Reflection;
using HarmonyLib;

namespace WhoIsTalking.Patches
{
    public class HarmonyP
    {
        public const string InstanceId = PluginInfo.GUID;
        private static Harmony instance;

        public static bool IsPatched { get; private set; }

        internal static void ApplyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null) instance = new Harmony(InstanceId);

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