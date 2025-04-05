using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace WhoIsTalking.Patches
{
    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("SetColor", MethodType.Normal)]
    internal class UpdateInfoPatch
    {
        private static void Postfix(VRRig __instance)
        {
            if (!__instance.isLocal)
            {
                __instance.GetOrAddComponent<NameTagHandler>(out var nth);
                nth.GetInfo();
            }
        }
    }
}