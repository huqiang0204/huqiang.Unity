using huqiang.Manager2D;
using huqiang.UIModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UGUI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Element2DCreate), true)]
[CanEditMultipleObjects]
public class Element2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();
        Element2DCreate ele = target as Element2DCreate;
        if (GUILayout.Button("Clear All AssetBundle"))
        {
            AssetBundle.UnloadAllAssetBundles(true);
            ElementAsset.bundles.Clear();
            EditorModelManager2D.Clear();
        }
        if (GUILayout.Button("Create"))
        {
            Create(ele.Assetname, ele.dicpath, ele.gameObject);
        }
        if (GUILayout.Button("Clone"))
        {
            if (ele.bytesUI != null)
                Clone(ele.Assetsname, ele.CloneName, ele.bytesUI.bytes, ele.transform);
        }
        if (GUILayout.Button("CloneAll"))
        {
            if (ele.bytesUI != null)
                CloneAll(ele.bytesUI.bytes, ele.transform);
        }
        serializedObject.ApplyModifiedProperties();
    }
    static void LoadBundle()
    {
        if (ElementAsset.bundles.Count == 0)
        {
            var dic = Application.dataPath + "/StreamingAssets";
            if (Directory.Exists(dic))
            {
                var bs = Directory.GetFiles(dic, "*.unity3d");
                for (int i = 0; i < bs.Length; i++)
                {
                    var ass = AssetBundle.LoadFromFile(bs[i]);
                    ElementAsset.AddBundle(ass.name, ass);
                }
            }
        }

    }
    static void Create(string Assetname, string dicpath, GameObject gameObject)
    {
        if (Assetname == null)
            return;
        if (Assetname == "")
            return;
        LoadBundle();
        Assetname = Assetname.Replace(" ", "");
        ModelManager2D.Initial();
        var dc = dicpath;
        if (dc == null | dc == "")
        {
            dc = Application.dataPath + "/AssetsBundle/";
        }
        ModelManager2D.SavePrefab(gameObject.transform, dc + Assetname);
        Debug.Log("create done");
    }
    static void Clone(string Assetsname, string CloneName, byte[] ui, Transform root)
    {
        if (ui != null)
        {
            if (Assetsname != null)
                if (CloneName != null)
                    if (CloneName != "")
                    {
                        LoadBundle();
                        ModelManager2D.Initial();
                        ModelManager2D.LoadModels(ui, "assTest");
                        EditorModelManager2D.LoadToGame(Assetsname, CloneName, null, root, "");
                    }

        }
    }
    static void CloneAll(byte[] ui, Transform root)
    {
        if (ui != null)
        {
            LoadBundle();
            ModelManager2D.Initial();
            var all = ModelManager2D.LoadModels(ui, "assTest");
            var models = all.models.child;
            for (int i = 0; i < models.Count; i++)
                EditorModelManager2D.LoadToGame(models[i], null, root, "");
        }
    }
}
