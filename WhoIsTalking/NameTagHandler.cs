
using System;                               // for Math
using UnityEngine;
using UnityEngine.SceneManagement;          // ① NEW – scene check
using Photon.Realtime;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;                   // Recorder.LevelMeter
using GorillaExtensions;
using JetBrains.Annotations;
using Photon.Voice.PUN;
using Unity.Cinemachine;
using UnityEngine;

namespace WhoIsTalking
{
    public class NameTagHandler : MonoBehaviour
    {
        /* ───────── prefab parts ───────── */
        GameObject NameFP, NameTP;
        Renderer FPRend, TPRend, FPSpeakerRend, TPSpeakerRend;
        TextMesh FPText, TPText;
        Camera ThirdPCam;

        Spinner FPSpeakerSpin, TPSpeakerSpin;
        Vector3 speakerBaseScale;               // original prefab scale
        Color currentColour;

        /* ───────── Photon refs ───────── */
        public VRRig rig;
        NetPlayer player;
        PhotonVoiceView voice;

        /* ───────── helpers ───────── */
        readonly Color Orange = new Color(1f, 0.3288f, 0f, 1f);
        private Color baseCol = Color.black;
        static readonly float[] audioSamples = new float[256];    // reused buffer
        private float baseVolume = 1f;
        private Color currentColour;
        private Renderer FPRend, TPRend, FPSpeakerRend, TPSpeakerRend;

        private Spinner FPSpeakerSpin, TPSpeakerSpin;
        private TextMesh FPText, TPText;

        /* ───────── proximity‑voice ───────── */
        AudioSource speakerSrc;
        float baseVolume = 1f;
        int lastViewID = -1;

        /* ───────── existing skin reference ───────── */
        Transform gorillaNew;

        /* ───────── ② NEW – quick Bayou helper ───────── */
        static bool BayouActive()
        {
            Scene s = SceneManager.GetSceneByName("Bayou");
            return s.IsValid() && s.isLoaded;
        }

        /*──────────────────────────────────────────────────────────*/
        void Start()
        {
            if (NameFP == null && NameTP == null)
                SetUpNameTag();

            RefreshInfo(baseCol);                               // cache refs on first spawn

            /* existing ► cache reference to the player’s skin object */
            gorillaNew = transform.root.Find("gorilla_new");

            currentColour = ColourHandling();
            RefreshInfo(baseCol);                               // cache refs on first spawn
        }

        /*──────────────────────────────────────────────────────────*/
        void SetUpNameTag()
        {
            /* first‑person tag */
            SetUpNameTagInstance(ref NameFP, "First Person NameTag", "FirstPersonOnly");
            FPSpeakerRend = NameFP.transform.GetChild(0).GetComponent<Renderer>();
            FPSpeakerRend.material.shader = AssetRef.Shader;
            FPRend = NameFP.transform.GetChild(1).GetComponent<Renderer>();
            FPRend.material.shader = AssetRef.Shader;
            FPText = FPRend.GetComponent<TextMesh>();

            FPSpeakerSpin = FPSpeakerRend.gameObject.AddComponent<Spinner>();
            FPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            /* third‑person / mirror tag */
            SetUpNameTagInstance(ref NameTP, "Third Person NameTag", "MirrorOnly");
            TPSpeakerRend = NameTP.transform.GetChild(0).GetComponent<Renderer>();
            TPSpeakerRend.material.shader = AssetRef.Shader;
            TPRend = NameTP.transform.GetChild(1).GetComponent<Renderer>();
            TPRend.material.shader = AssetRef.Shader;
            TPText = TPRend.GetComponent<TextMesh>();

            TPSpeakerSpin = TPSpeakerRend.gameObject.AddComponent<Spinner>();
            TPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            speakerBaseScale = FPSpeakerRend.transform.localScale;

            try { ThirdPCam = FindObjectOfType<CinemachineBrain>()?.GetComponent<Camera>(); }
            catch { }
        }
        private void SetUpNameTagInstance(ref GameObject nameTag, string goName, string layerName)
        {
            nameTag = Instantiate(AssetRef.Tag, transform);
            nameTag.transform.localPosition = new Vector3(0f, -1.727f, 0f);

            var layer = LayerMask.NameToLayer(layerName);
            nameTag.layer = layer;
            foreach (Transform t in nameTag.transform) t.gameObject.layer = layer;

            nameTag.name = goName;
        }

        public void RefreshInfo(Color c)
        {
            rig = GetComponent<VRRig>();
            player = rig?.OwningNetPlayer;
            voice = VRRigCache.rigsInUse[rig.OwningNetPlayer].voiceView;
            baseCol = c;
            RefreshSpeakerRef();
        }

        /*──────────────────────────────────────────────────────────*/
        void RefreshSpeakerRef()
        {
            /*──────── primary (Photon Voice) ────────*/
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

            /*──────── fallback (manual hierarchy) ────────*/
            Transform t = transform.Find("body/head/SpeakerHeadCollider/HeadSpeaker");
            if (t == null) return;

            AudioSource manualSrc = t.GetComponent<AudioSource>();
            if (manualSrc == null) return;

            if (speakerSrc != manualSrc)
            {
                speakerSrc = manualSrc;
                baseVolume = speakerSrc.volume;
                lastViewID = -1;
            }
        }

        /*──────── keep gorilla_new ref healthy ────────*/
        void RefreshGorillaNewRef()
        {
            if (gorillaNew == null)
                gorillaNew = transform.root.Find("gorilla_new");
        }

        /*──────────────────────────────────────────────────────────*/
        void FixedUpdate()
        {
            RefreshGorillaNewRef();
            RefreshSpeakerRef();

            try
            {
                FPSpeakerSpin.Speed = TPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

                Color targetCol = ColourHandling();
                float colourSpeed = (Mod.ColourChangeTime.Value > 0f)
                                    ? Time.deltaTime / Mod.ColourChangeTime.Value
                                    : 1f;
                currentColour = Color.Lerp(currentColour, targetCol, colourSpeed);

                /*──── ③ NEW: fade tags while Bayou scene is loaded ────*/
                bool bayou = BayouActive();

                bool skinVisible = gorillaNew == null || gorillaNew.gameObject.activeInHierarchy;

                float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
                bool withinRange = dist <= Mod.ViewDistance.Value.ClampSafe(0, 10);

                bool showFPTag = !bayou && Mod.ShowFirstPersonTag.Value && withinRange && skinVisible;
                bool showTPTag = !bayou && Mod.ShowThirdPersonTag.Value && withinRange && skinVisible;
                bool speaking = !bayou && Mod.Speaker.Value && voice.IsSpeaking;

                bool showFPIcon = showFPTag && speaking;
                bool showTPIcon = showTPTag && speaking;

                float fadeSpeed = (Mod.FadeTime.Value > 0f) ? 1f / Mod.FadeTime.Value : 1000f;

                FadeRenderer(FPRend, showFPTag, currentColour, fadeSpeed);
                FadeRenderer(FPSpeakerRend, showFPIcon, currentColour, fadeSpeed);
                FadeRenderer(TPRend, showTPTag, currentColour, fadeSpeed);
                FadeRenderer(TPSpeakerRend, showTPIcon, currentColour, fadeSpeed);

                /*──── mic‑pulse ────*/
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
                        targetVol = (dist <= Mod.ViewDistance.Value.ClampSafe(0, 10)) ? baseVolume : 0f;
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
                RefreshInfo(baseCol);          // regain refs if something went null
            }
        }

        /*──────────────────────────────────────────────────────────*/
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
                    foreach (float s in audioSamples) sum += s * s;
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

        /*──────────────────────────────────────────────────────────*/
        void FadeRenderer(Renderer rend, bool visible, Color rgb, float speed)
        {
            Color c = rend.material.color;
            float tgt = visible ? 1f : 0f;
            float a = Mathf.MoveTowards(c.a, tgt, speed * Time.deltaTime);

            rgb.a = a;
            rend.material.color = rgb;
            rend.forceRenderingOff = a <= 0.01f;
        }

        /*──────────────────────────────────────────────────────────*/
        Color ColourHandling()
        {
            if (rig.bodyRenderer.cosmeticBodyType == GorillaBodyType.Skeleton) return Color.green;
            switch (rig.setMatIndex)
            {
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
                default:
                    return baseCol;
            }
        }

        /*──────────────────────────────────────────────────────────*/
        void OnDisable()
        {
            if (speakerSrc != null)
                speakerSrc.volume = baseVolume;   // leave source untouched for other mods
        }
    }
}
