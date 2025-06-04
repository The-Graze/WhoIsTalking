using System;                               // for Math
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;                   // Recorder.LevelMeter
using Cinemachine;
using GorillaExtensions;

namespace WhoIsTalking
{
    public class NameTagHandler : MonoBehaviour
    {
        /* -------- prefab parts -------- */
        GameObject NameFP, NameTP;
        Renderer FPRend, TPRend, FPSpeakerRend, TPSpeakerRend;
        TextMesh FPText, TPText;
        Camera ThirdPCam;

        Spinner FPSpeakerSpin, TPSpeakerSpin;
        Vector3 speakerBaseScale;               // original prefab scale
        Color currentColour;

        /* -------- Photon refs -------- */
        public VRRig rig;
        NetPlayer player;
        PhotonVoiceView voice;

        /* -------- helpers -------- */
        readonly Color Orange = new Color(1f, 0.3288f, 0f, 1f);
        static readonly float[] audioSamples = new float[256];    // reused buffer

        /* -------- proximity-voice -------- */
        AudioSource speakerSrc;
        float baseVolume = 1f;
        int lastViewID = -1;

        void Start()
        {
            if (NameFP == null && NameTP == null)
                SetUpNameTag();

            GetInfo();                               // cache refs on first spawn

            currentColour = ColourHandling();
        }

        void SetUpNameTag()
        {
            /* first-person tag */
            SetUpNameTagInstance(ref NameFP, "First Person NameTag", "FirstPersonOnly");
            FPSpeakerRend = NameFP.transform.GetChild(0).GetComponent<Renderer>();
            FPSpeakerRend.material.shader = AssetRef.shader;
            FPRend = NameFP.transform.GetChild(1).GetComponent<Renderer>();
            FPRend.material.shader = AssetRef.shader;
            FPText = FPRend.GetComponent<TextMesh>();

            FPSpeakerSpin = FPSpeakerRend.gameObject.AddComponent<Spinner>();
            FPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            /* third-person / mirror tag */
            SetUpNameTagInstance(ref NameTP, "Third Person NameTag", "MirrorOnly");
            TPSpeakerRend = NameTP.transform.GetChild(0).GetComponent<Renderer>();
            TPSpeakerRend.material.shader = AssetRef.shader;
            TPRend = NameTP.transform.GetChild(1).GetComponent<Renderer>();
            TPRend.material.shader = AssetRef.shader;
            TPText = TPRend.GetComponent<TextMesh>();

            TPSpeakerSpin = TPSpeakerRend.gameObject.AddComponent<Spinner>();
            TPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            speakerBaseScale = FPSpeakerRend.transform.localScale;

            try { ThirdPCam = FindObjectOfType<CinemachineBrain>()?.GetComponent<Camera>(); }
            catch { }
        }

        void SetUpNameTagInstance(ref GameObject tag, string name, string layerName)
        {
            tag = Instantiate(AssetRef.Tag, transform);
            tag.transform.localPosition = new Vector3(0f, -1.727f, 0f);

            int layer = LayerMask.NameToLayer(layerName);
            tag.layer = layer;
            foreach (Transform t in tag.transform) t.gameObject.layer = layer;

            tag.name = name;
        }

        public void GetInfo()
        {
            rig = GetComponent<VRRig>();
            player = rig?.OwningNetPlayer;
            voice = VRRigCache.rigsInUse[rig.OwningNetPlayer].voiceView;

            RefreshSpeakerRef();
        }

        /*──────────────────────────────────────────────────────────*/
        void RefreshSpeakerRef()
        {
            /*───────────────────────── primary (Photon Voice) ─────────────────────────*/
            if (voice?.SpeakerInUse != null)
            {
                PhotonView pv = voice.SpeakerInUse.GetComponent<PhotonView>();
                AudioSource src = voice.SpeakerInUse.GetComponent<AudioSource>();

                if (src != null)
                {
                    int id = (pv != null) ? pv.ViewID : -1;   // –1 when no PhotonView

                    if (speakerSrc != src)        // swapped lobby or first run
                    {
                        speakerSrc = src;
                        baseVolume = speakerSrc.volume;
                        lastViewID = id;
                    }

                    return;   // all good – nothing else to do
                }
            }

            /*───────────────────────── fallback (manual hierarchy) ────────────────────*/
            // rig → body/head/SpeakerHeadCollider/HeadSpeaker
            Transform t = transform.Find(
                "body/head/SpeakerHeadCollider/HeadSpeaker");

            if (t == null) return;

            AudioSource manualSrc = t.GetComponent<AudioSource>();
            if (manualSrc == null) return;

            // only update if we *changed* speaker
            if (speakerSrc != manualSrc)
            {
                speakerSrc = manualSrc;
                baseVolume = speakerSrc.volume;
                lastViewID = -1;
            }
        }


        void FixedUpdate()
        {
            RefreshSpeakerRef();                       // keep refs valid

            try
            {
                FPSpeakerSpin.Speed = TPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

                Color targetCol = ColourHandling();
                float colourSpeed = (Mod.ColourChangeTime.Value > 0f) ?
                                      Time.deltaTime / Mod.ColourChangeTime.Value : 1f;
                currentColour = Color.Lerp(currentColour, targetCol, colourSpeed);

                float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
                bool withinRange = dist <= Mod.ViewDistance.Value.ClampSafe(0, 10);
                bool showFPTag = Mod.ShowFirstPersonTag.Value && withinRange;
                bool showTPTag = Mod.ShowThirdPersonTag.Value && withinRange;
                bool speaking = Mod.Speaker.Value && voice.IsSpeaking;

                bool showFPIcon = showFPTag && speaking;
                bool showTPIcon = showTPTag && speaking;

                float fadeSpeed = (Mod.FadeTime.Value > 0f) ? 1f / Mod.FadeTime.Value : 1000f;
                FadeRenderer(FPRend, showFPTag, currentColour, fadeSpeed);
                FadeRenderer(FPSpeakerRend, showFPIcon, currentColour, fadeSpeed);
                FadeRenderer(TPRend, showTPTag, currentColour, fadeSpeed);
                FadeRenderer(TPSpeakerRend, showTPIcon, currentColour, fadeSpeed);

                /*──── mic-pulse ────*/
                if (Mod.MicPulse.Value)
                    ApplyMicPulse();
                else
                {
                    FPSpeakerRend.transform.localScale = speakerBaseScale;
                    TPSpeakerRend.transform.localScale = speakerBaseScale;
                }

                /*──── proximity voice ────*/
                if (speakerSrc != null)
                {
                    float targetVol;
                    if (Mod.ProximityVoiceChat.Value && !player.IsLocal)
                        targetVol = (dist <= Mod.ViewDistance.Value.ClampSafe(0,10)) ? baseVolume : 0f;
                    else
                        targetVol = baseVolume;

                    speakerSrc.volume = Mathf.MoveTowards(
                        speakerSrc.volume,
                        targetVol,
                        fadeSpeed * Time.deltaTime);
                }

                /*──── billboard & text ────*/
                FPRend.transform.LookAt(Camera.main.transform.position);
                TPRend.transform.LookAt((ThirdPCam != null)
                                        ? ThirdPCam.transform.position
                                        : Camera.main.transform.position);

                FPText.text = TPText.text = player.NickName;
            }
            catch
            {
                GetInfo();          // regain refs if something went null
            }
        }

        void ApplyMicPulse()
        {
            float amp = 0f;

            if (voice?.RecorderInUse?.LevelMeter != null)
                amp = voice.RecorderInUse.LevelMeter.CurrentPeakAmp;
            else if (voice?.SpeakerInUse != null)
            {
                AudioSource src = voice.SpeakerInUse.GetComponent<AudioSource>();
                if (src != null && src.isPlaying)
                {
                    src.GetOutputData(audioSamples, 0);
                    double sum = 0;
                    foreach (float s in audioSamples)
                        sum += s * s;
                    amp = (float)Math.Sqrt(sum / audioSamples.Length);
                }
            }

            amp = Mathf.Clamp01(amp * Mod.MicPulseSensitivity.Value);

            float scale = Mathf.Lerp(Mod.MicPulseMinScale.Value,
                                     Mod.MicPulseMaxScale.Value,
                                     amp);

            Vector3 scl = speakerBaseScale * scale;
            FPSpeakerRend.transform.localScale = scl;
            TPSpeakerRend.transform.localScale = scl;
        }

        void FadeRenderer(Renderer rend, bool visible, Color rgb, float speed)
        {
            Color c = rend.material.color;
            float tgt = visible ? 1f : 0f;
            float a = Mathf.MoveTowards(c.a, tgt, speed * Time.deltaTime);

            rgb.a = a;
            rend.material.color = rgb;
            rend.forceRenderingOff = a <= 0.01f;
        }

        Color ColourHandling()
        {
            int idx = rig.setMatIndex;
            if (idx == 1)
                return Color.red;
            else if (idx == 2 || idx == 11)
                return Orange;
            else if (idx == 3 || idx == 7)
                return Color.blue;
            else if (idx == 12)
                return Color.green;
            else
                return rig.playerColor;
        }

        void OnDisable()
        {
            if (speakerSrc != null)
                speakerSrc.volume = baseVolume;   // leave source untouched for other mods
        }
    }
}
