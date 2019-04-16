using huqiang.Data;
using huqiang.Pool;
using huqiang.UIEvent;
using System;
using System.Collections.Generic;
using System.IO;
using UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UI
{
    public class PrefabAsset
    {
        public string name;
        public DataBuffer models;
    }
    public static class ModelManagerUI
    {
        struct TypeContext
        {
            public Type type;
            public Func<Component, bool> Compare;
            public Func<DataConversion> CreateConversion;
            public Func<Component, DataBuffer, FakeStruct> LoadFromObject;
        }
        static int point;
        /// <summary>
        /// 0-62,第63为负数位
        /// </summary>
        static TypeContext[] types = new TypeContext[63];
        /// <summary>
        /// 注册一个组件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Compare">不可空</param>
        /// <param name="create">可空</param>
        /// <param name="load">可空</param>
        public static void RegComponent(Type type, Func<Component, bool> Compare, Func<DataConversion> create, Func<Component, DataBuffer, FakeStruct> load)
        {
            if (point >= 63)
                return;
            for (int i = 0; i < point; i++)
                if (types[i].type == type)
                {
                    types[i].Compare = Compare;
                    types[i].CreateConversion = create;
                    types[i].LoadFromObject = load;
                    return;
                }
            types[point].type = type;
            types[point].Compare = Compare;
            types[point].CreateConversion = create;
            types[point].LoadFromObject = load;
            point++;
        }
        public static Int64 GetTypeIndex(Component com)
        {
            for (int i = 0; i < point; i++)
            {
                if (types[i].Compare(com))
                {
                    Int64 a = 1 << i;
                    return a;
                }
            }
            return 0;
        }
        public static Int64 GetTypeIndex(Component[] com)
        {
            if (com == null)
                return 0;
            Int64 a = 0;
            for (int i = 0; i < com.Length; i++)
            {
                var c = com[i];
                if (c != null)
                    a |= GetTypeIndex(c);
            }
            return a;
        }
        static int GetTypeIndex(Type type)
        {
            for (int i = 0; i < point; i++)
            {
                if (type == types[i].type)
                {
                    int a = 1 << i;
                    return a;
                }
            }
            return 0;
        }
        /// <summary>
        /// 根据所有类型生成一个id
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        public static int GetTypeIndex(Type[] typ)
        {
            if (typ == null)
                return 0;
            int a = 0;
            for (int i = 0; i < typ.Length; i++)
                a |= GetTypeIndex(typ[i]);
            return a;
        }

        public static void InitialComponent()
        {
            RegComponent(typeof(RectTransform), (o) => { return o is RectTransform; }, () => { return new ModelElement(); }, ModelElement.LoadFromObject);
            RegComponent(typeof(Image), (o) => { return o is Image; }, () => { return new ImageElement(); }, ImageElement.LoadFromObject);
            RegComponent(typeof(EmojiText), (o) => { return o is EmojiText; }, () => { return new TextElement(); }, TextElement.LoadFromObject);
            RegComponent(typeof(Text), (o) => { return o is Text; }, () => { return new TextElement(); }, TextElement.LoadFromObject);
            RegComponent(typeof(CustomRawImage), (o) => { return o is CustomRawImage; }, () => { return new RawImageElement(); }, RawImageElement.LoadFromObject);
            RegComponent(typeof(RawImage), (o) => { return o is RawImage; }, () => { return new RawImageElement(); }, RawImageElement.LoadFromObject);
            RegComponent(typeof(Mask), (o) => { return o is Mask; }, () => { return new MaskElement(); }, MaskElement.LoadFromObject);
            RegComponent(typeof(Outline), (o) => { return o is Outline; }, () => { return new OutLineElement(); }, OutLineElement.LoadFromObject);
        }
        public static void InitialModel()
        {
            RegModel(null, 32, typeof(RectTransform));
            RegModel(null, 32, typeof(RectTransform), typeof(Text));
            RegModel(null, 32, typeof(RectTransform), typeof(Image));
            RegModel(null, 32, typeof(RectTransform), typeof(RawImage));
            RegModel(null, 32, typeof(RectTransform), typeof(CustomRawImage));
            RegModel(null, 32, typeof(RectTransform), typeof(Image), typeof(Mask));
            RegModel(null, 32, typeof(RectTransform), typeof(EmojiText));
            RegModel(null, 32, typeof(RectTransform), typeof(Text), typeof(Outline));
        }
        static List<ModelBuffer> models = new List<ModelBuffer>();
        /// <summary>
        /// 注册一种模型的管理池
        /// </summary>
        /// <param name="reset">模型被重复利用时,进行重置,为空则不重置</param>
        /// <param name="buffsize">池子大小,建议32</param>
        /// <param name="types">所有的Component组件</param>
        public static void RegModel(Action<GameObject> reset, int buffsize, params Type[] types)
        {
            int typ = GetTypeIndex(types);
            for (int i = 0; i < models.Count; i++)
            {
                if (typ == models[i].type)
                    return;
            }
            ModelBuffer model = new ModelBuffer(typ, buffsize, reset, types);
            models.Add(model);
        }
        public static DataConversion Load(int type)
        {
            if (type < 0 | type >= point)
                return null;
            if (types[type].CreateConversion != null)
                return types[type].CreateConversion();
            return null;
        }
        public static FakeStruct LoadFromObject(Component com, DataBuffer buffer, ref Int16 type)
        {
            for (int i = 0; i < point; i++)
                if (types[i].Compare(com))
                {
                    type = (Int16)i;
                    if (types[i].LoadFromObject != null)
                        return types[i].LoadFromObject(com, buffer);
                    return null;
                }
            return null;
        }
        /// <summary>
        /// 将场景内的对象保存到文件
        /// </summary>
        /// <param name="uiRoot"></param>
        /// <param name="path"></param>
        public static void SavePrefab(GameObject uiRoot, string path)
        {
            DataBuffer db = new DataBuffer(1024);
            db.fakeStruct = ModelElement.LoadFromObject(uiRoot.transform, db);
            File.WriteAllBytes(path, db.ToBytes());
        }
        static List<PrefabAsset> prefabs = new List<PrefabAsset>();
        public unsafe static PrefabAsset LoadModels(byte[] buff, string name)
        {
            DataBuffer db = new DataBuffer(buff);
            var asset = new PrefabAsset();
            asset.models = db;
            asset.name = name;
            for (int i = 0; i < prefabs.Count; i++)
                if (prefabs[i].name == name)
                {
                    prefabs.RemoveAt(i);
                    break;
                }
            prefabs.Add(asset);
            return asset;
        }
        /// <summary>
        /// 查询一个模型数据,并实例化对象
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="mod"></param>
        /// <param name="o"></param>
        /// <param name="parent"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ModelElement LoadToElement(string asset, string mod)
        {
            for (int i = 0; i < prefabs.Count; i++)
            {
                if (asset == prefabs[i].name)
                {
                    var ms = prefabs[i].models.fakeStruct;
                    var fs = ModelElement.FindChild(ms, mod);
                    if (fs != null)
                    {
                        ModelElement element = new ModelElement();
                        element.Load(fs);
                        return element;
                    }
                    return null;
                }
            }
            return null;
        }
        public static ModelElement FindModel(string mod)
        {
            if (prefabs == null)
                return null;
            if (prefabs.Count > 0)
            {
                var ms = prefabs[0].models.fakeStruct;
                var fs = ModelElement.FindChild(ms, mod);
                if (fs != null)
                {
                    ModelElement element = new ModelElement();
                    element.Load(fs);
                    return element;
                }
            }
            return null;
        }
        #region 创建和回收
        public static GameObject CreateNew(Int64 type)
        {
            if (type == 0)
                return null;
            for (int i = 0; i < models.Count; i++)
                if (type == models[i].type)
                    return models[i].CreateObject();
            return null;
        }
        /// <summary>
        /// 挂载被回收得对象
        /// </summary>
        public static Transform CycleBuffer;
        static List<ModelElement> buffer=new List<ModelElement>();
        /// <summary>
        /// 回收一个对象，包括子对象
        /// </summary>
        /// <param name="game"></param>
        public static void RecycleElement(ModelElement u)
        {
            if (u.Context != null)
                buffer.Add(u);
            for (int i = 0; i < u.child.Count; i++)
                RecycleElement(u.child[i]);
        }
        static void Recycle()
        {
            for(int i=0;i<buffer.Count;i++)
            {
                Recycle(buffer[i]);
            }
            buffer.Clear();
        }
        static void Recycle(ModelElement u)
        {
           if(u.Context!=null)
            {
                for(int i=0;i<models.Count;i++)
                {
                    if(models[i].type==u.data.type)
                    {
                        models[i].ReCycle(u.Context.gameObject);
                        return;
                    }
                }
                GameObject.Destroy(u.Context.gameObject);
            }
        }
        #endregion
        public static void Initial()
        {
            InitialComponent();
            InitialModel();
            if (Application.platform == RuntimePlatform.Android | Application.platform == RuntimePlatform.IPhonePlayer)
            {
                UserInput.inputType = UserInput.InputType.OnlyTouch;
            }
            else
            {
                UserInput.inputType = UserInput.InputType.Blend;
            }
            if (Application.platform == RuntimePlatform.WindowsEditor | Application.platform == RuntimePlatform.WindowsPlayer)
            {
                IME.Initial();
            }
            var buff = new GameObject("buffer", typeof(Canvas));
            buff.SetActive(false);
            CycleBuffer = buff.transform;
        }
    }
}
