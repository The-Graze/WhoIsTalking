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
            GorillaParent.instance.vrrigParent.AddComponent<SpeakerManager>();
            
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
        private int speed;
        void Start()
        {
            LoadSpeaker = Instantiate(GameObject.Find("speaker(Clone)"));
            LoadSpeaker.transform.SetParent(gameObject.transform, false);
            LSpeaker = LoadSpeaker.transform.GetChild(0).gameObject;
            speed = 300;
            NameTag = LoadSpeaker.transform.GetChild(1).gameObject;
            NameTag.AddComponent<Looking>();
            StartCoroutine(ExecuteAfterTime(2));
        }
        void LateUpdate()
        {
            this.LSpeaker.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
            this.NameTag.GetComponent<Renderer>().material.shader = this.LSpeaker.GetComponent<Renderer>().material.shader;
            this.NameTag.GetComponent<TextMesh>().text = this.gameObject.GetComponent<PhotonView>().Controller.NickName;
            this.NameTag.GetComponent<TextMesh>().color = this.LSpeaker.GetComponent<Renderer>().material.color;
            if (gameObject.GetComponent<PhotonVoiceView>().IsSpeaking == true)
            {
                this.LSpeaker.SetActive(true);
                this.LSpeaker.transform.Rotate(transform.up * speed * Time.deltaTime);
            }
            else
            {
                this.LSpeaker.SetActive(false);
            }
        }
        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);

            this.LSpeaker.GetComponent<Renderer>().material.color = this.LoadSpeaker.transform.parent.Find("gorilla").GetComponent<Renderer>().material.color;
        }

    }
    class Looking : MonoBehaviour
    {
        
        private Transform Lookat;
        void Start()
        {
            Lookat = GorillaParent.instance.vrrigs[0].transform;
        }
        void Update()
        {
            transform.LookAt(new Vector3(Lookat.position.x, transform.position.y, Lookat.position.z));
        }
    }
}
