using huqiang.UIModel;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace huqiang.UIComposite
{
    public unsafe struct SliderInfo
    {
        public Vector2 StartOffset;
        public Vector2 EndOffset;
        public float MinScale;
        public float MaxScale;
        public UISlider.Direction direction;
        public static int Size = sizeof(SliderInfo);
        public static int ElementSize = Size / 4;
    }
    public class UISlider : ModelInital
    {
        public enum Direction
        {
            Horizontal, Vertical
        }
        RectTransform model;
        public RectTransform FillImage;
        Image image;
        public RectTransform Nob;
        SliderInfo info;
        float ratio;
        EventCallBack callBack;
        Vector2 pos;
        /// <summary>
        /// 设置Nob的尺寸
        /// </summary>
        /// <param name="value"></param>
        public void SetFillSize(float value)
        {
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;
            if(Nob!=null)
            {
                if (info.direction == Direction.Horizontal)
                {
                    float w = model.sizeDelta.x;
                    float h = model.sizeDelta.y;
                    Nob.sizeDelta = new Vector2(value * w,h);
                    ApplyValue();
                }
                else
                {
                    float w = model.sizeDelta.x;
                    float h = model.sizeDelta.y;
                    Nob.sizeDelta = new Vector2(w, value * h);
                    ApplyValue();
                }
            }
        }
        public Action<UISlider> OnValueChanged;
        public float Percentage { get { return ratio; } set {
                if (value < 0)
                    value = 0;
                else if (value > 1)
                    value = 1;
                ratio = value;
                RatioToPos();
                ApplyValue();
            } }
        public UISlider()
        {
            info.MinScale = 1;
            info.MinScale = 1;
        }
        public override void Initial(ModelElement mod)
        {
            model = mod.Context;
            callBack = EventCallBack.RegEvent<EventCallBack>(model);
            callBack.Drag = callBack.DragEnd = Draging;
            callBack.PointerDown = PointDown;
            callBack.AutoColor = false;
            FillImage = model.Find("FillImage") as RectTransform;
            if (FillImage != null)
                image = FillImage.GetComponent<Image>();
            Nob = model.Find("Nob") as RectTransform;
            var fake = mod.GetExtand();
            if(fake!=null)
            {
                unsafe
                {
                    info = *(SliderInfo*)fake.ip;
                }
            }
        }
        void Draging(EventCallBack back, UserAction action, Vector2 v)
        {
            pos += v;
            ApplyValue();
            if (OnValueChanged != null)
                OnValueChanged(this);
        }
        void PointDown(EventCallBack back, UserAction action)
        {
            var v = new Vector2(back.GlobalPosition.x,back.GlobalPosition.y);
            pos = action.CanPosition - v;
            pos.x /= back.GlobalScale.x;
            pos.y /= back.GlobalScale.y;
            ApplyValue();
            if (OnValueChanged != null)
                OnValueChanged(this);
        }
        void RatioToPos()
        {
            var size = model.sizeDelta;
            if (info.direction == Direction.Horizontal)
            {
                float rx = size.x * 0.5f;
                float lx = -rx;
                float nx = Nob.sizeDelta.x * 0.5f;
                Vector2 start = new Vector2(lx + info.StartOffset.x + nx, info.StartOffset.y);
                Vector2 end = new Vector2(rx - info.EndOffset.x - nx, info.EndOffset.y);
                float w = end.x - start.x;
                pos.x = ratio * w + start.x;
            }
            else
            {
                float ty = size.y * 0.5f;
                float dy = -ty;
                float ny = Nob.sizeDelta.y * 0.5f;
                Vector2 start = new Vector2(info.StartOffset.x, dy + info.StartOffset.y + ny);
                Vector2 end = new Vector2(info.EndOffset.x, ty - info.EndOffset.y - ny);
                float w = end.y - start.y;
                pos.y = ratio * w + start.y;
            }
        }
        void ApplyValue()
        {
            if (Nob == null)
                return;
            var size = model.sizeDelta;
            if (info.direction==Direction.Horizontal)
            {
                float rx = size.x * 0.5f;
                float lx = -rx;
                float nx = Nob.sizeDelta.x * 0.5f;
                Vector2 start = new Vector2(lx + info.StartOffset.x+nx, info.StartOffset.y);
                Vector2 end = new Vector2(rx - info.EndOffset.x-nx, info.EndOffset.y);
                if (pos.x < start.x)
                    pos.x = start.x;
                else if (pos.x > end.x)
                    pos.x = end.x;
                float w = end.x - start.x;
                ratio = (pos.x - start.x) / w;
                pos = (end - start) * ratio + start;
                if(Nob!=null)
                {
                    Nob.localPosition = pos;
                    float s = (info.MaxScale - info.MinScale) * ratio + info.MinScale;
                    Nob.localScale = new Vector3(s,s,s);
                }
            }
            else
            {
                float ty = size.y * 0.5f;
                float dy = -ty;
                float ny = Nob.sizeDelta.y * 0.5f;
                Vector2 start = new Vector2( info.StartOffset.x,dy+ info.StartOffset.y+ny);
                Vector2 end = new Vector2(info.EndOffset.x,ty- info.EndOffset.y-ny);
                if (pos.y < start.y)
                    pos.y = start.y;
                else if (pos.y > end.y)
                    pos.y = end.y;
                float w = end.y - start.y;
                ratio = (pos.y - start.y) / w;
                pos = (end - start) * ratio + start;
                if (Nob != null)
                {
                    Nob.localPosition = pos;
                    float s = (info.MaxScale - info.MinScale) * ratio + info.MinScale;
                    Nob.localScale = new Vector3(s,s,s);
                }
            }
            if(image!=null)
                image.fillAmount = ratio;
        }
    }
}
