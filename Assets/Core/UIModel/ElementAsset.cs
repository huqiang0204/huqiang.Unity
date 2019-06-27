using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace huqiang.UIModel
{
    public class ElementAsset
    {
        public class BundleInfo
        {
            public AssetBundle asset;
            public string name;
        }
        public static Progress LoadAssetsAsync(string name,Action<Progress,AssetBundleCreateRequest> callback=null)
        {
            Progress pro = new Progress();
            pro.name = name;
            pro.Play(LoadAssets(name));
            pro.PlayOver = callback;
            return pro;
        }
        public static AssetBundleCreateRequest LoadAssets(string name)
        {
            string path = Application.streamingAssetsPath + "/" + name;  // 其他平台
            return AssetBundle.LoadFromFileAsync(path);
        }
        public static List<BundleInfo> bundles = new List<BundleInfo>();
        public static T FindResource<T>(string bundle, string tname)where T:UnityEngine.Object
        {
            if (bundles == null)
                return null;
            for (int i = 0; i < bundles.Count; i++)
            {
                var tmp = bundles[i];
                if (bundle == tmp.name)
                {
                    return tmp.asset.LoadAsset<T>(tname);
                }
            }
            return null;
        }
        public static Texture FindTexture(string bundle, string tname)
        {
            if (bundle == null)
            {
                return UnityEngine.Resources.Load<Texture>(tname);
            }
            if (bundles == null)
                return null;
            for (int i = 0; i < bundles.Count; i++)
            {
                var tmp = bundles[i];
                if (bundle == tmp.name)
                {
                    return tmp.asset.LoadAsset<Texture>(tname);
                }
            }
            return null;
        }
        public static Sprite FindSprite(string bundle, string tname, string name)
        {
            if (bundle == null)
            {
                var ss = UnityEngine.Resources.LoadAll<Sprite>(tname);
                if (ss != null)
                {
                    for (int i = 0; i < ss.Length; i++)
                        if (ss[i].name == name)
                            return ss[i];
                }
                return null;
            }
            if (bundles == null)
                return null;
            for(int i=0;i<bundles.Count;i++)
            {
                var tmp = bundles[i];
                if(bundle==tmp.name)
                {
                    var sp = tmp.asset.LoadAssetWithSubAssets<Sprite>(tname);
                    for(int j = 0; j < sp.Length; j++)
                    {
                        if (sp[j].name == name)
                            return sp[j];
                    }
                    break;
                }
            }
            return null;
        }
        public static string TxtureFormAsset(string name)
        {
            if (bundles == null)
                return null;
            for(int i=0;i<bundles.Count;i++)
            {
                if (bundles[i].asset.LoadAsset<Texture>(name) != null)
                    return bundles[i].name;
            }
            return null;
        }
        public static AssetBundle FindBundle(string name)
        {
            if (bundles == null)
                return null;
            for (int i = 0; i < bundles.Count; i++)
                if (bundles[i].name == name)
                    return bundles[i].asset;
            return null;
        }
        public static void AddBundle(string name, AssetBundle asset)
        {
            BundleInfo b = new BundleInfo();
            b.name = name;
            b.asset = asset;
            bundles.Add(b);
        }
        public static void AddBundle(string name)
        {
            var dic = Application.streamingAssetsPath;
            dic += "/"+name;
            BundleInfo b = new BundleInfo();
            b.name = name;
            b.asset = AssetBundle.LoadFromFile(dic);
            bundles.Add(b);
        }
        public static void AddBundle(AssetBundle ab)
        {
            BundleInfo b = new BundleInfo();
            b.name = ab.name;
            b.asset = ab;
            bundles.Add(b);
        }
        public static Sprite[] FindSprites(string bundle, string tname, string[] names = null)
        {
            var bun = FindBundle(bundle);
            if (bun == null)
                return null;
            var sp = bun.LoadAssetWithSubAssets<Sprite>(tname);
            if (sp == null)
                return null;
            if (names == null)
                return sp;
            int len = names.Length;
            Sprite[] sprites = new Sprite[len];
            int c = 0;
            for (int i = 0; i < sp.Length; i++)
            {
                var s = sp[i];
                for (int j = 0; j < len; j++)
                {
                    if (s.name == names[j])
                    {
                        sprites[j] = s;
                        c++;
                        if (c >= len)
                            return sprites;
                        break;
                    }
                }
            }
            return sprites;
        }
    }
}