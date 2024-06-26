using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace WhoIsTalking.Patches
{
    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("UpdateName", MethodType.Normal)]
    internal class UpdateInfoPatch
    {
        private static void Postfix(VRRig __instance)
        {
            if (!__instance.isOfflineVRRig)
            {
                if (__instance.GetComponent<NameTagHandler>())
                {
                    __instance.GetComponent<NameTagHandler>().GetInfo();
                }
                else
                {
                    __instance.AddComponent<NameTagHandler>();
                }
            }
        }
    }
}