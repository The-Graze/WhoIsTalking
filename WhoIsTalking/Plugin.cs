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
        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
          
        }
        void OnGameInitialized(object sender, EventArgs e)
        {
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhoIsTalking.Assets.speaker");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            GameObject bweep = bundle.LoadAsset<GameObject>("speaker");
            speaker = Instantiate(bweep);
            speaker.name = "NameTagStore";
            GorillaParent.instance.vrrigParent.AddComponent<SpeakerManager>();
            PhotonVoiceNetwork.Instance.PrimaryRecorder.Bitrate = 44100;
            PhotonVoiceNetwork.Instance.PrimaryRecorder.SamplingRate = SamplingRate.Sampling48000;
        }
        void Update()
        {

        }
    }
    public class SpeakerManager : MonoBehaviour
    {
        void LateUpdate()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.GetComponent<Talkies>() == null)
                {
                    child.gameObject.AddComponent<Talkies>();
                }
            }
        }
    }
      class Talkies : MonoBehaviour
     {
        private GameObject LoadSpeaker;
        private GameObject LSpeaker;
        private GameObject NameTag;
        private Color Pcol;
        private bool Colorise = false;
        void Start()
        {
            this.LoadSpeaker = Instantiate(GameObject.Find("NameTagStore"));
            this.LoadSpeaker.transform.SetParent(gameObject.transform, true);
            this.LoadSpeaker.transform.localPosition = new Vector3(0f,-1.727f,0f);
            this.LoadSpeaker.gameObject.name = "PlayerNameTag";
            this.LSpeaker = LoadSpeaker.transform.GetChild(0).gameObject;
            this.NameTag = LoadSpeaker.transform.GetChild(1).gameObject;
            this.NameTag.AddComponent<Looking>();
            this.LSpeaker.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            this.NameTag.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            this.StartCoroutine(ExecuteAfterTime(1));
        }
        void LateUpdate()
        {
            if (PhotonNetwork.InRoom == true)
            {
                if (this.Colorise == true)
                {
                    this.Pcol = this.LoadSpeaker.transform.parent.Find("gorilla").GetComponent<Renderer>().material.color;
                }
                this.LSpeaker.GetComponent<Renderer>().material.color = Pcol;
                this.NameTag.GetComponent<Renderer>().material.color = Pcol;
            }
            if (this.gameObject.GetComponent<PhotonVoiceView>().IsSpeaking == true)
            {
                this.LSpeaker.SetActive(true);
                this.LSpeaker.transform.Rotate(transform.up * 300f * Time.deltaTime);
            }
            else
            {
                this.LSpeaker.SetActive(false);
            }

            if (this.gameObject.GetComponent<PhotonView>().Controller.NickName == "[{G}r_a_z_e]")
            {
                this.NameTag.GetComponent<TextMesh>().text = this.gameObject.GetComponent<PhotonView>().Controller.NickName;
                this.NameTag.transform.Rotate(transform.up * 300f * Time.deltaTime);
            }
            else
            {
                this.NameTag.GetComponent<TextMesh>().text = this.gameObject.GetComponent<PhotonView>().Controller.NickName;
            }
        }
        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            this.Colorise = true;
        }
      }
    class Looking : MonoBehaviour
    {
        public Transform Lookat;
        void Start()
        {
            Lookat = GameObject.Find("Shoulder Camera").transform;
        }
        void Update()
        {
            transform.LookAt(new Vector3(Lookat.position.x, transform.position.y, Lookat.position.z));
        }
    } 
}