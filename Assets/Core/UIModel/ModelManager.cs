using huqiang.Data;
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
    public unsafe struct ElementHead
    {
        public Int32 ex0;
        public Int32 ex1;
        public static int Size = sizeof(ElementHead);
        public byte[] ToBytes()
        {
            byte[] buff = new byte[Size];
            fixed (byte* bp = &buff[0])
                *(ElementHead*)bp = this;
            return buff;
        }
    }
    public enum ComponentType
    {
        None = 0,
        CanvasRenderer = 0x1,//1
        RectTransform = 0x2,//2
        Image = 0x4,//4
        RawImage = 0x8,//8
        CustomRawImage = 0x10,//16
        Text = 0x20,//32
        Mask = 0x40,//64
        TextEx=0x80//128
    }
    public class PrefabAsset
    {
        public string name;
        public ModelElement[] models;
    }
    public class ModelManager
    {
        static ModelManager()
        {
            types[0] = typeof(CanvasRenderer);
            types[1] = typeof(RectTransform);
            types[2] = typeof(Image);
            types[3] = typeof(RawImage);
            types[4] = typeof(CustomRawImage);
            types[5] = typeof(Text);
            types[6] = typeof(Mask);
            types[7] = typeof(EmojiText);
        }
        static List<PrefabAsset> prefabAssets=new List<PrefabAsset>();
        static ModelElement[] prefabs;
        class UIBuffer
        {
            Type[] types;
            Action<GameObject> Reset;
            GameObject[] buff;
            int point, size;
            public int type;
            public UIBuffer(int type, int buffersize, Type[] typ, Action<GameObject> reset)
            {
                if (buffersize > 0)
                    buff = new GameObject[buffersize];
                point = 0;
                size = buffersize;
                this.type = type;
                types = typ;
                Reset = reset;
            }
            /// <summary>
            /// 找回或创建一个新的实例
            /// </summary>
            /// <returns></returns>
            public GameObject CreateNew()
            {
                if (point > 0)
                {
                    point--;
                    buff[point].SetActive(true);
                    var com = buff[point].GetComponents<UIBehaviour>();
                    for (int i = 0; i < com.Length; i++)
                        com[i].enabled = true;
                    if (Reset != null)
                        Reset(buff[point]);
                    return buff[point];
                }
                GameObject g = new GameObject("", types);
                if (Reset != null)
                    Reset(g);
                return g;
            }
            /// <summary>
            /// 回收一个实例
            /// </summary>
            /// <param name="ui"></param>
            public bool ReCycle(GameObject ui)
            {
                if (ui.name == "m_caret")
                    return true;
                if (point >= size)
                {
                    GameObject.Destroy(ui);
                    return false;
                }
                else
                {
                    if (Reset != null)
                        Reset(ui);
                    for (int i = 0; i < point; i++)
                        if (buff[i] == ui)
                            return false;//防止重复回收
                    ui.SetActive(false);
                    ui.name = "buff";
                    buff[point] = ui;
                    point++;
                    return true;
                }
            }
        }
        class Model
        {
            public int type;
            public Func<ModelElement> create;
        }
        static int point = 8;
        static Type[] types = new Type[31];//1-31
        static List<Model> models;
        static List<UIBuffer> diction;
        static void RegType(Type type)
        {
            if (point >= 31)
                return;
            for (int i = 1; i < 31; i++)
                if (types[i] == type)
                    return;
            types[point] = type;
            point++;
        }
        static void RegType(Type[] typ)
        {
            if (typ == null)
                return;
            for (int i = 0; i < typ.Length; i++)
                RegType(typ[i]);
        }
        static int GetTypeIndex(Type type)
        {
            if (type == typeof(CanvasRenderer))
                return 0;
            if (typeof(SizeScaling).IsAssignableFrom(type))
                return 0;
            for (int i = 1; i < point; i++)
            {
                if (type == types[i])
                {
                    int a = 1 << i;
                    return a;
                }
            }
            return -1;
        }
        public static int GetTypeIndex(Type[] typ)
        {
            if (typ == null)
                return -1;
            int a = 0;
            for (int i = 0; i < typ.Length; i++)
                a |= GetTypeIndex(typ[i]);
            return a;
        }
        public static int GetTypeIndex(Component[] com)
        {
            if (com == null)
                return 0;
            int a = 0;
            for (int i = 0; i < com.Length; i++)
            {
                var c = com[i];
                if (c == null)
                {
                    Debug.Log(com[0].name);
                }
                else
                    a |= GetTypeIndex(c.GetType());
            }
            return a;
        }
        /// <summary>
        /// 注册一个可供回收得模型
        /// </summary>
        /// <param name="create"></param>
        /// <param name="reset"></param>
        /// <param name="buffsize"></param>
        /// <param name="types"></param>
        public static void RegModel(Func<ModelElement> create, Action<GameObject> reset, int buffsize, params Type[] types)
        {
            RegType(types);
            int typ = GetTypeIndex(types);
            if (models == null)
            {
                models = new List<Model>();
                diction = new List<UIBuffer>();
            }else
            {
                for (int i = 0; i < models.Count; i++)
                {
                    if (typ == models[i].type)
                        return;
                }
            }
            Model model = new Model();
            model.create = create;
            model.type = typ;
            models.Add(model);
            diction.Add(new UIBuffer(typ, buffsize, types, reset));
        }
        public static void Initial()
        {
            RegModel(() => { return new ModelElement(); }, null, 32, typeof(RectTransform));
            RegModel(() => { return new TextElement(); }, ResetText, 32, typeof(RectTransform), typeof(Text));
            RegModel(() => { return new ImageElement(); }, ResetImage, 32, typeof(RectTransform), typeof(Image));
            RegModel(() => { return new RawImageElement(); }, ResetRawImage, 32, typeof(RectTransform), typeof(RawImage));
            RegModel(() => { return new RawImageElement(); }, ResetRawImage, 32, typeof(RectTransform), typeof(CustomRawImage));
            RegModel(() => { return new ViewportElement(); }, ResetImage, 32, typeof(RectTransform), typeof(Image), typeof(Mask));
            RegModel(() => { return new EmojiTextElement(); }, null, 32, typeof(RectTransform), typeof(EmojiText));
        }
        static void ResetText(GameObject game)
        {
            game.GetComponent<Graphic>().material = Graphic.defaultGraphicMaterial;
        }
        static void ResetImage(GameObject game)
        {
            var img = game.GetComponent<Image>();
            img.material = Graphic.defaultGraphicMaterial;
            img.sprite = null;
        }
        static void ResetRawImage(GameObject game)
        {
            var raw = game.GetComponent<RawImage>();
            raw.material = Graphic.defaultGraphicMaterial;
            raw.texture = null;
        }
        public static void SavePrefab(GameObject uiRoot, string path)
        {
            MemoryStream ms = new MemoryStream();
            MemoryStream ps = new MemoryStream();
            var t = uiRoot.transform;
            ElementHead head = new ElementHead();
            StringBuffer buffer = new StringBuffer();
            for (int i = 0; i < t.childCount; i++)
            {
                var son = t.GetChild(i);
                var str = buffer.AddString(son.name);
                head.ex0 = str;//预制体名称
                head.ex1 = (int)ms.Position;//预制体开始位置
                byte[] buff = head.ToBytes();
                ps.Write(buff, 0, buff.Length);
                SaveToFile(son.gameObject, ms,buffer);
            }
            byte[] bs = buffer.ToBytes();
            byte[] bl = buffer.buffer.Count.ToBytes();
            if (File.Exists(path))
                File.Delete(path);
            var all = File.Create(path);
            all.Write(bl, 0, 4);
            all.Write(bs, 0, bs.Length);

            bl = t.childCount.ToBytes();
            all.Write(bl, 0, 4);
            bs = ps.ToArray();
            all.Write(bs, 0, bs.Length);
            bs = ms.ToArray();
            all.Write(bs, 0, bs.Length);
            all.Dispose();
            ps.Dispose();
            ms.Dispose();
        }
        public unsafe static PrefabAsset LoadModels(byte[] buff,string name)
        {
            PrefabAsset asset = new PrefabAsset();
            StringBuffer buffer = new StringBuffer();
            fixed (byte* bp = &buff[0])
            {
                byte* p = buffer.LoadStringAsset(buff, bp);
                Int32 c = *(Int32*)p;
                p += 4;
                int len = c * ElementHead.Size;
                byte* os = len + p;
                prefabs = new ModelElement[c];
                for (int i = 0; i < c; i++)
                {
                    ElementHead head = *(ElementHead*)p;
                    p += ElementHead.Size;
                    address = head.ex1 + os;
                    prefabs[i] = LoadToModel(buffer);
                }
            }
            asset.models = prefabs;
            asset.name = name;
            for (int i = 0; i < prefabAssets.Count; i++)
                if (prefabAssets[i].name == name)
                { prefabAssets.RemoveAt(i); break; }
            prefabAssets.Add(asset);
            return asset;
        }
        unsafe static byte* address;
        unsafe static ModelElement LoadToModel(StringBuffer buffer)
        {
            ElementHead head = *(ElementHead*)address;
            address += ElementHead.Size;
            for (int i = 0; i < models.Count; i++)
            {
                if (head.ex0 == models[i].type)
                {
                    var mod = models[i].create();
                    mod.buffer = buffer;
                    address = mod.LoadFromBuff(address);
                    if(head.ex1>0)
                    for(int c=0;c<head.ex1;c++)
                    {
                        mod.Child.Add(LoadToModel(buffer));
                    }
                    return mod;
                }
             }
            return null;
        }
        public static void SaveToFile(GameObject game, Stream stream,StringBuffer buffer)
        {
            var com = game.GetComponents<Component>();
            int typ = GetTypeIndex(com);
            for (int i = 0; i < models.Count; i++)
            {
                if (typ == models[i].type)
                {
                    var mod = models[i].create();
                    mod.buffer = buffer;
                    mod.Save(game);
                    mod.transAttribute.type = typ;
                    var t = game.transform;
                    int c = t.childCount;
                    mod.transAttribute.childCount = c;
                    ElementHead head = new ElementHead();
                    head.ex0 = typ;//元素类型
                    head.ex1 = c;//子元素长度
                    var buff = head.ToBytes();
                    stream.Write(buff, 0, buff.Length);
                    buff = mod.ToBytes();
                    stream.Write(buff, 0, buff.Length);
                    for (int j = 0; j < c; j++)
                    {
                        var g = t.GetChild(j).gameObject;
                        SaveToFile(g, stream,buffer);
                    }
                }
            }
        }

        public static ModelElement GetMod(ModelElement mod, string name)
        {
            if (mod.tag == "mod")
                if (mod.name == name)
                    return mod;
            var c = mod.Child;
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
            for(int i=0;i<prefabAssets.Count;i++)
            {
                if(asset==prefabAssets[i].name)
                {
                    var models = prefabAssets[i].models;
                    for(int j=0;j<models.Length;j++)
                    {
                        if (name == models[j].name)
                            return models[j];
                    }
                }
            }
            return null;
        }
        public static ModelElement LoadToGame(string mod, object o, Transform parent, string filter = "mod")
        {
            if (prefabs == null)
                return null;
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (mod == prefabs[i].name)
                {
                    LoadToGame(prefabs[i], o, parent, filter);
                    return prefabs[i];
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
            var g = CreateNew((ComponentType)mod.transAttribute.type);
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
            mod.Load(g);
            mod.Main = g;
            var c = mod.Child;
            for (int i = 0; i < c.Count; i++)
                LoadToGame(c[i], o, t,filter);
            if (o != null)
                GetObject(t, o, mod);
            return g;
        }
        public static GameObject CreateNew(params Type[] types)
        {
            return CreateNew((ComponentType)GetTypeIndex(types));
        }
        public static GameObject CreateNew(ComponentType type)
        {
            if (type == ComponentType.None)
                return null;
            for (int i = 0; i < diction.Count; i++)
                if (type == (ComponentType)diction[i].type)
                    return diction[i].CreateNew();
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
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i].name == str)
                    return prefabs[i];
            }
            return null;
        }
     
        public static void CopyLayout(Transform transform, ModelElement model, bool isSon = false)
        {
            if (!isSon)
                ModelElement.Load(transform.gameObject, model);
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                var n = t.name;
                var m = model.FindChild(n);
                if (m != null)
                {
                    ModelElement.Load(t.gameObject, model);
                    CopyLayout(t, m);
                }
            }
        }
        public static void GetComponent(Transform t, object o)
        {
            if (o != null)
                GetObject(t, o, null);
            for (int i = 0; i < t.childCount; i++)
                GetComponent(t.GetChild(i), o);
        }
        public static object[] CreateGrid(RectTransform parent, ModelElement model, Vector2 itemSize, Vector2 offset, int maxCount, Type type)
        {
            var size = parent.sizeDelta;
            float wx = size.x - offset.x;
            int wm = (int)(wx / itemSize.x);
            if (wm == 0)
                wm = 1;
            float sx = offset.x + itemSize.x * 0.5f - size.x * 0.5f;
            float sy = size.y * 0.5f - offset.y - itemSize.y * 0.5f;
            object[] os = new object[maxCount];
            for (int i = 0; i < maxCount; i++)
            {
                int c = i / wm;
                int r = i % wm;
                float ox = sx + c * itemSize.x;
                float oy = sy - r * itemSize.y;
                object obj = null;
                if (type != null)
                    obj = Activator.CreateInstance(type);
                var g = LoadToGame(model, obj, null, "");
                if (obj == null)
                    os[i] = g;
                else os[i] = obj;
                var t = g.transform;
                t.SetParent(parent);
                t.localScale = Vector3.one;
                t.localPosition = new Vector3(ox, oy, 0);
            }
            return os;
        }
        public static ModelElement LoadToGameR(string mod, List<ReflectionModel> reflections, Transform parent, string filter = "mod")
        {
            if (prefabs == null)
                return null;
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (mod == prefabs[i].name)
                {
                    LoadToGameR(prefabs[i], reflections, parent, filter);
                    return prefabs[i];
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
            var g = CreateNew((ComponentType)mod.transAttribute.type);
            if (g == null)
            {
                return null;
            }
            var t = g.transform;
            if (parent != null)
                t.SetParent(parent);
            var c = mod.Child;
            for (int i = 0; i < c.Count; i++)
                LoadToGameR(c[i], reflections, t,filter);
            mod.Load(g);
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
            int type = GetTypeIndex(ts);
            if (type > 0)
            {
                for (int i = 0; i < diction.Count; i++)
                {
                    if (diction[i].type == type)
                    {
                        diction[i].ReCycle(game);
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
