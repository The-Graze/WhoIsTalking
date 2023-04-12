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
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public GameObject speaker;
        public Shader through;
        void Start()
        {Utilla.Events.GameInitialized += OnGameInitialized;}
        void OnGameInitialized(object sender, EventArgs e)
        {
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhoIsTalking.Assets.speaker");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject bweep = bundle.LoadAsset<GameObject>("speaker");
            speaker = bweep;
            speaker.name = "NameTagStore";
            HarmonyPatches.ApplyHarmonyPatches();
        }
    }
    class Talkies : MonoBehaviour
    {
        private GameObject LoadSpeaker;
        private GameObject LSpeaker;
        private GameObject NameTag;
        private Color Pcol;
        private Renderer SpeakerRend;
        private Renderer NameRend;
        private PhotonVoiceView voice;
        private PhotonView view;
        private VRRig rig;
        private Plugin p = FindObjectOfType<Plugin>();
        private TextMesh nametagname;
        private Transform Lookat;
        void Start()
        {
            LoadSpeaker = Instantiate(p.speaker);
            LoadSpeaker.transform.SetParent(gameObject.transform, true);
            LoadSpeaker.transform.localPosition = new Vector3(0f, -1.727f, 0f);
            LoadSpeaker.name = "PlayerNameTag";
            LSpeaker = LoadSpeaker.transform.GetChild(0).gameObject;
            NameTag = LoadSpeaker.transform.GetChild(1).gameObject;
            SpeakerRend = LSpeaker.GetComponent<Renderer>();
            NameRend = NameTag.GetComponent<Renderer>();
            SpeakerRend.material.shader = Shader.Find("GUI/Text Shader");
            NameRend.material.shader = Shader.Find("GUI/Text Shader");
            voice = gameObject.GetComponent<PhotonVoiceView>();
            rig = GetComponent<VRRig>();
            Pcol = new Color(0, 0, 0);
            nametagname = NameTag.GetComponent<TextMesh>();
            view = GetComponent<PhotonView>();
            Lookat = FindObjectOfType<GorillaLocomotion.Player>().transform;
        }
        void Update()
        {
           SpeakerRend.enabled = voice.IsSpeaking || voice.IsRecording;
           Pcol.r = rig.materialsToChangeTo[0].color.r;
           Pcol.g = rig.materialsToChangeTo[0].color.g;
           Pcol.b = rig.materialsToChangeTo[0].color.b;
           nametagname.text = view.Controller.NickName;
           SpeakerRend.material.color = Pcol;
           NameRend.material.color = Pcol;
           NameTag.transform.LookAt(new Vector3(Lookat.position.x, transform.position.y, Lookat.position.z));
           LSpeaker.transform.Rotate(transform.up * 300f * Time.deltaTime);
        }
    }
}