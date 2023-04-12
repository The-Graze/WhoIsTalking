using HarmonyLib;

namespace WhoIsTalking.Patches
{
    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class VRRigPatch
    {
        internal static void Postfix(VRRig __instance)
        {
            if (__instance.GetComponent<Talkies>() == null)
            {
                __instance.gameObject.AddComponent<Talkies>();
            }
        }
    }
}