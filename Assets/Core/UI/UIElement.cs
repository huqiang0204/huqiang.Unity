using huqiang.Data;
using huqiang.UIEvent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace huqiang.UI
{
    public unsafe struct ElementData
    {
        public Int64 type;
        public Int32 childCount;
        public Int32 name;
        public Int32 tag;
        public Quaternion localRotation;
        public Vector3 localPosition;
        public Vector3 localScale;
        public Vector2 anchoredPosition;
        public Vector3 anchoredPosition3D;
        public Vector2 anchorMax;
        public Vector2 anchorMin;
        public Vector2 offsetMax;
        public Vector2 offsetMin;
        public Vector2 pivot;
        public Vector2 sizeDelta;
        public bool SizeScale;
        public ScaleType scaleType;
        public SizeType sizeType;
        public AnchorType anchorType;
        public ParentType parentType;
        public Margin margin;
        public Vector2 DesignSize;
        /// <summary>
        /// int32数组,高16位为索引,低16位为类型
        /// </summary>
        public Int32 coms;
        /// <summary>
        /// int16数组
        /// </summary>
        public Int32 child;
        public static int Size = sizeof(ElementData);
        public static int ElementSize = Size / 4;
    }
    public class UIElement:DataConversion
    {
        public RectTransform Context;
        public int regIndex;
        public ElementData data;
        public string name;
        public string tag;
        public UIElement parent { get; private set; }
        public List<DataConversion> components = new List<DataConversion>();
        public List<UIElement> child = new List<UIElement>();
        public void AddChild(UIElement element)
        {
            if (element.parent != null)
                element.parent.child.Remove(element);
            child.Add(element);
            element.parent = this;
        }
        public FakeStruct Model;
        public unsafe static FakeStruct FindChild(FakeStruct fake,string name)
        {
            var data = (ElementData*)fake.ip;
            var buff = fake.buffer;
            Int16[] chi = buff.GetData(data->child) as Int16[];
            if (chi != null)
            {
                for (int i = 0; i < chi.Length; i++)
                {
                    var fs = buff.GetData(chi[i]) as FakeStruct;
                    if (fs != null)
                    {
                        var son = (ElementData*)fake.ip;
                       var n = buff.GetData(son->name) as string;
                        if (n == name)
                            return fs;
                    }
                }
            }
            return null;
        }
        unsafe public override void Load(FakeStruct fake)
        {
            data = *(ElementData*)fake.ip;
            var buff = fake.buffer;
            Int32[] coms = buff.GetData(data.coms) as Int32[];
            if (coms != null)
            {
                for (int i = 0; i < coms.Length; i++)
                {
                    int index = coms[i];
                    i++;
                    int type = 1 << coms[i];
                    var fs = buff.GetData(index) as FakeStruct;
                    if (fs != null)
                    {
                        var dc = ModelManagerUI.Load(type);
                        if (dc != null)
                        {
                            dc.Load(fs);
                            components.Add(dc);
                        }
                    }
                }
            }
            Int16[] chi = fake.buffer.GetData(data.child) as Int16[];
            if (chi != null)
                for (int i = 0; i < chi.Length; i++)
                {
                    var fs = buff.GetData(chi[i]) as FakeStruct;
                    if (fs != null)
                    {
                        UIElement model = new UIElement();
                        model.Load(fs);
                        child.Add(model);
                        model.parent = this;
                    }
                }
            name = buff.GetData(data.name) as string;
            tag = buff.GetData(data.tag) as string;
            Model = fake;
        }
        public override void LoadToObject(Component com)
        {
            for (int i = 0; i < components.Count; i++)
                if (components[i] != null)
                    components[i].LoadToObject(com);
            Context = com as RectTransform;
            LoadToObject(Context,ref data, this);
        }
        static void LoadToObject(RectTransform com,ref ElementData data,UIElement ui)
        {
            var trans = com as RectTransform;
            trans.localRotation = data.localRotation;
            trans.localPosition = data.localPosition;
            trans.localScale = data.localScale;
            trans.anchoredPosition = data.anchoredPosition;
            trans.anchoredPosition3D = data.anchoredPosition3D;
            trans.anchorMax = data.anchorMax;
            trans.anchorMin = data.anchorMin;
            trans.offsetMax = data.offsetMax;
            trans.offsetMin = data.offsetMin;
            trans.pivot = data.pivot;
            trans.sizeDelta = data.sizeDelta;
            trans.name = ui.name;
            trans.tag = ui.tag;
        }
        public static unsafe FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var trans = com as RectTransform;
            if (trans == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, ElementData.ElementSize);
            ElementData* ed = (ElementData*)fake.ip;
            ed->localRotation = trans.localRotation;
            ed->localPosition = trans.localPosition;
            ed->localScale = trans.localScale;
            ed->anchoredPosition = trans.anchoredPosition;
            ed->anchoredPosition3D = trans.anchoredPosition3D;
            ed->anchorMax = trans.anchorMax;
            ed->anchorMin = trans.anchorMin;
            ed->offsetMax = trans.offsetMax;
            ed->offsetMin = trans.offsetMin;
            ed->pivot = trans.pivot;
            ed->sizeDelta = trans.sizeDelta;
            var coms = com.GetComponents<Component>();
            ed->type = ModelManagerUI.GetTypeIndex(coms);
            List<Int16> tmp = new List<short>();
            for (int i = 0; i < coms.Length; i++)
            {
                var rect = coms[i] as RectTransform;
                if(rect == null)
                {
                    var ss = coms[i] as SizeScaling;
                    if(ss != null)
                    {
                        ed->SizeScale = true;
                        ed->scaleType = ss.scaleType;
                        ed->sizeType = ss.sizeType;
                        ed->anchorType = ss.anchorType;
                        ed->parentType = ss.parentType;
                        ed->margin = ss.margin;
                        ed->DesignSize = ss.DesignSize;
                    }
                    else if (!(coms[i] is CanvasRenderer))
                    {
                        Int16 type = 0;
                        var fs = ModelManagerUI.LoadFromObject(coms[i], buffer, ref type);
                        tmp.Add((Int16)buffer.AddData(fs));
                        tmp.Add(type);
                    }
                }
            }
            ed->coms = buffer.AddData(tmp.ToArray());
            int c = trans.childCount;
            if (c > 0)
            {
                Int16[] buf = new short[c];
                for (int i = 0; i < c; i++)
                {
                    var fs = LoadFromObject(trans.GetChild(i), buffer);
                    buf[i] = (Int16)buffer.AddData(fs);
                }
                ed->child = buffer.AddData(buf);
            }
            return fake;
        }
        public UIElement FindChild(string name)
        {
            for (int i = 0; i < child.Count; i++)
                if (child[i].name == name)
                    return child[i];
            return null;
        }
#if UNITY_EDITOR
        public void AddSizeScale()
        {
            if (data.SizeScale)
            {
                if (Main != null)
                {
                    var scale = Main.GetComponent<SizeScaleEx>();
                    if (scale == null)
                        scale = Main.AddComponent<SizeScaleEx>();
                    scale.scaleType = data.scaleType;
                    scale.sizeType = data.sizeType;
                    scale.anchorType = data.anchorType;
                    scale.parentType = data.parentType;
                    scale.margin = data.margin;
                    scale.DesignSize = data.DesignSize;
                }
            }
        }
#endif

        #region 尺寸自适应
        public static Vector2[] Anchors = new[] { new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f),new Vector2(1, 0.5f),
        new Vector2(0.5f, 1),new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1)};
        public static void Docking(UIElement rect, ScaleType dock, Vector2 pSize, Vector2 ds)
        {
            switch (dock)
            {
                case ScaleType.None:
                    rect.data.localScale = Vector3.one;
                    break;
                case ScaleType.FillX:
                    float sx = pSize.x / ds.x;
                    rect.data.localScale = new Vector3(sx, sx, sx);
                    break;
                case ScaleType.FillY:
                    float sy = pSize.y / ds.y;
                    rect.data.localScale = new Vector3(sy, sy, sy);
                    break;
                case ScaleType.FillXY:
                    sx = pSize.x / ds.x;
                    sy = pSize.y / ds.y;
                    if (sx < sy)
                        rect.data.localScale = new Vector3(sx, sx, sx);
                    else rect.data.localScale = new Vector3(sy, sy, sy);
                    break;
                case ScaleType.Cover:
                    sx = pSize.x / ds.x;
                    sy = pSize.y / ds.y;
                    if (sx < sy)
                        rect.data.localScale = new Vector3(sy, sy, sy);
                    else rect.data.localScale = new Vector3(sx, sx, sx);
                    break;
            }
        }
        public static void Anchor(UIElement rect, Vector2 pivot, Vector2 offset)
        {
            Vector2 p;
            Vector2 pp = new Vector2(0.5f, 0.5f);
            if (rect.parent != null)
            {
                var t = rect.parent;
                p = t.data.sizeDelta;
                pp = t.data.pivot;
            }
            else { p = new Vector2(Screen.width, Screen.height); }
            rect.data.localScale = Vector3.one;
            float sx = p.x * (pivot.x - 0.5f);
            float sy = p.y * (pivot.y - 0.5f);
            float ox = sx + offset.x;
            float oy = sy + offset.y;
            rect.data.localPosition = new Vector3(ox, oy, 0);
        }
        public static void AnchorEx(UIElement rect, AnchorType type, Vector2 offset, Vector2 p, Vector2 psize)
        {
            AnchorEx(rect, Anchors[(int)type], offset, p, psize);
        }
        public static void AnchorEx(UIElement rect, Vector2 pivot, Vector2 offset, Vector2 parentPivot, Vector2 parentSize)
        {
            float ox = (parentPivot.x - 1) * parentSize.x;//原点x
            float oy = (parentPivot.y - 1) * parentSize.y;//原点y
            float tx = ox + pivot.x * parentSize.x;//锚点x
            float ty = oy + pivot.y * parentSize.y;//锚点y
            offset.x += tx;//偏移点x
            offset.y += ty;//偏移点y
            rect.data.localPosition = new Vector3(offset.x, offset.y, 0);
        }
        public static void MarginEx(UIElement rect, Margin margin, Vector2 parentPivot, Vector2 parentSize)
        {
            float w = parentSize.x - margin.left - margin.right;
            float h = parentSize.y - margin.top - margin.down;
            var m_pivot = rect.data.pivot;
            float ox = w * m_pivot.x - parentPivot.x * parentSize.x + margin.left;
            float oy = h * m_pivot.y - parentPivot.y * parentSize.y + margin.down;
            float sx = rect.data.localScale.x;
            float sy = rect.data.localScale.y;
            rect.data.sizeDelta = new Vector2(w / sx, h / sy);
            rect.data.localPosition = new Vector3(ox, oy, 0);
        }
        public static Vector2 AntiAnchorEx(Vector2 tp, AnchorType type, Vector2 p, Vector2 psize)
        {
            return AntiAnchorEx(tp, Anchors[(int)type], p, psize);
        }
        public static Vector2 AntiAnchorEx(Vector2 tp, Vector2 pivot, Vector2 parentPivot, Vector2 parentSize)
        {
            float ox = (parentPivot.x - 1) * parentSize.x;//原点x
            float oy = (parentPivot.y - 1) * parentSize.y;//原点y
            float tx = ox + pivot.x * parentSize.x;//锚点x
            float ty = oy + pivot.y * parentSize.y;//锚点y
            return new Vector2(tx - tp.x, ty - tp.y);
        }
        public static Margin AntiMarginEx(Vector3 p, Vector2 tp, Vector2 tsize, Vector3 ts, Vector2 psize)
        {
            float w = tsize.x * ts.x;
            float h = tsize.y * ts.y;
            float left = (tp.x - 1) * w;
            float right = (1 - tp.x) * w;
            float down = (tp.y - 1) * h;
            float top = (1 - tp.y) * h;
            float hw = psize.x * 0.5f;
            float hh = psize.y * 0.5f;
            return new Margin(left - hw, hw - right, hh - top, down - hh);
        }
        static void Resize(UIElement ele)
        {
            if (ele.Main == null)
                return;
            var transform = ele.Main.transform;
            Vector2 size;
            Vector2 p = Anchors[0];
            if (ele.data.parentType == ParentType.Tranfrom)
            {
                var t = (transform.parent as RectTransform);
                size = t.sizeDelta;
                p = t.pivot;
            }
            else
            {
                var t = transform.root as RectTransform;
                size = t.sizeDelta;
            }

            RectTransform rect = transform as RectTransform;
            Docking(ele, ele.data.scaleType, size, ele.data.DesignSize);
            if (ele.data.sizeType == SizeType.Anchor)
            {
                AnchorEx(ele, ele.data.anchorType,
                    new Vector2(ele.data.margin.left, ele.data.margin.right), p, size);
            }
            else if (ele.data.sizeType == SizeType.Margin)
            {
                var mar = ele.data.margin;
                if (ele.data.parentType == ParentType.BangsScreen)
                    if (Scale.ScreenHeight / Scale.ScreenWidth > 2f)
                        mar.top += 88;
                MarginEx(ele, mar, p, size);
            }
            else if (ele.data.sizeType == SizeType.MarginRatio)
            {
                var mar = new Margin();
                mar.left = ele.data.margin.left * size.x;
                mar.right = ele.data.margin.right * size.x;
                mar.top = ele.data.margin.top * size.y;
                mar.down = ele.data.margin.down * size.y;
                if (ele.data.parentType == ParentType.BangsScreen)
                    if (Scale.ScreenHeight / Scale.ScreenWidth > 2f)
                        mar.top += 88;
                MarginEx(ele, mar, p, size);
            }
        }
        public static void ScaleSize(UIElement element)
        {
            if (element.data.SizeScale)
                Resize(element);
            var child = element.child;
            for (int i = 0; i < child.Count; i++)
            {
                ScaleSize(child[i]);
            }
        }
        #endregion

        public bool activeSelf;
        public BaseEvent baseEvent;
        public void RegEvent<T>()where T:BaseEvent,new()
        {
            if (baseEvent != null)
            {
                var t = baseEvent as T;
                if (t != null)
                {
                    BaseEvent.RegEvent<T>(t);
                    return;
                }
            }
            baseEvent = BaseEvent.RegEvent<T>(this);
        }
        public override void Apply()
        {
            if (Context == null)
            {
                var obj = ModelManagerUI.CreateNew(data.type);
                LoadToObject(obj.transform);
            }
            else if (IsChanged)
                LoadToObject(Context, ref data, this);
            for (int i = 0; i < components.Count; i++)
                if (components[i] != null)
                    if (components[i].IsChanged)
                        components[i].Apply();
        }
        public void Recycle()
        {

        }
    }
}
