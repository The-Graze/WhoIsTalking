using UnityEngine;
using Photon.Realtime;
using Photon.Voice.PUN;
using Cinemachine;

namespace WhoIsTalking
{
    class NameTagHandler : MonoBehaviour
    {
        GameObject NameFP, NameTP;
        Renderer FPRend, TPRend, FPSpeakerRend, TPSpeakerRend;
        TextMesh FPText, TPText;
        Camera ThirdPCam;

        VRRig rig;
        NetPlayer player;
        PhotonVoiceView voice;

        Color Orange = new Color(1, 0.3288f, 0, 1);
        void Start()
        {
            if(NameFP == null && NameTP == null) 
            { 
                SetUpNameTag();
            }
        }
        void SetUpNameTag()
        {
            SetUpNameTagInstance(ref NameFP, "First Person NameTag", "GorillaCosmeticParticle");
            FPSpeakerRend = NameFP.transform.GetChild(0).GetComponent<Renderer>();
            FPSpeakerRend.material.shader = AssetRef.shader;
            FPRend = NameFP.transform.GetChild(1).GetComponent<Renderer>();
            FPRend.material.shader = AssetRef.shader;   
            FPText = FPRend.GetComponent<TextMesh>();
            FPSpeakerRend.gameObject.AddComponent<Spinner>().Speed = 1f;

            SetUpNameTagInstance(ref NameTP, "Third Person NameTag", "Gorilla Spectator");
            TPSpeakerRend = NameTP.transform.GetChild(0).GetComponent<Renderer>();
            TPSpeakerRend.material.shader = AssetRef.shader;
            TPRend = NameTP.transform.GetChild(1).GetComponent<Renderer>();
            TPRend.material.shader = AssetRef.shader;
            TPText = TPRend.GetComponent<TextMesh>();
            TPSpeakerRend.gameObject.AddComponent<Spinner>().Speed = 1f;
            try
            {
                Camera attemptCam1 = FindObjectOfType<CinemachineBrain>().GetComponent<Camera>();
                if (attemptCam1 != null)
                {
                    ThirdPCam = attemptCam1;
                }
                else
                {
                    Camera attemptCam2 = FindObjectOfType<CinemachineBrain>().GetComponent<Camera>();
                    if (attemptCam2)
                    {
                        ThirdPCam = attemptCam2;
                    }
                }
            }
            catch
            {

            }
        }

        void SetUpNameTagInstance(ref GameObject nameTag, string name, string layerName)
        {
            nameTag = Instantiate(AssetRef.Tag, transform);
            nameTag.transform.localPosition = new Vector3(0f, -1.727f, 0f);
            nameTag.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform t in nameTag.transform)
            {
                t.gameObject.layer = LayerMask.NameToLayer(layerName);
            }
            nameTag.name = name;
        }

        public void GetInfo()
        {
            rig = GetComponent<VRRig>();
            player = rig?.OwningNetPlayer;
            voice = VRRigCache.rigsInUse[rig.OwningNetPlayer].voiceView;
        }
        void FixedUpdate()
        {
            try
            {
                FirstPersonViewDistance();
                FPRend.material.color = ColourHandling();
                FPSpeakerRend.material.color = ColourHandling();
                FPRend.transform.LookAt(Camera.main.transform.position);

                TPSpeakerRend.material.color = ColourHandling();
                TPRend.material.color = ColourHandling();
                if (ThirdPCam != null)
                {
                    TPRend.transform.LookAt(ThirdPCam.transform.position);
                }
                else
                {
                    TPRend.transform.LookAt(Camera.main.transform.position);
                }

                if (Mod.Speaker.Value == true)
                {
                    TPSpeakerRend.forceRenderingOff = !voice.IsSpeaking;
                }
                else
                {
                    TPSpeakerRend.forceRenderingOff = true;
                }
                TPText.text = player.NickName;
                FPText.text = player.NickName;
            }
            catch
            {
                GetInfo();
            }
        }

        void FirstPersonViewDistance()
        {
            float Distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float viewDistance = 4f;
            bool CanSee = (bool)(Distance <= viewDistance);
            if (Mod.Speaker.Value == true)
            {
                FPSpeakerRend.forceRenderingOff = !voice.IsSpeaking || !CanSee;
            }
            else
            {
                FPSpeakerRend.forceRenderingOff = true;
            }
            FPRend.forceRenderingOff = !CanSee;
        }

        Color ColourHandling()
        {
            switch (rig.setMatIndex)    
            {
                default:
                    return rig.playerColor;
                case 1:
                    return Color.red;
                case 2:
                case 11:
                    return Orange;
                case 3:
                case 7:
                    return Color.blue;
                case 12:
                    return Color.green;
            }
        }
    }
}
