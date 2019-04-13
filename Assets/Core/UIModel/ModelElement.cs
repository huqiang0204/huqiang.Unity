using huqiang.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UGUI;
using UnityEngine;


namespace huqiang.UIModel
{
    public unsafe struct ElementAttribute
    {
        public Int32 childCount;
        public Int32 name;
        public Int32 tag;
        public Vector3 localEulerAngles;
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
        public Int32 type;
        public static int Size = sizeof(ElementAttribute);
        public static int ElementSize = Size / 4;
        public static void LoadFromBuff(ref ElementAttribute ele, void* p)
        {
            fixed (Int32* trans = &ele.childCount)
            {
                Int32* a = trans;
                Int32* b = (Int32*)p;
                for (int i = 0; i <ElementSize; i++)
                {
                    *a = *b;
                    a++;
                    b++;
                }
            }
        }
    }
    public class ModelElement
    {
        public string name;
        public string tag;
        public StringBuffer buffer;
        public ElementAttribute transAttribute;

        public unsafe virtual byte* LoadFromBuff(byte* point)
        {
            transAttribute = *(ElementAttribute*)point;
            ElementAttribute.LoadFromBuff(ref transAttribute,point);
            name = buffer[transAttribute.name];
            tag = buffer[transAttribute.tag];
            return point + ElementAttribute.Size;
        }
        public unsafe virtual byte[] ToBytes()
        {
            int size = ElementAttribute.Size;
            byte[] buff = new byte[size];
            fixed (byte* bp = &buff[0])
                *(ElementAttribute*)bp = transAttribute;
            return buff;
        }

        public static void Load(GameObject tar, ModelElement  model)
        {
            var r = tar.transform;
            if (r is RectTransform)
            {
                var t = r as RectTransform;
                t.pivot = model.transAttribute.pivot;
                t.anchorMax = model.transAttribute.anchorMax;
                t.anchorMin = model.transAttribute.anchorMin;
                t.offsetMax = model.transAttribute.offsetMax;
                t.offsetMin = model.transAttribute.offsetMin;
                t.anchoredPosition = model.transAttribute.anchoredPosition;
                t.anchoredPosition3D = model.transAttribute.anchoredPosition3D;
                t.localEulerAngles = model.transAttribute.localEulerAngles;
                t.localScale = model.transAttribute.localScale;
                t.localPosition = model.transAttribute.localPosition;
                t.sizeDelta = model.transAttribute.sizeDelta;
            }
            tar.name = model.name;
            tar.tag = model.tag;
        }
        public static void Save(GameObject tar, ModelElement model)
        {
            var r = tar.transform;
            if (r is RectTransform)
            {
                var s = r as RectTransform;
                model.transAttribute.localEulerAngles = s.localEulerAngles;
                model.transAttribute.localPosition = s.localPosition;
                model.transAttribute.localScale = s.localScale;
                model.transAttribute.anchoredPosition = s.anchoredPosition;
                model.transAttribute.anchoredPosition3D = s.anchoredPosition3D;
                model.transAttribute.anchorMax = s.anchorMax;
                model.transAttribute.anchorMin = s.anchorMin;
                model.transAttribute.offsetMax = s.offsetMax;
                model.transAttribute.offsetMin = s.offsetMin;
                model.transAttribute.pivot = s.pivot;
                model.transAttribute.sizeDelta = s.sizeDelta;
                var ss = s.GetComponent<SizeScaling>();
                if (ss != null)
                {
                    model.transAttribute.SizeScale = true;
                    model.transAttribute.scaleType = ss.scaleType;
                    model.transAttribute.sizeType = ss.sizeType;
                    model.transAttribute.anchorType = ss.anchorType;
                    model.transAttribute.parentType = ss.parentType;
                    model.transAttribute.margin = ss.margin;
                    model.transAttribute.DesignSize = ss.DesignSize;
                }
                else model.transAttribute.SizeScale = false;
                model.name = tar.name;
                model.tag = tar.tag;
                model.transAttribute.name =model.buffer.AddString(model.name);
                model.transAttribute.tag = model.buffer.AddString(model.tag);
            }
        }
        public List<ModelElement> Child =new List<ModelElement>();
        public GameObject Main;
        public virtual void Load(GameObject tar)
        {
            Load(tar, this);
            Main = tar;
        }
        public virtual void Save(GameObject tar)
        {
            Save(tar, this);
        }

        public void AddSizeScale()
        {
            if (transAttribute.SizeScale)
            {
                if(Main!=null)
                {
                    var scale= Main.GetComponent<SizeScaleEx>();
                    if (scale == null)
                        scale = Main.AddComponent<SizeScaleEx>();
                    scale.scaleType = transAttribute.scaleType;
                    scale.sizeType = transAttribute.sizeType;
                    scale.anchorType = transAttribute.anchorType;
                    scale.parentType = transAttribute.parentType;
                    scale.margin = transAttribute.margin;
                    scale.DesignSize = transAttribute.DesignSize;
                }
            }
        }
        public static Vector2[] Anchors = new[] { new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f),new Vector2(1, 0.5f),
        new Vector2(0.5f, 1),new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1)};
        public static void Docking(RectTransform rect, ScaleType dock, Vector2 pSize, Vector2 ds)
        {
            switch (dock)
            {
                case ScaleType.None:
                    rect.localScale = Vector3.one;
                    break;
                case ScaleType.FillX:
                    float sx = pSize.x / ds.x;
                    rect.localScale = new Vector3(sx, sx, sx);
                    break;
                case ScaleType.FillY:
                    float sy = pSize.y / ds.y;
                    rect.localScale = new Vector3(sy, sy, sy);
                    break;
                case ScaleType.FillXY:
                    sx = pSize.x / ds.x;
                    sy = pSize.y / ds.y;
                    if (sx < sy)
                        rect.localScale = new Vector3(sx, sx, sx);
                    else rect.localScale = new Vector3(sy, sy, sy);
                    break;
                case ScaleType.Cover:
                    sx = pSize.x / ds.x;
                    sy = pSize.y / ds.y;
                    if (sx < sy)
                        rect.localScale = new Vector3(sy, sy, sy);
                    else rect.localScale = new Vector3(sx, sx, sx);
                    break;
            }
        }
        public static void Anchor(RectTransform rect, Vector2 pivot, Vector2 offset)
        {
            Vector2 p;
            Vector2 pp = new Vector2(0.5f, 0.5f);
            if (rect.parent != null)
            {
                var t = rect.parent as RectTransform;
                p = t.sizeDelta;
                pp = t.pivot;
            }
            else { p = new Vector2(Screen.width, Screen.height); }
            rect.localScale = Vector3.one;
            float sx = p.x * (pivot.x - 0.5f);
            float sy = p.y * (pivot.y - 0.5f);
            float ox = sx + offset.x;
            float oy = sy + offset.y;
            rect.localPosition = new Vector3(ox, oy, 0);
        }
        public static void AnchorEx(RectTransform rect, AnchorType type, Vector2 offset, Vector2 p, Vector2 psize)
        {
            AnchorEx(rect, Anchors[(int)type], offset, p, psize);
        }
        public static void AnchorEx(RectTransform rect, Vector2 pivot, Vector2 offset, Vector2 parentPivot, Vector2 parentSize)
        {
            float ox = (parentPivot.x - 1) * parentSize.x;//原点x
            float oy = (parentPivot.y - 1) * parentSize.y;//原点y
            float tx = ox + pivot.x * parentSize.x;//锚点x
            float ty = oy + pivot.y * parentSize.y;//锚点y
            offset.x += tx;//偏移点x
            offset.y += ty;//偏移点y
            rect.localPosition = new Vector3(offset.x, offset.y, 0);
        }
        public static void MarginEx(RectTransform rect, Margin margin, Vector2 parentPivot, Vector2 parentSize)
        {
            float w = parentSize.x - margin.left - margin.right;
            float h = parentSize.y - margin.top - margin.down;
            var m_pivot = rect.pivot;
            float ox = w * m_pivot.x - parentPivot.x * parentSize.x + margin.left;
            float oy = h * m_pivot.y - parentPivot.y * parentSize.y + margin.down;
            float sx = rect.localScale.x;
            float sy = rect.localScale.y;
            rect.sizeDelta = new Vector2(w / sx, h / sy);
            rect.localPosition = new Vector3(ox, oy, 0);
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
        static void Resize(ModelElement ele)
        {
            if (ele.Main == null)
                return;
            var transform = ele.Main.transform;
            Vector2 size;
            Vector2 p = Anchors[0];
            if (ele.transAttribute.parentType == ParentType.Tranfrom)
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
            Docking(rect, ele.transAttribute.scaleType, size, ele.transAttribute.DesignSize);
            if (ele.transAttribute.sizeType == SizeType.Anchor)
            {
                AnchorEx(rect, ele.transAttribute.anchorType,
                    new Vector2(ele.transAttribute. margin.left, ele.transAttribute. margin.right), p, size);
            }
            else if (ele.transAttribute. sizeType == SizeType.Margin)
            {
                var mar = ele.transAttribute. margin;
                if (ele.transAttribute.parentType == ParentType.BangsScreen)
                    if (Scale.ScreenHeight / Scale.ScreenWidth > 2f)
                        mar.top += 88;
                MarginEx(rect, mar, p, size);
            }
            else if (ele.transAttribute.sizeType == SizeType.MarginRatio)
            {
                var mar = new Margin();
                mar.left = ele.transAttribute.margin.left * size.x;
                mar.right = ele.transAttribute.margin.right * size.x;
                mar.top = ele.transAttribute.margin.top * size.y;
                mar.down = ele.transAttribute.margin.down * size.y;
                if (ele.transAttribute.parentType == ParentType.BangsScreen)
                    if (Scale.ScreenHeight / Scale.ScreenWidth > 2f)
                        mar.top += 88;
                MarginEx(rect, mar, p, size);
            }
        }
        public static void ScaleSize(ModelElement element)
        {
            if (element.transAttribute.SizeScale)
                Resize(element);
            var child = element.Child;
            for (int i = 0; i < child.Count; i++)
            {
                ScaleSize(child[i]);
            }
        }
        public ModelElement FindChild(string name)
        {
            for (int i = 0; i < Child.Count; i++)
                if (Child[i].name == name)
                    return Child[i];
            return null;
        }
    }
}