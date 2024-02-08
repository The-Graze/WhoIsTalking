using BepInEx;
using Photon.Pun;
using Photon.Voice.PUN;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using WhoIsTalking.Patches;

namespace WhoIsTalking
{
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            Utilla.Events.GameInitialized += GameInit;
        }
        private void GameInit(object sender, EventArgs e)
        {
            AssetRef.shader = GameObject.Find("motdtext").GetComponent<Text>().material.shader;
            foreach (Transform t in GameObject.Find("Player Objects/RigCache/Rig Parent").transform)
            {
                if (t.GetComponent<VRRig>())
                {
                    t.gameObject.AddComponent<NameTagHandler>();
                }
            }
        }
    }
}