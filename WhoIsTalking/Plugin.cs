using BepInEx;
using Photon.Pun;
using Photon.Voice.PUN;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WhoIsTalking.Patches;

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public GameObject speaker;
        public static volatile Plugin Instance;
        public Shader shader;
        public float viewDistance = 3.5f;
        void Start()
        {
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhoIsTalking.Assets.speaker");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject bweep = bundle.LoadAsset<GameObject>("speaker");
            speaker = bweep;
            speaker.name = "NameTagStore";
            Instance = this;
            Utilla.Events.GameInitialized += OnGameInitialized;
            Config.Bind("Settings", "Camera Look Target", false);
            HarmonyPatches.ApplyHarmonyPatches();
        }
        void Update()
        {
            viewDistance = 3.5f;
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            shader = GameObject.Find("motdtext").GetComponent<Text>().material.shader;
        }
    }
    class Talkies : MonoBehaviour
    {
        GameObject LoadSpeaker;
        GameObject LoadSpeaker2;
        GameObject LSpeaker;
        GameObject Speaker2;
        GameObject NameTag;
        GameObject NameTag2;
        Renderer SpeakerRend;
        Renderer SpeakerRend2;
        Renderer NameRend;
        Renderer NameRend2;
        public PhotonVoiceView voice;
        public PhotonView view;
        public VRRig rig;
        Plugin p = Plugin.Instance;
        TextMesh nametagname;
        TextMesh nametagname2;
        Transform Lookat;
        Transform Lookat2;
        Spinner spwinner;
        Spinner spwinner2;
        Color Orange;
        float Distance;
        void Start()
        {
            if (rig.isOfflineVRRig)
            {
                Destroy(this);
            }
            else
            {
                LoadSpeaker = Instantiate(p.speaker, transform);
                LoadSpeaker2 = Instantiate(p.speaker, transform);
                LoadSpeaker.transform.localPosition = new Vector3(0f, -1.727f, 0f);
                LoadSpeaker2.transform.localPosition = new Vector3(0f, -1.727f, 0f);
                LoadSpeaker.name = "fpPlayerNameTag";
                LoadSpeaker2.name = "tpPlayerNameTag";
                LoadSpeaker.layer = LayerMask.NameToLayer("GorillaCosmeticParticle");
                LoadSpeaker2.layer = LayerMask.NameToLayer("Gorilla Spectator");
                LSpeaker = LoadSpeaker.transform.GetChild(0).gameObject;
                NameTag = LoadSpeaker.transform.GetChild(1).gameObject;
                NameTag2 = LoadSpeaker2.transform.GetChild(1).gameObject;
                Speaker2 = LoadSpeaker2.transform.GetChild(0).gameObject;
                nametagname = NameTag.GetComponent<TextMesh>();
                nametagname2 = NameTag2.GetComponent<TextMesh>();
                SpeakerRend = LSpeaker.GetComponent<Renderer>();
                SpeakerRend2 = Speaker2.GetComponent<Renderer>();
                NameRend = NameTag.GetComponent<Renderer>();
                NameRend2 = NameTag2.GetComponent<Renderer>();
                SpeakerRend.material.shader = p.shader;
                NameRend.material.shader = p.shader;
                nametagname = NameTag.GetComponent<TextMesh>();
                spwinner = LSpeaker.AddComponent<Spinner>();
                spwinner2 = Speaker2.AddComponent<Spinner>();
                spwinner.Speed = 1;
                spwinner2.Speed = 1;
                Orange = new Color(1, 0.3288f, 0, 1);
                Lookat = Camera.main.transform;
                Lookat2 = GameObject.Find("Shoulder Camera").transform;
            }
        }
        void LateUpdate()
        {
            nametagname.text = view.Controller.NickName;
            nametagname2.text = view.Controller.NickName;
            NameTag.transform.LookAt(Lookat);
            NameTag2.transform.LookAt(Lookat2);
            Distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            Cansee();
            Plugin.Instance.viewDistance = 3.5f;
            SpeakerRend2.enabled = voice.IsSpeaking || voice.IsRecording;
            SpeakerRend2.material.color = Pcol();
            NameRend2.material.color = Pcol();
        }
        bool Cansee()
        {
            if (Distance <= Plugin.Instance.viewDistance)
            {
                NameRend.enabled = true;
                SpeakerRend.enabled = voice.IsSpeaking || voice.IsRecording;
                SpeakerRend.material.color = Pcol();
                NameRend.material.color = Pcol();
                return true;
            }
            else
            {
                SpeakerRend.enabled = false;
                NameRend.enabled = false;
                return false;
            }
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