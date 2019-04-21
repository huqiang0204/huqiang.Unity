using huqiang.Data;
using huqiang.Manager2D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UGUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace huqiang.UIModel
{
    public class ReflectionModel
    {
        public string FieldName;
        public string TargetName;
        public Type FieldType;
        public object Value;
    }
    public abstract class ModelInital
    {
        public virtual void Initial(RectTransform rect, ModelElement mod) { }
    }
    public class ComponentType
    {
        public long None = 0;
        public long RectTransform = 0x1;
        public long Image = 0x2;//4
        public long RawImage = 0x4;//8
        public long CustomRawImage = 0x8;//16
        public long Text = 0x10;//32
        public long Mask = 0x20;//64
        public long TextEx = 0x40;//128
    }
    public class PrefabAsset
    {
        public string name;
        public ModelElement models;
    }
    public class ModelManagerUI
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
            asset.models = new ModelElement() ;
            asset.models.Load(db.fakeStruct);
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
        public static ModelElement GetMod(ModelElement mod, string name)
        {
            if (mod.tag == "mod")
                if (mod.name == name)
                    return mod;
            var c = mod.child;
            for (int i = 0; i < c.Count; i++)
            {
                var m = GetMod(c[i], name);
                if (m != null)
                    return m;
            }
            return null;
        }
        public static ModelElement FindModel(string asset,string name)
        {
            for(int i=0;i<prefabs.Count;i++)
            {
                if(asset==prefabs[i].name)
                {
                    var models = prefabs[i].models;
                    return models.FindChild(name);
                }
            }
            return null;
        }
        public static ModelElement LoadToGame(string asset, string mod, object o, Transform parent, string filter = "mod")
        {
            if (prefabs == null)
                return null;
            for (int i = 0; i < prefabs.Count; i++)
            {
                if (asset == prefabs[i].name)
                {
                    var m = prefabs[i].models.FindChild(mod);
                    LoadToGame(m, o, parent, filter);
                    return m;
                }
            }
            return null;
        }
        public static GameObject LoadToGame(ModelElement mod, object o, Transform parent, string filter="mod")
        {
            if (mod == null)
            {
#if DEBUG
                Debug.Log("Mod is null");
#endif
                return null;
            }
            if (mod.tag == filter)
            {
                return null;
            }
            var g = CreateNew(mod.data.type);
            if (g == null)
            {
#if DEBUG
                Debug.Log("Name:" + mod.name+ " is null");
#endif
                return null;
            }
            var t = g.transform;
            if (parent != null)
                t.SetParent(parent);
            mod.LoadToObject(t);
            mod.Main = g;
            var c = mod.child;
            for (int i = 0; i < c.Count; i++)
                LoadToGame(c[i], o, t,filter);
            if (o != null)
                GetObject(t, o, mod);
            return g;
        }
        public static GameObject CreateNew(params Type[] types)
        {
            return CreateNew(GetTypeIndex(types));
        }
        public static GameObject CreateNew(Int64 type)
        {
            if (type == 0)
                return null;
            for (int i = 0; i < models.Count; i++)
                if (type == models[i].type)
                    return models[i].CreateObject();
            return null;
        }
        static void GetObject(Transform t, object o, ModelElement mod)
        {
            var m = o.GetType().GetField(t.name);
            if (m != null)
            {
                if (m.FieldType == typeof(GameObject))
                    m.SetValue(o, t.gameObject);
                else if (typeof(EventCallBack).IsAssignableFrom(m.FieldType))
                    m.SetValue(o, EventCallBack.RegEventCallBack(t as RectTransform, m.FieldType));
                else if (typeof(ModelInital).IsAssignableFrom(m.FieldType))
                {
                    var obj = Activator.CreateInstance(m.FieldType) as ModelInital;
                    obj.Initial(t as RectTransform, mod);
                    m.SetValue(o, obj);
                }
                else if (typeof(Component).IsAssignableFrom(m.FieldType))
                    m.SetValue(o, t.GetComponent(m.FieldType));
            }
        }
        public static ModelElement FindModel(string str)
        {
            if (prefabs == null)
                return null;
            if (prefabs.Count > 0)
                return prefabs[0].models.FindChild(str);
            return null;
        }
        public static void GetComponent(Transform t, object o)
        {
            if (o != null)
                GetObject(t, o, null);
            for (int i = 0; i < t.childCount; i++)
                GetComponent(t.GetChild(i), o);
        }
        public static ModelElement LoadToGameR(string asset, string mod, List<ReflectionModel> reflections, Transform parent, string filter = "mod")
        {
            if (prefabs == null)
                return null;
            for (int i = 0; i < prefabs.Count; i++)
            {
                if (mod == prefabs[i].name)
                {
                    if (asset == prefabs[i].name)
                    {
                        var m = prefabs[i].models.FindChild(mod);
                        LoadToGameR(m,reflections, parent, filter);
                        return m;
                    }
                }
            }
            return null;
        }
        public static GameObject LoadToGameR(ModelElement mod, List<ReflectionModel> reflections, Transform parent, string filter = "mod")
        {
            if (mod == null)
            {
#if DEBUG
                Debug.Log("Mod is null");
#endif
                return null;
            }
            if (mod.tag == filter)
                return null;
            var g = CreateNew(mod.data.type);
            if (g == null)
            {
                return null;
            }
            var t = g.transform;
            if (parent != null)
                t.SetParent(parent);
            var c = mod.child;
            for (int i = 0; i < c.Count; i++)
                LoadToGameR(c[i], reflections, t,filter);
            mod.LoadToObject(t);
            mod.Main = g;
            if (reflections != null)
                GetObject(t, reflections, mod);
            return g;
        }
        public static void GetComponent(Transform t, List<ReflectionModel> reflections)
        {
            if (reflections != null)
                GetObject(t, reflections, null);
            for (int i = 0; i < t.childCount; i++)
                GetComponent(t.GetChild(i), reflections);
        }
        static void GetObject(Transform t, List<ReflectionModel> reflections, ModelElement mod)
        {
            for (int i = 0; i < reflections.Count; i++)
            {
                var m = reflections[i];
                if (m.TargetName == t.name)
                {
                    if (m.FieldType == typeof(GameObject))
                        m.Value = t.gameObject;
                    else if (typeof(EventCallBack).IsAssignableFrom(m.FieldType))
                        m.Value = EventCallBack.RegEventCallBack(t as RectTransform, m.FieldType);
                    else if (typeof(ModelInital).IsAssignableFrom(m.FieldType))
                    {
                        var obj = Activator.CreateInstance(m.FieldType) as ModelInital;
                        obj.Initial(t as RectTransform, mod);
                        m.Value = obj;
                    }
                    else if (typeof(Component).IsAssignableFrom(m.FieldType))
                        m.Value = t.GetComponent(m.FieldType);
                    break;
                }
            }
        }
        /// <summary>
        /// 挂载被回收得对象
        /// </summary>
        public static Transform CycleBuffer;
        /// <summary>
        /// 回收一个对象，包括子对象
        /// </summary>
        /// <param name="game"></param>
        public static void RecycleGameObject(GameObject game)
        {
            if (game == null)
                return;
            var rect = game.GetComponent<RectTransform>();
            if (rect != null)
                EventCallBack.ReleaseEvent(rect);
            var ts = game.GetComponents<Component>();
           long type = GetTypeIndex(ts);
            if (type > 0)
            {
                for (int i = 0; i < models.Count; i++)
                {
                    if (models[i].type == type)
                    {
                        models[i].ReCycle(game);
                        break;
                    }
                }
            }
            var p = game.transform;
            for (int i = p.childCount - 1; i >= 0; i--)
                RecycleGameObject(p.GetChild(i).gameObject);
            if (type > 0)
                p.SetParent(CycleBuffer);
            else GameObject.Destroy(game);

        }
        public static void RecycleSonObject(GameObject game)
        {
            if (game == null)
                return;
            var p = game.transform;
            for (int i = p.childCount - 1; i >= 0; i--)
                RecycleGameObject(p.GetChild(i).gameObject);
        }
    }
}
