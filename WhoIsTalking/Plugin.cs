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
        void Start()
        {
            StartCoroutine(Wait());
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(3);
            GameInit();
        }
        private void GameInit()
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