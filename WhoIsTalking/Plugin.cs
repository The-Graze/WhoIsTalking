using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace WhoIsTalking
{
    [BepInPlugin("Graze.WhoIsTalking", "Who Is Talking", "4.30")]
    public class Plugin : BaseUnityPlugin
    {
        Plugin()
        {
            Harmony instance = new Harmony("Graze.WhoIsTalking");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}