using BepInEx;
using Mono.Math.Prime.Generator;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using PlayFab.ClientModels;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Utilla;
using System.Collections;
using HarmonyLib;
using POpusCodec.Enums;
using System.Linq;
using WhoIsTalking.Patches;
using GorillaLocomotion;
using UnityEngine.Rendering;
using BepInEx.Configuration;

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public ConfigEntry<bool> selfName;
        public GameObject speaker;
        public static volatile Plugin Instance;
        public Shader shader;
        void Start()
        {
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhoIsTalking.Assets.speaker");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject bweep = bundle.LoadAsset<GameObject>("speaker");
            speaker = bweep;
            speaker.name = "NameTagStore";
            Instance = this;
            Utilla.Events.GameInitialized += OnGameInitialized;
            HarmonyPatches.ApplyHarmonyPatches();
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            shader = GameObject.Find("motdtext").GetComponent<Text>().material.shader;
        }
    }
    class Talkies : MonoBehaviour
    {
        GameObject LoadSpeaker;
        GameObject LSpeaker;
        GameObject NameTag;
        Renderer SpeakerRend;
        Renderer NameRend;
        public PhotonVoiceView voice;
        public PhotonView view;
        public VRRig rig;
        Plugin p = Plugin.Instance;
        TextMesh nametagname;
        Transform Lookat;
        Spinner spwinner;
        Color Orange;
        void Start()
        {
            if (rig.isOfflineVRRig)
            {
                Destroy(this);
            }
            else
            {
                LoadSpeaker = Instantiate(p.speaker, transform);
                LoadSpeaker.transform.localPosition = new Vector3(0f, -1.727f, 0f);
                LoadSpeaker.name = "PlayerNameTag";
                LSpeaker = LoadSpeaker.transform.GetChild(0).gameObject;
                NameTag = LoadSpeaker.transform.GetChild(1).gameObject;
                nametagname = NameTag.GetComponent<TextMesh>();
                SpeakerRend = LSpeaker.GetComponent<Renderer>();
                NameRend = NameTag.GetComponent<Renderer>();
                SpeakerRend.material.shader = p.shader;
                NameRend.material.shader = p.shader;
                Lookat = Camera.main.transform;
                nametagname = NameTag.GetComponent<TextMesh>();
                spwinner = LSpeaker.AddComponent<Spinner>();
                spwinner.Speed = 1;
                Orange = new Color(1, 0.3288f, 0, 1);
            }
        }
        void LateUpdate()
        {
            nametagname.text = view.Controller.NickName;
            SpeakerRend.material.color = Pcol();
            NameRend.material.color = Pcol();
            NameTag.transform.LookAt(new Vector3(Lookat.position.x, transform.position.y, Lookat.position.z));
            nametagname.text = view.Owner.NickName;

            SpeakerRend.enabled = voice.IsSpeaking || voice.IsRecording;
        }
        public Color Pcol()
        {
            Color temp = new Color();
            if (rig.setMatIndex == 2)
            {
                return Orange;
            }
            if (rig.setMatIndex == 3)
            {
                return Color.blue;
            }
            if (rig.setMatIndex == 7)
            {
                return Color.blue;
            }
            if (rig.setMatIndex == 11)
            {
                return Orange;
            }
            else
            {
                temp.r = rig.materialsToChangeTo[rig.tempMatIndex].color.r;
                temp.g = rig.materialsToChangeTo[rig.tempMatIndex].color.g;
                temp.b = rig.materialsToChangeTo[rig.tempMatIndex].color.b;
                temp.a = 1;
                return temp;
            }
        }
    }
}