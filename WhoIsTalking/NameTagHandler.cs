using UnityEngine;
using Photon.Realtime;
using Photon.Voice.PUN;

namespace WhoIsTalking
{
    class NameTagHandler : MonoBehaviour
    {
        GameObject NameFP, NameTP;
        Renderer FPRend, TPRend, FPSpeakerRend, TPSpeakerRend;
        TextMesh FPText, TPText;

        VRRig rig;
        Player player;
        PhotonVoiceView voice;

        Color Orange = new Color(1, 0.3288f, 0, 1);


        void Awake()
        {
            if(NameFP == null && NameTP == null) 
            { 
                SetUpNameTag();
            }
        }

        void OnEnable()
        {
            GetInfo();
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

        void GetInfo()
        {
            rig = GetComponent<VRRig>();
            player = rig.creator;
            voice = VRRigCache.rigsInUse[player].photonVoiceView;
        }

        void Update()
        {
            try
            {
                FirstPersonViewDistance();
                Color color = ColourHandling();
                FPRend.material.color = color;
                FPSpeakerRend.material.color = color;
                FPRend.transform.LookAt(Camera.main.transform.position);

                TPSpeakerRend.material.color = color;
                TPRend.material.color = color;
                TPRend.transform.LookAt(GorillaTagger.Instance.thirdPersonCamera.transform.position);


                TPSpeakerRend.forceRenderingOff = !voice.IsSpeaking;
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
            FPSpeakerRend.forceRenderingOff = !voice.IsSpeaking || !CanSee;
            FPRend.forceRenderingOff = !CanSee;
        }

        Color ColourHandling()
        {
            switch (rig.setMatIndex)
            {
                default:
                    return new Color(
                        rig.materialsToChangeTo[rig.tempMatIndex].color.r,
                        rig.materialsToChangeTo[rig.tempMatIndex].color.g,
                        rig.materialsToChangeTo[rig.tempMatIndex].color.b,
                        1
                    );
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
