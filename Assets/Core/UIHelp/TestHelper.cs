﻿using huqiang;
using huqiang.Data;
using huqiang.UIModel;
using System.IO;
using UGUI;
using UnityEngine;
using UnityEngine.UI;

public class TestHelper:UICompositeHelp
{
    public static void InitialUI()
    {
        ModelManagerUI.RegComponent(new ComponentType<RectTransform, ModelElement>(ModelElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<Image, ImageElement>(ImageElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<EmojiText, TextElement>(TextElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<Text, TextElement>(TextElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<CustomRawImage, RawImageElement>(RawImageElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<RawImage, RawImageElement>(RawImageElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<RectMask2D, RectMaskElement>(RectMaskElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<Mask, MaskElement>(MaskElement.LoadFromObject));
        ModelManagerUI.RegComponent(new ComponentType<Outline, OutLineElement>(OutLineElement.LoadFromObject));
    }
    public virtual void LoadBundle()
    {
#if UNITY_EDITOR
        if (ElementAsset.bundles.Count == 0)
        {
            //var dic = Application.dataPath + "/StreamingAssets";
            var dic = Application.streamingAssetsPath;
            if (Directory.Exists(dic))
            {
                var bs = Directory.GetFiles(dic, "*.unity3d");
                for (int i = 0; i < bs.Length; i++)
                {
                    ElementAsset.AddBundle(AssetBundle.LoadFromFile(bs[i]));
                }
            }
        }
#endif
    }
    public string AssetName = "baseUI";
    private void Awake()
    {
        LoadBundle();
        InitialUI();
        DataBuffer db = new DataBuffer(1024);
        db.fakeStruct = ModelElement.LoadFromObject(transform, db);
        PrefabAsset asset = new PrefabAsset();
        asset.models = new ModelElement();
        asset.models.Load(db.fakeStruct);
        asset.name = AssetName;
        ModelManagerUI.prefabs.Add(asset);
        var c = transform.childCount;
        for (int i = 0; i < c; i++)
            GameObject.Destroy(transform.GetChild(i).gameObject);
        App.Initial(transform as RectTransform);
        LoadTestPage();
    }

    public virtual void LoadTestPage()
    {
    }
    private void Update()
    {
        App.Update();
        OnUpdate();
    }
    public virtual void OnUpdate()
    {
    }
    private void OnDestroy()
    {
        App.Dispose();
        AssetBundle.UnloadAllAssetBundles(true);
        ElementAsset.bundles.Clear();
    }
}
