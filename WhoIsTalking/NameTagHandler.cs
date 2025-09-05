using System;                               // for Math
using UnityEngine;
using UnityEngine.SceneManagement;          // ① NEW – scene check
using Photon.Pun;
using Photon.Voice.PUN;                 // Recorder.LevelMeter
using GorillaExtensions;
using JetBrains.Annotations;
using Unity.Cinemachine;

namespace WhoIsTalking
{
    public class NameTagHandler : MonoBehaviour
    {
        /* ───────── prefab parts ───────── */
        private GameObject _nameFp, _nameTp;
        private Camera _thirdPCam;

        private Vector3 _speakerBaseScale;               // original prefab scale

        /* ───────── Photon refs ───────── */
        public VRRig rig;
        private NetPlayer _player;
        private PhotonVoiceView _voice;

        /* ───────── helpers ───────── */
        private readonly Color _orange = new Color(1f, 0.3288f, 0f, 1f);
        private Color _baseCol = Color.black;
        private static readonly float[] AudioSamples = new float[256];    // reused buffer
        private Color _currentColour;
        private Renderer _fpRend, _tpRend, _fpSpeakerRend, _tpSpeakerRend;

        private Spinner _fpSpeakerSpin, _tpSpeakerSpin;
        private TextMesh _fpText, _tpText;

        /* ───────── proximity‑voice ───────── */
        private AudioSource _speakerSrc;
        private float _baseVolume = 1f;
        private int _lastViewID = -1;

        /* ───────── existing skin reference ───────── */
        private Transform _gorillaNew;

        /* ───────── ② NEW – quick Bayou helper ───────── */
        private static bool BayouActive()
        {
            var s = SceneManager.GetSceneByName("Bayou");
            return s.IsValid() && s.isLoaded;
        }

        /*──────────────────────────────────────────────────────────*/
        public void Start()
        {
            if (_nameFp == null && _nameTp == null)
                SetUpNameTag();

            RefreshInfo(_baseCol);                               // cache refs on first spawn

            /* existing ► cache reference to the player’s skin object */
            _gorillaNew = transform.root.Find("gorilla_new");

            _currentColour = ColourHandling();
            RefreshInfo(_baseCol);                               // cache refs on first spawn
        }

        /*──────────────────────────────────────────────────────────*/
        private void SetUpNameTag()
        {
            /* first‑person tag */
            SetUpNameTagInstance(ref _nameFp, "First Person NameTag", "FirstPersonOnly");
            _fpSpeakerRend = _nameFp.transform.GetChild(0).GetComponent<Renderer>();
            _fpSpeakerRend.material.shader = AssetRef.Shader;
            _fpRend = _nameFp.transform.GetChild(1).GetComponent<Renderer>();
            _fpRend.material.shader = AssetRef.Shader;
            _fpText = _fpRend.GetComponent<TextMesh>();

            _fpSpeakerSpin = _fpSpeakerRend.gameObject.AddComponent<Spinner>();
            _fpSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            /* third‑person / mirror tag */
            SetUpNameTagInstance(ref _nameTp, "Third Person NameTag", "MirrorOnly");
            _tpSpeakerRend = _nameTp.transform.GetChild(0).GetComponent<Renderer>();
            _tpSpeakerRend.material.shader = AssetRef.Shader;
            _tpRend = _nameTp.transform.GetChild(1).GetComponent<Renderer>();
            _tpRend.material.shader = AssetRef.Shader;
            _tpText = _tpRend.GetComponent<TextMesh>();

            _tpSpeakerSpin = _tpSpeakerRend.gameObject.AddComponent<Spinner>();
            _tpSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

            _speakerBaseScale = _fpSpeakerRend.transform.localScale;

            try { _thirdPCam = FindFirstObjectByType<CinemachineBrain>()?.GetComponent<Camera>(); }
            catch
            {
                // ignored
            }
        }
        private void SetUpNameTagInstance(ref GameObject nameTag, string goName, string layerName)
        {
            if (nameTag != null) return;
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
            _player = rig?.OwningNetPlayer;
            _voice = VRRigCache.rigsInUse[rig?.OwningNetPlayer!].voiceView;
            _baseCol = c;
            RefreshSpeakerRef();
        }

        /*──────────────────────────────────────────────────────────*/
        void RefreshSpeakerRef()
        {
            /*──────── primary (Photon Voice) ────────*/
            if (_voice?.SpeakerInUse != null)
            {
                PhotonView pv = _voice.SpeakerInUse.GetComponent<PhotonView>();
                AudioSource src = _voice.SpeakerInUse.GetComponent<AudioSource>();

                if (src != null)
                {
                    int id = (pv != null) ? pv.ViewID : -1;   // –1 when no PhotonView

                    if (_speakerSrc != src)        // swapped lobby or first run
                    {
                        _speakerSrc = src;
                        _baseVolume = _speakerSrc.volume;
                        _lastViewID = id;
                    }

                    return;   // all good – nothing else to do
                }
            }

            /*──────── fallback (manual hierarchy) ────────*/
            Transform t = transform.Find("body/head/SpeakerHeadCollider/HeadSpeaker");
            if (t == null) return;

            AudioSource manualSrc = t.GetComponent<AudioSource>();
            if (manualSrc == null) return;

            if (_speakerSrc != manualSrc)
            {
                _speakerSrc = manualSrc;
                _baseVolume = _speakerSrc.volume;
                _lastViewID = -1;
            }
        }

        /*──────── keep gorilla_new ref healthy ────────*/
        void RefreshGorillaNewRef()
        {
            if (_gorillaNew == null)
                _gorillaNew = transform.root.Find("gorilla_new");
        }

        /*──────────────────────────────────────────────────────────*/
        private void FixedUpdate()
        {
            RefreshGorillaNewRef();
            RefreshSpeakerRef();

            try
            {
                _fpSpeakerSpin.Speed = _tpSpeakerSpin.Speed = Mod.SpinnerSpeed.Value;

                var targetCol = ColourHandling();
                var colourSpeed = (Mod.ColourChangeTime.Value > 0f)
                                    ? Time.deltaTime / Mod.ColourChangeTime.Value
                                    : 1f;
                _currentColour = Color.Lerp(_currentColour, targetCol, colourSpeed);

                /*──── ③ NEW: fade tags while Bayou scene is loaded ────*/
                var bayou = BayouActive();

                var skinVisible = _gorillaNew == null || _gorillaNew.gameObject.activeInHierarchy;

                var dist = Vector3.Distance(transform.position, Camera.main!.transform.position);
                var withinRange = dist <= Mod.ViewDistance.Value.ClampSafe(0, 10);

                var showFpTag = !bayou && Mod.ShowFirstPersonTag.Value && withinRange && skinVisible;
                var showTpTag = !bayou && Mod.ShowThirdPersonTag.Value && withinRange && skinVisible;
                var speaking = !bayou && Mod.Speaker.Value && _voice.IsSpeaking;

                var showFpIcon = showFpTag && speaking;
                var showTpIcon = showTpTag && speaking;

                var fadeSpeed = (Mod.FadeTime.Value > 0f) ? 1f / Mod.FadeTime.Value : 1000f;

                FadeRenderer(_fpRend, showFpTag, _currentColour, fadeSpeed);
                FadeRenderer(_fpSpeakerRend, showFpIcon, _currentColour, fadeSpeed);
                FadeRenderer(_tpRend, showTpTag, _currentColour, fadeSpeed);
                FadeRenderer(_tpSpeakerRend, showTpIcon, _currentColour, fadeSpeed);

                /*──── mic‑pulse ────*/
                if (Mod.MicPulse.Value)
                    ApplyMicPulse();
                else
                {
                    _fpSpeakerRend.transform.localScale = _speakerBaseScale;
                    _tpSpeakerRend.transform.localScale = _speakerBaseScale;
                }

                /*──── proximity voice ────*/
                if (_speakerSrc != null)
                {
                    float targetVol;
                    if (Mod.ProximityVoiceChat.Value && !_player.IsLocal)
                        targetVol = (dist <= Mod.ViewDistance.Value.ClampSafe(0, 10)) ? _baseVolume : 0f;
                    else
                        targetVol = _baseVolume;

                    _speakerSrc.volume = Mathf.MoveTowards(
                        _speakerSrc.volume,
                        targetVol,
                        fadeSpeed * Time.deltaTime);
                }

                /*──── billboard & text ────*/
                _fpRend.transform.LookAt(Camera.main.transform.position);
                _tpRend.transform.LookAt((_thirdPCam != null)
                                        ? _thirdPCam.transform.position
                                        : Camera.main.transform.position);

                _fpText.text = _tpText.text = _player.NickName;
            }
            catch
            {
                RefreshInfo(_baseCol);          // regain refs if something went null
            }
        }

        /*──────────────────────────────────────────────────────────*/
        void ApplyMicPulse()
        {
            float amp = 0f;

            if (_voice?.RecorderInUse?.LevelMeter != null)
                amp = _voice.RecorderInUse.LevelMeter.CurrentPeakAmp;
            else if (_voice?.SpeakerInUse != null)
            {
                AudioSource src = _voice.SpeakerInUse.GetComponent<AudioSource>();
                if (src != null && src.isPlaying)
                {
                    src.GetOutputData(AudioSamples, 0);
                    double sum = 0;
                    foreach (float s in AudioSamples) sum += s * s;
                    amp = (float)Math.Sqrt(sum / AudioSamples.Length);
                }
            }

            amp = Mathf.Clamp01(amp * Mod.MicPulseSensitivity.Value);

            float scale = Mathf.Lerp(Mod.MicPulseMinScale.Value,
                                     Mod.MicPulseMaxScale.Value,
                                     amp);

            Vector3 scl = _speakerBaseScale * scale;
            _fpSpeakerRend.transform.localScale = scl;
            _tpSpeakerRend.transform.localScale = scl;
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
                    return _orange;
                case 3:
                case 7:
                    return Color.blue;
                case 12:
                    return Color.green;
                default:
                    return _baseCol;
            }
        }

        /*──────────────────────────────────────────────────────────*/
        void OnDisable()
        {
            if (_speakerSrc != null)
                _speakerSrc.volume = _baseVolume;   // leave source untouched for other mods
        }
    }
}
