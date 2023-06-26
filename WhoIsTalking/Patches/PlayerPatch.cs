using HarmonyLib;
using Photon.Pun;
using Photon.Voice.PUN;

namespace WhoIsTalking.Patches
{
    [HarmonyPatch(typeof(VRRigSerializer))]
    [HarmonyPatch("OnInstantiateSetup", MethodType.Normal)]
    internal class VRRigPatch
    {
        internal static void Postfix(VRRigSerializer __instance)
        {
            __instance.vrrig.gameObject.AddComponent<Talkies>();
            __instance.vrrig.GetComponent<Talkies>().rig = __instance.vrrig;
            __instance.vrrig.GetComponent<Talkies>().view = __instance.GetComponent<PhotonView>();
            __instance.vrrig.GetComponent<Talkies>().voice = __instance.GetComponent<PhotonVoiceView>();
        }
    }
}