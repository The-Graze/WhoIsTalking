using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using WhoIsTalking.Patches;

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Mod : BaseUnityPlugin
    {
        Mod() => HarmonyP.ApplyPatches();
    }
}