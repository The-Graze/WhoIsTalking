using System;
using GorillaExtensions;
using JetBrains.Annotations;
using Photon.Voice.PUN;
using Unity.Cinemachine;
using UnityEngine;

namespace WhoIsTalking
{
    public class NameTagHandler : MonoBehaviour
    {
        private static readonly float[] audioSamples = new float[256]; // reused buffer

        /* -------- Photon refs -------- */
        public VRRig rig;

        /* -------- helpers -------- */
        private readonly Color Orange = new Color(1f, 0.3288f, 0f, 1f);
        private Color baseCol = Color.black;
        private float baseVolume = 1f;
        private Color currentColour;
        private Renderer FPRend, TPRend, FPSpeakerRend, TPSpeakerRend;

        private Spinner FPSpeakerSpin, TPSpeakerSpin;
        private TextMesh FPText, TPText;


        /* -------- prefab parts -------- */
        private GameObject NameFP, NameTP;
        private NetPlayer player;
        private Vector3 speakerBaseScale; // original prefab scale

        /* -------- proximity-voice -------- */
        private AudioSource speakerSrc;
        private Camera ThirdPCam;
        private PhotonVoiceView voice;

        private void Start()
        {
            if (NameFP == null && NameTP == null)
                SetUpNameTag();

            RefreshInfo(baseCol); // cache refs on first spawn

            currentColour = ColourHandling();
            RefreshInfo(baseCol); // cache refs on first spawn
        }


        private void FixedUpdate()
        {
            RefreshSpeakerRef(); // keep refs valid

            try
            {
                FPSpeakerSpin.Speed = TPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;


                var targetCol = ColourHandling();
                var colourSpeed = Mod.ColourChangeTime.Value > 0f ? Time.deltaTime / Mod.ColourChangeTime.Value : 1f;
                currentColour = Color.Lerp(currentColour, targetCol, colourSpeed);


                var dist = Vector3.Distance(transform.position, GorillaTagger.Instance.mainCamera.transform.position);
                var withinRange = dist <= Mod.ViewDistance.Value.ClampSafe(0, 10);
                var showFPTag = Mod.ShowFirstPersonTag.Value && withinRange && !CheckGhost();
                var speaking = Mod.Speaker.Value && voice.IsSpeaking;

                var showFPIcon = showFPTag && speaking;
                var showTPIcon = speaking;

                var fadeSpeed = Mod.FadeTime.Value > 0f ? 1f / Mod.FadeTime.Value : 1000f;
                FadeRenderer(FPRend, showFPTag, currentColour, fadeSpeed);
                FadeRenderer(FPSpeakerRend, showFPIcon, currentColour, fadeSpeed);
                FadeRenderer(TPRend, true, currentColour, fadeSpeed);
                FadeRenderer(TPSpeakerRend, showTPIcon, currentColour, fadeSpeed);

                /*──── mic-pulse ────*/
                if (Mod.MicPulse.Value)
                {
                    ApplyMicPulse();
                }
                else
                {
                    FPSpeakerRend.transform.localScale = speakerBaseScale;
                    TPSpeakerRend.transform.localScale = speakerBaseScale;
                }

                /*──── proximity voice ────*/
                if (speakerSrc)
                {
                    float targetVol;
                    if (Mod.ProximityVoiceChat.Value && !player.IsLocal)
                        targetVol = dist <= Mod.ViewDistance.Value.ClampSafe(0, 10) ? baseVolume : 0f;
                    else
                        targetVol = baseVolume;

                    speakerSrc.volume = Mathf.MoveTowards(
                        speakerSrc.volume,
                        targetVol,
                        fadeSpeed * Time.deltaTime);
                }

                /*──── billboard & text ────*/
                FPRend.transform.LookAt(GorillaTagger.Instance.mainCamera.transform.position);
                TPRend.transform.LookAt(ThirdPCam
                    ? ThirdPCam.transform.position
                    : GorillaTagger.Instance.mainCamera.transform.position);

                FPText.text = TPText.text = player.NickName;
            }
            catch
            {
                RefreshInfo(baseCol); // regain refs if something went null
            }
        }

        private void OnDisable()
        {
            if (speakerSrc != null)
                speakerSrc.volume = baseVolume; // leave source untouched for other mods
        }

        private void SetUpNameTag()
        {
            /* first-person tag */
            SetUpNameTagInstance(ref NameFP, "First Person NameTag", "FirstPersonOnly");
            FPSpeakerRend = NameFP.transform.GetChild(0).GetComponent<Renderer>();
            FPSpeakerRend.material.shader = AssetRef.Shader;
            FPRend = NameFP.transform.GetChild(1).GetComponent<Renderer>();
            FPRend.material.shader = AssetRef.Shader;
            FPText = FPRend.GetComponent<TextMesh>();

            FPSpeakerSpin = FPSpeakerRend.gameObject.AddComponent<Spinner>();
            FPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            /* third-person / mirror tag */
            SetUpNameTagInstance(ref NameTP, "Third Person NameTag", "MirrorOnly");
            TPSpeakerRend = NameTP.transform.GetChild(0).GetComponent<Renderer>();
            TPSpeakerRend.material.shader = AssetRef.Shader;
            TPRend = NameTP.transform.GetChild(1).GetComponent<Renderer>();
            TPRend.material.shader = AssetRef.Shader;
            TPText = TPRend.GetComponent<TextMesh>();

            TPSpeakerSpin = TPSpeakerRend.gameObject.AddComponent<Spinner>();
            TPSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            speakerBaseScale = FPSpeakerRend.transform.localScale;

            try
            {
                ThirdPCam = FindObjectOfType<CinemachineBrain>()?.GetComponent<Camera>();
            }
            catch
            {
                // ignored as it would be a user fault for no that
            }
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
            voice = VRRigCache.rigsInUse[rig!.OwningNetPlayer].voiceView;
            baseCol = c;
            RefreshSpeakerRef();
        }

        /*──────────────────────────────────────────────────────────*/
        private void RefreshSpeakerRef()
        {
            /*───────────────────────── primary (Photon Voice) ─────────────────────────*/
            if (voice?.SpeakerInUse != null)
            {
                var src = voice.SpeakerInUse.GetComponent<AudioSource>();
                if (src)
                {
                    if (speakerSrc != src) // swapped lobby or first run
                    {
                        speakerSrc = src;
                        baseVolume = speakerSrc.volume;
                    }

                    return; // all good – nothing else to do
                }
            }

            /*───────────────────────── fallback (manual hierarchy) ────────────────────*/
            // rig → body/head/SpeakerHeadCollider/HeadSpeaker
            var t = transform.Find(
                "body/head/SpeakerHeadCollider/HeadSpeaker");

            if (!t) return;

            var manualSrc = t.GetComponent<AudioSource>();
            if (!manualSrc) return;

            // only update if we *changed* speaker
            if (speakerSrc != manualSrc)
            {
                speakerSrc = manualSrc;
                baseVolume = speakerSrc.volume;
            }
        }

        private void ApplyMicPulse()
        {
            var amp = 0f;

            if (voice?.RecorderInUse?.LevelMeter != null)
            {
                amp = voice.RecorderInUse.LevelMeter.CurrentPeakAmp;
            }
            else if (voice?.SpeakerInUse != null)
            {
                var src = voice.SpeakerInUse.GetComponent<AudioSource>();
                if (src != null && src.isPlaying)
                {
                    src.GetOutputData(audioSamples, 0);
                    double sum = 0;
                    foreach (var s in audioSamples)
                        sum += s * s;
                    amp = (float)Math.Sqrt(sum / audioSamples.Length);
                }
            }

            amp = Mathf.Clamp01(amp * Mod.MicPulseSensitivity.Value);

            var scale = Mathf.Lerp(Mod.MicPulseMinScale.Value,
                Mod.MicPulseMaxScale.Value,
                amp);

            var scl = speakerBaseScale * scale;
            FPSpeakerRend.transform.localScale = scl;
            TPSpeakerRend.transform.localScale = scl;
        }

        private void FadeRenderer(Renderer rend, bool visible, Color rgb, float speed)
        {
            var c = rend.material.color;
            var tgt = visible ? 1f : 0f;
            var a = Mathf.MoveTowards(c.a, tgt, speed * Time.deltaTime);

            rgb.a = a;
            rend.material.color = rgb;
            rend.forceRenderingOff = a <= 0.01f;
        }

        private Color ColourHandling()
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

        private bool CheckGhost()
        {
            if (rig.IsInvisibleToLocalPlayer || rig.bodyRenderer.cosmeticBodyType == GorillaBodyType.Invisible)
                return true;

            return false;
        }
    }
}