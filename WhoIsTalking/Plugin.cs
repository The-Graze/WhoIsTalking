using BepInEx;
using Photon.Pun;
using Photon.Voice.PUN;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using WhoIsTalking.Patches;

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool ran;

        void Update()
        {
            if (PhotonNetwork.IsConnectedAndReady && !ran)
            {
                GameInit();
                ran = true;
            }
        }
        private void GameInit()
        {
            AssetRef.shader = GameObject.Find("motdtext").GetComponent<Text>().material.shader;
            foreach (VRRig t in Resources.FindObjectsOfTypeAll<VRRig>())
            {
                if (t.GetComponent<VRRig>() && !t.isOfflineVRRig)
                {
                    t.gameObject.AddComponent<NameTagHandler>();
                }
            }
        }
    }
}