using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace WhoIsTalking.Patches
{
    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("SetColor", MethodType.Normal)]
    internal class UpdateInfoPatch
    {
        private static void Postfix(VRRig __instance, Color color)
        {
            if (!__instance.isLocal)
            {
                __instance.GetOrAddComponent<NameTagHandler>(out var nth);
                nth.RefreshInfo(color);
            }
        }
    }
}