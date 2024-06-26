using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using WhoIsTalking.Patches;

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        Plugin() => HarmonyP.ApplyPatches();
    }
}