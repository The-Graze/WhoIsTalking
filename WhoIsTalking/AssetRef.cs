using System.IO;
using System.Reflection;
using UnityEngine;

namespace WhoIsTalking
{
    public class AssetRef
    {
        static Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhoIsTalking.speaker");
        static AssetBundle bundle = AssetBundle.LoadFromStream(str);
        public static GameObject Tag = bundle.LoadAsset<GameObject>("speaker");
        public static Shader shader = Shader.Find("UI/Default");
    }
}