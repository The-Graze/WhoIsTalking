using System.Reflection;
using UnityEngine;

namespace WhoIsTalking
{
    public class AssetRef
    {
        public static readonly GameObject Tag;
        public static Shader shader = Shader.Find("UI/Default");

        static AssetRef()
        {
            using (var str = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhoIsTalking.Assets.speaker"))
            {
                var bundle = AssetBundle.LoadFromStream(str);
                if (bundle != null)
                {
                    Tag = bundle.LoadAsset<GameObject>("speaker");
                    bundle.UnloadAsync(false);
                }
            }
        }
    }
}