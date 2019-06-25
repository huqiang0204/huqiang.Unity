using huqiang.UIModel;
using UnityEngine;

namespace UGUI
{
    public class Element2DCreate : MonoBehaviour
    {
        public TextAsset bytesUI;
        public void ClearAllAssetBundle()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            ElementAsset.bundles.Clear();
        }
        public string dicpath;
        public string Assetname = "prefabs";
        public string Assetsname;
        public string CloneName;
    }
}
