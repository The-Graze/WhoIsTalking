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

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public GameObject speaker;
        public Shader through;
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
            shader = GameObject.Find("motdtext").GetComponent<Text>().material.shader;
            HarmonyPatches.ApplyHarmonyPatches();
        }
    }
    class Talkies : MonoBehaviour
    {
        GameObject LoadSpeaker;
        GameObject LSpeaker;
        GameObject NameTag;
        public Color Pcol;
        public Renderer SpeakerRend;
        public Renderer NameRend;
        PhotonVoiceView voice;
        PhotonView view;
        public VRRig rig;
        Plugin p = Plugin.Instance;
        public TextMesh nametagname;
        Transform Lookat;
        Array data;
        Spinner spwinner;

        void Awake()
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
            //rig = GetComponent<VRRig>();
            Lookat = FindObjectOfType<GorillaLocomotion.Player>().transform;
            Pcol = new Color(0, 0, 0);
            nametagname = NameTag.GetComponent<TextMesh>();
            spwinner = LSpeaker.AddComponent<Spinner>();
            spwinner.Speed = 1;
        }

        void Start()
        {
            view = rig.photonView;
            voice = rig.gameObject.GetComponent<PhotonVoiceView>();
        }
        void Update()
        {
            Pcol.r = rig.materialsToChangeTo[0].color.r;
            Pcol.g = rig.materialsToChangeTo[0].color.g;
            Pcol.b = rig.materialsToChangeTo[0].color.b;
            nametagname.text = view.Controller.NickName;
            SpeakerRend.material.color = Pcol;
            NameRend.material.color = Pcol;
            NameTag.transform.LookAt(new Vector3(Lookat.position.x, transform.position.y, Lookat.position.z));
            if (view != null) 
            {
                nametagname.text = view.Owner.NickName;
            }
            else
            {
                NameRend.enabled = false;
            }
            if(voice != null) 
            {
                SpeakerRend.enabled = voice.IsSpeaking || voice.IsRecording;
            }
            else
            {
                SpeakerRend.enabled = false;
            }
            if (view == null || voice == null)
            {
                SpeakerRend.enabled = false;
                NameRend.enabled = false;
                Start();
            }
        }
    }
}