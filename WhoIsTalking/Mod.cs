using BepInEx;
using BepInEx.Configuration;
using WhoIsTalking.Patches;

namespace WhoIsTalking
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Mod : BaseUnityPlugin
    {
        /*───────────────────────────────────────────────────────────*
         *  Config entries                                           *
         *───────────────────────────────────────────────────────────*/
        public static ConfigEntry<bool> Speaker;
        public static ConfigEntry<float> ViewDistance;
        public static ConfigEntry<bool> ShowFirstPersonTag;
        public static ConfigEntry<bool> ShowThirdPersonTag;
        public static ConfigEntry<float> SpinnerSpeed;
        public static ConfigEntry<float> FadeTime;
        public static ConfigEntry<float> ColourChangeTime;

        // live mic-pulse controls
        public static ConfigEntry<bool> MicPulse;
        public static ConfigEntry<float> MicPulseMinScale;
        public static ConfigEntry<float> MicPulseMaxScale;
        public static ConfigEntry<float> MicPulseSensitivity;

        /* NEW ─────────────────────────────────────────────────────*/
        public static ConfigEntry<bool> ProximityVoiceChat;

        /*───────────────────────────────────────────────────────────*/
        public Mod()
        {
            HarmonyP.ApplyPatches();

            Speaker = Config.Bind("Settings", "Show Speaker Icon", true,
                "Display the animated microphone icon while a player is speaking.");

            ViewDistance = Config.Bind("Settings", "First-Person Tag Distance", 5f,
                "Maximum distance (metres) the first-person name-tag is rendered. (clammed to 10 to stop advatage gain)");

            ShowFirstPersonTag = Config.Bind("Settings", "Show First-Person Name-Tag", true,
                "Master toggle for the name-tag rendered in the player camera.");

            ShowThirdPersonTag = Config.Bind("Settings", "Show Third-Person Name-Tag", true,
                "Master toggle for the name-tag rendered for mirrors / external cams.");

            SpinnerSpeed = Config.Bind("Settings", "Speaker Icon Spin Speed", 0.7f,
                "Rotation speed of the speaker icon (revolutions per second).");

            FadeTime = Config.Bind("Settings", "Fade Duration", 0.30f,
                "Seconds taken for tags/icons to fade in or out. Set to 0 for instant.");

            ColourChangeTime = Config.Bind("Settings", "Colour Transition Duration", 2.5f,
                "Seconds taken for the name-tag colour to adjust when it changes. Set to 0 for instant.");

            /*──────────────── Mic-pulse settings ───────────────────*/
            MicPulse = Config.Bind("Mic-Pulse", "Enable Mic Pulse", true,
                "If true, the speaker icon scales with the player's voice loudness.");

            MicPulseMinScale = Config.Bind("Mic-Pulse", "Min Scale", 0.7f,
                "Scale multiplier when the player is silent / whispering.");

            MicPulseMaxScale = Config.Bind("Mic-Pulse", "Max Scale", 1.45f,
                "Scale multiplier at maximum loudness.");

            MicPulseSensitivity = Config.Bind("Mic-Pulse", "Sensitivity", 7f,
                "Multiplier applied to Photon Voice amplitude before mapping to scale.\n" +
                "Raise this if the pulse feels too weak, lower if it spikes too easily.");

            /*──────────────── Proximity-voice setting ──────────────*/
            ProximityVoiceChat = Config.Bind("Settings", "Proximity Voice Chat", false,
                "If enabled, remote players’ voices fade in when they’re within the\n" +
                "“First-Person Tag Distance” and fade out when they leave it.\n" +
                "Fade speed uses the same “Fade Duration” setting.");
        }
    }
}